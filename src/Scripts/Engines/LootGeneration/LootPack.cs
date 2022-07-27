using Server.Items;
using Server.Mobiles;
using System;
using System.Collections.Generic;
using System.Linq;
using Server.Engines.Plants;

namespace Server
{
	public class LootPack
	{
		private readonly LootPackEntry[] m_Entries;

		public LootPack(LootPackEntry[] entries)
		{
			m_Entries = entries;
		}

		public static int GetLuckChance(Mobile killer, Mobile victim)
		{
			if (!Core.AOS)
				return 0;

			int luck = killer.Luck;

			if (killer is PlayerMobile { SentHonorContext: { } } pmKiller && pmKiller.SentHonorContext.Target == victim)
				luck += pmKiller.SentHonorContext.PerfectionLuckBonus;

			if (luck < 0)
				return 0;

			if (!Core.SE && luck > 1200)
				luck = 1200;

			return (int)(Math.Pow(luck, 1 / 1.8) * 100);
		}

		public static int GetLuckChance(int luck)
		{
			return (int)(Math.Pow(luck, 1 / 1.8) * 100);
		}

		public static int GetLuckChanceForKiller(Mobile mob)
		{
			if (mob is not BaseCreature dead)
				return 240;

			List<DamageStore> list = dead.GetLootingRights();

			DamageStore highest = null;

			foreach (var ds in list.Where(ds => ds.HasRight && (highest == null || ds.Damage > highest.Damage)))
			{
				highest = ds;
			}

			return highest == null ? 0 : GetLuckChance(highest.Mobile, dead);
		}

		public static bool CheckLuck(int chance)
		{
			return chance > Utility.Random(10000);
		}

		public void Generate(IEntity e)
		{
			BaseContainer cont;
			LootStage stage = LootStage.Death;
			int luckChance = 0;
			bool hasBeenStolenFrom = false;
			Mobile from = e as Mobile;

			if (e is BaseCreature bc)
			{
				cont = bc.Backpack as BaseContainer;
				stage = bc.LootStage;
				luckChance = bc.KillersLuck;
				hasBeenStolenFrom = bc.StealPackGenerated;
			}
			else
			{
				cont = e as BaseContainer;
			}

			if (cont != null)
			{
				Generate(from, cont, stage, luckChance, hasBeenStolenFrom);
			}
		}

		public void Generate(Mobile from, Container cont, bool spawning, int luckChance)
		{
			Generate(from, cont as BaseContainer, spawning ? LootStage.Spawning : LootStage.Death, luckChance, false);
		}

		private void Generate(IEntity from, BaseContainer cont, LootStage stage, int luckChance, bool hasBeenStolenFrom)
		{
			if (cont == null)
			{
				return;
			}

			bool checkLuck = Core.AOS;

			foreach (var entry in m_Entries)
			{
				if (!entry.CanGenerate(stage, hasBeenStolenFrom))
					continue;

				bool shouldAdd = entry.Chance > Utility.Random(10000);

				if (!shouldAdd && checkLuck)
				{
					checkLuck = false;

					if (CheckLuck(luckChance))
					{
						shouldAdd = entry.Chance > Utility.Random(10000);
					}
				}

				if (!shouldAdd)
				{
					continue;
				}

				Item item = entry.Construct(from, luckChance, stage, hasBeenStolenFrom);

				if (item == null)
					continue;
				if (from is BaseCreature creature && item.LootType == LootType.Blessed)
				{
					Timer.DelayCall(TimeSpan.FromMilliseconds(25), () =>
					{
						var corpse = creature.Corpse;

						if (corpse != null)
						{
							if (!corpse.TryDropItem(creature, item, false))
							{
								corpse.DropItem(item);
							}
						}
						else
						{
							item.Delete();
						}
					});
				}
				else if (item.Stackable)
				{
					cont.DropItemStacked(item);
				}
				else
				{
					cont.DropItem(item);
				}
			}
		}

		public static readonly LootPackItem[] Gold = { new(typeof(Gold), 1) };

		public static readonly LootPackItem[] Instruments = { new(typeof(BaseInstrument), 1) };

		// Circles 1 - 3
		public static readonly LootPackItem[] LowScrollItems = {
			new(typeof(ReactiveArmorScroll), 1),
			new(typeof(ClumsyScroll), 1),
			new(typeof(CreateFoodScroll), 1),
			new(typeof(FeeblemindScroll), 1),
			new(typeof(HealScroll), 1),
			new(typeof(MagicArrowScroll), 1),
			new(typeof(NightSightScroll), 1),
			new(typeof(WeakenScroll), 1),
			new(typeof(AgilityScroll), 1),
			new(typeof(CunningScroll), 1),
			new(typeof(CureScroll), 1),
			new(typeof(HarmScroll), 1),
			new(typeof(MagicTrapScroll), 1),
			new(typeof(MagicUnTrapScroll), 1),
			new(typeof(ProtectionScroll), 1),
			new(typeof(StrengthScroll), 1),
			new(typeof(BlessScroll), 1),
			new(typeof(FireballScroll), 1),
			new(typeof(MagicLockScroll), 1),
			new(typeof(PoisonScroll), 1),
			new(typeof(TelekinisisScroll), 1),
			new(typeof(TeleportScroll), 1),
			new(typeof(UnlockScroll), 1),
			new(typeof(WallOfStoneScroll), 1)
		};

		// Circles 4 - 6
		public static readonly LootPackItem[] MedScrollItems = {
			new(typeof(ArchCureScroll), 1),
			new(typeof(ArchProtectionScroll), 1),
			new(typeof(CurseScroll), 1),
			new(typeof(FireFieldScroll), 1),
			new(typeof(GreaterHealScroll), 1),
			new(typeof(LightningScroll), 1),
			new(typeof(ManaDrainScroll), 1),
			new(typeof(RecallScroll), 1),
			new(typeof(BladeSpiritsScroll), 1),
			new(typeof(DispelFieldScroll), 1),
			new(typeof(IncognitoScroll), 1),
			new(typeof(MagicReflectScroll), 1),
			new(typeof(MindBlastScroll), 1),
			new(typeof(ParalyzeScroll), 1),
			new(typeof(PoisonFieldScroll), 1),
			new(typeof(SummonCreatureScroll), 1),
			new(typeof(DispelScroll), 1),
			new(typeof(EnergyBoltScroll), 1),
			new(typeof(ExplosionScroll), 1),
			new(typeof(InvisibilityScroll), 1),
			new(typeof(MarkScroll), 1),
			new(typeof(MassCurseScroll), 1),
			new(typeof(ParalyzeFieldScroll), 1),
			new(typeof(RevealScroll), 1)
		};

		// Circles 7 - 8
		public static readonly LootPackItem[] HighScrollItems = {
			new(typeof(ChainLightningScroll), 1),
			new(typeof(EnergyFieldScroll), 1),
			new(typeof(FlamestrikeScroll), 1),
			new(typeof(GateTravelScroll), 1),
			new(typeof(ManaVampireScroll), 1),
			new(typeof(MassDispelScroll), 1),
			new(typeof(MeteorSwarmScroll), 1),
			new(typeof(PolymorphScroll), 1),
			new(typeof(EarthquakeScroll), 1),
			new(typeof(EnergyVortexScroll), 1),
			new(typeof(ResurrectionScroll), 1),
			new(typeof(SummonAirElementalScroll), 1),
			new(typeof(SummonDaemonScroll), 1),
			new(typeof(SummonEarthElementalScroll), 1),
			new(typeof(SummonFireElementalScroll), 1),
			new(typeof(SummonWaterElementalScroll), 1)
		};

