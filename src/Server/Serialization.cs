using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Server.Guilds;

namespace Server;

public abstract class GenericReader
{
	public abstract Type ReadObjectType();

	public abstract string ReadString();
	public abstract DateTime ReadDateTime();
	public abstract DateTimeOffset ReadDateTimeOffset();
	public abstract TimeSpan ReadTimeSpan();
	public abstract DateTime ReadDeltaTime();
	public abstract decimal ReadDecimal();
	public abstract long ReadLong();
	public abstract ulong ReadULong();
	public abstract int PeekInt();
	public abstract int ReadInt();
	public abstract uint ReadUInt();
	public abstract short ReadShort();
	public abstract ushort ReadUShort();
	public abstract double ReadDouble();
	public abstract float ReadFloat();
	public abstract char ReadChar();
	public abstract byte ReadByte();
	public abstract sbyte ReadSByte();
	public abstract bool ReadBool();
	public abstract int ReadEncodedInt();
	public abstract IPAddress ReadIpAddress();

	public abstract Point3D ReadPoint3D();
	public abstract Point2D ReadPoint2D();
	public abstract Rectangle2D ReadRect2D();
	public abstract Rectangle3D ReadRect3D();
	public abstract Map ReadMap();

	public abstract Serial ReadSerial();

	public abstract Item ReadItem();
	public abstract Mobile ReadMobile();
	public abstract BaseGuild ReadGuild();

	public abstract T ReadItem<T>() where T : Item;
	public abstract T ReadMobile<T>() where T : Mobile;
	public abstract T ReadGuild<T>() where T : BaseGuild;

	public abstract ArrayList ReadItemList();
	public abstract ArrayList ReadMobileList();
	public abstract ArrayList ReadGuildList();

	public abstract List<Item> ReadStrongItemList();
	public abstract List<T> ReadStrongItemList<T>() where T : Item;

	public abstract List<Mobile> ReadStrongMobileList();
	public abstract List<T> ReadStrongMobileList<T>() where T : Mobile;

	public abstract List<BaseGuild> ReadStrongGuildList();
	public abstract List<T> ReadStrongGuildList<T>() where T : BaseGuild;

	public abstract HashSet<Item> ReadItemSet();
	public abstract HashSet<T> ReadItemSet<T>() where T : Item;

	public abstract HashSet<Mobile> ReadMobileSet();
	public abstract HashSet<T> ReadMobileSet<T>() where T : Mobile;

	public abstract HashSet<BaseGuild> ReadGuildSet();
	public abstract HashSet<T> ReadGuildSet<T>() where T : BaseGuild;

	public abstract Race ReadRace();

	public abstract bool End();
}

public abstract class GenericWriter
{
	public abstract void Close();

	public abstract long Position { get; }

	public abstract void WriteObjectType(object value);
	public abstract void WriteObjectType(Type value);

	public abstract void Write(string value);
	public abstract void Write(DateTime value);
	public abstract void Write(DateTimeOffset value);
	public abstract void Write(TimeSpan value);
	public abstract void Write(decimal value);
	public abstract void Write(long value);
	public abstract void Write(ulong value);
	public abstract void Write(int value);
	public abstract void Write(uint value);
	public abstract void Write(short value);
	public abstract void Write(ushort value);
	public abstract void Write(double value);
	public abstract void Write(float value);
	public abstract void Write(char value);
	public abstract void Write(byte value);
	public abstract void Write(sbyte value);
	public abstract void Write(bool value);
	public abstract void WriteEncodedInt(int value);
	public abstract void Write(IPAddress value);
	public abstract void Write(Serial value);

	public abstract void WriteDeltaTime(DateTime value);

	public abstract void Write(Point3D value);
	public abstract void Write(Point2D value);
	public abstract void Write(Rectangle2D value);
	public abstract void Write(Rectangle3D value);
	public abstract void Write(Map value);

	public abstract void Write(Item value);
	public abstract void Write(Mobile value);
	public abstract void Write(BaseGuild value);

	public abstract void Write(Race value);

	public abstract void WriteItemList(ArrayList list);
	public abstract void WriteItemList(ArrayList list, bool tidy);

	public abstract void WriteMobileList(ArrayList list);
	public abstract void WriteMobileList(ArrayList list, bool tidy);

	public abstract void WriteGuildList(ArrayList list);
	public abstract void WriteGuildList(ArrayList list, bool tidy);

	public abstract void Write(List<Item> list);
	public abstract void Write(List<Item> list, bool tidy);

	public abstract void WriteItemList<T>(List<T> list) where T : Item;
	public abstract void WriteItemList<T>(List<T> list, bool tidy) where T : Item;

	public abstract void Write(HashSet<Item> list);
	public abstract void Write(HashSet<Item> list, bool tidy);

	public abstract void WriteItemSet<T>(HashSet<T> set) where T : Item;
	public abstract void WriteItemSet<T>(HashSet<T> set, bool tidy) where T : Item;

	public abstract void Write(List<Mobile> list);
	public abstract void Write(List<Mobile> list, bool tidy);

	public abstract void WriteMobileList<T>(List<T> list) where T : Mobile;
	public abstract void WriteMobileList<T>(List<T> list, bool tidy) where T : Mobile;

	public abstract void Write(HashSet<Mobile> list);
	public abstract void Write(HashSet<Mobile> list, bool tidy);

	public abstract void WriteMobileSet<T>(HashSet<T> set) where T : Mobile;
	public abstract void WriteMobileSet<T>(HashSet<T> set, bool tidy) where T : Mobile;

	public abstract void Write(List<BaseGuild> list);
	public abstract void Write(List<BaseGuild> list, bool tidy);

