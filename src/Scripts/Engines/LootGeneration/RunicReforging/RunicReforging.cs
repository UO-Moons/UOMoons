using Server.Engines.Craft;
using Server.Gumps;
using Server.Mobiles;
using Server.SkillHandlers;
using Server.Targeting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Items;

public enum ReforgedPrefix
{
	None,
	Might,
	Mystic,
	Animated,
	Arcane,
	Exquisite,
	Vampiric,
	Invigorating,
	Fortified,
	Auspicious,
	Charmed,
	Vicious,
	Towering
}

public enum ReforgedSuffix
{
	None,
	Vitality,
	Sorcery,
	Haste,
	Wizadry,
	Quality,
	Vampire,
	Restoration,
	Defense,
	Fortune,
	Alchemy,
	Slaughter,
	Aegis,
	Blackthorn,
	Minax,
	Kotl,
	Khaldun,
	Doom,
	EnchantedOrigin,
	Fellowship
}

public enum ItemPower
{
	None,
	Minor,
	Lesser,
	Greater,
	Major,
	LesserArtifact,
	GreaterArtifact,
	MajorArtifact,
	LegendaryArtifact,
	ReforgedMinor,
	ReforgedLesser,
	ReforgedGreater,
	ReforgedMajor,
	ReforgedLegendary
}

public static class RunicReforging
{
	public static bool CanReforge(Mobile from, Item item, CraftSystem crsystem)
	{
		var allowableSpecial = m_AllowableTable.ContainsKey(item.GetType());

		var system = !allowableSpecial ? CraftSystem.GetSystem(item.GetType()) : m_AllowableTable[item.GetType()];

		var goodtogo = true;

		if (system == null)
		{
			from.SendLocalizedMessage(1152113); // You cannot reforge that item.
			goodtogo = false;
		}
		else if (system != crsystem)
		{
			from.SendLocalizedMessage(1152279); // You cannot re-forge that item with this tool.
			goodtogo = false;
		}
		else
		{
			var mods = GetTotalMods(item);
			var maxmods = item is JukaBow ||
			              item is BaseWeapon { DImodded: false } ||
			              (item is BaseArmor armor && armor.ArmorAttributes.MageArmor > 0 && BaseArmor.IsMageArmorType(armor)) ? 1 : 0;

			if (item is BaseWeapon weapon &&
			    (weapon.AosElementDamages[AosElementAttribute.Fire] > 0 ||
			     weapon.AosElementDamages[AosElementAttribute.Cold] > 0 ||
			     weapon.AosElementDamages[AosElementAttribute.Poison] > 0 ||
			     weapon.AosElementDamages[AosElementAttribute.Energy] > 0))
			{
				mods++;
			}

			if (mods > maxmods)
				goodtogo = false;
			else if (item is IResource resource && !CraftResources.IsStandard(resource.Resource))
				goodtogo = false;
			else if (item.LootType == LootType.Blessed || item.LootType == LootType.Newbied)
				goodtogo = false;
			else switch (item)
			{
				case BaseWeapon baseWeapon when Spells.Mysticism.EnchantSpell.IsUnderSpellEffects(from, baseWeapon):
				case BaseWeapon { FocusWeilder: { } }:
					goodtogo = false;
					break;
				default:
				{
					switch (allowableSpecial)
					{
						case false when item is IQuality quality && !quality.PlayerConstructed:
						case false when item is BaseClothing && !(item is BaseHat):
						case false when item is BaseJewel:
							goodtogo = false;
							break;
						default:
						{
							if (Imbuing.IsInNonImbueList(item.GetType()))
								goodtogo = false;
							break;
						}
					}

					break;
				}
			}

			if (!goodtogo)
			{
				from.SendLocalizedMessage(1152113); // You cannot reforge that item.
			}
		}

		return goodtogo;
	}

	public static void ApplyReforgedProperties(Item item, ReforgedPrefix prefix, ReforgedSuffix suffix, int budget, int perclow, int perchigh, int maxmods, int luckchance, BaseRunicTool tool, ReforgingOption option)
	{
		var props = new List<int>(ItemPropertyInfo.LookupLootTable(item));

		if (props.Count > 0)
		{
			ApplyReforgedProperties(item, props, prefix, suffix, budget, perclow, perchigh, maxmods, luckchance, tool, option);
		}

		ColUtility.Free(props);
	}

	public static void ApplyReforgedProperties(Item item, List<int> props, ReforgedPrefix prefix, ReforgedSuffix suffix, int budget, int perclow, int perchigh, int maxmods, int luckchance, BaseRunicTool tool = null, ReforgingOption option = ReforgingOption.None)
	{
		var reforged = tool != null;
		var powerful = reforged ? (option & ReforgingOption.Powerful) != 0 : IsPowerful(budget);

		if (prefix == ReforgedPrefix.None && (suffix == ReforgedSuffix.None || suffix > ReforgedSuffix.Aegis))
		{
			for (var i = 0; i < maxmods; i++)
			{
				ApplyRandomProperty(item, props, perclow, perchigh, ref budget, luckchance, reforged, powerful);
			}

			if (suffix != ReforgedSuffix.None)
			{
				ApplySuffixName(item, suffix);
			}
		}
		else
		{
			var prefixId = (int)prefix;
			var suffixId = (int)suffix;

			var index = GetCollectionIndex(item);
			var resIndex = -1;
			var preIndex = -1;
			// resIndex & preIndex = -1 indicates is not reforged

			if (reforged)
			{
				resIndex = GetResourceIndex(tool.Resource);
				preIndex = GetPrerequisiteIndex(option);
			}

			if (index == -1)
				return;

			List<NamedInfoCol> prefixCol = null;
			List<NamedInfoCol> suffixCol = null;

			if (prefix != ReforgedPrefix.None)
			{
				try
				{
					prefixCol = new List<NamedInfoCol>();
					prefixCol.AddRange(PrefixSuffixInfo[prefixId][index]);
				}
				catch
				{
					// ignored
				}
			}

			if (suffix != ReforgedSuffix.None)
			{
				suffixCol = new List<NamedInfoCol>();

				try
				{
					suffixCol.AddRange(PrefixSuffixInfo[suffixId][index]);
				}
				catch
				{
					// ignored
				}
			}

			//Removes things like blood drinking/balanced/splintering
			ValidateAttributes(item, prefixCol, reforged);
			ValidateAttributes(item, suffixCol, reforged);

			var i = 0;
			var mods = 0;

			if (prefix != ReforgedPrefix.None && suffix == ReforgedSuffix.None && prefixCol != null)
			{
				var specialAdd = 0;
				var nothing = 0;
				GetNamedModCount(prefixId, 0, maxmods, prefixCol.Count, 0, ref specialAdd, ref nothing);

				while (budget > 25 && mods < maxmods && i < 25)
				{
					if (prefixCol.Count > 0 && specialAdd > 0)
					{
						var random = Utility.Random(prefixCol.Count);

						if (ApplyPrefixSuffixAttribute(item, prefixCol[random], resIndex, preIndex, perclow, perchigh, ref budget, luckchance, reforged, powerful))
						{
							specialAdd--;
							mods++;
						}

						prefixCol.RemoveAt(random);
					}
					else if (ApplyRandomProperty(item, props, perclow, perchigh, ref budget, luckchance, reforged, powerful))
					{
						mods++;
					}

					i++;
				}

				ApplyPrefixName(item, prefix);
			}
			else if (prefix == ReforgedPrefix.None && suffix != ReforgedSuffix.None && suffixCol != null)
			{
				var specialAdd = 0;
				var nothing = 0;
				GetNamedModCount(0, suffixId, maxmods, 0, suffixCol.Count, ref nothing, ref specialAdd);

				while (budget > 25 && mods < maxmods && i < 25)
				{
					if (suffixCol.Count > 0 && specialAdd > 0)
					{
						var random = Utility.Random(suffixCol.Count);

						if (ApplyPrefixSuffixAttribute(item, suffixCol[random], resIndex, preIndex, perclow, perchigh, ref budget, luckchance, reforged, powerful))
						{
							specialAdd--;
							mods++;
						}

						suffixCol.RemoveAt(random);
					}
					else if (ApplyRandomProperty(item, props, perclow, perchigh, ref budget, luckchance, reforged, powerful))
					{
						mods++;
					}

					i++;
				}

				ApplySuffixName(item, suffix);
			}
			else if (prefix != ReforgedPrefix.None && suffix != ReforgedSuffix.None && prefixCol != null && suffixCol != null)
			{
				var specialAddPrefix = 0;
				var specialAddSuffix = 0;

				GetNamedModCount(prefixId, suffixId, maxmods, prefixCol.Count, suffixCol.Count, ref specialAddPrefix, ref specialAddSuffix);

				while (budget > 25 && mods < maxmods && i < 25)
				{
					if (prefixCol.Count > 0 && specialAddPrefix > 0)
					{
						var random = Utility.Random(prefixCol.Count);

						if (ApplyPrefixSuffixAttribute(item, prefixCol[random], resIndex, preIndex, perclow, perchigh, ref budget, luckchance, reforged, powerful))
						{
							specialAddPrefix--;
							mods++;
						}

						prefixCol.RemoveAt(random);
					}
					else if (suffixCol.Count > 0 && specialAddSuffix > 0)
					{
						var random = Utility.Random(suffixCol.Count);

						if (ApplyPrefixSuffixAttribute(item, suffixCol[random], resIndex, preIndex, perclow, perchigh, ref budget, luckchance, reforged, powerful))
						{
							specialAddSuffix--;
							mods++;
						}

						suffixCol.RemoveAt(random);
					}
					else if (ApplyRandomProperty(item, props, perclow, perchigh, ref budget, luckchance, reforged, powerful))
					{
						mods++;
					}

					i++;
				}

				ApplyPrefixName(item, prefix);

				ApplySuffixName(item, suffix);
			}

			if (m_Elements.ContainsKey(item))
				m_Elements.Remove(item);
		}
	}