		public static readonly LootPackItem[] MageryScrollItems = {
			new(typeof(ReactiveArmorScroll), 1), new(typeof(ClumsyScroll), 1), new(typeof(CreateFoodScroll), 1), new(typeof(FeeblemindScroll), 1),
			new(typeof(HealScroll), 1), new(typeof(MagicArrowScroll), 1), new(typeof(NightSightScroll), 1), new(typeof(WeakenScroll), 1), new(typeof(AgilityScroll), 1),
			new(typeof(CunningScroll), 1), new(typeof(CureScroll), 1), new(typeof(HarmScroll), 1), new(typeof(MagicTrapScroll), 1), new(typeof(MagicUnTrapScroll), 1),
			new(typeof(ProtectionScroll), 1), new(typeof(StrengthScroll), 1), new(typeof(BlessScroll), 1), new(typeof(FireballScroll), 1),
			new(typeof(MagicLockScroll), 1), new(typeof(PoisonScroll), 1), new(typeof(TelekinisisScroll), 1), new(typeof(TeleportScroll), 1),
			new(typeof(UnlockScroll), 1), new(typeof(WallOfStoneScroll), 1), new(typeof(ArchCureScroll), 1), new(typeof(ArchProtectionScroll), 1),
			new(typeof(CurseScroll), 1), new(typeof(FireFieldScroll), 1), new(typeof(GreaterHealScroll), 1), new(typeof(LightningScroll), 1),
			new(typeof(ManaDrainScroll), 1), new(typeof(RecallScroll), 1), new(typeof(BladeSpiritsScroll), 1), new(typeof(DispelFieldScroll), 1),
			new(typeof(IncognitoScroll), 1), new(typeof(MagicReflectScroll), 1), new(typeof(MindBlastScroll), 1), new(typeof(ParalyzeScroll), 1),
			new(typeof(PoisonFieldScroll), 1), new(typeof(SummonCreatureScroll), 1), new(typeof(DispelScroll), 1), new(typeof(EnergyBoltScroll), 1),
			new(typeof(ExplosionScroll), 1), new(typeof(InvisibilityScroll), 1), new(typeof(MarkScroll), 1), new(typeof(MassCurseScroll), 1),
			new(typeof(ParalyzeFieldScroll), 1), new(typeof(RevealScroll), 1), new(typeof(ChainLightningScroll), 1), new(typeof(EnergyFieldScroll), 1),
			new(typeof(FlamestrikeScroll), 1), new(typeof(GateTravelScroll), 1), new(typeof(ManaVampireScroll), 1), new(typeof(MassDispelScroll), 1),
			new(typeof(MeteorSwarmScroll), 1), new(typeof(PolymorphScroll), 1), new(typeof(EarthquakeScroll), 1), new(typeof(EnergyVortexScroll), 1),
			new(typeof(ResurrectionScroll), 1), new(typeof(SummonAirElementalScroll), 1), new(typeof(SummonDaemonScroll), 1),
			new(typeof(SummonEarthElementalScroll), 1), new(typeof(SummonFireElementalScroll), 1), new(typeof(SummonWaterElementalScroll), 1 )
		};

		public static readonly LootPackItem[] NecroScrollItems = {
			new(typeof(AnimateDeadScroll), 1),
			new(typeof(BloodOathScroll), 1),
			new(typeof(CorpseSkinScroll), 1),
			new(typeof(CurseWeaponScroll), 1),
			new(typeof(EvilOmenScroll), 1),
			new(typeof(HorrificBeastScroll), 1),
			new(typeof(MindRotScroll), 1),
			new(typeof(PainSpikeScroll), 1),
			new(typeof(SummonFamiliarScroll), 1),
			new(typeof(WraithFormScroll), 1),
			new(typeof(LichFormScroll), 1),
			new(typeof(PoisonStrikeScroll), 1),
			new(typeof(StrangleScroll), 1),
			new(typeof(WitherScroll), 1),
			new(typeof(VengefulSpiritScroll), 1),
			new(typeof(VampiricEmbraceScroll), 1),
			new(typeof(ExorcismScroll), 1)
		};

		public static readonly LootPackItem[] ArcanistScrollItems = {
			new(typeof(ArcaneCircleScroll), 1),
			new(typeof(GiftOfRenewalScroll), 1),
			new(typeof(ImmolatingWeaponScroll), 1),
			new(typeof(AttuneWeaponScroll), 1),
			new(typeof(ThunderstormScroll), 1),
			new(typeof(NatureFuryScroll), 1),
			new(typeof(ReaperFormScroll), 1),
			new(typeof(WildfireScroll), 1),
			new(typeof(EssenceOfWindScroll), 1),
			new(typeof(DryadAllureScroll), 1),
			new(typeof(EtherealVoyageScroll), 1),
			new(typeof(WordOfDeathScroll), 1),
			new(typeof(GiftOfLifeScroll), 1),
			new(typeof(ArcaneEmpowermentScroll), 1)
		};
		
		public static readonly LootPackItem[] MysticScrollItems = new[]
		{
			new LootPackItem(typeof(NetherBoltScroll), 1),
			new LootPackItem(typeof(HealingStoneScroll), 1),
			new LootPackItem(typeof(PurgeMagicScroll), 1),
			new LootPackItem(typeof(EnchantScroll), 1),
			new LootPackItem(typeof(SleepScroll), 1),
			new LootPackItem(typeof(EagleStrikeScroll), 1),
			new LootPackItem(typeof(AnimatedWeaponScroll), 1),
			new LootPackItem(typeof(StoneFormScroll), 1),
			new LootPackItem(typeof(SpellTriggerScroll), 1),
			new LootPackItem(typeof(MassSleepScroll), 1),
			new LootPackItem(typeof(CleansingWindsScroll), 1),
			new LootPackItem(typeof(BombardScroll), 1),
			new LootPackItem(typeof(SpellPlagueScroll), 1),
			new LootPackItem(typeof(HailStormScroll), 1),
			new LootPackItem(typeof(NetherCycloneScroll), 1),
			new LootPackItem(typeof(RisingColossusScroll), 1)
		};


		public static readonly LootPackItem[] GemItems = { new(typeof(Amber), 1) };
		public static readonly LootPackItem[] RareGemItems = { new(typeof(BlueDiamond), 1) };

		public static readonly LootPackItem[] MageryRegItems = {
			new(typeof(BlackPearl), 1),
			new(typeof(Bloodmoss), 1),
			new(typeof(Garlic), 1),
			new(typeof(Ginseng), 1),
			new(typeof(MandrakeRoot), 1),
			new(typeof(Nightshade), 1),
			new(typeof(SulfurousAsh), 1),
			new(typeof(SpidersSilk), 1)
		};

		public static readonly LootPackItem[] NecroRegItems = {
			new(typeof(BatWing), 1),
			new(typeof(GraveDust), 1),
			new(typeof(DaemonBlood), 1),
			new(typeof(NoxCrystal), 1),
			new(typeof(PigIron), 1)
		};

		public static readonly LootPackItem[] MysticRegItems = {
			new(typeof(Bone), 1),
			new(typeof(DragonBlood), 1),
			new(typeof(FertileDirt), 1),
			new(typeof(DaemonBone), 1)
		};

