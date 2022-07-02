using Server.Network;
using System.IO;
using System.Text;

namespace Server;

public sealed class ObjectPropertyList : Packet
{
	private int _hash;
	private int _strings;

	public IEntity Entity { get; }
	public int Hash => 0x40000000 + _hash;

	public int Header { get; set; }
	public string HeaderArgs { get; set; }

	public static bool Enabled { get; set; }

	public ObjectPropertyList(IEntity e)
		: base(0xD6)
	{
		EnsureCapacity(128);

		Entity = e;

		m_Stream.Write((short)1);
		m_Stream.Write(e.Serial);
		m_Stream.Write((byte)0);
		m_Stream.Write((byte)0);
		m_Stream.Write(e.Serial);
	}

	public void Add(int number)
	{
		if (number == 0)
		{
			return;
		}

		AddHash(number);

		if (Header == 0)
		{
			Header = number;
			HeaderArgs = "";
		}

		m_Stream.Write(number);
		m_Stream.Write((short)0);
	}

	public void Terminate()
	{
		m_Stream.Write(0);

		m_Stream.Seek(11, SeekOrigin.Begin);
		m_Stream.Write(_hash);
	}

	private static byte[] _buffer = new byte[1024];
	private static readonly Encoding m_Encoding = Encoding.Unicode;

	public void AddHash(int val)
	{
		_hash ^= val & 0x3FFFFFF;
		_hash ^= (val >> 26) & 0x3F;
	}

	public void Add(int number, string arguments)
	{
		if (number == 0)
		{
			return;
		}

		arguments ??= "";

		if (Header == 0)
		{
			Header = number;
			HeaderArgs = arguments;
		}

		AddHash(number);
		AddHash(arguments.GetHashCode());

		m_Stream.Write(number);

		int byteCount = m_Encoding.GetByteCount(arguments);

		if (byteCount > _buffer.Length)
		{
			_buffer = new byte[byteCount];
		}

		byteCount = m_Encoding.GetBytes(arguments, 0, arguments.Length, _buffer, 0);

		m_Stream.Write((short)byteCount);
		m_Stream.Write(_buffer, 0, byteCount);
	}

	public void Add(int number, string format, object arg0)
	{
		Add(number, string.Format(format, arg0));
	}

	public void Add(int number, string format, object arg0, object arg1)
	{
		Add(number, string.Format(format, arg0, arg1));
	}

	public void Add(int number, string format, object arg0, object arg1, object arg2)
	{
		Add(number, string.Format(format, arg0, arg1, arg2));
	}

	public void Add(int number, string format, params object[] args)
	{
		Add(number, string.Format(format, args));
	}

	// Each of these are localized to "~1_NOTHING~" which allows the string argument to be used
	private static readonly int[] m_StringNumbers = new[] { 1042971, 1070722 };

	private int GetStringNumber()
	{
		return m_StringNumbers[_strings++ % m_StringNumbers.Length];
	}

	public void Add(string text)
	{
		Add(GetStringNumber(), text);
	}

	public void Add(string format, string arg0)
	{
		Add(GetStringNumber(), string.Format(format, arg0));
	}

	public void Add(string format, string arg0, string arg1)
	{
		Add(GetStringNumber(), string.Format(format, arg0, arg1));
	}

	public void Add(string format, string arg0, string arg1, string arg2)
	{
		Add(GetStringNumber(), string.Format(format, arg0, arg1, arg2));
	}

	public void Add(string format, params object[] args)
	{
		Add(GetStringNumber(), string.Format(format, args));
	}
}

public sealed class OplInfo : Packet
{
	/*public OPLInfo( ObjectPropertyList list ) : base( 0xBF )
	{
		EnsureCapacity( 13 );

		m_Stream.Write( (short) 0x10 );
		m_Stream.Write( (int) list.Entity.Serial );
		m_Stream.Write( (int) list.Hash );
	}*/

	public OplInfo(ObjectPropertyList list)
		: base(0xDC, 9)
	{
		m_Stream.Write(list.Entity.Serial);
		m_Stream.Write(list.Hash);
	}
}
