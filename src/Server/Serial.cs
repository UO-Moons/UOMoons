using System;

namespace Server;

public struct Serial : IComparable<Serial>, IEquatable<Serial>
{
	private static Serial _nextMobileQueued = new(0x3FFFFFFF);
	private static Serial _nextItemQueued = new(0x7FFFFFFF);

	private static Serial _nextMobile = new(0x00000001);
	private static Serial _nextItem = new(0x40000001);

	public static readonly Serial MinValue = new(int.MinValue);
	public static readonly Serial MaxValue = new(int.MaxValue);

	public static readonly Serial MinusOne = new(-1);
	public static readonly Serial Zero = new(0);

	public static Serial NewMobile
	{
		get
		{
			Serial s;

			if (World.Volatile)
			{
				while (World.FindMobile(_nextMobileQueued) != null)
					--_nextMobileQueued.Value;

				s = _nextMobileQueued;
			}
			else
			{
				while (World.FindMobile(_nextMobile) != null)
					++_nextMobile.Value;

				s = _nextMobile;
			}

			return s;
		}
	}

	public static Serial NewItem
	{
		get
		{
			Serial s;

			if (World.Volatile)
			{
				while (World.FindItem(_nextItemQueued) != null)
					--_nextItemQueued.Value;

				s = _nextItemQueued;
			}
			else
			{
				while (World.FindItem(_nextItem) != null)
					++_nextItem.Value;

				s = _nextItem;
			}

			return s;
		}
	}

	public int Value { get; private set; }

	public bool IsMobile => Value is > 0 and < 0x40000000;

	public bool IsItem => Value is >= 0x40000000 and <= 0x7FFFFFFF;

	public bool IsValid => Value > 0;

	public Serial(int serial)
	{
		Value = serial;
	}

	public override int GetHashCode()
	{
		return Value;
	}

	public int CompareTo(Serial other)
	{
		return Value.CompareTo(other.Value);
	}

	public int CompareTo(object o)
	{
		return o is Serial s ? CompareTo(s) : -1;
	}

	public bool Equals(Serial other)
	{
		return Value.Equals(other.Value);
	}

	public override bool Equals(object o)
	{
		return o is Serial s && Equals(s);
	}

	public static bool operator ==(Serial l, Serial r)
	{
		return l.Value == r.Value;
	}

	public static bool operator !=(Serial l, Serial r)
	{
		return l.Value != r.Value;
	}

	public static bool operator >(Serial l, Serial r)
	{
		return l.Value > r.Value;
	}

	public static bool operator <(Serial l, Serial r)
	{
		return l.Value < r.Value;
	}

	public static bool operator >=(Serial l, Serial r)
	{
		return l.Value >= r.Value;
	}

	public static bool operator <=(Serial l, Serial r)
	{
		return l.Value <= r.Value;
	}

	public override string ToString()
	{
		return $"0x{Value:X8}";
	}

	public static implicit operator int(Serial a)
	{
		return a.Value;
	}
	/*
	public static implicit operator Serial(int a)
	{
		return new Serial(a);
	}
	*/
}