		public static readonly LootPackItem[] PeerlessResourceItems = {
			new(typeof(Blight), 1),
			new(typeof(Scourge), 1),
			new(typeof(Taint), 1),
			new(typeof(Putrefaction), 1),
			new(typeof(Corruption), 1),
			new(typeof(Muculent), 1)
		};

		public static readonly LootPackItem[] PotionItems = {
			new(typeof(AgilityPotion), 1), new(typeof(StrengthPotion), 1),
			new(typeof(RefreshPotion), 1), new(typeof(LesserCurePotion), 1),
			new(typeof(LesserHealPotion), 1), new(typeof(LesserPoisonPotion), 1)
		};

		public static readonly LootPackItem[] LootBodyParts = {
			new(typeof(LeftArm), 1), new(typeof(RightArm), 1),
			new(typeof(Torso), 1), new(typeof(RightLeg), 1),
			new(typeof(LeftLeg), 1)
		};

		public static readonly LootPackItem[] LootBones = {
			new(typeof(Bone), 1), new(typeof(RibCage), 2),
			new(typeof(BonePile), 3)
		};

		public static readonly LootPackItem[] LootBodyPartsAndBones = {
			new(typeof(LeftArm), 1), new(typeof(RightArm), 1),
			new(typeof(Torso), 1), new(typeof(RightLeg), 1),
			new(typeof(LeftLeg), 1), new(typeof(Bone), 1),
			new(typeof(RibCage), 1), new(typeof(BonePile), 1)
		};


		public static readonly LootPackItem[] StatueItems = {
			new(typeof(StatueSouth), 1), new(typeof(StatueSouth2), 1),
			new(typeof(StatueNorth), 1), new(typeof(StatueWest), 1),
			new(typeof(StatueEast), 1), new(typeof(StatueEast2), 1),
			new(typeof(StatueSouthEast), 1), new(typeof(BustSouth), 1),
			new(typeof(BustEast), 1)
		};

		#region Old Magic Items
		public static readonly LootPackItem[] OldMagicItems = {
				new( typeof( BaseJewel ), 1 ),
				new( typeof( BaseArmor ), 4 ),
				new( typeof( BaseWeapon ), 3 ),
				new( typeof( BaseRanged ), 1 ),
				new( typeof( BaseShield ), 1 )
			};
		#endregion

		#region AOS Magic Items
		public static readonly LootPackItem[] AosMagicItemsPoor = {
				new( typeof( BaseWeapon ), 3 ),
				new( typeof( BaseRanged ), 1 ),
				new( typeof( BaseArmor ), 4 ),
				new( typeof( BaseShield ), 1 ),
				new( typeof( BaseJewel ), 2 )
			};

		public static readonly LootPackItem[] AosMagicItemsMeagerType1 = {
				new( typeof( BaseWeapon ), 56 ),
				new( typeof( BaseRanged ), 14 ),
				new( typeof( BaseArmor ), 81 ),
				new( typeof( BaseShield ), 11 ),
				new( typeof( BaseJewel ), 42 )
			};

		public static readonly LootPackItem[] AosMagicItemsMeagerType2 = {
				new( typeof( BaseWeapon ), 28 ),
				new( typeof( BaseRanged ), 7 ),
				new( typeof( BaseArmor ), 40 ),
				new( typeof( BaseShield ), 5 ),
				new( typeof( BaseJewel ), 21 )
			};

		public static readonly LootPackItem[] AosMagicItemsAverageType1 = {
				new( typeof( BaseWeapon ), 90 ),
				new( typeof( BaseRanged ), 23 ),
				new( typeof( BaseArmor ), 130 ),
				new( typeof( BaseShield ), 17 ),
				new( typeof( BaseJewel ), 68 )
			};

		public static readonly LootPackItem[] AosMagicItemsAverageType2 = {
				new( typeof( BaseWeapon ), 54 ),
				new( typeof( BaseRanged ), 13 ),
				new( typeof( BaseArmor ), 77 ),
				new( typeof( BaseShield ), 10 ),
				new( typeof( BaseJewel ), 40 )
			};

		public static readonly LootPackItem[] AosMagicItemsRichType1 = {
				new( typeof( BaseWeapon ), 211 ),
				new( typeof( BaseRanged ), 53 ),
				new( typeof( BaseArmor ), 303 ),
				new( typeof( BaseShield ), 39 ),
				new( typeof( BaseJewel ), 158 )
			};

		public static readonly LootPackItem[] AosMagicItemsRichType2 = {
				new( typeof( BaseWeapon ), 170 ),
				new( typeof( BaseRanged ), 43 ),
				new( typeof( BaseArmor ), 245 ),
				new( typeof( BaseShield ), 32 ),
				new( typeof( BaseJewel ), 128 )
			};

		public static readonly LootPackItem[] AosMagicItemsFilthyRichType1 = {
				new( typeof( BaseWeapon ), 219 ),
				new( typeof( BaseRanged ), 55 ),
				new( typeof( BaseArmor ), 315 ),
				new( typeof( BaseShield ), 41 ),
				new( typeof( BaseJewel ), 164 )
			};

		public static readonly LootPackItem[] AosMagicItemsFilthyRichType2 = {
				new( typeof( BaseWeapon ), 239 ),
				new( typeof( BaseRanged ), 60 ),
				new( typeof( BaseArmor ), 343 ),
				new( typeof( BaseShield ), 90 ),
				new( typeof( BaseJewel ), 45 )
			};

		public static readonly LootPackItem[] AosMagicItemsUltraRich = {
				new( typeof( BaseWeapon ), 276 ),
				new( typeof( BaseRanged ), 69 ),
				new( typeof( BaseArmor ), 397 ),
				new( typeof( BaseShield ), 52 ),
				new( typeof( BaseJewel ), 207 )
			};
		#endregion

		#region ML definitions
		public static readonly LootPack MlRich = new(new[]
			{
				new LootPackEntry(false,  true, Gold,                     100.00, "4d50+450" ),
				new LootPackEntry(false, false, AosMagicItemsRichType1,   100.00, 1, 3, 0, 75 ),
				new LootPackEntry(false, false, AosMagicItemsRichType1,    80.00, 1, 3, 0, 75 ),
				new LootPackEntry(false, false, AosMagicItemsRichType1,    60.00, 1, 5, 0, 100 ),
				new LootPackEntry(false, false, Instruments,                1.00, 1 )
			});
		#endregion

		#region SE definitions
		public static readonly LootPack SePoor = new(new[]
			{
				new LootPackEntry(false,  true, Gold,                     100.00, "2d10+20" ),
				new LootPackEntry(false, false, AosMagicItemsPoor,          1.00, 1, 5, 0, 100 ),
				new LootPackEntry(false, false, Instruments,                0.02, 1 )
			});

		public static readonly LootPack SeMeager = new(new[]
			{
				new LootPackEntry(false,  true, Gold,                     100.00, "4d10+40" ),
				new LootPackEntry(false, false, AosMagicItemsMeagerType1,  20.40, 1, 2, 0, 50 ),
				new LootPackEntry(false, false, AosMagicItemsMeagerType2,  10.20, 1, 5, 0, 100 ),
				new LootPackEntry(false, false, Instruments,                0.10, 1 )
			});

