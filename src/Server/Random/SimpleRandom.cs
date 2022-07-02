using System;

namespace Server;

public sealed class SimpleRandom : IRandomImpl
{
	private readonly Random _random = new();

	public int Next(int c)
	{
		int r;
		lock (_random)
			r = _random.Next(c);
		return r;
	}

	public bool NextBool()
	{
		return NextDouble() >= .5;
	}

	public void NextBytes(byte[] b)
	{
		lock (_random)
			_random.NextBytes(b);
	}

	public double NextDouble()
	{
		double r;
		lock (_random)
			r = _random.NextDouble();
		return r;
	}
}
