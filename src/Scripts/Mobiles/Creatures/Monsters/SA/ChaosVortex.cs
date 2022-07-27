using System;
using Server.Items;

namespace Server.Mobiles;

[CorpseName("a chaos vortex corpse")]
public class ChaosVortex : BaseCreature
{
	[Constructable]
	public ChaosVortex()
		: base(AIType.AI_Melee, FightMode.Weakest, 10, 1, 0.4, 0.2)
	{
		Name = "a chaos vortex";
		Body = 164;
		Hue = 34212;

		SetStr(450);
		SetDex(200);
		SetInt(100);

		SetHits(27000);
		SetMana(0);

		SetDamage(21, 23);

		SetDamageType(ResistanceType.Physical, 20);
		SetDamageType(ResistanceType.Fire, 20);
		SetDamageType(ResistanceType.Cold, 20);
		SetDamageType(ResistanceType.Poison, 20);
		SetDamageType(ResistanceType.Energy, 20);

		SetResistance(ResistanceType.Physical, 65, 75);
		SetResistance(ResistanceType.Fire, 65, 75);
		SetResistance(ResistanceType.Cold, 65, 75);
		SetResistance(ResistanceType.Poison, 65, 75);
		SetResistance(ResistanceType.Energy, 65, 75);

		SetSkill(SkillName.MagicResist, 100, 110);
		SetSkill(SkillName.Tactics, 110, 130);
		SetSkill(SkillName.Wrestling, 124, 140);

		Fame = 22500;
		Karma = -22500;
	}

	public override int GetAngerSound()
	{
		return 0x15;
	}

	public override int GetAttackSound()
	{
		return 0x28;
	}

	public override bool AlwaysMurderer => true;
	public override bool BleedImmune => true;
	public override Poison PoisonImmune => Poison.Lethal;

	private DateTime NextTeleport { get; set; }

	public override void AlterSpellDamageFrom(Mobile from, ref int damage)
	{
		if (from is BaseCreature creature && (creature.Summoned || creature.Controlled))
			damage /= 2;

		if (NextTeleport < DateTime.UtcNow)
			DoTeleport(from);

		base.AlterSpellDamageFrom(from, ref damage);
	}

	public override void AlterMeleeDamageFrom(Mobile from, ref int damage)
	{
		if (from is BaseCreature creature && (creature.Summoned || creature.Controlled))
			damage /= 2;

		if (NextTeleport < DateTime.UtcNow)
			DoTeleport(from);

		base.AlterMeleeDamageFrom(from, ref damage);
	}

	public void DoTeleport(Mobile m)
	{
		if (!InRange(m.Location, 1))
		{
			Point3D p = Point3D.Zero;

			for (int i = 0; i < 25; i++)
			{
				var x = Utility.RandomMinMax(X - 1, X + 1);
				var y = Utility.RandomMinMax(Y - 1, Y + 1);
				var z = Map.GetAverageZ(x, y);

				if (!Map.CanSpawnMobile(x, y, z) || (x == X && y == Y))
					continue;
				p = new Point3D(x, y, z);
				break;
			}

			if (p == Point3D.Zero)
				p = Location;

			Point3D from = m.Location;

			Effects.SendLocationParticles(EffectItem.Create(from, m.Map, EffectItem.DefaultDuration), 0x3728, 10, 10, 2023);
			Effects.SendLocationParticles(EffectItem.Create(p, m.Map, EffectItem.DefaultDuration), 0x3728, 10, 10, 5023);

			m.MoveToWorld(p, Map);

			m.PlaySound(0x1FE);
		}

		NextTeleport = DateTime.UtcNow + TimeSpan.FromSeconds(Utility.RandomMinMax(30, 60));
	}

	public override void GenerateLoot()
	{
		AddLoot(LootPack.FilthyRich, 2);
		AddLoot(LootPack.LootItemCallback(ClayGolem.CheckSpawnCrystal, 33.0, 5, false, false));
	}

	public ChaosVortex(Serial serial)
		: base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadInt();
	}
}
