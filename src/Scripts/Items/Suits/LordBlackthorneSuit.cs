namespace Server.Items;

public class LordBlackthorneSuit : BaseSuit
{
	[Constructable]
	public LordBlackthorneSuit() : base(AccessLevel.GameMaster, 0x0, 0x2043)
	{
	}

	public LordBlackthorneSuit(Serial serial) : base(serial)
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
