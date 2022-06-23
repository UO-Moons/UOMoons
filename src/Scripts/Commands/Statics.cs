using Server.Commands;
using Server.Gumps;
using Server.Items;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Server;

public class Statics
{
	public static void Initialize()
	{
		CommandSystem.Register("Freeze", AccessLevel.Administrator, Freeze_OnCommand);
		CommandSystem.Register("FreezeMap", AccessLevel.Administrator, FreezeMap_OnCommand);
		CommandSystem.Register("FreezeWorld", AccessLevel.Administrator, FreezeWorld_OnCommand);

		CommandSystem.Register("Unfreeze", AccessLevel.Administrator, Unfreeze_OnCommand);
		CommandSystem.Register("UnfreezeMap", AccessLevel.Administrator, UnfreezeMap_OnCommand);
		CommandSystem.Register("UnfreezeWorld", AccessLevel.Administrator, UnfreezeWorld_OnCommand);
	}

	private static readonly Point3D NullP3D = new(int.MinValue, int.MinValue, int.MinValue);

	[Usage("Freeze")]
	[Description("Makes a targeted area of dynamic items static.")]
	public static void Freeze_OnCommand(CommandEventArgs e)
	{
		BoundingBoxPicker.Begin(e.Mobile, FreezeBox_Callback, null);
	}

	[Usage("FreezeMap")]
	[Description("Makes every dynamic item in your map static.")]
	public static void FreezeMap_OnCommand(CommandEventArgs e)
	{
		Map map = e.Mobile.Map;

		if (map != null && map != Map.Internal)
			SendWarning(e.Mobile, "You are about to freeze <u>all items in {0}</u>.", BaseFreezeWarning, map, NullP3D, NullP3D, FreezeWarning_Callback);
	}

	[Usage("FreezeWorld")]
	[Description("Makes every dynamic item on all maps static.")]
	public static void FreezeWorld_OnCommand(CommandEventArgs e)
	{
		SendWarning(e.Mobile, "You are about to freeze <u>every item on every map</u>.", BaseFreezeWarning, null, NullP3D, NullP3D, FreezeWarning_Callback);
	}

	public static void SendWarning(Mobile m, string header, string baseWarning, Map map, Point3D start, Point3D end, WarningGumpCallback callback)
	{
		m.SendGump(new WarningGump(1060635, 30720, string.Format(baseWarning, string.Format(header, map)), 0xFFC000, 420, 400, callback, new StateInfo(map, start, end)));
	}

	private const string BaseFreezeWarning = "{0}  " +
	                                         "Those items <u>will be removed from the world</u> and placed into the server data files.  " +
	                                         "Other players <u>will not see the changes</u> unless you distribute your data files to them.<br><br>" +
	                                         "This operation may not complete unless the server and client are using different data files.  " +
	                                         "If you receive a message stating 'output data files could not be opened,' then you are probably sharing data files.  " +
	                                         "Create a new directory for the world data files (statics*.mul and staidx*.mul) and add that to Scritps/Misc/DataPath.cs.<br><br>" +
	                                         "The change will be in effect immediately on the server, however, you must restart your client and update it's data files for the changes to become visible.  " +
	                                         "It is strongly recommended that you make backup of the data files mentioned above.  " +
	                                         "Do you wish to proceed?";

	private static void FreezeBox_Callback(Mobile from, Map map, Point3D start, Point3D end, object state)
	{
		SendWarning(from, "You are about to freeze a section of items.", BaseFreezeWarning, map, start, end, FreezeWarning_Callback);
	}

	private static void FreezeWarning_Callback(Mobile from, bool okay, object state)
	{
		if (!okay)
			return;

		StateInfo si = (StateInfo)state;

		Freeze(from, si.MMap, si.MStart, si.MEnd);
	}

