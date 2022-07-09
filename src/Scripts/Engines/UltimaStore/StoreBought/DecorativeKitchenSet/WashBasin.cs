using Server.Gumps;

namespace Server.Items;

public class WaterContainerComponent : AddonComponent, IWaterSource
{
	private int _quantity;
	public int EmptyItemId { get; set; }
	public int FullItemId { get; set; }
	public int MaxQuantity { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public virtual bool IsEmpty => (_quantity <= 0);

	[CommandProperty(AccessLevel.GameMaster)]
	public virtual bool IsFull => (_quantity >= MaxQuantity);

	[CommandProperty(AccessLevel.GameMaster)]
	public virtual int Quantity
	{
		get => _quantity;
		set
		{
			if (value != _quantity)
			{
				_quantity = value < 1 ? 0 : value > MaxQuantity ? MaxQuantity : value;

				ItemId = IsEmpty ? EmptyItemId : FullItemId;
			}
		}
	}

	[Constructable]
	public WaterContainerComponent(int itemId, int fullitemid, int maxquantity)
		: base(itemId)
	{
		MaxQuantity = maxquantity;
		EmptyItemId = itemId;
		FullItemId = fullitemid;
	}

	public WaterContainerComponent(Serial serial)
		: base(serial)
	{
	}

	public override void AddNameProperties(ObjectPropertyList list)
	{
		list.Add(1049522, "#1115912"); // a container with ~1_DRINK_NAME~
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);

		writer.Write(_quantity);
		writer.Write(EmptyItemId);
		writer.Write(FullItemId);
		writer.Write(MaxQuantity);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadInt();

		_quantity = reader.ReadInt();
		EmptyItemId = reader.ReadInt();
		FullItemId = reader.ReadInt();
		MaxQuantity = reader.ReadInt();
	}
}

public class WashBasinAddon : BaseAddon
{
	[Constructable]
	public WashBasinAddon(DirectionType type)
	{
		switch (type)
		{
			case DirectionType.South:
				AddComponent(new LocalizedAddonComponent(41646, 1125668), 1, 0, 0);
				AddComponent(new WaterContainerComponent(41645, 41647, 5), 0, 0, 0);
				AddComponent(new LocalizedAddonComponent(41644, 1125668), -1, 0, 0);
				break;
			case DirectionType.East:
				AddComponent(new LocalizedAddonComponent(41653, 1125668), 0, -1, 0);
				AddComponent(new WaterContainerComponent(41652, 41654, 5), 0, 0, 0);
				AddComponent(new LocalizedAddonComponent(41651, 1125668), 0, 1, 0);
				break;
		}
	}

	public WashBasinAddon(Serial serial)
		: base(serial)
	{
	}

	public override BaseAddonDeed Deed => new WashBasinDeed();

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

public class WashBasinDeed : BaseAddonDeed, IRewardOption
{
	public override int LabelNumber => 1158966;  // Wash Basin

	public override BaseAddon Addon
	{
		get
		{
			WashBasinAddon addon = new(_direction);

			return addon;
		}
	}

	private DirectionType _direction;

	[Constructable]
	public WashBasinDeed()
	{
		LootType = LootType.Blessed;
	}

	public WashBasinDeed(Serial serial)
		: base(serial)
	{
	}

	public void GetOptions(RewardOptionList list)
	{
		list.Add((int)DirectionType.South, 1075386); // South
		list.Add((int)DirectionType.East, 1075387); // East
	}

	public void OnOptionSelected(Mobile from, int choice)
	{
		_direction = (DirectionType)choice;

		if (!Deleted)
			base.OnDoubleClick(from);
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (IsChildOf(from.Backpack))
		{
			from.CloseGump(typeof(AddonOptionGump));
			from.SendGump(new AddonOptionGump(this, 1154194)); // Choose a Facing:
		}
		else
		{
			from.SendLocalizedMessage(1062334); // This item must be in your backpack to be used.
		}
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
