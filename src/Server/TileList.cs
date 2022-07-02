using System;

namespace Server;

public class TileList
{
	private StaticTile[] _tiles;

	public TileList()
	{
		_tiles = new StaticTile[8];
		Count = 0;
	}

	public int Count { get; private set; }

	public void AddRange(StaticTile[] tiles)
	{
		if (Count + tiles.Length > _tiles.Length)
		{
			StaticTile[] old = _tiles;
			_tiles = new StaticTile[(Count + tiles.Length) * 2];

			for (int i = 0; i < old.Length; ++i)
				_tiles[i] = old[i];
		}

		for (int i = 0; i < tiles.Length; ++i)
			_tiles[Count++] = tiles[i];
	}

	public void Add(ushort id, sbyte z)
	{
		if (Count + 1 > _tiles.Length)
		{
			StaticTile[] old = _tiles;
			_tiles = new StaticTile[old.Length * 2];

			for (int i = 0; i < old.Length; ++i)
				_tiles[i] = old[i];
		}

		_tiles[Count].m_ID = id;
		_tiles[Count].m_Z = z;
		++Count;
	}

	private static readonly StaticTile[] m_EmptyTiles = Array.Empty<StaticTile>();

	public StaticTile[] ToArray()
	{
		if (Count == 0)
			return m_EmptyTiles;

		StaticTile[] tiles = new StaticTile[Count];

		for (int i = 0; i < Count; ++i)
			tiles[i] = _tiles[i];

		Count = 0;

		return tiles;
	}
}
