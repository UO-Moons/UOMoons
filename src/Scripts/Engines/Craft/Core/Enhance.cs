using Server.Items;
using Server.Mobiles;
using Server.Targeting;
using System;
using System.Collections.Generic;

namespace Server.Engines.Craft;

public enum EnhanceResult
{
	None,
	NotInBackpack,
	BadItem,
	BadResource,
	AlreadyEnhanced,
	Success,
	Failure,
	Broken,
	NoResources,
	NoSkill,
	Enchanted
}

public class Enhance
{
	private static readonly Dictionary<Type, CraftSystem> SpecialTable;

	private static bool IsSpecial(IEntity item, CraftSystem system)
	{
		foreach (KeyValuePair<Type, CraftSystem> kvp in SpecialTable)
		{
			if (kvp.Key == item.GetType() && kvp.Value == system)
			{
				return true;
			}
		}

		return false;
	}

	private static bool CanEnhance(IEntity item)
	{
		return item is BaseArmor or BaseWeapon or FishingPole;
	}

	public static EnhanceResult Invoke(Mobile from, CraftSystem craftSystem, ITool tool, Item item, CraftResource resource, Type resType, ref object resMessage)
	{
		if (item == null)
		{
			return EnhanceResult.BadItem;
		}

		if (!item.IsChildOf(from.Backpack))
		{
			return EnhanceResult.NotInBackpack;
		}

		IResource ires = item as IResource;

		if (!CanEnhance(item) || ires == null)
		{
			return EnhanceResult.BadItem;
		}

		if (item is IArcaneEquip {IsArcane: true})
		{
			return EnhanceResult.BadItem;
		}

		if (CraftResources.IsStandard(resource))
		{
			return EnhanceResult.BadResource;
		}

		int num = craftSystem.CanCraft(from, tool, item.GetType());

		if (num > 0)
		{
			resMessage = num;
			return EnhanceResult.None;
		}

		CraftItem craftItem = craftSystem.CraftItems.SearchFor(item.GetType());

		if (IsSpecial(item, craftSystem))
		{
			craftItem = craftSystem.CraftItems.SearchForSubclass(item.GetType());
		}

		if (craftItem == null || craftItem.Resources.Count == 0)
		{
			return EnhanceResult.BadItem;
		}

		#region Mondain's Legacy
		if (craftItem.ForceNonExceptional)
		{
			return EnhanceResult.BadItem;
		}
		#endregion

		bool allRequiredSkills = false;
		if (craftItem.GetSuccessChance(from, resType, craftSystem, false, ref allRequiredSkills) <= 0.0)
		{
			return EnhanceResult.NoSkill;
		}

		CraftResourceInfo info = CraftResources.GetInfo(resource);

		if (info == null || info.ResourceTypes.Length == 0)
		{
			return EnhanceResult.BadResource;
		}

		CraftAttributeInfo attributes = info.AttributeInfo;

		if (attributes == null)
		{
			return EnhanceResult.BadResource;
		}

		int resHue = 0, maxAmount = 0;

		if (!craftItem.ConsumeRes(from, resType, craftSystem, ref resHue, ref maxAmount, ConsumeType.None, ref resMessage))
		{
			return EnhanceResult.NoResources;
		}

		if (!CraftResources.IsStandard(ires.Resource))
		{
			return EnhanceResult.AlreadyEnhanced;
		}

		if (craftSystem is DefBlacksmithy)
		{
			if (from.FindItemOnLayer(Layer.OneHanded) is AncientSmithyHammer hammer)
			{
				hammer.UsesRemaining--;
				if (hammer.UsesRemaining < 1)
				{
					hammer.Delete();
				}
			}
		}

		int phys = 0, fire = 0, cold = 0, pois = 0, nrgy = 0;
		int dura = 0, luck = 0, lreq = 0, dinc = 0;
		int baseChance = 0;

		bool physBonus = false;
		bool fireBonus = false;
		bool coldBonus = false;
		bool nrgyBonus = false;
		bool poisBonus = false;
		bool duraBonus = false;
		bool luckBonus = false;
		bool lreqBonus = false;
		bool dincBonus = false;

		switch (item)
		{
			case BaseWeapon weapon:
				baseChance = 20;

				dura = weapon.MaxHitPoints;
				luck = weapon.Attributes.Luck;
				lreq = weapon.WeaponAttributes.LowerStatReq;
				dinc = weapon.Attributes.WeaponDamage;

				fireBonus = (attributes.WeaponFireDamage > 0);
				coldBonus = (attributes.WeaponColdDamage > 0);
				nrgyBonus = (attributes.WeaponEnergyDamage > 0);
				poisBonus = (attributes.WeaponPoisonDamage > 0);

				duraBonus = (attributes.WeaponDurability > 0);
				luckBonus = (attributes.WeaponLuck > 0);
				lreqBonus = (attributes.WeaponLowerRequirements > 0);
				dincBonus = (dinc > 0);
				break;
			case BaseArmor baseArmor:
			{
				BaseArmor armor = baseArmor;

				baseChance = 20;

				phys = armor.PhysicalResistance;
				fire = armor.FireResistance;
				cold = armor.ColdResistance;
				pois = armor.PoisonResistance;
				nrgy = armor.EnergyResistance;

				dura = armor.MaxHitPoints;
				luck = armor.Attributes.Luck;
				lreq = armor.ArmorAttributes.LowerStatReq;

				physBonus = (attributes.ArmorPhysicalResist > 0);
				fireBonus = (attributes.ArmorFireResist > 0);
				coldBonus = (attributes.ArmorColdResist > 0);
				nrgyBonus = (attributes.ArmorEnergyResist > 0);
				poisBonus = (attributes.ArmorPoisonResist > 0);

				duraBonus = (attributes.ArmorDurability > 0);
				luckBonus = (attributes.ArmorLuck > 0);
				lreqBonus = (attributes.ArmorLowerRequirements > 0);
				dincBonus = false;
				break;
			}
			case FishingPole fishingPole:
			{
				FishingPole pole = fishingPole;

				baseChance = 20;

				//luck = pole.AOSAttributes.Luck;

				luckBonus = attributes.ArmorLuck > 0;
				lreqBonus = attributes.ArmorLowerRequirements > 0;
				dincBonus = false;
				break;
			}
		}

		int skill = from.Skills[craftSystem.MainSkill].Fixed / 10;

		if (skill >= 100)
		{
			baseChance -= (skill - 90) / 10;
		}

		EnhanceResult res = EnhanceResult.Success;

		PlayerMobile user = from as PlayerMobile;

		if (physBonus)
		{
			CheckResult(ref res, baseChance + phys);
		}

		if (fireBonus)
		{
			CheckResult(ref res, baseChance + fire);
		}

		if (coldBonus)
		{
			CheckResult(ref res, baseChance + cold);
		}

		if (nrgyBonus)
		{
			CheckResult(ref res, baseChance + nrgy);
		}

		if (poisBonus)
		{
			CheckResult(ref res, baseChance + pois);
		}

		if (duraBonus)
		{
			CheckResult(ref res, baseChance + (dura / 40));
		}

		if (luckBonus)
		{
			CheckResult(ref res, baseChance + 10 + (luck / 2));
		}

		if (lreqBonus)
		{
			CheckResult(ref res, baseChance + (lreq / 4));
		}

		if (dincBonus)
		{
			CheckResult(ref res, baseChance + (dinc / 4));
		}

		//if (user.NextEnhanceSuccess)
		//{
		//   user.NextEnhanceSuccess = false;
		//    user.SendLocalizedMessage(1149969); // The magical aura that surrounded you disipates and you feel that your item enhancement chances have returned to normal.
		//   res = EnhanceResult.Success;
		//}

		switch (res)
		{
			case EnhanceResult.Broken:
			{
				if (!craftItem.ConsumeRes(from, resType, craftSystem, ref resHue, ref maxAmount, ConsumeType.Half, ref resMessage))
				{
					return EnhanceResult.NoResources;
				}

				item.Delete();
				break;
			}
			case EnhanceResult.Success:
			{
				if (!craftItem.ConsumeRes(from, resType, craftSystem, ref resHue, ref maxAmount, ConsumeType.All, ref resMessage))
				{
					return EnhanceResult.NoResources;
				}

				if (item is IResource resource1)
				{
					resource1.Resource = resource;
				}

				if (item is BaseWeapon weapon)
				{
					weapon.DistributeMaterialBonus(attributes);

					int hue = weapon.GetElementalDamageHue();

					if (hue > 0)
					{
						weapon.Hue = hue;
					}
				}
				else if (item is BaseArmor armor)
				{
					armor.DistributeMaterialBonus(attributes);
				}
				//else if (item is FishingPole)
				//{
				//    ((FishingPole)item).DistributeMaterialBonus(attributes);
				//}
				break;
			}
			case EnhanceResult.Failure:
			{
				if (!craftItem.ConsumeRes(from, resType, craftSystem, ref resHue, ref maxAmount, ConsumeType.Half, ref resMessage))
				{
					return EnhanceResult.NoResources;
				}

				break;
			}
		}

		return res;
	}

