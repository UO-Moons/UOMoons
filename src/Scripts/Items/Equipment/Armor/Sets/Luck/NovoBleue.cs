namespace Server.Items;

public class NovoBleue : GoldBracelet
{
	public override int LabelNumber => 1080239;  // Novo Bleue
	public override SetItem SetID => SetItem.Luck;
	public override int Pieces => 2;
	public override bool IsArtifact => true;

	[Constructable]
	public NovoBleue() : base()
	{
		Hue = 1165;
		Attributes.Luck = 150;
		Attributes.CastSpeed = 1;
		Attributes.CastRecovery = 1;
		SetHue = 1165;
		SetAttributes.Luck = 100;
		SetAttributes.RegenHits = 2;
		SetAttributes.RegenMana = 2;
		SetAttributes.CastSpeed = 1;
		SetAttributes.CastRecovery = 4;
	}

	public NovoBleue(Serial serial) : base(serial)
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
