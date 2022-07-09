namespace Server.Items;

[Furniture]
public class StandingKittenMailbox : Mailbox
{
	public override int LabelNumber => 1113927;  // Mailbox

	public override int DefaultGumpID => 0x9D38;

	public override int SouthMailBoxId => 0xA3F2;
	public override int SouthEmptyMailBoxId => 0xA3F1;
	public override int EastMailBoxId => 0xA3F0;
	public override int EastEmptyMailBoxId => 0xA3EF;

	[Constructable]
	public StandingKittenMailbox()
		: base(0xA3EF)
	{
	}

	public StandingKittenMailbox(Serial serial)
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
