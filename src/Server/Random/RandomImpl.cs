using System;
using System.IO;

namespace Server;

public interface IHardwareRng
{
	bool IsSupported();
}

public enum RdRandError
{
	Unknown = -4,
	Unsupported = -3,
	Supported = -2,
	NotReady = -1,

	Failure = 0,

	Success = 1,
}

public interface IRandomImpl
{
	int Next(int c);
	bool NextBool();
	void NextBytes(byte[] b);
	double NextDouble();
}

/// <summary>
/// Handles random number generation.
/// </summary>
public static class RandomImpl
{
	private static readonly IRandomImpl m_Random;

	static RandomImpl()
	{
		if (Core.Is64Bit && File.Exists("rdrand64.dll"))
		{
			m_Random = new RdRand64();
		}
		else if (!Core.Is64Bit && File.Exists("rdrand32.dll"))
		{
			m_Random = new RdRand32();
		}
		else
		{
			m_Random = new SimpleRandom();
		}

		if (m_Random is IHardwareRng rNg)
		{
			if (!rNg.IsSupported())
			{
				m_Random = new CspRandom();
			}
		}
	}

	public static bool IsHardwareRng => m_Random is IHardwareRng;

	public static Type Type => m_Random.GetType();

	public static int Next(int c)
	{
		return m_Random.Next(c);
	}

	public static bool NextBool()
	{
		return m_Random.NextBool();
	}

	public static void NextBytes(byte[] b)
	{
		m_Random.NextBytes(b);
	}

	public static double NextDouble()
	{
		return m_Random.NextDouble();
	}
}
