using System;
using System.Collections.Generic;
using System.Linq;
using Server.Engines.Champions;
using Server.Engines.PartySystem;
using Server.Factions;
using Server.Guilds;
using Server.Items;
using Server.Regions;

namespace Server.Spells.Necromancy;

public class ExorcismSpell : NecromancerSpell
{
	private static readonly SpellInfo m_Info = new(
		"Exorcism", "Ort Corp Grav",
		203,
		9031,
		Reagent.NoxCrystal,
		Reagent.GraveDust);
	private static readonly int Range = Core.ML ? 48 : 18;
	private static readonly Point3D[] m_BritanniaLocs = {
		new(1470, 843, 0),
		new(1857, 865, -1),
		new(4220, 563, 36),
		new(1732, 3528, 0),
		new(1300, 644, 8),
		new(3355, 302, 9),
		new(1606, 2490, 5),
		new(2500, 3931, 3),
		new(4264, 3707, 0)
	};
	private static readonly Point3D[] m_IllshLocs = {
		new(1222, 474, -17),
		new(718, 1360, -60),
		new(297, 1014, -19),
		new(986, 1006, -36),
		new(1180, 1288, -30),
		new(1538, 1341, -3),
		new(528, 223, -38)
	};
	private static readonly Point3D[] m_MalasLocs = {
		new(976, 517, -30)
	};
	private static readonly Point3D[] m_TokunoLocs = {
		new(710, 1162, 25),
		new(1034, 515, 18),
		new(295, 712, 55)
	};
	public ExorcismSpell(Mobile caster, Item scroll)
		: base(caster, scroll, m_Info)
	{
	}

	public override TimeSpan CastDelayBase => TimeSpan.FromSeconds(2.0);
	public override double RequiredSkill => 80.0;
	public override int RequiredMana => 40;
	public override bool DelayedDamage => false;

	public override bool CheckCast()
	{
		if (!Core.SE)
			return base.CheckCast();

		if (!(Caster.Skills.SpiritSpeak.Value < 100.0))
			return base.CheckCast();

		Caster.SendLocalizedMessage(1072112); // You must have GM Spirit Speak to use this spell
		return false;

	}

	public override int ComputeKarmaAward()
	{
		return 0;	//no karma lost from this spell!
	}

	public override void OnCast()
	{
		if (Caster.Region.GetRegion(typeof(ChampionSpawnRegion)) is not ChampionSpawnRegion r || !Caster.InRange(r.ChampionSpawn, Range))
		{
			Caster.SendLocalizedMessage(1072111); // You are not in a valid exorcism region.
		}
		else if (CheckSequence())
		{
			Map map = Caster.Map;

			if (map != null)
			{
				IPooledEnumerable eable = r.ChampionSpawn.GetMobilesInRange(Range);

				List<Mobile> targets = eable.Cast<Mobile>().Where(IsValidTarget).ToList();

				eable.Free();

				for (int i = 0; i < targets.Count; ++i)
				{
					Mobile m = targets[i];

					//Suprisingly, no sparkle type effects
					Point3D p = GetNearestShrine(m, ref map);

					m.MoveToWorld(p, map);
				}
			}
		}

		FinishSequence();
	}

	public static Point3D GetNearestShrine(Mobile m, ref Map map)
	{
		Point3D[] locList;

		if (map == Map.Felucca || map == Map.Trammel)
			locList = m_BritanniaLocs;
		else if (map == Map.Ilshenar)
			locList = m_IllshLocs;
		else if (map == Map.Tokuno)
			locList = m_TokunoLocs;
		else if (map == Map.Malas)
			locList = m_MalasLocs;
		else
		{
			// No map, lets use trammel
			locList = m_BritanniaLocs;
			map = Map.Trammel;
		}

		Point3D closest = Point3D.Zero;
		double minDist = double.MaxValue;

		for (int i = 0; i < locList.Length; i++)
		{
			Point3D p = locList[i];

			double dist = m.GetDistanceToSqrt(p);
			if (minDist > dist)
			{
				closest = p;
				minDist = dist;
			}
		}

		return closest;
	}

	private bool IsValidTarget(Mobile m)
	{
		if (!m.Player || m.Alive)
			return false;

		Corpse c = m.Corpse as Corpse;
		Map map = m.Map;

		if (c is { Deleted: false } && map != null && c.Map == map)
		{
			if (SpellHelper.IsAnyT2A(map, c.Location) && SpellHelper.IsAnyT2A(map, m.Location))
				return false;	//Same Map, both in T2A, ie, same 'sub server'.

			if (m.Region.IsPartOf<DungeonRegion>() == Region.Find(c.Location, map).IsPartOf<DungeonRegion>())
				return false; //Same Map, both in Dungeon region OR They're both NOT in a dungeon region.
		}

		Party p = Party.Get(m);

		if (p != null && p.Contains(Caster))
			return false;

		if (m.Guild != null && Caster.Guild != null)
		{
			Guild mGuild = m.Guild as Guild;
			Guild cGuild = Caster.Guild as Guild;

			if (mGuild != null && mGuild.IsAlly(cGuild))
				return false;

			if (mGuild == cGuild)
				return false;
		}

		Faction f = Faction.Find(m);

		return Faction.Facet != m.Map || f == null || f != Faction.Find(Caster);
	}
}
