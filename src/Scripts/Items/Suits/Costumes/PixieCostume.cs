namespace Server.Items;

public class PixieCostume : BaseCostume
{
	[Constructable]
	public PixieCostume() : base("pixie", 0x0, 128)
	{
	}

	public override int LabelNumber => 1114236;// pixie costume

	public PixieCostume(Serial serial) : base(serial)
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
