using Server.Regions;
using System;

namespace Server.Items;

public sealed class MushroomTrap : BaseTrap
{
	[Constructable]
	public MushroomTrap() : base(0x1125)
	{
	}

	protected override bool PassivelyTriggered => true;
	protected override TimeSpan PassiveTriggerDelay => TimeSpan.Zero;
	protected override int PassiveTriggerRange => 2;
	protected override TimeSpan ResetDelay => TimeSpan.Zero;

	protected override void OnTrigger(Mobile from)
	{
		if (!from.Alive || ItemId != 0x1125 || from.AccessLevel > AccessLevel.Player)
			return;

		ItemId = 0x1126;
		Effects.PlaySound(Location, Map, 0x306);

		Spells.SpellHelper.Damage(TimeSpan.FromSeconds(0.5), from, from, Utility.Dice(2, 4, 0));

		Timer.DelayCall(TimeSpan.FromSeconds(2.0), OnMushroomReset);
	}

	private void OnMushroomReset()
	{
		if (Region.Find(Location, Map).IsPartOf(typeof(DungeonRegion)))
			ItemId = 0x1125; // reset
		else
			Delete();
	}

	public MushroomTrap(Serial serial) : base(serial)
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

		if (ItemId == 0x1126)
			OnMushroomReset();
	}
}
