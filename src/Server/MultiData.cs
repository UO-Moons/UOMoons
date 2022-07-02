using System;
using System.Collections.Generic;
using System.IO;
using Server.Network;

namespace Server;

public static class MultiData
{
	public static Dictionary<int, MultiComponentList> Components { get; }

	private static readonly BinaryReader m_IndexReader;
	private static readonly BinaryReader m_StreamReader;

	public static bool UsingUopFormat { get; }

	public static MultiComponentList GetComponents(int multiId)
	{
		MultiComponentList mcl;

		multiId &= 0x3FFF; // The value of the actual multi is shifted by 0x4000, so this is left alone.

		if (Components.ContainsKey(multiId))
		{
			mcl = Components[multiId];
		}
		else if (!UsingUopFormat)
		{
			Components[multiId] = mcl = Load(multiId);
		}
		else
		{
			mcl = MultiComponentList.Empty;
		}

		return mcl;
	}

	public static MultiComponentList Load(int multiId)
	{
		try
		{
			m_IndexReader.BaseStream.Seek(multiId * 12, SeekOrigin.Begin);

			int lookup = m_IndexReader.ReadInt32();
			int length = m_IndexReader.ReadInt32();

			if (lookup < 0 || length <= 0)
			{
				return MultiComponentList.Empty;
			}

			m_StreamReader.BaseStream.Seek(lookup, SeekOrigin.Begin);

			return new MultiComponentList(m_StreamReader, length / (MultiComponentList.PostHsFormat ? 16 : 12));
		}
		catch
		{
			return MultiComponentList.Empty;
		}
	}

	public static void UopLoad(string path)
	{
		var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
		BinaryReader streamReader = new(stream);

		// Head Information Start
		if (streamReader.ReadInt32() != 0x0050594D) // Not a UOP Files
			return;

		if (streamReader.ReadInt32() > 5) // Bad Version
			return;

		// Multi ID List Array Start
		var chunkIds = new Dictionary<ulong, int>();
		var chunkIds2 = new Dictionary<ulong, int>();

		UopHash.BuildChunkIDs(ref chunkIds, ref chunkIds2);
		// Multi ID List Array End

		streamReader.ReadUInt32();                      // format timestamp? 0xFD23EC43
		long startAddress = streamReader.ReadInt64();

		streamReader.ReadInt32();
		streamReader.ReadInt32();
		//int blockSize = streamReader.ReadInt32();       // files in each block
		//int totalSize = streamReader.ReadInt32();       // Total File Count

		stream.Seek(startAddress, SeekOrigin.Begin);    // Head Information End

		long nextBlock;

		do
		{
			int blockFileCount = streamReader.ReadInt32();
			nextBlock = streamReader.ReadInt64();

			int index = 0;

			do
			{
				long offset = streamReader.ReadInt64();

				int headerSize = streamReader.ReadInt32();          // header length
				int compressedSize = streamReader.ReadInt32();      // compressed size
				int decompressedSize = streamReader.ReadInt32();    // decompressed size

				ulong filehash = streamReader.ReadUInt64();         // filename hash (HashLittle2)
				streamReader.ReadUInt32();
				//uint datablockhash = streamReader.ReadUInt32();     // data hash (Adler32)
				short flag = streamReader.ReadInt16();              // compression method (0 = none, 1 = zlib)

				index++;

				if (offset == 0 || decompressedSize == 0 || filehash == 0x126D1E99DDEDEE0A) // Exclude housing.bin
					continue;

				// Multi ID Search Start
				if (!chunkIds.TryGetValue(filehash, out int chunkId))
				{
					if (chunkIds2.TryGetValue(filehash, out int tmpChunkId))
					{
						chunkId = tmpChunkId;
					}
				}
				// Multi ID Search End                        

				long positionpoint = stream.Position;  // save current position

				// Decompress Data Start
				stream.Seek(offset + headerSize, SeekOrigin.Begin);

				byte[] sourceData = new byte[compressedSize];

				if (stream.Read(sourceData, 0, compressedSize) != compressedSize)
					continue;

				byte[] data;

				if (flag == 1)
				{
					byte[] destData = new byte[decompressedSize];
					/*ZLibError error = */
					Compression.Compressor.Decompress(destData, ref decompressedSize, sourceData, compressedSize);

					data = destData;
				}
				else
				{
					data = sourceData;
				}
				// End Decompress Data

				var tileList = new List<MultiTileEntry>();

				using (MemoryStream fs = new(data))
				{
					using (BinaryReader reader = new(fs))
					{
						uint a = reader.ReadUInt32();
						uint count = reader.ReadUInt32();

						for (uint i = 0; i < count; i++)
						{
							ushort itemId = reader.ReadUInt16();
							short x = reader.ReadInt16();
							short y = reader.ReadInt16();
							short z = reader.ReadInt16();

							ushort flagint = reader.ReadUInt16();

							TileFlag flagg;

							switch (flagint)
							{
								default:
								{ flagg = TileFlag.Background; break; }
								case 1: { flagg = TileFlag.None; break; }
								case 257: { flagg = TileFlag.Generic; break; }
							}

							uint clilocsCount = reader.ReadUInt32();

							if (clilocsCount != 0)
							{
								fs.Seek(fs.Position + clilocsCount * 4, SeekOrigin.Begin); // binary block bypass
							}

							tileList.Add(new MultiTileEntry(itemId, x, y, z, flagg));
						}

						reader.Close();
					}
				}

				Components[chunkId] = new MultiComponentList(tileList);

				stream.Seek(positionpoint, SeekOrigin.Begin); // back to position
			}
			while (index < blockFileCount);
		}
		while (stream.Seek(nextBlock, SeekOrigin.Begin) != 0);

		chunkIds.Clear();
		chunkIds2.Clear();
	}

