using System;

namespace Server;

[Parsable]
public struct Point2D : IPoint2D, IComparable, IComparable<Point2D>
{
	internal int m_X;
	internal int m_Y;

	public static readonly Point2D Zero = new(0, 0);

	public Point2D(int x, int y)
	{
		m_X = x;
		m_Y = y;
	}

	public Point2D(IPoint2D p) : this(p.X, p.Y)
	{
	}

	[CommandProperty(AccessLevel.Counselor)]
	public int X
	{
		get => m_X;
		set => m_X = value;
	}

	[CommandProperty(AccessLevel.Counselor)]
	public int Y
	{
		get => m_Y;
		set => m_Y = value;
	}

	public override string ToString()
	{
		return $"({m_X}, {m_Y})";
	}

	public static Point2D Parse(string value)
	{
		int start = value.IndexOf('(');
		int end = value.IndexOf(',', start + 1);

		string param1 = value[(start + 1)..end].Trim();

		start = end;
		end = value.IndexOf(')', start + 1);

		string param2 = value[(start + 1)..end].Trim();

		return new Point2D(Convert.ToInt32(param1), Convert.ToInt32(param2));
	}

	public int CompareTo(Point2D other)
	{
		int v = m_X.CompareTo(other.m_X);

		if (v == 0)
			v = m_Y.CompareTo(other.m_Y);

		return v;
	}

	public int CompareTo(object other)
	{
		return other switch
		{
			Point2D point => CompareTo(point),
			null => -1,
			_ => throw new ArgumentException(nameof(other))
		};
	}

	public override bool Equals(object o)
	{
		if (o is not IPoint2D point2D) return false;

		return m_X == point2D.X && m_Y == point2D.Y;
	}

	public override int GetHashCode()
	{
		return m_X ^ m_Y;
	}

	public static bool operator ==(Point2D l, Point2D r)
	{
		return l.m_X == r.m_X && l.m_Y == r.m_Y;
	}

	public static bool operator !=(Point2D l, Point2D r)
	{
		return l.m_X != r.m_X || l.m_Y != r.m_Y;
	}

	public static bool operator ==(Point2D l, IPoint2D r)
	{
		if (r is null)
			return false;

		return l.m_X == r.X && l.m_Y == r.Y;
	}

	public static bool operator !=(Point2D l, IPoint2D r)
	{
		if (r is null)
			return false;

		return l.m_X != r.X || l.m_Y != r.Y;
	}

	public static bool operator >(Point2D l, Point2D r)
	{
		return l.m_X > r.m_X && l.m_Y > r.m_Y;
	}

	public static bool operator >(Point2D l, Point3D r)
	{
		return l.m_X > r.m_X && l.m_Y > r.m_Y;
	}

	public static bool operator >(Point2D l, IPoint2D r)
	{
		if (r is null)
			return false;

		return l.m_X > r.X && l.m_Y > r.Y;
	}

	public static bool operator <(Point2D l, Point2D r)
	{
		return l.m_X < r.m_X && l.m_Y < r.m_Y;
	}

	public static bool operator <(Point2D l, Point3D r)
	{
		return l.m_X < r.m_X && l.m_Y < r.m_Y;
	}

	public static bool operator <(Point2D l, IPoint2D r)
	{
		if (r is null)
			return false;

		return l.m_X < r.X && l.m_Y < r.Y;
	}

	public static bool operator >=(Point2D l, Point2D r)
	{
		return l.m_X >= r.m_X && l.m_Y >= r.m_Y;
	}

	public static bool operator >=(Point2D l, Point3D r)
	{
		return l.m_X >= r.m_X && l.m_Y >= r.m_Y;
	}

	public static bool operator >=(Point2D l, IPoint2D r)
	{
		if (r is null)
			return false;

		return l.m_X >= r.X && l.m_Y >= r.Y;
	}

	public static bool operator <=(Point2D l, Point2D r)
	{
		return l.m_X <= r.m_X && l.m_Y <= r.m_Y;
	}

	public static bool operator <=(Point2D l, Point3D r)
	{
		return l.m_X <= r.m_X && l.m_Y <= r.m_Y;
	}

	public static bool operator <=(Point2D l, IPoint2D r)
	{
		if (r is null)
			return false;

		return l.m_X <= r.X && l.m_Y <= r.Y;
	}
}

