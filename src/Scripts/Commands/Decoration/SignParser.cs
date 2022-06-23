using Server.Items;
using System.Collections.Generic;
using System.IO;

namespace Server.Commands;

public class SignParser
{
	private class SignEntry
	{
		public readonly string MText;
		public readonly Point3D MLocation;
		public readonly int MItemId;
		public readonly int MMap;

		public SignEntry(string text, Point3D pt, int itemId, int mapLoc)
		{
			MText = text;
			MLocation = pt;
			MItemId = itemId;
			MMap = mapLoc;
		}
	}

	public static void Initialize()
	{
		CommandSystem.Register("SignGen", AccessLevel.Administrator, SignGen_OnCommand);
	}

	[Usage("SignGen")]
	[Description("Generates world/shop signs on all facets.")]
	public static void SignGen_OnCommand(CommandEventArgs c)
	{
		Parse(c.Mobile);
	}

	public static void Parse(Mobile from)
	{
		string cfg = Path.Combine(Core.BaseDirectory, "Data/signs.cfg");

		if (File.Exists(cfg))
		{
			List<SignEntry> list = new();
			from.SendMessage("Generating signs, please wait.");

			using (StreamReader ip = new(cfg))
			{
				while (ip.ReadLine() is { } line)
				{
					string[] split = line.Split(' ');

					SignEntry e = new(
						line[(split[0].Length + 1 + split[1].Length + 1 + split[2].Length + 1 + split[3].Length + 1 + split[4].Length + 1)..],
						new Point3D(Utility.ToInt32(split[2]), Utility.ToInt32(split[3]), Utility.ToInt32(split[4])),
						Utility.ToInt32(split[1]), Utility.ToInt32(split[0]));

					list.Add(e);
				}
			}

			Map[] brit = { Map.Felucca, Map.Trammel };
			Map[] fel = { Map.Felucca };
			Map[] tram = { Map.Trammel };
			Map[] ilsh = { Map.Ilshenar };
			Map[] malas = { Map.Malas };
			Map[] tokuno = { Map.Tokuno };

			for (int i = 0; i < list.Count; ++i)
			{
				SignEntry e = list[i];

				Map[] maps = e.MMap switch
				{
					0 => brit,
					1 => fel,
					2 => tram,
					3 => ilsh,
					4 => malas,
					5 => tokuno,
					_ => null
				};

				for (int j = 0; maps != null && j < maps.Length; ++j)
					Add_Static(e.MItemId, e.MLocation, maps[j], e.MText);
			}

			from.SendMessage("Sign generating complete.");
		}
		else
		{
			from.SendMessage("{0} not found!", cfg);
		}
	}

	private static readonly Queue<Item> MToDelete = new();

	public static void Add_Static(int itemId, Point3D location, Map map, string name)
	{
		IPooledEnumerable eable = map.GetItemsInRange(location, 0);

		foreach (Item item in eable)
		{
			if (item is Sign && item.Z == location.Z && item.ItemId == itemId)
				MToDelete.Enqueue(item);
		}

		eable.Free();

		while (MToDelete.Count > 0)
			MToDelete.Dequeue().Delete();

		Item sign;

		if (name.StartsWith("#"))
		{
			sign = new LocalizedSign(itemId, Utility.ToInt32(name.Substring(1)));
		}
		else
		{
			sign = new Sign(itemId)
			{
				Name = name
			};
		}

		if (map == Map.Malas)
		{
			sign.Hue = location.X switch
			{
				>= 965 when location.Y >= 502 && location.X <= 1012 && location.Y <= 537 => 0x47E,
				>= 1960 when location.Y >= 1278 && location.X < 2106 && location.Y < 1413 => 0x44E,
				_ => sign.Hue
			};
		}

		sign.MoveToWorld(location, map);
	}
}
