namespace Server.Items;

public class EcruCitrineRing : GoldRing
{
	public override int LabelNumber => 1073457; // ecru citrine ring

	[Constructable]
	public EcruCitrineRing()
		: base()
	{
		Weight = 1.0;

		BaseRunicTool.ApplyAttributesTo(this, true, 0, Utility.RandomMinMax(2, 3), 0, 100);

		if (Utility.RandomBool())
			Attributes.EnhancePotions = 50;
		else
			Attributes.BonusStr += 5;
	}

	public EcruCitrineRing(Serial serial)
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
		_ = reader.ReadInt();
	}
}
