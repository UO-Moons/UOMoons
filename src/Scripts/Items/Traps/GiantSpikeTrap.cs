using System;

namespace Server.Items;

public class GiantSpikeTrap : BaseTrap
{
	[Constructable]
	public GiantSpikeTrap() : base(1)
	{
	}

	protected override bool PassivelyTriggered => true;
	protected override TimeSpan PassiveTriggerDelay => TimeSpan.Zero;
	protected override int PassiveTriggerRange => 3;
	protected override TimeSpan ResetDelay => TimeSpan.FromSeconds(0.0);

	protected override void OnTrigger(Mobile from)
	{
		if (from.AccessLevel > AccessLevel.Player)
			return;

		Effects.SendLocationEffect(Location, Map, 0x1D99, 48, 2, GetEffectHue(), 0);

		if (from.Alive && CheckRange(from.Location, 0))
			Spells.SpellHelper.Damage(TimeSpan.FromTicks(1), from, from, Utility.Dice(10, 7, 0));
	}

	public GiantSpikeTrap(Serial serial) : base(serial)
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
