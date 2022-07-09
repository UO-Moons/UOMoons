using Server.Accounting;
using Server.ContextMenus;
using Server.Mobiles;
using Server.Multis;
using Server.Network;
using System.Collections.Generic;
using System.Linq;

namespace Server.Items;

public abstract class BaseContainer : Container, IEngravable
{
	public BaseContainer(int itemId)
		: base(itemId)
	{
	}

	public BaseContainer(Serial serial)
		: base(serial)
	{
	}

	public override int DefaultMaxWeight => IsSecure ? 0 : base.DefaultMaxWeight;

	private string _engravedText = string.Empty;

	[CommandProperty(AccessLevel.GameMaster)]
	public string EngravedText
	{
		get => _engravedText;
		set
		{
			_engravedText = value ?? string.Empty;

			InvalidateProperties();
		}
	}

	public override bool IsAccessibleTo(Mobile m)
	{
		return BaseHouse.CheckAccessible(m, this) && base.IsAccessibleTo(m);
	}

	public override bool CheckHold(Mobile m, Item item, bool message, bool checkItems, bool checkWeight, int plusItems, int plusWeight)
	{
		if (IsSecure && !BaseHouse.CheckHold(m, this, item, message, checkItems, checkWeight, plusItems, plusWeight))
			return false;

		return base.CheckHold(m, item, message, checkItems, checkWeight, plusItems, plusWeight);
	}

	public override bool CheckItemUse(Mobile from, Item item)
	{
		if (IsDecoContainer && item is BaseBook)
		{
			return true;
		}

		return base.CheckItemUse(from, item);
	}

	public virtual bool Security => true;

	public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
	{
		base.GetContextMenuEntries(from, list);

		if (Security)
		{
			SetSecureLevelEntry.AddTo(from, this, list);
		}
	}

	public override void GetChildContextMenuEntries(Mobile from, List<ContextMenuEntry> list, Item item)
	{
		if (IsLockedDown)
		{
			BaseHouse house = BaseHouse.FindHouseAt(this);

			if (house != null && house.IsOwner(from) && house.IsLockedDown(this) && house.IsLockedDown(item))
			{
				list.Add(new ReleaseEntry(from, item, house));
			}
		}
		else
		{
			base.GetChildContextMenuEntries(from, list, item);
		}
	}

	public virtual void DropItemStacked(Item dropped)
	{
		List<Item> list = Items;

		ItemFlags.SetTaken(dropped, true);

		if (list.Any(item => item is not Container && item.StackWith(null, dropped, false)))
		{
			return;
		}

		DropItem(dropped);
	}

	public override bool TryDropItem(Mobile from, Item dropped, bool sendFullMessage)
	{
		if (!CheckHold(from, dropped, sendFullMessage, !CheckStack(from, dropped)))
		{
			return false;
		}

		if (dropped.QuestItem && from.Backpack != this)
		{
			from.SendLocalizedMessage(1074769); // An item must be in your backpack (and not in a container within) to be toggled as a quest item.
			return false;
		}

		BaseHouse house = BaseHouse.FindHouseAt(this);

		if (house != null && house.IsLockedDown(this))
		{
			if (dropped is VendorRentalContract || (dropped is Container container && container.FindItemByType(typeof(VendorRentalContract)) != null))
			{
				from.SendLocalizedMessage(1062492); // You cannot place a rental contract in a locked down container.
				return false;
			}

			if (!house.LockDown(from, dropped, false))
			{
				return false;
			}
		}

		List<Item> list = Items;

		if (list.Any(item => item is not Container && item.StackWith(from, dropped, false)))
		{
			return true;
		}

		DropItem(dropped);

		ItemFlags.SetTaken(dropped, true);

		EventSink.InvokeContainerDroppedTo(new ContainerDroppedToEventArgs(from, this, dropped));

		return true;
	}

	public override bool OnDragDropInto(Mobile from, Item item, Point3D p)
	{
		if (!CheckHold(from, item, true, true))
		{
			return false;
		}

		if (item.QuestItem && from.Backpack != this)
		{
			from.SendLocalizedMessage(1074769); // An item must be in your backpack (and not in a container within) to be toggled as a quest item.
			return false;
		}

		BaseHouse house = BaseHouse.FindHouseAt(this);

		if (house != null && house.IsLockedDown(this))
		{
			if (item is VendorRentalContract || (item is Container container && container.FindItemByType(typeof(VendorRentalContract)) != null))
			{
				from.SendLocalizedMessage(1062492); // You cannot place a rental contract in a locked down container.
				return false;
			}

			if (!house.LockDown(from, item, false))
			{
				return false;
			}
		}

		item.Location = new Point3D(p.X, p.Y, 0);

		AddItem(item);

		from.SendSound(GetDroppedSound(item), GetWorldLocation());

		ItemFlags.SetTaken(item, true);

		EventSink.InvokeContainerDroppedTo(new ContainerDroppedToEventArgs(from, this, item));

		return true;
	}

