using System;
using System.Collections.Generic;
using System.Linq;
using Server.Items;
using Server.Mobiles;

namespace Server.Misc
{
	public class LootHelpers
	{
		public static Type[] Artifacts { get; } = {
			typeof( CandelabraOfSouls ), typeof( GoldBricks ), typeof( PhillipsWoodenSteed ),
			typeof( ArcticDeathDealer ), typeof( BlazeOfDeath ), typeof( BurglarsBandana ),
			typeof( CavortingClub ), typeof( DreadPirateHat ),
			typeof( EnchantedTitanLegBone ), typeof( GwennosHarp ), typeof( IolosLute ),
			typeof( LunaLance ), typeof( NightsKiss ), typeof( NoxRangersHeavyCrossbow ),
			typeof( PolarBearMask ), typeof( VioletCourage ), typeof( HeartOfTheLion ),
			typeof( ColdBlood ), typeof( AlchemistsBauble )
		};

		private static Type[] ArtifactsLevelFiveToSeven { get; } =
		{
			/*typeof(ForgedPardon), typeof(ManaPhasingOrb), typeof(RunedSashOfWarding), typeof(SurgeShield)*/
		};

		private static Type[] ArtifactsLevelSeven { get; } = {
			typeof(CoffinPiece)/*, typeof(MasterSkeletonKey)*/
		};

		private static Type[] SosArtifacts { get; } = {
			/*typeof(AntiqueWeddingDress),
			typeof(KelpWovenLeggings),
			typeof(RunedDriftwoodBow),
			typeof(ValkyrieArmor)*/
		};

		private static Type[] SosDecor { get; } = {
			/*typeof(GrapeVine),
			typeof(LargeFishingNet)*/
		};

		public static Type[] ImbuingIngreds { get; } =
		{
			typeof(AbyssalCloth),   typeof(EssencePrecision), typeof(EssenceAchievement), typeof(EssenceBalance),
			typeof(EssenceControl), typeof(EssenceDiligence), typeof(EssenceDirection),   typeof(EssenceFeeling),
			typeof(EssenceOrder),   typeof(EssencePassion),   typeof(EssencePersistence), typeof(EssenceSingularity)
		};

		public static void Fill(ParagonChest cont, int level)
		{
			cont.TrapType = TrapType.ExplosionTrap;
			cont.TrapPower = level * 25;
			cont.TrapLevel = level;
			cont.Locked = true;

			cont.RequiredSkill = level switch
			{
				1 => 36,
				2 => 76,
				3 => 84,
				4 => 92,
				5 => 100,
				_ => cont.RequiredSkill
			};

			cont.LockLevel = cont.RequiredSkill - 10;
			cont.MaxLockLevel = cont.RequiredSkill + 40;

			cont.DropItem(new Gold(level * 200));

			for (int i = 0; i < level; ++i)
				cont.DropItem(Loot.RandomScroll(0, 63, SpellbookType.Regular));

			for (int i = 0; i < level * 2; ++i)
			{
				var item = Core.AOS ? Loot.RandomArmorOrShieldOrWeaponOrJewelry() : Loot.RandomArmorOrShieldOrWeapon();

				switch (item)
				{
					case BaseWeapon weapon:
					{
						if (Core.AOS)
						{
							GetRandomAosStats(out int attributeCount, out int min, out int max);
							BaseRunicTool.ApplyAttributesTo(weapon, attributeCount, min, max);
						}
						else
						{
							weapon.DamageLevel = (WeaponDamageLevel)Utility.Random(6);
							weapon.AccuracyLevel = (WeaponAccuracyLevel)Utility.Random(6);
							weapon.DurabilityLevel = (DurabilityLevel)Utility.Random(6);
						}

						cont.DropItem(item);
						break;
					}
					case BaseArmor armor:
					{
						if (Core.AOS)
						{
							GetRandomAosStats(out int attributeCount, out int min, out int max);
							BaseRunicTool.ApplyAttributesTo(armor, attributeCount, min, max);
						}
						else
						{
							armor.ProtectionLevel = (ArmorProtectionLevel)Utility.Random(6);
							armor.Durability = (DurabilityLevel)Utility.Random(6);
						}

						cont.DropItem(item);
						break;
					}
					case BaseHat hat:
					{
						if (Core.AOS)
						{
							GetRandomAosStats(out int attributeCount, out int min, out int max);
							BaseRunicTool.ApplyAttributesTo(hat, attributeCount, min, max);
						}

						cont.DropItem(item);
						break;
					}
					case BaseJewel jewel:
					{
						GetRandomAosStats(out int attributeCount, out int min, out int max);

						BaseRunicTool.ApplyAttributesTo(jewel, attributeCount, min, max);

						cont.DropItem(item);
						break;
					}
				}
			}

			for (int i = 0; i < level; i++)
			{
				Item item = Loot.RandomPossibleReagent();
				item.Amount = Utility.RandomMinMax(40, 60);
				cont.DropItem(item);
			}

			for (int i = 0; i < level; i++)
			{
				Item item = Loot.RandomGem();
				cont.DropItem(item);
			}

			cont.DropItem(new TreasureMap(level + 1, Utility.RandomBool() ? Map.Felucca : Map.Trammel));
		}