	public abstract void WriteGuildList<T>(List<T> list) where T : BaseGuild;
	public abstract void WriteGuildList<T>(List<T> list, bool tidy) where T : BaseGuild;

	public abstract void Write(HashSet<BaseGuild> list);
	public abstract void Write(HashSet<BaseGuild> list, bool tidy);

	public abstract void WriteGuildSet<T>(HashSet<T> set) where T : BaseGuild;
	public abstract void WriteGuildSet<T>(HashSet<T> set, bool tidy) where T : BaseGuild;
}

public class BinaryFileWriter : GenericWriter
{
	private readonly bool _prefixStrings;
	private readonly Stream _file;

	protected virtual int BufferSize => 81920;

	private readonly byte[] _buffer;

	private int _index;

	private readonly Encoding _encoding;

	public BinaryFileWriter(Stream strm, bool prefixStr)
	{
		_prefixStrings = prefixStr;

		_encoding = Utility.UTF8;
		_buffer = new byte[BufferSize];
		_file = strm;
	}

	public BinaryFileWriter(string filename, bool prefixStr)
		: this(filename, prefixStr, false)
	{ }

	public BinaryFileWriter(string filename, bool prefixStr, bool async)
	{
		_prefixStrings = prefixStr;

		_buffer = new byte[BufferSize];

		if (async)
		{
			_file = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None, BufferSize, true);
		}
		else
		{
			_file = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None, BufferSize, FileOptions.WriteThrough);
		}

		_encoding = Utility.UTF8WithEncoding;
	}

	public void Flush()
	{
		if (_index > 0)
		{
			_position += _index;

			_file.Write(_buffer, 0, _index);
			_index = 0;
		}
	}

	private long _position;

	public override long Position => _position + _index;

	public Stream UnderlyingStream
	{
		get
		{
			if (_index > 0)
			{
				Flush();
			}

			return _file;
		}
	}

	public override void Close()
	{
		if (_index > 0)
		{
			Flush();
		}

		_file.Close();
	}

	public override void WriteEncodedInt(int value)
	{
		var v = (uint)value;

		while (v >= 0x80)
		{
			if (_index + 1 > _buffer.Length)
			{
				Flush();
			}

			_buffer[_index++] = (byte)(v | 0x80);
			v >>= 7;
		}

		if (_index + 1 > _buffer.Length)
		{
			Flush();
		}

		_buffer[_index++] = (byte)v;
	}

	private byte[] _characterBuffer;
	private int _maxBufferChars;
	private const int LargeByteBufferSize = 256;

	internal void InternalWriteString(string value)
	{
		var length = _encoding.GetByteCount(value);

		WriteEncodedInt(length);

		if (_characterBuffer == null)
		{
			_characterBuffer = new byte[LargeByteBufferSize];
			_maxBufferChars = LargeByteBufferSize / _encoding.GetMaxByteCount(1);
		}

		if (length > LargeByteBufferSize)
		{
			var current = 0;
			var charsLeft = value.Length;

			while (charsLeft > 0)
			{
				var charCount = charsLeft > _maxBufferChars ? _maxBufferChars : charsLeft;
				var byteLength = _encoding.GetBytes(value, current, charCount, _characterBuffer, 0);

				if (_index + byteLength > _buffer.Length)
				{
					Flush();
				}

				Buffer.BlockCopy(_characterBuffer, 0, _buffer, _index, byteLength);
				_index += byteLength;

				current += charCount;
				charsLeft -= charCount;
			}
		}
		else
		{
			var byteLength = _encoding.GetBytes(value, 0, value.Length, _characterBuffer, 0);

			if (_index + byteLength > _buffer.Length)
			{
				Flush();
			}

			Buffer.BlockCopy(_characterBuffer, 0, _buffer, _index, byteLength);
			_index += byteLength;
		}
	}

	public override void Write(string value)
	{
		if (_prefixStrings)
		{
			if (value == null)
			{
				if (_index + 1 > _buffer.Length)
				{
					Flush();
				}

				_buffer[_index++] = 0;
			}
			else
			{
				if (_index + 1 > _buffer.Length)
				{
					Flush();
				}

				_buffer[_index++] = 1;

				InternalWriteString(value);
			}
		}
		else
		{
			InternalWriteString(value);
		}
	}

	public override void WriteObjectType(object value)
	{
		WriteObjectType(value?.GetType());
	}

	public override void WriteObjectType(Type value)
	{
		var hash = Assembler.FindHashByFullName(value?.FullName);

		WriteEncodedInt(hash);
	}

	public override void Write(DateTime value)
	{
		Write(value.Ticks);
	}

	public override void Write(DateTimeOffset value)
	{
		Write(value.Ticks);
		Write(value.Offset.Ticks);
	}

	public override void WriteDeltaTime(DateTime value)
	{
		var ticks = value.Ticks;
		var now = DateTime.UtcNow.Ticks;

		TimeSpan d;

		try
		{
			d = new TimeSpan(ticks - now);
		}
		catch (Exception)
		{
			d = TimeSpan.MaxValue;
		}

		Write(d);
	}

	public override void Write(IPAddress value)
	{
		Write(Utility.GetLongAddressValue(value));
	}

	public override void Write(TimeSpan value)
	{
		Write(value.Ticks);
	}

	public override void Write(decimal value)
	{
		var bits = decimal.GetBits(value);

		for (var i = 0; i < bits.Length; ++i)
		{
			Write(bits[i]);
		}
	}

	public override void Write(long value)
	{
		if (_index + 8 > _buffer.Length)
		{
			Flush();
		}

		_buffer[_index] = (byte)value;
		_buffer[_index + 1] = (byte)(value >> 8);
		_buffer[_index + 2] = (byte)(value >> 16);
		_buffer[_index + 3] = (byte)(value >> 24);
		_buffer[_index + 4] = (byte)(value >> 32);
		_buffer[_index + 5] = (byte)(value >> 40);
		_buffer[_index + 6] = (byte)(value >> 48);
		_buffer[_index + 7] = (byte)(value >> 56);
		_index += 8;
	}

	public override void Write(ulong value)
	{
		if (_index + 8 > _buffer.Length)
		{
			Flush();
		}

		_buffer[_index] = (byte)value;
		_buffer[_index + 1] = (byte)(value >> 8);
		_buffer[_index + 2] = (byte)(value >> 16);
		_buffer[_index + 3] = (byte)(value >> 24);
		_buffer[_index + 4] = (byte)(value >> 32);
		_buffer[_index + 5] = (byte)(value >> 40);
		_buffer[_index + 6] = (byte)(value >> 48);
		_buffer[_index + 7] = (byte)(value >> 56);
		_index += 8;
	}

	public override void Write(int value)
	{
		if (_index + 4 > _buffer.Length)
		{
			Flush();
		}

		_buffer[_index] = (byte)value;
		_buffer[_index + 1] = (byte)(value >> 8);
		_buffer[_index + 2] = (byte)(value >> 16);
		_buffer[_index + 3] = (byte)(value >> 24);
		_index += 4;
	}

	public override void Write(uint value)
	{
		if (_index + 4 > _buffer.Length)
		{
			Flush();
		}

		_buffer[_index] = (byte)value;
		_buffer[_index + 1] = (byte)(value >> 8);
		_buffer[_index + 2] = (byte)(value >> 16);
		_buffer[_index + 3] = (byte)(value >> 24);
		_index += 4;
	}

	public override void Write(short value)
	{
		if (_index + 2 > _buffer.Length)
		{
			Flush();
		}

		_buffer[_index] = (byte)value;
		_buffer[_index + 1] = (byte)(value >> 8);
		_index += 2;
	}

	public override void Write(ushort value)
	{
		if (_index + 2 > _buffer.Length)
		{
			Flush();
		}

		_buffer[_index] = (byte)value;
		_buffer[_index + 1] = (byte)(value >> 8);
		_index += 2;
	}

	public override unsafe void Write(double value)
	{
		if (_index + 8 > _buffer.Length)
		{
			Flush();
		}

		fixed (byte* pBuffer = _buffer)
		{
			*(double*)(pBuffer + _index) = value;
		}

		_index += 8;
	}

	public override unsafe void Write(float value)
	{
		if (_index + 4 > _buffer.Length)
		{
			Flush();
		}

		fixed (byte* pBuffer = _buffer)
		{
			*(float*)(pBuffer + _index) = value;
		}

		_index += 4;
	}

	private readonly char[] _singleCharBuffer = new char[1];

	public override void Write(char value)
	{
		if (_index + 8 > _buffer.Length)
		{
			Flush();
		}

		_singleCharBuffer[0] = value;

		var byteCount = _encoding.GetBytes(_singleCharBuffer, 0, 1, _buffer, _index);
		_index += byteCount;
	}

	public override void Write(byte value)
	{
		if (_index + 1 > _buffer.Length)
		{
			Flush();
		}

		_buffer[_index++] = value;
	}

	public override void Write(sbyte value)
	{
		if (_index + 1 > _buffer.Length)
		{
			Flush();
		}

		_buffer[_index++] = (byte)value;
	}

	public override void Write(bool value)
	{
		if (_index + 1 > _buffer.Length)
		{
			Flush();
		}

		_buffer[_index++] = (byte)(value ? 1 : 0);
	}

	public override void Write(Point3D value)
	{
		Write(value.m_X);
		Write(value.m_Y);
		Write(value.m_Z);
	}

	public override void Write(Point2D value)
	{
		Write(value.m_X);
		Write(value.m_Y);
	}

	public override void Write(Rectangle2D value)
	{
		Write(value.Start);
		Write(value.End);
	}

	public override void Write(Rectangle3D value)
	{
		Write(value.Start);
		Write(value.End);
	}

	public override void Write(Map value)
	{
		if (value != null)
		{
			Write((byte)value.MapIndex);
		}
		else
		{
			Write((byte)0xFF);
		}
	}

	public override void Write(Race value)
	{
		if (value != null)
		{
			Write((byte)value.RaceIndex);
		}
		else
		{
			Write((byte)0xFF);
		}
	}

	public override void Write(Serial value)
	{
		Write(value.Value);
	}

	public override void Write(Item value)
	{
		if (value == null || value.Deleted)
		{
			Write(Serial.MinusOne);
		}
		else
		{
			Write(value.Serial);
		}
	}

	public override void Write(Mobile value)
	{
		if (value == null || value.Deleted)
		{
			Write(Serial.MinusOne);
		}
		else
		{
			Write(value.Serial);
		}
	}

	public override void Write(BaseGuild value)
	{
		if (value == null)
		{
			Write(0);
		}
		else
		{
			Write(value.Id);
		}
	}

	public override void WriteMobileList(ArrayList list)
	{
		WriteMobileList(list, false);
	}

	public override void WriteMobileList(ArrayList list, bool tidy)
	{
		if (tidy)
		{
			for (var i = 0; i < list.Count;)
			{
				if (((Mobile)list[i]).Deleted)
				{
					list.RemoveAt(i);
				}
				else
				{
					++i;
				}
			}
		}

		Write(list.Count);

		for (var i = 0; i < list.Count; ++i)
		{
			Write((Mobile)list[i]);
		}
	}

	public override void WriteItemList(ArrayList list)
	{
		WriteItemList(list, false);
	}

	public override void WriteItemList(ArrayList list, bool tidy)
	{
		if (tidy)
		{
			for (var i = 0; i < list.Count;)
			{
				if (((Item)list[i]).Deleted)
				{
					list.RemoveAt(i);
				}
				else
				{
					++i;
				}
			}
		}

		Write(list.Count);

		for (var i = 0; i < list.Count; ++i)
		{
			Write((Item)list[i]);
		}
	}

	public override void WriteGuildList(ArrayList list)
	{
		WriteGuildList(list, false);
	}

	public override void WriteGuildList(ArrayList list, bool tidy)
	{
		if (tidy)
		{
			for (var i = 0; i < list.Count;)
			{
				if (((BaseGuild)list[i]).Disbanded)
				{
					list.RemoveAt(i);
				}
				else
				{
					++i;
				}
			}
		}

		Write(list.Count);

		for (var i = 0; i < list.Count; ++i)
		{
			Write((BaseGuild)list[i]);
		}
	}

	public override void Write(List<Item> list)
	{
		Write(list, false);
	}

	public override void Write(List<Item> list, bool tidy)
	{
		if (tidy)
		{
			for (var i = 0; i < list.Count;)
			{
				if (list[i].Deleted)
				{
					list.RemoveAt(i);
				}
				else
				{
					++i;
				}
			}
		}

		Write(list.Count);

		for (var i = 0; i < list.Count; ++i)
		{
			Write(list[i]);
		}
	}

	public override void WriteItemList<T>(List<T> list)
	{
		WriteItemList(list, false);
	}

	public override void WriteItemList<T>(List<T> list, bool tidy)
	{
		if (tidy)
		{
			for (var i = 0; i < list.Count;)
			{
				if (list[i].Deleted)
				{
					list.RemoveAt(i);
				}
				else
				{
					++i;
				}
			}
		}

		Write(list.Count);

		for (var i = 0; i < list.Count; ++i)
		{
			Write(list[i]);
		}
	}

	public override void Write(HashSet<Item> set)
	{
		Write(set, false);
	}

	public override void Write(HashSet<Item> set, bool tidy)
	{
		if (tidy)
		{
			set.RemoveWhere(item => item.Deleted);
		}

		Write(set.Count);

		foreach (var item in set)
		{
			Write(item);
		}
	}

	public override void WriteItemSet<T>(HashSet<T> set)
	{
		WriteItemSet(set, false);
	}

	public override void WriteItemSet<T>(HashSet<T> set, bool tidy)
	{
		if (tidy)
		{
			set.RemoveWhere(item => item.Deleted);
		}

		Write(set.Count);

		foreach (Item item in set)
		{
			Write(item);
		}
	}

	public override void Write(List<Mobile> list)
	{
		Write(list, false);
	}

	public override void Write(List<Mobile> list, bool tidy)
	{
		if (tidy)
		{
			for (var i = 0; i < list.Count;)
			{
				if (list[i].Deleted)
				{
					list.RemoveAt(i);
				}
				else
				{
					++i;
				}
			}
		}

		Write(list.Count);

		for (var i = 0; i < list.Count; ++i)
		{
			Write(list[i]);
		}
	}

	public override void WriteMobileList<T>(List<T> list)
	{
		WriteMobileList(list, false);
	}

	public override void WriteMobileList<T>(List<T> list, bool tidy)
	{
		if (tidy)
		{
			for (var i = 0; i < list.Count;)
			{
				if (list[i].Deleted)
				{
					list.RemoveAt(i);
				}
				else
				{
					++i;
				}
			}
		}

		Write(list.Count);

		for (var i = 0; i < list.Count; ++i)
		{
			Write(list[i]);
		}
	}

	public override void Write(HashSet<Mobile> set)
	{
		Write(set, false);
	}

	public override void Write(HashSet<Mobile> set, bool tidy)
	{
		if (tidy)
		{
			set.RemoveWhere(mobile => mobile.Deleted);
		}

		Write(set.Count);

		foreach (var mob in set)
		{
			Write(mob);
		}
	}

	public override void WriteMobileSet<T>(HashSet<T> set)
	{
		WriteMobileSet(set, false);
	}

	public override void WriteMobileSet<T>(HashSet<T> set, bool tidy)
	{
		if (tidy)
		{
			set.RemoveWhere(mob => mob.Deleted);
		}

		Write(set.Count);

		foreach (var mob in set)
		{
			Write(mob);
		}
	}

	public override void Write(List<BaseGuild> list)
	{
		Write(list, false);
	}

	public override void Write(List<BaseGuild> list, bool tidy)
	{
		if (tidy)
		{
			for (var i = 0; i < list.Count;)
			{
				if (list[i].Disbanded)
				{
					list.RemoveAt(i);
				}
				else
				{
					++i;
				}
			}
		}

		Write(list.Count);

		for (var i = 0; i < list.Count; ++i)
		{
			Write(list[i]);
		}
	}

	public override void WriteGuildList<T>(List<T> list)
	{
		WriteGuildList(list, false);
	}

	public override void WriteGuildList<T>(List<T> list, bool tidy)
	{
		if (tidy)
		{
			for (var i = 0; i < list.Count;)
			{
				if (list[i].Disbanded)
				{
					list.RemoveAt(i);
				}
				else
				{
					++i;
				}
			}
		}

		Write(list.Count);

		for (var i = 0; i < list.Count; ++i)
		{
			Write(list[i]);
		}
	}

	public override void Write(HashSet<BaseGuild> set)
	{
		Write(set, false);
	}

	public override void Write(HashSet<BaseGuild> set, bool tidy)
	{
		if (tidy)
		{
			set.RemoveWhere(guild => guild.Disbanded);
		}

		Write(set.Count);

		foreach (var guild in set)
		{
			Write(guild);
		}
	}

	public override void WriteGuildSet<T>(HashSet<T> set)
	{
		WriteGuildSet(set, false);
	}

	public override void WriteGuildSet<T>(HashSet<T> set, bool tidy)
	{
		if (tidy)
		{
			set.RemoveWhere(guild => guild.Disbanded);
		}

		Write(set.Count);

		foreach (var guild in set)
		{
			Write(guild);
		}
	}
}