	static MultiData()
	{
		Components = new Dictionary<int, MultiComponentList>();

		string multicollectionPath = Core.FindDataFile("MultiCollection.uop");

		if (File.Exists(multicollectionPath))
		{
			UopLoad(multicollectionPath);
			UsingUopFormat = true;
		}
		else
		{
			string idxPath = Core.FindDataFile("multi.idx");
			string mulPath = Core.FindDataFile("multi.mul");

			if (File.Exists(idxPath) && File.Exists(mulPath))
			{
				var idx = new FileStream(idxPath, FileMode.Open, FileAccess.Read, FileShare.Read);
				m_IndexReader = new BinaryReader(idx);

				var stream = new FileStream(mulPath, FileMode.Open, FileAccess.Read, FileShare.Read);
				m_StreamReader = new BinaryReader(stream);

				string vdPath = Core.FindDataFile("verdata.mul");

				if (File.Exists(vdPath))
				{
					using (FileStream fs = new(vdPath, FileMode.Open, FileAccess.Read, FileShare.Read))
					{
						BinaryReader bin = new(fs);

						int count = bin.ReadInt32();

						for (int i = 0; i < count; ++i)
						{
							int file = bin.ReadInt32();
							int index = bin.ReadInt32();
							int lookup = bin.ReadInt32();
							int length = bin.ReadInt32();
							/*int extra = */
							bin.ReadInt32();

							if (file == 14 && index >= 0 && lookup >= 0 && length > 0)
							{
								bin.BaseStream.Seek(lookup, SeekOrigin.Begin);

								Components[index] = new MultiComponentList(bin, length / 12);

								bin.BaseStream.Seek(24 + i * 20, SeekOrigin.Begin);
							}
						}

						bin.Close();
					}
				}
			}
			else
			{
				Console.WriteLine("Warning: Multi data files not found!");
			}
		}
	}
}

public struct MultiTileEntry
{
	public ushort ItemId;
	public short OffsetX, OffsetY, OffsetZ;
	public TileFlag Flags;

