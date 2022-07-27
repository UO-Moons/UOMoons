using Server.Network;
using System;

namespace Server.Items;

public enum SawTrapType
{
	WestWall,
	NorthWall,
	WestFloor,
	NorthFloor
}

public class SawTrap : BaseTrap
{
	[CommandProperty(AccessLevel.GameMaster)]
	private SawTrapType Type
	{
		get
		{
			return ItemId switch
			{
				0x1103 => SawTrapType.NorthWall,
				0x1116 => SawTrapType.WestWall,
				0x11AC => SawTrapType.NorthFloor,
				0x11B1 => SawTrapType.WestFloor,
				_ => SawTrapType.NorthWall
			};
		}
	}

	private static int GetBaseId(SawTrapType type)
	{
		return type switch
		{
			SawTrapType.NorthWall => 0x1103,
			SawTrapType.WestWall => 0x1116,
			SawTrapType.NorthFloor => 0x11AC,
			SawTrapType.WestFloor => 0x11B1,
			_ => 0
		};
	}

	[Constructable]
	public SawTrap() : this(SawTrapType.NorthFloor)
	{
	}

	[Constructable]
	public SawTrap(SawTrapType type) : base(GetBaseId(type))
	{
	}

	protected override bool PassivelyTriggered => false;
	protected override TimeSpan PassiveTriggerDelay => TimeSpan.Zero;
	protected override int PassiveTriggerRange => 0;
	protected override TimeSpan ResetDelay => TimeSpan.FromSeconds(0.0);

	protected override void OnTrigger(Mobile from)
	{
		if (!from.Alive || from.AccessLevel > AccessLevel.Player)
			return;

		Effects.SendLocationEffect(Location, Map, GetBaseId(Type) + 1, 6, 3, GetEffectHue(), 0);
		Effects.PlaySound(Location, Map, 0x21C);

		Spells.SpellHelper.Damage(TimeSpan.FromTicks(1), from, from, Utility.RandomMinMax(5, 15));

		from.LocalOverheadMessage(MessageType.Regular, 0x22, 500853); // You stepped onto a blade trap!
	}

	public SawTrap(Serial serial) : base(serial)
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
