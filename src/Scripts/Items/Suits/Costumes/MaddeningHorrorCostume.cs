namespace Server.Items;

public class MaddeningHorrorCostume : BaseCostume
{
	[Constructable]
	public MaddeningHorrorCostume() : base("maddening horror", 0x0, 721)
	{
	}

	public override int LabelNumber => 1114233;// maddening horror costume

	public MaddeningHorrorCostume(Serial serial) : base(serial)
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
