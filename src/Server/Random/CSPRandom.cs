using System;
using System.Security.Cryptography;
using System.Threading;

namespace Server;

public sealed class CspRandom : IRandomImpl
{
	private readonly RandomNumberGenerator _csp = RandomNumberGenerator.Create();

	private const int BufferSize = 0x4000;
	private const int LargeRequest = 0x40;

	private byte[] _working = new byte[BufferSize];
	private byte[] _buffer = new byte[BufferSize];

	private int _index;

	private readonly object _sync = new();

	private readonly ManualResetEvent _filled = new(false);

	public CspRandom()
	{
		_csp.GetBytes(_working);
		ThreadPool.QueueUserWorkItem(Fill);
	}

	private void CheckSwap(int c)
	{
		if (_index + c < BufferSize)
			return;

		_filled.WaitOne();

		(_buffer, _working) = (_working, _buffer);
		_index = 0;

		_filled.Reset();

		ThreadPool.QueueUserWorkItem(Fill);
	}

	private void Fill(object o)
	{
		lock (_csp)
			_csp.GetBytes(_buffer);

		_filled.Set();
	}

	private void GetBytes(byte[] b)
	{
		int c = b.Length;

		lock (_sync)
		{
			CheckSwap(c);
			Buffer.BlockCopy(_working, _index, b, 0, c);
			_index += c;
		}
	}

	private void GetBytes(byte[] b, int offset, int count)
	{
		lock (_sync)
		{
			CheckSwap(count);
			Buffer.BlockCopy(_working, _index, b, offset, count);
			_index += count;
		}
	}

	public int Next(int c)
	{
		return (int)(c * NextDouble());
	}

	public bool NextBool()
	{
		return (NextByte() & 1) == 1;
	}

	private byte NextByte()
	{
		lock (_sync)
		{
			CheckSwap(1);
			return _working[_index++];
		}
	}

	public void NextBytes(byte[] b)
	{
		int c = b.Length;

		if (c >= LargeRequest)
		{
			lock (_csp)
				_csp.GetBytes(b);
			return;
		}
		GetBytes(b);
	}

	public unsafe double NextDouble()
	{
		byte[] b = new byte[8];

		if (BitConverter.IsLittleEndian)
		{
			b[7] = 0;
			GetBytes(b, 0, 7);
		}
		else
		{
			b[0] = 0;
			GetBytes(b, 1, 7);
		}

		ulong r = 0;
		fixed (byte* buf = b)
			r = *(ulong*)(&buf[0]) >> 3;

		/* double: 53 bits of significand precision
		 * ulong.MaxValue >> 11 = 9007199254740991
		 * 2^53 = 9007199254740992
		 */

		return (double)r / 9007199254740992;
	}
}
