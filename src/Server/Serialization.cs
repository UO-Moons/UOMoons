using Server.Guilds;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace Server
{
	public abstract class GenericReader
	{
		//protected GenericReader() { }

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
		///protected GenericWriter() { }

		public abstract void Close();

		public abstract long Position { get; }

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

		public abstract void WriteDeltaTime(DateTime value);

		public abstract void Write(Point3D value);
		public abstract void Write(Point2D value);
		public abstract void Write(Rectangle2D value);
		public abstract void Write(Rectangle3D value);
		public abstract void Write(Map value);

		public abstract void Write(Item value);
		public abstract void Write(Mobile value);
		public abstract void Write(BaseGuild value);

		public abstract void WriteItem<T>(T value) where T : Item;
		public abstract void WriteMobile<T>(T value) where T : Mobile;
		public abstract void WriteGuild<T>(T value) where T : BaseGuild;

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

		// Compiler won't notice their 'where' to differentiate the generic methods.
	}

	public class BinaryFileWriter : GenericWriter
	{
		private readonly bool _prefixStrings;
		private readonly Stream _mFile;

		protected virtual int BufferSize => 64 * 1024;

		private readonly byte[] _mBuffer;

		private int _mIndex;

		private readonly Encoding _mEncoding;

		public BinaryFileWriter(Stream strm, bool prefixStr)
		{
			_prefixStrings = prefixStr;
			_mEncoding = Utility.UTF8;
			_mBuffer = new byte[BufferSize];
			_mFile = strm;
		}

		public BinaryFileWriter(string filename, bool prefixStr)
		{
			_prefixStrings = prefixStr;
			_mBuffer = new byte[BufferSize];
			_mFile = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None);
			_mEncoding = Utility.UTF8WithEncoding;
		}

		public void Flush()
		{
			if (_mIndex > 0)
			{
				_mPosition += _mIndex;

				_mFile.Write(_mBuffer, 0, _mIndex);
				_mIndex = 0;
			}
		}

		private long _mPosition;

		public override long Position => _mPosition + _mIndex;

		public Stream UnderlyingStream
		{
			get
			{
				if (_mIndex > 0)
					Flush();

				return _mFile;
			}
		}

		public override void Close()
		{
			if (_mIndex > 0)
				Flush();

			_mFile.Close();
		}

		public override void WriteEncodedInt(int value)
		{
			uint v = (uint)value;

			while (v >= 0x80)
			{
				if (_mIndex + 1 > _mBuffer.Length)
					Flush();

				_mBuffer[_mIndex++] = (byte)(v | 0x80);
				v >>= 7;
			}

			if (_mIndex + 1 > _mBuffer.Length)
				Flush();

			_mBuffer[_mIndex++] = (byte)v;
		}

		private byte[] _mCharacterBuffer;
		private int _mMaxBufferChars;
		private const int LargeByteBufferSize = 256;

		internal void InternalWriteString(string value)
		{
			int length = _mEncoding.GetByteCount(value);

			WriteEncodedInt(length);

			if (_mCharacterBuffer == null)
			{
				_mCharacterBuffer = new byte[LargeByteBufferSize];
				_mMaxBufferChars = LargeByteBufferSize / _mEncoding.GetMaxByteCount(1);
			}

			if (length > LargeByteBufferSize)
			{
				int current = 0;
				int charsLeft = value.Length;

				while (charsLeft > 0)
				{
					int charCount = charsLeft > _mMaxBufferChars ? _mMaxBufferChars : charsLeft;
					int byteLength = _mEncoding.GetBytes(value, current, charCount, _mCharacterBuffer, 0);

					if (_mIndex + byteLength > _mBuffer.Length)
						Flush();

					Buffer.BlockCopy(_mCharacterBuffer, 0, _mBuffer, _mIndex, byteLength);
					_mIndex += byteLength;

					current += charCount;
					charsLeft -= charCount;
				}
			}
			else
			{
				int byteLength = _mEncoding.GetBytes(value, 0, value.Length, _mCharacterBuffer, 0);

				if (_mIndex + byteLength > _mBuffer.Length)
					Flush();

				Buffer.BlockCopy(_mCharacterBuffer, 0, _mBuffer, _mIndex, byteLength);
				_mIndex += byteLength;
			}
		}

		public override void Write(string value)
		{
			if (_prefixStrings)
			{
				if (value == null)
				{
					if (_mIndex + 1 > _mBuffer.Length)
						Flush();

					_mBuffer[_mIndex++] = 0;
				}
				else
				{
					if (_mIndex + 1 > _mBuffer.Length)
						Flush();

					_mBuffer[_mIndex++] = 1;

					InternalWriteString(value);
				}
			}
			else
			{
				InternalWriteString(value);
			}
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
			long ticks = value.Ticks;
			long now = DateTime.UtcNow.Ticks;

			TimeSpan d;

			try { d = new TimeSpan(ticks - now); }
			catch
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
			int[] bits = decimal.GetBits(value);

			for (int i = 0; i < bits.Length; ++i)
				Write(bits[i]);
		}

		public override void Write(long value)
		{
			if (_mIndex + 8 > _mBuffer.Length)
				Flush();

			_mBuffer[_mIndex] = (byte)value;
			_mBuffer[_mIndex + 1] = (byte)(value >> 8);
			_mBuffer[_mIndex + 2] = (byte)(value >> 16);
			_mBuffer[_mIndex + 3] = (byte)(value >> 24);
			_mBuffer[_mIndex + 4] = (byte)(value >> 32);
			_mBuffer[_mIndex + 5] = (byte)(value >> 40);
			_mBuffer[_mIndex + 6] = (byte)(value >> 48);
			_mBuffer[_mIndex + 7] = (byte)(value >> 56);
			_mIndex += 8;
		}

		public override void Write(ulong value)
		{
			if (_mIndex + 8 > _mBuffer.Length)
				Flush();

			_mBuffer[_mIndex] = (byte)value;
			_mBuffer[_mIndex + 1] = (byte)(value >> 8);
			_mBuffer[_mIndex + 2] = (byte)(value >> 16);
			_mBuffer[_mIndex + 3] = (byte)(value >> 24);
			_mBuffer[_mIndex + 4] = (byte)(value >> 32);
			_mBuffer[_mIndex + 5] = (byte)(value >> 40);
			_mBuffer[_mIndex + 6] = (byte)(value >> 48);
			_mBuffer[_mIndex + 7] = (byte)(value >> 56);
			_mIndex += 8;
		}

		public override void Write(int value)
		{
			if (_mIndex + 4 > _mBuffer.Length)
				Flush();

			_mBuffer[_mIndex] = (byte)value;
			_mBuffer[_mIndex + 1] = (byte)(value >> 8);
			_mBuffer[_mIndex + 2] = (byte)(value >> 16);
			_mBuffer[_mIndex + 3] = (byte)(value >> 24);
			_mIndex += 4;
		}

		public override void Write(uint value)
		{
			if (_mIndex + 4 > _mBuffer.Length)
				Flush();

			_mBuffer[_mIndex] = (byte)value;
			_mBuffer[_mIndex + 1] = (byte)(value >> 8);
			_mBuffer[_mIndex + 2] = (byte)(value >> 16);
			_mBuffer[_mIndex + 3] = (byte)(value >> 24);
			_mIndex += 4;
		}

		public override void Write(short value)
		{
			if (_mIndex + 2 > _mBuffer.Length)
				Flush();

			_mBuffer[_mIndex] = (byte)value;
			_mBuffer[_mIndex + 1] = (byte)(value >> 8);
			_mIndex += 2;
		}

		public override void Write(ushort value)
		{
			if (_mIndex + 2 > _mBuffer.Length)
				Flush();

			_mBuffer[_mIndex] = (byte)value;
			_mBuffer[_mIndex + 1] = (byte)(value >> 8);
			_mIndex += 2;
		}

		public override unsafe void Write(double value)
		{
			if (_mIndex + 8 > _mBuffer.Length)
				Flush();

			fixed (byte* pBuffer = _mBuffer)
				*((double*)(pBuffer + _mIndex)) = value;

			_mIndex += 8;
		}

		public override unsafe void Write(float value)
		{
			if (_mIndex + 4 > _mBuffer.Length)
				Flush();

			fixed (byte* pBuffer = _mBuffer)
				*((float*)(pBuffer + _mIndex)) = value;

			_mIndex += 4;
		}

		private readonly char[] _mSingleCharBuffer = new char[1];

		public override void Write(char value)
		{
			if (_mIndex + 8 > _mBuffer.Length)
				Flush();

			_mSingleCharBuffer[0] = value;

			int byteCount = _mEncoding.GetBytes(_mSingleCharBuffer, 0, 1, _mBuffer, _mIndex);
			_mIndex += byteCount;
		}

		public override void Write(byte value)
		{
			if (_mIndex + 1 > _mBuffer.Length)
				Flush();

			_mBuffer[_mIndex++] = value;
		}

		public override void Write(sbyte value)
		{
			if (_mIndex + 1 > _mBuffer.Length)
				Flush();

			_mBuffer[_mIndex++] = (byte)value;
		}

		public override void Write(bool value)
		{
			if (_mIndex + 1 > _mBuffer.Length)
				Flush();

			_mBuffer[_mIndex++] = (byte)(value ? 1 : 0);
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
				Write((byte)value.MapIndex);
			else
				Write((byte)0xFF);
		}

		public override void Write(Race value)
		{
			if (value != null)
				Write((byte)value.RaceIndex);
			else
				Write((byte)0xFF);
		}

		public override void Write(Item value)
		{
			if (value == null || value.Deleted)
				Write(Serial.MinusOne);
			else
				Write(value.Serial);
		}

		public override void Write(Mobile value)
		{
			if (value == null || value.Deleted)
				Write(Serial.MinusOne);
			else
				Write(value.Serial);
		}

		public override void Write(BaseGuild value)
		{
			if (value == null)
				Write(Serial.MinusOne);
			else
				Write(value.Serial);
		}

		public override void WriteItem<T>(T value)
		{
			Write(value);
		}

		public override void WriteMobile<T>(T value)
		{
			Write(value);
		}

		public override void WriteGuild<T>(T value)
		{
			Write(value);
		}

		public override void WriteMobileList(ArrayList list)
		{
			WriteMobileList(list, false);
		}
		public override void WriteMobileList(ArrayList list, bool tidy)
		{
			if (tidy)
			{
				for (int i = 0; i < list.Count;)
				{
					if (((Mobile)list[i]).Deleted)
						list.RemoveAt(i);
					else
						++i;
				}
			}

			Write(list.Count);

			for (int i = 0; i < list.Count; ++i)
				Write((Mobile)list[i]);
		}

		public override void WriteItemList(ArrayList list)
		{
			WriteItemList(list, false);
		}
		public override void WriteItemList(ArrayList list, bool tidy)
		{
			if (tidy)
			{
				for (int i = 0; i < list.Count;)
				{
					if (((Item)list[i]).Deleted)
						list.RemoveAt(i);
					else
						++i;
				}
			}

			Write(list.Count);

			for (int i = 0; i < list.Count; ++i)
				Write((Item)list[i]);
		}

		public override void WriteGuildList(ArrayList list)
		{
			WriteGuildList(list, false);
		}
		public override void WriteGuildList(ArrayList list, bool tidy)
		{
			if (tidy)
			{
				for (int i = 0; i < list.Count;)
				{
					if (((BaseGuild)list[i]).Disbanded)
						list.RemoveAt(i);
					else
						++i;
				}
			}

			Write(list.Count);

			for (int i = 0; i < list.Count; ++i)
				Write((BaseGuild)list[i]);
		}

		public override void Write(List<Item> list)
		{
			Write(list, false);
		}
		public override void Write(List<Item> list, bool tidy)
		{
			if (tidy)
			{
				for (int i = 0; i < list.Count;)
				{
					if (list[i].Deleted)
						list.RemoveAt(i);
					else
						++i;
				}
			}

			Write(list.Count);

			for (int i = 0; i < list.Count; ++i)
				Write(list[i]);
		}

		public override void WriteItemList<T>(List<T> list)
		{
			WriteItemList(list, false);
		}
		public override void WriteItemList<T>(List<T> list, bool tidy)
		{
			if (tidy)
			{
				for (int i = 0; i < list.Count;)
				{
					if (list[i].Deleted)
						list.RemoveAt(i);
					else
						++i;
				}
			}

			Write(list.Count);

			for (int i = 0; i < list.Count; ++i)
				Write(list[i]);
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

			foreach (Item item in set)
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

			foreach (T item in set)
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
				for (int i = 0; i < list.Count;)
				{
					if (list[i].Deleted)
						list.RemoveAt(i);
					else
						++i;
				}
			}

			Write(list.Count);

			for (int i = 0; i < list.Count; ++i)
				Write(list[i]);
		}

		public override void WriteMobileList<T>(List<T> list)
		{
			WriteMobileList(list, false);
		}
		public override void WriteMobileList<T>(List<T> list, bool tidy)
		{
			if (tidy)
			{
				for (int i = 0; i < list.Count;)
				{
					if (list[i].Deleted)
						list.RemoveAt(i);
					else
						++i;
				}
			}

			Write(list.Count);

			for (int i = 0; i < list.Count; ++i)
				Write(list[i]);
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

			foreach (Mobile mob in set)
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

			foreach (T mob in set)
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
				for (int i = 0; i < list.Count;)
				{
					if (list[i].Disbanded)
						list.RemoveAt(i);
					else
						++i;
				}
			}

			Write(list.Count);

			for (int i = 0; i < list.Count; ++i)
				Write(list[i]);
		}

		public override void WriteGuildList<T>(List<T> list)
		{
			WriteGuildList(list, false);
		}
		public override void WriteGuildList<T>(List<T> list, bool tidy)
		{
			if (tidy)
			{
				for (int i = 0; i < list.Count;)
				{
					if (list[i].Disbanded)
						list.RemoveAt(i);
					else
						++i;
				}
			}

			Write(list.Count);

			for (int i = 0; i < list.Count; ++i)
				Write(list[i]);
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

			foreach (BaseGuild guild in set)
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

			foreach (T guild in set)
			{
				Write(guild);
			}
		}
	}

	public sealed class BinaryFileReader : GenericReader
	{
		private readonly BinaryReader m_File;

		public BinaryFileReader(BinaryReader br) { m_File = br; }

		public void Close()
		{
			m_File.Close();
		}

		public long Position => m_File.BaseStream.Position;

		public long Seek(long offset, SeekOrigin origin)
		{
			return m_File.BaseStream.Seek(offset, origin);
		}

		public override string ReadString()
		{
			return ReadByte() != 0 ? m_File.ReadString() : null;
		}

		public override DateTime ReadDeltaTime()
		{
			long ticks = m_File.ReadInt64();
			long now = DateTime.UtcNow.Ticks;

			switch (ticks)
			{
				case > 0 when (ticks + now) < 0:
					return DateTime.MaxValue;
				case < 0 when (ticks + now) < 0:
					return DateTime.MinValue;
				default:
					try { return new DateTime(now + ticks); }
					catch
					{
						return ticks > 0 ? DateTime.MaxValue : DateTime.MinValue;
					}
			}
		}

		public override IPAddress ReadIpAddress()
		{
			return new IPAddress(m_File.ReadInt64());
		}

		public override int ReadEncodedInt()
		{
			int v = 0, shift = 0;
			byte b;

			do
			{
				b = m_File.ReadByte();
				v |= (b & 0x7F) << shift;
				shift += 7;
			} while (b >= 0x80);

			return v;
		}

		public override DateTime ReadDateTime()
		{
			return new DateTime(m_File.ReadInt64());
		}

		public override DateTimeOffset ReadDateTimeOffset()
		{
			long ticks = m_File.ReadInt64();
			TimeSpan offset = new(m_File.ReadInt64());

			return new DateTimeOffset(ticks, offset);
		}

		public override TimeSpan ReadTimeSpan()
		{
			return new TimeSpan(m_File.ReadInt64());
		}

		public override decimal ReadDecimal()
		{
			return m_File.ReadDecimal();
		}

		public override long ReadLong()
		{
			return m_File.ReadInt64();
		}

		public override ulong ReadULong()
		{
			return m_File.ReadUInt64();
		}

		public override int PeekInt()
		{
			int value = 0;
			long returnTo = m_File.BaseStream.Position;

			try
			{
				value = m_File.ReadInt32();
			}
			catch (EndOfStreamException)
			{
				// Ignore this exception, the defalut value 0 will be returned
			}

			m_File.BaseStream.Seek(returnTo, SeekOrigin.Begin);
			return value;
		}

		public override int ReadInt()
		{
			return m_File.ReadInt32();
		}

		public override uint ReadUInt()
		{
			return m_File.ReadUInt32();
		}

		public override short ReadShort()
		{
			return m_File.ReadInt16();
		}

		public override ushort ReadUShort()
		{
			return m_File.ReadUInt16();
		}

		public override double ReadDouble()
		{
			return m_File.ReadDouble();
		}

		public override float ReadFloat()
		{
			return m_File.ReadSingle();
		}

		public override char ReadChar()
		{
			return m_File.ReadChar();
		}

		public override byte ReadByte()
		{
			return m_File.ReadByte();
		}

		public override sbyte ReadSByte()
		{
			return m_File.ReadSByte();
		}

		public override bool ReadBool()
		{
			return m_File.ReadBoolean();
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

		public override Item ReadItem()
		{
			return World.FindItem(ReadInt());
		}

		public override Mobile ReadMobile()
		{
			return World.FindMobile(ReadInt());
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
			int count = ReadInt();

			if (count <= 0) return new ArrayList();
			ArrayList list = new(count);

			for (int i = 0; i < count; ++i)
			{
				Item item = ReadItem();

				if (item != null)
				{
					list.Add(item);
				}
			}

			return list;

		}

		public override ArrayList ReadMobileList()
		{
			int count = ReadInt();

			if (count <= 0) return new ArrayList();
			ArrayList list = new(count);

			for (int i = 0; i < count; ++i)
			{
				Mobile m = ReadMobile();

				if (m != null)
				{
					list.Add(m);
				}
			}

			return list;

		}

		public override ArrayList ReadGuildList()
		{
			int count = ReadInt();

			if (count <= 0) return new ArrayList();
			ArrayList list = new(count);

			for (int i = 0; i < count; ++i)
			{
				BaseGuild g = ReadGuild();

				if (g != null)
				{
					list.Add(g);
				}
			}

			return list;

		}

		public override List<Item> ReadStrongItemList()
		{
			return ReadStrongItemList<Item>();
		}

		public override List<T> ReadStrongItemList<T>()
		{
			int count = ReadInt();

			if (count <= 0) return new List<T>();
			List<T> list = new(count);

			for (int i = 0; i < count; ++i)
			{
				if (ReadItem() is T item)
				{
					list.Add(item);
				}
			}

			return list;

		}

		public override HashSet<Item> ReadItemSet()
		{
			return ReadItemSet<Item>();
		}

		public override HashSet<T> ReadItemSet<T>()
		{
			int count = ReadInt();

			if (count <= 0) return new HashSet<T>();
			HashSet<T> set = new();

			for (int i = 0; i < count; ++i)
			{
				if (ReadItem() is T item)
				{
					set.Add(item);
				}
			}

			return set;

		}

		public override List<Mobile> ReadStrongMobileList()
		{
			return ReadStrongMobileList<Mobile>();
		}

		public override List<T> ReadStrongMobileList<T>()
		{
			int count = ReadInt();

			if (count <= 0) return new List<T>();
			List<T> list = new(count);

			for (int i = 0; i < count; ++i)
			{
				if (ReadMobile() is T m)
				{
					list.Add(m);
				}
			}

			return list;

		}

		public override HashSet<Mobile> ReadMobileSet()
		{
			return ReadMobileSet<Mobile>();
		}

		public override HashSet<T> ReadMobileSet<T>()
		{
			int count = ReadInt();

			if (count <= 0) return new HashSet<T>();
			HashSet<T> set = new();

			for (int i = 0; i < count; ++i)
			{
				if (ReadMobile() is T item)
				{
					set.Add(item);
				}
			}

			return set;

		}

		public override List<BaseGuild> ReadStrongGuildList()
		{
			return ReadStrongGuildList<BaseGuild>();
		}

		public override List<T> ReadStrongGuildList<T>()
		{
			int count = ReadInt();

			if (count <= 0) return new List<T>();
			List<T> list = new(count);

			for (int i = 0; i < count; ++i)
			{
				if (ReadGuild() is T g)
				{
					list.Add(g);
				}
			}

			return list;

		}

		public override HashSet<BaseGuild> ReadGuildSet()
		{
			return ReadGuildSet<BaseGuild>();
		}

		public override HashSet<T> ReadGuildSet<T>()
		{
			int count = ReadInt();

			if (count <= 0) return new HashSet<T>();
			HashSet<T> set = new();

			for (int i = 0; i < count; ++i)
			{
				if (ReadGuild() is T item)
				{
					set.Add(item);
				}
			}

			return set;

		}

		public override Race ReadRace()
		{
			return Race.Races[ReadByte()];
		}

		public override bool End()
		{
			return m_File.PeekChar() == -1;
		}
	}

	public sealed class AsyncWriter : GenericWriter
	{
		public static int ThreadCount { get; private set; }

		private readonly int _bufferSize;

		private long _mLastPos, _mCurPos;
		private bool _mClosed;
		private readonly bool _prefixStrings;

		private MemoryStream _mMem;
		private BinaryWriter _mBin;
		private readonly FileStream _mFile;

		private readonly Queue<MemoryStream> _mWriteQueue;
		private Thread _mWorkerThread;

		public AsyncWriter(string filename, bool prefix)
			: this(filename, 1048576, prefix)//1 mb buffer
		{
		}

		public AsyncWriter(string filename, int buffSize, bool prefix)
		{
			_prefixStrings = prefix;
			_mClosed = false;
			_mWriteQueue = new Queue<MemoryStream>();
			_bufferSize = buffSize;

			_mFile = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None);
			_mMem = new MemoryStream(_bufferSize + 1024);
			_mBin = new BinaryWriter(_mMem, Utility.UTF8WithEncoding);
		}

		private void Enqueue(MemoryStream mem)
		{
			lock (_mWriteQueue)
				_mWriteQueue.Enqueue(mem);

			if (_mWorkerThread != null && _mWorkerThread.IsAlive) return;
			_mWorkerThread = new Thread(new WorkerThread(this).Worker)
			{
				Priority = ThreadPriority.BelowNormal
			};
			_mWorkerThread.Start();
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

				int lastCount = 0;

				do
				{
					MemoryStream mem = null;

					lock (m_Owner._mWriteQueue)
					{
						if ((lastCount = m_Owner._mWriteQueue.Count) > 0)
							mem = m_Owner._mWriteQueue.Dequeue();
					}

					if (mem != null && mem.Length > 0)
						mem.WriteTo(m_Owner._mFile);
				} while (lastCount > 1);

				if (m_Owner._mClosed)
					m_Owner._mFile.Close();

				ThreadCount--;

				if (ThreadCount <= 0)
					World.NotifyDiskWriteComplete();
			}
		}

		private void OnWrite()
		{
			long curlen = _mMem.Length;
			_mCurPos += curlen - _mLastPos;
			_mLastPos = curlen;
			if (curlen < _bufferSize) return;
			Enqueue(_mMem);
			_mMem = new MemoryStream(_bufferSize + 1024);
			_mBin = new BinaryWriter(_mMem, Utility.UTF8WithEncoding);
			_mLastPos = 0;
		}

		public MemoryStream MemStream
		{
			get => _mMem;
			set
			{
				if (_mMem.Length > 0)
					Enqueue(_mMem);

				_mMem = value;
				_mBin = new BinaryWriter(_mMem, Utility.UTF8WithEncoding);
				_mLastPos = 0;
				_mCurPos = _mMem.Length;
				_mMem.Seek(0, SeekOrigin.End);
			}
		}

		public override void Close()
		{
			Enqueue(_mMem);
			_mClosed = true;
		}

		public override long Position => _mCurPos;

		public override void Write(IPAddress value)
		{
			_mBin.Write(Utility.GetLongAddressValue(value));
			OnWrite();
		}

		public override void Write(string value)
		{
			if (_prefixStrings)
			{
				if (value == null)
				{
					_mBin.Write((byte)0);
				}
				else
				{
					_mBin.Write((byte)1);
					_mBin.Write(value);
				}
			}
			else
			{
				_mBin.Write(value);
			}
			OnWrite();
		}

		public override void WriteDeltaTime(DateTime value)
		{
			long ticks = value.Ticks;
			long now = DateTime.UtcNow.Ticks;

			TimeSpan d;

			try { d = new TimeSpan(ticks - now); }
			catch
			{
				d = TimeSpan.MaxValue;
			}

			Write(d);
		}

		public override void Write(DateTime value)
		{
			_mBin.Write(value.Ticks);
			OnWrite();
		}

		public override void Write(DateTimeOffset value)
		{
			_mBin.Write(value.Ticks);
			_mBin.Write(value.Offset.Ticks);
			OnWrite();
		}

		public override void Write(TimeSpan value)
		{
			_mBin.Write(value.Ticks);
			OnWrite();
		}

		public override void Write(decimal value)
		{
			_mBin.Write(value);
			OnWrite();
		}

		public override void Write(long value)
		{
			_mBin.Write(value);
			OnWrite();
		}

		public override void Write(ulong value)
		{
			_mBin.Write(value);
			OnWrite();
		}

		public override void WriteEncodedInt(int value)
		{
			uint v = (uint)value;

			while (v >= 0x80)
			{
				_mBin.Write((byte)(v | 0x80));
				v >>= 7;
			}

			_mBin.Write((byte)v);
			OnWrite();
		}

		public override void Write(int value)
		{
			_mBin.Write(value);
			OnWrite();
		}

		public override void Write(uint value)
		{
			_mBin.Write(value);
			OnWrite();
		}

		public override void Write(short value)
		{
			_mBin.Write(value);
			OnWrite();
		}

		public override void Write(ushort value)
		{
			_mBin.Write(value);
			OnWrite();
		}

		public override void Write(double value)
		{
			_mBin.Write(value);
			OnWrite();
		}

		public override void Write(float value)
		{
			_mBin.Write(value);
			OnWrite();
		}

		public override void Write(char value)
		{
			_mBin.Write(value);
			OnWrite();
		}

		public override void Write(byte value)
		{
			_mBin.Write(value);
			OnWrite();
		}

		public override void Write(sbyte value)
		{
			_mBin.Write(value);
			OnWrite();
		}

		public override void Write(bool value)
		{
			_mBin.Write(value);
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
				Write((byte)value.MapIndex);
			else
				Write((byte)0xFF);
		}

		public override void Write(Race value)
		{
			if (value != null)
				Write((byte)value.RaceIndex);
			else
				Write((byte)0xFF);
		}

		public override void Write(Item value)
		{
			if (value == null || value.Deleted)
				Write(Serial.MinusOne);
			else
				Write(value.Serial);
		}

		public override void Write(Mobile value)
		{
			if (value == null || value.Deleted)
				Write(Serial.MinusOne);
			else
				Write(value.Serial);
		}

		public override void Write(BaseGuild value)
		{
			if (value == null)
				Write(0);
			else
				Write(value.Serial);
		}

		public override void WriteItem<T>(T value)
		{
			Write(value);
		}

		public override void WriteMobile<T>(T value)
		{
			Write(value);
		}

		public override void WriteGuild<T>(T value)
		{
			Write(value);
		}

		public override void WriteMobileList(ArrayList list)
		{
			WriteMobileList(list, false);
		}
		public override void WriteMobileList(ArrayList list, bool tidy)
		{
			if (tidy)
			{
				for (int i = 0; i < list.Count;)
				{
					if (((Mobile)list[i]).Deleted)
						list.RemoveAt(i);
					else
						++i;
				}
			}

			Write(list.Count);

			for (int i = 0; i < list.Count; ++i)
				Write((Mobile)list[i]);
		}

		public override void WriteItemList(ArrayList list)
		{
			WriteItemList(list, false);
		}
		public override void WriteItemList(ArrayList list, bool tidy)
		{
			if (tidy)
			{
				for (int i = 0; i < list.Count;)
				{
					if (((Item)list[i]).Deleted)
						list.RemoveAt(i);
					else
						++i;
				}
			}

			Write(list.Count);

			for (int i = 0; i < list.Count; ++i)
				Write((Item)list[i]);
		}

		public override void WriteGuildList(ArrayList list)
		{
			WriteGuildList(list, false);
		}
		public override void WriteGuildList(ArrayList list, bool tidy)
		{
			if (tidy)
			{
				for (int i = 0; i < list.Count;)
				{
					if (((BaseGuild)list[i]).Disbanded)
						list.RemoveAt(i);
					else
						++i;
				}
			}

			Write(list.Count);

			for (int i = 0; i < list.Count; ++i)
				Write((BaseGuild)list[i]);
		}

		public override void Write(List<Item> list)
		{
			Write(list, false);
		}
		public override void Write(List<Item> list, bool tidy)
		{
			if (tidy)
			{
				for (int i = 0; i < list.Count;)
				{
					if (list[i].Deleted)
						list.RemoveAt(i);
					else
						++i;
				}
			}

			Write(list.Count);

			for (int i = 0; i < list.Count; ++i)
				Write(list[i]);
		}

		public override void WriteItemList<T>(List<T> list)
		{
			WriteItemList(list, false);
		}
		public override void WriteItemList<T>(List<T> list, bool tidy)
		{
			if (tidy)
			{
				for (int i = 0; i < list.Count;)
				{
					if (list[i].Deleted)
						list.RemoveAt(i);
					else
						++i;
				}
			}

			Write(list.Count);

			for (int i = 0; i < list.Count; ++i)
				Write(list[i]);
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

			foreach (Item item in set)
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

			foreach (T item in set)
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
				for (int i = 0; i < list.Count;)
				{
					if (list[i].Deleted)
						list.RemoveAt(i);
					else
						++i;
				}
			}

			Write(list.Count);

			for (int i = 0; i < list.Count; ++i)
				Write(list[i]);
		}

		public override void WriteMobileList<T>(List<T> list)
		{
			WriteMobileList(list, false);
		}
		public override void WriteMobileList<T>(List<T> list, bool tidy)
		{
			if (tidy)
			{
				for (int i = 0; i < list.Count;)
				{
					if (list[i].Deleted)
						list.RemoveAt(i);
					else
						++i;
				}
			}

			Write(list.Count);

			for (int i = 0; i < list.Count; ++i)
				Write(list[i]);
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

			foreach (Mobile mob in set)
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

			foreach (T mob in set)
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
				for (int i = 0; i < list.Count;)
				{
					if (list[i].Disbanded)
						list.RemoveAt(i);
					else
						++i;
				}
			}

			Write(list.Count);

			for (int i = 0; i < list.Count; ++i)
				Write(list[i]);
		}

		public override void WriteGuildList<T>(List<T> list)
		{
			WriteGuildList(list, false);
		}
		public override void WriteGuildList<T>(List<T> list, bool tidy)
		{
			if (tidy)
			{
				for (int i = 0; i < list.Count;)
				{
					if (list[i].Disbanded)
						list.RemoveAt(i);
					else
						++i;
				}
			}

			Write(list.Count);

			for (int i = 0; i < list.Count; ++i)
				Write(list[i]);
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

			foreach (BaseGuild guild in set)
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

			foreach (T guild in set)
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
}
