using Server.ContextMenus;
using Server.Engines.Craft;
using Server.Network;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Items;

public class SalvageBag : Bag
{
	private bool m_Failure;

	public override int LabelNumber => 1079931;  // Salvage Bag

	[Constructable]
	public SalvageBag()
		: this(Utility.RandomBlueHue())
	{
	}

	[Constructable]
	private SalvageBag(int hue)
	{
		Weight = 2.0;
		Hue = hue;
		m_Failure = false;
	}

	public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
	{
		base.GetContextMenuEntries(from, list);

		if (!from.Alive)
			return;
		list.Add(new SalvageIngotsEntry(this, IsChildOf(from.Backpack) && Resmeltables()));
		list.Add(new SalvageClothEntry(this, IsChildOf(from.Backpack) && Scissorables()));
		list.Add(new SalvageAllEntry(this, IsChildOf(from.Backpack) && Resmeltables() && Scissorables()));
	}

	#region Checks
	private bool Resmeltables() //Where context menu checks for metal items and dragon barding deeds
	{
		foreach (var i in Items.Where(i => i is { Deleted: false }))
		{
			switch (i)
			{
				case BaseWeapon weapon when CraftResources.GetType(weapon.Resource) == CraftResourceType.Metal:
				case BaseArmor armor when CraftResources.GetType(armor.Resource) == CraftResourceType.Metal:
				case DragonBardingDeed:
					return true;
			}
		}

		return false;
	}

	private bool Scissorables() //Where context menu checks for Leather items and cloth items
	{
		foreach (var i in Items.Where(i => i is { Deleted: false }).OfType<IScissorable>())
		{
			switch (i)
			{
				case BaseClothing:
				case BaseArmor armor when CraftResources.GetType(armor.Resource) == CraftResourceType.Leather:
				case Cloth:
				case BoltOfCloth:
				case Hides:
				case BonePile:
					return true;
			}
		}

		return false;
	}
	#endregion

	#region Resmelt.cs
	private bool Resmelt(Mobile from, Item item, CraftResource resource)
	{
		try
		{
			if (CraftResources.GetType(resource) != CraftResourceType.Metal)
				return false;

			CraftResourceInfo info = CraftResources.GetInfo(resource);

			if (info == null || info.ResourceTypes.Length == 0)
				return false;

			CraftItem craftItem = DefBlacksmithy.CraftSystem.CraftItems.SearchFor(item.GetType());

			if (craftItem == null || craftItem.Resources.Count == 0)
				return false;

			CraftRes craftResource = craftItem.Resources.GetAt(0);

			if (craftResource.Amount < 2)
				return false; // Not enough metal to resmelt

			double difficulty = resource switch
			{
				CraftResource.DullCopper => 65.0,
				CraftResource.ShadowIron => 70.0,
				CraftResource.Copper => 75.0,
				CraftResource.Bronze => 80.0,
				CraftResource.Gold => 85.0,
				CraftResource.Agapite => 90.0,
				CraftResource.Verite => 95.0,
				CraftResource.Valorite => 99.0,
				_ => 0.0
			};

			Type resourceType = info.ResourceTypes[0];
			Item ingot = (Item)Activator.CreateInstance(resourceType);

			if (item is DragonBardingDeed or BaseArmor { PlayerConstructed: true } or BaseWeapon
			    {
				    PlayerConstructed: true
			    } or BaseClothing { PlayerConstructed: true })
			{
				double mining = from.Skills[SkillName.Mining].Value;
				if (mining > 100.0)
					mining = 100.0;
				double amount = (((4 + mining) * craftResource.Amount - 4) * 0.0068);
				if (amount < 2)
				{
					if (ingot != null) ingot.Amount = 2;
				}
				else if (ingot != null) ingot.Amount = (int)amount;
			}
			else
			{
				if (ingot != null) ingot.Amount = 2;
			}

			if (difficulty > from.Skills[SkillName.Mining].Value)
			{
				m_Failure = true;
				ingot?.Delete();
			}
			else
				item.Delete();

			from.AddToBackpack(ingot);

			from.PlaySound(0x2A);
			from.PlaySound(0x240);

			return true;
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}

		return false;
	}
	#endregion

