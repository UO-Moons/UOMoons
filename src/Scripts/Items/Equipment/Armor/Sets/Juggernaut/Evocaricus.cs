namespace Server.Items;

public class Evocaricus : VikingSword
{
	public override int LabelNumber => 1074309;// Evocaricus (Juggernaut Set)
	public override SetItem SetId => SetItem.Juggernaut;
	public override int Pieces => 2;
	public override bool IsArtifact => true;

	[Constructable]
	public Evocaricus()
		: base()
	{
		SetHue = 0x76D;
		Attributes.WeaponDamage = 50;
		SetSelfRepair = 3;
		SetAttributes.DefendChance = 10;
		SetAttributes.BonusStr = 10;
		SetAttributes.WeaponSpeed = 35;
	}

	public Evocaricus(Serial serial)
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
		_ = reader.ReadInt();
	}
}
