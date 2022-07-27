using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Factions;

public class Generator
{
	public static void Generate(Town town)
	{
		Map facet = Faction.Facet;

		TownDefinition def = town.Definition;

		if (!CheckExistance(def.Monolith, facet, typeof(TownMonolith)))
		{
			TownMonolith mono = new(town);
			mono.MoveToWorld(def.Monolith, facet);
			mono.Sigil = new Sigil(town);
		}

		if (!CheckExistance(def.TownStone, facet, typeof(TownStone)))
			new TownStone(town).MoveToWorld(def.TownStone, facet);
	}

	public static void Generate(Faction faction)
	{
		Map facet = Faction.Facet;

		List<Town> towns = Town.Towns;

		StrongholdDefinition stronghold = faction.Definition.Stronghold;

		if (!CheckExistance(stronghold.JoinStone, facet, typeof(JoinStone)))
			new JoinStone(faction).MoveToWorld(stronghold.JoinStone, facet);

		if (!CheckExistance(stronghold.FactionStone, facet, typeof(FactionStone)))
			new FactionStone(faction).MoveToWorld(stronghold.FactionStone, facet);

		for (int i = 0; i < stronghold.Monoliths.Length; ++i)
		{
			Point3D monolith = stronghold.Monoliths[i];

			if (!CheckExistance(monolith, facet, typeof(StrongholdMonolith)))
				new StrongholdMonolith(towns[i], faction).MoveToWorld(monolith, facet);
		}
	}

	private static bool CheckExistance(Point3D loc, Map facet, Type type)
	{
		return facet.GetItemsInRange(loc, 0).Any(type.IsInstanceOfType);
	}
}