	public static bool HasSelection(int index, Item toreforge, BaseRunicTool tool, ReforgingOption options, int prefix, int suffix)
	{
		// No Vampire prefix/suffix for non-weapons
		if (index == 6 && !(toreforge is BaseWeapon))
			return false;

		// Cannot choose same suffix/prefix
		//if (index != 0 && (index == prefix || index == suffix))
		//    return false;HasOption(options, ReforgingOption.StructuralAndFundamental, ReforgingOption.PowerfulAndFundamental)

		var type = ItemPropertyInfo.GetItemType(toreforge);

		if (type == ItemType.Melee)
		{
			switch (tool.Resource)
			{
				case CraftResource.DullCopper:
					if (index == 8 && HasOption(options, ReforgingOption.PowerfulStructuralAndFundamental))
						return false;
					break;
				case CraftResource.ShadowIron:
					switch (index)
					{
						case 8 when HasOption(options, ReforgingOption.PowerfulAndFundamental, ReforgingOption.StructuralAndFundamental):
						case >= 8 and <= 10 when HasOption(options, ReforgingOption.PowerfulStructuralAndFundamental):
							return false;
					}
					break;
				case CraftResource.Copper:
					switch (index)
					{
						case 8 when HasOption(options, ReforgingOption.PowerfulAndStructural, ReforgingOption.PowerfulAndFundamental):
						case >= 8 and <= 10 when HasOption(options, ReforgingOption.StructuralAndFundamental, ReforgingOption.PowerfulStructuralAndFundamental):
							return false;
					}

					break;
				case CraftResource.Bronze:
					switch (index)
					{
						case 8:
						case 9 when HasOption(options, ReforgingOption.Powerful):
						case >= 8 and <= 10 when HasOption(options, ReforgingOption.PowerfulAndStructural, ReforgingOption.PowerfulAndFundamental, ReforgingOption.StructuralAndFundamental, ReforgingOption.PowerfulStructuralAndFundamental):
							return false;
					}

					break;
				case CraftResource.Gold:
					switch (index)
					{
						case 8:
						case >= 8 and <= 10 when HasOption(options, ReforgingOption.Powerful, ReforgingOption.PowerfulAndStructural, ReforgingOption.PowerfulAndFundamental, ReforgingOption.StructuralAndFundamental, ReforgingOption.PowerfulStructuralAndFundamental):
							return false;
					}

					break;
				case CraftResource.Agapite:
				case CraftResource.Verite:
					switch (index)
					{
						case >= 8 and <= 10:
						case 12 when HasOption(options, ReforgingOption.PowerfulStructuralAndFundamental):
							return false;
					}

					break;
				case CraftResource.Valorite:
					switch (index)
					{
						case >= 8 and <= 10:
						case 12 when HasOption(options, ReforgingOption.PowerfulAndStructural, ReforgingOption.PowerfulAndFundamental, ReforgingOption.StructuralAndFundamental, ReforgingOption.PowerfulStructuralAndFundamental):
							return false;
					}

					break;

				case CraftResource.OakWood:
					switch (index)
					{
						case 8 when HasOption(options, ReforgingOption.StructuralAndFundamental):
						case >= 8 and <= 10 when HasOption(options, ReforgingOption.PowerfulStructuralAndFundamental):
							return false;
					}

					break;
				case CraftResource.AshWood:
					switch (index)
					{
						case 8:
						case >= 8 and <= 10 when HasOption(options, ReforgingOption.PowerfulAndStructural, ReforgingOption.PowerfulAndFundamental, ReforgingOption.StructuralAndFundamental, ReforgingOption.PowerfulStructuralAndFundamental):
							return false;
					}

					break;
				case CraftResource.YewWood:
					switch (index)
					{
						case >= 8 and <= 10:
						case 12 when HasOption(options, ReforgingOption.PowerfulStructuralAndFundamental):
							return false;
					}

					break;
				case CraftResource.Heartwood:
					switch (index)
					{
						case >= 8 and <= 10:
						case 12 when HasOption(options, ReforgingOption.PowerfulAndStructural, ReforgingOption.PowerfulAndFundamental, ReforgingOption.StructuralAndFundamental, ReforgingOption.PowerfulStructuralAndFundamental):
							return false;
					}

					break;
			}
		}
		else if (type == ItemType.Ranged)
		{
			switch (tool.Resource)
			{
				case CraftResource.OakWood:
					switch (index)
					{
						case 10 when HasOption(options, ReforgingOption.PowerfulAndStructural):
						case 8 or 10 when HasOption(options, ReforgingOption.PowerfulAndFundamental):
						case >= 8 and <= 10 when HasOption(options, ReforgingOption.PowerfulStructuralAndFundamental) || HasOption(options, ReforgingOption.StructuralAndFundamental):
							return false;
					}

					break;
				case CraftResource.AshWood:
					switch (index)
					{
						case 8:
						case 10:
						case >= 8 and <= 10 when HasOption(options, ReforgingOption.PowerfulAndStructural):
						case >= 8 and <= 11 when HasOption(options, ReforgingOption.PowerfulAndFundamental, ReforgingOption.StructuralAndFundamental, ReforgingOption.PowerfulStructuralAndFundamental):
							return false;
					}

					break;
				case CraftResource.YewWood:
					if (index is >= 8 and <= 11)
						return false;
					break;
				case CraftResource.Heartwood:
					switch (index)
					{
						case >= 8 and <= 11:
						case 12 when HasOption(options, ReforgingOption.StructuralAndFundamental, ReforgingOption.PowerfulStructuralAndFundamental):
							return false;
					}

					break;
			}
		}
		else if (type == ItemType.Shield)
		{
			if (index == 10)
				return false;

			switch (tool.Resource)
			{
				case CraftResource.DullCopper:
					if (index == 8 && HasOption(options, ReforgingOption.PowerfulStructuralAndFundamental))
						return false;
					break;
				case CraftResource.ShadowIron:
					switch (index)
					{
						case 8 when HasOption(options, ReforgingOption.PowerfulAndFundamental, ReforgingOption.StructuralAndFundamental):
						case 8 or 9 when HasOption(options, ReforgingOption.PowerfulStructuralAndFundamental):
							return false;
					}

					break;
				case CraftResource.Copper:
					switch (index)
					{
						case 8 when HasOption(options, ReforgingOption.PowerfulAndStructural, ReforgingOption.PowerfulAndFundamental):
						case 8 or 9 when HasOption(options, ReforgingOption.StructuralAndFundamental):
						case 5 or 8 or 9 when HasOption(options, ReforgingOption.PowerfulStructuralAndFundamental):
							return false;
					}
					break;
				case CraftResource.Bronze:
					switch (index)
					{
						case 8:
						case 9 when HasOption(options, ReforgingOption.PowerfulAndStructural):
						case 9 or 5 when HasOption(options, ReforgingOption.PowerfulAndFundamental):
						case 9 or 5 or 11 when HasOption(options, ReforgingOption.StructuralAndFundamental, ReforgingOption.PowerfulStructuralAndFundamental):
							return false;
					}
					break;
				case CraftResource.Gold:
					switch (index)
					{
						case 8:
						case 9 when HasOption(options, ReforgingOption.Powerful):
						case 5 or 9 when HasOption(options, ReforgingOption.PowerfulAndStructural):
						case 9 or 5 or 11 when HasOption(options, ReforgingOption.PowerfulAndFundamental, ReforgingOption.StructuralAndFundamental, ReforgingOption.PowerfulStructuralAndFundamental):
							return false;
					}
					break;
				case CraftResource.Agapite:
					switch (index)
					{
						case 8:
						case 9:
						case 5 or 11 when HasOption(options, ReforgingOption.PowerfulAndStructural, ReforgingOption.PowerfulAndFundamental, ReforgingOption.StructuralAndFundamental, ReforgingOption.PowerfulStructuralAndFundamental):
							return false;
					}
					break;
				case CraftResource.Verite:
				case CraftResource.Valorite:
					switch (index)
					{
						case 8:
						case 9:
						case 11:
						case 5 when HasOption(options, ReforgingOption.PowerfulAndStructural, ReforgingOption.PowerfulAndFundamental, ReforgingOption.StructuralAndFundamental, ReforgingOption.PowerfulStructuralAndFundamental):
							return false;
					}
					break;

				case CraftResource.OakWood:
					if (index == 8 && HasOption(options, ReforgingOption.PowerfulAndFundamental, ReforgingOption.StructuralAndFundamental))
						return false;
					if ((index == 8 || index == 9) && HasOption(options, ReforgingOption.PowerfulStructuralAndFundamental))
						return false;
					break;
				case CraftResource.AshWood:
					switch (index)
					{
						case 8:
						case 9 when HasOption(options, ReforgingOption.PowerfulAndStructural):
						case 9 or 5 when HasOption(options, ReforgingOption.PowerfulAndFundamental):
						case 9 or 5 or 11 when HasOption(options, ReforgingOption.StructuralAndFundamental, ReforgingOption.PowerfulStructuralAndFundamental):
							return false;
					}
					break;
				case CraftResource.YewWood:
					switch (index)
					{
						case 8:
						case 9:
						case 5 or 11 when HasOption(options, ReforgingOption.PowerfulAndStructural, ReforgingOption.PowerfulAndFundamental, ReforgingOption.StructuralAndFundamental, ReforgingOption.PowerfulStructuralAndFundamental):
							return false;
					}
					break;
				case CraftResource.Heartwood:
					switch (index)
					{
						case 8:
						case 9:
						case 11:
						case 5 when HasOption(options, ReforgingOption.PowerfulAndStructural, ReforgingOption.PowerfulAndFundamental, ReforgingOption.StructuralAndFundamental, ReforgingOption.PowerfulStructuralAndFundamental):
							return false;
					}
					break;
			}
		}
		else if (type == ItemType.Armor)
		{
			switch (tool.Resource)
			{
				case CraftResource.DullCopper:
					switch (index)
					{
						case 10 or 11 when HasOption(options, ReforgingOption.PowerfulAndFundamental):
						case 5 or 10 or 11 when HasOption(options, ReforgingOption.StructuralAndFundamental, ReforgingOption.PowerfulStructuralAndFundamental):
							return false;
					}
					break;
				case CraftResource.ShadowIron:
					switch (index)
					{
						case 10 or 11 when HasOption(options, ReforgingOption.PowerfulAndStructural):
						case 5 or 10 or 11 when HasOption(options, ReforgingOption.PowerfulAndFundamental, ReforgingOption.StructuralAndFundamental):
						case 5 or 9 or 10 or 11 when HasOption(options, ReforgingOption.PowerfulStructuralAndFundamental):
							return false;
					}
					break;
				case CraftResource.Copper:
					switch (index)
					{
						case 10:
						case 11:
						case 5 when HasOption(options, ReforgingOption.PowerfulAndStructural, ReforgingOption.PowerfulAndFundamental):
						case 5 or 9 when HasOption(options, ReforgingOption.StructuralAndFundamental):
						case 5 or 9 or 5 when HasOption(options, ReforgingOption.PowerfulStructuralAndFundamental):
							return false;
					}
					break;
				case CraftResource.Bronze:
					switch (index)
					{
						case 10:
						case 11:
						case 5 or 9 when HasOption(options, ReforgingOption.PowerfulAndStructural, ReforgingOption.PowerfulAndFundamental):
						case 5 or 9 or 12 when HasOption(options, ReforgingOption.StructuralAndFundamental, ReforgingOption.PowerfulStructuralAndFundamental):
							return false;
					}
					break;
				case CraftResource.Gold:
					switch (index)
					{
						case 10:
						case 11:
						case 9 when HasOption(options, ReforgingOption.Powerful):
						case 5 or 9 or 12 when HasOption(options, ReforgingOption.PowerfulAndStructural, ReforgingOption.PowerfulAndFundamental, ReforgingOption.StructuralAndFundamental, ReforgingOption.PowerfulStructuralAndFundamental):
							return false;
					}
					break;
				case CraftResource.Agapite:
					switch (index)
					{
						case >= 9 and <= 11:
						case 12 when HasOption(options, ReforgingOption.Powerful):
						case 12 or 5 when HasOption(options, ReforgingOption.PowerfulAndStructural, ReforgingOption.PowerfulAndFundamental, ReforgingOption.StructuralAndFundamental, ReforgingOption.PowerfulStructuralAndFundamental):
							return false;
					}
					break;
				case CraftResource.Verite:
				case CraftResource.Valorite:
					switch (index)
					{
						case >= 9 and <= 12:
						case 5 when HasOption(options, ReforgingOption.PowerfulAndStructural, ReforgingOption.PowerfulAndFundamental, ReforgingOption.StructuralAndFundamental, ReforgingOption.PowerfulStructuralAndFundamental):
							return false;
					}
					break;

				case CraftResource.SpinedLeather:
					switch (index)
					{
						case 10 or 11 when HasOption(options, ReforgingOption.PowerfulAndStructural):
						case 10 or 11 or 5 when HasOption(options, ReforgingOption.PowerfulAndFundamental, ReforgingOption.StructuralAndFundamental):
						case 9 or 10 or 11 or 5 when HasOption(options, ReforgingOption.PowerfulStructuralAndFundamental):
							return false;
					}
					break;
				case CraftResource.HornedLeather:
					switch (index)
					{
						case 10:
						case 11:
						case 9 when HasOption(options, ReforgingOption.Powerful):
						case 5 or 9 or 12 when HasOption(options, ReforgingOption.PowerfulAndStructural, ReforgingOption.PowerfulAndFundamental, ReforgingOption.StructuralAndFundamental, ReforgingOption.PowerfulStructuralAndFundamental):
							return false;
					}
					break;
				case CraftResource.BarbedLeather:
					switch (index)
					{
						case >= 9 and <= 12:
						case 5 when HasOption(options, ReforgingOption.PowerfulAndStructural, ReforgingOption.PowerfulAndFundamental, ReforgingOption.StructuralAndFundamental, ReforgingOption.PowerfulStructuralAndFundamental):
							return false;
					}
					break;

				case CraftResource.OakWood:
					switch (index)
					{
						case 10 or 11 when HasOption(options, ReforgingOption.PowerfulAndStructural, ReforgingOption.PowerfulAndFundamental, ReforgingOption.StructuralAndFundamental):
						case 9 when HasOption(options, ReforgingOption.PowerfulStructuralAndFundamental):
							return false;
					}
					break;
				case CraftResource.AshWood:
					switch (index)
					{
						case 10:
						case 11:
						case 5 or 9 when HasOption(options, ReforgingOption.PowerfulAndStructural, ReforgingOption.PowerfulAndFundamental):
						case 5 or 9 or 12 when HasOption(options, ReforgingOption.StructuralAndFundamental, ReforgingOption.PowerfulStructuralAndFundamental):
							return false;
					}
					break;
				case CraftResource.YewWood:
					switch (index)
					{
						case 9:
						case 10:
						case 11:
						case 12 when HasOption(options, ReforgingOption.Powerful):
						case 5 or 12 when HasOption(options, ReforgingOption.PowerfulAndStructural, ReforgingOption.PowerfulAndFundamental, ReforgingOption.StructuralAndFundamental, ReforgingOption.PowerfulStructuralAndFundamental):
							return false;
					}
					break;
				case CraftResource.Heartwood:
					switch (index)
					{
						case >= 9 and <= 12:
						case 5 when HasOption(options, ReforgingOption.PowerfulAndStructural, ReforgingOption.PowerfulAndStructural, ReforgingOption.PowerfulAndFundamental, ReforgingOption.StructuralAndFundamental, ReforgingOption.PowerfulStructuralAndFundamental):
							return false;
					}
					break;
			}
		}

		return true;
	}

	public static bool HasOption(ReforgingOption options, params ReforgingOption[] optionArray)
	{
		return optionArray.Any(option => (options & option) == option);
	}

	private static void ValidateAttributes(Item item, List<NamedInfoCol> list, bool reforged)
	{
		if (list == null || list.Count == 0)
			return;

		list.IterateReverse(col =>
		{
			if (col != null && list.Contains(col) && !ItemPropertyInfo.ValidateProperty(item, col.Attribute, reforged))
			{
				list.Remove(col);
			}
		});
	}

	private static void GetNamedModCount(int prefixId, int suffixId, int maxmods, int precolcount, int suffixcolcount, ref int prefixCount, ref int suffixCount)
	{
		if (prefixId > 0 && suffixId > 0)
		{
			if (0.5 > Utility.RandomDouble())
			{
				// Even Split
				if (0.5 > Utility.RandomDouble())
				{
					prefixCount = maxmods / 2;
					suffixCount = maxmods - prefixCount;
				}
				else
				{
					suffixCount = maxmods / 2;
					prefixCount = maxmods - suffixCount;
				}
			}
			else if (0.5 > Utility.RandomDouble())
			{
				prefixCount = maxmods / 2 - 1;
				suffixCount = maxmods - prefixCount;
			}
			else
			{
				suffixCount = maxmods / 2 - 1;
				prefixCount = maxmods - suffixCount;
			}
		}
		else
		{
			int mods;

			switch (maxmods)
			{
				default:
					mods = maxmods / 2; break;
				case 6:
				case 5:
				case 4: mods = Utility.RandomBool() ? 2 : 3; break;
				case 3: mods = Utility.RandomBool() ? 1 : 2; break;
				case 2:
				case 1: mods = 1; break;
			}

			if (prefixId > 0)
				prefixCount = mods;
			else
				suffixCount = mods;
		}

		if (prefixCount > precolcount)
			prefixCount = precolcount;

		if (suffixCount > suffixcolcount)
			suffixCount = suffixcolcount;
	}

	public static int GetPropertyCount(BaseRunicTool tool)
	{
		return tool.Resource switch
		{
			CraftResource.DullCopper => Utility.RandomMinMax(1, 2),
			CraftResource.ShadowIron => Utility.RandomMinMax(1, 2),
			CraftResource.Copper => Utility.RandomMinMax(2, 3),
			CraftResource.Bronze => 3,
			CraftResource.Gold => 3,
			CraftResource.Agapite => Utility.RandomMinMax(3, 4),
			CraftResource.Verite => Utility.RandomMinMax(3, 4),
			CraftResource.Valorite => 5,
			CraftResource.SpinedLeather => Utility.RandomMinMax(1, 2),
			CraftResource.HornedLeather => 3,
			CraftResource.BarbedLeather => 5,
			CraftResource.OakWood => Utility.RandomMinMax(1, 2),
			CraftResource.AshWood => 2,
			CraftResource.YewWood => 3,
			CraftResource.Heartwood => 5,
			_ => 1
		};
	}