		public static void Fill(Mobile from, LockableContainer cont, int level, bool isSos)
		{
			Map map = from.Map;
			int luck = from is PlayerMobile mobile ? mobile.RealLuck : from.Luck;

			cont.Movable = false;
			cont.Locked = true;
			int count;

			if (level == 0)
			{
				cont.LockLevel = 0; // Can't be unlocked

				cont.DropItem(new Gold(Utility.RandomMinMax(50, 100)));

				if (Utility.RandomDouble() < 0.75)
					cont.DropItem(new TreasureMap(0, Map.Trammel));
			}
			else
			{
				cont.TrapType = TrapType.ExplosionTrap;
				cont.TrapPower = level * 25;
				cont.TrapLevel = level;

				cont.RequiredSkill = level switch
				{
					1 => 36,
					2 => 76,
					3 => 84,
					4 => 92,
					5 => 100,
					6 => 100,
					_ => cont.RequiredSkill
				};

				cont.LockLevel = cont.RequiredSkill - 10;
				cont.MaxLockLevel = cont.RequiredSkill + 40;

				//Publish 67 gold change
				cont.DropItem(Core.SA ? new Gold(isSos ? level * 10000 : level * 5000) : new Gold(level * 1000));

				#region Scrolls
				if (isSos)
				{
					count = level switch
					{
						0 => Utility.RandomMinMax(2, 5),
						1 => Utility.RandomMinMax(2, 5),
						2 => Utility.RandomMinMax(10, 15),
						_ => 20
					};
				}
				else
				{
					count = level * 5;
				}

				for (int i = 0; i < count; ++i)
					cont.DropItem(Loot.RandomScroll(0, 63, SpellbookType.Regular));

				#endregion

				if (Core.SA)
				{
					#region Magical Items
					double propsScale = 1.0;

					switch (level)
					{
						case 1:
							count = isSos ? Utility.RandomMinMax(2, 6) : 32;
							propsScale = 0.5625;
							break;
						case 2:
							count = isSos ? Utility.RandomMinMax(10, 15) : 40;
							propsScale = 0.6875;
							break;
						case 3:
							count = isSos ? Utility.RandomMinMax(15, 20) : 48;
							propsScale = 0.875;
							break;
						case 4:
							count = isSos ? Utility.RandomMinMax(15, 20) : 56;
							break;
						case 5:
							count = isSos ? Utility.RandomMinMax(15, 20) : 64;
							break;
						case 6:
							count = isSos ? Utility.RandomMinMax(15, 20) : 72;
							break;
						case 7:
							count = isSos ? Utility.RandomMinMax(15, 20) : 80;
							break;
						default:
							count = 0;
							break;
					}

					for (int i = 0; i < count; ++i)
					{
						var item = Loot.RandomArmorOrShieldOrWeaponOrJewelry();

						if (item != null && RandomItemGenerator.Enabled)
						{
							GetRandomItemStat(out var min, out var max, propsScale);

							RunicReforging.GenerateRandomItem(item, luck, min, max, map);

							cont.DropItem(item);
						}
						else if (item is BaseWeapon weapon)
						{
							GetRandomAosStats(out var attributeCount, out var min, out var max);

							BaseRunicTool.ApplyAttributesTo(weapon, attributeCount, min, max);

							cont.DropItem(weapon);
						}
						else if (item is BaseArmor armor)
						{
							GetRandomAosStats(out var attributeCount, out var min, out var max);

							BaseRunicTool.ApplyAttributesTo(armor, attributeCount, min, max);

							cont.DropItem(armor);
						}
						else if (item is BaseHat hat)
						{
							GetRandomAosStats(out var attributeCount, out var min, out var max);

							BaseRunicTool.ApplyAttributesTo(hat, attributeCount, min, max);

							cont.DropItem(hat);
						}
						else if (item is BaseJewel jewel)
						{
							GetRandomAosStats(out var attributeCount, out var min, out var max);

							BaseRunicTool.ApplyAttributesTo(jewel, attributeCount, min, max);

							cont.DropItem(jewel);
						}
					}
					#endregion
				}
				else
				{
					int numberItems;
					if (Core.SE)
					{
						numberItems = level switch
						{
							1 => 5,
							2 => 10,
							3 => 15,
							4 => 38,
							5 => 50,
							6 => 60,
							_ => 0,
						};
					}
					else
						numberItems = level * 6;

					for (int i = 0; i < numberItems; ++i)
					{
						var item = Core.AOS ? Loot.RandomArmorOrShieldOrWeaponOrJewelry() : Loot.RandomArmorOrShieldOrWeapon();

						switch (item)
						{
							case BaseWeapon weapon:
								{
									if (Core.AOS)
									{
										GetRandomAosStats(out int attributeCount, out int min, out int max);
										BaseRunicTool.ApplyAttributesTo(weapon, attributeCount, min, max);
									}
									else
									{
										weapon.DamageLevel = (WeaponDamageLevel)Utility.Random(6);
										weapon.AccuracyLevel = (WeaponAccuracyLevel)Utility.Random(6);
										weapon.DurabilityLevel = (DurabilityLevel)Utility.Random(6);
									}

									cont.DropItem(item);
									break;
								}
							case BaseArmor armor:
								{
									if (Core.AOS)
									{
										GetRandomAosStats(out int attributeCount, out int min, out int max);
										BaseRunicTool.ApplyAttributesTo(armor, attributeCount, min, max);
									}
									else
									{
										armor.ProtectionLevel = (ArmorProtectionLevel)Utility.Random(6);
										armor.Durability = (DurabilityLevel)Utility.Random(6);
									}

									cont.DropItem(item);
									break;
								}
							case BaseHat hat:
								{
									if (Core.AOS)
									{
										GetRandomAosStats(out int attributeCount, out int min, out int max);
										BaseRunicTool.ApplyAttributesTo(hat, attributeCount, min, max);
									}

									cont.DropItem(item);
									break;
								}
							case BaseJewel jewel:
								{
									GetRandomAosStats(out int attributeCount, out int min, out int max);

									BaseRunicTool.ApplyAttributesTo(jewel, attributeCount, min, max);

									cont.DropItem(item);
									break;
								}
						}
					}
				}
			}

			#region Reagents
			if (isSos)
			{
				count = level switch
				{
					0 or 1 => Utility.RandomMinMax(15, 20),
					2 => Utility.RandomMinMax(25, 40),
					_ => Utility.RandomMinMax(45, 60),
				};
			}
			else
			{
				count = level == 0 ? 12 : Utility.RandomMinMax(40, 60) * (level + 1);
			}

			for (int i = 0; i < count; i++)
			{
				cont.DropItemStacked(Loot.RandomPossibleReagent());
			}
			#endregion

			#region Gems
			if (level == 0)
				count = 2;
			else
				count = level * 3 + 1;

			for (int i = 0; i < count; i++)
			{
				cont.DropItem(Loot.RandomGem());
			}
			#endregion

			#region Imbuing Ingreds
			if (Core.SA && level > 1)
			{
				Item item = Loot.Construct(ImbuingIngreds[Utility.Random(ImbuingIngreds.Length)]);

				item.Amount = level;
				cont.DropItem(item);
			}
			#endregion

			if (Core.SA)
			{
				Item arty = null;
				Item special = null;
				Item newSpecial = null;

				if (isSos)
				{
					if (0.004 * level > Utility.RandomDouble())
						arty = Loot.Construct(SosArtifacts);
					if (0.006 * level > Utility.RandomDouble())
						special = Loot.Construct(SosDecor);
					else if (0.009 * level > Utility.RandomDouble())
						special = new TreasureMap(Utility.RandomMinMax(level, Math.Min(7, level + 1)), cont.Map);

					/*if (level >= 4)
					{
						switch (Utility.Random(4))
						{
							case 0: newSpecial = new AncientAquariumFishNet(); break;
							case 1: newSpecial = new LiveRock(); break;
							case 2: newSpecial = new SaltedSerpentSteaks(); break;
							case 3: newSpecial = new OceanSapphire(); break;
						}
					}*/
				}
				else
				{
					if (level >= 7)
					{
						if (0.025 > Utility.RandomDouble())
							special = Loot.Construct(ArtifactsLevelSeven);
						else if (0.10 > Utility.RandomDouble())
							special = Loot.Construct(ArtifactsLevelFiveToSeven);
						else if (0.25 > Utility.RandomDouble())
							special = GetRandomSpecial(level, cont.Map);

						arty = Loot.Construct(Artifacts);
					}
					else if (level >= 6)
					{
						if (0.025 > Utility.RandomDouble())
							special = Loot.Construct(ArtifactsLevelFiveToSeven);
						else if (0.20 > Utility.RandomDouble())
							special = GetRandomSpecial(level, cont.Map);

						arty = Loot.Construct(Artifacts);
					}
					else if (level >= 5)
					{
						if (0.005 > Utility.RandomDouble())
							special = Loot.Construct(ArtifactsLevelFiveToSeven);
						else if (0.15 > Utility.RandomDouble())
							special = GetRandomSpecial(level, cont.Map);
					}
					else if (0.10 > Utility.RandomDouble())
					{
						special = GetRandomSpecial(level, cont.Map);
					}
				}

				if (arty != null)
				{
					Container pack = new Backpack
					{
						Hue = 1278
					};

					pack.DropItem(arty);
					cont.DropItem(pack);
				}

				if (special != null)
					cont.DropItem(special);

				if (newSpecial != null)
					cont.DropItem(newSpecial);

				int rolls = 2;

				if (level >= 5)
					rolls += level - 2;

				//RefinementComponent.Roll(cont, rolls, 0.10);
			}
			else
			{
				if (level == 6 && Core.AOS)
					cont.DropItem((Item)Activator.CreateInstance(Artifacts[Utility.Random(Artifacts.Length)]));
			}
		}

