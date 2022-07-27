using Server.Multis;
using Server.Network;

namespace Server.Items;

public class WineRack : LockableContainer, IFlipable, IDyable
{
	public override int LabelNumber => 1126367; // wine rack

	public override int DefaultGumpID => 0x44;

	public virtual int SouthId => 0xA568;
	public virtual int SouthEmptyId => 0xA567;
	public virtual int EastId => 0xA56A;
	public virtual int EastEmptyId => 0xA569;

	public bool IsEmpty => Items.Count == 0;

	public bool Dye(Mobile from, DyeTub sender)
	{
		if (Deleted)
			return false;

		Hue = sender.DyedHue;

		return true;
	}

	[CommandProperty(AccessLevel.Decorator)]
	public override int ItemId
	{
		get { return base.ItemId; }
		set
		{
			base.ItemId = value;

			CheckWineRack();
		}
	}

	public void CheckWineRack()
	{
		if (IsEmpty)
		{
			if (ItemId == SouthId)
			{
				base.ItemId = SouthEmptyId;
			}
			else if (ItemId == EastId)
			{
				base.ItemId = EastEmptyId;
			}
		}
		else
		{
			if (ItemId == SouthEmptyId)
			{
				base.ItemId = SouthId;
			}
			else if (ItemId == EastEmptyId)
			{
				base.ItemId = EastId;
			}
		}
	}

	[Constructable]
	public WineRack()
		: this(0xA567)
	{
	}

	[Constructable]
	public WineRack(int id)
		: base(id)
	{
	}

	public virtual void OnFlip(Mobile from)
	{
		if (ItemId == SouthId)
		{
			base.ItemId = EastId;
		}
		else if (ItemId == EastId)
		{
			base.ItemId = SouthId;
		}
		else if (ItemId == SouthEmptyId)
		{
			base.ItemId = EastEmptyId;
		}
		else if (ItemId == EastEmptyId)
		{
			base.ItemId = SouthEmptyId;
		}
	}

	public override bool DisplaysContent => false;

	public override void GetProperties(ObjectPropertyList list)
	{
		base.GetProperties(list);

		list.Add(1072241, "{0}\t{1}\t{2}\t{3}", TotalItems, MaxItems, TotalWeight, MaxWeight);
		// Contents: ~1_COUNT~/~2_MAXCOUNT~ items, ~3_WEIGHT~/~4_MAXWEIGHT~ stones
	}

	public override bool OnDragDropInto(Mobile from, Item item, Point3D p)
	{
		bool dropped = base.OnDragDropInto(from, item, p);
		BaseHouse house = BaseHouse.FindHouseAt(this);

		if (house != null && dropped)
		{
			OnItemDropped(from, item, house);
		}

		return dropped;
	}

	public override bool TryDropItem(Mobile from, Item dropped, bool sendFullMessage)
	{
		if (!CheckHold(from, dropped, true, true, true))
		{
			return false;
		}

		BaseHouse house = BaseHouse.FindHouseAt(this);

		if (house != null && IsLockedDown)
		{
			if (!house.CheckAccessibility(this, from))
			{
				PrivateOverheadMessage(MessageType.Regular, 0x21, 1061637, from.NetState); // You are not allowed to access this!
				from.SendLocalizedMessage(501727); // You cannot lock that down!
				return false;
			}
		}

		DropItem(dropped);

		if (house != null && !IsLockedDown)
		{
			OnItemDropped(from, dropped, house);
		}

		return true;
	}

	public override bool IsAccessibleTo(Mobile m)
	{
		return true;
	}

	public override bool CheckLift(Mobile from, Item item, ref LRReason reject)
	{
		if (item == this)
		{
			return base.CheckLift(from, item, ref reject);
		}

		BaseHouse house = BaseHouse.FindHouseAt(this);

		if (house != null && IsSecure)
		{
			SecureInfo secure = house.GetSecureInfoFor(this);

			return secure != null && house.HasSecureAccess(from, secure);
		}

		return base.CheckLift(from, item, ref reject);
	}

	public virtual void OnItemDropped(Mobile from, Item item, BaseHouse house)
	{
		SecureInfo secure = house.GetSecureInfoFor(this);

		if (secure != null && !house.HasSecureAccess(from, secure))
		{
			item.InvalidateProperties();
		}
	}

	public override void OnItemAdded(Item item)
	{
		base.OnItemAdded(item);

		CheckWineRack();
	}

	public override void OnItemRemoved(Item item)
	{
		base.OnItemRemoved(item);

		CheckWineRack();
	}

	public WineRack(Serial serial)
		: base(serial)
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