	private static bool ApplyPrefixSuffixAttribute(Item item, NamedInfoCol col, int resIndex, int preIndex, int percLow, int percHigh, ref int budget, int luckchance, bool reforged, bool powerful)
	{
		var start = budget;
		var attribute = col.Attribute;

		// Converts Collection entry into actual attribute
		if (attribute is string s)
		{
			switch (s)
			{
				case "RandomEater": attribute = GetRandomEater(); break;
				case "HitSpell": attribute = GetRandomHitSpell(); break;
				case "HitArea": attribute = GetRandomHitArea(); break;
				case "Slayer": attribute = BaseRunicTool.GetRandomSlayer(); break;
				case "WeaponVelocity": break;
				case "ElementalDamage": attribute = GetRandomElemental(); break;
			}
		}

		var id = ItemPropertyInfo.GetId(attribute);

		// prop is invalid, or the item already has a value for this prop
		if (id == -1 || Imbuing.GetValueForId(item, id) > 0 || !ItemPropertyInfo.ValidateProperty(item, id, reforged))
		{
			return false;
		}

		if (reforged)
		{
			ApplyReforgedNameProperty(item, id, col, resIndex, preIndex, 0, 100, ref budget, luckchance, true, powerful);
		}
		else
		{
			ApplyProperty(item, id, percLow, percHigh, ref budget, luckchance, false, powerful);
		}

		return start != budget;
	}

	private static readonly Dictionary<Item, int[]> m_Elements = new();

	public static bool ApplyResistance(Item item, int value, AosElementAttribute attribute)
	{
		var resists = GetElementalAttributes(item);

		if (!m_Elements.ContainsKey(item))
		{
			switch (item)
			{
				case BaseArmor armor:
					m_Elements[armor] = new[] { armor.PhysicalBonus, armor.FireBonus, armor.ColdBonus, armor.PoisonBonus, armor.EnergyBonus };
					break;
				case BaseWeapon weapon:
					m_Elements[weapon] = new[] { weapon.WeaponAttributes.ResistPhysicalBonus, weapon.WeaponAttributes.ResistFireBonus,
						weapon.WeaponAttributes.ResistColdBonus, weapon.WeaponAttributes.ResistPoisonBonus, weapon.WeaponAttributes.ResistEnergyBonus };
					break;
				default:
				{
					if (resists != null)
					{
						m_Elements[item] = new[] { resists[AosElementAttribute.Physical], resists[AosElementAttribute.Fire], resists[AosElementAttribute.Cold],
							resists[AosElementAttribute.Poison], resists[AosElementAttribute.Energy] };
					}
					else
					{
						return false;
					}

					break;
				}
			}
		}

		switch (attribute)
		{
			default:
			case AosElementAttribute.Physical:
				switch (item)
				{
					case BaseArmor armor when !m_Elements.ContainsKey(armor) || armor.PhysicalBonus == m_Elements[armor][0]:
						armor.PhysicalBonus = value;
						return true;
					case BaseWeapon weapon when !m_Elements.ContainsKey(weapon) || weapon.WeaponAttributes.ResistPhysicalBonus == m_Elements[weapon][0]:
						weapon.WeaponAttributes.ResistPhysicalBonus = value;
						return true;
					default:
					{
						if (resists != null && (!m_Elements.ContainsKey(item) || resists[attribute] == m_Elements[item][0]))
						{
							resists[attribute] = value;
							return true;
						}

						break;
					}
				}
				break;
			case AosElementAttribute.Fire:
				switch (item)
				{
					case BaseArmor armor when !m_Elements.ContainsKey(armor) || armor.FireBonus == m_Elements[armor][1]:
						armor.FireBonus = value;
						return true;
					case BaseWeapon weapon when !m_Elements.ContainsKey(weapon) || weapon.WeaponAttributes.ResistFireBonus == m_Elements[weapon][1]:
						weapon.WeaponAttributes.ResistFireBonus = value;
						return true;
					default:
					{
						if (resists != null && (!m_Elements.ContainsKey(item) || resists[attribute] == m_Elements[item][1]))
						{
							resists[attribute] = value;
							return true;
						}

						break;
					}
				}
				break;
			case AosElementAttribute.Cold:
				switch (item)
				{
					case BaseArmor armor when !m_Elements.ContainsKey(armor) || armor.ColdBonus == m_Elements[armor][2]:
						armor.ColdBonus = value;
						return true;
					case BaseWeapon weapon when !m_Elements.ContainsKey(weapon) || weapon.WeaponAttributes.ResistColdBonus == m_Elements[weapon][2]:
						weapon.WeaponAttributes.ResistColdBonus = value;
						return true;
					default:
					{
						if (resists != null && (!m_Elements.ContainsKey(item) || resists[attribute] == m_Elements[item][2]))
						{
							resists[attribute] = value;
							return true;
						}

						break;
					}
				}
				break;
			case AosElementAttribute.Poison:
				switch (item)
				{
					case BaseArmor armor when !m_Elements.ContainsKey(armor) || armor.PoisonBonus == m_Elements[armor][3]:
						armor.PoisonBonus = value;
						return true;
					case BaseWeapon weapon when !m_Elements.ContainsKey(weapon) || weapon.WeaponAttributes.ResistPoisonBonus == m_Elements[weapon][3]:
						weapon.WeaponAttributes.ResistPoisonBonus = value;
						return true;
					default:
					{
						if (resists != null && (!m_Elements.ContainsKey(item) || resists[attribute] == m_Elements[item][3]))
						{
							resists[attribute] = value;
							return true;
						}

						break;
					}
				}
				break;
			case AosElementAttribute.Energy:
				switch (item)
				{
					case BaseArmor armor when !m_Elements.ContainsKey(armor) || armor.EnergyBonus == m_Elements[armor][4]:
						armor.EnergyBonus = value;
						return true;
					case BaseWeapon weapon when !m_Elements.ContainsKey(weapon) || weapon.WeaponAttributes.ResistEnergyBonus == m_Elements[weapon][4]:
						weapon.WeaponAttributes.ResistEnergyBonus = value;
						return true;
					default:
					{
						if (resists != null && (!m_Elements.ContainsKey(item) || resists[attribute] == m_Elements[item][4]))
						{
							resists[attribute] = value;
							return true;
						}

						break;
					}
				}
				break;
		}

		return false;
	}

	public static int Scale(int min, int max, int perclow, int perchigh, int luckchance, bool reforged)
	{
		int percent;

		percent = Utility.RandomMinMax(reforged ? perclow : 0, perchigh);

		if (LootPack.CheckLuck(luckchance))
			percent += 10;

		if (percent < perclow) percent = perclow;
		if (percent > perchigh) percent = perchigh;

		var scaledBy = Math.Abs(min - max) + 1;

		scaledBy = 10000 / scaledBy;

		percent *= 10000 + scaledBy;

		return min + (max - min) * percent / 1000001;
	}

	private static int CalculateValue(Item item, object attribute, int min, int max, int perclow, int perchigh, ref int budget, int luckchance)
	{
		return CalculateValue(item, attribute, min, max, perclow, perchigh, ref budget, luckchance, false);
	}

	private static int CalculateValue(Item item, object attribute, int min, int max, int perclow, int perchigh, ref int budget, int luckchance, bool reforged)
	{
		var scale = Math.Max(1, ItemPropertyInfo.GetScale(item, attribute, true));

		if (min < scale)
		{
			min = scale;
		}

		var value = Scale(min, max, perclow, perchigh, luckchance, reforged);

		if (scale > 1 && value > scale)
		{
			value = value / scale * scale;
		}

		var totalweight = ItemPropertyInfo.GetTotalWeight(item, attribute, value);

		while (budget <= totalweight)
		{
			value -= scale;

			if (value <= 0)
			{
				if (ItemPropertyInfo.GetTotalWeight(item, attribute, 3) > budget)
					budget = 0;

				return 0;
			}

			totalweight = ItemPropertyInfo.GetTotalWeight(item, attribute, value);
		}

		return value;
	}

	private static int GetTotalMods(Item item)
	{
		return Imbuing.GetTotalMods(item);
	}

	private static ItemPropertyInfo GetItemProps(object attr)
	{
		var id = attr switch
		{
			AosAttribute attribute => ItemPropertyInfo.GetIdForAttribute(attribute),
			AosWeaponAttribute attribute => ItemPropertyInfo.GetIdForAttribute(attribute),
			SkillName name => ItemPropertyInfo.GetIdForAttribute(name),
			SlayerName name => ItemPropertyInfo.GetIdForAttribute(name),
			SAAbsorptionAttribute attribute => ItemPropertyInfo.GetIdForAttribute(attribute),
			AosArmorAttribute attribute => ItemPropertyInfo.GetIdForAttribute(attribute),
			AosElementAttribute attribute => ItemPropertyInfo.GetIdForAttribute(attribute),
			_ => -1
		};

		if (ItemPropertyInfo.Table.ContainsKey(id))
			return ItemPropertyInfo.Table[id];

		return null;
	}

	private static int GetCollectionIndex(IEntity item)
	{
		switch (item)
		{
			case BaseWeapon:
				return 0;
			case BaseShield:
				return 2;
			case BaseArmor:
			case BaseClothing:
				return 1;
			case BaseJewel:
				return 3;
			default:
				return -1;
		}
	}

	private static int GetResourceIndex(CraftResource resource)
	{
		// RunicIndex 0 - dullcopper; 1 - shadow; 2 - copper; 3 - spined; 4 - Oak; 5 - ash
		return resource switch
		{
			CraftResource.DullCopper => 0,
			CraftResource.ShadowIron => 1,
			CraftResource.Bronze => 2,
			CraftResource.Gold => 2,
			CraftResource.Agapite => 2,
			CraftResource.Verite => 2,
			CraftResource.Valorite => 2,
			CraftResource.Copper => 2,
			CraftResource.SpinedLeather => 3,
			CraftResource.OakWood => 4,
			CraftResource.YewWood => 5,
			CraftResource.Heartwood => 5,
			CraftResource.Bloodwood => 5,
			CraftResource.Frostwood => 5,
			CraftResource.HornedLeather => 5,
			CraftResource.BarbedLeather => 5,
			CraftResource.AshWood => 5,
			_ => 0
		};
	}

	private static int GetPrerequisiteIndex(ReforgingOption option)
	{
		if ((option & ReforgingOption.Powerful) != 0 &&
		    (option & ReforgingOption.Structural) != 0 &&
		    (option & ReforgingOption.Fundamental) != 0)
			return 6;

		if ((option & ReforgingOption.Structural) != 0 &&
		    (option & ReforgingOption.Fundamental) != 0)
			return 5;

		if ((option & ReforgingOption.Powerful) != 0 &&
		    (option & ReforgingOption.Structural) != 0)
			return 4;

		if ((option & ReforgingOption.Fundamental) != 0)
			return 3;

		if ((option & ReforgingOption.Structural) != 0)
			return 2;

		return (option & ReforgingOption.Powerful) != 0 ? 1 : 0;
	}

	private static int CalculateMinIntensity(int perclow, int perchi, ReforgingOption option)
	{
		if (option == ReforgingOption.None)
			return perclow;

		return perclow + (int)((perchi - perclow) * (GetPrerequisiteIndex(option) * 5.0 / 100.0));
	}

	private static readonly Dictionary<Type, CraftSystem> m_AllowableTable = new();

	public static Dictionary<int, NamedInfoCol[][]> PrefixSuffixInfo { get; } = new();

	public static void Initialize()
	{
		m_AllowableTable[typeof(LeatherGlovesOfMining)] = DefTailoring.CraftSystem;
		m_AllowableTable[typeof(RingmailGlovesOfMining)] = DefBlacksmithy.CraftSystem;
		m_AllowableTable[typeof(StuddedGlovesOfMining)] = DefTailoring.CraftSystem;
		m_AllowableTable[typeof(JukaBow)] = DefBowFletching.CraftSystem;
		m_AllowableTable[typeof(TribalSpear)] = DefBlacksmithy.CraftSystem;
		m_AllowableTable[typeof(Pickaxe)] = DefBlacksmithy.CraftSystem;
		m_AllowableTable[typeof(Cleaver)] = DefBlacksmithy.CraftSystem;
		m_AllowableTable[typeof(SkinningKnife)] = DefBlacksmithy.CraftSystem;
		m_AllowableTable[typeof(ButcherKnife)] = DefBlacksmithy.CraftSystem;
		m_AllowableTable[typeof(GargishNecklace)] = DefBlacksmithy.CraftSystem;
		m_AllowableTable[typeof(GargishEarrings)] = DefBlacksmithy.CraftSystem;
		m_AllowableTable[typeof(GargishAmulet)] = DefBlacksmithy.CraftSystem;
		m_AllowableTable[typeof(GargishStoneAmulet)] = DefMasonry.CraftSystem;
		//m_AllowableTable[typeof(BarbedWhip)] = DefTailoring.CraftSystem;
		//m_AllowableTable[typeof(SpikedWhip)] = DefTailoring.CraftSystem;
		//m_AllowableTable[typeof(BladedWhip)] = DefTailoring.CraftSystem;
	}

