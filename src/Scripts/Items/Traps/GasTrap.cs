using Server.Network;
using System;

namespace Server.Items;

public enum GasTrapType
{
	NorthWall,
	WestWall,
	Floor
}

public class GasTrap : BaseTrap
{
	[CommandProperty(AccessLevel.GameMaster)]
	private Poison Poison { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	private GasTrapType Type
	{
		get
		{
			return ItemId switch
			{
				0x113C => GasTrapType.NorthWall,
				0x1147 => GasTrapType.WestWall,
				0x11A8 => GasTrapType.Floor,
				_ => GasTrapType.WestWall
			};
		}
	}

	private static int GetBaseId(GasTrapType type)
	{
		return type switch
		{
			GasTrapType.NorthWall => 0x113C,
			GasTrapType.WestWall => 0x1147,
			GasTrapType.Floor => 0x11A8,
			_ => 0
		};
	}

	[Constructable]
	public GasTrap() : this(GasTrapType.Floor)
	{
	}

	[Constructable]
	public GasTrap(GasTrapType type) : this(type, Poison.Lesser)
	{
	}

	[Constructable]
	public GasTrap(Poison poison) : this(GasTrapType.Floor, Poison.Lesser)
	{
		Poison = poison;
	}

	[Constructable]
	private GasTrap(GasTrapType type, Poison poison) : base(GetBaseId(type))
	{
		Poison = poison;
	}

	protected override bool PassivelyTriggered => false;
	protected override TimeSpan PassiveTriggerDelay => TimeSpan.Zero;
	protected override int PassiveTriggerRange => 0;
	protected override TimeSpan ResetDelay => TimeSpan.FromSeconds(0.0);

	protected override void OnTrigger(Mobile from)
	{
		if (Poison == null || !from.Player || !from.Alive || from.AccessLevel > AccessLevel.Player)
			return;

		Effects.SendLocationEffect(Location, Map, GetBaseId(Type) - 2, 16, 3, GetEffectHue(), 0);
		Effects.PlaySound(Location, Map, 0x231);

		from.ApplyPoison(from, Poison);

		from.LocalOverheadMessage(MessageType.Regular, 0x22, 500855); // You are enveloped by a noxious gas cloud!
	}

	public GasTrap(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
		Poison.Serialize(Poison, writer);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		var version = reader.ReadInt();
		Poison = version switch
		{
			0 => Poison.Deserialize(reader),
			_ => Poison
		};
	}
}
