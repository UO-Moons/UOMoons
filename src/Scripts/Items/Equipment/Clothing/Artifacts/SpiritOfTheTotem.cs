namespace Server.Items;

public class SpiritOfTheTotem : BearMask
{
	public override int LabelNumber => 1061599;  // Spirit of the Totem
	public override int ArtifactRarity => 11;
	public override int BasePhysicalResistance => 20;
	public override int InitHits => Utility.RandomMinMax(255, 255);

	[Constructable]
	public SpiritOfTheTotem()
	{
		Hue = 0x455;

		Attributes.BonusStr = 20;
		Attributes.ReflectPhysical = 15;
		Attributes.AttackChance = 15;
	}

	public SpiritOfTheTotem(Serial serial) : base(serial)
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