	public static void Freeze(Mobile from, Map targetMap, Point3D start3d, Point3D end3d)
	{
		Hashtable mapTable = new();

		if (start3d == NullP3D && end3d == NullP3D)
		{
			if (targetMap == null)
				CommandLogging.WriteLine(from, "{0} {1} invoking freeze for every item in every map", from.AccessLevel, CommandLogging.Format(from));
			else
				CommandLogging.WriteLine(from, "{0} {1} invoking freeze for every item in {0}", from.AccessLevel, CommandLogging.Format(from), targetMap);

			foreach (Item item in World.Items.Values)
			{
				if (targetMap != null && item.Map != targetMap)
					continue;

				if (item.Parent != null)
					continue;

				if (item is Static or BaseFloor or BaseWall)
				{
					Map itemMap = item.Map;

					if (itemMap == null || itemMap == Map.Internal)
						continue;

					Hashtable table = (Hashtable)mapTable[itemMap];

					if (table == null)
						mapTable[itemMap] = table = new Hashtable();

					Point2D p = new(item.X >> 3, item.Y >> 3);

					DeltaState state = (DeltaState)table[p];

					if (state == null)
						table[p] = state = new DeltaState(p);

					state.MList.Add(item);
				}
			}
		}
		else if (targetMap != null)
		{
			Point2D start = targetMap.Bound(new Point2D(start3d)), end = targetMap.Bound(new Point2D(end3d));

			CommandLogging.WriteLine(from, "{0} {1} invoking freeze from {2} to {3} in {4}", from.AccessLevel, CommandLogging.Format(from), start, end, targetMap);

			IPooledEnumerable eable = targetMap.GetItemsInBounds(new Rectangle2D(start.X, start.Y, end.X - start.X + 1, end.Y - start.Y + 1));

			foreach (Item item in eable)
			{
				if (item is Static or BaseFloor or BaseWall)
				{
					Map itemMap = item.Map;

					if (itemMap == null || itemMap == Map.Internal)
						continue;

					Hashtable table = (Hashtable)mapTable[itemMap];

					if (table == null)
						mapTable[itemMap] = table = new Hashtable();

					Point2D p = new(item.X >> 3, item.Y >> 3);

					DeltaState state = (DeltaState)table[p];

					if (state == null)
						table[p] = state = new DeltaState(p);

					state.MList.Add(item);
				}
			}

			eable.Free();
		}

		if (mapTable.Count == 0)
		{
			from.SendGump(new NoticeGump(1060637, 30720, "No freezable items were found.  Only the following item types are frozen:<br> - Static<br> - BaseFloor<br> - BaseWall", 0xFFC000, 320, 240, null, null));
			return;
		}

		bool badDataFile = false;

		int totalFrozen = 0;

		foreach (DictionaryEntry de in mapTable)
		{
			Map map = (Map)de.Key;
			Hashtable table = (Hashtable)de.Value;

			TileMatrix matrix = map.Tiles;

			using FileStream idxStream = OpenWrite(matrix.IndexStream);
			using FileStream mulStream = OpenWrite(matrix.DataStream);
			if (idxStream == null || mulStream == null)
			{
				badDataFile = true;
				continue;
			}

			BinaryReader idxReader = new(idxStream);

			BinaryWriter idxWriter = new(idxStream);
			BinaryWriter mulWriter = new(mulStream);

			foreach (DeltaState state in table.Values)
			{
				StaticTile[] oldTiles = ReadStaticBlock(idxReader, mulStream, state.Mx, state.My, matrix.BlockWidth, matrix.BlockHeight, out int oldTileCount);

				if (oldTileCount < 0)
					continue;

				int newTileCount = 0;
				StaticTile[] newTiles = new StaticTile[state.MList.Count];

				for (int i = 0; i < state.MList.Count; ++i)
				{
					Item item = state.MList[i];

					int xOffset = item.X - state.Mx * 8;
					int yOffset = item.Y - state.My * 8;

					if (xOffset is < 0 or >= 8 || yOffset is < 0 or >= 8)
						continue;

					StaticTile newTile = new((ushort)item.ItemId, (byte)xOffset, (byte)yOffset, (sbyte)item.Z, (short)item.Hue);

					newTiles[newTileCount++] = newTile;

					item.Delete();

					++totalFrozen;
				}

				int mulPos = -1;
				int length = -1;
				int extra = 0;

				if (oldTileCount + newTileCount > 0)
				{
					mulWriter.Seek(0, SeekOrigin.End);

					mulPos = (int)mulWriter.BaseStream.Position;
					length = (oldTileCount + newTileCount) * 7;
					extra = 1;

					for (int i = 0; i < oldTileCount; ++i)
					{
						StaticTile toWrite = oldTiles[i];

						mulWriter.Write((ushort)toWrite.ID);
						mulWriter.Write((byte)toWrite.X);
						mulWriter.Write((byte)toWrite.Y);
						mulWriter.Write((sbyte)toWrite.Z);
						mulWriter.Write((short)toWrite.Hue);
					}

					for (int i = 0; i < newTileCount; ++i)
					{
						StaticTile toWrite = newTiles[i];

						mulWriter.Write((ushort)toWrite.ID);
						mulWriter.Write((byte)toWrite.X);
						mulWriter.Write((byte)toWrite.Y);
						mulWriter.Write((sbyte)toWrite.Z);
						mulWriter.Write((short)toWrite.Hue);
					}

					mulWriter.Flush();
				}

				int idxPos = (state.Mx * matrix.BlockHeight + state.My) * 12;

				idxWriter.Seek(idxPos, SeekOrigin.Begin);
				idxWriter.Write(mulPos);
				idxWriter.Write(length);
				idxWriter.Write(extra);

				idxWriter.Flush();

				matrix.SetStaticBlock(state.Mx, state.My, null);
			}
		}

		if (totalFrozen == 0 && badDataFile)
			from.SendGump(new NoticeGump(1060637, 30720, "Output data files could not be opened and the freeze operation has been aborted.<br><br>This probably means your server and client are using the same data files.  Instructions on how to resolve this can be found in the first warning window.", 0xFFC000, 320, 240, null, null));
		else
			from.SendGump(new NoticeGump(1060637, 30720,
				$"Freeze operation completed successfully.<br><br>{totalFrozen} item{(totalFrozen != 1 ? "s were" : " was")} frozen.<br><br>You must restart your client and update it's data files to see the changes.", 0xFFC000, 320, 240, null, null));
	}