		public static readonly LootPack SeAverage = new(new[]
			{
				new LootPackEntry(false,  true, Gold,                     100.00, "8d10+100" ),
				new LootPackEntry(false, false, AosMagicItemsAverageType1, 32.80, 1, 3, 0, 50 ),
				new LootPackEntry(false, false, AosMagicItemsAverageType1, 32.80, 1, 4, 0, 75 ),
				new LootPackEntry(false, false, AosMagicItemsAverageType2, 19.50, 1, 5, 0, 100 ),
				new LootPackEntry(false, false, Instruments,                0.40, 1 )
			});

		public static readonly LootPack SeRich = new(new[]
			{
				new LootPackEntry(false,  true, Gold,                     100.00, "15d10+225" ),
				new LootPackEntry(false, false, AosMagicItemsRichType1,    76.30, 1, 4, 0, 75 ),
				new LootPackEntry(false, false, AosMagicItemsRichType1,    76.30, 1, 4, 0, 75 ),
				new LootPackEntry(false, false, AosMagicItemsRichType2,    61.70, 1, 5, 0, 100 ),
				new LootPackEntry(false, false, Instruments,                1.00, 1 )
			});

		public static readonly LootPack SeFilthyRich = new(new[]
			{
				new LootPackEntry(false,  true, Gold,                        100.00, "3d100+400" ),
				new LootPackEntry(false, false, AosMagicItemsFilthyRichType1, 79.50, 1, 5, 0, 100 ),
				new LootPackEntry(false, false, AosMagicItemsFilthyRichType1, 79.50, 1, 5, 0, 100 ),
				new LootPackEntry(false, false, AosMagicItemsFilthyRichType2, 77.60, 1, 5, 25, 100 ),
				new LootPackEntry(false, false, Instruments,                   2.00, 1 )
			});

		public static readonly LootPack SeUltraRich = new(new[]
			{
				new LootPackEntry(false,  true, Gold,                     100.00, "6d100+600" ),
				new LootPackEntry(false, false, AosMagicItemsUltraRich,   100.00, 1, 5, 25, 100 ),
				new LootPackEntry(false, false, AosMagicItemsUltraRich,   100.00, 1, 5, 25, 100 ),
				new LootPackEntry(false, false, AosMagicItemsUltraRich,   100.00, 1, 5, 25, 100 ),
				new LootPackEntry(false, false, AosMagicItemsUltraRich,   100.00, 1, 5, 25, 100 ),
				new LootPackEntry(false, false, AosMagicItemsUltraRich,   100.00, 1, 5, 25, 100 ),
				new LootPackEntry(false, false, AosMagicItemsUltraRich,   100.00, 1, 5, 33, 100 ),
				new LootPackEntry(false, false, Instruments,                2.00, 1 )
			});

		public static readonly LootPack SeSuperBoss = new(new[]
			{
				new LootPackEntry(false,  true, Gold,                     100.00, "10d100+800" ),
				new LootPackEntry(false, false, AosMagicItemsUltraRich,   100.00, 1, 5, 25, 100 ),
				new LootPackEntry(false, false, AosMagicItemsUltraRich,   100.00, 1, 5, 25, 100 ),
				new LootPackEntry(false, false, AosMagicItemsUltraRich,   100.00, 1, 5, 25, 100 ),
				new LootPackEntry(false, false, AosMagicItemsUltraRich,   100.00, 1, 5, 25, 100 ),
				new LootPackEntry(false, false, AosMagicItemsUltraRich,   100.00, 1, 5, 33, 100 ),
				new LootPackEntry(false, false, AosMagicItemsUltraRich,   100.00, 1, 5, 33, 100 ),
				new LootPackEntry(false, false, AosMagicItemsUltraRich,   100.00, 1, 5, 33, 100 ),
				new LootPackEntry(false, false, AosMagicItemsUltraRich,   100.00, 1, 5, 33, 100 ),
				new LootPackEntry(false, false, AosMagicItemsUltraRich,   100.00, 1, 5, 50, 100 ),
				new LootPackEntry(false, false, AosMagicItemsUltraRich,   100.00, 1, 5, 50, 100 ),
				new LootPackEntry(false, false, Instruments,                2.00, 1 )
			});
		#endregion

		#region AOS definitions
		public static readonly LootPack AosPoor = new(new[]
			{
				new LootPackEntry(false,  true, Gold,         100.00, "1d10+10" ),
				new LootPackEntry(false, false, AosMagicItemsPoor,      0.02, 1, 5, 0, 90 ),
				new LootPackEntry(false, false, Instruments,    0.02, 1 )
			});

		public static readonly LootPack AosMeager = new(new[]
			{
				new LootPackEntry(false,  true, Gold,         100.00, "3d10+20" ),
				new LootPackEntry(false, false, AosMagicItemsMeagerType1,   1.00, 1, 2, 0, 10 ),
				new LootPackEntry(false, false, AosMagicItemsMeagerType2,   0.20, 1, 5, 0, 90 ),
				new LootPackEntry(false, false, Instruments,    0.10, 1 )
			});

		public static readonly LootPack AosAverage = new(new[]
			{
				new LootPackEntry(false,  true, Gold,         100.00, "5d10+50" ),
				new LootPackEntry(false, false, AosMagicItemsAverageType1,  5.00, 1, 4, 0, 20 ),
				new LootPackEntry(false, false, AosMagicItemsAverageType1,  2.00, 1, 3, 0, 50 ),
				new LootPackEntry(false, false, AosMagicItemsAverageType2,  0.50, 1, 5, 0, 90 ),
				new LootPackEntry(false, false, Instruments,    0.40, 1 )
			});

		public static readonly LootPack AosRich = new(new[]
			{
				new LootPackEntry(false,  true, Gold,         100.00, "10d10+150" ),
				new LootPackEntry(false, false, AosMagicItemsRichType1,    20.00, 1, 4, 0, 40 ),
				new LootPackEntry(false, false, AosMagicItemsRichType1,    10.00, 1, 5, 0, 60 ),
				new LootPackEntry(false, false, AosMagicItemsRichType2,     1.00, 1, 5, 0, 90 ),
				new LootPackEntry(false, false, Instruments,    1.00, 1 )
			});

		public static readonly LootPack AosFilthyRich = new(new[]
			{
				new LootPackEntry(false,  true, Gold,         100.00, "2d100+200" ),
				new LootPackEntry(false, false, AosMagicItemsFilthyRichType1,  33.00, 1, 4, 0, 50 ),
				new LootPackEntry(false, false, AosMagicItemsFilthyRichType1,  33.00, 1, 4, 0, 60 ),
				new LootPackEntry(false, false, AosMagicItemsFilthyRichType2,  20.00, 1, 5, 0, 75 ),
				new LootPackEntry(false, false, AosMagicItemsFilthyRichType2,   5.00, 1, 5, 0, 100 ),
				new LootPackEntry(false, false, Instruments,    2.00, 1 )
			});

		public static readonly LootPack AosUltraRich = new(new[]
			{
				new LootPackEntry(false,  true, Gold,         100.00, "5d100+500" ),
				new LootPackEntry(false, false, AosMagicItemsUltraRich,   100.00, 1, 5, 25, 100 ),
				new LootPackEntry(false, false, AosMagicItemsUltraRich,   100.00, 1, 5, 25, 100 ),
				new LootPackEntry(false, false, AosMagicItemsUltraRich,   100.00, 1, 5, 25, 100 ),
				new LootPackEntry(false, false, AosMagicItemsUltraRich,   100.00, 1, 5, 25, 100 ),
				new LootPackEntry(false, false, AosMagicItemsUltraRich,   100.00, 1, 5, 25, 100 ),
				new LootPackEntry(false, false, AosMagicItemsUltraRich,   100.00, 1, 5, 35, 100 ),
				new LootPackEntry(false, false, Instruments,    2.00, 1 )
			});

