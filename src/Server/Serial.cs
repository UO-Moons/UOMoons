using System;

namespace Server
{
	public struct Serial : IComparable, IComparable<Serial>
	{
		public static Serial LastMobile { get; private set; } = Zero;
		public static Serial LastItem { get; private set; } = 0x40000000;

		public static readonly Serial MinusOne = new(-1);
		public static readonly Serial Zero = new(0);

		public int Value { get; }

		public static Serial NewMobile
		{
			get
			{
				while (World.FindMobile(LastMobile += 1) != null) ;

				return LastMobile;
			}
		}

		public static Serial NewItem
		{
			get
			{
				while (World.FindItem(LastItem += 1) != null) ;

				return LastItem;
			}
		}

		public static Serial NewGuild => World.Guilds.Count;

		private Serial(int serial)
		{
			Value = serial;
		}

		public bool IsMobile => Value is > 0 and < 0x40000000;

		public bool IsItem => Value is >= 0x40000000 and <= 0x7FFFFFFF;

		public bool IsValid => Value > 0;

		public override int GetHashCode()
		{
			return Value;
		}

		public int CompareTo(Serial other)
		{
			return Value.CompareTo(other.Value);
		}

		public int CompareTo(object other)
		{
			return other switch
			{
				Serial serial => CompareTo(serial),
				null => -1,
				_ => throw new ArgumentException()
			};
		}

		public override bool Equals(object o)
		{
			if (o is not Serial serial) return false;

			return serial.Value == Value;
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

		/*public static Serial operator ++ ( Serial l )
		{
			return new Serial( l + 1 );
		}*/

		public override string ToString()
		{
			return $"0x{Value:X8}";
		}

		public static implicit operator int(Serial a)
		{
			return a.Value;
		}

		public static implicit operator Serial(int a)
		{
			return new Serial(a);
		}
	}
}