	public override bool OnDroppedInto(Mobile from, Container target, Point3D p)
	{
		bool canDrop = base.OnDroppedInto(from, target, p);

		if (canDrop && target is BankBox box)
		{
			CheckBank(box, from);
		}

		return canDrop;
	}

	public override void UpdateTotal(Item sender, TotalType type, int delta)
	{
		base.UpdateTotal(sender, type, delta);

		if (type == TotalType.Weight && RootParent is Mobile mobile)
		{
			mobile.InvalidateProperties();
		}
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (from.IsStaff() || RootParent is PlayerVendor ||
		    (from.InRange(GetWorldLocation(), 2) && (Parent != null || (Z >= from.Z - 8 && Z <= from.Z + 16))))
		{
			Open(from);
		}
		else
		{
			from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
		}
	}

	public override void AddNameProperty(ObjectPropertyList list)
	{
		base.AddNameProperty(list);

		if (!string.IsNullOrEmpty(EngravedText))
		{
			list.Add(1072305, Utility.FixHtml(EngravedText)); // Engraved: ~1_INSCRIPTION~
		}
	}

	public override bool DropToWorld(Mobile m, Point3D p)
	{
		return base.DropToWorld(m, p);
	}

	public virtual void Open(Mobile from)
	{
		DisplayTo(from);
	}

	public void CheckBank(BankBox bank, Mobile from)
	{
		if (AccountGold.Enabled && bank.Owner == from && from.Account != null)
		{
			List<BankCheck> checks = new(Items.OfType<BankCheck>());

			foreach (BankCheck check in checks)
			{
				if (from.Account.DepositGold(check.Worth))
				{
					from.SendLocalizedMessage(1042672, true, check.Worth.ToString("#,0"));
					check.Delete();
				}
				else
				{
					from.AddToBackpack(check);
				}
			}

			checks.Clear();
			checks.TrimExcess();

			UpdateTotals();
		}
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(1000);
		writer.Write(_engravedText);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		var version = reader.PeekInt();
		switch (version)
		{
			case 1000:
				reader.ReadInt();
				_engravedText = reader.ReadString();
				break;
		}
	}
}

public class BaseCreatureBackpack : Backpack
{
	[Constructable]
	public BaseCreatureBackpack(string name)
	{
		Name = name;
		Layer = Layer.Backpack;
		Hue = 5;
		Weight = 3.0;
	}

	public override void AddNameProperty(ObjectPropertyList list)
	{
		if (Name != null)
			list.Add(1075257, Name); // Contents of ~1_PETNAME~'s pack.
		else
			base.AddNameProperty(list);
	}

	public override void OnItemRemoved(Item item)
	{
		if (Items.Count == 0)
			Delete();

		base.OnItemRemoved(item);
	}

	public override bool OnDragLift(Mobile from)
	{
		if (from.AccessLevel > AccessLevel.Player)
			return true;

		from.SendLocalizedMessage(500169); // You cannot pick that up.
		return false;
	}

	public override bool OnDragDropInto(Mobile from, Item item, Point3D p)
	{
		return false;
	}

	public override bool TryDropItem(Mobile from, Item dropped, bool sendFullMessage)
	{
		return false;
	}

	public BaseCreatureBackpack(Serial serial) : base(serial)
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

public class PackAnimalsBackpack : Backpack
{
	[Constructable]
	public PackAnimalsBackpack()
	{
		Layer = Layer.Backpack;
		Weight = 13.0;
	}

	public override bool CheckHold(Mobile m, Item item, bool message, bool checkItems, bool checkWeight, int plusItems, int plusWeight)
	{
		return base.CheckHold(m, item, false, checkItems, checkWeight, plusItems, plusWeight);
	}

	public override int DefaultMaxWeight => 1600;

	public override bool CheckContentDisplay(Mobile from)
	{
		object root = RootParent;

		if (root is BaseCreature {Controlled: true} creature && creature.ControlMaster == from)
			return true;

		return base.CheckContentDisplay(from);
	}

	public PackAnimalsBackpack(Serial serial) : base(serial)
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

public class Backpack : BaseContainer, IDyable
{
	[Constructable]
	public Backpack() : base(0xE75)
	{
		Layer = Layer.Backpack;
		Weight = 3.0;
	}

	public override int DefaultMaxWeight
	{
		get
		{
			if (Core.ML)
			{
				if (Parent is Mobile {Player: true} m && m.Backpack == this)
				{
					return 550;
				}

				return base.DefaultMaxWeight;
			}

			return base.DefaultMaxWeight;
		}
	}

	public Backpack(Serial serial) : base(serial)
	{
	}

	public bool Dye(Mobile from, DyeTub sender)
	{
		if (Deleted) return false;

		Hue = sender.DyedHue;

		return true;
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadInt();
	}
}

public class Pouch : TrapableContainer
{
	[Constructable]
	public Pouch() : base(0xE79)
	{
		Weight = 1.0;
	}

	public Pouch(Serial serial) : base(serial)
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

public abstract class BaseBagBall : BaseContainer, IDyable
{
	public BaseBagBall(int itemId) : base(itemId)
	{
		Weight = 1.0;
	}