public sealed class BinaryFileReader : GenericReader
{
	private readonly BinaryReader _file;

	public BinaryFileReader(BinaryReader br)
	{
		_file = br;
	}

	public void Close()
	{
		_file.Close();
	}

	public long Position => _file.BaseStream.Position;

	public long Seek(long offset, SeekOrigin origin)
	{
		return _file.BaseStream.Seek(offset, origin);
	}

	public override Type ReadObjectType()
	{
		var hash = ReadEncodedInt();

		return Assembler.FindTypeByFullNameHash(hash);
	}

	public override string ReadString()
	{
		return ReadByte() != 0 ? _file.ReadString() : null;
	}

	public override DateTime ReadDeltaTime()
	{
		var offset = ReadTimeSpan();

		try
		{
			return DateTime.UtcNow + offset;
		}
		catch
		{
			if (offset <= TimeSpan.MinValue)
				return DateTime.MinValue;

			return offset >= TimeSpan.MaxValue ? DateTime.MaxValue : DateTime.UtcNow;
		}
	}

	public override IPAddress ReadIpAddress()
	{
		return new IPAddress(_file.ReadInt64());
	}

	public override int ReadEncodedInt()
	{
		int v = 0, shift = 0;
		byte b;

		do
		{
			b = _file.ReadByte();
			v |= (b & 0x7F) << shift;
			shift += 7;
		}
		while (b >= 0x80);

		return v;
	}

