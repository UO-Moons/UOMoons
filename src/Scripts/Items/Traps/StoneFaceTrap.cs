using System;

namespace Server.Items;

public enum StoneFaceTrapType
{
	NorthWestWall,
	NorthWall,
	WestWall
}

public class StoneFaceTrap : BaseTrap
{
	[CommandProperty(AccessLevel.GameMaster)]
	public StoneFaceTrapType Type
	{
		get
		{
			return ItemId switch
			{
				0x10F5 or 0x10F6 or 0x10F7 => StoneFaceTrapType.NorthWestWall,
				0x10FC or 0x10FD or 0x10FE => StoneFaceTrapType.NorthWall,
				0x110F or 0x1110 or 0x1111 => StoneFaceTrapType.WestWall,
				_ => StoneFaceTrapType.NorthWestWall,
			};
		}
		init
		{
			bool breathing = Breathing;

			ItemId = breathing ? GetFireId(value) : GetBaseId(value);
		}
	}

	private bool Breathing
	{
		get => ItemId == GetFireId(Type);
		set => ItemId = value ? GetFireId(Type) : GetBaseId(Type);
	}

	private static int GetBaseId(StoneFaceTrapType type)
	{
		return type switch
		{
			StoneFaceTrapType.NorthWestWall => 0x10F5,
			StoneFaceTrapType.NorthWall => 0x10FC,
			StoneFaceTrapType.WestWall => 0x110F,
			_ => 0
		};
	}

	private static int GetFireId(StoneFaceTrapType type)
	{
		return type switch
		{
			StoneFaceTrapType.NorthWestWall => 0x10F7,
			StoneFaceTrapType.NorthWall => 0x10FE,
			StoneFaceTrapType.WestWall => 0x1111,
			_ => 0
		};
	}

	[Constructable]
	public StoneFaceTrap() : base(0x10FC)
	{
		Light = LightType.Circle225;
	}

	protected override bool PassivelyTriggered => true;
	protected override TimeSpan PassiveTriggerDelay => TimeSpan.Zero;
	protected override int PassiveTriggerRange => 2;
	protected override TimeSpan ResetDelay => TimeSpan.Zero;

	protected override void OnTrigger(Mobile from)
	{
		if (!from.Alive || from.AccessLevel > AccessLevel.Player)
			return;

		Effects.PlaySound(Location, Map, 0x359);

		Breathing = true;

		Timer.DelayCall(TimeSpan.FromSeconds(2.0), FinishBreath);
		Timer.DelayCall(TimeSpan.FromSeconds(1.0), TriggerDamage);
	}

	public virtual void FinishBreath()
	{
		Breathing = false;
	}

	public virtual void TriggerDamage()
	{
		foreach (Mobile mob in GetMobilesInRange(1))
		{
			if (mob.Alive && !mob.IsDeadBondedPet && mob.AccessLevel == AccessLevel.Player)
				Spells.SpellHelper.Damage(TimeSpan.FromTicks(1), mob, mob, Utility.Dice(3, 15, 0));
		}
	}

	public StoneFaceTrap(Serial serial) : base(serial)
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
		Breathing = false;
	}
}

public class StoneFaceTrapNoDamage : StoneFaceTrap
{
	[Constructable]
	public StoneFaceTrapNoDamage()
	{
	}

	public StoneFaceTrapNoDamage(Serial serial) : base(serial)
	{
	}

	public override void TriggerDamage()
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