		public static readonly LootPack AosSuperBoss = new(new[]
			{
				new LootPackEntry(false,  true, Gold,         100.00, "5d100+500" ),
				new LootPackEntry(false, false, AosMagicItemsUltraRich,   100.00, 1, 5, 25, 100 ),
				new LootPackEntry(false, false, AosMagicItemsUltraRich,   100.00, 1, 5, 25, 100 ),
				new LootPackEntry(false, false, AosMagicItemsUltraRich,   100.00, 1, 5, 25, 100 ),
				new LootPackEntry(false, false, AosMagicItemsUltraRich,   100.00, 1, 5, 25, 100 ),
				new LootPackEntry(false, false, AosMagicItemsUltraRich,   100.00, 1, 5, 33, 100 ),
				new LootPackEntry(false, false, AosMagicItemsUltraRich,   100.00, 1, 5, 33, 100 ),
				new LootPackEntry(false, false, AosMagicItemsUltraRich,   100.00, 1, 5, 33, 100 ),
				new LootPackEntry(false, false, AosMagicItemsUltraRich,   100.00, 1, 5, 33, 100 ),
				new LootPackEntry(false, false, AosMagicItemsUltraRich,   100.00, 1, 5, 50, 100 ),
				new LootPackEntry(false, false, AosMagicItemsUltraRich,   100.00, 1, 5, 50, 100 ),
				new LootPackEntry(false, false, Instruments,    2.00, 1 )
			});
		#endregion

		#region Pre-AOS definitions
		public static readonly LootPack OldPoor = new(new[]
			{
				new LootPackEntry(false,  true, Gold,         100.00, "1d25" ),
				new LootPackEntry(false, false, Instruments,    0.02, 1 )
			});

		public static readonly LootPack OldMeager = new(new[]
			{
				new LootPackEntry(false,  true, Gold,         100.00, "5d10+25" ),
				new LootPackEntry(false, false, Instruments,    0.10, 1 ),
				new LootPackEntry(false, false, OldMagicItems,  1.00, 1, 1, 0, 60 ),
				new LootPackEntry(false, false, OldMagicItems,  0.20, 1, 1, 10, 70 )
			});

		public static readonly LootPack OldAverage = new(new[]
			{
				new LootPackEntry(false,  true, Gold,         100.00, "10d10+50" ),
				new LootPackEntry(false, false, Instruments,    0.40, 1 ),
				new LootPackEntry(false, false, OldMagicItems,  5.00, 1, 1, 20, 80 ),
				new LootPackEntry(false, false, OldMagicItems,  2.00, 1, 1, 30, 90 ),
				new LootPackEntry(false, false, OldMagicItems,  0.50, 1, 1, 40, 100 )
			});

		public static readonly LootPack OldRich = new(new[]
			{
				new LootPackEntry(false,  true, Gold,         100.00, "10d10+250" ),
				new LootPackEntry(false, false, Instruments,    1.00, 1 ),
				new LootPackEntry(false, false, OldMagicItems, 20.00, 1, 1, 60, 100 ),
				new LootPackEntry(false, false, OldMagicItems, 10.00, 1, 1, 65, 100 ),
				new LootPackEntry(false, false, OldMagicItems,  1.00, 1, 1, 70, 100 )
			});

		public static readonly LootPack OldFilthyRich = new(new[]
			{
				new LootPackEntry(false,  true, Gold,         100.00, "2d125+400" ),
				new LootPackEntry(false, false, Instruments,    2.00, 1 ),
				new LootPackEntry(false, false, OldMagicItems, 33.00, 1, 1, 50, 100 ),
				new LootPackEntry(false, false, OldMagicItems, 33.00, 1, 1, 60, 100 ),
				new LootPackEntry(false, false, OldMagicItems, 20.00, 1, 1, 70, 100 ),
				new LootPackEntry(false, false, OldMagicItems,  5.00, 1, 1, 80, 100 )
			});

		public static readonly LootPack OldUltraRich = new(new[]
			{
				new LootPackEntry(false,  true, Gold,         100.00, "5d100+500" ),
				new LootPackEntry(false, false, Instruments,    2.00, 1 ),
				new LootPackEntry(false, false, OldMagicItems,    100.00, 1, 1, 40, 100 ),
				new LootPackEntry(false, false, OldMagicItems,    100.00, 1, 1, 40, 100 ),
				new LootPackEntry(false, false, OldMagicItems,    100.00, 1, 1, 50, 100 ),
				new LootPackEntry(false, false, OldMagicItems,    100.00, 1, 1, 50, 100 ),
				new LootPackEntry(false, false, OldMagicItems,    100.00, 1, 1, 60, 100 ),
				new LootPackEntry(false, false, OldMagicItems,    100.00, 1, 1, 60, 100 )
			});

		public static readonly LootPack OldSuperBoss = new(new[]
			{
				new LootPackEntry(false,  true, Gold,         100.00, "5d100+500" ),
				new LootPackEntry(false, false, Instruments,    2.00, 1 ),
				new LootPackEntry(false, false, OldMagicItems,    100.00, 1, 1, 40, 100 ),
				new LootPackEntry(false, false, OldMagicItems,    100.00, 1, 1, 40, 100 ),
				new LootPackEntry(false, false, OldMagicItems,    100.00, 1, 1, 40, 100 ),
				new LootPackEntry(false, false, OldMagicItems,    100.00, 1, 1, 50, 100 ),
				new LootPackEntry(false, false, OldMagicItems,    100.00, 1, 1, 50, 100 ),
				new LootPackEntry(false, false, OldMagicItems,    100.00, 1, 1, 50, 100 ),
				new LootPackEntry(false, false, OldMagicItems,    100.00, 1, 1, 60, 100 ),
				new LootPackEntry(false, false, OldMagicItems,    100.00, 1, 1, 60, 100 ),
				new LootPackEntry(false, false, OldMagicItems,    100.00, 1, 1, 60, 100 ),
				new LootPackEntry(false, false, OldMagicItems,    100.00, 1, 1, 70, 100 )
			});
		#endregion

		#region Generic accessors
		public static LootPack Poor => Core.SE ? SePoor : Core.AOS ? AosPoor : OldPoor;
		public static LootPack Meager => Core.SE ? SeMeager : Core.AOS ? AosMeager : OldMeager;
		public static LootPack Average => Core.SE ? SeAverage : Core.AOS ? AosAverage : OldAverage;
		public static LootPack Rich => Core.SE ? SeRich : Core.AOS ? AosRich : OldRich;
		public static LootPack FilthyRich => Core.SE ? SeFilthyRich : Core.AOS ? AosFilthyRich : OldFilthyRich;
		public static LootPack UltraRich => Core.SE ? SeUltraRich : Core.AOS ? AosUltraRich : OldUltraRich;
		public static LootPack SuperBoss => Core.SE ? SeSuperBoss : Core.AOS ? AosSuperBoss : OldSuperBoss;
		#endregion

