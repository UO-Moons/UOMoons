using System;

namespace Server.Items;

public sealed class FireColumnTrap : BaseTrap
{
	protected override bool PassivelyTriggered => true;
	protected override TimeSpan PassiveTriggerDelay => TimeSpan.FromSeconds(2.0);
	protected override int PassiveTriggerRange => 3;
	protected override TimeSpan ResetDelay => TimeSpan.FromSeconds(0.5);

	[Constructable]
	public FireColumnTrap() : base(0x1B71)
	{
		MinDamage = 10;
		MaxDamage = 40;

		WarningFlame = true;
	}


	[CommandProperty(AccessLevel.GameMaster)]
	private int MinDamage { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	private int MaxDamage { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	private bool WarningFlame { get; set; }

	protected override void OnTrigger(Mobile from)
	{
		if (from.AccessLevel > AccessLevel.Player)
			return;

		if (WarningFlame)
			DoEffect();

		if (!from.Alive || !CheckRange(from.Location, 0))
			return;

		Spells.SpellHelper.Damage(TimeSpan.FromSeconds(0.5), from, from, Utility.RandomMinMax(MinDamage, MaxDamage), 0, 100, 0, 0, 0);

		if (!WarningFlame)
			DoEffect();
	}

	private void DoEffect()
	{
		Effects.SendLocationParticles(EffectItem.Create(Location, Map, EffectItem.DefaultDuration), 0x3709, 10, 30, 5052);
		Effects.PlaySound(Location, Map, 0x225);
	}

	public FireColumnTrap(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
		writer.Write(WarningFlame);
		writer.Write(MinDamage);
		writer.Write(MaxDamage);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		var version = reader.ReadInt();

		switch (version)
		{
			case 0:
			{
				WarningFlame = reader.ReadBool();
				MinDamage = reader.ReadInt();
				MaxDamage = reader.ReadInt();
				break;
			}
		}
	}
}
