namespace Server.Items;

public class TravestysFineTeakwoodTray : BaseItem
{
	public override int LabelNumber => 1075094;  // Travesty's Fine Teakwood Tray

	[Constructable]
	public TravestysFineTeakwoodTray() : base(Utility.Random(0x991, 2))
	{
	}

	public TravestysFineTeakwoodTray(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadInt();
	}
}
