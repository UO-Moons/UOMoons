using Server.Multis;

namespace Server.Items;

public enum FencingType
{
	Arch,
	NwCornerPiece,
	SouthFacingPieces,
	EastFacingPieces,
	GateSouth,
	GateEast,
	CornerPiece
}

[Furniture]
public class DecorativeStableFencing : BaseItem, IFlipable, IDyable
{
	public override int LabelNumber => m_IDs[(int)_type][0];

	private static readonly int[][] m_IDs =
	{
		new[] { 1126213, 42189, 42190 },    // Arch
		new[] { 1126197, 42171 },           // NWCornerPiece
		new[] { 1126197, 42172, 42173 },    // SouthFacingPieces
		new[] { 1126197, 42173, 42172 },    // EastFacingPieces
		new[] { 1126211, 42176 },           // GateSouth
		new[] { 1126211, 42187 },           // GateEast
		new[] { 1126197, 42174 }            // CornerPiece
	};

	private FencingType _type;

	[CommandProperty(AccessLevel.GameMaster)]
	public bool CanFlip => m_IDs[(int)_type].Length > 2;

	public bool Dye(Mobile from, DyeTub sender)
	{
		if (Deleted)
			return false;

		Hue = sender.DyedHue;

		return true;
	}

	[Constructable]
	public DecorativeStableFencing(FencingType type)
		: base(m_IDs[(int)type][1])
	{
		LootType = LootType.Blessed;
		_type = type;
	}

	public void OnFlip(Mobile from)
	{
		var list = m_IDs[(int)_type];

		if (CanFlip && list.Length > 2)
		{
			for (var i = 1; i < list.Length; i++)
			{
				var id = list[i];

				if (ItemId == id)
				{
					if (i >= list.Length - 1)
					{
						ItemId = list[1];
						break;
					}

					ItemId = list[i + 1];
					break;
				}
			}
		}
	}

	public override void GetProperties(ObjectPropertyList list)
	{
		base.GetProperties(list);

		list.Add(1153494); // House Only
	}

	public override bool DropToWorld(Mobile from, Point3D p)
	{
		var h = BaseHouse.FindHouseAt(p, from.Map, ItemData.Height);

		if (h != null)
		{
			return base.DropToWorld(from, p);
		}

		if (from.Backpack == null || !from.Backpack.TryDropItem(from, this, false))
		{
			Delete();
		}

		return false;
	}

	public override bool DropToMobile(Mobile from, Mobile target, Point3D p)
	{
		return target.Backpack is not PackAnimalsBackpack && base.DropToMobile(from, target, p);
	}

	public override bool OnDroppedInto(Mobile from, Container target, Point3D p)
	{
		if (target.RootParent == null)
		{
			var h = BaseHouse.FindHouseAt(target.Location, target.Map, ItemData.Height);

			if (h == null || target is PackAnimalsBackpack)
			{
				return false;
			}
		}

		return base.OnDroppedInto(from, target, p);
	}

	public override bool OnDroppedOnto(Mobile from, Item target)
	{
		if (target.RootParent == null)
		{
			var h = BaseHouse.FindHouseAt(target.Location, target.Map, ItemData.Height);

			if (h == null || target is PackAnimalsBackpack)
			{
				return false;
			}
		}

		return base.OnDroppedOnto(from, target);
	}

	public DecorativeStableFencing(Serial serial)
		: base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);

		writer.Write((int)_type);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadInt();

		_type = (FencingType)reader.ReadInt();
	}
}
