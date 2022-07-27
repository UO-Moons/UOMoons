using Server.Engines.Harvest;
using Server.Engines.Quests;
using Server.Engines.Quests.Hag;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Targets;

public class BladedItemTarget : Target
{
	private readonly Item m_Item;

	public BladedItemTarget(Item item) : base(2, false, TargetFlags.None)
	{
		m_Item = item;
	}

	protected override void OnTargetOutOfRange(Mobile from, object targeted)
	{
		if (targeted is UnholyBone bone && from.InRange(bone, 12))
			bone.Carve(from, m_Item);
		else
			base.OnTargetOutOfRange(from, targeted);
	}

	protected override void OnTarget(Mobile from, object targeted)
	{
		if (m_Item.Deleted)
			return;

		switch (targeted)
		{
			case ICarvable carvable:
				carvable.Carve(from, m_Item);
				break;
			case SwampDragon { HasBarding: true } dragon:
			{
				if (!dragon.Controlled || dragon.ControlMaster != from)
					from.SendLocalizedMessage(1053022); // You cannot remove barding from a swamp dragon you do not own.
				else
					dragon.HasBarding = false;
				break;
			}
			default:
			{
				if (targeted is StaticTarget { ItemID: 0xD15 or 0xD16 }) // red mushroom
				{
					if (from is PlayerMobile player)
					{
						QuestSystem qs = player.Quest;

						if (qs is WitchApprenticeQuest)
						{
							if (qs.FindObjective(typeof(FindIngredientObjective)) is FindIngredientObjective {Completed: false, Ingredient: Ingredient.RedMushrooms} obj)
							{
								player.SendLocalizedMessage(1055036); // You slice a red cap mushroom from its stem.
								obj.Complete();
								return;
							}
						}
					}
				}

				HarvestSystem system = Lumberjacking.System;
				HarvestDefinition def = Lumberjacking.System.Definition;

				if (!system.GetHarvestDetails(from, m_Item, targeted, out var tileId, out Map map, out Point3D loc))
				{
					from.SendLocalizedMessage(500494); // You can't use a bladed item on that!
				}
				else if (!def.Validate(tileId))
				{
					from.SendLocalizedMessage(500494); // You can't use a bladed item on that!
				}
				else
				{
					HarvestBank bank = def.GetBank(map, loc.X, loc.Y);

					if (bank == null)
						return;

					if (bank.Current < 5)
					{
						from.SendLocalizedMessage(500493); // There's not enough wood here to harvest.
					}
					else
					{
						bank.Consume(5, from);

						Item item = new Kindling();

						if (from.PlaceInBackpack(item))
						{
							from.SendLocalizedMessage(500491); // You put some kindling into your backpack.
							from.SendLocalizedMessage(500492); // An axe would probably get you more wood.
						}
						else
						{
							from.SendLocalizedMessage(500490); // You can't place any kindling into your backpack!

							item.Delete();
						}
					}
				}

				break;
			}
		}
	}
}
