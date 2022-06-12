namespace Server.Items;

public class FireRubyBracelet : GoldBracelet
{
	public override int LabelNumber => 1073454;// fire ruby bracelet

	[Constructable]
	public FireRubyBracelet()
			: base()
	{
		Weight = 1.0;

		BaseRunicTool.ApplyAttributesTo(this, true, 0, Utility.RandomMinMax(1, 4), 0, 100);

		if (Utility.Random(100) < 10)
			Attributes.RegenHits += 2;
		else
			Resistances.Fire += 10;
	}

	public FireRubyBracelet(Serial serial)
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
