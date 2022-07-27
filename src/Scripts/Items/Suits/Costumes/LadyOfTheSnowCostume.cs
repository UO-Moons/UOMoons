namespace Server.Items;

public class LadyOfTheSnowCostume : BaseCostume
{
	[Constructable]
	public LadyOfTheSnowCostume() : base("lady of the snow", 0x0, 252)
	{
	}

	public override int LabelNumber => 1114241;// Lady of the Snow costume

	public LadyOfTheSnowCostume(Serial serial) : base(serial)
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
