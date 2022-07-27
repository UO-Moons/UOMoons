namespace Server.Items;

public class DupreSuit : BaseSuit
{
	[Constructable]
	public DupreSuit() : base(AccessLevel.GameMaster, 0x0, 0x2050)
	{
	}

	public DupreSuit(Serial serial) : base(serial)
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