	#region Salvaging
	private void SalvageIngots(Mobile from)
	{
		Item[] tools = from.Backpack.FindItemsByType(typeof(BaseTool));

		bool toolFound = false;
		foreach (Item tool in tools)
		{
			if (tool is BaseTool baseTool && baseTool.CraftSystem == DefBlacksmithy.CraftSystem)
				toolFound = true;
		}

		if (!toolFound)
		{
			from.SendLocalizedMessage(1079822); // You need a blacksmithing tool in order to salvage ingots.
			return;
		}

		DefBlacksmithy.CheckAnvilAndForge(from, 2, out _, out bool forge);

		if (!forge)
		{
			from.SendLocalizedMessage(1044265); // You must be near a forge.
			return;
		}

		int salvaged = 0;
		int notSalvaged = 0;

		Container sBag = this;

		List<Item> smeltables = sBag.FindItemsByType<Item>();

		for (int i = smeltables.Count - 1; i >= 0; i--)
		{
			Item item = smeltables[i];

			switch (item)
			{
				case BaseArmor armor when Resmelt(from, armor, armor.Resource):
					salvaged++;
					break;
				case BaseArmor:
					notSalvaged++;
					break;
				case BaseWeapon weapon when Resmelt(from, weapon, weapon.Resource):
					salvaged++;
					break;
				case BaseWeapon:
					notSalvaged++;
					break;
				case DragonBardingDeed deed when Resmelt(from, deed, deed.Resource):
					salvaged++;
					break;
				case DragonBardingDeed:
					notSalvaged++;
					break;
			}
		}
		if (m_Failure)
		{
			from.SendLocalizedMessage(1079975); // You failed to smelt some metal for lack of skill.
			m_Failure = false;
		}
		else
			from.SendLocalizedMessage(1079973, $"{salvaged}\t{salvaged + notSalvaged}"); // Salvaged: ~1_COUNT~/~2_NUM~ blacksmithed items
	}

	private void SalvageCloth(Mobile from)
	{
		if (from.Backpack.FindItemByType(typeof(Scissors)) is not Scissors scissors)
		{
			from.SendLocalizedMessage(1079823); // You need scissors in order to salvage cloth.
			return;
		}

		int salvaged = 0;
		int notSalvaged = 0;

		Container sBag = this;

		List<Item> scissorables = sBag.FindItemsByType<Item>();

		for (int i = scissorables.Count - 1; i >= 0; --i)
		{
			Item item = scissorables[i];

			if (item is IScissorable scissorable)
			{
				if (Scissors.CanScissor(from, scissorable) && scissorable.Scissor(from, scissors))
					++salvaged;
				else
					++notSalvaged;
			}
		}

		from.SendLocalizedMessage(1079974, $"{salvaged}\t{salvaged + notSalvaged}"); // Salvaged: ~1_COUNT~/~2_NUM~ tailored items

		foreach (Item i in FindItemsByType(typeof(Item), true))
		{
			if ((i is Leather) || (i is Cloth) || (i is SpinedLeather) || (i is HornedLeather) || (i is BarbedLeather) || (i is Bandage) || (i is Bone))
			{
				from.AddToBackpack(i);
			}
		}
	}

	private void SalvageAll(Mobile from)
	{
		SalvageIngots(from);

		SalvageCloth(from);
	}
	#endregion

	#region ContextMenuEntries
	private class SalvageAllEntry : ContextMenuEntry
	{
		private readonly SalvageBag m_Bag;

		public SalvageAllEntry(SalvageBag bag, bool enabled)
			: base(6276)
		{
			m_Bag = bag;

			if (!enabled)
				Flags |= CMEFlags.Disabled;
		}

		public override void OnClick()
		{
			if (m_Bag.Deleted)
				return;

			Mobile from = Owner.From;

			if (from.CheckAlive())
				m_Bag.SalvageAll(from);
		}
	}

	private class SalvageIngotsEntry : ContextMenuEntry
	{
		private readonly SalvageBag m_Bag;

		public SalvageIngotsEntry(SalvageBag bag, bool enabled)
			: base(6277)
		{
			m_Bag = bag;

			if (!enabled)
				Flags |= CMEFlags.Disabled;
		}

		public override void OnClick()
		{
			if (m_Bag.Deleted)
				return;

			Mobile from = Owner.From;

			if (from.CheckAlive())
				m_Bag.SalvageIngots(from);
		}
	}

	private class SalvageClothEntry : ContextMenuEntry
	{
		private readonly SalvageBag m_Bag;

		public SalvageClothEntry(SalvageBag bag, bool enabled)
			: base(6278)
		{
			m_Bag = bag;

			if (!enabled)
				Flags |= CMEFlags.Disabled;
		}

		public override void OnClick()
		{
			if (m_Bag.Deleted)
				return;

			Mobile from = Owner.From;

			if (from.CheckAlive())
				m_Bag.SalvageCloth(from);
		}
	}
	#endregion

	#region Serialization
	public SalvageBag(Serial serial)
		: base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.WriteEncodedInt(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadEncodedInt();
	}
	#endregion
}
