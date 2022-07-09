namespace Server.Items;

[Furniture]
public class ScarecrowMailbox : Mailbox
{
	public override int LabelNumber => 1113927;  // Mailbox
	public override int DefaultGumpID => 0x9D39;
	public override int SouthMailBoxId => 0xA3F5;
	public override int SouthEmptyMailBoxId => 0xA3F6;	
	public override int EastMailBoxId => 0xA3F3;	
	public override int EastEmptyMailBoxId => 0xA3F4;

	[Constructable]
	public ScarecrowMailbox()
		: base(0xA3F4)
	{
	}

	public ScarecrowMailbox(Serial serial)
		: base(serial)
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