	private const string BaseUnfreezeWarning = "{0}  " +
	                                           "Those items <u>will be removed from the static files</u> and exchanged with unmovable dynamic items.  " +
	                                           "Other players <u>will not see the changes</u> unless you distribute your data files to them.<br><br>" +
	                                           "This operation may not complete unless the server and client are using different data files.  " +
	                                           "If you receive a message stating 'output data files could not be opened,' then you are probably sharing data files.  " +
	                                           "Create a new directory for the world data files (statics*.mul and staidx*.mul) and add that to Scritps/Misc/DataPath.cs.<br><br>" +
	                                           "The change will be in effect immediately on the server, however, you must restart your client and update it's data files for the changes to become visible.  " +
	                                           "It is strongly recommended that you make backup of the data files mentioned above.  " +
	                                           "Do you wish to proceed?";

	[Usage("Unfreeze")]
	[Description("Makes a targeted area of static items dynamic.")]
	public static void Unfreeze_OnCommand(CommandEventArgs e)
	{
		BoundingBoxPicker.Begin(e.Mobile, UnfreezeBox_Callback, null);
	}

	[Usage("UnfreezeMap")]
	[Description("Makes every static item in your map dynamic.")]
	public static void UnfreezeMap_OnCommand(CommandEventArgs e)
	{
		Map map = e.Mobile.Map;

		if (map != null && map != Map.Internal)
			SendWarning(e.Mobile, "You are about to unfreeze <u>all items in {0}</u>.", BaseUnfreezeWarning, map, NullP3D, NullP3D, UnfreezeWarning_Callback);
	}

	[Usage("UnfreezeWorld")]
	[Description("Makes every static item on all maps dynamic.")]
	public static void UnfreezeWorld_OnCommand(CommandEventArgs e)
	{
		SendWarning(e.Mobile, "You are about to unfreeze <u>every item on every map</u>.", BaseUnfreezeWarning, null, NullP3D, NullP3D, UnfreezeWarning_Callback);
	}

	private static void UnfreezeBox_Callback(Mobile from, Map map, Point3D start, Point3D end, object state)
	{
		SendWarning(from, "You are about to unfreeze a section of items.", BaseUnfreezeWarning, map, start, end, UnfreezeWarning_Callback);
	}

