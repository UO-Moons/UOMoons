namespace Server.Items;

public class BloodwormCostume : BaseCostume
{
	[Constructable]
	public BloodwormCostume() : base("bloodworm", 0x0, 287)
	{
	}

	public override int LabelNumber => 1114006;// bloodworm halloween costume

	public BloodwormCostume(Serial serial) : base(serial)
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