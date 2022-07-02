using System;
using System.IO;
using Server.Items;

namespace Server;

public enum SaveStrategyTypes
{
	StandardSaveStrategy,
	DualSaveStrategy,
	DynamicSaveStrategy,
	ParallelSaveStrategy
}

public enum OldClientResponse
{
	Ignore,
	Warn,
	Annoy,
	LenientKick,
	Kick
}

public static class Utilities
{
	public static int WriteVersion(this GenericWriter writer, int version)
	{
		writer.Write(version);
		return version;
	}

	public static int ReaderVersion(this GenericReader reader, int version)
	{
		return reader.ReadInt();
	}

	public static void CheckFileStructure(string path)
	{
		if (!Directory.Exists(path))
		{
			if (path != null) Directory.CreateDirectory(path);
		}
	}

	public static bool IsPlayer(this Mobile from)
	{
		return from.AccessLevel <= AccessLevel.Player;
	}

	public static bool IsVip(this Mobile from)
	{
		return from.AccessLevel <= AccessLevel.VIP;
	}

	public static bool IsStaff(this Mobile from)
	{
		return from.AccessLevel >= AccessLevel.Counselor;
	}

	public static bool IsCounselor(this Mobile from)
	{
		return from.AccessLevel == AccessLevel.Counselor;
	}

	public static bool IsGameMaster(this Mobile from)
	{
		return from.AccessLevel == AccessLevel.GameMaster;
	}

	public static bool IsSeer(this Mobile from)
	{
		return from.AccessLevel == AccessLevel.Seer;
	}

	public static bool IsDeveloper(this Mobile from)
	{
		return from.AccessLevel == AccessLevel.Developer;
	}

	public static bool IsDecorator(this Mobile from)
	{
		return from.AccessLevel == AccessLevel.Decorator;
	}

	public static bool IsAdministrator(this Mobile from)
	{
		return from.AccessLevel == AccessLevel.Administrator;
	}

	public static bool IsOwner(this Mobile from)
	{
		return from.AccessLevel >= AccessLevel.Owner;
	}

	public static bool IsDigit(this string text)
	{
		return IsDigit(text, out _);
	}

	public static bool IsDigit(this string text, out int value)
	{
		return int.TryParse(text, out value);
	}

	public static SaveStrategy GetSaveStrategy(this SaveStrategyTypes saveStrategyTypes)
	{
		return saveStrategyTypes switch
		{
			SaveStrategyTypes.StandardSaveStrategy => new StandardSaveStrategy(),
			SaveStrategyTypes.DualSaveStrategy => new DualSaveStrategy(),
			SaveStrategyTypes.DynamicSaveStrategy => new DynamicSaveStrategy(),
			SaveStrategyTypes.ParallelSaveStrategy => new ParallelSaveStrategy(Core.ProcessorCount),
			_ => new StandardSaveStrategy()
		};
	}

	public static SaveStrategyTypes GetSaveType(this SaveStrategy saveStrategy)
	{
		switch (saveStrategy)
		{
			case DualSaveStrategy:
			case StandardSaveStrategy:
				return SaveStrategyTypes.StandardSaveStrategy;
			case DynamicSaveStrategy:
				return SaveStrategyTypes.DynamicSaveStrategy;
			case ParallelSaveStrategy:
				return SaveStrategyTypes.ParallelSaveStrategy;
			default:
				return SaveStrategyTypes.StandardSaveStrategy;
		}
	}

	public static void PlaceItemIn(this Container container, Item item, Point3D location)
	{
		container.AddItem(item);
		item.Location = location;
	}

	public static void PlaceItemIn(this Container container, Item item, int x = 0, int y = 0, int z = 0)
	{
		PlaceItemIn(container, item, new Point3D(x, y, z));
	}

	public static Item BlessItem(this Item item)
	{
		item.LootType = LootType.Blessed;

		return item;
	}

	public static Item MakeNewbie(this Item item)
	{
		if (!Core.AOS)
		{
			item.LootType = LootType.Newbied;
		}

		return item;
	}

	public static void DumpToConsole(params object[] elements)
	{
		Console.WriteLine();
		/*
		foreach (var element in elements)
		{
			Console.WriteLine(ObjectDumper.Dump(element));
			Console.WriteLine();
		}
		*/
	}
}
