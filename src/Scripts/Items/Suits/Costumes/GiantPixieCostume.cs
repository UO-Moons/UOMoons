namespace Server.Items;

public class GiantPixieCostume : BaseCostume
{
	[Constructable]
	public GiantPixieCostume() : base("giant pixie", 0x0, 176)
	{
	}

	public override int LabelNumber => 1114244;// giant pixie costume

	public GiantPixieCostume(Serial serial) : base(serial)
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
