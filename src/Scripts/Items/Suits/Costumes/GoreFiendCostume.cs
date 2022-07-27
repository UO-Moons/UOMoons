namespace Server.Items;

public class GoreFiendCostume : BaseCostume
{
	[Constructable]
	public GoreFiendCostume() : base("gore fiend", 0x0, 305)
	{
	}

	public override int LabelNumber => 1114227;// gore fiend costume

	public GoreFiendCostume(Serial serial) : base(serial)
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