[Parsable]
public struct Point3D : IPoint3D, IComparable, IComparable<Point3D>
{
	internal int m_X;
	internal int m_Y;
	internal int m_Z;

	public static readonly Point3D Zero = new(0, 0, 0);

	public Point3D(int x, int y, int z)
	{
		m_X = x;
		m_Y = y;
		m_Z = z;
	}

	public Point3D(IPoint3D p)
		: this(p.X, p.Y, p.Z)
	{
	}

	public Point3D(IPoint2D p, int z)
		: this(p.X, p.Y, z)
	{
	}

	[CommandProperty(AccessLevel.Counselor)]
	public int X
	{
		get => m_X;
		set => m_X = value;
	}

	[CommandProperty(AccessLevel.Counselor)]
	public int Y
	{
		get => m_Y;
		set => m_Y = value;
	}

	[CommandProperty(AccessLevel.Counselor)]
	public int Z
	{
		get => m_Z;
		set => m_Z = value;
	}

	public override string ToString()
	{
		return $"({m_X}, {m_Y}, {m_Z})";
	}

	public override bool Equals(object o)
	{
		if (o is not IPoint3D point3D)
			return false;

		return m_X == point3D.X && m_Y == point3D.Y && m_Z == point3D.Z;
	}

	//public override int GetHashCode()
	//{
	//	return m_X ^ m_Y ^ m_Z;
	//}

	public override int GetHashCode()
	{
		unchecked
		{
			var hash = 1 + Math.Abs(X) + Math.Abs(Y) + Math.Abs(Z);

			hash = (hash * 397) ^ X;
			hash = (hash * 397) ^ Y;
			hash = (hash * 397) ^ Z;

			return hash;
		}
	}

	public static Point3D Parse(string value)
	{
		int start = value.IndexOf('(');
		int end = value.IndexOf(',', start + 1);

		string param1 = value[(start + 1)..end].Trim();

		start = end;
		end = value.IndexOf(',', start + 1);

		string param2 = value[(start + 1)..end].Trim();

		start = end;
		end = value.IndexOf(')', start + 1);

		string param3 = value[(start + 1)..end].Trim();

		return new Point3D(Convert.ToInt32(param1), Convert.ToInt32(param2), Convert.ToInt32(param3));
	}

	public static bool operator ==(Point3D l, Point3D r)
	{
		return l.m_X == r.m_X && l.m_Y == r.m_Y && l.m_Z == r.m_Z;
	}

	public static bool operator !=(Point3D l, Point3D r)
	{
		return l.m_X != r.m_X || l.m_Y != r.m_Y || l.m_Z != r.m_Z;
	}

	public static bool operator ==(Point3D l, IPoint3D r)
	{
		if (r is null)
		{
			return false;
		}

		return l.m_X == r.X && l.m_Y == r.Y && l.m_Z == r.Z;
	}

	public static bool operator !=(Point3D l, IPoint3D r)
	{
		if (r is null)
			return false;

		return l.m_X != r.X || l.m_Y != r.Y || l.m_Z != r.Z;
	}

	public int CompareTo(Point3D other)
	{
		int v = m_X.CompareTo(other.m_X);

		if (v == 0)
		{
			v = m_Y.CompareTo(other.m_Y);

			if (v == 0)
				v = m_Z.CompareTo(other.m_Z);
		}

		return v;
	}

	public int CompareTo(object other)
	{
		return other switch
		{
			Point3D point => CompareTo(point),
			null => -1,
			_ => throw new ArgumentException()
		};
	}
}

[NoSort]
[Parsable]
[PropertyObject]
public struct Rectangle2D
{
	private Point2D _start;
	private Point2D _end;

	public Rectangle2D(IPoint2D start, IPoint2D end)
	{
		_start = new Point2D(start);
		_end = new Point2D(end);
	}

	public Rectangle2D(int x, int y, int width, int height)
	{
		_start = new Point2D(x, y);
		_end = new Point2D(x + width, y + height);
	}

	public void Set(int x, int y, int width, int height)
	{
		_start = new Point2D(x, y);
		_end = new Point2D(x + width, y + height);
	}