	public static void Configure()
	{
		Commands.CommandSystem.Register("GetCreatureScore", AccessLevel.GameMaster, e =>
		{
			e.Mobile.BeginTarget(12, false, TargetFlags.None, (_, targeted) =>
			{
				if (targeted is BaseCreature creature)
				{
					creature.PrivateOverheadMessage(Network.MessageType.Regular, 0x25, false, GetDifficultyFor(creature).ToString(), e.Mobile.NetState);
				}
			});
		});

		// TypeIndex 0 - Weapon; 1 - Armor; 2 - Shield; 3 - Jewels
		// RunicIndex 0 - dullcopper; 1 - shadow; 2 - copper; 3 - spined; 4 - Oak; 5 - ash
		PrefixSuffixInfo[0] = null;
		PrefixSuffixInfo[1] = new[] //Might
		{
			new[] // Weapon
			{
				new NamedInfoCol(AosWeaponAttribute.HitLeechHits, HitsAndManaLeechTable),
				new NamedInfoCol(AosAttribute.BonusHits, WeaponHitsTable),
				new NamedInfoCol(AosAttribute.BonusStr, WeaponStrTable),
				new NamedInfoCol(AosAttribute.RegenHits, WeaponRegenTable),
			},

			new[] // armor
			{
				new NamedInfoCol("RandomEater", EaterTable),
				new NamedInfoCol(AosAttribute.BonusHits, ArmorHitsTable),
				new NamedInfoCol(AosAttribute.BonusStr, ArmorStrTable),
				new NamedInfoCol(AosAttribute.RegenHits, ArmorRegenTable),
			},

			new[] // shield
			{
				new NamedInfoCol("RandomEater", EaterTable),
				new NamedInfoCol(AosAttribute.BonusHits, ArmorHitsTable),
				new NamedInfoCol(AosAttribute.BonusStr, ArmorStrTable),
				new NamedInfoCol(AosAttribute.RegenHits, ArmorRegenTable),
			},

			new[] // jewels
			{
				new NamedInfoCol(AosAttribute.BonusHits, ArmorHitsTable),
				new NamedInfoCol(AosAttribute.BonusStr, ArmorStrTable),
			}
		};

		PrefixSuffixInfo[2] = new[] //Mystic
		{
			new[] // Weapon
			{
				new NamedInfoCol(AosWeaponAttribute.HitLeechMana, HitsAndManaLeechTable),
				new NamedInfoCol(AosAttribute.BonusMana, WeaponStamManaLmcTable),
				new NamedInfoCol(AosAttribute.BonusInt, DexIntTable),
				new NamedInfoCol(AosAttribute.LowerManaCost, WeaponStamManaLmcTable),
				new NamedInfoCol(AosAttribute.RegenMana, WeaponRegenTable),
				/*new NamedInfoCol(AosAttribute.LowerRegCost, LowerRegTable), */
			},
			new[] // armor
			{
				new NamedInfoCol(AosAttribute.BonusInt, DexIntTable),
				new NamedInfoCol(AosAttribute.BonusMana, ArmorStamManaLmcTable),
				new NamedInfoCol(AosAttribute.LowerManaCost, ArmorStamManaLmcTable),
				new NamedInfoCol(AosAttribute.RegenMana, ArmorRegenTable),
			},
			new[]
			{
				new NamedInfoCol(AosAttribute.BonusMana, ArmorStamManaLmcTable),
				new NamedInfoCol(AosAttribute.BonusInt, DexIntTable),
				new NamedInfoCol(AosAttribute.LowerManaCost, ArmorStamManaLmcTable),
				new NamedInfoCol(AosAttribute.RegenMana, ArmorRegenTable),
			},
			new[]
			{
				new NamedInfoCol(AosAttribute.BonusMana, ArmorStamManaLmcTable),
				new NamedInfoCol(AosAttribute.BonusInt, DexIntTable),
				new NamedInfoCol(AosAttribute.LowerManaCost, ArmorStamManaLmcTable),
				new NamedInfoCol(AosAttribute.LowerRegCost, LowerRegTable),
			},
		};

		PrefixSuffixInfo[3] = new[] // Animated
		{
			new[] // Weapon
			{
				new NamedInfoCol(AosWeaponAttribute.HitLeechStam, HitStamLeechTable),
				new NamedInfoCol(AosAttribute.BonusStam, WeaponStamManaLmcTable),
				new NamedInfoCol(AosAttribute.BonusDex, DexIntTable),
				new NamedInfoCol(AosAttribute.RegenStam, WeaponRegenTable),
				new NamedInfoCol(AosAttribute.WeaponSpeed, WeaponWeaponSpeedTable),
			},
			new[] // armor
			{
				new NamedInfoCol(AosAttribute.BonusStam, ArmorStamManaLmcTable),
				new NamedInfoCol(AosAttribute.BonusDex, DexIntTable),
				new NamedInfoCol(AosAttribute.RegenStam, ArmorRegenTable),
			},
			new[]
			{
				new NamedInfoCol(AosAttribute.BonusStam, ArmorStamManaLmcTable),
				new NamedInfoCol(AosAttribute.BonusDex, DexIntTable),
				new NamedInfoCol(AosAttribute.RegenStam, ArmorRegenTable),
				new NamedInfoCol(AosAttribute.WeaponSpeed, ShieldWeaponSpeedTable),
			},
			new[]
			{
				new NamedInfoCol(AosAttribute.BonusStam, ArmorStamManaLmcTable),
				new NamedInfoCol(AosAttribute.BonusDex, DexIntTable),
				new NamedInfoCol(AosAttribute.WeaponSpeed, ShieldWeaponSpeedTable),
			},
		};
		PrefixSuffixInfo[4] = new[] //Arcane
		{
			new[] // Weapon
			{
				new NamedInfoCol(AosWeaponAttribute.HitLeechMana, HitsAndManaLeechTable),
				new NamedInfoCol(AosWeaponAttribute.HitManaDrain, HitWeaponTable2),
				new NamedInfoCol(AosAttribute.BonusMana, WeaponStamManaLmcTable),
				new NamedInfoCol(AosAttribute.BonusInt, DexIntTable),
				new NamedInfoCol(AosAttribute.LowerManaCost, WeaponStamManaLmcTable),
				new NamedInfoCol(AosAttribute.CastSpeed, 1),
				new NamedInfoCol(AosAttribute.SpellChanneling, 1),
				new NamedInfoCol(AosWeaponAttribute.MageWeapon, MageWeaponTable),
				new NamedInfoCol(AosAttribute.RegenMana, WeaponRegenTable),
			},
			new[] // armor
			{
				new NamedInfoCol(AosAttribute.BonusMana, ArmorStamManaLmcTable),
				new NamedInfoCol(AosAttribute.BonusInt, DexIntTable),
				new NamedInfoCol(AosAttribute.LowerManaCost, ArmorStamManaLmcTable),
				new NamedInfoCol(AosAttribute.RegenMana, ArmorRegenTable),
				new NamedInfoCol(AosAttribute.LowerRegCost, LowerRegTable),
			},
			new[]
			{
				new NamedInfoCol(AosAttribute.BonusMana, ArmorStamManaLmcTable),
				new NamedInfoCol(AosAttribute.BonusInt, DexIntTable),
				new NamedInfoCol(AosAttribute.LowerManaCost, ArmorStamManaLmcTable),
				new NamedInfoCol(AosAttribute.RegenMana, ArmorRegenTable),
			},
			new[]
			{
				new NamedInfoCol(AosAttribute.BonusMana, ArmorStamManaLmcTable),
				new NamedInfoCol(AosAttribute.BonusInt, DexIntTable),
				new NamedInfoCol(AosAttribute.LowerManaCost, ArmorStamManaLmcTable),
				new NamedInfoCol(AosAttribute.RegenMana, ArmorRegenTable),
				new NamedInfoCol(AosAttribute.LowerRegCost, LowerRegTable),
				new NamedInfoCol(AosAttribute.CastSpeed, 1),
				new NamedInfoCol(AosAttribute.CastRecovery, 4),
				new NamedInfoCol(AosAttribute.SpellDamage, 18),
			},
		};
		PrefixSuffixInfo[5] = new[] // Exquisite
		{
			new[] // Weapon
			{
				new NamedInfoCol(AosWeaponAttribute.SelfRepair, SelfRepairTable), //
				new NamedInfoCol(AosWeaponAttribute.DurabilityBonus, DurabilityTable), //
				new NamedInfoCol(AosWeaponAttribute.LowerStatReq, LowerStatReqTable), //
				new NamedInfoCol("Slayer", 1), //
				new NamedInfoCol(AosWeaponAttribute.MageWeapon, MageWeaponTable), // 
				new NamedInfoCol(AosAttribute.SpellChanneling, 1), //
				new NamedInfoCol(AosAttribute.BalancedWeapon, 1), //
				new NamedInfoCol("WeaponVelocity", WeaponVelocityTable), // 
				new NamedInfoCol("ElementalDamage", ElementalDamageTable), //
			},
			new[] // armor
			{
				new NamedInfoCol(AosArmorAttribute.SelfRepair, SelfRepairTable),
				new NamedInfoCol(AosArmorAttribute.DurabilityBonus, DurabilityTable),
				new NamedInfoCol(AosArmorAttribute.LowerStatReq, LowerStatReqTable),
			},
			new[] // shield
			{
				new NamedInfoCol(AosArmorAttribute.SelfRepair, SelfRepairTable),
				new NamedInfoCol(AosArmorAttribute.DurabilityBonus, DurabilityTable),
				new NamedInfoCol(AosArmorAttribute.LowerStatReq, LowerStatReqTable),
			},
			new NamedInfoCol[]
			{
			},
		};
		PrefixSuffixInfo[6] = new[] //Vampiric
		{
			new[] // Weapon
			{
				new NamedInfoCol(AosWeaponAttribute.HitLeechHits, HitsAndManaLeechTable),
				new NamedInfoCol(AosWeaponAttribute.HitLeechStam, HitStamLeechTable),
				new NamedInfoCol(AosWeaponAttribute.HitLeechMana, HitsAndManaLeechTable),
				new NamedInfoCol(AosWeaponAttribute.HitManaDrain, HitWeaponTable2),
				new NamedInfoCol(AosWeaponAttribute.HitFatigue, HitWeaponTable2),
				new NamedInfoCol(AosWeaponAttribute.BloodDrinker, 1),
			},
			new NamedInfoCol[] // armor
			{
			},
			new NamedInfoCol[]
			{
			},
			new NamedInfoCol[]
			{
			},
		};
		PrefixSuffixInfo[7] = new[] // Invigorating
		{
			new[] // Weapon
			{
				new NamedInfoCol(AosAttribute.RegenHits, WeaponRegenTable),
				new NamedInfoCol(AosAttribute.RegenStam, WeaponRegenTable),
				new NamedInfoCol(AosAttribute.RegenMana, WeaponRegenTable),
			},
			new[] // armor
			{
				new NamedInfoCol(AosAttribute.RegenHits, ArmorRegenTable),
				new NamedInfoCol(AosAttribute.RegenStam, ArmorRegenTable),
				new NamedInfoCol(AosAttribute.RegenMana, ArmorRegenTable),

				new NamedInfoCol("RandomEater",  EaterTable),
			},
			new[]
			{
				new NamedInfoCol(AosAttribute.RegenHits, ArmorRegenTable),
				new NamedInfoCol(AosAttribute.RegenStam, ArmorRegenTable),
				new NamedInfoCol(AosAttribute.RegenMana, ArmorRegenTable),
				new NamedInfoCol(AosArmorAttribute.SoulCharge, ShieldSoulChargeTable),
				new NamedInfoCol("RandomEater", EaterTable),
			},
			new[]
			{
				new NamedInfoCol(AosAttribute.RegenHits, ArmorRegenTable),
				new NamedInfoCol(AosAttribute.RegenStam, ArmorRegenTable),
				new NamedInfoCol(AosAttribute.RegenMana, ArmorRegenTable),
			},
		};
		PrefixSuffixInfo[8] = new[] // Fortified
		{
			new[] // Weapon
			{
				new NamedInfoCol(AosWeaponAttribute.ResistPhysicalBonus, ResistTable),
				new NamedInfoCol(AosWeaponAttribute.ResistFireBonus, ResistTable),
				new NamedInfoCol(AosWeaponAttribute.ResistColdBonus, ResistTable),
				new NamedInfoCol(AosWeaponAttribute.ResistPoisonBonus, ResistTable),
				new NamedInfoCol(AosWeaponAttribute.ResistEnergyBonus, ResistTable),
			},
			new[] // armor
			{
				new NamedInfoCol(AosElementAttribute.Physical, ResistTable),
				new NamedInfoCol(AosElementAttribute.Fire, ResistTable),
				new NamedInfoCol(AosElementAttribute.Cold, ResistTable),
				new NamedInfoCol(AosElementAttribute.Poison, ResistTable),
				new NamedInfoCol(AosElementAttribute.Energy, ResistTable),
				new NamedInfoCol("RandomEater", EaterTable),
			},
			new[]
			{
				new NamedInfoCol(AosElementAttribute.Physical, ResistTable),
				new NamedInfoCol(AosElementAttribute.Fire, ResistTable),
				new NamedInfoCol(AosElementAttribute.Cold, ResistTable),
				new NamedInfoCol(AosElementAttribute.Poison, ResistTable),
				new NamedInfoCol(AosElementAttribute.Energy, ResistTable),
				new NamedInfoCol("RandomEater", EaterTable),
			},
			new[]
			{
				new NamedInfoCol(AosElementAttribute.Physical, ResistTable),
				new NamedInfoCol(AosElementAttribute.Fire, ResistTable),
				new NamedInfoCol(AosElementAttribute.Cold, ResistTable),
				new NamedInfoCol(AosElementAttribute.Poison, ResistTable),
				new NamedInfoCol(AosElementAttribute.Energy, ResistTable),
			},
		};
		PrefixSuffixInfo[9] = new[] // Auspicious
		{
			new[] // Weapon
			{
				new NamedInfoCol(AosAttribute.Luck, LuckTable, RangedLuckTable),
			},
			new[] // armor
			{
				new NamedInfoCol(AosAttribute.Luck, LuckTable),
			},
			new[]
			{
				new NamedInfoCol(AosAttribute.Luck, LuckTable),
			},
			new[]
			{
				new NamedInfoCol(AosAttribute.Luck, LuckTable),
			},
		};
		PrefixSuffixInfo[10] = new[] // Charmed
		{
			new[] // Weapon
			{
				new NamedInfoCol(AosAttribute.EnhancePotions, WeaponEnhancePots),
				new NamedInfoCol(AosAttribute.BalancedWeapon, 1),
			},
			new[] // armor
			{
				new NamedInfoCol(AosAttribute.EnhancePotions, ArmorEnhancePotsTable),
			},
			new NamedInfoCol[]
			{
			},
			new[]
			{
				new NamedInfoCol(AosAttribute.EnhancePotions, ArmorEnhancePotsTable),
			},
		};
		PrefixSuffixInfo[11] = new[] //Vicious
		{
			new[] // Weapon
			{
				new NamedInfoCol("HitSpell", HitWeaponTable1),
				new NamedInfoCol("HitArea", HitWeaponTable1),
				new NamedInfoCol(AosAttribute.AttackChance, WeaponHciTable, RangedHciTable),
				new NamedInfoCol(AosAttribute.WeaponDamage, WeaponDamageTable),
				new NamedInfoCol(AosWeaponAttribute.BattleLust, 1),
				new NamedInfoCol(AosWeaponAttribute.SplinteringWeapon, 30),
				new NamedInfoCol("Slayer", 1),
			},
			new[] // armor
			{
				new NamedInfoCol(AosAttribute.AttackChance, ArmorHcidciTable),
			},
			new[]
			{
				new NamedInfoCol(AosAttribute.AttackChance, WeaponHciTable),
			},
			new[]
			{
				new NamedInfoCol(AosAttribute.AttackChance, WeaponHciTable),
				new NamedInfoCol(AosAttribute.SpellDamage, 18),
			},
		};
		PrefixSuffixInfo[12] = new[] // Towering
		{
			new[] // Weapon
			{
				new NamedInfoCol(AosWeaponAttribute.HitLowerAttack, HitWeaponTable1),
				new NamedInfoCol(AosWeaponAttribute.ReactiveParalyze, 1),
				new NamedInfoCol(AosAttribute.DefendChance, WeaponDciTable, RangedDciTable),
			},
			new[] // armor
			{
				new NamedInfoCol(AosAttribute.DefendChance, ArmorHcidciTable),
				new NamedInfoCol(SAAbsorptionAttribute.CastingFocus, ArmorCastingFocusTable),
			},
			new[]
			{
				new NamedInfoCol(AosAttribute.DefendChance, WeaponDciTable),
				new NamedInfoCol(AosArmorAttribute.ReactiveParalyze, 1),
				new NamedInfoCol(AosArmorAttribute.SoulCharge, ShieldSoulChargeTable),
			},
			new[]
			{
				new NamedInfoCol(AosAttribute.DefendChance, ArmorHcidciTable),
				//new NamedInfoCol(SAAbsorptionAttribute.CastingFocus, ArmorCastingFocusTable),
			},
		};
	}

