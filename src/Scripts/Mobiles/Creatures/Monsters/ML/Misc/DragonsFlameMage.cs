using Server.Items;

namespace Server.Mobiles;

[CorpseName("a black order mage corpse")]
public class DragonsFlameMage : BaseCreature
{
	[Constructable]
	public DragonsFlameMage()
		: base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
	{
		Name = "Black Order Mage";
		Title = "of the Dragon's Flame Sect";
		Female = Utility.RandomBool();
		Race = Race.Human;
		Hue = Race.RandomSkinHue();
		HairItemId = Race.RandomHair(Female);
		HairHue = Race.RandomHairHue();
		_ = Race.RandomFacialHair(this);

		AddItem(new NinjaTabi());
		AddItem(new FancyShirt(0x51D));
		AddItem(new Hakama(0x51D));
		AddItem(new Kasa(0x51D));

		SetStr(340, 360);
		SetDex(200, 215);
		SetInt(400, 415);

		SetHits(600, 615);

		SetDamage(13, 15);

		SetDamageType(ResistanceType.Physical, 10);
		SetDamageType(ResistanceType.Fire, 20);
		SetDamageType(ResistanceType.Cold, 20);
		SetDamageType(ResistanceType.Energy, 50);

		SetResistance(ResistanceType.Physical, 40, 50);
		SetResistance(ResistanceType.Fire, 30, 50);
		SetResistance(ResistanceType.Cold, 55, 60);
		SetResistance(ResistanceType.Poison, 50, 60);
		SetResistance(ResistanceType.Energy, 60, 70);

		SetSkill(SkillName.EvalInt, 70.1, 80.0);
		SetSkill(SkillName.Magery, 90.1, 100.0);
		SetSkill(SkillName.MagicResist, 85.1, 95.0);
		SetSkill(SkillName.Tactics, 70.1, 80.0);
		SetSkill(SkillName.Wrestling, 60.1, 80.0);

		Fame = 13000;
		Karma = -13000;

		VirtualArmor = 58;
	}

	public DragonsFlameMage(Serial serial)
		: base(serial)
	{
	}

	public override bool AlwaysMurderer => true;
	//public override bool DisplayFameTitle => false;

	public override void GenerateLoot()
	{
		AddLoot(LootPack.AosFilthyRich, 4);
	}

	public override void AlterSpellDamageFrom(Mobile from, ref int damage)
	{
		if (from != null)
			_ = from.Damage(damage / 2, from);
	}

	public override void OnDeath(Container c)
	{
		if (Utility.RandomDouble() < 0.3)
			c.DropItem(new DragonFlameSectBadge());

		base.OnDeath(c);
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		_ = reader.ReadInt();
	}
}