	public override DateTime ReadDateTime()
	{
		return new DateTime(_file.ReadInt64());
	}

	public override DateTimeOffset ReadDateTimeOffset()
	{
		var ticks = _file.ReadInt64();
		var offset = new TimeSpan(_file.ReadInt64());

		return new DateTimeOffset(ticks, offset);
	}

	public override TimeSpan ReadTimeSpan()
	{
		return new TimeSpan(_file.ReadInt64());
	}

	public override decimal ReadDecimal()
	{
		return _file.ReadDecimal();
	}

	public override long ReadLong()
	{
		return _file.ReadInt64();
	}

	public override ulong ReadULong()
	{
		return _file.ReadUInt64();
	}

	public override int PeekInt()
	{
		var value = 0;
		var returnTo = _file.BaseStream.Position;

		try
		{
			value = _file.ReadInt32();
		}
		catch (EndOfStreamException)
		{
			// Ignore this exception, the default value 0 will be returned
		}

		_file.BaseStream.Seek(returnTo, SeekOrigin.Begin);
		return value;
	}

	public override int ReadInt()
	{
		return _file.ReadInt32();
	}

	public override uint ReadUInt()
	{
		return _file.ReadUInt32();
	}

	public override short ReadShort()
	{
		return _file.ReadInt16();
	}

