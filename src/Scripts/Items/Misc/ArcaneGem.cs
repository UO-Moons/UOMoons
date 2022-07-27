using Server.Targeting;
using System.Collections.Generic;
using System.Linq;

namespace Server.Items;

public class ArcaneGem : BaseItem
{
	public override string DefaultName => "arcane gem";

	[Constructable]
	public ArcaneGem() : base(0x1EA7)
	{
		Stackable = Core.ML;
		Weight = 1.0;
	}

	public ArcaneGem(Serial serial) : base(serial)
	{
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (!IsChildOf(from.Backpack))
		{
			from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
		}
		else
		{
			from.BeginTarget(2, false, TargetFlags.None, OnTarget);
			from.SendMessage("What do you wish to use the gem on?");
		}
	}

	private static int GetChargesFor(Mobile m)
	{
		int v = (int)(m.Skills[SkillName.Tailoring].Value / 5);

		return v switch
		{
			< 16 => 16,
			> 24 => 24,
			_ => v
		};
	}

	public const int DefaultArcaneHue = 2117;

	private void OnTarget(Mobile from, object obj)
	{
		if (!IsChildOf(from.Backpack))
		{
			from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
			return;
		}

		if (obj is IArcaneEquip and Item item)
		{
			CraftResource resource = item switch
			{
				BaseClothing clothing => clothing.Resource,
				BaseArmor armor => armor.Resource,
				// Sanity, weapons cannot receive gems...
				BaseWeapon weapon => weapon.Resource,
				_ => CraftResource.None
			};

			IArcaneEquip eq = (IArcaneEquip)item;

			if (!item.IsChildOf(from.Backpack))
			{
				from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
				return;
			}

			if (item.LootType == LootType.Blessed)
			{
				from.SendMessage("You can only use this on exceptionally crafted robes, thigh boots, cloaks, or leather gloves.");
				return;
			}

			if (resource != CraftResource.None && resource != CraftResource.RegularLeather)
			{
				from.SendLocalizedMessage(1049690); // Arcane gems can not be used on that type of leather.
				return;
			}

			int charges = GetChargesFor(from);

			if (eq.IsArcane)
			{
				if (eq.CurArcaneCharges >= eq.MaxArcaneCharges)
				{
					from.SendMessage("That item is already fully charged.");
				}
				else
				{
					if (eq.CurArcaneCharges <= 0)
						item.Hue = DefaultArcaneHue;

					if ((eq.CurArcaneCharges + charges) > eq.MaxArcaneCharges)
						eq.CurArcaneCharges = eq.MaxArcaneCharges;
					else
						eq.CurArcaneCharges += charges;

					from.SendMessage("You recharge the item.");
					if (Amount <= 1)
						Delete();
					else Amount--;
				}
			}
			else if (from.Skills[SkillName.Tailoring].Value >= 80.0)
			{
				bool isExceptional = false;

				if (item is BaseEquipment equipment)
				{
					isExceptional = equipment.Quality == ItemQuality.Exceptional;
				}

				if (isExceptional)
				{
					switch (item)
					{
						case BaseClothing clothing:
							clothing.Quality = ItemQuality.Normal;
							clothing.Crafter = from;
							break;
						case BaseArmor armor:
							armor.Quality = ItemQuality.Normal;
							armor.Crafter = from;
							armor.PhysicalBonus = armor.FireBonus = armor.ColdBonus = armor.PoisonBonus = armor.EnergyBonus = 0; // Is there a method to remove bonuses?
							break;
						// Sanity, weapons cannot receive gems...
						case BaseWeapon weapon:
							weapon.Quality = ItemQuality.Normal;
							weapon.Crafter = from;
							break;
					}

					eq.CurArcaneCharges = eq.MaxArcaneCharges = charges;

					item.Hue = DefaultArcaneHue;

					from.SendMessage("You enhance the item with your gem.");
					if (Amount <= 1)
						Delete();
					else Amount--;
				}
				else
				{
					from.SendMessage("Only exceptional items can be enhanced with the gem.");
				}
			}
			else
			{
				from.SendMessage("You do not have enough skill in tailoring to enhance the item.");
			}
		}
		else
		{
			from.SendMessage("You can only use this on exceptionally crafted robes, thigh boots, cloaks, or leather gloves.");
		}
	}

	public static bool ConsumeCharges(Mobile from, int amount)
	{
		List<Item> items = from.Items;
		int avail = items.OfType<IArcaneEquip>().Where(eq => eq.IsArcane).Sum(eq => eq.CurArcaneCharges);

		if (avail < amount)
			return false;

		for (int i = 0; i < items.Count; ++i)
		{
			Item obj = items[i];

			if (obj is not IArcaneEquip { IsArcane: true } eq)
				continue;

			if (eq.CurArcaneCharges > amount)
			{
				eq.CurArcaneCharges -= amount;
				break;
			}

			amount -= eq.CurArcaneCharges;
			eq.CurArcaneCharges = 0;
		}

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