	private static void UnfreezeWarning_Callback(Mobile from, bool okay, object state)
	{
		if (!okay)
			return;

		StateInfo si = (StateInfo)state;

		Unfreeze(from, si.MMap, si.MStart, si.MEnd);
	}

	private static void DoUnfreeze(Map map, Point2D start, Point2D end, ref bool badDataFile, ref int totalUnfrozen)
	{
		start = map.Bound(start);
		end = map.Bound(end);

		int xStartBlock = start.X >> 3;
		int yStartBlock = start.Y >> 3;
		int xEndBlock = end.X >> 3;
		int yEndBlock = end.Y >> 3;

		int xTileStart = start.X, yTileStart = start.Y;
		int xTileWidth = end.X - start.X + 1, yTileHeight = end.Y - start.Y + 1;

		TileMatrix matrix = map.Tiles;

		using FileStream idxStream = OpenWrite(matrix.IndexStream);
		using FileStream mulStream = OpenWrite(matrix.DataStream);
		if (idxStream == null || mulStream == null)
		{
			badDataFile = true;
			return;
		}

		BinaryReader idxReader = new(idxStream);

		BinaryWriter idxWriter = new(idxStream);
		BinaryWriter mulWriter = new(mulStream);

		for (int x = xStartBlock; x <= xEndBlock; ++x)
		{
			for (int y = yStartBlock; y <= yEndBlock; ++y)
			{
				StaticTile[] oldTiles = ReadStaticBlock(idxReader, mulStream, x, y, matrix.BlockWidth, matrix.BlockHeight, out int oldTileCount);

				if (oldTileCount < 0)
					continue;

				int newTileCount = 0;
				StaticTile[] newTiles = new StaticTile[oldTileCount];

				int baseX = (x << 3) - xTileStart, baseY = (y << 3) - yTileStart;

				for (int i = 0; i < oldTileCount; ++i)
				{
					StaticTile oldTile = oldTiles[i];

					int px = baseX + oldTile.X;
					int py = baseY + oldTile.Y;

					if (px < 0 || px >= xTileWidth || py < 0 || py >= yTileHeight)
					{
						newTiles[newTileCount++] = oldTile;
					}
					else
					{
						++totalUnfrozen;

						Item item = new Static(oldTile.ID)
						{
							Hue = oldTile.Hue
						};

						item.MoveToWorld(new Point3D(px + xTileStart, py + yTileStart, oldTile.Z), map);
					}
				}

				int mulPos = -1;
				int length = -1;
				int extra = 0;

				if (newTileCount > 0)
				{
					mulWriter.Seek(0, SeekOrigin.End);

					mulPos = (int)mulWriter.BaseStream.Position;
					length = newTileCount * 7;
					extra = 1;

					for (int i = 0; i < newTileCount; ++i)
					{
						StaticTile toWrite = newTiles[i];

						mulWriter.Write((ushort)toWrite.ID);
						mulWriter.Write((byte)toWrite.X);
						mulWriter.Write((byte)toWrite.Y);
						mulWriter.Write((sbyte)toWrite.Z);
						mulWriter.Write((short)toWrite.Hue);
					}

					mulWriter.Flush();
				}

				int idxPos = (x * matrix.BlockHeight + y) * 12;

				idxWriter.Seek(idxPos, SeekOrigin.Begin);
				idxWriter.Write(mulPos);
				idxWriter.Write(length);
				idxWriter.Write(extra);

				idxWriter.Flush();

				matrix.SetStaticBlock(x, y, null);
			}
		}
	}

	public static void DoUnfreeze(Map map, ref bool badDataFile, ref int totalUnfrozen)
	{
		DoUnfreeze(map, Point2D.Zero, new Point2D(map.Width - 1, map.Height - 1), ref badDataFile, ref totalUnfrozen);
	}