	public override ushort ReadUShort()
	{
		return _file.ReadUInt16();
	}

	public override double ReadDouble()
	{
		return _file.ReadDouble();
	}

	public override float ReadFloat()
	{
		return _file.ReadSingle();
	}

	public override char ReadChar()
	{
		return _file.ReadChar();
	}

	public override byte ReadByte()
	{
		return _file.ReadByte();
	}

	public override sbyte ReadSByte()
	{
		return _file.ReadSByte();
	}

	public override bool ReadBool()
	{
		return _file.ReadBoolean();
	}

	public override Point3D ReadPoint3D()
	{
		return new Point3D(ReadInt(), ReadInt(), ReadInt());
	}

	public override Point2D ReadPoint2D()
	{
		return new Point2D(ReadInt(), ReadInt());
	}

	public override Rectangle2D ReadRect2D()
	{
		return new Rectangle2D(ReadPoint2D(), ReadPoint2D());
	}

	public override Rectangle3D ReadRect3D()
	{
		return new Rectangle3D(ReadPoint3D(), ReadPoint3D());
	}

	public override Map ReadMap()
	{
		return Map.Maps[ReadByte()];
	}

	public override Serial ReadSerial()
	{
		return new Serial(ReadInt());
	}

	public override Item ReadItem()
	{
		return World.FindItem(ReadSerial());
	}

	public override Mobile ReadMobile()
	{
		return World.FindMobile(ReadSerial());
	}

	public override BaseGuild ReadGuild()
	{
		return BaseGuild.Find(ReadInt());
	}

	public override T ReadItem<T>()
	{
		return ReadItem() as T;
	}

	public override T ReadMobile<T>()
	{
		return ReadMobile() as T;
	}

	public override T ReadGuild<T>()
	{
		return ReadGuild() as T;
	}

	public override ArrayList ReadItemList()
	{
		var count = ReadInt();

		if (count > 0)
		{
			var list = new ArrayList(count);

			for (var i = 0; i < count; ++i)
			{
				var item = ReadItem();

				if (item != null)
				{
					list.Add(item);
				}
			}

			return list;
		}

		return new ArrayList();
	}

	public override ArrayList ReadMobileList()
	{
		var count = ReadInt();

		if (count > 0)
		{
			var list = new ArrayList(count);

			for (var i = 0; i < count; ++i)
			{
				var m = ReadMobile();

				if (m != null)
				{
					list.Add(m);
				}
			}

			return list;
		}

		return new ArrayList();
	}

	public override ArrayList ReadGuildList()
	{
		var count = ReadInt();

		if (count > 0)
		{
			var list = new ArrayList(count);

			for (var i = 0; i < count; ++i)
			{
				var g = ReadGuild();

				if (g != null)
				{
					list.Add(g);
				}
			}

			return list;
		}

		return new ArrayList();
	}