	public static void CheckResult(ref EnhanceResult res, int chance)
	{
		if (res != EnhanceResult.Success)
		{
			return; // we've already failed..
		}

		int random = Utility.Random(100);

		if (10 > random)
		{
			res = EnhanceResult.Failure;
		}
		else if (chance > random)
		{
			res = EnhanceResult.Broken;
		}
	}

	public static void BeginTarget(Mobile from, CraftSystem craftSystem, ITool tool)
	{
		CraftContext context = craftSystem.GetContext(from);
		PlayerMobile user = from as PlayerMobile;

		if (context == null)
		{
			return;
		}

		int lastRes = context.LastResourceIndex;
		CraftSubResCol subRes = craftSystem.CraftSubRes;

		if (lastRes >= 0 && lastRes < subRes.Count)
		{
			CraftSubRes res = subRes.GetAt(lastRes);

			if (from.Skills[craftSystem.MainSkill].Value < res.RequiredSkill)
			{
				from.SendGump(new CraftGump(from, craftSystem, tool, res.Message));
			}
			else
			{
				CraftResource resource = CraftResources.GetFromType(res.ItemType);

				if (resource != CraftResource.None)
				{
					from.Target = new InternalTarget(craftSystem, tool, res.ItemType, resource);

					//if (user.NextEnhanceSuccess)
					//{
					//    from.SendLocalizedMessage(1149869, "100"); // Target an item to enhance with the properties of your selected material (Success Rate: ~1_VAL~%).
					//}
					//else
					//{
					from.SendLocalizedMessage(1061004); // Target an item to enhance with the properties of your selected material.
					//}
				}
				else
				{
					from.SendGump(new CraftGump(from, craftSystem, tool, 1061010)); // You must select a special material in order to enhance an item with its properties.
				}
			}
		}
		else
		{
			from.SendGump(new CraftGump(from, craftSystem, tool, 1061010)); // You must select a special material in order to enhance an item with its properties.
		}
	}

