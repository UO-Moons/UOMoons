namespace Server;

public class Point3DList
{
	private Point3D[] _list;

	public int Count { get; private set; }

	public Point3DList()
	{
		_list = new Point3D[8];
		Count = 0;
	}

	public void Clear()
	{
		Count = 0;
	}

	public Point3D Last => _list[Count - 1];

	public Point3D this[int index] => _list[index];

	public void Add(int x, int y, int z)
	{
		if (Count + 1 > _list.Length)
		{
			Point3D[] old = _list;
			_list = new Point3D[old.Length * 2];

			for (int i = 0; i < old.Length; ++i)
				_list[i] = old[i];
		}

		_list[Count].m_X = x;
		_list[Count].m_Y = y;
		_list[Count].m_Z = z;
		++Count;
	}

	public void Add(Point3D p)
	{
		if (Count + 1 > _list.Length)
		{
			Point3D[] old = _list;
			_list = new Point3D[old.Length * 2];

			for (int i = 0; i < old.Length; ++i)
				_list[i] = old[i];
		}

		_list[Count].m_X = p.m_X;
		_list[Count].m_Y = p.m_Y;
		_list[Count].m_Z = p.m_Z;
		++Count;
	}

	private static readonly Point3D[] m_EmptyList = System.Array.Empty<Point3D>();

	public Point3D[] ToArray()
	{
		if (Count == 0)
			return m_EmptyList;

		Point3D[] list = new Point3D[Count];

		for (int i = 0; i < Count; ++i)
			list[i] = _list[i];

		Count = 0;

		return list;
	}
}
