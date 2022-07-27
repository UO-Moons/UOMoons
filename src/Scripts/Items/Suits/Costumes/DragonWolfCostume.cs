namespace Server.Items;

public class DragonWolfCostume : BaseCostume
{
	public override string CreatureName => "dragon wolf";

	[Constructable]
	public DragonWolfCostume() : base("dragon wolf", 0x0, 719)
	{
	}

	public override string DefaultName => "a dragon wolf costume";

	public DragonWolfCostume(Serial serial) : base(serial)
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
