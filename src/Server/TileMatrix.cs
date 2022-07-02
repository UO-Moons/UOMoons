using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Server;

public class TileMatrix
{
	private readonly StaticTile[][][][][] _staticTiles;
	private readonly LandTile[][][] _landTiles;

	private readonly LandTile[] _invalidLandBlock;
	private readonly UopIndex _mapIndex;

	private readonly int _fileIndex;

	private readonly Map _owner;
	private readonly int[][] _staticPatches;
	private readonly int[][] _landPatches;

	/*public Map Owner
	{
		get
		{
			return m_Owner;
		}
	}*/

	public TileMatrixPatch Patch { get; }

	public int BlockWidth { get; }

	public int BlockHeight { get; }

	/*public int Width
	{
		get
		{
			return m_Width;
		}
	}

	public int Height
	{
		get
		{
			return m_Height;
		}
	}*/

	public FileStream MapStream { get; set; }

	/*public bool MapUOPPacked
	{
		get{ return ( m_MapIndex != null ); }
	}*/

	public FileStream IndexStream { get; set; }

	public FileStream DataStream { get; set; }

	public BinaryReader IndexReader { get; set; }

	public bool Exists => MapStream != null && IndexStream != null && DataStream != null;

	private static readonly List<TileMatrix> m_Instances = new();
	private readonly List<TileMatrix> _fileShare = new();

