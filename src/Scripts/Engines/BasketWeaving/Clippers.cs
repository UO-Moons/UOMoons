using System.Collections.Generic;
using Server.Gumps;
using Server.Network;
using Server.ContextMenus;
using Server.Engines.Craft;
using Server.Engines.Plants;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Items;

[Flipable(0x0DFC, 0x0DFD)]
public class Clippers : BaseTool
{
	public override int LabelNumber => 1112117;  // clippers

	[Constructable]
	public Clippers()
		: base(0x0DFC)
	{
		Weight = 1.0;
		Hue = 1168;
	}

	[Constructable]
	public Clippers(int uses)
		: base(uses, 0x0DFC)
	{
		Weight = 1.0;
		Hue = 1168;
	}

	public override void GetProperties(ObjectPropertyList list)
	{
		base.GetProperties(list);

		//Makers mark not displayed on OSI
		if (Crafter != null)
		{
			list.Add(1050043, Crafter.TitleName); // crafted by ~1_NAME~
		}
	}

	public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
	{
		base.GetContextMenuEntries(from, list);
		AddContextMenuEntries(from, this, list);
	}

	public static void AddContextMenuEntries(Mobile from, Item item, List<ContextMenuEntry> list)
	{
		if (!item.IsChildOf(from.Backpack) && item.Parent != from)
			return;

		if (from is not PlayerMobile pm)
			return;

		list.Add(new ToggleClippings(pm, true, false, false, 1112282)); //Set to clip plants
		list.Add(new ToggleClippings(pm, false, true, false, 1112283)); //Set to cut reeds
		list.Add(new ToggleClippings(pm, false, false, true, 1150660)); //Set to cut topiaries
	}

	private class ToggleClippings : ContextMenuEntry
	{
		private readonly PlayerMobile _mMobile;
		private readonly bool _mValueclips;
		private readonly bool _mValuereeds;
		private readonly bool _mValuetopiaries;

		public ToggleClippings(PlayerMobile mobile, bool valueclips, bool valuereeds, bool valuetopiaries, int number)
			: base(number)
		{
			_mMobile = mobile;
			_mValueclips = valueclips;
			_mValuereeds = valuereeds;
			_mValuetopiaries = valuetopiaries;
		}

		public override void OnClick()
		{
			bool oldValueclips = _mMobile.ToggleCutClippings;
			bool oldValuereeds = _mMobile.ToggleCutReeds;
			bool oldValuetopiaries = _mMobile.ToggleCutTopiaries;

			if (_mValueclips)
			{
				if (oldValueclips)
				{
					_mMobile.ToggleCutClippings = true;
					_mMobile.ToggleCutReeds = false;
					_mMobile.ToggleCutTopiaries = false;
					_mMobile.SendLocalizedMessage(1112284); // You are already set to make plant clippings 
				}
				else
				{
					_mMobile.ToggleCutClippings = true;
					_mMobile.ToggleCutReeds = false;
					_mMobile.ToggleCutTopiaries = false;
					_mMobile.SendLocalizedMessage(1112285); // You are now set to make plant clippings
				}
			}
			else if (_mValuereeds)
			{
				if (oldValuereeds)
				{
					_mMobile.ToggleCutReeds = true;
					_mMobile.ToggleCutClippings = false;
					_mMobile.ToggleCutTopiaries = false;
					_mMobile.SendLocalizedMessage(1112287); // You are already set to cut reeds. 
				}
				else
				{
					_mMobile.ToggleCutReeds = true;
					_mMobile.ToggleCutClippings = false;
					_mMobile.ToggleCutTopiaries = false;
					_mMobile.SendLocalizedMessage(1112286); // You are now set to cut reeds.
				}
			}
			else if (_mValuetopiaries)
			{
				if (oldValuetopiaries)
				{
					_mMobile.ToggleCutTopiaries = true;
					_mMobile.ToggleCutReeds = false;
					_mMobile.ToggleCutClippings = false;
					_mMobile.SendLocalizedMessage(1150653); // You are already set to cut topiaries! 
				}
				else
				{
					_mMobile.ToggleCutTopiaries = true;
					_mMobile.ToggleCutReeds = false;
					_mMobile.ToggleCutClippings = false;
					_mMobile.SendLocalizedMessage(1150652); // You are now set to cut topiaries.
				}
			}
		}
	}

	public Clippers(Serial serial)
		: base(serial)
	{ }

	public virtual PlantHue PlantHue => PlantHue.None;

	public override CraftSystem CraftSystem => DefTinkering.CraftSystem;

	public void ConsumeUse(Mobile from)
	{
		if (UsesRemaining > 1)
		{
			--UsesRemaining;
		}
		else
		{
			from?.SendLocalizedMessage(1112126); // Your clippers break as you use up the last charge..

			Delete();
		}
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (!IsChildOf(from.Backpack)) return;
		from.SendLocalizedMessage(1112118); // What plant do you wish to use these clippers on?
		from.Target = new InternalTarget(this);
	}

