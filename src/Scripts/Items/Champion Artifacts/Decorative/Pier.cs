namespace Server.Items;

public class Pier : BaseItem
{
	private static readonly int[] itemids = new int[]
	{
		0x3486, 0x348b, 0x3ae
	};

	[Constructable]
	public Pier()
		: base(itemids[Utility.Random(3)])
	{
	}

	public Pier(int itemid)
		: base(itemid)
	{
	}

	public Pier(Serial serial) : base(serial)
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
