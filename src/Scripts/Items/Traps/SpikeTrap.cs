using Server.Network;
using System;

namespace Server.Items;

public enum SpikeTrapType
{
	WestWall,
	NorthWall,
	WestFloor,
	NorthFloor
}

public sealed class SpikeTrap : BaseTrap
{
	[CommandProperty(AccessLevel.GameMaster)]
	private SpikeTrapType Type
	{
		get
		{
			switch (ItemId)
			{
				case 4360: case 4361: case 4366: return SpikeTrapType.WestWall;
				case 4379: case 4380: case 4385: return SpikeTrapType.NorthWall;
				case 4506: case 4507: case 4511: return SpikeTrapType.WestFloor;
				case 4512: case 4513: case 4517: return SpikeTrapType.NorthFloor;
			}

			return SpikeTrapType.WestWall;
		}
	}

	private bool Extended
	{
		set => ItemId = value ? GetExtendedId(Type) : GetBaseId(Type);
	}

	private static int GetBaseId(SpikeTrapType type)
	{
		return type switch
		{
			SpikeTrapType.WestWall => 4360,
			SpikeTrapType.NorthWall => 4379,
			SpikeTrapType.WestFloor => 4506,
			SpikeTrapType.NorthFloor => 4512,
			_ => 0
		};
	}

	private static int GetExtendedId(SpikeTrapType type)
	{
		return GetBaseId(type) + GetExtendedOffset(type);
	}

	private static int GetExtendedOffset(SpikeTrapType type)
	{
		return type switch
		{
			SpikeTrapType.WestWall => 6,
			SpikeTrapType.NorthWall => 6,
			SpikeTrapType.WestFloor => 5,
			SpikeTrapType.NorthFloor => 5,
			_ => 0
		};
	}

	[Constructable]
	public SpikeTrap() : this(SpikeTrapType.WestFloor)
	{
	}

	[Constructable]
	public SpikeTrap(SpikeTrapType type) : base(GetBaseId(type))
	{
	}

	protected override bool PassivelyTriggered => false;
	protected override TimeSpan PassiveTriggerDelay => TimeSpan.Zero;
	protected override int PassiveTriggerRange => 0;
	protected override TimeSpan ResetDelay => TimeSpan.FromSeconds(6.0);

	protected override void OnTrigger(Mobile from)
	{
		if (!from.Alive || from.AccessLevel > AccessLevel.Player)
			return;

		Effects.SendLocationEffect(Location, Map, GetBaseId(Type) + 1, 18, 3, GetEffectHue(), 0);
		Effects.PlaySound(Location, Map, 0x22C);

		foreach (Mobile mob in GetMobilesInRange(0))
		{
			if (mob.Alive && !mob.IsDeadBondedPet)
				Spells.SpellHelper.Damage(TimeSpan.FromTicks(1), mob, mob, Utility.RandomMinMax(1, 6) * 6);
		}

		Timer.DelayCall(TimeSpan.FromSeconds(1.0), OnSpikeExtended);

		from.LocalOverheadMessage(MessageType.Regular, 0x22, 500852); // You stepped onto a spike trap!
	}

	private void OnSpikeExtended()
	{
		Extended = true;
		Timer.DelayCall(TimeSpan.FromSeconds(5.0), OnSpikeRetracted);
	}

	private void OnSpikeRetracted()
	{
		Extended = false;
		Effects.SendLocationEffect(Location, Map, GetExtendedId(Type) - 1, 6, 3, GetEffectHue(), 0);
	}

	public SpikeTrap(Serial serial) : base(serial)
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
		Extended = false;
	}
}