	public override List<Item> ReadStrongItemList()
	{
		return ReadStrongItemList<Item>();
	}

	public override List<T> ReadStrongItemList<T>()
	{
		var count = ReadInt();

		if (count > 0)
		{
			var list = new List<T>(count);

			for (var i = 0; i < count; ++i)
			{
				if (ReadItem() is T item)
				{
					list.Add(item);
				}
			}

			return list;
		}

		return new List<T>();
	}

	public override HashSet<Item> ReadItemSet()
	{
		return ReadItemSet<Item>();
	}

	public override HashSet<T> ReadItemSet<T>()
	{
		var count = ReadInt();

		if (count > 0)
		{
			var set = new HashSet<T>();

			for (var i = 0; i < count; ++i)
			{
				if (ReadItem() is T item)
				{
					set.Add(item);
				}
			}

			return set;
		}

		return new HashSet<T>();
	}

	public override List<Mobile> ReadStrongMobileList()
	{
		return ReadStrongMobileList<Mobile>();
	}

	public override List<T> ReadStrongMobileList<T>()
	{
		var count = ReadInt();

		if (count > 0)
		{
			var list = new List<T>(count);

			for (var i = 0; i < count; ++i)
			{
				if (ReadMobile() is T m)
				{
					list.Add(m);
				}
			}

			return list;
		}

		return new List<T>();
	}

	public override HashSet<Mobile> ReadMobileSet()
	{
		return ReadMobileSet<Mobile>();
	}

	public override HashSet<T> ReadMobileSet<T>()
	{
		var count = ReadInt();

		if (count > 0)
		{
			var set = new HashSet<T>();

			for (var i = 0; i < count; ++i)
			{
				if (ReadMobile() is T m)
				{
					set.Add(m);
				}
			}

			return set;
		}

		return new HashSet<T>();
	}

	public override List<BaseGuild> ReadStrongGuildList()
	{
		return ReadStrongGuildList<BaseGuild>();
	}

	public override List<T> ReadStrongGuildList<T>()
	{
		var count = ReadInt();

		if (count > 0)
		{
			var list = new List<T>(count);

			for (var i = 0; i < count; ++i)
			{
				if (ReadGuild() is T g)
				{
					list.Add(g);
				}
			}

			return list;
		}

		return new List<T>();
	}

	public override HashSet<BaseGuild> ReadGuildSet()
	{
		return ReadGuildSet<BaseGuild>();
	}

	public override HashSet<T> ReadGuildSet<T>()
	{
		var count = ReadInt();

		if (count > 0)
		{
			var set = new HashSet<T>();

			for (var i = 0; i < count; ++i)
			{
				if (ReadGuild() is T g)
				{
					set.Add(g);
				}
			}

			return set;
		}

		return new HashSet<T>();
	}

	public override Race ReadRace()
	{
		return Race.Races[ReadByte()];
	}

	public override bool End()
	{
		return _file.PeekChar() == -1;
	}
}

public sealed class AsyncWriter : GenericWriter
{
	public static int ThreadCount { get; private set; }

	private readonly int _bufferSize;

	private long _lastPos, _curPos;
	private bool _closed;
	private readonly bool _prefixStrings;

	private MemoryStream _mem;
	private BinaryWriter _bin;
	private readonly FileStream _file;

	private readonly Queue _writeQueue;
	private Thread _workerThread;

	public AsyncWriter(string filename, bool prefix)
		: this(filename, 1048576, prefix) //1 mb buffer
	{ }

