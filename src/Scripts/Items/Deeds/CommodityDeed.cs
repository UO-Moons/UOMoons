using Server.Targeting;
using System;
using System.Linq;

namespace Server.Items;

public interface ICommodity
{
	TextDefinition Description { get; }
	bool IsDeedable { get; }
}

public static class CommodityDeedExtensions
{
	public static int GetAmount(this Container cont, Type type, bool recurse)
	{
		int amount = cont.GetAmount(type, recurse);

		var deeds = cont.FindItemsByType(typeof(CommodityDeed), recurse);
		amount += (from CommodityDeed deed in deeds where deed.Commodity != null where deed.Commodity.GetType() == type select deed.Commodity.Amount).Sum();

		return amount;
	}

	public static int GetAmount(this Container cont, Type[] types, bool recurse)
	{
		int amount = cont.GetAmount(types, recurse);

		var deeds = cont.FindItemsByType(typeof(CommodityDeed), recurse);
		amount += (from CommodityDeed deed in deeds where deed.Commodity != null where types.Any(type => deed.Commodity.GetType() == type) select deed.Commodity.Amount).Sum();

		return amount;
	}

	public static void ConsumeTotal(this Container cont, Type type, int amount, bool recurse, bool includeDeeds)
	{
		int left = amount;

		var items = cont.FindItemsByType(type, recurse);
		foreach (var item in items)
		{
			if (item.Amount <= left)
			{
				left -= item.Amount;
				item.Delete();
			}
			else
			{
				item.Amount -= left;
				left = 0;
				break;
			}
		}

		if (!includeDeeds) return;

		var deeds = cont.FindItemsByType(typeof(CommodityDeed), recurse);
		foreach (var item in deeds)
		{
			var deed = (CommodityDeed) item;
			if (deed.Commodity == null)
				continue;
			if (deed.Commodity.GetType() != type)
				continue;
			if (deed.Commodity.Amount <= left)
			{
				left -= deed.Commodity.Amount;
				deed.Delete();
			}
			else
			{
				deed.Commodity.Amount -= left;
				deed.InvalidateProperties();
				break;
			}
		}
	}
}

public class CommodityDeed : BaseItem
{
	[CommandProperty(AccessLevel.GameMaster)]
	public Item Commodity { get; private set; }

	public bool SetCommodity(Item item)
	{
		InvalidateProperties();

		if (Commodity != null || item is not ICommodity {IsDeedable: true}) return false;
		Commodity = item;
		Commodity.Internalize();
		InvalidateProperties();

		return true;

	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
		writer.Write(Commodity);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadInt();
		Commodity = reader.ReadItem();
	}

	private CommodityDeed(Item commodity) : base(0x14F0)
	{
		Weight = 1.0;
		Hue = 0x47;
		Commodity = commodity;
		LootType = LootType.Blessed;
	}

	[Constructable]
	public CommodityDeed() : this(null)
	{
	}

	public CommodityDeed(Serial serial) : base(serial)
	{
	}

	public override void OnDelete()
	{
		Commodity?.Delete();

		base.OnDelete();
	}

	public override int LabelNumber => Commodity == null ? 1047016 : 1047017;

	public override void GetProperties(ObjectPropertyList list)
	{
		base.GetProperties(list);

		if (Commodity != null)
		{
			var args = Commodity.Name == null ? $"#{(Commodity is ICommodity commodity ? commodity.Description : Commodity.LabelNumber)}\t{Commodity.Amount}" : $"{Commodity.Name}\t{Commodity.Amount}";

			list.Add(1060658, args); // ~1_val~: ~2_val~
		}
		else
		{
			list.Add(1060748); // unfilled
		}
	}

	public override void OnSingleClick(Mobile from)
	{
		base.OnSingleClick(from);

		if (Commodity == null) return;
		var args = Commodity.Name == null ? $"#{(Commodity is ICommodity commodity ? commodity.Description : Commodity.LabelNumber)}\t{Commodity.Amount}" : $"{Commodity.Name}\t{Commodity.Amount}";

		LabelTo(from, 1060658, args); // ~1_val~: ~2_val~
	}

	public override void OnDoubleClick(Mobile from)
	{
		int number;

		BankBox box = from.FindBankNoCreate();
		CommodityDeedBox cox = CommodityDeedBox.Find(this);

		// Veteran Rewards mods
		if (Commodity != null)
		{
			if (box != null && IsChildOf(box))
			{
				number = 1047031; // The commodity has been redeemed.

				box.DropItem(Commodity);

				Commodity = null;
				Delete();
			}
			else if (cox != null)
			{
				if (cox.IsSecure)
				{
					number = 1047031; // The commodity has been redeemed.

					cox.DropItem(Commodity);

					Commodity = null;
					Delete();
				}
				else
					number = 1080525; // The commodity deed box must be secured before you can use it.
			}
			else
			{
				number = Core.ML ? 1080526 : 1047024;
			}
		}
		else if (cox is {IsSecure: false})
		{
			number = 1080525; // The commodity deed box must be secured before you can use it.
		}
		else if ((box == null || !IsChildOf(box)) && cox == null)
		{
			number = Core.ML ? 1080526 : 1047026;
		}
		else
		{
			number = 1047029; // Target the commodity to fill this deed with.

			from.Target = new InternalTarget(this);
		}

		from.SendLocalizedMessage(number);
	}

	private class InternalTarget : Target
	{
		private readonly CommodityDeed _mDeed;

		public InternalTarget(CommodityDeed deed) : base(3, false, TargetFlags.None)
		{
			_mDeed = deed;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (_mDeed.Deleted)
				return;

			int number;

			if (_mDeed.Commodity != null)
			{
				number = 1047028; // The commodity deed has already been filled.
			}
			else if (targeted is Item item)
			{
				BankBox box = from.FindBankNoCreate();
				CommodityDeedBox cox = CommodityDeedBox.Find(_mDeed);

				// Veteran Rewards mods
				if (box != null && _mDeed.IsChildOf(box) && item.IsChildOf(box) ||
					cox is {IsSecure: true} && item.IsChildOf(cox))
				{
					if (_mDeed.SetCommodity(item))
					{
						_mDeed.Hue = 0x592;
						number = 1047030; // The commodity deed has been filled.
					}
					else
					{
						number = 1047027; // That is not a commodity the bankers will fill a commodity deed with.
					}
				}
				else
				{
					number = Core.ML ? 1080526 : 1047026;
				}
			}
			else
			{
				number = 1047027; // That is not a commodity the bankers will fill a commodity deed with.
			}

			from.SendLocalizedMessage(number);
		}
	}
}
