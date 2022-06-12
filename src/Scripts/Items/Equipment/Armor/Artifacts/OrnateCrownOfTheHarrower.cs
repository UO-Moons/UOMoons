namespace Server.Items;

public class OrnateCrownOfTheHarrower : BoneHelm
{
	public override int LabelNumber => 1061095;  // Ornate Crown of the Harrower
	public override int ArtifactRarity => 11;
	public override int BasePoisonResistance => 17;
	public override int InitHits => Utility.RandomMinMax(255, 255);

	[Constructable]
	public OrnateCrownOfTheHarrower()
	{
		Hue = 0x4F6;
		Attributes.RegenHits = 2;
		Attributes.RegenStam = 3;
		Attributes.WeaponDamage = 25;
	}

	public OrnateCrownOfTheHarrower(Serial serial) : base(serial)
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
		_ = reader.ReadInt();
	}
}