	public MultiTileEntry(ushort itemId, short xOffset, short yOffset, short zOffset, TileFlag flags)
	{
		ItemId = itemId;
		OffsetX = xOffset;
		OffsetY = yOffset;
		OffsetZ = zOffset;
		Flags = flags;
	}
}

public sealed class MultiComponentList
{
	public static bool PostHsFormat { get; set; }

	private Point2D _min, _max, _center;
	public static readonly MultiComponentList Empty = new();

	public Point2D Min => _min;
	public Point2D Max => _max;

	public Point2D Center => _center;

	public int Width { get; private set; }
	public int Height { get; private set; }

	public StaticTile[][][] Tiles { get; private set; }
	public MultiTileEntry[] List { get; private set; }

	public void Add(int itemId, int x, int y, int z)
	{
		itemId &= TileData.MaxItemValue;
		itemId |= 0x10000;

		int vx = x + _center.m_X;
		int vy = y + _center.m_Y;

		if (vx >= 0 && vx < Width && vy >= 0 && vy < Height)
		{
			var oldTiles = Tiles[vx][vy];

			for (int i = oldTiles.Length - 1; i >= 0; --i)
			{
				ItemData data = TileData.ItemTable[itemId & TileData.MaxItemValue];

				if (oldTiles[i].Z == z && oldTiles[i].Height > 0 == data.Height > 0)
				{
					bool newIsRoof = (data.Flags & TileFlag.Roof) != 0;
					bool oldIsRoof = (TileData.ItemTable[oldTiles[i].Id & TileData.MaxItemValue].Flags & TileFlag.Roof) != 0;

					if (newIsRoof == oldIsRoof)
					{
						Remove(oldTiles[i].Id, x, y, z);
					}
				}
			}

			oldTiles = Tiles[vx][vy];

			var newTiles = new StaticTile[oldTiles.Length + 1];

			for (int i = 0; i < oldTiles.Length; ++i)
			{
				newTiles[i] = oldTiles[i];
			}

			newTiles[oldTiles.Length] = new StaticTile((ushort)itemId, (sbyte)z);

			Tiles[vx][vy] = newTiles;

			var oldList = List;
			var newList = new MultiTileEntry[oldList.Length + 1];

			for (int i = 0; i < oldList.Length; ++i)
			{
				newList[i] = oldList[i];
			}

			newList[oldList.Length] = new MultiTileEntry((ushort)itemId, (short)x, (short)y, (short)z, TileFlag.Background);

			List = newList;

			if (x < _min.m_X)
			{
				_min.m_X = x;
			}

			if (y < _min.m_Y)
			{
				_min.m_Y = y;
			}

			if (x > _max.m_X)
			{
				_max.m_X = x;
			}

			if (y > _max.m_Y)
			{
				_max.m_Y = y;
			}
		}
	}

	public void RemoveXyzh(int x, int y, int z, int minHeight)
	{
		int vx = x + _center.m_X;
		int vy = y + _center.m_Y;

		if (vx >= 0 && vx < Width && vy >= 0 && vy < Height)
		{
			var oldTiles = Tiles[vx][vy];

			for (int i = 0; i < oldTiles.Length; ++i)
			{
				StaticTile tile = oldTiles[i];

				if (tile.Z == z && tile.Height >= minHeight)
				{
					var newTiles = new StaticTile[oldTiles.Length - 1];

					for (int j = 0; j < i; ++j)
					{
						newTiles[j] = oldTiles[j];
					}

					for (int j = i + 1; j < oldTiles.Length; ++j)
					{
						newTiles[j - 1] = oldTiles[j];
					}

					Tiles[vx][vy] = newTiles;

					break;
				}
			}

			var oldList = List;

			for (int i = 0; i < oldList.Length; ++i)
			{
				MultiTileEntry tile = oldList[i];

				if (tile.OffsetX == (short)x && tile.OffsetY == (short)y && tile.OffsetZ == (short)z &&
				    TileData.ItemTable[tile.ItemId & TileData.MaxItemValue].Height >= minHeight)
				{
					var newList = new MultiTileEntry[oldList.Length - 1];

					for (int j = 0; j < i; ++j)
					{
						newList[j] = oldList[j];
					}

					for (int j = i + 1; j < oldList.Length; ++j)
					{
						newList[j - 1] = oldList[j];
					}

					List = newList;

					break;
				}
			}
		}
	}

