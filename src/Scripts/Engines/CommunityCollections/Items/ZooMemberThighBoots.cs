namespace Server.Items;

public class ZooMemberThighBoots : ThighBoots
{
	public override int LabelNumber => 1073221;// Britannia Royal Zoo Member

	[Constructable]
	public ZooMemberThighBoots()
		: this(0)
	{
	}

	[Constructable]
	public ZooMemberThighBoots(int hue)
		: base(hue)
	{
	}

	public ZooMemberThighBoots(Serial serial)
		: base(serial)
	{
	}

	public override bool Dye(Mobile from, DyeTub sender)
	{
		from.SendLocalizedMessage(sender.FailMessage);
		return false;
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
