namespace Server.Items;

public class DreamWraithCostume : BaseCostume
{
	[Constructable]
	public DreamWraithCostume() : base("dream wraith", 0x0, 740)
	{
	}

	public override int LabelNumber => 1114008;// dream wraith halloween costume

	public DreamWraithCostume(Serial serial) : base(serial)
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