		public static readonly LootPack LowScrolls = new(new[] { new LootPackEntry(false, true, LowScrollItems, 100.00, 1) });
		public static readonly LootPack MedScrolls = new(new[] { new LootPackEntry(false, true, MedScrollItems, 100.00, 1) });
		public static readonly LootPack HighScrolls = new(new[] { new LootPackEntry(false, true, HighScrollItems, 100.00, 1) });
		public static readonly LootPack MageryScrolls = new(new[] { new LootPackEntry(false, true, MageryScrollItems, 100.00, 1) });
		public static readonly LootPack NecroScrolls = new(new[] { new LootPackEntry(false, true, NecroScrollItems, 100.00, 1) });
		public static readonly LootPack ArcanistScrolls = new(new[] { new LootPackEntry(false, true, ArcanistScrollItems, 100.00, 1) });
		//public static readonly LootPack MysticScrolls = new LootPack(new[] { new LootPackEntry(false, true, MysticScrollItems, 100.00, 1) });

		public static readonly LootPack MageryRegs = new(new[] { new LootPackEntry(false, true, MageryRegItems, 100.00, 1) });
		public static readonly LootPack NecroRegs = new(new[] { new LootPackEntry(false, true, NecroRegItems, 100.00, 1) });
		public static readonly LootPack MysticRegs = new(new[] { new LootPackEntry(false, true, MysticRegItems, 100.00, 1) });
		public static readonly LootPack PeerlessResource = new(new[] { new LootPackEntry(false, true, PeerlessResourceItems, 100.00, 1) });

		public static readonly LootPack Gems = new(new[] { new LootPackEntry(false, true, GemItems, 100.00, 1) });
		public static readonly LootPack RareGems = new(new[] { new LootPackEntry(false, true, RareGemItems, 100.00, 1) });

		public static readonly LootPack Potions = new(new[] { new LootPackEntry(false, true, PotionItems, 100.00, 1) });
		public static readonly LootPack BodyParts = new(new[] { new LootPackEntry(false, true, LootBodyParts, 100.00, 1) });
		public static readonly LootPack Bones = new(new[] { new LootPackEntry(false, true, LootBones, 100.00, 1) });
		public static readonly LootPack BodyPartsAndBones = new(new[] { new LootPackEntry(false, true, LootBodyPartsAndBones, 100.00, 1) });
		public static readonly LootPack Statue = new(new[] { new LootPackEntry(false, true, StatueItems, 100.00, 1) });

		public static readonly LootPack Parrot = new(new[] { new LootPackEntry(false, false, new[] { new LootPackItem(typeof(ParrotItem), 1) }, 10.00, 1) });
		public static readonly LootPack Talisman = new(new[] { new LootPackEntry(false, false, new[] { new LootPackItem(typeof(RandomTalisman), 1) }, 100.00, 1) });

		public static readonly LootPack PeculiarSeed1 = new(new[] { new LootPackEntry(false, true, new[] { new LootPackItem(_ => Seed.RandomPeculiarSeed(1), 1) }, 33.3, 1) });
		public static readonly LootPack PeculiarSeed2 = new(new[] { new LootPackEntry(false, true, new[] { new LootPackItem(_ => Seed.RandomPeculiarSeed(2), 1) }, 33.3, 1) });
		public static readonly LootPack PeculiarSeed3 = new(new[] { new LootPackEntry(false, true, new[] { new LootPackItem(_ => Seed.RandomPeculiarSeed(3), 1) }, 33.3, 1) });
		public static readonly LootPack PeculiarSeed4 = new(new[] { new LootPackEntry(false, true, new[] { new LootPackItem(_ => Seed.RandomPeculiarSeed(4), 1) }, 33.3, 1) });
		public static readonly LootPack BonsaiSeed = new(new[] { new LootPackEntry(false, true, new[] { new LootPackItem(_ => Seed.RandomBonsaiSeed(), 1) }, 25.0, 1) });

		public static LootPack LootItems(LootPackItem[] items)
		{
			return new LootPack(new[] { new LootPackEntry(false, false, items, 100.0, 1) });
		}

		public static LootPack LootItems(LootPackItem[] items, int amount)
		{
			return new LootPack(new[] { new LootPackEntry(false, false, items, 100.0, amount) });
		}

		public static LootPack LootItems(LootPackItem[] items, double chance)
		{
			return new LootPack(new[] { new LootPackEntry(false, false, items, chance, 1) });
		}

		public static LootPack LootItems(LootPackItem[] items, double chance, int amount)
		{
			return new LootPack(new[] { new LootPackEntry(false, false, items, chance, amount) });
		}

		public static LootPack LootItems(LootPackItem[] items, double chance, int amount, bool resource)
		{
			return new LootPack(new[] { new LootPackEntry(false, resource, items, chance, amount) });
		}

		public static LootPack LootItems(LootPackItem[] items, double chance, int amount, bool spawn, bool steal)
		{
			return new LootPack(new[] { new LootPackEntry(spawn, steal, items, chance, amount) });
		}

		public static LootPack LootItem<T>() where T : Item
		{
			return new LootPack(new[] { new LootPackEntry(false, false, new[] { new LootPackItem(typeof(T), 1) }, 100.0, 1) });
		}

		public static LootPack LootItem<T>(bool resource) where T : Item
		{
			return new LootPack(new[] { new LootPackEntry(false, resource, new[] { new LootPackItem(typeof(T), 1) }, 100.0, 1) });
		}

		public static LootPack LootItem<T>(double chance) where T : Item
		{
			return new LootPack(new[] { new LootPackEntry(false, false, new[] { new LootPackItem(typeof(T), 1) }, chance, 1) });
		}

		public static LootPack LootItem<T>(double chance, bool resource) where T : Item
		{
			return new LootPack(new[] { new LootPackEntry(false, resource, new[] { new LootPackItem(typeof(T), 1) }, chance, 1) });
		}

		public static LootPack LootItem<T>(bool onSpawn, bool onSteal) where T : Item
		{
			return new LootPack(new[] { new LootPackEntry(onSpawn, onSteal, new[] { new LootPackItem(typeof(T), 1) }, 100.0, 1) });
		}

		public static LootPack LootItem<T>(int amount) where T : Item
		{
			return new LootPack(new[] { new LootPackEntry(false, false, new[] { new LootPackItem(typeof(T), 1) }, 100.0, amount) });
		}

		public static LootPack LootItem<T>(int min, int max) where T : Item
		{
			return new LootPack(new[] { new LootPackEntry(false, false, new[] { new LootPackItem(typeof(T), 1) }, 100.0, Utility.RandomMinMax(min, max)) });
		}

		public static LootPack LootItem<T>(int min, int max, bool resource) where T : Item
		{
			return new LootPack(new[] { new LootPackEntry(false, resource, new[] { new LootPackItem(typeof(T), 1) }, 100.0, Utility.RandomMinMax(min, max)) });
		}

		public static LootPack LootItem<T>(int min, int max, bool spawnTime, bool onSteal) where T : Item
		{
			return new LootPack(new[] { new LootPackEntry(spawnTime, onSteal, new[] { new LootPackItem(typeof(T), 1) }, 100.0, Utility.RandomMinMax(min, max)) });
		}

		public static LootPack LootItem<T>(int amount, bool resource) where T : Item
		{
			return new LootPack(new[] { new LootPackEntry(false, resource, new[] { new LootPackItem(typeof(T), 1) }, 100.0, amount) });
		}

