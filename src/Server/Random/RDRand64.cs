using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Server;

public sealed class RdRand64 : IRandomImpl, IHardwareRng
{
	internal class SafeNativeMethods
	{
		[DllImport("rdrand64")]
		internal static extern RdRandError rdrand_64(ref ulong rand, bool retry);

		[DllImport("rdrand64")]
		internal static extern RdRandError rdrand_get_bytes(int n, byte[] buffer);
	}

	private const int BufferSize = 0x10000;
	private const int LargeRequest = 0x40;

	private byte[] _working = new byte[BufferSize];
	private byte[] _buffer = new byte[BufferSize];

	private int _index;

	private readonly object _sync = new();

	private readonly ManualResetEvent _filled = new(false);

	public RdRand64()
	{
		SafeNativeMethods.rdrand_get_bytes(BufferSize, _working);
		ThreadPool.QueueUserWorkItem(Fill);
	}

	public bool IsSupported()
	{
		ulong r = 0;
		return SafeNativeMethods.rdrand_64(ref r, true) == RdRandError.Success;
	}

	private void CheckSwap(int c)
	{
		if (_index + c < BufferSize)
			return;

		_filled.WaitOne();

		(_working, _buffer) = (_buffer, _working);
		_index = 0;

		_filled.Reset();

		ThreadPool.QueueUserWorkItem(Fill);
	}

	private void Fill(object o)
	{
		SafeNativeMethods.rdrand_get_bytes(BufferSize, _buffer);
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
			SafeNativeMethods.rdrand_get_bytes(c, b);
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

		ulong r;
		fixed (byte* buf = b)
			r = *(ulong*)(&buf[0]) >> 3;

		/* double: 53 bits of significand precision
		 * ulong.MaxValue >> 11 = 9007199254740991
		 * 2^53 = 9007199254740992
		 */

		return (double)r / 9007199254740992;
	}
}
