namespace Server.Items;

public class WolfStatue : BaseItem
{
	public override bool IsArtifact => true;
	[Constructable]
	public WolfStatue()
		: base(0x25D3)
	{
		LootType = LootType.Blessed;
		Weight = 1.0;
	}

	public WolfStatue(Serial serial)
		: base(serial)
	{
	}

	public override int LabelNumber => 1073190;// A Wolf Contribution Statue from the Britannia Royal Zoo.
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
