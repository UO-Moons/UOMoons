using System;

namespace Server.Items;

[Flipable(0xF50, 0xF4F)]
public class Crossbow : BaseRanged
{
	public override int EffectId => 0x1BFE;
	public override Type AmmoType => typeof(CrossBowBolt);
	public override Item Ammo => new CrossBowBolt();
	public override WeaponAbility PrimaryAbility => WeaponAbility.ConcussionBlow;
	public override WeaponAbility SecondaryAbility => WeaponAbility.MortalStrike;
	public override int DefMaxRange => 8;
	public override int StrReq => Core.AOS ? 35 : 30;
	public override int MinDamageBase => Core.ML ? 18 : Core.AOS ? 18 : Core.UOR ? 8 : 6;
	public override int MaxDamageBase => Core.ML ? 22 : Core.AOS ? 20 : Core.UOR ? 43 : 26;
	public override float SpeedBase => Core.ML ? 4.50f : Core.AOS ? 24 : 18;
	public override int InitMinHits => 31;
	public override int InitMaxHits => 80;

	[Constructable]
	public Crossbow() : base(0xF50)
	{
		Weight = 7.0;
		Layer = Layer.TwoHanded;
	}

	public Crossbow(Serial serial) : base(serial)
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
