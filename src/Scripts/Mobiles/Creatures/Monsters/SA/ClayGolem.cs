using Server.Items;

namespace Server.Mobiles;

[CorpseName("a clay golem corpse")]
public class ClayGolem : Golem
{
	[Constructable]
	public ClayGolem()
	{
		Name = "a clay golem";
		Hue = 654;

		SetStr(450, 600);
		SetDex(100, 150);
		SetInt(100, 150);

		SetHits(700, 900);

		SetDamage(13, 24);

		SetDamageType(ResistanceType.Physical, 100);

		SetResistance(ResistanceType.Physical, 45, 55);
		SetResistance(ResistanceType.Fire, 50, 60);
		SetResistance(ResistanceType.Cold, 45, 55);
		SetResistance(ResistanceType.Poison, 99);
		SetResistance(ResistanceType.Energy, 35, 45);

		SetSkill(SkillName.MagicResist, 150, 200);
		SetSkill(SkillName.Tactics, 80, 120);
		SetSkill(SkillName.Wrestling, 80, 110);
		SetSkill(SkillName.Parry, 70, 80);
		SetSkill(SkillName.DetectHidden, 70.0, 80.0);

		Fame = 4500;
		Karma = -4500;
	}

	public override void GenerateLoot()
	{
		AddLoot(LootPack.Rich, 2);
		AddLoot(LootPack.LootItem<ExecutionersCap>());
		AddLoot(LootPack.LootItemCallback(SpawnGears, 5.0, 1, false, false));
		AddLoot(LootPack.LootItemCallback(CheckSpawnCrystal, 20.0, 1, false, false));
	}

	public static Item CheckSpawnCrystal(IEntity e)
	{
		return Region.Find(e.Location, e.Map).IsPartOf("Shame") ? new ShameCrystal() : null;
	}

	public ClayGolem(Serial serial)
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
