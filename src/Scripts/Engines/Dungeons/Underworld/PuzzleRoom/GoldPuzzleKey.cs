namespace Server.Items;

public class GoldPuzzleKey : BaseItem
{
	public override int Lifespan => 1800;
	public override int LabelNumber => 1024111;  // gold key

	[Constructable]
	public GoldPuzzleKey() : base(4114)
	{
		Hue = 1174;
	}

	public GoldPuzzleKey(Serial serial) : base(serial)
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
