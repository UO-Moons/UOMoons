using Server.Engines.NewMagincia;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Engines.Plants;

public class MaginciaPlantSystem : BaseItem
{
	public const bool Enabled = true;
	private const int PlantDelay = 4;
	private Dictionary<Mobile, DateTime> PlantDelayTable { get; } = new();

	private static MaginciaPlantSystem FelInstance { get; set; }
	private static MaginciaPlantSystem TramInstance { get; set; }

	public static void Initialize()
	{
		if (Enabled)
		{
			if (FelInstance == null)
			{
				FelInstance = new MaginciaPlantSystem();
				FelInstance.MoveToWorld(new Point3D(3715, 2049, 5), Map.Felucca);
			}

			if (TramInstance != null) return;
			TramInstance = new MaginciaPlantSystem();
			TramInstance.MoveToWorld(new Point3D(3715, 2049, 5), Map.Trammel);
		}
	}

	private MaginciaPlantSystem()
		: base(3240)
	{
		Movable = false;
	}

	private bool CheckPlantDelay(Mobile from)
	{
		if (!PlantDelayTable.ContainsKey(from)) return true;
		if (PlantDelayTable[from] <= DateTime.UtcNow) return true;
		TimeSpan left = PlantDelayTable[from] - DateTime.UtcNow;

		// Time remaining to plant on the Isle of Magincia again: ~1_val~ days ~2_val~ hours ~3_val~ minutes.
		from.SendLocalizedMessage(1150459,
			$"{left.Days.ToString()}\t{left.Hours.ToString()}\t{left.Minutes.ToString()}");
		return false;

	}

	private void OnPlantDelete(Mobile from)
	{
		if (PlantDelayTable.ContainsKey(from))
			PlantDelayTable.Remove(from);
	}

	private void OnPlantPlanted(Mobile from)
	{
		if (from.AccessLevel == AccessLevel.Player)
			PlantDelayTable[from] = DateTime.UtcNow + TimeSpan.FromDays(PlantDelay);
		else
			from.SendMessage("As staff, you bypass the {0} day plant delay.", PlantDelay);
	}

	public override void Delete()
	{
	}

	public static bool CanAddPlant(Mobile from, Point3D p)
	{
		if (!IsValidLocation(p))
		{
			from.SendLocalizedMessage(1150457); // The ground here is not good for gardening.
			return false;
		}

		Map map = from.Map;

		IPooledEnumerable eable = map.GetItemsInRange(p, 17);
		int plantCount = 0;

		foreach (Item item in eable)
		{
			if (item is MaginciaPlantItem)
			{
				if (item.Location != p)
					plantCount++;
				else
				{
					from.SendLocalizedMessage(1150367); // This plot already has a plant!
					eable.Free();
					return false;
				}
			}
			else if (!item.Movable && item.Location == p)
			{
				from.SendLocalizedMessage(1150457); // The ground here is not good for gardening.
				eable.Free();
				return false;
			}
		}

		eable.Free();

		if (plantCount > 34)
		{
			from.SendLocalizedMessage(1150491); // There are too many objects in this area to plant (limit 34 per 17x17 area).
			return false;
		}

		StaticTile[] staticTiles = map.Tiles.GetStaticTiles(p.X, p.Y, true);

		if (staticTiles.Length > 0)
		{
			from.SendLocalizedMessage(1150457); // The ground here is not good for gardening.
			return false;
		}

		return true;
	}

	public static bool CheckDelay(Mobile from)
	{
		MaginciaPlantSystem system = null;
		Map map = from.Map;

		if (map == Map.Trammel)
			system = TramInstance;
		else if (map == Map.Felucca)
			system = FelInstance;

		if (system != null) return system.CheckPlantDelay(from);
		from.SendLocalizedMessage(1150457); // The ground here is not good for gardening.
		return false;

	}

	private static bool IsValidLocation(Point3D p)
	{
		/*foreach (Rectangle2D rec in m_MagGrowBounds)
		{
		    if (rec.Contains(p))
		        return true;
		}*/
		if (m_NoGrowZones.Any(rec => rec.Contains(p)))
		{
			return false;
		}

		return MaginciaLottoSystem.MagHousingZones.Select(rec => new Rectangle2D(rec.X - 2, rec.Y - 2, rec.Width + 4, rec.Height + 7)).All(newRec => !newRec.Contains(p));
	}

	public static void OnPlantDelete(Mobile owner, Map map)
	{
		if (owner == null || map == null)
			return;

		if (map == Map.Trammel)
			TramInstance.OnPlantDelete(owner);
		else if (map == Map.Felucca)
			FelInstance.OnPlantDelete(owner);
	}

	public static void OnPlantPlanted(Mobile from, Map map)
	{
		if (map == Map.Felucca)
			FelInstance.OnPlantPlanted(from);
		else if (map == Map.Trammel)
			TramInstance.OnPlantPlanted(from);
	}

	public static Rectangle2D[] MagGrowBounds { get; } =
	{
		new(3663, 2103, 19, 19),
		new(3731, 2199, 7, 7),
	};

	private static readonly Rectangle2D[] m_NoGrowZones = {
		new(3683, 2144, 21, 40),
		new(3682, 2189, 39, 44),
		new(3654, 2233, 23, 30),
		new(3727, 2217, 15, 45),
		new(3558, 2134, 8, 8),
		new(3679, 2018, 70, 28)
	};

	private void DefragPlantDelayTable()
	{
		List<Mobile> toRemove = (from kvp in PlantDelayTable where kvp.Value < DateTime.UtcNow select kvp.Key).ToList();

		foreach (Mobile m in toRemove)
			PlantDelayTable.Remove(m);
	}

	public MaginciaPlantSystem(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0); // version

		DefragPlantDelayTable();

		writer.Write(PlantDelayTable.Count);
		foreach (KeyValuePair<Mobile, DateTime> kvp in PlantDelayTable)
		{
			writer.Write(kvp.Key);
			writer.Write(kvp.Value);
		}
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadInt();

		int c = reader.ReadInt();

		for (int i = 0; i < c; i++)
		{
			Mobile m = reader.ReadMobile();
			DateTime dt = reader.ReadDateTime();

			if (m != null && dt > DateTime.UtcNow)
				PlantDelayTable[m] = dt;
		}

		if (Map == Map.Felucca)
			FelInstance = this;
		else if (Map == Map.Trammel)
			TramInstance = this;
		else
			Delete();
	}
}
