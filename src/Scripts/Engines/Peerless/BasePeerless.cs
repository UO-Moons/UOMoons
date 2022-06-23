using Server.Items;
using Server.Spells;

namespace Server.Mobiles;

public class BasePeerless : BaseCreature
{
	[CommandProperty(AccessLevel.GameMaster)]
	public PeerlessAltar Altar { get; set; }

	public override bool CanBeParagon => false;
	public virtual bool DropPrimer => Core.TOL;
	public virtual bool GiveMlSpecial => true;

	public override bool Unprovokable => true;
	public virtual double ChangeCombatant => 0.3;

	public BasePeerless(Serial serial)
		: base(serial)
	{
	}

	public override void OnThink()
	{
		base.OnThink();

		if (HasFireRing && Combatant != null && Alive && Hits > 0.8 * HitsMax && _mNextFireRing > Core.TickCount && Utility.RandomDouble() < FireRingChance)
			FireRing();

		if (CanSpawnHelpers && Combatant != null && Alive && CanSpawnWave())
			SpawnHelpers();
	}

	public override void OnDeath(Container c)
	{
		base.OnDeath(c);

		if (GivesMlMinorArtifact && 0.5 > Utility.RandomDouble())
		{
			MondainsLegacy.DropPeerlessMinor(c);
		}

		if (GiveMlSpecial)
		{
			if (Utility.RandomDouble() < 0.10)
				c.DropItem(new HumanFeyLeggings());

			if (Utility.RandomDouble() < 0.025)
				c.DropItem(new CrimsonCincture());

			if (0.05 > Utility.RandomDouble())
			{
				switch (Utility.Random(32))
				{
					case 0: c.DropItem(new AssassinChest()); break;
					case 1: c.DropItem(new AssassinArms()); break;
					case 2: c.DropItem(new AssassinLegs()); break;
					case 3: c.DropItem(new AssassinGloves()); break;
					case 4: c.DropItem(new DeathChest()); break;
					case 5: c.DropItem(new DeathArms()); break;
					case 6: c.DropItem(new DeathLegs()); break;
					case 7: c.DropItem(new DeathBoneHelm()); break;
					case 8: c.DropItem(new DeathGloves()); break;
					case 9: c.DropItem(new MyrmidonArms()); break;
					case 10: c.DropItem(new MyrmidonLegs()); break;
					case 11: c.DropItem(new MyrmidonGorget()); break;
					case 12: c.DropItem(new MyrmidonChest()); break;
					case 13: c.DropItem(new LeafweaveGloves()); break;
					case 14: c.DropItem(new LeafweaveLegs()); break;
					case 15: c.DropItem(new LeafweavePauldrons()); break;
					case 16: c.DropItem(new PaladinGloves()); break;
					case 17: c.DropItem(new PaladinGorget()); break;
					case 18: c.DropItem(new PaladinArms()); break;
					case 19: c.DropItem(new PaladinLegs()); break;
					case 20: c.DropItem(new PaladinHelm()); break;
					case 21: c.DropItem(new PaladinChest()); break;
					case 22: c.DropItem(new HunterArms()); break;
					case 23: c.DropItem(new HunterGloves()); break;
					case 24: c.DropItem(new HunterLegs()); break;
					case 25: c.DropItem(new HunterChest()); break;
					case 26: c.DropItem(new GreymistArms()); break;
					case 27: c.DropItem(new GreymistGloves()); break;
					case 28: c.DropItem(new GreymistLegs()); break;
					case 29: c.DropItem(new MalekisHonor()); break;
					case 30: c.DropItem(new Feathernock()); break;
					case 31: c.DropItem(new Swiftflight()); break;
				}
			}
		}

		Altar?.OnPeerlessDeath();
	}

	public BasePeerless(AIType aiType, FightMode fightMode, int rangePerception, int rangeFight, double activeSpeed, double passiveSpeed)
		: base(aiType, fightMode, rangePerception, rangeFight, activeSpeed, passiveSpeed)
	{
		_mNextFireRing = Core.TickCount + 10000;
		CurrentWave = MaxHelpersWaves;
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version

		writer.Write(Altar);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadInt();

		Altar = reader.ReadItem() as PeerlessAltar;
	}

	#region Helpers		
	public virtual bool CanSpawnHelpers => false;
	public virtual int MaxHelpersWaves => 0;
	public virtual double SpawnHelpersChance => 0.05;

	public int CurrentWave { get; set; }

	public bool AllHelpersDead => Altar == null || Altar.AllHelpersDead();

	public virtual bool CanSpawnWave()
	{
		if (MaxHelpersWaves > 0 && CurrentWave > 0)
		{
			double hits = (Hits / (double)HitsMax);
			double waves = (CurrentWave / (double)(MaxHelpersWaves + 1));

			if (hits < waves && Utility.RandomDouble() < SpawnHelpersChance)
			{
				CurrentWave -= 1;
				return true;
			}
		}

		return false;
	}

	public virtual void SpawnHelpers()
	{
	}

	public void SpawnHelper(BaseCreature helper, int range)
	{
		SpawnHelper(helper, GetSpawnPosition(range));
	}

	public void SpawnHelper(BaseCreature helper, int x, int y, int z)
	{
		SpawnHelper(helper, new Point3D(x, y, z));
	}

	public void SpawnHelper(BaseCreature helper, Point3D location)
	{
		if (helper == null)
			return;

		helper.Home = location;
		helper.RangeHome = 4;

		Altar?.AddHelper(helper);

		helper.MoveToWorld(location, Map);
	}

	#endregion

	public virtual void PackResources(int amount)
	{
		for (int i = 0; i < amount; i++)
			switch (Utility.Random(6))
			{
				case 0:
					PackItem(new Blight());
					break;
				case 1:
					PackItem(new Scourge());
					break;
				case 2:
					PackItem(new Taint());
					break;
				case 3:
					PackItem(new Putrefaction());
					break;
				case 4:
					PackItem(new Corruption());
					break;
				case 5:
					PackItem(new Muculent());
					break;
			}
	}

	public virtual void PackItems(Item item, int amount)
	{
		for (int i = 0; i < amount; i++)
			PackItem(item);
	}

	public virtual void PackTalismans(int amount)
	{
		int count = Utility.Random(amount);

		for (int i = 0; i < count; i++)
			PackItem(Loot.RandomTalisman());
	}

	#region Fire Ring
	private static readonly int[] MNorth = {
		-1, -1,
		1, -1,
		-1, 2,
		1, 2
	};

	private static readonly int[] MEast = {
		-1, 0,
		2, 0
	};

	public virtual bool HasFireRing => false;
	public virtual double FireRingChance => 1.0;

	private long _mNextFireRing = Core.TickCount;

	public virtual void FireRing()
	{
		for (int i = 0; i < MNorth.Length; i += 2)
		{
			Point3D p = Location;

			p.X += MNorth[i];
			p.Y += MNorth[i + 1];

			IPoint3D po = p;

			SpellHelper.GetSurfaceTop(ref po);

			Effects.SendLocationEffect(po, Map, 0x3E27, 50);
		}

		for (int i = 0; i < MEast.Length; i += 2)
		{
			Point3D p = Location;

			p.X += MEast[i];
			p.Y += MEast[i + 1];

			IPoint3D po = p;

			SpellHelper.GetSurfaceTop(ref po);

			Effects.SendLocationEffect(po, Map, 0x3E31, 50);
		}

		_mNextFireRing = Core.TickCount + 10000;
	}
	#endregion
}