	public TileMatrix(Map owner, int fileIndex, int mapId, int width, int height)
	{
		lock (m_Instances)
		{
			for (int i = 0; i < m_Instances.Count; ++i)
			{
				TileMatrix tm = m_Instances[i];

				if (tm._fileIndex == fileIndex)
				{
					lock (_fileShare)
					{
						lock (tm._fileShare)
						{
							tm._fileShare.Add(this);
							_fileShare.Add(tm);
						}
					}
				}
			}

			m_Instances.Add(this);
		}

		_fileIndex = fileIndex;
		BlockWidth = width >> 3;
		BlockHeight = height >> 3;

		_owner = owner;

		if (fileIndex != 0x7F)
		{
			string mapPath = Core.FindDataFile("map{0}.mul", fileIndex);

			if (File.Exists(mapPath))
			{
				MapStream = new FileStream(mapPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			}
			else
			{
				mapPath = Core.FindDataFile("map{0}LegacyMUL.uop", fileIndex);

				if (File.Exists(mapPath))
				{
					MapStream = new FileStream(mapPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
					_mapIndex = new UopIndex(MapStream);
				}
			}

			string indexPath = Core.FindDataFile("staidx{0}.mul", fileIndex);

			if (File.Exists(indexPath))
			{
				IndexStream = new FileStream(indexPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				IndexReader = new BinaryReader(IndexStream);
			}

			string staticsPath = Core.FindDataFile("statics{0}.mul", fileIndex);

			if (File.Exists(staticsPath))
				DataStream = new FileStream(staticsPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		}

		EmptyStaticBlock = new StaticTile[8][][];

		for (int i = 0; i < 8; ++i)
		{
			EmptyStaticBlock[i] = new StaticTile[8][];

			for (int j = 0; j < 8; ++j)
				EmptyStaticBlock[i][j] = Array.Empty<StaticTile>();
		}

		_invalidLandBlock = new LandTile[196];

		_landTiles = new LandTile[BlockWidth][][];
		_staticTiles = new StaticTile[BlockWidth][][][][];
		_staticPatches = new int[BlockWidth][];
		_landPatches = new int[BlockWidth][];

		Patch = new TileMatrixPatch(this, mapId);
	}

	public StaticTile[][][] EmptyStaticBlock { get; }

	[MethodImpl(MethodImplOptions.Synchronized)]
	public void SetStaticBlock(int x, int y, StaticTile[][][] value)
	{
		if (x < 0 || y < 0 || x >= BlockWidth || y >= BlockHeight)
			return;

		_staticTiles[x] ??= new StaticTile[BlockHeight][][][];

		_staticTiles[x][y] = value;

		_staticPatches[x] ??= new int[(BlockHeight + 31) >> 5];

		_staticPatches[x][y >> 5] |= 1 << (y & 0x1F);
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public StaticTile[][][] GetStaticBlock(int x, int y)
	{
		if (x < 0 || y < 0 || x >= BlockWidth || y >= BlockHeight || DataStream == null || IndexStream == null)
			return EmptyStaticBlock;

		_staticTiles[x] ??= new StaticTile[BlockHeight][][][];

		StaticTile[][][] tiles = _staticTiles[x][y];

		if (tiles == null)
		{
			lock (_fileShare)
			{
				for (int i = 0; tiles == null && i < _fileShare.Count; ++i)
				{
					TileMatrix shared = _fileShare[i];

					lock (shared)
					{
						if (x < shared.BlockWidth && y < shared.BlockHeight)
						{
							StaticTile[][][][] theirTiles = shared._staticTiles[x];

							if (theirTiles != null)
								tiles = theirTiles[y];

							if (tiles != null)
							{
								int[] theirBits = shared._staticPatches[x];

								if (theirBits != null && (theirBits[y >> 5] & (1 << (y & 0x1F))) != 0)
									tiles = null;
							}
						}
					}
				}
			}

			tiles ??= ReadStaticBlock(x, y);

			_staticTiles[x][y] = tiles;
		}

		return tiles;
	}

	public StaticTile[] GetStaticTiles(int x, int y)
	{
		StaticTile[][][] tiles = GetStaticBlock(x >> 3, y >> 3);

		return tiles[x & 0x7][y & 0x7];
	}

	private readonly TileList _tilesList = new();

	[MethodImpl(MethodImplOptions.Synchronized)]
	public StaticTile[] GetStaticTiles(int x, int y, bool multis)
	{
		StaticTile[][][] tiles = GetStaticBlock(x >> 3, y >> 3);

		if (multis)
		{
			IPooledEnumerable<StaticTile[]> eable = _owner.GetMultiTilesAt(x, y);

			if (eable == Map.NullEnumerable<StaticTile[]>.Instance)
				return tiles[x & 0x7][y & 0x7];

			bool any = false;

			foreach (StaticTile[] multiTiles in eable)
			{
				if (!any)
					any = true;

				_tilesList.AddRange(multiTiles);
			}

			eable.Free();

			if (!any)
				return tiles[x & 0x7][y & 0x7];

			_tilesList.AddRange(tiles[x & 0x7][y & 0x7]);

			return _tilesList.ToArray();
		}
		else
		{
			return tiles[x & 0x7][y & 0x7];
		}
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public void SetLandBlock(int x, int y, LandTile[] value)
	{
		if (x < 0 || y < 0 || x >= BlockWidth || y >= BlockHeight)
			return;

		_landTiles[x] ??= new LandTile[BlockHeight][];

		_landTiles[x][y] = value;

		_landPatches[x] ??= new int[(BlockHeight + 31) >> 5];

		_landPatches[x][y >> 5] |= 1 << (y & 0x1F);
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public LandTile[] GetLandBlock(int x, int y)
	{
		if (x < 0 || y < 0 || x >= BlockWidth || y >= BlockHeight || MapStream == null)
			return _invalidLandBlock;

		_landTiles[x] ??= new LandTile[BlockHeight][];

		LandTile[] tiles = _landTiles[x][y];

		if (tiles == null)
		{
			lock (_fileShare)
			{
				for (int i = 0; tiles == null && i < _fileShare.Count; ++i)
				{
					TileMatrix shared = _fileShare[i];

					lock (shared)
					{
						if (x < shared.BlockWidth && y < shared.BlockHeight)
						{
							LandTile[][] theirTiles = shared._landTiles[x];

							if (theirTiles != null)
								tiles = theirTiles[y];

							if (tiles != null)
							{
								int[] theirBits = shared._landPatches[x];

								if (theirBits != null && (theirBits[y >> 5] & (1 << (y & 0x1F))) != 0)
									tiles = null;
							}
						}
					}
				}
			}

			tiles ??= ReadLandBlock(x, y);

			_landTiles[x][y] = tiles;
		}

		return tiles;
	}

	public LandTile GetLandTile(int x, int y)
	{
		LandTile[] tiles = GetLandBlock(x >> 3, y >> 3);

		return tiles[((y & 0x7) << 3) + (x & 0x7)];
	}

	private TileList[][] _lists;

	private StaticTile[] _tileBuffer = new StaticTile[128];

	[MethodImpl(MethodImplOptions.Synchronized)]
	private unsafe StaticTile[][][] ReadStaticBlock(int x, int y)
	{
		try
		{
			IndexReader.BaseStream.Seek(((x * BlockHeight) + y) * 12, SeekOrigin.Begin);

			int lookup = IndexReader.ReadInt32();
			int length = IndexReader.ReadInt32();

			if (lookup < 0 || length <= 0)
			{
				return EmptyStaticBlock;
			}

			int count = length / 7;

			DataStream.Seek(lookup, SeekOrigin.Begin);

			if (_tileBuffer.Length < count)
				_tileBuffer = new StaticTile[count];

			StaticTile[] staTiles = _tileBuffer;//new StaticTile[tileCount];

			fixed (StaticTile* pTiles = staTiles)
			{
				if (DataStream.SafeFileHandle != null)
					NativeReader.Read(DataStream.SafeFileHandle.DangerousGetHandle(), pTiles, length);
				if (_lists == null)
				{
					_lists = new TileList[8][];

					for (int i = 0; i < 8; ++i)
					{
						_lists[i] = new TileList[8];

						for (int j = 0; j < 8; ++j)
							_lists[i][j] = new TileList();
					}
				}

				TileList[][] lists = _lists;

				StaticTile* pCur = pTiles, pEnd = pTiles + count;

				while (pCur < pEnd)
				{
					lists[pCur->m_X & 0x7][pCur->m_Y & 0x7].Add(pCur->m_ID, pCur->m_Z);
					pCur += 1;
				}

				StaticTile[][][] tiles = new StaticTile[8][][];

				for (int i = 0; i < 8; ++i)
				{
					tiles[i] = new StaticTile[8][];

					for (int j = 0; j < 8; ++j)
						tiles[i][j] = lists[i][j].ToArray();
				}

				return tiles;
			}
		}
		catch (EndOfStreamException)
		{
			if (DateTime.UtcNow >= _nextStaticWarning)
			{
				Console.WriteLine("Warning: Static EOS for {0} ({1}, {2})", _owner, x, y);
				_nextStaticWarning = DateTime.UtcNow + TimeSpan.FromMinutes(1.0);
			}

			return EmptyStaticBlock;
		}
	}

	private DateTime _nextStaticWarning;
	private DateTime _nextLandWarning;

	public static void Force()
	{
		if (Assembler.Assemblies == null || Assembler.Assemblies.Length == 0)
			throw new Exception();
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	private unsafe LandTile[] ReadLandBlock(int x, int y)
	{
		try
		{
			int offset = (x * BlockHeight + y) * 196 + 4;

			if (_mapIndex != null)
				offset = _mapIndex.Lookup(offset);

			MapStream.Seek(offset, SeekOrigin.Begin);

			LandTile[] tiles = new LandTile[64];

			fixed (LandTile* pTiles = tiles)
			{
				if (MapStream.SafeFileHandle != null)
					NativeReader.Read(MapStream.SafeFileHandle.DangerousGetHandle(), pTiles, 192);
			}

			return tiles;
		}
		catch
		{
			if (DateTime.UtcNow >= _nextLandWarning)
			{
				Console.WriteLine("Warning: Land EOS for {0} ({1}, {2})", _owner, x, y);
				_nextLandWarning = DateTime.UtcNow + TimeSpan.FromMinutes(1.0);
			}

			return _invalidLandBlock;
		}
	}

	public void Dispose()
	{
		if (_mapIndex != null)
			_mapIndex.Close();
		else
		{
			MapStream?.Close();
		}

		DataStream?.Close();

		IndexReader?.Close();
	}
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct LandTile
{
	internal short m_ID;
	internal sbyte m_Z;

	public int Id => m_ID;

	public int Z
	{
		get => m_Z;
		set => m_Z = (sbyte)value;
	}

	public static int Height => 0;

	public bool Ignored => m_ID is 2 or 0x1DB or >= 0x1AE and <= 0x1B5;

	public LandTile(short id, sbyte z)
	{
		m_ID = id;
		m_Z = z;
	}

	public void Set(short id, sbyte z)
	{
		m_ID = id;
		m_Z = z;
	}
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct StaticTile
{
	internal ushort m_ID;
	internal byte m_X;
	internal byte m_Y;
	internal sbyte m_Z;
	internal short m_Hue;

	public int Id => m_ID;

	public int X
	{
		get => m_X;
		set => m_X = (byte)value;
	}

	public int Y
	{
		get => m_Y;
		set => m_Y = (byte)value;
	}

	public int Z
	{
		get => m_Z;
		set => m_Z = (sbyte)value;
	}

	public int Hue
	{
		get => m_Hue;
		set => m_Hue = (short)value;
	}

	public int Height => TileData.ItemTable[m_ID & TileData.MaxItemValue].Height;

	public StaticTile(ushort id, sbyte z)
	{
		m_ID = id;
		m_Z = z;

		m_X = 0;
		m_Y = 0;
		m_Hue = 0;
	}

	public StaticTile(ushort id, byte x, byte y, sbyte z, short hue)
	{
		m_ID = id;
		m_X = x;
		m_Y = y;
		m_Z = z;
		m_Hue = hue;
	}

	public void Set(ushort id, sbyte z)
	{
		m_ID = id;
		m_Z = z;
	}

	public void Set(ushort id, byte x, byte y, sbyte z, short hue)
	{
		m_ID = id;
		m_X = x;
		m_Y = y;
		m_Z = z;
		m_Hue = hue;
	}
}

public class UopIndex
{
	private class UopEntry : IComparable<UopEntry>
	{
		public int Offset;
		public readonly int Length;
		public int Order;

		public UopEntry(int offset, int length)
		{
			Offset = offset;
			Length = length;
			Order = 0;
		}

		public int CompareTo(UopEntry other)
		{
			return Order.CompareTo(other.Order);
		}
	}

	private class OffsetComparer : IComparer<UopEntry>
	{
		public static readonly IComparer<UopEntry> Instance = new OffsetComparer();

		public int Compare(UopEntry x, UopEntry y)
		{
			return x!.Offset.CompareTo(y!.Offset);
		}
	}

	private readonly BinaryReader _reader;
	private readonly int _length;
	private readonly UopEntry[] _entries;

	public int Version { get; }

	public UopIndex(Stream stream)
	{
		_reader = new BinaryReader(stream);
		_length = (int)stream.Length;

		if (_reader.ReadInt32() != 0x50594D)
			throw new ArgumentException("Invalid UOP file.");

		Version = _reader.ReadInt32();
		_reader.ReadInt32();
		int nextTable = _reader.ReadInt32();

		List<UopEntry> entries = new();

		do
		{
			stream.Seek(nextTable, SeekOrigin.Begin);
			int count = _reader.ReadInt32();
			nextTable = _reader.ReadInt32();
			_reader.ReadInt32();

			for (int i = 0; i < count; ++i)
			{
				int offset = _reader.ReadInt32();

				if (offset == 0)
				{
					stream.Seek(30, SeekOrigin.Current);
					continue;
				}

				_reader.ReadInt64();
				int length = _reader.ReadInt32();

				entries.Add(new UopEntry(offset, length));

				stream.Seek(18, SeekOrigin.Current);
			}
		}
		while (nextTable != 0 && nextTable < _length);

		entries.Sort(OffsetComparer.Instance);

		for (int i = 0; i < entries.Count; ++i)
		{
			stream.Seek(entries[i].Offset + 2, SeekOrigin.Begin);

			int dataOffset = _reader.ReadInt16();
			entries[i].Offset += 4 + dataOffset;

			stream.Seek(dataOffset, SeekOrigin.Current);
			entries[i].Order = _reader.ReadInt32();
		}

		entries.Sort();
		_entries = entries.ToArray();
	}

	public int Lookup(int offset)
	{
		int total = 0;

		for (int i = 0; i < _entries.Length; ++i)
		{
			int newTotal = total + _entries[i].Length;

			if (offset < newTotal)
				return _entries[i].Offset + (offset - total);

			total = newTotal;
		}

		return _length;
	}

	public void Close()
	{
		_reader.Close();
	}
}
