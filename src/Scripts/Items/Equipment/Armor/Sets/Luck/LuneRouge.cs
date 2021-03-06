namespace Server.Items;

public class LuneRouge : GoldRing
{
	public override int LabelNumber => 1154372;  // Lune Rouge
	public override SetItem SetId => SetItem.Luck2;
	public override int Pieces => 2;
	public override bool IsArtifact => true;

	[Constructable]
	public LuneRouge() : base()
	{
		Hue = 1166;
		Attributes.Luck = 150;
		Attributes.AttackChance = 10;
		Attributes.WeaponDamage = 20;
		SetHue = 1166;
		SetAttributes.Luck = 100;
		SetAttributes.AttackChance = 10;
		SetAttributes.WeaponDamage = 20;
		SetAttributes.WeaponSpeed = 10;
		SetAttributes.RegenHits = 2;
		SetAttributes.RegenStam = 3;
	}

	public LuneRouge(Serial serial) : base(serial)
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