		public static LootPack LootItem<T>(double chance, int amount) where T : Item
		{
			return new LootPack(new[] { new LootPackEntry(false, false, new[] { new LootPackItem(typeof(T), 1) }, chance, amount) });
		}

		public static LootPack LootItem<T>(double chance, int amount, bool spawnTime, bool onSteal) where T : Item
		{
			return new LootPack(new[] { new LootPackEntry(spawnTime, onSteal, new[] { new LootPackItem(typeof(T), 1) }, chance, amount) });
		}

		public static LootPack RandomLootItem(Type[] types, bool onSpawn, bool onSteal)
		{
			return RandomLootItem(types, 100.0, 1, onSpawn, onSteal);
		}

		//public static LootPack RandomLootItem(Type[] types, double chance, int amount)
		//{
		//	return RandomLootItem(types, chance, amount, false);
		//}

		private static LootPack RandomLootItem(IReadOnlyList<Type> types, double chance = 100.0, int amount = 1, bool onSpawn = false, bool onSteal = false)
		{
			var items = new LootPackItem[types.Count];

			for (var i = 0; i < items.Length; i++)
			{
				items[i] = new LootPackItem(types[i], 1);
			}

			return new LootPack(new[] { new LootPackEntry(onSpawn, onSteal, items, chance, amount) });
		}

		public static LootPack LootItemCallback(Func<IEntity, Item> callback)
		{
			return new LootPack(new[] { new LootPackEntry(false, false, new[] { new LootPackItem(callback, 1) }, 100.0, 1) });
		}

		public static LootPack LootItemCallback(Func<IEntity, Item> callback, double chance, int amount, bool onSpawn, bool onSteal)
		{
			return new LootPack(new[] { new LootPackEntry(onSpawn, onSteal, new[] { new LootPackItem(callback, 1) }, chance, amount) });
		}

		private static LootPack LootGold(int amount)
		{
			return new LootPack(new[] { new LootPackEntry(false, true, new[] { new LootPackItem(typeof(Gold), 1) }, 100.0, amount) });
		}

		public static LootPack LootGold(int min, int max)
		{
			if (min > max)
				min = max;

			if (min > 0)
			{
				return LootGold(Utility.RandomMinMax(min, max));
			}

			return null;
		}

	}

	public class LootPackEntry
	{
		public int Chance { get; }
		private LootPackDice Quantity { get; }
		private bool AtSpawnTime { get; }
		private bool OnStolen { get; }
		private int MaxProps { get; }
		private int MinIntensity { get; }
		private int MaxIntensity { get; }
		private LootPackItem[] Items { get; }
		private bool StandardLootItem { get; }

		public static bool IsInTokuno(IEntity e)
		{
			if (e == null)
			{
				return false;
			}

			Region r = Region.Find(e.Location, e.Map);

			if (r.IsPartOf("Fan Dancer's Dojo"))
			{
				return true;
			}

			if (r.IsPartOf("Yomotsu Mines"))
			{
				return true;
			}

			return e.Map == Map.Tokuno;
		}

		public static bool IsMondain(IEntity e)
		{
			return e != null && MondainsLegacy.IsMlRegion(Region.Find(e.Location, e.Map));
		}

		public static bool IsStygian(IEntity e)
		{
			if (e == null)
				return false;

			return e.Map == Map.TerMur || (!IsInTokuno(e) && !IsMondain(e) && Utility.RandomBool());
		}

		public bool CanGenerate(LootStage stage, bool hasBeenStolenFrom)
		{
			switch (stage)
			{
				case LootStage.Spawning:
					if (!AtSpawnTime)
						return false;
					break;
				case LootStage.Stolen:
					if (!OnStolen)
						return false;
					break;
				case LootStage.Death:
					if (OnStolen && hasBeenStolenFrom)
						return false;
					break;
			}

			return true;
		}

		public Item Construct(IEntity from, int luckChance, LootStage stage, bool hasBeenStolenFrom)
		{
			var totalChance = Items.Sum(t => t.Chance);

			var rnd = Utility.Random(totalChance);

			foreach (var item in Items)
			{
				if (rnd < item.Chance)
				{
					var loot = item.ConstructCallback != null ? item.ConstructCallback(from) : item.Construct(IsInTokuno(from), IsMondain(from), IsStygian(from));

					if (loot != null)
					{
						return Mutate(from, luckChance, loot);
					}
				}

				rnd -= item.Chance;
			}

			return null;
		}

		private int GetRandomOldBonus()
		{
			int rnd = Utility.RandomMinMax(MinIntensity, MaxIntensity);

			if (50 > rnd)
				return 1;
			rnd -= 50;

			if (25 > rnd)
				return 2;
			rnd -= 25;

			if (14 > rnd)
				return 3;
			rnd -= 14;

			return 8 > rnd ? 4 : 5;
		}

		public Item Mutate(IEntity from, int luckChance, Item item)
		{
			switch (item)
			{
				case null:
					return null;
				case BaseWeapon when 1 > Utility.Random(100):
					item.Delete();
					item = new FireHorn();
					return item;
			}

			if (StandardLootItem && item is BaseWeapon or BaseArmor or BaseJewel or BaseHat)
			{
				if (RandomItemGenerator.Enabled && from is BaseCreature creature)
				{
					if (RandomItemGenerator.GenerateRandomItem(item, creature.LastKiller, creature))
					{
						return item;
					}
				}

				if (Core.AOS)
				{
					var bonusProps = GetBonusProperties();

					if (bonusProps < MaxProps && LootPack.CheckLuck(luckChance))
					{
						++bonusProps;
					}

					var props = 1 + bonusProps;

					// Make sure we're not spawning items with 6 properties.
					if (props > MaxProps)
					{
						props = MaxProps;
					}

					switch (item)
					{
						// Use the older style random generation
						case BaseWeapon weapon:
							BaseRunicTool.ApplyAttributesTo(weapon, false, luckChance, props, MinIntensity,
								MaxIntensity);
							break;
						case BaseArmor armor:
							BaseRunicTool.ApplyAttributesTo(armor, false, luckChance, props, MinIntensity,
								MaxIntensity);
							break;
						case BaseJewel jewel:
							BaseRunicTool.ApplyAttributesTo(jewel, false, luckChance, props, MinIntensity,
								MaxIntensity);
							break;
						default:
							BaseRunicTool.ApplyAttributesTo((BaseHat)item, false, luckChance, props, MinIntensity,
								MaxIntensity);
							break;
					}
				}
				else // not aos
				{
					switch (item)
					{
						case BaseWeapon weapon:
						{
							if (80 > Utility.Random(100))
								weapon.AccuracyLevel = (WeaponAccuracyLevel)GetRandomOldBonus();

							if (60 > Utility.Random(100))
								weapon.DamageLevel = (WeaponDamageLevel)GetRandomOldBonus();

							if (40 > Utility.Random(100))
								weapon.DurabilityLevel = (DurabilityLevel)GetRandomOldBonus();

							if (5 > Utility.Random(100))
								weapon.Slayer = SlayerName.Silver;

							if (from != null && weapon.AccuracyLevel == 0 && weapon.DamageLevel == 0 &&
							    weapon.DurabilityLevel == 0 && weapon.Slayer == SlayerName.None &&
							    5 > Utility.Random(100))
								weapon.Slayer = SlayerGroup.GetLootSlayerType(from.GetType());
							break;
						}
						case BaseArmor armor:
						{
							if (80 > Utility.Random(100))
								armor.ProtectionLevel = (ArmorProtectionLevel)GetRandomOldBonus();

							if (40 > Utility.Random(100))
								armor.Durability = (DurabilityLevel)GetRandomOldBonus();
							break;
						}
					}
				}
			}
			else if (item is BaseInstrument instr)
			{
				var slayer = Core.AOS ? BaseRunicTool.GetRandomSlayer() : SlayerGroup.GetLootSlayerType(from.GetType());

				if (slayer == SlayerName.None)
				{
					instr.Delete();
					return null;
				}

				instr.Quality = ItemQuality.Normal;
				instr.Slayer = slayer;
			}


			if (item.Stackable)
			{
				item.Amount = Quantity.Roll();
			}

			return item;
		}

