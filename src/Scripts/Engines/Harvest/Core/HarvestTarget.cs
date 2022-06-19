using Server.Engines.Quests;
using Server.Engines.Quests.Hag;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Engines.Harvest;

public class HarvestTarget : Target
{
	private readonly Item _mTool;
	private readonly HarvestSystem _mSystem;

	public HarvestTarget(Item tool, HarvestSystem system)
		: base(-1, true, TargetFlags.None)
	{
		_mTool = tool;
		_mSystem = system;
		Range = 3;

		DisallowMultis = true;
	}

	protected override void OnTarget(Mobile from, object targeted)
	{
		if (_mSystem is Mining)
		{
			if (targeted is StaticTarget {ItemID: 0xED3 or 0xEDF or 0xEE0 or 0xEE1 or 0xEE2 or 0xEE8})
				// grave
			{
				if (from is PlayerMobile player)
				{
					QuestSystem qs = player.Quest;

					if (qs is WitchApprenticeQuest)
					{
						if (qs.FindObjective(typeof(FindIngredientObjective)) is FindIngredientObjective
						    {
							    Completed: false, Ingredient: Ingredient.Bones
						    } obj)
						{
							player.SendLocalizedMessage(1055037); // You finish your grim work, finding some of the specific bones listed in the Hag's recipe.
							obj.Complete();

							return;
						}
					}
				}
			}
		}

		switch (_mSystem)
		{
			case Lumberjacking when targeted is IChopable chopable:
				chopable.OnChop(from);
				break;
			case Lumberjacking when targeted is IAxe o && _mTool is BaseAxe axe:
			{
				Item item = (Item)o;

				if (!item.IsChildOf(from.Backpack))
				{
					from.SendLocalizedMessage(1062334); // This item must be in your backpack to be used.
				}
				else if (o.Axe(from, axe))
				{
					from.PlaySound(0x13E);
				}

				break;
			}
			case Lumberjacking when targeted is ICarvable carvable:
				carvable.Carve(from, _mTool);
				break;
			case Lumberjacking when FurnitureAttribute.Check(targeted as Item):
				DestroyFurniture(from, (Item)targeted);
				break;
			case Mining when targeted is TreasureMap map:
				map.OnBeginDig(from);
				break;
			default:
			{
				// If we got here and we're lumberjacking then we didn't target something that can be done from the pack
				if (_mSystem is Lumberjacking && _mTool.Parent != from)
				{
					from.SendLocalizedMessage(500487); // The axe must be equipped for any serious wood chopping.
					return;
				}
				_mSystem.StartHarvesting(from, _mTool, targeted);
				break;
			}
		}
	}

	private static void DestroyFurniture(Mobile from, Item item)
	{
		if (!from.InRange(item.GetWorldLocation(), 3))
		{
			from.SendLocalizedMessage(500446); // That is too far away.
			return;
		}

		if (!item.IsChildOf(from.Backpack) && !item.Movable)
		{
			from.SendLocalizedMessage(500462); // You can't destroy that while it is here.
			return;
		}

		from.SendLocalizedMessage(500461); // You destroy the item.
		Effects.PlaySound(item.GetWorldLocation(), item.Map, 0x3B3);

		if (item is Container container)
		{
			if (container is TrapableContainer trapableContainer)
			{
				trapableContainer.ExecuteTrap(from);
			} container.Destroy();
		}
		else
		{
			item.Delete();
		}
	}
}
