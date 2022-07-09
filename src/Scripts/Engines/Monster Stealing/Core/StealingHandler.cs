using Server.Items;
using Server.Mobiles;
using Server.Regions;
using System;
using Server.Engines.Champions;

namespace Server.Engines.CreatureStealing;

class StealingHandler
{
	private static readonly Type[] SpecialItemList =
	{
		typeof(SeedOfLife),
		typeof(BalmOfStrength),
		typeof(BalmOfWisdom),
		typeof(BalmOfSwiftness),
		typeof(ManaDraught),
		typeof(BalmOfProtection),
		typeof(StoneSkinLotion),
		typeof(GemOfSalvation),
		typeof(LifeShieldLotion),
		typeof(SmugglersLantern),
		typeof(SmugglersToolBox)
	};

	public static void HandleSteal(BaseCreature bc, PlayerMobile thief, ref Item stolen)
	{
		if (!CheckLocation(thief, bc))
		{
			return;
		}

		double stealing = thief.Skills.Stealing.Value;

		if (stealing >= 100)
		{
			int chance = GetStealingChance(thief, bc, stealing);

			if ((Utility.Random(100) + 1) <= chance)
			{
				thief.SendLocalizedMessage(1094947);//You successfully steal a special item from the creature!

				Item item;

				//if (bc is ExodusZealot)
				//{
				//	item = Activator.CreateInstance(ExodusChest.RituelItem[Utility.Random(ExodusChest.RituelItem.Length)]) as Item;
				//}
				//else
				//{
					item = Activator.CreateInstance(SpecialItemList[Utility.Random(SpecialItemList.Length - 2)]) as Item;
				//}

				stolen = item;
			}
		}
	}

	public static void HandleSmugglersEdgeSteal(BaseCreature from, PlayerMobile thief)
	{
		if (from.HasBeenStolen || !CheckLocation(thief, from))
			return;

		if (0.05 > Utility.RandomDouble())
		{
			double tempSkill = Utility.RandomMinMax(80, 110);
			double realSkill = thief.Skills[SkillName.Stealing].Value;

			if (realSkill > tempSkill)
				tempSkill = realSkill;

			if (tempSkill > 100)
			{
				int chance = GetStealingChance(thief, from, tempSkill);

				switch (realSkill)
				{
					case <= 109.9:
						chance += 1;
						break;
					case <= 114.9:
						chance += 2;
						break;
					case >= 115.0:
						chance += 3;
						break;
				}

				if (chance >= Utility.Random(100))
				{
					if (Activator.CreateInstance(SpecialItemList[Utility.Random(SpecialItemList.Length)]) is Item item)
					{
						thief.AddToBackpack(item);

						thief.SendLocalizedMessage(1094947);//You successfully steal a special item from the creature!
					}
				}
				else
				{
					Container pack = from.Backpack;

					if (pack != null && pack.Items.Count > 0)
					{
						int randomIndex = Utility.Random(pack.Items.Count);

						Item stolen = TryStealItem(pack.Items[randomIndex], tempSkill);

						if (stolen != null)
						{
							thief.AddToBackpack(stolen);

							thief.SendLocalizedMessage(502724); // You succesfull steal the item.
						}
						else
						{
							thief.SendLocalizedMessage(502723); // You fail to steal the item.
						}
					}
				}

				from.HasBeenStolen = true;
			}
		}
	}

	private static bool CheckLocation(Mobile thief, Mobile from)
	{
		if (!((thief.Map == Map.Felucca && thief.Region is DungeonRegion) || thief.Region is ChampionSpawnRegion /*|| from is ExodusZealot*/))
		{
			return false;
		}

		return true;
	}

	private static int GetStealingChance(Mobile thief, Mobile from, double stealing)
	{
		int fame = from.Fame;

		fame = Math.Max(1, fame);
		fame = Math.Min(30000, fame);

		int chance = 0;

		switch (stealing)
		{
			case 120:
				chance += 10;
				break;
			case >= 110.1:
				chance += 8;
				break;
			case >= 100.1:
				chance += 5;
				break;
			case 100:
				chance += 2;
				break;
		}

		int level = (int)(40.0 / 29999.0 * fame - 40.0 / 29999.0);

		switch (level)
		{
			case >= 40:
				chance += 5;
				break;
			case >= 35:
				chance += 3;
				break;
			case >= 30:
				chance += 2;
				break;
			case >= 25:
				chance += 1;
				break;
		}

		return chance;
	}

	private static Item TryStealItem(Item toSteal, double skill)
	{
		Item stolen = null;
		double w = toSteal.Weight + toSteal.TotalWeight;

		if (w <= 10)
		{
			if (toSteal.Stackable && toSteal.Amount > 1)
			{
				int maxAmount = (int)((skill / 10.0) / toSteal.Weight);

				if (maxAmount < 1)
				{
					maxAmount = 1;
				}
				else if (maxAmount > toSteal.Amount)
				{
					maxAmount = toSteal.Amount;
				}

				int amount = Utility.RandomMinMax(1, maxAmount);

				if (amount >= toSteal.Amount)
				{
					int pileWeight = (int)Math.Ceiling(toSteal.Weight * toSteal.Amount);
					pileWeight *= 10;

					double chance = (skill - (pileWeight - 22.5)) / ((pileWeight + 27.5) - (pileWeight - 22.5));

					if (chance >= Utility.RandomDouble())
					{
						stolen = toSteal;
					}
				}
				else
				{
					int pileWeight = (int)Math.Ceiling(toSteal.Weight * amount);
					pileWeight *= 10;

					double chance = (skill - (pileWeight - 22.5)) / ((pileWeight + 27.5) - (pileWeight - 22.5));

					if (chance >= Utility.RandomDouble())
					{
						stolen = Mobile.LiftItemDupe(toSteal, toSteal.Amount - amount) ?? toSteal;
					}
				}
			}
			else
			{
				int iw = (int)Math.Ceiling(w);
				iw *= 10;

				double chance = (skill - (iw - 22.5)) / ((iw + 27.5) - (iw - 22.5));

				if (chance >= Utility.RandomDouble())
				{
					stolen = toSteal;
				}
			}

			if (stolen != null)
			{
				ItemFlags.SetTaken(stolen, true);
				ItemFlags.SetStealable(stolen, false);
				stolen.Movable = true;
			}
		}

		return stolen;
	}
}