	public void Remove(int itemId, int x, int y, int z)
	{
		int vx = x + _center.m_X;
		int vy = y + _center.m_Y;

		if (vx >= 0 && vx < Width && vy >= 0 && vy < Height)
		{
			var oldTiles = Tiles[vx][vy];

			for (int i = 0; i < oldTiles.Length; ++i)
			{
				StaticTile tile = oldTiles[i];

				if (tile.Id == itemId && tile.Z == z)
				{
					var newTiles = new StaticTile[oldTiles.Length - 1];

					for (int j = 0; j < i; ++j)
					{
						newTiles[j] = oldTiles[j];
					}

					for (int j = i + 1; j < oldTiles.Length; ++j)
					{
						newTiles[j - 1] = oldTiles[j];
					}

					Tiles[vx][vy] = newTiles;

					break;
				}
			}

			var oldList = List;

			for (int i = 0; i < oldList.Length; ++i)
			{
				MultiTileEntry tile = oldList[i];

				if (tile.ItemId == itemId && tile.OffsetX == (short)x && tile.OffsetY == (short)y &&
				    tile.OffsetZ == (short)z)
				{
					var newList = new MultiTileEntry[oldList.Length - 1];

					for (int j = 0; j < i; ++j)
					{
						newList[j] = oldList[j];
					}

					for (int j = i + 1; j < oldList.Length; ++j)
					{
						newList[j - 1] = oldList[j];
					}

					List = newList;

					break;
				}
			}
		}
	}

	public void Resize(int newWidth, int newHeight)
	{
		int oldWidth = Width, oldHeight = Height;
		var oldTiles = Tiles;

		int totalLength = 0;

		var newTiles = new StaticTile[newWidth][][];

		for (int x = 0; x < newWidth; ++x)
		{
			newTiles[x] = new StaticTile[newHeight][];

			for (int y = 0; y < newHeight; ++y)
			{
				if (x < oldWidth && y < oldHeight)
				{
					newTiles[x][y] = oldTiles[x][y];
				}
				else
				{
					newTiles[x][y] = Array.Empty<StaticTile>();
				}

				totalLength += newTiles[x][y].Length;
			}
		}

		Tiles = newTiles;
		List = new MultiTileEntry[totalLength];
		Width = newWidth;
		Height = newHeight;

		_min = Point2D.Zero;
		_max = Point2D.Zero;

		int index = 0;

		for (int x = 0; x < newWidth; ++x)
		{
			for (int y = 0; y < newHeight; ++y)
			{
				var tiles = newTiles[x][y];

				foreach (StaticTile tile in tiles)
				{
					int vx = x - _center.X;
					int vy = y - _center.Y;

					if (vx < _min.m_X)
					{
						_min.m_X = vx;
					}

					if (vy < _min.m_Y)
					{
						_min.m_Y = vy;
					}

					if (vx > _max.m_X)
					{
						_max.m_X = vx;
					}

					if (vy > _max.m_Y)
					{
						_max.m_Y = vy;
					}

					List[index++] = new MultiTileEntry((ushort)tile.Id, (short)vx, (short)vy, (short)tile.Z, TileFlag.Background);
				}
			}
		}
	}

