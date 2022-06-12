namespace Server.Items;

public class JackalsCollar : PlateGorget
{
	public override int LabelNumber => 1061594;  // Jackal's Collar
	public override int ArtifactRarity => 11;
	public override int BaseFireResistance => 23;
	public override int BaseColdResistance => 17;
	public override int InitHits => Utility.RandomMinMax(255, 255);

	[Constructable]
	public JackalsCollar()
	{
		Hue = 0x6D1;
		Attributes.BonusDex = 15;
		Attributes.RegenHits = 2;
	}

	public JackalsCollar(Serial serial) : base(serial)
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