		public LootPackEntry(bool atSpawnTime, bool onStolen, LootPackItem[] items, double chance, string quantity)
			: this(atSpawnTime, onStolen, items, chance, new LootPackDice(quantity), 0, 0, 0, false)
		{ }

		public LootPackEntry(bool atSpawnTime, bool onStolen, LootPackItem[] items, double chance, string quantity, bool standardLoot)
			: this(atSpawnTime, onStolen, items, chance, new LootPackDice(quantity), 0, 0, 0, standardLoot)
		{ }

		public LootPackEntry(bool atSpawnTime, bool onStolen, LootPackItem[] items, double chance, int quantity)
			: this(atSpawnTime, onStolen, items, chance, new LootPackDice(0, 0, quantity), 0, 0, 0, false)
		{ }

		public LootPackEntry(bool atSpawnTime, bool onStolen, LootPackItem[] items, double chance, int quantity, bool standardLoot)
			: this(atSpawnTime, onStolen, items, chance, new LootPackDice(0, 0, quantity), 0, 0, 0, standardLoot)
		{ }

		public LootPackEntry(
			bool atSpawnTime,
			bool onStolen,
			LootPackItem[] items,
			double chance,
			string quantity,
			int maxProps,
			int minIntensity,
			int maxIntensity)
			: this(atSpawnTime, onStolen, items, chance, new LootPackDice(quantity), maxProps, minIntensity, maxIntensity, false)
		{ }

		public LootPackEntry(
			bool atSpawnTime,
			bool onStolen,
			LootPackItem[] items,
			double chance,
			string quantity,
			int maxProps,
			int minIntensity,
			int maxIntensity,
			bool standardLoot)
			: this(atSpawnTime, onStolen, items, chance, new LootPackDice(quantity), maxProps, minIntensity, maxIntensity, standardLoot)
		{ }

		public LootPackEntry(bool atSpawnTime, bool onStolen, LootPackItem[] items, double chance, int quantity, int maxProps, int minIntensity, int maxIntensity)
			: this(atSpawnTime, onStolen, items, chance, new LootPackDice(0, 0, quantity), maxProps, minIntensity, maxIntensity, false)
		{ }

		public LootPackEntry(bool atSpawnTime, bool onStolen, LootPackItem[] items, double chance, int quantity, int maxProps, int minIntensity, int maxIntensity, bool standardLoot)
			: this(atSpawnTime, onStolen, items, chance, new LootPackDice(0, 0, quantity), maxProps, minIntensity, maxIntensity, standardLoot)
		{ }

		public LootPackEntry(
			bool atSpawnTime,
			bool onStolen,
			LootPackItem[] items,
			double chance,
			LootPackDice quantity,
			int maxProps,
			int minIntensity,
			int maxIntensity,
			bool standardLootItem)
		{
			AtSpawnTime = atSpawnTime;
			OnStolen = onStolen;
			Items = items;
			Chance = (int)(100 * chance);
			Quantity = quantity;
			MaxProps = maxProps;
			MinIntensity = minIntensity;
			MaxIntensity = maxIntensity;
			StandardLootItem = standardLootItem;
		}

		public int GetBonusProperties()
		{
			int p0 = 0, p1 = 0, p2 = 0, p3 = 0, p4 = 0, p5 = 0;

			switch (MaxProps)
			{
				case 1: p0 = 3; p1 = 1; break;
				case 2: p0 = 6; p1 = 3; p2 = 1; break;
				case 3: p0 = 10; p1 = 6; p2 = 3; p3 = 1; break;
				case 4: p0 = 16; p1 = 12; p2 = 6; p3 = 5; p4 = 1; break;
				case 5: p0 = 30; p1 = 25; p2 = 20; p3 = 15; p4 = 9; p5 = 1; break;
			}

			var pc = p0 + p1 + p2 + p3 + p4 + p5;

			var rnd = Utility.Random(pc);

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
	}
	public class LootPackItem
	{
		private Type Type { get; }
		public int Chance { get; }

		public Func<IEntity, Item> ConstructCallback { get; }

		public Item Construct(bool inTokuno, bool isMondain, bool isStygian)
		{
			try
			{
				Item item;

				if (Type == typeof(BaseRanged))
				{
					item = Loot.RandomRangedWeapon(inTokuno, isMondain, isStygian);
				}
				else if (Type == typeof(BaseWeapon))
				{
					item = Loot.RandomWeapon(inTokuno, isMondain, isStygian);
				}
				else if (Type == typeof(BaseArmor))
				{
					item = Loot.RandomArmorOrHat(inTokuno, isMondain, isStygian);
				}
				else if (Type == typeof(BaseShield))
				{
					item = Loot.RandomShield(isStygian);
				}
				else if (Type == typeof(BaseJewel))
				{
					item = Loot.RandomJewelry(isStygian);
				}
				else if (Type == typeof(BaseInstrument))
				{
					item = Loot.RandomInstrument();
				}
				else if (Type == typeof(Amber)) // gem
				{
					item = Loot.RandomGem();
				}
				else if (Type == typeof(BlueDiamond)) // rare gem
				{
					item = Loot.RandomRareGem();
				}
				else
				{
					item = Activator.CreateInstance(Type) as Item;
				}

				return item;
			}
			catch
			{
				// ignored
			}

			return null;
		}

		public LootPackItem(Func<IEntity, Item> callback, int chance)
		{
			ConstructCallback = callback;
			Chance = chance;
		}

		public LootPackItem(Type type, int chance)
		{
			Type = type;
			Chance = chance;
		}
	}

	public class LootPackDice
	{
		private int Count { get; }
		private int Sides { get; }
		private int Bonus { get; }

		public int Roll()
		{
			var v = Bonus;

			for (var i = 0; i < Count; ++i)
				v += Utility.Random(1, Sides);

			return v;
		}

		public LootPackDice(string str)
		{
			var start = 0;
			var index = str.IndexOf('d', start);

			if (index < start)
				return;

			Count = Utility.ToInt32(str[start..index]);

			const bool negative = false;

			start = index + 1;
			index = str.IndexOf('+', start);

			if (negative == index < start)
				index = str.IndexOf('-', start);

			if (index < start)
				index = str.Length;

			Sides = Utility.ToInt32(str[start..index]);

			if (index == str.Length)
				return;

			start = index + 1;
			index = str.Length;

			Bonus = Utility.ToInt32(str[start..index]);
		}

		public LootPackDice(int count, int sides, int bonus)
		{
			Count = count;
			Sides = sides;
			Bonus = bonus;
		}
	}
}