	public AsyncWriter(string filename, int buffSize, bool prefix)
	{
		_prefixStrings = prefix;
		_closed = false;
		_writeQueue = Queue.Synchronized(new Queue());
		_bufferSize = buffSize;

		_file = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None, _bufferSize, FileOptions.Asynchronous);
		_mem = new MemoryStream(_bufferSize + 1024);
		_bin = new BinaryWriter(_mem, Utility.UTF8WithEncoding);
	}

	private void Enqueue(MemoryStream mem)
	{
		_writeQueue.Enqueue(mem);

		if (_workerThread is not {IsAlive: true})
		{
			_workerThread = new Thread(new WorkerThread(this).Worker)
			{
				Priority = ThreadPriority.BelowNormal
			};
			_workerThread.Start();
		}
	}

	private class WorkerThread
	{
		private readonly AsyncWriter m_Owner;

		public WorkerThread(AsyncWriter owner)
		{
			m_Owner = owner;
		}

		public void Worker()
		{
			ThreadCount++;

			while (m_Owner._writeQueue.Count > 0)
			{
				var mem = (MemoryStream)m_Owner._writeQueue.Dequeue();

				if (mem != null && mem.Length > 0)
				{
					mem.WriteTo(m_Owner._file);
				}
			}

			if (m_Owner._closed)
			{
				m_Owner._file.Close();
			}

			if (--ThreadCount <= 0)
			{
				World.NotifyDiskWriteComplete();
			}
		}
	}

	private void OnWrite()
	{
		var curlen = _mem.Length;
		_curPos += curlen - _lastPos;
		_lastPos = curlen;
		if (curlen >= _bufferSize)
		{
			Enqueue(_mem);
			_mem = new MemoryStream(_bufferSize + 1024);
			_bin = new BinaryWriter(_mem, Utility.UTF8WithEncoding);
			_lastPos = 0;
		}
	}

	public MemoryStream MemStream
	{
		get => _mem;
		set
		{
			if (_mem.Length > 0)
			{
				Enqueue(_mem);
			}

			_mem = value;
			_bin = new BinaryWriter(_mem, Utility.UTF8WithEncoding);
			_lastPos = 0;
			_curPos = _mem.Length;
			_mem.Seek(0, SeekOrigin.End);
		}
	}

	public override void Close()
	{
		Enqueue(_mem);
		_closed = true;
	}

	public override long Position => _curPos;

	public override void WriteObjectType(object value)
	{
		WriteObjectType(value?.GetType());
	}

	public override void WriteObjectType(Type value)
	{
		var hash = Assembler.FindHashByFullName(value?.FullName);

		WriteEncodedInt(hash);
	}

	public override void Write(IPAddress value)
	{
		_bin.Write(Utility.GetLongAddressValue(value));
		OnWrite();
	}

	public override void Write(string value)
	{
		if (_prefixStrings)
		{
			if (value == null)
			{
				_bin.Write((byte)0);
			}
			else
			{
				_bin.Write((byte)1);
				_bin.Write(value);
			}
		}
		else
		{
			_bin.Write(value);
		}
		OnWrite();
	}

	public override void WriteDeltaTime(DateTime value)
	{
		var ticks = value.Ticks;
		var now = DateTime.UtcNow.Ticks;

		TimeSpan d;

		try
		{
			d = new TimeSpan(ticks - now);
		}
		catch (Exception)
		{
			d = TimeSpan.MaxValue;
		}

		Write(d);
	}

	public override void Write(DateTime value)
	{
		_bin.Write(value.Ticks);
		OnWrite();
	}

	public override void Write(DateTimeOffset value)
	{
		_bin.Write(value.Ticks);
		_bin.Write(value.Offset.Ticks);
		OnWrite();
	}

	public override void Write(TimeSpan value)
	{
		_bin.Write(value.Ticks);
		OnWrite();
	}

	public override void Write(decimal value)
	{
		_bin.Write(value);
		OnWrite();
	}

	public override void Write(long value)
	{
		_bin.Write(value);
		OnWrite();
	}

	public override void Write(ulong value)
	{
		_bin.Write(value);
		OnWrite();
	}

	public override void WriteEncodedInt(int value)
	{
		var v = (uint)value;

		while (v >= 0x80)
		{
			_bin.Write((byte)(v | 0x80));
			v >>= 7;
		}

		_bin.Write((byte)v);
		OnWrite();
	}

	public override void Write(int value)
	{
		_bin.Write(value);
		OnWrite();
	}

	public override void Write(uint value)
	{
		_bin.Write(value);
		OnWrite();
	}

	public override void Write(short value)
	{
		_bin.Write(value);
		OnWrite();
	}

	public override void Write(ushort value)
	{
		_bin.Write(value);
		OnWrite();
	}

	public override void Write(double value)
	{
		_bin.Write(value);
		OnWrite();
	}

	public override void Write(float value)
	{
		_bin.Write(value);
		OnWrite();
	}

	public override void Write(char value)
	{
		_bin.Write(value);
		OnWrite();
	}

	public override void Write(byte value)
	{
		_bin.Write(value);
		OnWrite();
	}

	public override void Write(sbyte value)
	{
		_bin.Write(value);
		OnWrite();
	}

	public override void Write(bool value)
	{
		_bin.Write(value);
		OnWrite();
	}

	public override void Write(Point3D value)
	{
		Write(value.m_X);
		Write(value.m_Y);
		Write(value.m_Z);
	}

	public override void Write(Point2D value)
	{
		Write(value.m_X);
		Write(value.m_Y);
	}

	public override void Write(Rectangle2D value)
	{
		Write(value.Start);
		Write(value.End);
	}

	public override void Write(Rectangle3D value)
	{
		Write(value.Start);
		Write(value.End);
	}

	public override void Write(Map value)
	{
		if (value != null)
		{
			Write((byte)value.MapIndex);
		}
		else
		{
			Write((byte)0xFF);
		}
	}

	public override void Write(Race value)
	{
		if (value != null)
		{
			Write((byte)value.RaceIndex);
		}
		else
		{
			Write((byte)0xFF);
		}
	}

	public override void Write(Serial value)
	{
		Write(value.Value);
	}

	public override void Write(Item value)
	{
		if (value == null || value.Deleted)
		{
			Write(Serial.MinusOne);
		}
		else
		{
			Write(value.Serial);
		}
	}

	public override void Write(Mobile value)
	{
		if (value == null || value.Deleted)
		{
			Write(Serial.MinusOne);
		}
		else
		{
			Write(value.Serial);
		}
	}

	public override void Write(BaseGuild value)
	{
		if (value == null)
		{
			Write(0);
		}
		else
		{
			Write(value.Id);
		}
	}

	public override void WriteMobileList(ArrayList list)
	{
		WriteMobileList(list, false);
	}

	public override void WriteMobileList(ArrayList list, bool tidy)
	{
		if (tidy)
		{
			for (var i = 0; i < list.Count;)
			{
				if (((Mobile)list[i]).Deleted)
				{
					list.RemoveAt(i);
				}
				else
				{
					++i;
				}
			}
		}

		Write(list.Count);

		for (var i = 0; i < list.Count; ++i)
		{
			Write((Mobile)list[i]);
		}
	}

	public override void WriteItemList(ArrayList list)
	{
		WriteItemList(list, false);
	}

	public override void WriteItemList(ArrayList list, bool tidy)
	{
		if (tidy)
		{
			for (var i = 0; i < list.Count;)
			{
				if (((Item)list[i]).Deleted)
				{
					list.RemoveAt(i);
				}
				else
				{
					++i;
				}
			}
		}

		Write(list.Count);

		for (var i = 0; i < list.Count; ++i)
		{
			Write((Item)list[i]);
		}
	}

	public override void WriteGuildList(ArrayList list)
	{
		WriteGuildList(list, false);
	}

	public override void WriteGuildList(ArrayList list, bool tidy)
	{
		if (tidy)
		{
			for (var i = 0; i < list.Count;)
			{
				if (((BaseGuild)list[i]).Disbanded)
				{
					list.RemoveAt(i);
				}
				else
				{
					++i;
				}
			}
		}

		Write(list.Count);

		for (var i = 0; i < list.Count; ++i)
		{
			Write((BaseGuild)list[i]);
		}
	}

	public override void Write(List<Item> list)
	{
		Write(list, false);
	}

	public override void Write(List<Item> list, bool tidy)
	{
		if (tidy)
		{
			for (var i = 0; i < list.Count;)
			{
				if (list[i].Deleted)
				{
					list.RemoveAt(i);
				}
				else
				{
					++i;
				}
			}
		}

		Write(list.Count);

		for (var i = 0; i < list.Count; ++i)
		{
			Write(list[i]);
		}
	}

	public override void WriteItemList<T>(List<T> list)
	{
		WriteItemList(list, false);
	}

	public override void WriteItemList<T>(List<T> list, bool tidy)
	{
		if (tidy)
		{
			for (var i = 0; i < list.Count;)
			{
				if (list[i].Deleted)
				{
					list.RemoveAt(i);
				}
				else
				{
					++i;
				}
			}
		}

		Write(list.Count);

		for (var i = 0; i < list.Count; ++i)
		{
			Write(list[i]);
		}
	}

	public override void Write(HashSet<Item> set)
	{
		Write(set, false);
	}

	public override void Write(HashSet<Item> set, bool tidy)
	{
		if (tidy)
		{
			set.RemoveWhere(item => item.Deleted);
		}

		Write(set.Count);

		foreach (var item in set)
		{
			Write(item);
		}
	}

	public override void WriteItemSet<T>(HashSet<T> set)
	{
		WriteItemSet(set, false);
	}

	public override void WriteItemSet<T>(HashSet<T> set, bool tidy)
	{
		if (tidy)
		{
			set.RemoveWhere(item => item.Deleted);
		}

		Write(set.Count);

		foreach (var item in set)
		{
			Write(item);
		}
	}

	public override void Write(List<Mobile> list)
	{
		Write(list, false);
	}

	public override void Write(List<Mobile> list, bool tidy)
	{
		if (tidy)
		{
			for (var i = 0; i < list.Count;)
			{
				if (list[i].Deleted)
				{
					list.RemoveAt(i);
				}
				else
				{
					++i;
				}
			}
		}

		Write(list.Count);

		for (var i = 0; i < list.Count; ++i)
		{
			Write(list[i]);
		}
	}

	public override void WriteMobileList<T>(List<T> list)
	{
		WriteMobileList(list, false);
	}

	public override void WriteMobileList<T>(List<T> list, bool tidy)
	{
		if (tidy)
		{
			for (var i = 0; i < list.Count;)
			{
				if (list[i].Deleted)
				{
					list.RemoveAt(i);
				}
				else
				{
					++i;
				}
			}
		}

		Write(list.Count);

		for (var i = 0; i < list.Count; ++i)
		{
			Write(list[i]);
		}
	}

	public override void Write(HashSet<Mobile> set)
	{
		Write(set, false);
	}

	public override void Write(HashSet<Mobile> set, bool tidy)
	{
		if (tidy)
		{
			set.RemoveWhere(mobile => mobile.Deleted);
		}

		Write(set.Count);

		foreach (var mob in set)
		{
			Write(mob);
		}
	}

	public override void WriteMobileSet<T>(HashSet<T> set)
	{
		WriteMobileSet(set, false);
	}

	public override void WriteMobileSet<T>(HashSet<T> set, bool tidy)
	{
		if (tidy)
		{
			set.RemoveWhere(mob => mob.Deleted);
		}

		Write(set.Count);

		foreach (var mob in set)
		{
			Write(mob);
		}
	}

	public override void Write(List<BaseGuild> list)
	{
		Write(list, false);
	}

	public override void Write(List<BaseGuild> list, bool tidy)
	{
		if (tidy)
		{
			for (var i = 0; i < list.Count;)
			{
				if (list[i].Disbanded)
				{
					list.RemoveAt(i);
				}
				else
				{
					++i;
				}
			}
		}

		Write(list.Count);

		for (var i = 0; i < list.Count; ++i)
		{
			Write(list[i]);
		}
	}

	public override void WriteGuildList<T>(List<T> list)
	{
		WriteGuildList(list, false);
	}

	public override void WriteGuildList<T>(List<T> list, bool tidy)
	{
		if (tidy)
		{
			for (var i = 0; i < list.Count;)
			{
				if (list[i].Disbanded)
				{
					list.RemoveAt(i);
				}
				else
				{
					++i;
				}
			}
		}

		Write(list.Count);

		for (var i = 0; i < list.Count; ++i)
		{
			Write(list[i]);
		}
	}

	public override void Write(HashSet<BaseGuild> set)
	{
		Write(set, false);
	}

	public override void Write(HashSet<BaseGuild> set, bool tidy)
	{
		if (tidy)
		{
			set.RemoveWhere(guild => guild.Disbanded);
		}

		Write(set.Count);

		foreach (var guild in set)
		{
			Write(guild);
		}
	}

	public override void WriteGuildSet<T>(HashSet<T> set)
	{
		WriteGuildSet(set, false);
	}

	public override void WriteGuildSet<T>(HashSet<T> set, bool tidy)
	{
		if (tidy)
		{
			set.RemoveWhere(guild => guild.Disbanded);
		}

		Write(set.Count);

		foreach (var guild in set)
		{
			Write(guild);
		}
	}
}

public interface ISerializable
{
	int TypeReference { get; }
	int SerialIdentity { get; }

	void Serialize(GenericWriter writer);
}
