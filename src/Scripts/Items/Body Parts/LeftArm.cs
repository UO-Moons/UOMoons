namespace Server.Items;

public class LeftArm : BaseItem
{
	[Constructable]
	public LeftArm() : base(0x1DA1)
	{
	}

	public LeftArm(Serial serial) : base(serial)
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