	public BaseBagBall(Serial serial) : base(serial)
	{
	}

	public bool Dye(Mobile from, DyeTub sender)
	{
		if (Deleted)
			return false;

		Hue = sender.DyedHue;

		return true;
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

public class SmallBagBall : BaseBagBall
{
	[Constructable]
	public SmallBagBall() : base(0x2256)
	{
	}

	public SmallBagBall(Serial serial) : base(serial)
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

public class LargeBagBall : BaseBagBall
{
	[Constructable]
	public LargeBagBall() : base(0x2257)
	{
	}

	public LargeBagBall(Serial serial) : base(serial)
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

public class Bag : BaseContainer, IDyable
{
	[Constructable]
	public Bag() : base(0xE76)
	{
		Weight = 2.0;
	}

	public Bag(Serial serial) : base(serial)
	{
	}

	public bool Dye(Mobile from, DyeTub sender)
	{
		if (Deleted)
			return false;

		Hue = sender.DyedHue;

		return true;
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

public class Barrel : BaseContainer
{
	[Constructable]
	public Barrel() : base(0xE77)
	{
		Weight = 25.0;
	}

	public Barrel(Serial serial) : base(serial)
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

public class Keg : BaseContainer
{
	[Constructable]
	public Keg() : base(0xE7F)
	{
		Weight = 15.0;
	}

	public Keg(Serial serial) : base(serial)
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

[Furniture]
[Flipable(0x9AA, 0xE7D)]
public class WoodenBox : LockableContainer
{
	[Constructable]
	public WoodenBox() : base(0x9AA)
	{
		Weight = 4.0;
	}

	public WoodenBox(Serial serial) : base(serial)
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

[Furniture]
[Flipable(0x9A9, 0xE7E)]
public class SmallCrate : LockableContainer
{
	[Constructable]
	public SmallCrate() : base(0x9A9)
	{
		Weight = 2.0;
	}

	public SmallCrate(Serial serial) : base(serial)
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

[Furniture]
[Flipable(0xE3F, 0xE3E)]
public class MediumCrate : LockableContainer
{
	[Constructable]
	public MediumCrate() : base(0xE3F)
	{
		Weight = 2.0;
	}

	public MediumCrate(Serial serial) : base(serial)
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

[Furniture]
[Flipable(0xE3D, 0xE3C)]
public class LargeCrate : LockableContainer
{
	[Constructable]
	public LargeCrate() : base(0xE3D)
	{
		Weight = 1.0;
	}

	public LargeCrate(Serial serial) : base(serial)
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

[DynamicFliping]
[Flipable(0x9A8, 0xE80)]
public class MetalBox : LockableContainer
{
	[Constructable]
	public MetalBox() : base(0x9A8)
	{
	}

	public MetalBox(Serial serial) : base(serial)
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

[DynamicFliping]
[Flipable(0x9AB, 0xE7C)]
public class MetalChest : LockableContainer
{
	[Constructable]
	public MetalChest() : base(0x9AB)
	{
	}

	public MetalChest(Serial serial) : base(serial)
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

[DynamicFliping]
[Flipable(0xE41, 0xE40)]
public class MetalGoldenChest : LockableContainer
{
	[Constructable]
	public MetalGoldenChest() : base(0xE41)
	{
	}

	public MetalGoldenChest(Serial serial) : base(serial)
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

[Furniture]
[Flipable(0xe43, 0xe42)]
public class WoodenChest : LockableContainer
{
	[Constructable]
	public WoodenChest() : base(0xe43)
	{
		Weight = 2.0;
	}

	public WoodenChest(Serial serial) : base(serial)
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

[Furniture]
[Flipable(0x280B, 0x280C)]
public class PlainWoodenChest : LockableContainer
{
	[Constructable]
	public PlainWoodenChest() : base(0x280B)
	{
	}

	public PlainWoodenChest(Serial serial) : base(serial)
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

[Furniture]
[Flipable(0x280D, 0x280E)]
public class OrnateWoodenChest : LockableContainer
{
	[Constructable]
	public OrnateWoodenChest() : base(0x280D)
	{
	}

	public OrnateWoodenChest(Serial serial) : base(serial)
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

[Furniture]
[Flipable(0x280F, 0x2810)]
public class GildedWoodenChest : LockableContainer
{
	[Constructable]
	public GildedWoodenChest() : base(0x280F)
	{
	}

	public GildedWoodenChest(Serial serial) : base(serial)
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

[Furniture]
[Flipable(0x2811, 0x2812)]
public class WoodenFootLocker : LockableContainer
{
	[Constructable]
	public WoodenFootLocker() : base(0x2811)
	{
		GumpID = 0x10B;
	}

	public WoodenFootLocker(Serial serial) : base(serial)
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

[Furniture]
[Flipable(0x2813, 0x2814)]
public class FinishedWoodenChest : LockableContainer
{
	[Constructable]
	public FinishedWoodenChest() : base(0x2813)
	{
	}

	public FinishedWoodenChest(Serial serial) : base(serial)
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
