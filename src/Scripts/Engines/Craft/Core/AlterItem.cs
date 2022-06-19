using Server.Engines.VeteranRewards;
using Server.Items;
using Server.Targeting;
using System;
using System.Linq;

namespace Server.Engines.Craft;

[AttributeUsage(AttributeTargets.Class)]
public class AlterableAttribute : Attribute
{
	public Type CraftSystem { get; }
	public Type AlteredType { get; }
	public bool Inherit { get; }

	public AlterableAttribute(Type craftSystem, Type alteredType, bool inherit = false)
	{
		CraftSystem = craftSystem;
		AlteredType = alteredType;
		Inherit = inherit;
	}

	/// <summary>
	/// this enables any craftable item where their parent class can be altered, can be altered too.
	/// This is mainly for the ML craftable artifacts.
	/// </summary>
	/// <returns></returns>
	public bool CheckInherit(Type original)
	{
		if (Inherit)
		{
			return true;
		}

		var system = CraftContext.Systems.FirstOrDefault(sys => sys.GetType() == CraftSystem);

		return system?.CraftItems.SearchFor(original) != null;
	}
}

public class AlterItem
{
	public static void BeginTarget(Mobile from, CraftSystem system, ITool tool)
	{
		from.Target = new AlterItemTarget(system, tool);
		from.SendLocalizedMessage(1094730); //Target the item to altar
	}

	public static void BeginTarget(Mobile from, CraftSystem system, Item contract)
	{
		from.Target = new AlterItemTarget(system, contract);
		from.SendLocalizedMessage(1094730); //Target the item to altar
	}
}

public class AlterItemTarget : Target
{
	private readonly CraftSystem _mSystem;
	private readonly ITool _mTool;
	private readonly Item _mContract;

	public AlterItemTarget(CraftSystem system, Item contract)
		: base(2, false, TargetFlags.None)
	{
		_mSystem = system;
		_mContract = contract;
	}

	public AlterItemTarget(CraftSystem system, ITool tool)
		: base(1, false, TargetFlags.None)
	{
		_mSystem = system;
		_mTool = tool;
	}

	private static AlterableAttribute GetAlterableAttribute(object o, bool inherit)
	{
		Type t = o.GetType();

		object[] attrs = t.GetCustomAttributes(typeof(AlterableAttribute), inherit);

		if (attrs.Length > 0)
		{
			if (attrs[0] is AlterableAttribute attr && (!inherit || attr.CheckInherit(t)))
			{
				return attr;
			}
		}

		return null;
	}

