namespace Server.Items;

public class MalekisHonor : MetalKiteShield
{
	public override int LabelNumber => 1074312;// Maleki's Honor (Juggernaut Set)
	public override SetItem SetId => SetItem.Juggernaut;
	public override int Pieces => 2;
	public override int BasePhysicalResistance => 3;
	public override int BaseFireResistance => 3;
	public override int BaseColdResistance => 3;
	public override int BasePoisonResistance => 3;
	public override int BaseEnergyResistance => 3;
	public override bool IsArtifact => true;

	[Constructable]
	public MalekisHonor()
		: base()
	{
		SetHue = 0x76D;
		SetSelfRepair = 3;
		SetAttributes.DefendChance = 10;
		SetAttributes.BonusStr = 10;
		SetAttributes.WeaponSpeed = 35;
	}

	public MalekisHonor(Serial serial)
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