	public MultiComponentList(MultiComponentList toCopy)
	{
		_min = toCopy._min;
		_max = toCopy._max;

		_center = toCopy._center;

		Width = toCopy.Width;
		Height = toCopy.Height;

		Tiles = new StaticTile[Width][][];

		for (int x = 0; x < Width; ++x)
		{
			Tiles[x] = new StaticTile[Height][];

			for (int y = 0; y < Height; ++y)
			{
				Tiles[x][y] = new StaticTile[toCopy.Tiles[x][y].Length];

				for (int i = 0; i < Tiles[x][y].Length; ++i)
				{
					Tiles[x][y][i] = toCopy.Tiles[x][y][i];
				}
			}
		}

		List = new MultiTileEntry[toCopy.List.Length];

		for (int i = 0; i < List.Length; ++i)
		{
			List[i] = toCopy.List[i];
		}
	}

	public void Serialize(GenericWriter writer)
	{
		writer.Write(2); // version;

		writer.Write(_min);
		writer.Write(_max);
		writer.Write(_center);

		writer.Write(Width);
		writer.Write(Height);

		writer.Write(List.Length);

		foreach (MultiTileEntry ent in List)
		{
			writer.Write(ent.ItemId);
			writer.Write(ent.OffsetX);
			writer.Write(ent.OffsetY);
			writer.Write(ent.OffsetZ);

			writer.Write((ulong)ent.Flags);
		}
	}

	public MultiComponentList(GenericReader reader)
	{
		int version = reader.ReadInt();

		_min = reader.ReadPoint2D();
		_max = reader.ReadPoint2D();
		_center = reader.ReadPoint2D();
		Width = reader.ReadInt();
		Height = reader.ReadInt();

		int length = reader.ReadInt();

		var allTiles = List = new MultiTileEntry[length];

		if (version == 0)
		{
			for (int i = 0; i < length; ++i)
			{
				int id = reader.ReadShort();

				if (id >= 0x4000)
				{
					id -= 0x4000;
				}

				allTiles[i].ItemId = (ushort)id;
				allTiles[i].OffsetX = reader.ReadShort();
				allTiles[i].OffsetY = reader.ReadShort();
				allTiles[i].OffsetZ = reader.ReadShort();

				allTiles[i].Flags = (TileFlag)reader.ReadUInt();
			}
		}
		else
		{
			for (int i = 0; i < length; ++i)
			{
				allTiles[i].ItemId = reader.ReadUShort();
				allTiles[i].OffsetX = reader.ReadShort();
				allTiles[i].OffsetY = reader.ReadShort();
				allTiles[i].OffsetZ = reader.ReadShort();

				if (version > 1)
					allTiles[i].Flags = (TileFlag)reader.ReadULong();
				else
					allTiles[i].Flags = (TileFlag)reader.ReadUInt();
			}
		}

		var tiles = new TileList[Width][];
		Tiles = new StaticTile[Width][][];

		for (int x = 0; x < Width; ++x)
		{
			tiles[x] = new TileList[Height];
			Tiles[x] = new StaticTile[Height][];

			for (int y = 0; y < Height; ++y)
			{
				tiles[x][y] = new TileList();
			}
		}

		for (int i = 0; i < allTiles.Length; ++i)
		{
			if (i == 0 || allTiles[i].Flags != 0)
			{
				int xOffset = allTiles[i].OffsetX + _center.m_X;
				int yOffset = allTiles[i].OffsetY + _center.m_Y;
				int itemId = (allTiles[i].ItemId & TileData.MaxItemValue) | 0x10000;

				tiles[xOffset][yOffset].Add((ushort)itemId, (sbyte)allTiles[i].OffsetZ);
			}
		}

		for (int x = 0; x < Width; ++x)
		{
			for (int y = 0; y < Height; ++y)
			{
				Tiles[x][y] = tiles[x][y].ToArray();
			}
		}
	}