	private class InternalTarget : Target
	{
		private readonly Clippers _mItem;

		public InternalTarget(Clippers item)
			: base(2, false, TargetFlags.None)
		{
			_mItem = item;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (from is not PlayerMobile pm || _mItem == null || _mItem.Deleted)
			{
				return;
			}

			if (targeted is not PlantItem {PlantStatus: PlantStatus.DecorativePlant} plant)
			{
				from.SendLocalizedMessage(1112119); // You may only use these clippers on decorative plants.
				return;
			}

			if (pm.ToggleCutClippings)
			{
				from.PlaySound(0x248);
				from.AddToBackpack(
					new PlantClippings
					{
						Hue = plant.Hue,
						PlantHue = plant.PlantHue
					});
				plant.Delete();
				_mItem.ConsumeUse(from);
			}
			else if (pm.ToggleCutReeds)
			{
				from.PlaySound(0x248);
				from.AddToBackpack(
					new DryReeds
					{
						Hue = plant.Hue,
						PlantHue = plant.PlantHue
					});
				plant.Delete();
				_mItem.ConsumeUse(from);
			}
			else if (pm.ToggleCutTopiaries)
			{
				if (plant.PlantType != PlantType.HedgeTall && plant.PlantType != PlantType.HedgeShort &&
				    plant.PlantType != PlantType.JuniperBush) return;
				from.CloseGump(typeof(TopiaryGump));
				from.SendGump(new TopiaryGump(plant, _mItem));
			}
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

public class TopiaryGump : Gump
{
	private readonly PlantItem _mPlant;
	private readonly Clippers _mClippers;

	public TopiaryGump(PlantItem plant, Clippers clippers) : base(0, 0)
	{
		_mPlant = plant;
		_mClippers = clippers;

		AddPage(0);

		AddBackground(50, 89, 508, 195, 2600);

		AddLabel(103, 114, 0, @"Choose a Topiary:");

		AddButton(92, 155, 1209, 1210, 1, GumpButtonType.Reply, 0);
		AddItem(75, 178, 18713);

		AddButton(133, 155, 1209, 1210, 2, GumpButtonType.Reply, 0);
		AddItem(119, 178, 18714);

		AddButton(177, 155, 1209, 1210, 3, GumpButtonType.Reply, 0);
		AddItem(165, 182, 18715);

		AddButton(217, 155, 1209, 1210, 4, GumpButtonType.Reply, 0);
		AddItem(205, 182, 18736);

		AddButton(267, 155, 1209, 1210, 5, GumpButtonType.Reply, 0);
		AddItem(220, 133, 18813);

		AddButton(333, 155, 1209, 1210, 6, GumpButtonType.Reply, 0);
		AddItem(272, 133, 18814);

		AddButton(388, 155, 1209, 1210, 7, GumpButtonType.Reply, 0);
		AddItem(374, 178, 18784);

		AddButton(426, 155, 1209, 1210, 8, GumpButtonType.Reply, 0);
		AddItem(413, 175, 18713);

		AddButton(480, 155, 1209, 1210, 9, GumpButtonType.Reply, 0);
		AddItem(463, 176, 19369);
	}

	public override void OnResponse(NetState sender, RelayInfo info)
	{
		Mobile from = sender.Mobile;

		switch (info.ButtonID)
		{
			case 0:
			{
				break;
			}
			case 1:
			{
				from.PlaySound(0x248);
				_mPlant.ItemId = 18713;
				_mClippers.ConsumeUse(from);
				break;
			}
			case 2:
			{
				from.PlaySound(0x248);
				_mPlant.ItemId = 18714;
				_mClippers.ConsumeUse(from);
				break;
			}
			case 3:
			{
				from.PlaySound(0x248);
				_mPlant.ItemId = 18715;
				_mClippers.ConsumeUse(from);
				break;
			}
			case 4:
			{
				from.PlaySound(0x248);
				_mPlant.ItemId = 18736;
				_mClippers.ConsumeUse(from);
				break;
			}
			case 5:
			{
				from.PlaySound(0x248);
				_mPlant.ItemId = 18813;
				_mClippers.ConsumeUse(from);
				break;
			}
			case 6:
			{
				from.PlaySound(0x248);
				_mPlant.ItemId = 18814;
				_mClippers.ConsumeUse(from);
				break;
			}
			case 7:
			{
				from.PlaySound(0x248);
				_mPlant.ItemId = 18814;
				_mClippers.ConsumeUse(from);
				break;
			}
			case 8:
			{
				from.PlaySound(0x248);
				_mPlant.ItemId = 18713;
				_mClippers.ConsumeUse(from);
				break;
			}
			case 9:
			{
				from.PlaySound(0x248);
				_mPlant.ItemId = 19369;
				_mClippers.ConsumeUse(from);
				break;
			}
		}
	}
}
