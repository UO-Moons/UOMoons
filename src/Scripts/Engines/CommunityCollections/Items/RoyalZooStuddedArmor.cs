namespace Server.Items;

public class RoyalZooStuddedLegs : StuddedLegs
{
	public override bool IsArtifact => true;
	[Constructable]
	public RoyalZooStuddedLegs()
	{
		Hue = 0x109;
		Attributes.BonusHits = 2;
		Attributes.BonusMana = 3;
		Attributes.LowerManaCost = 10;
		ArmorAttributes.MageArmor = 1;
	}

	public RoyalZooStuddedLegs(Serial serial)
		: base(serial)
	{
	}

	public override int LabelNumber => 1073223;// Studded Armor of the Britannia Royal Zoo
	public override int BasePhysicalResistance => 10;
	public override int BaseFireResistance => 10;
	public override int BaseColdResistance => 10;
	public override int BasePoisonResistance => 10;
	public override int BaseEnergyResistance => 10;
	public override int InitMinHits => 255;
	public override int InitMaxHits => 255;
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

public class RoyalZooStuddedGloves : StuddedGloves
{
	public override bool IsArtifact => true;
	[Constructable]
	public RoyalZooStuddedGloves()
	{
		Hue = 0x109;
		Attributes.BonusHits = 2;
		Attributes.BonusMana = 3;
		Attributes.LowerManaCost = 10;
		ArmorAttributes.MageArmor = 1;
	}

	public RoyalZooStuddedGloves(Serial serial)
		: base(serial)
	{
	}

	public override int LabelNumber => 1073223;// Studded Armor of the Britannia Royal Zoo
	public override int BasePhysicalResistance => 10;
	public override int BaseFireResistance => 10;
	public override int BaseColdResistance => 10;
	public override int BasePoisonResistance => 10;
	public override int BaseEnergyResistance => 10;
	public override int InitMinHits => 255;
	public override int InitMaxHits => 255;
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

public class RoyalZooStuddedGorget : StuddedGorget
{
	public override bool IsArtifact => true;
	[Constructable]
	public RoyalZooStuddedGorget()
	{
		Hue = 0x109;
		Attributes.BonusHits = 2;
		Attributes.BonusMana = 3;
		Attributes.LowerManaCost = 10;
		ArmorAttributes.MageArmor = 1;
	}

	public RoyalZooStuddedGorget(Serial serial)
		: base(serial)
	{
	}

	public override int LabelNumber => 1073223;// Studded Armor of the Britannia Royal Zoo
	public override int BasePhysicalResistance => 10;
	public override int BaseFireResistance => 10;
	public override int BaseColdResistance => 10;
	public override int BasePoisonResistance => 10;
	public override int BaseEnergyResistance => 10;
	public override int InitMinHits => 255;
	public override int InitMaxHits => 255;
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

public class RoyalZooStuddedArms : StuddedArms
{
	public override bool IsArtifact => true;
	[Constructable]
	public RoyalZooStuddedArms()
	{
		Hue = 0x109;
		Attributes.BonusHits = 2;
		Attributes.BonusMana = 3;
		Attributes.LowerManaCost = 10;
		ArmorAttributes.MageArmor = 1;
	}

	public RoyalZooStuddedArms(Serial serial)
		: base(serial)
	{
	}

	public override int LabelNumber => 1073223;// Studded Armor of the Britannia Royal Zoo
	public override int BasePhysicalResistance => 10;
	public override int BaseFireResistance => 10;
	public override int BaseColdResistance => 10;
	public override int BasePoisonResistance => 10;
	public override int BaseEnergyResistance => 10;
	public override int InitMinHits => 255;
	public override int InitMaxHits => 255;
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

public class RoyalZooStuddedChest : StuddedChest
{
	public override bool IsArtifact => true;
	[Constructable]
	public RoyalZooStuddedChest()
	{
		Hue = 0x109;
		Attributes.BonusHits = 2;
		Attributes.BonusMana = 3;
		Attributes.LowerManaCost = 10;
		ArmorAttributes.MageArmor = 1;
	}

	public RoyalZooStuddedChest(Serial serial)
		: base(serial)
	{
	}

	public override int LabelNumber => 1073223;// Studded Armor of the Britannia Royal Zoo
	public override int BasePhysicalResistance => 10;
	public override int BaseFireResistance => 10;
	public override int BaseColdResistance => 10;
	public override int BasePoisonResistance => 10;
	public override int BaseEnergyResistance => 10;
	public override int InitMinHits => 255;
	public override int InitMaxHits => 255;
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

public class RoyalZooStuddedFemaleChest : FemaleStuddedChest
{
	public override bool IsArtifact => true;
	[Constructable]
	public RoyalZooStuddedFemaleChest()
	{
		Hue = 0x109;
		Attributes.BonusHits = 2;
		Attributes.BonusMana = 3;
		Attributes.LowerManaCost = 10;
		ArmorAttributes.MageArmor = 1;
	}

	public RoyalZooStuddedFemaleChest(Serial serial)
		: base(serial)
	{
	}

	public override int LabelNumber => 1073223;// Studded Armor of the Britannia Royal Zoo
	public override int BasePhysicalResistance => 10;
	public override int BaseFireResistance => 10;
	public override int BaseColdResistance => 10;
	public override int BasePoisonResistance => 10;
	public override int BaseEnergyResistance => 10;
	public override int InitMinHits => 255;
	public override int InitMaxHits => 255;
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
