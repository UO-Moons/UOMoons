using Server.Items;

namespace Server.Mobiles;

[CorpseName("a black order grand mage corpse")]
public class DragonsFlameGrandMage : DragonsFlameMage
{
	[Constructable]
	public DragonsFlameGrandMage()
	{
		Name = "Black Order Grand Mage";
		Title = "of the Dragon's Flame Sect";
		SetStr(340, 360);
		SetDex(200, 215);
		SetInt(500, 515);

		SetHits(800);

		SetDamage(15, 20);

		Fame = 25000;
		Karma = -25000;

		VirtualArmor = 60;
	}

	public DragonsFlameGrandMage(Serial serial)
		: base(serial)
	{
	}

	public override bool AlwaysMurderer => true;
	//public override bool DisplayFameTitle => false;

	public override void GenerateLoot()
	{
		AddLoot(LootPack.AosFilthyRich, 6);
	}

	public override void OnDeath(Container c)
	{
		c.DropItem(new DragonFlameKey());

		if (Utility.RandomDouble() < 0.5)
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
