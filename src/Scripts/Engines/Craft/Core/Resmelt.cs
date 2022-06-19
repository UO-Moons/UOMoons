using Server.Items;
using Server.Targeting;
using System;

namespace Server.Engines.Craft;

public enum SmeltResult
{
	Success,
	Invalid,
	NoSkill
}

public class Resmelt
{
	public static void Do(Mobile from, CraftSystem craftSystem, ITool tool)
	{
		int num = craftSystem.CanCraft(from, tool, null);

		if (num > 0 && num != 1044267)
		{
			from.SendGump(new CraftGump(from, craftSystem, tool, num));
		}
		else
		{
			from.Target = new ResmeltTarget(craftSystem, tool);
			from.SendLocalizedMessage(1044273); // Target an item to recycle.
		}
	}

	private class ResmeltTarget : Target
	{
		private readonly CraftSystem _mCraftSystem;
		private readonly ITool _mTool;
		public ResmeltTarget(CraftSystem craftSystem, ITool tool)
			: base(2, false, TargetFlags.None)
		{
			_mCraftSystem = craftSystem;
			_mTool = tool;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			int num = _mCraftSystem.CanCraft(from, _mTool, null);

			if (num > 0)
			{
				if (num == 1044267)
				{

					DefBlacksmithy.CheckAnvilAndForge(from, 2, out bool anvil, out bool forge);

					if (!anvil)
					{
						num = 1044266; // You must be near an anvil
					}
					else if (!forge)
					{
						num = 1044265; // You must be near a forge.
					}
				}

				from.SendGump(new CraftGump(from, _mCraftSystem, _mTool, num));
			}
			else
			{
				SmeltResult result = SmeltResult.Invalid;
				bool isStoreBought = false;
				switch (targeted)
				{
					case BaseArmor armor:
						result = Resmelt(from, armor, armor.Resource);
						isStoreBought = !armor.PlayerConstructed;
						break;
					case BaseWeapon weapon:
						result = Resmelt(from, weapon, weapon.Resource);
						isStoreBought = !weapon.PlayerConstructed;
						break;
					case DragonBardingDeed deed:
						result = Resmelt(from, deed, deed.Resource);
						break;
				}

				var message = result switch
				{
					SmeltResult.NoSkill => 1044269,
					SmeltResult.Success => isStoreBought ? 500418 : 1044270,
					_ => 1044272,
				};
				from.SendGump(new CraftGump(from, _mCraftSystem, _mTool, message));
			}
		}

		private SmeltResult Resmelt(Mobile from, Item item, CraftResource resource)
		{
			try
			{
				if (Ethics.Ethic.IsImbued(item))
				{
					return SmeltResult.Invalid;
				}

				if (CraftResources.GetType(resource) != CraftResourceType.Metal)
				{
					return SmeltResult.Invalid;
				}

				CraftResourceInfo info = CraftResources.GetInfo(resource);

				if (info == null || info.ResourceTypes.Length == 0)
				{
					return SmeltResult.Invalid;
				}

				CraftItem craftItem = _mCraftSystem.CraftItems.SearchFor(item.GetType());

				if (craftItem == null || craftItem.Resources.Count == 0)
				{
					return SmeltResult.Invalid;
				}

				CraftRes craftResource = craftItem.Resources.GetAt(0);

				if (craftResource.Amount < 2)
				{
					return SmeltResult.Invalid; // Not enough metal to resmelt
				}

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

				double skill = Math.Max(from.Skills[SkillName.Mining].Value, from.Skills[SkillName.Blacksmith].Value);

				if (difficulty > skill)
				{
					return SmeltResult.NoSkill;
				}

				Type resourceType = info.ResourceTypes[0];
				Item ingot = (Item)Activator.CreateInstance(resourceType);

				if (item is DragonBardingDeed or BaseArmor {PlayerConstructed: true} or BaseWeapon
				    {
					    PlayerConstructed: true
				    } or BaseClothing
				    {
					    PlayerConstructed: true
				    })
				{
					if (ingot != null) ingot.Amount = (int) (craftResource.Amount * .66);
				}
				else
				{
					if (ingot != null) ingot.Amount = 1;
				}

				item.Delete();
				from.AddToBackpack(ingot);

				from.PlaySound(0x2A);
				from.PlaySound(0x240);
				return SmeltResult.Success;
			}
			catch
			{
				// ignored
			}

			return SmeltResult.Invalid;
		}
	}
}