	public static void Unfreeze(Mobile from, Map map, Point3D start, Point3D end)
	{
		int totalUnfrozen = 0;
		bool badDataFile = false;

		if (map == null)
		{
			CommandLogging.WriteLine(from, "{0} {1} invoking unfreeze for every item in every map", from.AccessLevel, CommandLogging.Format(from));

			DoUnfreeze(Map.Felucca, ref badDataFile, ref totalUnfrozen);
			DoUnfreeze(Map.Trammel, ref badDataFile, ref totalUnfrozen);
			DoUnfreeze(Map.Ilshenar, ref badDataFile, ref totalUnfrozen);
			DoUnfreeze(Map.Malas, ref badDataFile, ref totalUnfrozen);
			DoUnfreeze(Map.Tokuno, ref badDataFile, ref totalUnfrozen);
		}
		else if (start == NullP3D && end == NullP3D)
		{
			CommandLogging.WriteLine(from, "{0} {1} invoking unfreeze for every item in {2}", from.AccessLevel, CommandLogging.Format(from), map);

			DoUnfreeze(map, ref badDataFile, ref totalUnfrozen);
		}
		else
		{
			CommandLogging.WriteLine(from, "{0} {1} invoking unfreeze from {2} to {3} in {4}", from.AccessLevel, CommandLogging.Format(from), new Point2D(start), new Point2D(end), map);

			DoUnfreeze(map, new Point2D(start), new Point2D(end), ref badDataFile, ref totalUnfrozen);
		}

		if (totalUnfrozen == 0 && badDataFile)
			from.SendGump(new NoticeGump(1060637, 30720, "Output data files could not be opened and the unfreeze operation has been aborted.<br><br>This probably means your server and client are using the same data files.  Instructions on how to resolve this can be found in the first warning window.", 0xFFC000, 320, 240, null, null));
		else
			from.SendGump(new NoticeGump(1060637, 30720,
				$"Unfreeze operation completed successfully.<br><br>{totalUnfrozen} item{(totalUnfrozen != 1 ? "s were" : " was")} unfrozen.<br><br>You must restart your client and update it's data files to see the changes.", 0xFFC000, 320, 240, null, null));
	}

	private static FileStream OpenWrite(FileStream orig)
	{
		if (orig == null)
			return null;

		try { return new FileStream(orig.Name, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite); }
		catch { return null; }
	}

	private static byte[] _mBuffer;

	private static StaticTile[] _mTileBuffer = new StaticTile[128];

	private static StaticTile[] ReadStaticBlock(BinaryReader idxReader, FileStream mulStream, int x, int y, int width, int height, out int count)
	{
		try
		{
			if (x < 0 || x >= width || y < 0 || y >= height)
			{
				count = -1;
				return _mTileBuffer;
			}

			idxReader.BaseStream.Seek(((x * height) + y) * 12, SeekOrigin.Begin);

			int lookup = idxReader.ReadInt32();
			int length = idxReader.ReadInt32();

			if (lookup < 0 || length <= 0)
			{
				count = 0;
			}
			else
			{
				count = length / 7;

				mulStream.Seek(lookup, SeekOrigin.Begin);

				if (_mTileBuffer.Length < count)
					_mTileBuffer = new StaticTile[count];

				StaticTile[] staTiles = _mTileBuffer;

				if (_mBuffer == null || length > _mBuffer.Length)
					_mBuffer = new byte[length];

				mulStream.Read(_mBuffer, 0, length);

				int index = 0;

				for (int i = 0; i < count; ++i)
				{
					staTiles[i].Set((ushort)(_mBuffer[index++] | (_mBuffer[index++] << 8)),
						_mBuffer[index++], _mBuffer[index++], (sbyte)_mBuffer[index++],
						(short)(_mBuffer[index++] | (_mBuffer[index++] << 8)));
				}
			}
		}
		catch
		{
			count = -1;
		}

		return _mTileBuffer;
	}

	private class DeltaState
	{
		public readonly int Mx;
		public readonly int My;
		public readonly List<Item> MList;

		public DeltaState(Point2D p)
		{
			Mx = p.X;
			My = p.Y;
			MList = new List<Item>();
		}
	}

	private class StateInfo
	{
		public readonly Map MMap;
		public readonly Point3D MStart;
		public readonly Point3D MEnd;

		public StateInfo(Map map, Point3D start, Point3D end)
		{
			MMap = map;
			MStart = start;
			MEnd = end;
		}
	}
}