	public MultiComponentList(BinaryReader reader, int count)
	{
		var allTiles = List = new MultiTileEntry[count];

		for (int i = 0; i < count; ++i)
		{
			allTiles[i].ItemId = reader.ReadUInt16();
			allTiles[i].OffsetX = reader.ReadInt16();
			allTiles[i].OffsetY = reader.ReadInt16();
			allTiles[i].OffsetZ = reader.ReadInt16();

			if (PostHsFormat)
				allTiles[i].Flags = (TileFlag)reader.ReadUInt64();
			else
				allTiles[i].Flags = (TileFlag)reader.ReadUInt32();

			MultiTileEntry e = allTiles[i];

			if (i == 0 || e.Flags != 0)
			{
				if (e.OffsetX < _min.m_X)
				{
					_min.m_X = e.OffsetX;
				}

				if (e.OffsetY < _min.m_Y)
				{
					_min.m_Y = e.OffsetY;
				}

				if (e.OffsetX > _max.m_X)
				{
					_max.m_X = e.OffsetX;
				}

				if (e.OffsetY > _max.m_Y)
				{
					_max.m_Y = e.OffsetY;
				}
			}
		}

		_center = new Point2D(-_min.m_X, -_min.m_Y);
		Width = _max.m_X - _min.m_X + 1;
		Height = _max.m_Y - _min.m_Y + 1;

		var tiles = new TileList[Width][];
		Tiles = new StaticTile[Width][][];

		for (int x = 0; x < Width; ++x)
		{
			tiles[x] = new TileList[Height];
			Tiles[x] = new StaticTile[Height][];

			for (int y = 0; y < Height; ++y)
			{
				tiles[x][y] = new TileList();
			}
		}

		for (int i = 0; i < allTiles.Length; ++i)
		{
			if (i == 0 || allTiles[i].Flags != 0)
			{
				int xOffset = allTiles[i].OffsetX + _center.m_X;
				int yOffset = allTiles[i].OffsetY + _center.m_Y;
				int itemId = (allTiles[i].ItemId & TileData.MaxItemValue) | 0x10000;

				tiles[xOffset][yOffset].Add((ushort)itemId, (sbyte)allTiles[i].OffsetZ);
			}
		}

		for (int x = 0; x < Width; ++x)
		{
			for (int y = 0; y < Height; ++y)
			{
				Tiles[x][y] = tiles[x][y].ToArray();
			}
		}
	}

	public MultiComponentList(IReadOnlyList<MultiTileEntry> list)
	{
		var allTiles = List = new MultiTileEntry[list.Count];

		for (int i = 0; i < list.Count; ++i)
		{
			allTiles[i].ItemId = list[i].ItemId;
			allTiles[i].OffsetX = list[i].OffsetX;
			allTiles[i].OffsetY = list[i].OffsetY;
			allTiles[i].OffsetZ = list[i].OffsetZ;

			allTiles[i].Flags = list[i].Flags;

			MultiTileEntry e = allTiles[i];

			if (i == 0 || e.Flags != 0)
			{
				if (e.OffsetX < _min.m_X)
				{
					_min.m_X = e.OffsetX;
				}

				if (e.OffsetY < _min.m_Y)
				{
					_min.m_Y = e.OffsetY;
				}

				if (e.OffsetX > _max.m_X)
				{
					_max.m_X = e.OffsetX;
				}

				if (e.OffsetY > _max.m_Y)
				{
					_max.m_Y = e.OffsetY;
				}
			}
		}

		_center = new Point2D(-_min.m_X, -_min.m_Y);
		Width = _max.m_X - _min.m_X + 1;
		Height = _max.m_Y - _min.m_Y + 1;

		var tiles = new TileList[Width][];
		Tiles = new StaticTile[Width][][];

		for (int x = 0; x < Width; ++x)
		{
			tiles[x] = new TileList[Height];
			Tiles[x] = new StaticTile[Height][];

			for (int y = 0; y < Height; ++y)
			{
				tiles[x][y] = new TileList();
			}
		}

		for (int i = 0; i < allTiles.Length; ++i)
		{
			if (i == 0 || allTiles[i].Flags != 0)
			{
				int xOffset = allTiles[i].OffsetX + _center.m_X;
				int yOffset = allTiles[i].OffsetY + _center.m_Y;
				int itemId = (allTiles[i].ItemId & TileData.MaxItemValue) | 0x10000;

				tiles[xOffset][yOffset].Add((ushort)itemId, (sbyte)allTiles[i].OffsetZ);
			}
		}

		for (int x = 0; x < Width; ++x)
		{
			for (int y = 0; y < Height; ++y)
			{
				Tiles[x][y] = tiles[x][y].ToArray();
			}
		}
	}