	public class NamedInfoCol
	{
		public object Attribute { get; }
		public int[][] Info { get; }
		public int[][] SecondaryInfo { get; }

		public int HardCap { get; }

		public NamedInfoCol(object attr, int[][] info, int[][] secondary = null)
		{
			Attribute = attr;
			Info = info;
			SecondaryInfo = secondary;
		}

		public NamedInfoCol(object attr, int hardcap)
		{
			Attribute = attr;
			HardCap = hardcap;
		}

		public int RandomRangedIntensity(Item item, int id, int resIndex, int preIndex)
		{
			if (Info == null || HardCap == 1)
				return HardCap;

			var range = item is BaseRanged && SecondaryInfo != null ? SecondaryInfo[resIndex] : Info[resIndex];

			var max = range[preIndex];
			var min = Math.Max(ItemPropertyInfo.GetMinIntensity(item, id), (int)(range[0] * .75));
			int value;

			if (Utility.RandomBool())
			{
				value = Utility.RandomBool() ? min : max;
			}
			else
			{
				value = Utility.RandomMinMax(min, max);
			}

			var scale = ItemPropertyInfo.GetScale(item, id, true);

			if (scale > 1 && value > scale)
			{
				value = value / scale * scale;
			}

			return value;
		}
	}

	public static object GetRandomHitSpell()
	{
		switch (Utility.Random(4))
		{
			default:
				return AosWeaponAttribute.HitMagicArrow;
			case 1: return AosWeaponAttribute.HitFireball;
			case 2: return AosWeaponAttribute.HitHarm;
			case 3: return AosWeaponAttribute.HitLightning;
			//case 4: return AosWeaponAttribute.HitCurse;
		}
	}

	private static object GetRandomHitArea()
	{
		switch (Utility.Random(5))
		{
			default:
				return AosWeaponAttribute.HitPhysicalArea;
			case 1: return AosWeaponAttribute.HitFireArea;
			case 2: return AosWeaponAttribute.HitColdArea;
			case 3: return AosWeaponAttribute.HitPoisonArea;
			case 4: return AosWeaponAttribute.HitEnergyArea;
		}
	}

	private static object GetRandomEater()
	{
		switch (Utility.Random(6))
		{
			default:
				return SAAbsorptionAttribute.EaterKinetic;
			case 1: return SAAbsorptionAttribute.EaterFire;
			case 2: return SAAbsorptionAttribute.EaterCold;
			case 3: return SAAbsorptionAttribute.EaterPoison;
			case 4: return SAAbsorptionAttribute.EaterEnergy;
			case 5: return SAAbsorptionAttribute.EaterDamage;
		}
	}

	private static AosElementAttribute GetRandomElemental()
	{
		switch (Utility.Random(5))
		{
			default:
				return AosElementAttribute.Fire;
			case 1: return AosElementAttribute.Cold;
			case 2: return AosElementAttribute.Poison;
			case 3: return AosElementAttribute.Energy;
			case 4: return AosElementAttribute.Chaos;
		}
	}

	private static SkillName GetRandomSkill(Item item)
	{
		var skillbonuses = GetAosSkillBonuses(item);

		if (skillbonuses == null)
		{
			return SkillName.Alchemy;
		}

		var possibleSkills = m_Skills;
		SkillName sk;

		bool found;

		do
		{
			found = false;
			sk = possibleSkills[Utility.Random(possibleSkills.Length)];

			if ((item is GargishRing || item is GargishBracelet) && sk == SkillName.Archery)
				sk = SkillName.Throwing;

			for (var i = 0; !found && i < 5; ++i)
			{
				found = skillbonuses.GetValues(i, out var check, out _) && check == sk;
			}
		} while (found);

		return sk;
	}

	public static int GetName(int value)
	{
		switch (value)
		{
			default:
				return 1062648;
			case 1: return 1151717;
			case 2:
			case 3:
			case 4:
			case 5:
			case 6:
			case 7:
			case 8:
			case 9:
			case 10:
			case 11:
			case 12: return 1151706 + (value - 2);
		}
	}

	public static void ApplyPrefixName(Item item, ReforgedPrefix prefix)
	{
		switch (item)
		{
			case BaseWeapon weapon:
				weapon.ReforgedPrefix = prefix;
				break;
			case BaseShield shield:
				shield.ReforgedPrefix = prefix;
				break;
			case BaseArmor armor:
				armor.ReforgedPrefix = prefix;
				break;
			case BaseJewel jewel:
				jewel.ReforgedPrefix = prefix;
				break;
			case BaseClothing clothing:
				clothing.ReforgedPrefix = prefix;
				break;
		}
	}

	public static void ApplySuffixName(Item item, ReforgedSuffix suffix)
	{
		switch (item)
		{
			case BaseWeapon weapon:
				weapon.ReforgedSuffix = suffix;
				break;
			case BaseShield shield:
				shield.ReforgedSuffix = suffix;
				break;
			case BaseArmor armor:
				armor.ReforgedSuffix = suffix;
				break;
			case BaseJewel jewel:
				jewel.ReforgedSuffix = suffix;
				break;
			case BaseClothing clothing:
				clothing.ReforgedSuffix = suffix;
				break;
		}
	}

	public static int GetPrefixName(ReforgedPrefix prefix)
	{
		return NameTable[(int)prefix - 1][0];
	}

	public static int GetSuffixName(ReforgedSuffix suffix)
	{
		return NameTable[(int)suffix - 1][1];
	}

	public static int[][] NameTable { get; } =
	{
		new[] { 1151682, 1151683 }, // Might
		new[] { 1151684, 1151685 }, // Mystic
		new[] { 1151686, 1151687 }, // Animated
		new[] { 1151688, 1151689 }, // Arcane
		new[] { 1151690, 1151691 }, // Exquisite
		new[] { 1151692, 1151693 }, // Vampiric
		new[] { 1151694, 1151695 }, // Invigorating
		new[] { 1151696, 1151697 }, // Fortified
		new[] { 1151698, 1151699 }, // Auspicious
		new[] { 1151700, 1151701 }, // Charmed
		new[] { 1151702, 1151703 }, // Vicious
		new[] { 1151704, 1151705 }, // Towering
		new[] {       0, 1154548 }, // Blackthorn
		new[] {       0, 1154507 }, // Minax
		new[] {       0, 1156900 }, // Kotl
		new[] {       0, 1158672 }, // Khaldun
		new[] {       0, 1155589 }, // Doom
		new[] {       0, 1157614 }, // Sorcerers Dungeon
		new[] {       0, 1159317 }, // Fellowship
	};

	public static void AddSuffixName(ObjectPropertyList list, ReforgedSuffix suffix, string name)
	{
		if (suffix >= ReforgedSuffix.Blackthorn)
		{
			list.Add(GetSuffixName(suffix), name);
		}
		else
		{
			list.Add(1151758, $"{name}\t#{GetSuffixName(suffix)}");// ~1_ITEM~ of ~2_SUFFIX~
		}
	}

	private static readonly SkillName[] m_Skills = {
		SkillName.Swords,
		SkillName.Fencing,
		SkillName.Macing,
		SkillName.Archery,
		SkillName.Wrestling,
		SkillName.Parry,
		SkillName.Tactics,
		SkillName.Anatomy,
		SkillName.Healing,
		SkillName.Magery,
		SkillName.Meditation,
		SkillName.EvalInt,
		SkillName.MagicResist,
		SkillName.AnimalTaming,
		SkillName.AnimalLore,
		SkillName.Veterinary,
		SkillName.Musicianship,
		SkillName.Provocation,
		SkillName.Discordance,
		SkillName.Peacemaking,
		SkillName.Chivalry,
		SkillName.Focus,
		SkillName.Necromancy,
		SkillName.Stealing,
		SkillName.Stealth,
		SkillName.SpiritSpeak,
		SkillName.Bushido,
		SkillName.Ninjitsu
	};

	#region Random Item Generation
	public static Item GenerateRandomItem(IEntity e)
	{
		var item = Loot.RandomArmorOrShieldOrWeaponOrJewelry(LootPackEntry.IsInTokuno(e), LootPackEntry.IsMondain(e), LootPackEntry.IsStygian(e));

		if (item != null)
			GenerateRandomItem(item, null, Utility.RandomMinMax(100, 700), 0, ReforgedPrefix.None, ReforgedSuffix.None);

		return item;
	}

	/// <summary>
	/// This can be called from lootpack once loot pack conversions are implemented (if need be)
	/// </summary>
	/// <param name="item">item to mutate</param>
	/// <param name="luck">adjust luck chance</param>
	/// <param name="minBudget"></param>
	/// <param name="maxBudget"></param>
	/// <returns></returns>
	public static bool GenerateRandomItem(Item item, int luck, int minBudget, int maxBudget)
	{
		if (item is not BaseWeapon && item is not BaseArmor && item is not BaseJewel && item is not BaseHat)
			return false;

		var budget = Utility.RandomMinMax(minBudget, maxBudget);
		GenerateRandomItem(item, null, budget, luck, ReforgedPrefix.None, ReforgedSuffix.None);
		return true;
	}

	/// <summary>
	/// Called in DemonKnight.cs for forcing rad items
	/// </summary>
	/// <param name="item"></param>
	/// <param name="luck">raw luck</param>
	/// <param name="budget"></param>
	/// <param name="prefix"></param>
	/// <param name="suffix"></param>
	/// <returns></returns>
	public static bool GenerateRandomArtifactItem(Item item, int luck, int budget, ReforgedPrefix prefix = ReforgedPrefix.None, ReforgedSuffix suffix = ReforgedSuffix.None)
	{
		if (prefix == ReforgedPrefix.None)
			prefix = ChooseRandomPrefix(item, budget);

		if (suffix == ReforgedSuffix.None)
			suffix = ChooseRandomSuffix(item, budget);

		if (item is not BaseWeapon && item is not BaseArmor && item is not BaseJewel && item is not BaseHat)
			return false;

		GenerateRandomItem(item, null, budget, LootPack.GetLuckChance(luck), prefix, suffix, artifact: true);
		return true;
	}

	public static Item GenerateRandomItem(Mobile killer, BaseCreature creature)
	{
		var item = Loot.RandomArmorOrShieldOrWeaponOrJewelry(LootPackEntry.IsInTokuno(killer), LootPackEntry.IsMondain(killer), LootPackEntry.IsStygian(killer));

		if (item != null)
			GenerateRandomItem(item, killer, Math.Max(100, GetDifficultyFor(creature)), LootPack.GetLuckChance(GetLuckForKiller(creature)), ReforgedPrefix.None, ReforgedSuffix.None);

		return item;
	}

	/// <summary>
	/// Called in LootPack.cs
	/// </summary>
	public static bool GenerateRandomItem(Item item, Mobile killer, BaseCreature creature)
	{
		if (item is not BaseWeapon && item is not BaseArmor && item is not BaseJewel && item is not BaseHat)
			return false;

		GenerateRandomItem(item, killer, Math.Max(100, GetDifficultyFor(creature)), LootPack.GetLuckChance(GetLuckForKiller(creature)), ReforgedPrefix.None, ReforgedSuffix.None);
		return true;

	}

	public static bool GenerateRandomItem(Item item, Mobile killer, BaseCreature creature, ReforgedPrefix prefix, ReforgedSuffix suffix)
	{
		if (item is not BaseWeapon && item is not BaseArmor && item is not BaseJewel && item is not BaseHat)
			return false;

		GenerateRandomItem(item, killer, Math.Max(100, GetDifficultyFor(creature)), LootPack.GetLuckChance(GetLuckForKiller(creature)), prefix, suffix);
		return true;
	}