	protected override void OnTarget(Mobile from, object o)
	{
		int number = -1;

		SkillName skill = _mSystem.MainSkill;
		double value = from.Skills[skill].Value;

		var alterInfo = GetAlterableAttribute(o, false) ?? GetAlterableAttribute(o, true);

		if (o is not Item origItem || !origItem.IsChildOf(from.Backpack))
		{
			number = 1094729; // The item must be in your backpack for you to alter it.
		}
		else if (origItem is BlankScroll)
		{
			if (_mContract == null)
			{
				if (value >= 100.0)
				{
					Item contract = null;

					if (skill == SkillName.Blacksmith)
					{
						contract = new AlterContract(RepairSkillType.Smithing, from);
					}
					else if (skill == SkillName.Carpentry)
					{
						contract = new AlterContract(RepairSkillType.Carpentry, from);
					}
					else if (skill == SkillName.Tailoring)
					{
						contract = new AlterContract(RepairSkillType.Tailoring, from);
					}
					else if (skill == SkillName.Tinkering)
					{
						contract = new AlterContract(RepairSkillType.Tinkering, from);
					}

					if (contract != null)
					{
						from.AddToBackpack(contract);

						number = 1044154; // You create the item.

						// Consume a blank scroll
						origItem.Consume();
					}
				}
				else
				{
					number = 1111869; // You must be at least grandmaster level to create an alter service contract.
				}
			}
			else
			{
				number = 1094728; // You may not alter that item.
			}
		}
		else if (alterInfo == null)
		{
			number = 1094728; // You may not alter that item.
		}
		else if (!IsAlterable(origItem))
		{
			number = 1094728; // You may not alter that item.
		}
		else if (alterInfo.CraftSystem != _mSystem.GetType())
		{
			number = _mTool != null ? 1094728 : 1094793;// You may not alter that item.// You cannot alter that item with this type of alter contract.
		}
		else if (_mContract == null && value < 100.0)
		{
			number = 1111870; // You must be at least grandmaster level to alter an item.
		}
		else if (origItem is BaseWeapon {EnchantedWeilder: { }})
		{
			number = 1111849; // You cannot alter an item that is currently enchanted.
		}
		else
		{
			if (Activator.CreateInstance(alterInfo.AlteredType) is not Item newitem)
			{
				return;
			}

			if (origItem is BaseWeapon weapon1 && newitem is BaseWeapon weapon2)
			{
				weapon2.Slayer = weapon1.Slayer;
				weapon2.Slayer2 = weapon1.Slayer2;
				weapon2.Slayer3 = weapon1.Slayer3;
				weapon2.Resource = weapon1.Resource;

				if (weapon1.PlayerConstructed)
				{
					weapon2.PlayerConstructed = true;
					weapon2.Crafter = weapon1.Crafter;
					weapon2.Quality = weapon1.Quality;
				}
				weapon2.Altered = true;
			}
			else if (origItem is BaseArmor armor && newitem is BaseArmor armor1)
			{
				if (armor.PlayerConstructed)
				{
					armor1.PlayerConstructed = true;
					armor1.Crafter = armor.Crafter;
					armor1.Quality = armor.Quality;
				}
				armor1.Resource = armor.Resource;
				armor1.PhysicalBonus = armor.PhysicalBonus;
				armor1.FireBonus = armor.FireBonus;
				armor1.ColdBonus = armor.ColdBonus;
				armor1.PoisonBonus = armor.PoisonBonus;
				armor1.EnergyBonus = armor.EnergyBonus;
				armor1.Altered = true;
			}
			else if (origItem is BaseClothing clothing && newitem is BaseClothing clothing1)
			{
				if (clothing.PlayerConstructed)
				{
					clothing1.PlayerConstructed = true;
					clothing1.Crafter = clothing.Crafter;
					clothing1.Quality = clothing.Quality;
				}
				clothing1.Altered = true;
			}
			else if (origItem is BaseClothing clothing2 && newitem is BaseArmor armor2)
			{
				if (clothing2.PlayerConstructed)
				{
					var qual = (int)clothing2.Quality;
					armor2.PlayerConstructed = true;
					armor2.Crafter = clothing2.Crafter;
					armor2.Quality = (ItemQuality)qual;
				}
				armor2.Altered = true;
			}
			else if (origItem is BaseQuiver && newitem is BaseArmor armor3)
			{
				/*BaseQuiver oldquiver = (BaseQuiver)origItem;
				BaseArmor newarmor = (BaseArmor)newitem;*/

				armor3.Altered = true;
			}
			else
			{
				return;
			}

			if (origItem.Name != null)
			{
				newitem.Name = origItem.Name;
			}

			AlterResists(newitem, origItem);

			newitem.Hue = origItem.Hue;
			newitem.LootType = origItem.LootType;
			newitem.Insured = origItem.Insured;

			origItem.OnAfterDuped(newitem);
			newitem.Parent = null;

			if (origItem is IDurability durability && newitem is IDurability durability1)
			{
				durability1.MaxHitPoints = durability.MaxHitPoints;
				durability1.HitPoints = durability.HitPoints;
			}

			if (from.Backpack == null)
			{
				newitem.MoveToWorld(from.Location, from.Map);
			}
			else
			{
				from.Backpack.DropItem(newitem);
			}

			newitem.InvalidateProperties();

			if (_mContract != null)
			{
				_mContract.Delete();
			}

			origItem.Delete();

			//EventSink.InvokeAlterItem(new AlterItemEventArgs(from, m_Tool is Item ? (Item)m_Tool : m_Contract, origItem, newitem));

			number = 1094727; // You have altered the item.
		}

		if (_mTool != null)
		{
			from.SendGump(new CraftGump(from, _mSystem, _mTool, number));
		}
		else
		{
			from.SendLocalizedMessage(number);
		}
	}

	private static void AlterResists(Item newItem, Item oldItem)
	{
	}

	private static bool RetainsName(Item item)
	{
		if (item is BaseGlasses or ElvenGlasses || item.IsArtifact)
		{
			return true;
		}

		if (item is IArtifact {ArtifactRarity: > 0})
		{
			return true;
		}

		return item.LabelNumber is >= 1073505 and <= 1073552 or >= 1073111 and <= 1075040;
	}


	private static readonly Type[] ArmorType =
	{
		typeof(RingmailGloves),    typeof(RingmailGlovesOfMining),
		typeof(PlateGloves),   typeof(LeatherGloves)
	};

	private static bool IsAlterable(IEntity item)
	{
		switch (item)
		{
			/*|| weapon.NegativeAttributes.Antique != 0*/
			case BaseWeapon weapon when weapon.SetId != SetItem.None || !weapon.CanAlter:
				return false;
			case BaseWeapon {RequiredRace: { }} weapon when weapon.RequiredRace == Race.Gargoyle && !weapon.IsArtifact:
			/*|| armor.NegativeAttributes.Antique != 0*/
			case BaseArmor armor when armor.SetId != SetItem.None || !armor.CanAlter:
				return false;
			case BaseArmor {RequiredRace: { }} armor when armor.RequiredRace == Race.Gargoyle && !armor.IsArtifact:
				return false;
			/*
		    if (armor is RingmailGlovesOfMining && armor.Resource > CraftResource.Iron)
		    {
		        return false;
		    }
		    */
			case BaseArmor armor when ArmorType.Any(t => t == armor.GetType()) && armor.Resource > CraftResource.Iron:
			/*|| cloth.NegativeAttributes.Antique != 0*/
			case BaseClothing cloth when cloth.SetId != SetItem.None || !cloth.CanAlter:
				return false;
			case BaseClothing {RequiredRace: { }} cloth when cloth.RequiredRace == Race.Gargoyle && !cloth.IsArtifact:
			case BaseQuiver quiver when quiver.SetId != SetItem.None || !quiver.CanAlter:
				return false;
		}

		return item is not IRewardItem;
	}
}
