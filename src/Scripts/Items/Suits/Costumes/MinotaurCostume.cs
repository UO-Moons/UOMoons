namespace Server.Items;

public class MinotaurCostume : BaseCostume
{
	[Constructable]
	public MinotaurCostume() : base("minotaur", 0x0, 263)
	{
	}

	public override int LabelNumber => 1114237;// minotaur costume

	public MinotaurCostume(Serial serial) : base(serial)
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
