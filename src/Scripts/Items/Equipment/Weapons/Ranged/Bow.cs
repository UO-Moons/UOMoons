using System;

namespace Server.Items;

[Flipable(0x13B2, 0x13B1)]
public class Bow : BaseRanged
{
	public override int EffectId => 0xF42;
	public override Type AmmoType => typeof(Arrow);
	public override Item Ammo => new Arrow();
	public override WeaponAbility PrimaryAbility => WeaponAbility.ParalyzingBlow;
	public override WeaponAbility SecondaryAbility => WeaponAbility.MortalStrike;
	public override int DefMaxRange => 10;
	public override int StrReq => Core.AOS ? 30 : 20;
	public override int MinDamageBase => Core.ML ? 15 : Core.AOS ? 16 : Core.UOR ? 9 : 9;
	public override int MaxDamageBase => Core.ML ? 19 : Core.AOS ? 18 : Core.UOR ? 41 : 24;
	public override float SpeedBase => Core.ML ? 4.25f : Core.AOS ? 25 : 20;
	public override int InitMinHits => 31;
	public override int InitMaxHits => 60;

	public override WeaponAnimation DefAnimation => WeaponAnimation.ShootBow;

	[Constructable]
	public Bow() : base(0x13B2)
	{
		Weight = 6.0;
		Layer = Layer.TwoHanded;
	}

	public Bow(Serial serial) : base(serial)
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