	private MultiComponentList()
	{
		Tiles = Array.Empty<StaticTile[][]>();
		List = Array.Empty<MultiTileEntry>();
	}
}

public class UopHash
{
	public static void BuildChunkIDs(ref Dictionary<ulong, int> chunkIds, ref Dictionary<ulong, int> chunkIds2)
	{

		string[] formats = GetHashFormat(out int maxId);

		for (int i = 0; i < maxId; ++i)
		{
			chunkIds[HashLittle2(string.Format(formats[0], i))] = i;
		}
		if (formats[1] != "")
		{
			for (int i = 0; i < maxId; ++i)
				chunkIds2[HashLittle2(string.Format(formats[1], i))] = i;
		}
	}

	private static string[] GetHashFormat(out int maxId)
	{
		/*
		 * MaxID is only used for constructing a lookup table.
		 * Decrease to save some possibly unneeded computation.
		 */
		maxId = 0x10000;

		return new[] { "build/multicollection/{0:000000}.bin", "" };
	}

	public static ulong HashLittle2(string s)
	{
		int length = s.Length;

		uint b, c;
		var a = b = c = 0xDEADBEEF + (uint)length;

		int k = 0;

		while (length > 12)
		{
			a += s[k];
			a += (uint)s[k + 1] << 8;
			a += (uint)s[k + 2] << 16;
			a += (uint)s[k + 3] << 24;
			b += s[k + 4];
			b += (uint)s[k + 5] << 8;
			b += (uint)s[k + 6] << 16;
			b += (uint)s[k + 7] << 24;
			c += s[k + 8];
			c += (uint)s[k + 9] << 8;
			c += (uint)s[k + 10] << 16;
			c += (uint)s[k + 11] << 24;

			a -= c; a ^= (c << 4) | (c >> 28); c += b;
			b -= a; b ^= (a << 6) | (a >> 26); a += c;
			c -= b; c ^= (b << 8) | (b >> 24); b += a;
			a -= c; a ^= (c << 16) | (c >> 16); c += b;
			b -= a; b ^= (a << 19) | (a >> 13); a += c;
			c -= b; c ^= (b << 4) | (b >> 28); b += a;

			length -= 12;
			k += 12;
		}

		if (length != 0)
		{
			switch (length)
			{
				case 12: c += (uint)s[k + 11] << 24; goto case 11;
				case 11: c += (uint)s[k + 10] << 16; goto case 10;
				case 10: c += (uint)s[k + 9] << 8; goto case 9;
				case 9: c += s[k + 8]; goto case 8;
				case 8: b += (uint)s[k + 7] << 24; goto case 7;
				case 7: b += (uint)s[k + 6] << 16; goto case 6;
				case 6: b += (uint)s[k + 5] << 8; goto case 5;
				case 5: b += s[k + 4]; goto case 4;
				case 4: a += (uint)s[k + 3] << 24; goto case 3;
				case 3: a += (uint)s[k + 2] << 16; goto case 2;
				case 2: a += (uint)s[k + 1] << 8; goto case 1;
				case 1: a += s[k]; break;
			}

			c ^= b; c -= (b << 14) | (b >> 18);
			a ^= c; a -= (c << 11) | (c >> 21);
			b ^= a; b -= (a << 25) | (a >> 7);
			c ^= b; c -= (b << 16) | (b >> 16);
			a ^= c; a -= (c << 4) | (c >> 28);
			b ^= a; b -= (a << 14) | (a >> 18);
			c ^= b; c -= (b << 24) | (b >> 8);
		}

		return ((ulong)b << 32) | c;
	}
}
