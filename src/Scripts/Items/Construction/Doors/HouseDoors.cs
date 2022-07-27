using Server.ContextMenus;
using Server.Gumps;
using Server.Multis;
using System;
using System.Collections.Generic;

namespace Server.Items;

public class MetalHouseDoor : BaseHouseDoor
{
	[Constructable]
	public MetalHouseDoor(DoorFacing facing) : base(facing, 0x675 + 2 * (int)facing, 0x676 + 2 * (int)facing, 0xEC, 0xF3, GetOffset(facing))
	{
	}

	public MetalHouseDoor(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer) // Default Serialize method
	{
		base.Serialize(writer);

		writer.Write(0); // version
	}

	public override void Deserialize(GenericReader reader) // Default Deserialize method
	{
		base.Deserialize(reader);

		reader.ReadInt();
	}
}

public class DarkWoodHouseDoor : BaseHouseDoor
{
	[Constructable]
	public DarkWoodHouseDoor(DoorFacing facing) : base(facing, 0x6A5 + 2 * (int)facing, 0x6A6 + 2 * (int)facing, 0xEA, 0xF1, GetOffset(facing))
	{
	}

	public DarkWoodHouseDoor(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer) // Default Serialize method
	{
		base.Serialize(writer);

		writer.Write(0); // version
	}

	public override void Deserialize(GenericReader reader) // Default Deserialize method
	{
		base.Deserialize(reader);

		reader.ReadInt();
	}
}

public class GenericHouseDoor : BaseHouseDoor
{
	[Constructable]
	public GenericHouseDoor(DoorFacing facing, int baseItemId, int openedSound, int closedSound, bool autoAdjust = true)
		: base(facing, baseItemId + (autoAdjust ? 2 * (int)facing : 0), baseItemId + 1 + (autoAdjust ? 2 * (int)facing : 0), openedSound, closedSound, GetOffset(facing))
	{
	}

	public GenericHouseDoor(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer) // Default Serialize method
	{
		base.Serialize(writer);

		writer.Write(0); // version
	}

	public override void Deserialize(GenericReader reader) // Default Deserialize method
	{
		base.Deserialize(reader);

		reader.ReadInt();
	}
}

public abstract class BaseHouseDoor : BaseDoor, ISecurable
{
	[CommandProperty(AccessLevel.GameMaster)]
	public DoorFacing Facing { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public SecureLevel Level { get; set; }

	public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
	{
		base.GetContextMenuEntries(from, list);
		SetSecureLevelEntry.AddTo(from, this, list);
	}

	public BaseHouseDoor(DoorFacing facing, int closedId, int openedId, int openedSound, int closedSound, Point3D offset) : base(closedId, openedId, openedSound, closedSound, offset)
	{
		Facing = facing;
		Level = SecureLevel.Anyone;
	}

	private BaseHouse FindHouse()
	{
		var loc = Open ? new Point3D(X - Offset.X, Y - Offset.Y, Z - Offset.Z) : Location;

		return BaseHouse.FindHouseAt(loc, Map, 20);
	}

	private bool CheckAccess(Mobile m)
	{
		BaseHouse house = FindHouse();

		if (house == null)
			return false;

		if (!house.IsAosRules)
			return true;

		if (house.Public ? house.IsBanned(m) : !house.HasAccess(m))
			return false;

		return house.HasSecureAccess(m, Level);
	}

	public override void OnOpened(Mobile from)
	{
		BaseHouse house = FindHouse();

		if (house != null && house.IsFriend(from) && from.IsPlayer() && house.RefreshDecay())
		{
			from.SendLocalizedMessage(1043293); // Your house's age and contents have been refreshed.
		}

		if (!Core.AOS && house is { Public: true } && !house.IsFriend(from))
		{
			house.AddVisit(from);
		}
	}

	public override bool UseLocks()
	{
		BaseHouse house = FindHouse();

		return house == null || !house.IsAosRules;
	}

	public override void Use(Mobile from)
	{
		if (!CheckAccess(from))
			from.SendLocalizedMessage(1061637); // You are not allowed to access this.
		else
			base.Use(from);
	}

	public BaseHouseDoor(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version

		writer.Write((int)Level);

		writer.Write((int)Facing);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		int version = reader.ReadInt();

		switch (version)
		{
			case 0:
			{
				Level = (SecureLevel)reader.ReadInt();

				Facing = (DoorFacing)reader.ReadInt();
				break;
			}
		}
	}

	public override bool IsInside(Mobile from)
	{
		int x, y, w, h;

		const int r = 2;
		const int bs = r * 2 + 1;
		const int ss = r + 1;

		switch (Facing)
		{
			case DoorFacing.WestCw:
			case DoorFacing.EastCcw: x = -r; y = -r; w = bs; h = ss; break;

			case DoorFacing.EastCw:
			case DoorFacing.WestCcw: x = -r; y = 0; w = bs; h = ss; break;

			case DoorFacing.SouthCw:
			case DoorFacing.NorthCcw: x = -r; y = -r; w = ss; h = bs; break;

			case DoorFacing.NorthCw:
			case DoorFacing.SouthCcw: x = 0; y = -r; w = ss; h = bs; break;

			//No way to test the 'insideness' of SE Sliding doors on OSI, so leaving them default to false until furthur information gained

			default: return false;
		}

		int rx = from.X - X;
		int ry = from.Y - Y;
		int az = Math.Abs(from.Z - Z);

		return rx >= x && rx < x + w && ry >= y && ry < y + h && az <= 4;
	}
}