	public static Rectangle2D Parse(string value)
	{
		int start = value.IndexOf('(');
		int end = value.IndexOf(',', start + 1);

		string param1 = value[(start + 1)..end].Trim();

		start = end;
		end = value.IndexOf(',', start + 1);

		string param2 = value[(start + 1)..end].Trim();

		start = end;
		end = value.IndexOf(',', start + 1);

		string param3 = value[(start + 1)..end].Trim();

		start = end;
		end = value.IndexOf(')', start + 1);

		string param4 = value[(start + 1)..end].Trim();

		return new Rectangle2D(Convert.ToInt32(param1), Convert.ToInt32(param2), Convert.ToInt32(param3), Convert.ToInt32(param4));
	}

	[CommandProperty(AccessLevel.Counselor)]
	public Point2D Start
	{
		get => _start;
		set => _start = value;
	}

	[CommandProperty(AccessLevel.Counselor)]
	public Point2D End
	{
		get => _end;
		set => _end = value;
	}

	[CommandProperty(AccessLevel.Counselor)]
	public int X
	{
		get => _start.m_X;
		set => _start.m_X = value;
	}

	[CommandProperty(AccessLevel.Counselor)]
	public int Y
	{
		get => _start.m_Y;
		set => _start.m_Y = value;
	}

	[CommandProperty(AccessLevel.Counselor)]
	public int Width
	{
		get => _end.m_X - _start.m_X;
		set => _end.m_X = _start.m_X + value;
	}

	[CommandProperty(AccessLevel.Counselor)]
	public int Height
	{
		get => _end.m_Y - _start.m_Y;
		set => _end.m_Y = _start.m_Y + value;
	}

	public void MakeHold(Rectangle2D r)
	{
		if (r._start.m_X < _start.m_X)
			_start.m_X = r._start.m_X;

		if (r._start.m_Y < _start.m_Y)
			_start.m_Y = r._start.m_Y;

		if (r._end.m_X > _end.m_X)
			_end.m_X = r._end.m_X;

		if (r._end.m_Y > _end.m_Y)
			_end.m_Y = r._end.m_Y;
	}

	public bool Contains(Point3D p)
	{
		return _start.m_X <= p.m_X && _start.m_Y <= p.m_Y && _end.m_X > p.m_X && _end.m_Y > p.m_Y;
		//return ( m_Start <= p && m_End > p );
	}

	public bool Contains(Point2D p)
	{
		return _start.m_X <= p.m_X && _start.m_Y <= p.m_Y && _end.m_X > p.m_X && _end.m_Y > p.m_Y;
		//return ( m_Start <= p && m_End > p );
	}

	public bool Contains(IPoint2D p)
	{
		return _start <= p && _end > p;
	}

	public override string ToString()
	{
		return $"({X}, {Y})+({Width}, {Height})";
	}
}

[NoSort]
[PropertyObject]
public struct Rectangle3D
{
	private Point3D _start;
	private Point3D _end;

	public Rectangle3D(Point3D start, Point3D end)
	{
		_start = start;
		_end = end;
	}

	public Rectangle3D(int x, int y, int z, int width, int height, int depth)
	{
		_start = new Point3D(x, y, z);
		_end = new Point3D(x + width, y + height, z + depth);
	}

	[CommandProperty(AccessLevel.Counselor)]
	public Point3D Start
	{
		get => _start;
		set => _start = value;
	}

	[CommandProperty(AccessLevel.Counselor)]
	public Point3D End
	{
		get => _end;
		set => _end = value;
	}

	[CommandProperty(AccessLevel.Counselor)]
	public int Width => _end.X - _start.X;

	[CommandProperty(AccessLevel.Counselor)]
	public int Height => _end.Y - _start.Y;

	[CommandProperty(AccessLevel.Counselor)]
	public int Depth => _end.Z - _start.Z;

	public bool Contains(Point3D p)
	{
		return p.m_X >= _start.m_X
		       && p.m_X < _end.m_X
		       && p.m_Y >= _start.m_Y
		       && p.m_Y < _end.m_Y
		       && p.m_Z >= _start.m_Z
		       && p.m_Z < _end.m_Z;
	}

	public bool Contains(IPoint3D p)
	{
		return p.X >= _start.m_X
		       && p.X < _end.m_X
		       && p.Y >= _start.m_Y
		       && p.Y < _end.m_Y
		       && p.Z >= _start.m_Z
		       && p.Z < _end.m_Z;
	}
}