		private static Item GetRandomSpecial(int level, Map map)
		{
			Item special = Utility.Random(8) switch
			{
				1 => new MessageInABottle(),
				2 => new ScrollOfAlacrity(PowerScroll.Skills[Utility.Random(PowerScroll.Skills.Count)]),
				3 => new Skeletonkey(),
				4 => new TastyTreat(5),
				5 => new TreasureMap(Utility.RandomMinMax(level, Math.Min(7, level + 1)), map),
				6 => GetRandomRecipe(),
				7 => ScrollOfTranscendence.CreateRandom(1, 5),
				_ => new CreepingVine()
			};

			return special;
		}

		public static void GetRandomItemStat(out int min, out int max, double scale = 1.0)
		{
			int rnd = Utility.Random(100);

			switch (rnd)
			{
				case <= 1:
					min = 500; max = 1300;
					break;
				case < 5:
					min = 400; max = 1100;
					break;
				case < 25:
					min = 350; max = 900;
					break;
				case < 50:
					min = 250; max = 800;
					break;
				default:
					min = 100; max = 600;
					break;
			}

			min = (int)(min * scale);
			max = (int)(max * scale);
		}

		public static void PuzzleChestLoot(Container cont)
		{
			cont.DropItem(new Gold(600, 900));

			List<Item> gems = new();
			for (int i = 0; i < 9; i++)
			{
				Item gem = Loot.RandomGem();
				Type gemType = gem.GetType();

				foreach (var listGem in gems.Where(listGem => listGem.GetType() == gemType))
				{
					listGem.Amount++;
					gem.Delete();
					break;
				}

				if (!gem.Deleted)
					gems.Add(gem);
			}

			foreach (Item gem in gems)
				cont.DropItem(gem);

			if (0.2 > Utility.RandomDouble())
				cont.DropItem(new BagOfReagents(50));

			if (Core.SA)
			{
				double propsScale = 1.0;

				for (int i = 0; i < 2; ++i)
				{
					var item = Loot.RandomArmorOrShieldOrWeaponOrJewelry();

					if (item != null && RandomItemGenerator.Enabled)
					{
						GetRandomItemStat(out var min, out var max, propsScale);

						RunicReforging.GenerateRandomItem(item, 0, min, max);

						cont.DropItem(item);
					}
					else switch (item)
					{
						case BaseWeapon weapon:
						{
							GetRandomAosStats(out var attributeCount, out var min, out var max);

							BaseRunicTool.ApplyAttributesTo(weapon, attributeCount, min, max);

							cont.DropItem(weapon);
							break;
						}
						case BaseArmor armor:
						{
							GetRandomAosStats(out var attributeCount, out var min, out var max);

							BaseRunicTool.ApplyAttributesTo(armor, attributeCount, min, max);

							cont.DropItem(armor);
							break;
						}
						case BaseHat hat:
						{
							GetRandomAosStats(out var attributeCount, out var min, out var max);

							BaseRunicTool.ApplyAttributesTo(hat, attributeCount, min, max);

							cont.DropItem(hat);
							break;
						}
						case BaseJewel jewel:
						{
							GetRandomAosStats(out var attributeCount, out var min, out var max);

							BaseRunicTool.ApplyAttributesTo(jewel, attributeCount, min, max);

							cont.DropItem(jewel);
							break;
						}
					}
				}
			}
			else
			{
				for (int i = 0; i < 2; i++)
				{
					var item = Core.AOS
						? Loot.RandomArmorOrShieldOrWeaponOrJewelry()
						: Loot.RandomArmorOrShieldOrWeapon();

					switch (item)
					{
						case BaseWeapon weapon:
							{
								if (Core.AOS)
								{
									GetRandomAosStats(out int attributeCount, out int min, out int max);

									BaseRunicTool.ApplyAttributesTo(weapon, attributeCount, min, max);
								}
								else
								{
									weapon.DamageLevel = (WeaponDamageLevel)Utility.Random(6);
									weapon.AccuracyLevel = (WeaponAccuracyLevel)Utility.Random(6);
									weapon.DurabilityLevel = (DurabilityLevel)Utility.Random(6);
								}

								cont.DropItem(item);
								break;
							}
						case BaseArmor armor:
							{
								if (Core.AOS)
								{
									GetRandomAosStats(out int attributeCount, out int min, out int max);

									BaseRunicTool.ApplyAttributesTo(armor, attributeCount, min, max);
								}
								else
								{
									armor.ProtectionLevel = (ArmorProtectionLevel)Utility.Random(6);
									armor.Durability = (DurabilityLevel)Utility.Random(6);
								}

								cont.DropItem(item);
								break;
							}
						case BaseHat hat:
							{
								if (Core.AOS)
								{
									GetRandomAosStats(out int attributeCount, out int min, out int max);

									BaseRunicTool.ApplyAttributesTo(hat, attributeCount, min, max);
								}

								cont.DropItem(item);
								break;
							}
						case BaseJewel jewel:
							{
								GetRandomAosStats(out int attributeCount, out int min, out int max);

								BaseRunicTool.ApplyAttributesTo(jewel, attributeCount, min, max);

								cont.DropItem(item);
								break;
							}
					}
				}
			}
		}

