namespace Server.Items;

public class ArtifactLargeVase : BaseItem
{
	[Constructable]
	public ArtifactLargeVase() : base(0x0B47)
	{
	}

	public ArtifactLargeVase(Serial serial) : base(serial)
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
		_ = reader.ReadInt();
	}
}
