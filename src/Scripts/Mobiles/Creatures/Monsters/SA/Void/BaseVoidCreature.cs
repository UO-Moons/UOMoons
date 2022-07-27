using Server.Items;
using System;
using System.Collections.Generic;

namespace Server.Mobiles;

public enum VoidEvolution
{
	None = 0,
	Killing = 1,
	Grouping = 2,
	Survival = 3
}

public class BaseVoidCreature : BaseCreature
{
	private static int MutateCheck => Utility.RandomMinMax(30, 120);

	public static bool RemoveFromSpawners => true;

	protected virtual int GroupAmount => 2;
	protected virtual VoidEvolution Evolution => VoidEvolution.None;
	public virtual int Stage => 0;

	[CommandProperty(AccessLevel.GameMaster)]
	private bool BuddyMutate { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	private DateTime NextMutate { get; set; }

	public override bool PlayerRangeSensitive => Evolution != VoidEvolution.Killing && Stage < 3;
	public override bool AlwaysMurderer => true;

	public BaseVoidCreature(AIType aiType, int perception, int range, double passive, double active)
		: base(aiType, FightMode.Good, perception, range, passive, active)
	{
		NextMutate = DateTime.UtcNow + TimeSpan.FromMinutes(MutateCheck);
		BuddyMutate = true;
	}

	public override void OnThink()
	{
		base.OnThink();

		if (Stage >= 3 || NextMutate > DateTime.UtcNow)
			return;

		if (!MutateGrouped() && Alive && !Deleted)
		{
			Mutate(VoidEvolution.Survival);
		}
	}

	public bool MutateGrouped()
	{
		if (!BuddyMutate)
			return false;

		List<BaseVoidCreature> buddies = new();
		IPooledEnumerable eable = GetMobilesInRange(12);

		foreach (Mobile m in eable)
		{
			if (m == this || !IsEvolutionType(m) || m.Deleted || !m.Alive ||
			    buddies.Contains((BaseVoidCreature)m)) continue;
			if (((BaseVoidCreature)m).BuddyMutate)
				buddies.Add((BaseVoidCreature)m);
		}

		eable.Free();

		if (buddies.Count >= GroupAmount)
		{
			Mutate(VoidEvolution.Grouping);

			foreach (BaseVoidCreature k in buddies)
				k.Mutate(VoidEvolution.Grouping);

			ColUtility.Free(buddies);

			return true;
		}

		ColUtility.Free(buddies);
		return false;
	}

	public bool IsEvolutionType(Mobile from)
	{
		if (Stage == 0 && from.GetType() != GetType())
			return false;

		return from is BaseVoidCreature;
	}

	public readonly Type[][] EvolutionCycle =
	{
		new[] { typeof(Betballem),     typeof(Ballem),     typeof(UsagralemBallem) },
		new[] { typeof(Anlorzen),      typeof(Anlorlem),   typeof(Anlorvaglem) },
		new[] { typeof(Anzuanord),     typeof(Relanord),   typeof(Vasanord) }
	};

	private BaseCreature m_MutateTo;

	public void Mutate(VoidEvolution evolution)
	{
		if (!Alive || Deleted || Stage == 3)
			return;

		VoidEvolution evo = evolution;

		if (Stage > 0)
			evo = Evolution;

		if (0.05 > Utility.RandomDouble())
		{
			SpawnOrtanords();
		}

		Type type = EvolutionCycle[(int)evo - 1][Stage];

		BaseCreature bc = (BaseCreature)Activator.CreateInstance(type);

		m_MutateTo = bc;

		if (bc == null)
			return;

		bc.MoveToWorld(Location, Map);

		bc.Home = Home;
		bc.RangeHome = RangeHome;

		if (0.05 > Utility.RandomDouble())
			SpawnOrtanords();

		if (bc is BaseVoidCreature)
			((BaseVoidCreature)bc).BuddyMutate = BuddyMutate;

		Delete();
	}

	public void SpawnOrtanords()
	{
		BaseCreature ortanords = new Ortanord();

		Point3D spawnLoc = Location;

		for (var i = 0; i < 25; i++)
		{
			var x = Utility.RandomMinMax(X - 5, X + 5);
			var y = Utility.RandomMinMax(Y - 5, Y + 5);
			var z = Map.GetAverageZ(x, y);

			Point3D p = new(x, y, z);

			if (!Map.CanSpawnMobile(p))
				continue;
			spawnLoc = p;
			break;
		}

		ortanords.MoveToWorld(spawnLoc, Map);
		ortanords.BoltEffect(0);
	}

	public override void OnDeath(Container c)
	{
		base.OnDeath(c);

		double baseChance = 0.02;
		double chance = 0.0;

		if (Stage > 0)
			chance = baseChance * (Stage + 3);

		if (Stage > 0 && Utility.RandomDouble() < chance)
			c.DropItem(new VoidEssence());

		if (Stage == 3 && Utility.RandomDouble() < 0.12)
			c.DropItem(new VoidCore());
	}

	public override void Delete()
	{
		if (m_MutateTo != null)
		{
			ISpawner s = Spawner;

			if (s is XmlSpawner xml)
			{
				if (xml.SpawnObjects == null)
					return;

				foreach (XmlSpawner.SpawnObject so in xml.SpawnObjects)
				{
					for (var i = 0; i < so.SpawnedObjects.Count; ++i)
					{
						if (so.SpawnedObjects[i] != this)
							continue;
						so.SpawnedObjects[i] = m_MutateTo;

						Spawner = null;
						base.Delete();
						return;
					}
				}
			}
		}

		base.Delete();
	}

	public BaseVoidCreature(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);

		writer.Write(NextMutate);
		writer.Write(BuddyMutate);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadInt();

		NextMutate = reader.ReadDateTime();
		BuddyMutate = reader.ReadBool();
	}
}