	/// <summary>
	/// Called from TreasureMapChest.cs
	/// </summary>
	/// <param name="item">item to mutate</param>
	/// <param name="luck">raw luck</param>
	/// <param name="minBudget"></param>
	/// <param name="maxBudget"></param>
	/// <param name="map"></param>
	/// <returns></returns>
	public static bool GenerateRandomItem(Item item, int luck, int minBudget, int maxBudget, Map map)
	{
		if (item is not BaseWeapon && item is not BaseArmor && item is not BaseJewel && item is not BaseHat)
			return false;

		var budget = Utility.RandomMinMax(minBudget, maxBudget);
		GenerateRandomItem(item, null, budget, LootPack.GetLuckChance(luck), ReforgedPrefix.None, ReforgedSuffix.None, map);
		return true;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="item">item to mutate</param>
	/// <param name="killer">who killed the monster, if applicable</param>
	/// <param name="basebudget">where to we start, regarding the difficulty of the monster we killed</param>
	/// <param name="luckchance">adjusted luck</param>
	/// <param name="forcedprefix"></param>
	/// <param name="forcedsuffix"></param>
	/// <param name="map"></param>
	/// <param name="artifact"></param>
	public static void GenerateRandomItem(Item item, Mobile killer, int basebudget, int luckchance, ReforgedPrefix forcedprefix, ReforgedSuffix forcedsuffix, Map map = null, bool artifact = false)
	{
		if (map == null && killer != null)
		{
			map = killer.Map;
		}

		if (item == null)
			return;

		var budget = basebudget;

		if (killer is BaseCreature { Controlled: true } bc)
		{
			killer = bc.ControlMaster;
		}

		var rawLuck = killer != null ? killer is PlayerMobile mobile ? mobile.RealLuck : killer.Luck : 0;

		int mods;
		int perclow;
		int perchigh;

		var prefix = forcedprefix;
		var suffix = forcedsuffix;

		if (artifact)
		{
			ChooseArtifactMods(item, budget, out mods, out perclow, out perchigh);
		}
		else
		{
			var budgetBonus = 0;

			if (killer != null)
			{
				if (map != null && map.Rules == ZoneRules.FeluccaRules)
				{
					budgetBonus = RandomItemGenerator.FeluccaBudgetBonus;
				}
			}

			var divisor = GetDivisor(basebudget);

			double perc;
			var highest = 0.0;

			for (var i = 0; i < 1 + rawLuck / 600; i++)
			{
				perc = (100.0 - Math.Sqrt(Utility.RandomMinMax(0, 10000))) / 100.0;

				if (perc > highest)
					highest = perc;
			}

			perc = highest;

			if (perc > 1.0) perc = 1.0;
			var toAdd = Math.Min(500, RandomItemGenerator.MaxAdjustedBudget - basebudget);

			budget = Utility.RandomMinMax(basebudget - basebudget / divisor, (int)(basebudget + toAdd * perc)) + budgetBonus;

			// Gives a rare chance for a high end item to drop on a low budgeted monster
			if (rawLuck > 0 && !IsPowerful(budget) && LootPack.CheckLuck(luckchance / 6))
			{
				budget = Utility.RandomMinMax(600, 1150);
			}

			budget = Math.Min(RandomItemGenerator.MaxAdjustedBudget, budget);
			budget = Math.Max(RandomItemGenerator.MinAdjustedBudget, budget);

			if (!(item is BaseWeapon) && prefix == ReforgedPrefix.Vampiric)
				prefix = ReforgedPrefix.None;

			if (!(item is BaseWeapon) && suffix == ReforgedSuffix.Vampire)
				suffix = ReforgedSuffix.None;

			if (forcedprefix == ReforgedPrefix.None && budget >= Utility.Random(2700) && suffix < ReforgedSuffix.Minax)
				prefix = ChooseRandomPrefix(item, budget);

			if (forcedsuffix == ReforgedSuffix.None && budget >= Utility.Random(2700))
				suffix = ChooseRandomSuffix(item, budget, prefix);

			if (!IsPowerful(budget))
			{
				mods = Math.Max(1, GetProperties(5));

				perchigh = Math.Max(50, Math.Min(500, budget) / mods);
				perclow = Math.Max(20, perchigh / 3);
			}
			else
			{
				var maxmods = Math.Max(5, Math.Min(RandomItemGenerator.MaxProps - 1, (int)Math.Ceiling(budget / (double)Utility.RandomMinMax(100, 140))));
				var minmods = Math.Max(4, maxmods - 4);

				mods = Math.Max(minmods, GetProperties(maxmods));

				perchigh = 100;
				perclow = Utility.RandomMinMax(50, 70);
			}

			if (perchigh > 100) perchigh = 100;
			if (perclow < 10) perclow = 10;
			if (perclow > 80) perclow = 80;
		}

		if (mods < RandomItemGenerator.MaxProps - 1 && LootPack.CheckLuck(luckchance))
			mods++;

		var props = new List<int>(ItemPropertyInfo.LookupLootTable(item));
		var powerful = IsPowerful(budget);

		ApplyReforgedProperties(item, props, prefix, suffix, budget, perclow, perchigh, mods, luckchance);

		var addonbudget = 0;

		if (!artifact)
		{
			addonbudget = TryApplyRandomDisadvantage(item);
		}

		if (addonbudget > 0)
		{
			for (var i = 0; i < 5; i++)
			{
				ApplyRandomProperty(item, props, perclow, perchigh, ref addonbudget, luckchance, false, powerful);

				if (addonbudget <= 0 || mods + i + 1 >= RandomItemGenerator.MaxProps)
					break;
			}
		}

		var neg = GetNegativeAttributes(item);

		if (neg != null)
		{
			if (item is IDurability durability && (neg.Antique == 1 || neg.Brittle == 1 || durability is BaseJewel))
			{
				durability.MaxHitPoints = 255;
				durability.HitPoints = 255;
			}

			var wepAttrs = GetAosWeaponAttributes(item);

			if (wepAttrs != null && wepAttrs[AosWeaponAttribute.SelfRepair] > 0)
			{
				wepAttrs[AosWeaponAttribute.SelfRepair] = 0;
			}

			var armAttrs = GetAosArmorAttributes(item);

			if (armAttrs != null && armAttrs[AosArmorAttribute.SelfRepair] > 0)
			{
				armAttrs[AosArmorAttribute.SelfRepair] = 0;
			}
		}

		var power = ApplyItemPower(item, false);

		if (artifact && power < ItemPower.LesserArtifact)
		{
			var extra = 5000;

			do
			{
				ApplyRandomProperty(item, props, perclow, perchigh, ref extra, luckchance, false, powerful);
			}
			while (ApplyItemPower(item, false) < ItemPower.LesserArtifact);
		}

		// hues
		if (power == ItemPower.LegendaryArtifact && (item is BaseArmor || item is BaseClothing))
		{
			item.Hue = 2500;
		}

		item.Hue = suffix switch
		{
			ReforgedSuffix.Minax => 1157,
			ReforgedSuffix.Khaldun => 2745,
			ReforgedSuffix.Kotl => 2591,
			ReforgedSuffix.EnchantedOrigin => 1171,
			ReforgedSuffix.Doom => 2301,
			ReforgedSuffix.Fellowship => 2751,
			_ => item.Hue
		};

		ColUtility.Free(props);
	}

	private static bool IsPowerful(int budget)
	{
		return budget >= 550;
	}

	private static int GetProperties(int max)
	{
		if (max > RandomItemGenerator.MaxProps - 1)
			max = RandomItemGenerator.MaxProps - 1;

		int p0 = 0, p1 = 0, p2 = 0, p3 = 0, p4 = 0, p5 = 0, p6 = 0, p7 = 0, p8 = 0, p9 = 0, p10 = 0;
		const int p11 = 0;

		switch (max)
		{
			case 1: p0 = 3; p1 = 1; break;
			case 2: p0 = 6; p1 = 3; p2 = 1; break;
			case 3: p0 = 10; p1 = 6; p2 = 3; p3 = 1; break;
			case 4: p0 = 16; p1 = 12; p2 = 6; p3 = 5; p4 = 1; break;
			case 5: p0 = 30; p1 = 25; p2 = 20; p3 = 15; p4 = 9; p5 = 1; break;
			case 6: p0 = 35; p1 = 30; p2 = 25; p3 = 20; p4 = 15; p5 = 9; p6 = 1; break;
			case 7: p0 = 40; p1 = 35; p2 = 30; p3 = 25; p4 = 20; p5 = 15; p6 = 9; p7 = 1; break;
			case 8: p0 = 50; p1 = 40; p2 = 35; p3 = 30; p4 = 25; p5 = 20; p6 = 15; p7 = 9; p8 = 1; break;
			case 9: p0 = 70; p1 = 55; p2 = 45; p3 = 35; p4 = 30; p5 = 25; p6 = 20; p7 = 15; p8 = 9; p9 = 1; break;
			case 10: p0 = 90; p1 = 70; p2 = 55; p3 = 45; p4 = 40; p5 = 35; p6 = 25; p7 = 15; p8 = 10; p9 = 5; p10 = 1; break;
		}

		var pc = p0 + p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9 + p10 + p11;

		var rnd = Utility.Random(pc);

		if (rnd < p10)
			return 10;
		rnd -= p10;

		if (rnd < p9)
			return 9;
		rnd -= p9;

		if (rnd < p8)
			return 8;
		rnd -= p8;

		if (rnd < p7)
			return 7;
		rnd -= p7;

		if (rnd < p6)
			return 6;
		rnd -= p6;

		if (rnd < p5)
			return 5;
		rnd -= p5;

		if (rnd < p4)
			return 4;
		rnd -= p4;

		if (rnd < p3)
			return 3;
		rnd -= p3;

		if (rnd < p2)
			return 2;
		rnd -= p2;

		return rnd < p1 ? 1 : 0;
	}

	private static void ChooseArtifactMods(IEntity item, int budget, out int mods, out int perclow, out int perchigh)
	{
		var maxmods = Math.Min(10, budget / 120);
		mods = Utility.RandomMinMax(6, maxmods);

		perchigh = 100;
		perclow = item is BaseShield ? 100 : 80;
	}

	private static int GetDivisor(int basebudget)
	{
		return basebudget switch
		{
			< 400 => 5,
			< 550 => 4,
			< 650 => 3,
			_ => 2
		};
	}

	public static int GetLuckForKiller(BaseCreature dead)
	{
		var highest = dead.GetHighestDamager();

		if (highest != null)
		{
			return highest is PlayerMobile mobile ? mobile.RealLuck : highest.Luck;
		}

		return 0;
	}

	private static readonly Dictionary<int, int> m_Standard = new()
	{
		{ 1,  10 },
		{ 2,  10 },
		{ 3,  10 },
		{ 4,  10 },
		{ 5,  10 },
		{ 7,  10 },
		{ 8,  10 },
		{ 9,  2 },
		{ 10, 2 },
		{ 11, 5 },
		{ 12, 5 },
	};

	private static readonly Dictionary<int, int> m_StandardPowerful = new()
	{
		{ 1,  10 },
		{ 2,  10 },
		{ 3,  10 },
		{ 4,  10 },
		{ 5,  10 },
		{ 7,  10 },
		{ 8,  10 },
		{ 9,  0 },
		{ 10, 0 },
		{ 11, 2 },
		{ 12, 2 },
	};

	private static readonly Dictionary<int, int> m_Weapon = new()
	{
		{ 1,  10 },
		{ 2,  10 },
		{ 3,  10 },
		{ 4,  10 },
		{ 5,  10 },
		{ 6,  10 },
		{ 7,  10 },
		{ 8,  10 },
		{ 9,  2 },
		{ 10, 2 },
		{ 11, 5 },
		{ 12, 5 },
	};

	private static readonly Dictionary<int, int> m_WeaponPowerful = new()
	{
		{ 1,  10 },
		{ 2,  10 },
		{ 3,  10 },
		{ 4,  10 },
		{ 5,  10 },
		{ 6,  10 },
		{ 7,  10 },
		{ 8,  10 },
		{ 9,  0 },
		{ 10, 0 },
		{ 11, 2 },
		{ 12, 2 },
	};

	public static ReforgedPrefix ChooseRandomPrefix(Item item, int budget, ReforgedSuffix suffix = ReforgedSuffix.None)
	{
		Dictionary<int, int> table;
		var powerful = budget > 600;

		if (item is BaseWeapon)
		{
			table = powerful ? m_WeaponPowerful : m_Weapon;
		}
		else
		{
			table = powerful ? m_StandardPowerful : m_Standard;
		}

		var random = GetRandomName(table);

		while (suffix != 0 && random == (int)suffix)
			random = GetRandomName(table);

		return (ReforgedPrefix)random;
	}

	public static ReforgedSuffix ChooseRandomSuffix(Item item, int budget, ReforgedPrefix prefix = ReforgedPrefix.None)
	{
		//int random = item is BaseWeapon ? m_Weapon[Utility.Random(m_Weapon.Length)] : m_Standard[Utility.Random(m_Standard.Length)];
		Dictionary<int, int> table;
		var powerful = budget > 600;

		if (item is BaseWeapon)
		{
			table = powerful ? m_WeaponPowerful : m_Weapon;
		}
		else
		{
			table = powerful ? m_StandardPowerful : m_Standard;
		}

		var random = GetRandomName(table);

		while (prefix != 0 && random == (int)prefix)
			random = GetRandomName(table);

		return (ReforgedSuffix)random;
	}

	private static int GetRandomName(Dictionary<int, int> table)
	{
		var total = table.Sum(kvp => kvp.Value);

		var random = Utility.RandomMinMax(1, total);
		total = 0;

		foreach (var kvp in table)
		{
			total += kvp.Value;

			if (total >= random)
			{
				return kvp.Key;
			}
		}

		return 0;
	}

	private static int GetDifficultyFor(BaseCreature bc)
	{
		return RandomItemGenerator.GetDifficultyFor(bc);
	}

	private static int TryApplyRandomDisadvantage(Item item)
	{
		var attrs = GetAosAttributes(item);
		var neg = GetNegativeAttributes(item);

		if (attrs == null || neg == null)
			return 0;

		Imbuing.GetMaxWeight(item);
		var power = GetItemPower(item, Imbuing.GetTotalWeight(item, -1, false, false), Imbuing.GetTotalMods(item), false);
		var chance = Utility.RandomDouble();

		if (item is BaseJewel && power >= ItemPower.MajorArtifact)
		{
			if (chance > .25)
				neg.Antique = 1;
			else
				item.LootType = LootType.Cursed;
			return 100;
		}

		switch (power)
		{
			default:
				return 0;
			case ItemPower.Lesser: // lesser magic
			{
				if (.95 >= chance)
					return 0;

				switch (Utility.Random(item is BaseJewel ? 6 : 8))
				{
					case 0: neg.Prized = 1; break;
					case 1: neg.Antique = 1; break;
					case 2:
					case 3: neg.Unwieldly = 1; break;
					case 4:
					case 5: item.LootType = LootType.Cursed; break;
					case 6:
					case 7: neg.Massive = 1; break;
				}

				return 100;
			}
			case ItemPower.Greater:// greater magic
			{
				if (.75 >= chance)
					return 0;

				chance = Utility.RandomDouble();

				if (.75 > chance)
				{
					switch (Utility.Random(item is BaseJewel ? 4 : 6))
					{
						case 0: neg.Prized = 1; break;
						case 1: neg.Antique = 1; break;
						case 2:
						case 3: neg.Unwieldly = 1; break;
						case 4:
						case 5: neg.Massive = 1; break;
					}

					return 100;
				}

				switch (chance)
				{
					case < .5:
						neg.Prized = 1;
						return 100;
					case < .85:
					{
						if (Utility.RandomBool() || item is BaseJewel)
							neg.Antique = 1;
						else
							neg.Brittle = 1;

						return 150;
					}
					default:
						item.LootType = LootType.Cursed;
						return 100;
				}
			}
			case ItemPower.Major: // major magic
			{
				if (.50 >= chance)
					return 0;

				chance = Utility.RandomDouble();

				switch (chance)
				{
					case < .4:
						neg.Prized = 1;
						return 100;
					case < .6:
						switch (Utility.Random(item is BaseJewel ? 6 : 8))
						{
							case 0: neg.Prized = 1; break;
							case 1: neg.Antique = 1; break;
							case 2:
							case 3: neg.Unwieldly = 1; break;
							case 4:
							case 5: item.LootType = LootType.Cursed; break;
							case 6:
							case 7: neg.Massive = 1; break;
						}

						return 100;
				}

				if (.9 > chance || item is BaseJewel)
				{
					neg.Antique = 1;
					return 150;
				}
				neg.Brittle = 1;
				return 150;
			}
			case ItemPower.LesserArtifact: // lesser arty
			case ItemPower.GreaterArtifact: // greater arty
			{
				if (0.001 > chance)
					return 0;

				chance = Utility.RandomDouble();

				switch (chance)
				{
					case < 0.33 when !(item is BaseJewel):
						neg.Brittle = 1;
						return 150;
					case < 0.66:
						item.LootType = LootType.Cursed;
						return 150;
					case < 0.85:
						neg.Antique = 1;
						return 150;
					default:
						neg.Prized = 1;
						return 100;
				}
			}
			case ItemPower.MajorArtifact:
			case ItemPower.LegendaryArtifact:
			{
				if (0.0001 > Utility.RandomDouble())
					return 0;

				if (0.85 > chance)
				{
					neg.Antique = 1;
					return 100;
				}

				if (.95 > chance)
				{
					item.LootType = LootType.Cursed;
					return 100;
				}
				neg.Brittle = 1;
				return 100;
			}
		}
	}

	public static ItemPower ApplyItemPower(Item item, bool reforged)
	{
		var ip = GetItemPower(item, Imbuing.GetTotalWeight(item, -1, false, false), Imbuing.GetTotalMods(item), reforged);

		if (item is ICombatEquipment equipment)
		{
			equipment.ItemPower = ip;
		}

		return ip;
	}

	private static ItemPower GetItemPower(Item item, int weight, int totalMods, bool reforged)
	{
		// pre-arty uses max imbuing weight + 100
		// arty ranges from pre-arty to a flat 1200
		double preArty = Imbuing.GetMaxWeight(item) + 100;
		var arty = 1200 - preArty;

		if (totalMods == 0)
			return ItemPower.None;

		if (weight < preArty * .4)
			return reforged ? ItemPower.ReforgedMinor : ItemPower.Minor;

		if (weight < preArty * .6)
			return reforged ? ItemPower.ReforgedLesser : ItemPower.Lesser;

		if (weight < preArty * .8)
			return reforged ? ItemPower.ReforgedGreater : ItemPower.Greater;

		if (weight <= preArty)
			return reforged ? ItemPower.ReforgedGreater : ItemPower.Major;

		if (weight < preArty + arty * .2)
			return reforged ? ItemPower.ReforgedMajor : ItemPower.LesserArtifact;

		if (weight < preArty + arty * .4)
			return reforged ? ItemPower.ReforgedMajor : ItemPower.GreaterArtifact;

		if (weight < preArty + arty * .7 || totalMods <= 5)
			return ItemPower.MajorArtifact;

		return reforged ? ItemPower.ReforgedLegendary : ItemPower.LegendaryArtifact;
	}

	private static bool ApplyRandomProperty(Item item, IList<int> props, int perclow, int perchigh, ref int budget, int luckchance, bool reforged, bool powerful)
	{
		if (props == null || props.Count == 0)
		{
			return false;
		}

		var id = -1;

		while (true)
		{
			var random = props[Utility.Random(props.Count)];

			if (random == 1000)
			{
				random = ItemPropertyInfo.GetId(BaseRunicTool.GetRandomSlayer());
			}
			else if (random >= 1001)
			{
				random = ItemPropertyInfo.GetId(GetRandomSkill(item));
			}

			if (Imbuing.GetValueForId(item, random) == 0 && ItemPropertyInfo.ValidateProperty(item, random, reforged))
			{
				id = random;
				break;
			}

			props.Remove(random);

			if (props.Count == 0)
			{
				break;
			}
		}

		if (id == -1)
		{
			return false;
		}

		return ApplyProperty(item, id, perclow, perchigh, ref budget, luckchance, reforged, powerful);
	}

	/// <summary>
	/// unsafe applies property. Checks need to be made prior to calling this, see Imbuing.GetValueForID and ItemPropertyInfo.ValidateProperty
	/// </summary>
	/// <param name="item"></param>
	/// <param name="id"></param>
	/// <param name="perclow"></param>
	/// <param name="perchigh"></param>
	/// <param name="budget"></param>
	/// <param name="luckchance"></param>
	/// <param name="reforged"></param>
	/// <param name="powerful"></param>
	/// <returns></returns>
	public static bool ApplyProperty(Item item, int id, int perclow, int perchigh, ref int budget, int luckchance, bool reforged, bool powerful)
	{
		var min = ItemPropertyInfo.GetMinIntensity(item, id);
		var naturalMax = ItemPropertyInfo.GetMaxIntensity(item, id, false, true);
		var max = naturalMax;
		int[] overcap = null;

		if (powerful)
		{
			overcap = ItemPropertyInfo.GetMaxOverCappedRange(item, id);

			if (overcap != null)
			{
				max = overcap[^1];
			}
		}

		var value = CalculateValue(item, ItemPropertyInfo.GetAttribute(id), min, max, perclow, perchigh, ref budget, luckchance, reforged);

		// We're using overcap, so the value must have gone over the natural max, but under the overrcap max
		if (overcap is { Length: > 0 } && value > naturalMax && value < max)
		{
			value = overcap.Length > 1 ? AdjustOvercap(overcap, value) : naturalMax;
		}

		Imbuing.SetProperty(item, id, value);
		budget -= Imbuing.GetIntensityForId(item, id, -1, value);

		return true;
	}

	private static void ApplyReforgedNameProperty(Item item, int id, NamedInfoCol info, int resIndex, int preIndex,
		int perclow, int perchigh, ref int budget, int luckchance, bool reforged, bool powerful)
	{
		var value = info.RandomRangedIntensity(item, id, resIndex, preIndex);

		Imbuing.SetProperty(item, id, value);

		budget -= Imbuing.GetIntensityForId(item, id, -1, value);
	}

	private static int AdjustOvercap(IReadOnlyList<int> overcap, int value)
	{
		for (var i = overcap.Count - 1; i >= 0; i--)
		{
			if (value >= overcap[i])
			{
				return overcap[i];
			}
		}

		return overcap[0];
	}

	public static AosAttributes GetAosAttributes(Item item)
	{
		return item switch
		{
			BaseWeapon weapon => weapon.Attributes,
			BaseArmor armor => armor.Attributes,
			BaseClothing clothing => clothing.Attributes,
			BaseJewel jewel => jewel.Attributes,
			BaseTalisman talisman => talisman.Attributes,
			BaseQuiver quiver => quiver.Attributes,
			Spellbook spellbook => spellbook.Attributes,
			_ => null
		};

		//if (item is FishingPole)
		//    return ((FishingPole)item).Attributes;
	}

	public static AosArmorAttributes GetAosArmorAttributes(Item item)
	{
		return item switch
		{
			BaseArmor armor => armor.ArmorAttributes,
			BaseClothing clothing => clothing.ClothingAttributes,
			_ => null
		};
	}

	public static AosWeaponAttributes GetAosWeaponAttributes(Item item)
	{
		return item switch
		{
			BaseWeapon weapon => weapon.WeaponAttributes,
			BaseGlasses glasses => glasses.WeaponAttributes,
			GargishGlasses glasses => glasses.WeaponAttributes,
			_ => item switch
			{
				ElvenGlasses glasses => glasses.WeaponAttributes,
				BaseArmor armor => armor.WeaponAttributes,
				BaseClothing clothing => clothing.WeaponAttributes,
				_ => null
			}
		};
	}

	public static ExtendedWeaponAttributes GetExtendedWeaponAttributes(Item item)
	{
		return item is BaseWeapon weapon ? weapon.ExtendedWeaponAttributes : null;
	}

	public static AosElementAttributes GetElementalAttributes(Item item)
	{
		return item switch
		{
			BaseClothing clothing => clothing.Resistances,
			BaseJewel jewel => jewel.Resistances,
			BaseWeapon weapon => weapon.AosElementDamages,
			BaseQuiver quiver => quiver.Resistances,
			_ => null
		};
	}

	public static SAAbsorptionAttributes GetSaAbsorptionAttributes(Item item)
	{
		return item switch
		{
			BaseArmor armor => armor.AbsorptionAttributes,
			BaseJewel jewel => jewel.AbsorptionAttributes,
			BaseWeapon weapon => weapon.AbsorptionAttributes,
			BaseClothing clothing => clothing.SAAbsorptionAttributes,
			_ => null
		};
	}

	public static AosSkillBonuses GetAosSkillBonuses(Item item)
	{
		return item switch
		{
			BaseJewel jewel => jewel.SkillBonuses,
			BaseWeapon weapon => weapon.SkillBonuses,
			BaseArmor armor => armor.SkillBonuses,
			BaseTalisman talisman => talisman.SkillBonuses,
			Spellbook spellbook => spellbook.SkillBonuses,
			BaseQuiver quiver => quiver.SkillBonuses,
			BaseClothing clothing => clothing.SkillBonuses,
			_ => null
		};
	}

	public static NegativeAttributes GetNegativeAttributes(Item item)
	{
		return item switch
		{
			BaseWeapon weapon => weapon.NegativeAttributes,
			BaseArmor armor => armor.NegativeAttributes,
			BaseClothing clothing => clothing.NegativeAttributes,
			BaseJewel jewel => jewel.NegativeAttributes,
			BaseTalisman talisman => talisman.NegativeAttributes,
			Spellbook spellbook => spellbook.NegativeAttributes,
			_ => null
		};
	}

	public static int GetArtifactRarity(Item item)
	{
		return item is IArtifact artifact ? artifact.ArtifactRarity : 0;
	}
	/* Reforging Test:
	 * Powerful/Structural - Luck [30]
	 * 150: 14
	 * 140: 4
	 * 120: 2
	 * 100: 10
	 *
	*/
	#endregion

	#region Tables
	#region All

	private static readonly int[][] DexIntTable =
	{
		new[] { 3, 4, 4, 4, 5, 5, 5 },
		new[] { 4, 4, 5, 5, 5, 5, 5 },
		new[] { 5, 5, 5, 5, 5, 5, 5 },
		new[] { 4, 4, 5, 5, 5, 5, 5 },
		new[] { 4, 4, 5, 5, 5, 5, 5 },
		new[] { 5, 5, 5, 5, 5, 5, 5 },
	};

	private static readonly int[][] LowerStatReqTable =
	{
		new[] { 60, 70, 80, 100, 100, 100, 100 },
		new[] { 80, 100, 100, 100, 100, 100, 100 },
		new[] { 100, 100, 100, 100, 100, 100, 100 },
		new[] { 70, 100, 100, 100, 100, 100, 100 },
		new[] { 80, 100, 100, 100, 100, 100, 100 },
		new[] { 100, 100, 100, 100, 100, 100, 100 },
	};

	private static readonly int[][] SelfRepairTable =
	{
		new[] { 2, 4, 0, 0, 0, 0, 0 },
		new[] { 5, 5, 0, 0, 0, 0, 0 },
		new[] { 6, 7, 0, 0, 0, 0, 0 },
		new[] { 5, 5, 0, 0, 0, 0, 0 },
		new[] { 5, 5, 0, 0, 0, 0, 0 },
		new[] { 7, 7, 0, 0, 0, 0, 0 },
	};

	private static readonly int[][] DurabilityTable =
	{
		new[] { 90, 100, 0, 0, 0, 0, 0 },
		new[] { 110, 140, 0, 0, 0, 0, 0 },
		new[] { 150, 150, 0, 0, 0, 0, 0 },
		new[] { 100, 140, 0, 0, 0, 0, 0 },
		new[] { 110, 140, 0, 0, 0, 0, 0 },
		new[] { 150, 150, 0, 0, 0, 0, 0 },
	};

	private static readonly int[][] ResistTable =
	{
		new[] { 10, 15, 15, 15, 20, 20, 20 },
		new[] { 15, 15, 15, 20, 20, 20, 20 },
		new[] { 20, 20, 20, 20, 20, 20, 20 },
		new[] { 20, 20, 20, 20, 20, 20, 20 },
		new[] { 15, 15, 20, 20, 20, 20, 20 },
		new[] { 20, 20, 20, 20, 20, 20, 20 },
	};

	private static readonly int[][] EaterTable =
	{
		new[] { 9, 12, 12, 15, 15, 15, 15 },
		new[] { 12, 15, 15, 15, 15, 15, 15 },
		new[] { 15, 15, 15, 15, 15, 15, 15 },
		new[] { 12, 15, 15, 15, 15, 15, 15 },
		new[] { 12, 15, 15, 15, 15, 15, 15 },
		new[] { 15, 15, 15, 15, 15, 15, 15 },
	};
	#endregion

	#region Weapon Tables

	private static readonly int[][] ElementalDamageTable =
	{
		new[] { 60, 70, 80, 100, 100, 100, 100 },
		new[] { 80, 100, 100, 100, 100, 100, 100 },
		new[] { 100, 100, 100, 100, 100, 100, 100 },
		Array.Empty<int>(),
		new[] { 100, 100, 100, 100, 100, 100, 100 },
		new[] { 100, 100, 100, 100, 100, 100, 100 },
	};

	// Hit magic, area, HLA
	private static readonly int[][] HitWeaponTable1 =
	{
		new[] { 30, 50, 50, 60, 70, 70, 70 },
		new[] { 50, 60, 70, 70, 70, 70, 70 },
		new[] { 70, 70, 70, 70, 70, 70, 70 },
		Array.Empty<int>(),
		new[] { 50, 60, 70, 70, 70, 70, 70 },
		new[] { 70, 70, 70, 70, 70, 70, 70 },
	};

	// hit fatigue, mana drain, HLD
	private static readonly int[][] HitWeaponTable2 =
	{
		new[] { 30, 40, 50, 50, 60, 70, 70 },
		new[] { 50, 50, 50, 60, 70, 70, 70 },
		new[] { 50, 60, 70, 70, 70, 70, 70 },
		Array.Empty<int>(),
		new[] { 50, 50, 50, 60, 70, 70, 70 },
		new[] { 70, 70, 70, 70, 70, 70, 70 },
	};

	private static readonly int[][] WeaponVelocityTable =
	{
		new[] { 25, 35, 40, 40, 40, 45, 50 },
		new[] { 40, 40, 40, 45, 50, 50, 50 },
		new[] { 40, 45, 50, 50, 50, 50, 50 },
		Array.Empty<int>(),
		new[] { 40, 40, 40, 45, 50, 50, 50 },
		new[] { 45, 50, 50, 50, 50, 50, 50 },
	};

	private static readonly int[][] HitsAndManaLeechTable =
	{
		new[] { 15, 25, 25, 30, 35, 35, 35 },
		new[] { 25, 25, 30, 35, 35, 35, 35 },
		new[] { 30, 35, 35, 35, 35, 35, 35 },
		Array.Empty<int>(),
		new[] { 25, 25, 30, 35, 35, 35, 35 },
		new[] { 35, 35, 35, 35, 35, 35, 35 },
	};

	public static readonly int[][] HitStamLeechTable =
	{
		new[] { 30, 50, 50, 60, 70, 70, 70 },
		new[] { 50, 60, 70, 70, 70, 70, 70 },
		new[] { 70, 70, 70, 70, 70, 70, 70 },
		Array.Empty<int>(),
		new[] { 50, 60, 70, 70, 70, 70, 70 },
		new[] { 70, 70, 70, 70, 70, 70, 70 },
	};

	private static readonly int[][] LuckTable =
	{
		new[] { 80, 100, 100, 120, 140, 150, 150 },
		new[] { 100, 120, 140, 150, 150, 150, 150 },
		new[] { 130, 150, 150, 150, 150, 150, 150 },
		new[] { 100, 120, 140, 150, 150, 150, 150 },
		new[] { 100, 120, 140, 150, 150, 150, 150 },
		new[] { 150, 150, 150, 150, 150, 150, 150 },
	};

	private static readonly int[][] MageWeaponTable =
	{
		new[] { 25, 20, 20, 20, 20, 15, 15 },
		new[] { 20, 20, 20, 15, 15, 15, 15 },
		new[] { 20, 15, 15, 15, 15, 15, 15 },
		Array.Empty<int>(),
		new[] { 20, 20, 20, 15, 15, 15, 15 },
		new[] { 15, 15, 15, 15, 15, 15, 15 },
	};

	private static readonly int[][] WeaponRegenTable =
	{
		new[] { 2, 3, 6, 6, 6, 6, 6 },
		new[] { 3, 6, 6, 6, 6, 6, 6 },
		new[] { 6, 6, 6, 6, 6, 9, 9 },
		Array.Empty<int>(),
		new[] { 3, 6, 6, 6, 6, 6, 9 },
		new[] { 6, 9, 9, 9, 9, 9, 9 },
	};

	private static readonly int[][] WeaponHitsTable =
	{
		new[] { 2, 3, 3, 3, 4, 4, 4 },
		new[] { 3, 3, 4, 4, 4, 4, 4 },
		new[] { 4, 4, 4, 4, 4, 4, 4 },
		Array.Empty<int>(),
		new[] { 3, 3, 4, 4, 4, 4, 4 },
		new[] { 4, 4, 4, 4, 4, 4, 4 },
	};

	private static readonly int[][] WeaponStamManaLmcTable =
	{
		new[] { 2, 4, 4, 4, 5, 5, 5 },
		new[] { 4, 4, 5, 5, 5, 5, 5 },
		new[] { 5, 5, 5, 5, 5, 5, 5 },
		Array.Empty<int>(),
		new[] { 4, 4, 5, 5, 5, 5, 5 },
		new[] { 5, 5, 5, 5, 5, 5, 5 },
	};

	private static readonly int[][] WeaponStrTable =
	{
		new[] { 2, 4, 4, 4, 5, 5, 5 },
		new[] { 4, 4, 5, 5, 5, 5, 5 },
		new[] { 5, 5, 5, 5, 5, 5, 5 },
		Array.Empty<int>(),
		new[] { 4, 4, 5, 5, 5, 5, 5 },
		new[] { 5, 5, 5, 5, 5, 5, 5 },
	};

	private static readonly int[][] WeaponHciTable =
	{
		new[] { 5, 10, 15, 15, 15, 20, 20 },
		new[] { 15, 15, 15, 20, 20, 20, 20 },
		new[] { 15, 20, 20, 20, 20, 20, 20 },
		Array.Empty<int>(),
		new[] { 15, 15, 20, 20, 20, 20, 20 },
		new[] { 20, 20, 20, 20, 20, 20, 20 },
	};

	private static readonly int[][] WeaponDciTable =
	{
		new[] { 10, 15, 15, 15, 20, 20, 20 },
		new[] { 15, 15, 20, 20, 20, 20, 20 },
		new[] { 20, 20, 20, 20, 20, 20, 20 },
		Array.Empty<int>(),
		new[] { 15, 15, 20, 20, 20, 20, 20 },
		new[] { 20, 20, 20, 20, 20, 20, 20 },
	};

	private static readonly int[][] WeaponDamageTable =
	{
		new[] { 30, 50, 50, 60, 70, 70, 70 },
		new[] { 50, 60, 70, 70, 70, 70, 70 },
		new[] { 70, 70, 70, 70, 70, 70, 70 },
		Array.Empty<int>(),
		new[] { 50, 60, 70, 70, 70, 70, 70 },
		new[] { 70, 70, 70, 70, 70, 70, 70 },
	};

	private static readonly int[][] WeaponEnhancePots =
	{
		new[] { 5, 10, 10, 10, 10, 15, 15 },
		new[] { 10, 10, 10, 15, 15, 15, 15 },
		new[] { 10, 15, 15, 15, 15, 15, 15 },
		Array.Empty<int>(),
		new[] { 10, 10, 10, 15, 15, 15, 15 },
		new[] { 15, 15, 15, 15, 15, 15, 15 },
	};

	private static readonly int[][] WeaponWeaponSpeedTable =
	{
		new[] { 20, 30, 30, 35, 40, 40, 40 },
		new[] { 30, 35, 40, 40, 40, 40, 40 },
		new[] { 35, 40, 40, 40, 40, 40, 40 },
		Array.Empty<int>(),
		new[] { 30, 35, 40, 40, 40, 40, 40 },
		new[] { 40, 40, 40, 40, 40, 40, 40 },
	};
	#endregion

	#region Ranged Weapons

	private static readonly int[][] RangedLuckTable =
	{
		new[] { 90, 120, 120, 140, 170, 170, 170 },
		new[] { 120, 140, 160, 170, 170, 170, 170 },
		new[] { 160, 170, 170, 170, 170, 170, 170 },
		Array.Empty<int>(),
		new[] { 120, 140, 160, 170, 170, 170, 170 },
		new[] { 170, 170, 170, 170, 170, 170, 170 },
	};

	private static readonly int[][] RangedHciTable =
	{
		new[] { 15, 25, 25, 30, 35, 35, 35 },
		new[] { 25, 30, 35, 35, 35, 35, 35 },
		new[] { 35, 35, 35, 35, 35, 35, 35 },
		Array.Empty<int>(),
		new[] { 25, 25, 30, 35, 35, 35, 35 },
		new[] { 35, 35, 35, 35, 35, 35, 35 },
	};

	private static readonly int[][] RangedDciTable =
	{
		Array.Empty<int>(),
		Array.Empty<int>(),
		Array.Empty<int>(),
		Array.Empty<int>(),
		new[] { 25, 25, 30, 35, 35, 35, 35 },
		new[] { 35, 35, 35, 35, 35, 35, 35 },
	};
	#endregion

	#region Armor Tables

	private static readonly int[][] LowerRegTable =
	{
		new[] { 10, 20, 20, 20, 25, 25, 25 },
		new[] { 20, 20, 25, 25, 25, 25, 25 },
		new[] { 25, 25, 25, 25, 25, 25, 25 },
		new[] { 20, 20, 25, 25, 25, 25, 25 },
		new[] { 20, 20, 25, 25, 25, 25, 25 },
		new[] { 25, 25, 25, 25, 25, 25, 25 },
	};

	private static readonly int[][] ArmorHitsTable =
	{
		new[] { 3, 5, 5, 6, 7, 7, 7 },
		new[] { 5, 6, 7, 7, 7, 7, 7 },
		new[] { 7, 7, 7, 7, 7, 7, 7 },
		new[] { 5, 5, 6, 7, 7, 7, 7 },
		new[] { 5, 6, 7, 7, 7, 7, 7 },
		new[] { 7, 7, 7, 7, 7, 7, 7 },
	};

	private static readonly int[][] ArmorStrTable =
	{
		new[] { 3, 4, 4, 4, 5, 5, 5 },
		new[] { 4, 4, 5, 5, 5, 5, 5 },
		new[] { 5, 5, 5, 5, 5, 5, 5 },
		new[] { 4, 4, 5, 5, 5, 5, 5 },
		new[] { 4, 4, 5, 5, 5, 5, 5 },
		new[] { 5, 5, 5, 5, 5, 5, 5 },
	};

	private static readonly int[][] ArmorRegenTable =
	{
		new[] { 2, 3, 3, 3, 4, 4, 4 },
		new[] { 3, 3, 4, 4, 4, 4, 4 },
		new[] { 4, 4, 4, 4, 4, 4, 4 },
		new[] { 3, 3, 4, 4, 4, 4, 4 },
		new[] { 3, 3, 4, 4, 4, 4, 4 },
		new[] { 4, 4, 4, 4, 4, 4, 4 },
	};

	private static readonly int[][] ArmorStamManaLmcTable =
	{
		new[] { 4, 8, 8, 8, 10, 10, 10 },
		new[] { 8, 8, 10, 10, 10, 10, 10 },
		new[] { 10, 10, 10, 10, 10, 10, 10 },
		new[] { 8, 8, 10, 10, 10, 10, 10 },
		new[] { 8, 8, 10, 10, 10, 10, 10 },
		new[] { 10, 10, 10, 10, 10, 10, 10 },
	};

	private static readonly int[][] ArmorEnhancePotsTable =
	{
		new[] { 2, 2, 3, 3, 3, 3, 3 },
		new[] { 3, 3, 3, 3, 3, 3, 3 },
		new[] { 3, 3, 3, 3, 3, 3, 3 },
		new[] { 3, 3, 3, 3, 3, 3, 3 },
		new[] { 3, 3, 3, 3, 3, 3, 3 },
		new[] { 3, 3, 3, 3, 3, 3, 3 },
	};

	private static readonly int[][] ArmorHcidciTable =
	{
		new[] { 4, 4, 5, 5, 5, 5, 5 },
		new[] { 5, 5, 5, 5, 5, 5, 5 },
		new[] { 5, 5, 5, 5, 5, 5, 5 },
		new[] { 5, 5, 5, 5, 5, 5, 5 },
		new[] { 5, 5, 5, 5, 5, 5, 5 },
		new[] { 5, 5, 5, 5, 5, 5, 5 },
	};

	private static readonly int[][] ArmorCastingFocusTable =
	{
		new[] { 1, 2, 2, 2, 3, 3, 3 },
		new[] { 2, 2, 3, 3, 3, 3, 3 },
		new[] { 3, 3, 3, 3, 3, 3, 3 },
		new[] { 2, 2, 3, 3, 3, 3, 3 },
		new[] { 2, 2, 3, 3, 3, 3, 3 },
		new[] { 3, 3, 3, 3, 3, 3, 3 },
	};

	private static readonly int[][] ShieldWeaponSpeedTable =
	{
		new[] { 5, 5, 5, 5, 10, 10, 10 },
		new[] { 5, 5, 10, 10, 10, 10, 10 },
		new[] { 10, 10, 10, 10, 10, 10, 10 },
		Array.Empty<int>(),
		new[] { 5, 5, 10, 10, 10, 10, 10 },
		new[] { 10, 10, 10, 10, 10, 10, 10 },
	};

	private static readonly int[][] ShieldSoulChargeTable =
	{
		new[] { 15, 20, 20, 20, 25, 25, 25 },
		new[] { 20, 20, 25, 30, 30, 30, 30 },
		new[] { 25, 30, 30, 30, 30, 30, 30 },
		Array.Empty<int>(),
		new[] { 20, 20, 25, 30, 30, 30, 30 },
		new[] { 25, 30, 30, 30, 30, 30, 30 },
	};
	#endregion
	#endregion
}

public class RunicReforgingTarget : Target
{
	private readonly BaseRunicTool m_Tool;