	private class InternalTarget : Target
	{
		private readonly CraftSystem _mCraftSystem;
		private readonly ITool _mTool;
		private readonly Type _mResourceType;
		private readonly CraftResource _mResource;

		public InternalTarget(CraftSystem craftSystem, ITool tool, Type resourceType, CraftResource resource)
			: base(2, false, TargetFlags.None)
		{
			_mCraftSystem = craftSystem;
			_mTool = tool;
			_mResourceType = resourceType;
			_mResource = resource;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (targeted is Item item)
			{
				object message = null;
				EnhanceResult res = Enhance.Invoke(from, _mCraftSystem, _mTool, item, _mResource, _mResourceType, ref message);

				message = res switch
				{
					EnhanceResult.NotInBackpack => 1061005,
					EnhanceResult.AlreadyEnhanced => 1061012,
					EnhanceResult.BadItem => 1061011,
					EnhanceResult.BadResource => 1061010,
					EnhanceResult.Broken => 1061080,
					EnhanceResult.Failure => 1061082,
					EnhanceResult.Success => 1061008,
					EnhanceResult.NoSkill => 1044153,
					EnhanceResult.Enchanted => 1080131,
					_ => message
				};

				from.SendGump(new CraftGump(from, _mCraftSystem, _mTool, message));
			}
		}
	}
}
