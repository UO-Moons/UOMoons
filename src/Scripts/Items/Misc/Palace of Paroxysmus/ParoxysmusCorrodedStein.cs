namespace Server.Items;

public class ParoxysmusCorrodedStein : BaseItem
{
	public override int LabelNumber => 1072083;  // Paroxysmus' Corroded Stein

	[Constructable]
	public ParoxysmusCorrodedStein() : base(0x9D6)
	{
	}

	public ParoxysmusCorrodedStein(Serial serial) : base(serial)
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