	public RunicReforgingTarget(BaseRunicTool tool)
		: base(-1, false, TargetFlags.None)
	{
		m_Tool = tool;
	}

	protected override void OnTarget(Mobile from, object targeted)
	{
		if (targeted is not Item item || !BaseTool.CheckAccessible(m_Tool, from, true))
			return;

		switch (item)
		{
			case BaseRunicTool tool when tool.IsChildOf(from.Backpack):
			{
				if (item == m_Tool)
					from.SendLocalizedMessage(1010087); // You cannot use that!
				else if (tool.GetType() != m_Tool.GetType())
					from.SendLocalizedMessage(1152274); // You may only combine runic tools of the same type.
				else if (tool.Resource != m_Tool.Resource)
					from.SendLocalizedMessage(1152275); // You may only combine runic tools of the same material.
				else if (m_Tool.UsesRemaining + tool.UsesRemaining > 100)
					from.SendLocalizedMessage(1152276); // The combined charges of the two tools cannot exceed 100.
				else
				{
					m_Tool.UsesRemaining += tool.UsesRemaining;
					tool.Delete();

					from.SendLocalizedMessage(1152278); // You combine the runic tools, consolidating their Uses Remaining.
				}

				break;
			}
			case BaseRunicTool:
				from.SendLocalizedMessage(1152277); // Both tools must be in your backpack in order to combine them.
				break;
			case ICombatEquipment when item.IsChildOf(from.Backpack):
			{
				if (RunicReforging.CanReforge(from, item, m_Tool.CraftSystem))
				{
					from.SendGump(new RunicReforgingGump(from, item, m_Tool));
				}

				break;
			}
			case ICombatEquipment:
				from.SendLocalizedMessage(1152271); // The item must be in your backpack to re-forge it.
				break;
			default:
				from.SendLocalizedMessage(1152113); // You cannot reforge that item.
				break;
		}
	}
}