		public static Item GetRandomRecipe()
		{
			List<Engines.Craft.Recipe> recipes = new(Engines.Craft.Recipe.Recipes.Values);

			return new RecipeScroll(recipes[Utility.Random(recipes.Count)]);
		}

		public static void GetRandomAosStats(out int attributeCount, out int min, out int max)
		{
			int rnd = Utility.Random(15);

			if (Core.SE)
			{
				switch (rnd)
				{
					case < 1:
						attributeCount = Utility.RandomMinMax(3, 5);
						min = 50; max = 100;
						break;
					case < 3:
						attributeCount = Utility.RandomMinMax(2, 5);
						min = 40; max = 80;
						break;
					case < 6:
						attributeCount = Utility.RandomMinMax(2, 4);
						min = 30; max = 60;
						break;
					case < 10:
						attributeCount = Utility.RandomMinMax(1, 3);
						min = 20; max = 40;
						break;
					default:
						attributeCount = 1;
						min = 10; max = 20;
						break;
				}
			}
			else
			{
				if (rnd < 1)
				{
					attributeCount = Utility.RandomMinMax(2, 5);
					min = 20; max = 70;
				}
				else if (rnd < 3)
				{
					attributeCount = Utility.RandomMinMax(2, 4);
					min = 20; max = 50;
				}
				else if (rnd < 6)
				{
					attributeCount = Utility.RandomMinMax(2, 3);
					min = 20; max = 40;
				}
				else if (rnd < 10)
				{
					attributeCount = Utility.RandomMinMax(1, 2);
					min = 10; max = 30;
				}
				else
				{
					attributeCount = 1;
					min = 10; max = 20;
				}
			}
		}
	}
}
