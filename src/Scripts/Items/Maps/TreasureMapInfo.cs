using Server.Engines.Craft;
using Server.Engines.PartySystem;
using Server.Mobiles;
using Server.SkillHandlers;
using Server.Spells;
using System;
using System.Collections.Generic;
using System.Linq;
using Server.Misc;

namespace Server.Items;

public enum TreasureLevel
{
	Stash,
	Supply,
	Cache,
	Hoard,
	Trove
}

public enum TreasurePackage
{
	Artisan,
	Assassin,
	Mage,
	Ranger,
	Warrior
}

public enum TreasureFacet
{
	Trammel,
	Felucca,
	Ilshenar,
	Malas,
	Tokuno,
	TerMur,
	Eodon
}

public enum ChestQuality
{
	None,
	Rusty,
	Standard,
	Gold
}

public static class TreasureMapInfo
{
	public static bool NewSystem => Core.TOL;

	/// <summary>
	/// This is called from BaseCreature. Instead of editing EVERY creature that drops a map, we'll simply convert it here.
	/// </summary>
	/// <param name="level"></param>
	public static int ConvertLevel(int level)
	{
		if (!NewSystem || level == -1)
			return level;

		return level switch
		{
			2 or 3 => (int)TreasureLevel.Supply,
			4 or 5 => (int)TreasureLevel.Cache,
			6 => (int)TreasureLevel.Hoard,
			7 => (int)TreasureLevel.Trove,
			_ => (int)TreasureLevel.Stash,
		};
	}

	public static TreasureFacet GetFacet(IEntity e)
	{
		return GetFacet(e.Location, e.Map);
	}

	public static int PackageLocalization(TreasurePackage package)
	{
		return package switch
		{
			TreasurePackage.Artisan => 1158989,
			TreasurePackage.Assassin => 1158987,
			TreasurePackage.Mage => 1158986,
			TreasurePackage.Ranger => 1158990,
			TreasurePackage.Warrior => 1158988,
			_ => 0
		};
	}

	public static TreasureFacet GetFacet(IPoint2D p, Map map)
	{
		if (map == Map.TerMur)
		{
			return SpellHelper.IsEodon(map, new Point3D(p.X, p.Y, 0)) ? TreasureFacet.Eodon : TreasureFacet.TerMur;
		}

		if (map == Map.Felucca)
		{
			return TreasureFacet.Felucca;
		}

		if (map == Map.Malas)
		{
			return TreasureFacet.Malas;
		}

		if (map == Map.Ilshenar)
		{
			return TreasureFacet.Ilshenar;
		}

		return map == Map.Tokuno ? TreasureFacet.Tokuno : TreasureFacet.Trammel;
	}

	private static IEnumerable<Type> GetRandomEquipment(TreasurePackage package, TreasureFacet facet, int amount)
	{
		Type[] weapons = GetWeaponList(package, facet);
		Type[] armor = GetArmorList(package, facet);
		Type[] jewels = GetJewelList(facet);

		for (int i = 0; i < amount; i++)
		{
			Type[] list = Utility.Random(5) switch
			{
				1 or 2 => armor,
				3 or 4 => jewels,
				_ => weapons,
			};
			yield return list[Utility.Random(list.Length)];
		}
	}

	private static Type[] GetWeaponList(TreasurePackage package, TreasureFacet facet)
	{
		Type[] list = facet switch
		{
			TreasureFacet.Trammel => m_WeaponTable[(int)package][0],
			TreasureFacet.Felucca => m_WeaponTable[(int)package][0],
			TreasureFacet.Ilshenar => m_WeaponTable[(int)package][1],
			TreasureFacet.Malas => m_WeaponTable[(int)package][2],
			TreasureFacet.Tokuno => m_WeaponTable[(int)package][3],
			TreasureFacet.TerMur => m_WeaponTable[(int)package][4],
			TreasureFacet.Eodon => m_WeaponTable[(int)package][5],
			_ => null
		};

		// tram/fel lists are always default
		if (list == null || list.Length == 0)
		{
			list = m_WeaponTable[(int)package][0];
		}

		return list;
	}

	private static Type[] GetArmorList(TreasurePackage package, TreasureFacet facet)
	{
		Type[] list = facet switch
		{
			TreasureFacet.Trammel => m_ArmorTable[(int)package][0],
			TreasureFacet.Felucca => m_ArmorTable[(int)package][0],
			TreasureFacet.Ilshenar => m_ArmorTable[(int)package][1],
			TreasureFacet.Malas => m_ArmorTable[(int)package][2],
			TreasureFacet.Tokuno => m_ArmorTable[(int)package][3],
			TreasureFacet.TerMur => m_ArmorTable[(int)package][4],
			TreasureFacet.Eodon => m_ArmorTable[(int)package][5],
			_ => null
		};

		// tram/fel lists are always default
		if (list == null || list.Length == 0)
		{
			list = m_ArmorTable[(int)package][0];
		}

		return list;
	}

	private static Type[] GetJewelList(TreasureFacet facet)
	{
		if (facet == TreasureFacet.TerMur)
		{
			return m_JewelTable[1];
		}

		return m_JewelTable[0];
	}

	private static SkillName[] GetTranscendenceList(TreasureLevel level, TreasurePackage package)
	{
		return level is TreasureLevel.Supply or TreasureLevel.Cache ? null : m_TranscendenceTable[(int)package];
	}

	private static SkillName[] GetAlacrityList(TreasureLevel level, TreasurePackage package, TreasureFacet facet)
	{
		if (level == TreasureLevel.Stash || (facet == TreasureFacet.Felucca && level == TreasureLevel.Cache))
		{
			return null;
		}

		return m_AlacrityTable[(int)package];
	}

	private static SkillName[] GetPowerScrollList(TreasureLevel level, TreasurePackage package, TreasureFacet facet)
	{
		if (facet != TreasureFacet.Felucca)
			return null;

		return level >= TreasureLevel.Cache ? m_PowerscrollTable[(int)package] : null;
	}

	private static Type[] GetCraftingMaterials(TreasureLevel level, TreasurePackage package, ChestQuality quality)
	{
		if (package == TreasurePackage.Artisan && level <= TreasureLevel.Supply && quality != ChestQuality.None)
		{
			return MaterialTable[(int)quality - 1];
		}

		return null;
	}

	private static Type[] GetSpecialMaterials(TreasureLevel level, TreasurePackage package, TreasureFacet facet)
	{
		if (package == TreasurePackage.Artisan && level == TreasureLevel.Supply)
		{
			return m_SpecialMaterialTable[(int)facet];
		}

		return null;
	}

	private static Type[] GetDecorativeList(TreasureLevel level, TreasurePackage package, TreasureFacet facet)
	{
		Type[] list = null;

		switch (level)
		{
			case >= TreasureLevel.Cache:
			{
				list = DecorativeTable[(int)package];

				if (facet == TreasureFacet.Malas)
				{
					_ = list.Concat(new[] { typeof(CoffinPiece) });
				}

				break;
			}
			case TreasureLevel.Supply:
				list = m_DecorativeMinorArtifacts;
				break;
		}

		return list;
	}

	private static Type[] GetReagentList(TreasureLevel level, TreasurePackage package, TreasureFacet facet)
	{
		if (level != TreasureLevel.Stash || package != TreasurePackage.Mage)
			return null;

		return facet switch
		{
			TreasureFacet.Felucca => Loot.RegTypes,
			TreasureFacet.Trammel => Loot.RegTypes,
			TreasureFacet.Malas => Loot.NecroRegTypes,
			TreasureFacet.TerMur => Loot.MysticRegTypes,
			_ => null
		};
	}

	public static Recipe[] GetRecipeList(TreasureLevel level, TreasurePackage package)
	{
		if (package == TreasurePackage.Artisan && level == TreasureLevel.Supply)
		{
			return Recipe.Recipes.Values.ToArray();
		}

		return null;
	}

	private static Type[] GetSpecialLootList(TreasureLevel level, TreasurePackage package)
	{
		if (level == TreasureLevel.Stash)
			return null;

		var list = level == TreasureLevel.Supply ? m_SpecialSupplyLoot[(int)package] : m_SpecialCacheHordeAndTrove;

		if (package > TreasurePackage.Artisan)
		{
			_ = list.Concat(m_FunctionalMinorArtifacts);
		}

		return list;
	}

	private static int GetGemCount(ChestQuality quality, TreasureLevel level)
	{
		int baseAmount = quality switch
		{
			ChestQuality.Rusty => 7,
			ChestQuality.Standard => Utility.RandomBool() ? 7 : 9,
			ChestQuality.Gold => Utility.RandomList(7, 9, 11),
			_ => 0
		};

		return baseAmount + (int)level * 5;
	}

	private static int GetGoldCount(TreasureLevel level)
	{
		return level switch
		{
			TreasureLevel.Supply => Utility.RandomMinMax(20000, 50000),
			TreasureLevel.Cache => Utility.RandomMinMax(30000, 60000),
			TreasureLevel.Hoard => Utility.RandomMinMax(40000, 70000),
			TreasureLevel.Trove => Utility.RandomMinMax(50000, 70000),
			_ => Utility.RandomMinMax(10000, 40000),
		};
	}

	public static int GetRefinementRolls(ChestQuality quality)
	{
		return quality switch
		{
			ChestQuality.Rusty => 2,
			ChestQuality.Standard => 4,
			ChestQuality.Gold => 6,
			_ => 2
		};
	}

	private static int GetResourceAmount(TreasureLevel level)
	{
		return level switch
		{
			TreasureLevel.Stash => 50,
			TreasureLevel.Supply => 100,
			_ => 0
		};
	}

	private static int GetRegAmount(ChestQuality quality)
	{
		return quality switch
		{
			ChestQuality.Rusty => 20,
			ChestQuality.Standard => 40,
			ChestQuality.Gold => 60,
			_ => 20
		};
	}

	private static int GetSpecialResourceAmount(ChestQuality quality)
	{
		return quality switch
		{
			ChestQuality.Rusty => 1,
			ChestQuality.Standard => 2,
			ChestQuality.Gold => 3,
			_ => 1
		};
	}

	private static int GetEquipmentAmount(Mobile from, TreasureLevel level, TreasurePackage package)
	{
		var amount = level switch
		{
			TreasureLevel.Supply => 8,
			TreasureLevel.Cache => package == TreasurePackage.Assassin ? 24 : 12,
			TreasureLevel.Hoard => 18,
			TreasureLevel.Trove => 36,
			_ => 6
		};
		Party p = Party.Get(from);

		if (p is not { Count: > 1 })
			return amount;

		for (int i = 0; i < p.Count - 1; i++)
		{
			if (Utility.RandomBool())
			{
				amount++;
			}
		}

		return amount;
	}

	private static void GetMinMaxBudget(TreasureLevel level, Item item, out int min, out int max)
	{
		int preArtifact = Imbuing.GetMaxWeight(item) + 100;
		_ = 0;

		switch (level)
		{
			default:
			case TreasureLevel.Stash:
			case TreasureLevel.Supply: min = 250; max = preArtifact; break;
			case TreasureLevel.Cache:
			case TreasureLevel.Hoard:
			case TreasureLevel.Trove: min = 500; max = 1300; break;
		}
	}

	private static readonly Type[][][] m_WeaponTable = {
		new[] // Artisan
		{
			new[] { typeof(HammerPick),/* typeof(SledgeHammerWeapon), typeof(SmithyHammer),*/ typeof(WarAxe), typeof(WarHammer), typeof(Axe), typeof(BattleAxe), typeof(DoubleAxe), typeof(ExecutionersAxe), typeof(Hatchet), typeof(LargeBattleAxe), typeof(OrnateAxe), typeof(TwoHandedAxe), typeof(Pickaxe) }, // Trammel, Felucca
			null, // Ilshenar
			null, // Malas
			null, // Tokuno
			new[] { typeof(HammerPick), /*typeof(SledgeHammerWeapon), typeof(SmithyHammer),*/ typeof(WarAxe), typeof(WarHammer), typeof(Axe), typeof(BattleAxe), typeof(DoubleAxe), typeof(ExecutionersAxe), typeof(Hatchet), typeof(LargeBattleAxe), typeof(OrnateAxe), typeof(TwoHandedAxe), typeof(Pickaxe), typeof(DualShortAxes) },  // TerMur
			Array.Empty<Type>()  // Eodon
		},
		new[] // Assassin
		{
			new[] { typeof(Dagger), typeof(Kryss), typeof(Cleaver), typeof(Cutlass), typeof(ElvenMachete) },
			null,
			null,
			null,
			new[] { typeof(Dagger), typeof(Kryss), typeof(Cleaver), typeof(Cutlass) },
			new[] { typeof(Dagger), typeof(Kryss), typeof(Cleaver), typeof(Cutlass)/*, typeof(BladedWhip), typeof(BarbedWhip), typeof(SpikedWhip)*/ },
		},
		new[] // Mage
		{
			new[] { typeof(BlackStaff), typeof(ShepherdsCrook), typeof(GnarledStaff), typeof(QuarterStaff) },
			null,
			null,
			null,
			null,
			null,
		},
		new[] // Ranger
		{
			new[] { typeof(Bow), typeof(Crossbow), typeof(HeavyCrossbow), typeof(CompositeBow), typeof(ButcherKnife), typeof(SkinningKnife) },
			new[] { typeof(Bow), typeof(Crossbow), typeof(HeavyCrossbow), typeof(CompositeBow), typeof(ButcherKnife), typeof(SkinningKnife), typeof(SoulGlaive) },
			new[] { typeof(Bow), typeof(Crossbow), typeof(HeavyCrossbow), typeof(CompositeBow), typeof(ButcherKnife), typeof(SkinningKnife), typeof(ElvenCompositeLongbow) },
			null,
			new[] { typeof(Bow), typeof(Crossbow), typeof(HeavyCrossbow), typeof(CompositeBow), typeof(ButcherKnife), typeof(SkinningKnife), typeof(GargishButcherKnife), typeof(Cyclone), typeof(SoulGlaive) },
			null,
		},
		new[] // Warrior
		{
			new[] { typeof(Lance), typeof(Pike), typeof(Pitchfork), typeof(ShortSpear), typeof(WarFork), typeof(Club), typeof(Mace), typeof(Maul), typeof(WarAxe), typeof(Bardiche), typeof(Broadsword), typeof(CrescentBlade), typeof(Halberd), typeof(Longsword), typeof(Scimitar), typeof(VikingSword) },
			null,
			null,
			new[] { typeof(Lance), typeof(Pike), typeof(Pitchfork), typeof(ShortSpear), typeof(WarFork), typeof(Club), typeof(Mace), typeof(Maul), typeof(WarAxe), typeof(Bardiche), typeof(Broadsword), typeof(CrescentBlade), typeof(Halberd), typeof(Longsword), typeof(Scimitar), typeof(VikingSword), typeof(Bokuto), typeof(Daisho) },
			null,
			null,
		},
	};

	private static readonly Type[][][] m_ArmorTable = {
		new[] // Artisan
		{
			new[] { typeof(Bonnet), typeof(Cap), typeof(Circlet), typeof(ElvenGlasses), typeof(FeatheredHat), typeof(FlowerGarland), typeof(JesterHat), typeof(SkullCap), typeof(StrawHat), typeof(TallStrawHat), typeof(WideBrimHat) }, // Trammel/Fel
			null, // Ilshenar
			null, // Malas
			null, // Tokuno
			null, // TerMur
			new[] { typeof(Bonnet), typeof(Cap), typeof(Circlet), typeof(ElvenGlasses), typeof(FeatheredHat), typeof(FlowerGarland), typeof(JesterHat), typeof(SkullCap), typeof(StrawHat), typeof(TallStrawHat), typeof(WideBrimHat)/*, typeof(ChefsToque)*/ }, // Eodon
		},
		new[] // Assassin
		{
			new[] { typeof(ChainLegs), typeof(ChainCoif), typeof(ChainChest), typeof(RingmailLegs), typeof(RingmailGloves), typeof(RingmailChest), typeof(RingmailArms), typeof(Bandana) }, // Trammel/Fel
			null, // Ilshenar
			null, // Malas
			new[] { typeof(ChainLegs), typeof(ChainCoif), typeof(ChainChest), typeof(RingmailLegs), typeof(RingmailGloves), typeof(RingmailArms), typeof(RingmailArms), typeof(Bandana), typeof(LeatherSuneate), typeof(LeatherMempo), typeof(LeatherJingasa), typeof(LeatherHiroSode), typeof(LeatherHaidate), typeof(LeatherDo) }, // Tokuno
			null, // TerMur
			null, // Eodon
		},
		new[] // Mage
		{
			new[] { typeof(LeafGloves), typeof(LeafLegs), typeof(LeafTonlet), typeof(LeafGorget), typeof(LeafArms),typeof(LeafChest), typeof(LeatherArms), typeof(LeatherChest), typeof(LeatherLegs), typeof(LeatherGloves), typeof(LeatherGorget), typeof(WizardsHat) }, // Trammel/Fel
			null, // Ilshenar
			new[] { typeof(LeafGloves), typeof(LeafLegs), typeof(LeafTonlet), typeof(LeafGorget), typeof(LeafArms),typeof(LeafChest), typeof(LeatherArms), typeof(LeatherChest), typeof(LeatherLegs), typeof(LeatherGloves), typeof(LeatherGorget), typeof(WizardsHat), typeof(BoneLegs), typeof(BoneHelm), typeof(BoneGloves), typeof(BoneChest), typeof(BoneArms) }, // Malas
			null, // Tokuno
			new[] { typeof(LeatherArms), typeof(LeatherChest), typeof(LeatherLegs), typeof(LeatherGloves), typeof(LeatherGorget), typeof(WizardsHat) }, // TerMur
			new[] { typeof(LeatherArms), typeof(LeatherChest), typeof(LeatherLegs), typeof(LeatherGloves), typeof(LeatherGorget), typeof(WizardsHat) }, // Eodon
		},
		new[] // Ranger
		{
			new[] { typeof(HidePants), typeof(HidePauldrons), typeof(HideGorget), typeof(HideFemaleChest), typeof(HideChest), typeof(HideGloves), typeof(StuddedLegs), typeof(StuddedGorget), typeof(StuddedGloves), typeof(StuddedChest), typeof(StuddedBustierArms), typeof(StuddedArms), typeof(RavenHelm), typeof(VultureHelm), typeof(WingedHelm) }, // Trammel/Fel
			null, // Ilshenar
			null, // Malas
			new[] { typeof(StuddedLegs), typeof(StuddedGorget), typeof(StuddedGloves), typeof(StuddedChest), typeof(StuddedBustierArms), typeof(StuddedArms) }, // Tokuno
			new[] { typeof(HidePants), typeof(HidePauldrons), typeof(HideGorget), typeof(HideFemaleChest), typeof(HideChest), typeof(HideGloves), typeof(StuddedLegs), typeof(StuddedGorget), typeof(StuddedGloves), typeof(StuddedChest), typeof(StuddedBustierArms), typeof(StuddedArms), typeof(GargishLeatherKilt), typeof(GargishLeatherLegs), typeof(GargishLeatherArms), typeof(GargishLeatherChest) }, // TerMur
			new[] { typeof(StuddedLegs), typeof(StuddedGorget), typeof(StuddedGloves), typeof(StuddedChest), typeof(StuddedBustierArms), typeof(StuddedArms)/*, typeof(TigerPeltSkirt), typeof(TigerPeltShorts), typeof(TigerPeltLegs), typeof(TigerPeltLongSkirt), typeof(TigerPeltHelm), typeof(TigerPeltChest), typeof(TigerPeltCollar), typeof(TigerPeltBustier)*/, typeof(VultureHelm), typeof(TribalMask) }, // Eodon
		},
		new[] // Warrior
		{
			new[] { typeof(PlateLegs), typeof(PlateHelm), typeof(PlateGorget), typeof(PlateGloves), typeof(PlateChest), typeof(PlateArms), typeof(Bascinet), typeof(CloseHelm), typeof(Helmet), typeof(LeatherCap), typeof(NorseHelm), typeof(TricorneHat), typeof(BronzeShield), typeof(Buckler), typeof(ChaosShield), typeof(HeaterShield), typeof(MetalKiteShield), typeof(MetalShield), typeof(OrderShield), typeof(WoodenKiteShield) }, // Trammel/Fel
			null, // Ilshenar
			new[] { typeof(PlateLegs), typeof(PlateHelm), typeof(PlateGorget), typeof(PlateGloves), typeof(PlateChest), typeof(PlateArms), typeof(Bascinet), typeof(CloseHelm), typeof(Helmet), typeof(LeatherCap), typeof(NorseHelm), typeof(TricorneHat), typeof(BronzeShield), typeof(Buckler), typeof(ChaosShield), typeof(HeaterShield), typeof(MetalKiteShield), typeof(MetalShield), typeof(OrderShield), typeof(WoodenKiteShield), typeof(DragonHelm), typeof(DragonGloves), typeof(DragonChest), typeof(DragonArms), typeof(DragonLegs) }, // Malas
			new[] { typeof(PlateLegs), typeof(PlateHelm), typeof(PlateGorget), typeof(PlateGloves), typeof(PlateChest), typeof(PlateArms), typeof(Bascinet), typeof(CloseHelm), typeof(Helmet), typeof(LeatherCap), typeof(NorseHelm), typeof(TricorneHat), typeof(BronzeShield), typeof(Buckler), typeof(ChaosShield), typeof(HeaterShield), typeof(MetalKiteShield), typeof(MetalShield), typeof(OrderShield), typeof(WoodenKiteShield), typeof(PlateSuneate), typeof(PlateMempo), typeof(PlateHiroSode), typeof(PlateHatsuburi), typeof(PlateHaidate), typeof(PlateDo), typeof(PlateBattleKabuto), typeof(DecorativePlateKabuto), typeof(LightPlateJingasa), typeof(SmallPlateJingasa)  }, // Tokuno
			new[] { typeof(PlateLegs), typeof(PlateHelm), typeof(PlateGorget), typeof(PlateGloves), typeof(PlateChest), typeof(PlateArms), typeof(Bascinet), typeof(CloseHelm), typeof(Helmet), typeof(LeatherCap), typeof(NorseHelm), typeof(TricorneHat), typeof(BronzeShield), typeof(Buckler), typeof(ChaosShield), typeof(HeaterShield), typeof(MetalKiteShield), typeof(MetalShield), typeof(OrderShield), typeof(WoodenKiteShield), typeof(GargishPlateArms), typeof(GargishPlateChest), typeof(GargishPlateKilt), typeof(GargishPlateLegs), typeof(GargishStoneKilt), typeof(GargishStoneLegs), typeof(GargishStoneArms), typeof(GargishStoneChest) }, // TerMur
			new[] { typeof(PlateLegs), typeof(PlateHelm), typeof(PlateGorget), typeof(PlateGloves), typeof(PlateChest), typeof(PlateArms), typeof(Bascinet), typeof(CloseHelm), typeof(Helmet), typeof(LeatherCap), typeof(NorseHelm), typeof(TricorneHat), typeof(BronzeShield), typeof(Buckler), typeof(ChaosShield), typeof(HeaterShield), typeof(MetalKiteShield), typeof(MetalShield), typeof(OrderShield), typeof(WoodenKiteShield)/*, typeof(DragonTurtleHideHelm), typeof(DragonTurtleHideLegs), typeof(DragonTurtleHideChest), typeof(DragonTurtleHideBustier), typeof(DragonTurtleHideArms)*/ }, // Eodon
		}
	};

	private static readonly Type[][] MaterialTable = {
		new[] { typeof(SpinedLeather), typeof(OakBoard), typeof(AshBoard), typeof(DullCopperIngot), typeof(ShadowIronIngot), typeof(CopperIngot) },
		new[] { typeof(HornedLeather), typeof(YewBoard), typeof(HeartwoodBoard), typeof(BronzeIngot), typeof(GoldIngot), typeof(AgapiteIngot) },
		new[] { typeof(BarbedLeather), typeof(BloodwoodBoard), typeof(FrostwoodBoard), typeof(ValoriteIngot), typeof(VeriteIngot) }
	};

	private static readonly Type[][] m_JewelTable = {
		new[] { typeof(GoldRing), typeof(GoldBracelet), typeof(SilverRing), typeof(SilverBracelet) }, // standard
		new[] { typeof(GoldRing), typeof(GoldBracelet), typeof(SilverRing), typeof(SilverBracelet), typeof(GargishBracelet) } // Ranger/TerMur
	};

	private static readonly Type[][] DecorativeTable = {
		/*new Type[] { typeof(SkullTiledFloorAddonDeed) },
		new Type[] { typeof(AncientWeapon3) },
		new Type[] { typeof(DecorativeHourglass) },
		new Type[] { typeof(AncientWeapon1), typeof(CreepingVine) },
		new Type[] { typeof(AncientWeapon2) },*/
	};

	private static readonly Type[][] m_SpecialMaterialTable = {
		null, // tram
		null, // fel
		null, // ilsh
		new[] { typeof(LuminescentFungi), typeof(BarkFragment), typeof(Blight), typeof(Corruption), typeof(Muculent), typeof(Putrefaction), typeof(Scourge), typeof(Taint)  }, // malas
		null, // tokuno
		LootHelpers.ImbuingIngreds, // ter
		null // eodon
	};

	private static readonly Type[][] m_SpecialSupplyLoot = {
		/* new Type[] { typeof(LegendaryMapmakersGlasses), typeof(ManaPhasingOrb), typeof(RunedSashOfWarding), typeof(ShieldEngravingTool), null },
		 new Type[] { typeof(ForgedPardon), typeof(LegendaryMapmakersGlasses), typeof(ManaPhasingOrb), typeof(RunedSashOfWarding), typeof(Skeletonkey), typeof(MasterSkeletonKey), typeof(SurgeShield) },
		 new Type[] { typeof(LegendaryMapmakersGlasses), typeof(ManaPhasingOrb), typeof(RunedSashOfWarding) },
		 new Type[] { typeof(LegendaryMapmakersGlasses), typeof(ManaPhasingOrb), typeof(RunedSashOfWarding), typeof(TastyTreat) },
		 new Type[] { typeof(LegendaryMapmakersGlasses), typeof(ManaPhasingOrb), typeof(RunedSashOfWarding) },*/
	};

	private static readonly Type[] m_SpecialCacheHordeAndTrove = {
		/* typeof(OctopusNecklace), typeof(SkullGnarledStaff), typeof(SkullLongsword)*/
	};

	private static readonly Type[] m_DecorativeMinorArtifacts = {
		typeof(CandelabraOfSouls), typeof(GoldBricks), typeof(PhillipsWoodenSteed)/*, typeof(AncientShipModelOfTheHMSCape)*/, typeof(AdmiralsHeartyRum)
	};

	private static readonly Type[] m_FunctionalMinorArtifacts = {
		typeof(ArcticDeathDealer), typeof(BlazeOfDeath), typeof(BurglarsBandana),
		typeof(CavortingClub), typeof(DreadPirateHat),
		typeof(EnchantedTitanLegBone), typeof(GwennosHarp), typeof(IolosLute),
		typeof(LunaLance), typeof(NightsKiss), typeof(NoxRangersHeavyCrossbow),
		typeof(PolarBearMask), typeof(VioletCourage), typeof(HeartOfTheLion),
		typeof(ColdBlood), typeof(AlchemistsBauble), typeof(CaptainQuacklebushsCutlass),
		typeof(ShieldOfInvulnerability),
	};

	private static readonly SkillName[][] m_TranscendenceTable = {
		new[] { SkillName.ArmsLore, SkillName.Blacksmith, SkillName.Carpentry, SkillName.Cartography, SkillName.Cooking, SkillName.Cooking, SkillName.Fletching, SkillName.Mining, SkillName.Tailoring },
		new[] { SkillName.Anatomy, SkillName.DetectHidden, SkillName.Fencing, SkillName.Poisoning, SkillName.RemoveTrap, SkillName.Snooping, SkillName.Stealth },
		new[] { SkillName.Magery, SkillName.Meditation, SkillName.MagicResist, SkillName.Spellweaving },
		new[] { SkillName.Alchemy, SkillName.AnimalLore, SkillName.AnimalTaming, SkillName.Archery, },
		new[] { SkillName.Chivalry, SkillName.Focus, SkillName.Parry, SkillName.Swords, SkillName.Tactics, SkillName.Wrestling },
	};

	private static readonly SkillName[][] m_AlacrityTable = {
		new[] { SkillName.ArmsLore, SkillName.Blacksmith, SkillName.Carpentry, SkillName.Cartography, SkillName.Cooking, SkillName.Cooking, SkillName.Fletching, SkillName.Mining, SkillName.Tailoring, SkillName.Lumberjacking },
		new[] { SkillName.DetectHidden, SkillName.Fencing, SkillName.Hiding, SkillName.Lockpicking, SkillName.Poisoning, SkillName.RemoveTrap, SkillName.Snooping, SkillName.Stealing, SkillName.Stealth },
		new[] { SkillName.Alchemy, SkillName.EvalInt, SkillName.Inscribe, SkillName.Magery, SkillName.Meditation, SkillName.Spellweaving, SkillName.SpiritSpeak },
		new[] { SkillName.AnimalLore, SkillName.AnimalTaming, SkillName.Archery, SkillName.Musicianship, SkillName.Peacemaking, SkillName.Provocation, SkillName.Tinkering, SkillName.Tracking, SkillName.Veterinary },
		new[] { SkillName.Chivalry, SkillName.Focus, SkillName.Macing, SkillName.Parry, SkillName.Swords, SkillName.Wrestling },
	};

	private static readonly SkillName[][] m_PowerscrollTable = {
		null,
		new[] { SkillName.Ninjitsu },
		new[] { SkillName.Magery, SkillName.Meditation, SkillName.Mysticism, SkillName.Spellweaving, SkillName.SpiritSpeak },
		new[] { SkillName.AnimalTaming, SkillName.Discordance, SkillName.Provocation, SkillName.Veterinary },
		new[] { SkillName.Bushido, SkillName.Chivalry, SkillName.Focus, SkillName.Healing, SkillName.Parry, SkillName.Swords, SkillName.Tactics },
	};

	public static void Fill(Mobile from, TreasureMapChest chest, TreasureMap tMap)
	{
		TreasureLevel level = tMap.TreasureLevel;
		TreasurePackage package = tMap.Package;
		TreasureFacet facet = tMap.TreasureFacet;
		ChestQuality quality = chest.ChestQuality;

		chest.Movable = false;
		chest.Locked = true;

		chest.TrapType = TrapType.ExplosionTrap;

		switch ((int)level)
		{
			default:
				chest.RequiredSkill = 5;
				chest.TrapPower = 25;
				chest.TrapLevel = 1;
				break;
			case 1:
				chest.RequiredSkill = 45;
				chest.TrapPower = 75;
				chest.TrapLevel = 3;
				break;
			case 2:
				chest.RequiredSkill = 75;
				chest.TrapPower = 125;
				chest.TrapLevel = 5;
				break;
			case 3:
				chest.RequiredSkill = 80;
				chest.TrapPower = 150;
				chest.TrapLevel = 6;
				break;
			case 4:
				chest.RequiredSkill = 80;
				chest.TrapPower = 170;
				chest.TrapLevel = 7;
				break;
		}

		chest.LockLevel = chest.RequiredSkill - 10;
		chest.MaxLockLevel = chest.RequiredSkill + 40;

		//if (Engines.JollyRoger.JollyRogerEvent.Instance.Running && 0.10 > Utility.RandomDouble())
		//{
		//    chest.DropItem(new MysteriousFragment());
		//}

		#region Refinements
		/*if (level == TreasureLevel.Stash)
		{
		    RefinementComponent.Roll(chest, GetRefinementRolls(quality), 0.9);
		}*/
		#endregion

		#region TMaps
		bool dropMap = false;
		if (level < TreasureLevel.Trove && 0.1 > Utility.RandomDouble())
		{
			chest.DropItem(new TreasureMap(tMap.Level + 1, chest.Map));
			dropMap = true;
		}
		#endregion

		int amount;
		double dropChance = 0.0;

		#region Gold
		int goldAmount = GetGoldCount(level);
		Bag lootBag = new BagOfGold();

		while (goldAmount > 0)
		{
			if (goldAmount <= 20000)
			{
				lootBag.DropItem(new Gold(goldAmount));
				goldAmount = 0;
			}
			else
			{
				lootBag.DropItem(new Gold(20000));
				goldAmount -= 20000;
			}

			chest.DropItem(lootBag);
		}
		#endregion

		#region Regs
		var list = GetReagentList(level, package, facet);

		if (list != null)
		{
			amount = GetRegAmount(quality);
			lootBag = new BagOfRegs();

			for (int i = 0; i < amount; i++)
			{
				lootBag.DropItemStacked(Loot.Construct(list));
			}

			chest.DropItem(lootBag);
		}
		#endregion

		#region Gems
		amount = GetGemCount(quality, level);

		if (amount > 0)
		{
			lootBag = new BagOfGems();

			foreach (Type gemType in Loot.GemTypes)
			{
				Item gem = Loot.Construct(gemType);
				gem.Amount = amount;

				lootBag.DropItem(gem);

			}

			chest.DropItem(lootBag);
		}
		#endregion

		#region Crafting Resources
		// TODO: DO each drop, or do only 1 drop?
		list = GetCraftingMaterials(level, package, quality);

		if (list != null)
		{
			amount = GetResourceAmount(level);

			foreach (Type type in list)
			{
				Item craft = Loot.Construct(type);
				craft.Amount = amount;

				chest.DropItem(craft);
			}
		}
		#endregion

		#region Special Resources
		// TODO: DO each drop, or do only 1 drop?
		list = GetSpecialMaterials(level, package, facet);

		if (list != null)
		{
			amount = GetSpecialResourceAmount(quality);

			foreach (Type type in list)
			{
				Item specialCraft = Loot.Construct(type);
				specialCraft.Amount = amount;

				chest.DropItem(specialCraft);
			}
		}
		#endregion

		#region Special Scrolls
		amount = (int)level + 1;

		if (dropMap)
		{
			amount--;
		}

		if (amount > 0)
		{
			SkillName[] transList = GetTranscendenceList(level, package);
			SkillName[] alacList = GetAlacrityList(level, package, facet);
			SkillName[] pscrollList = GetPowerScrollList(level, package, facet);

			List<Tuple<int, SkillName>> scrollList = new();

			if (transList != null)
			{
				scrollList.AddRange(transList.Select(sk => new Tuple<int, SkillName>(1, sk)));
			}

			if (alacList != null)
			{
				scrollList.AddRange(alacList.Select(sk => new Tuple<int, SkillName>(2, sk)));
			}

			if (pscrollList != null)
			{
				scrollList.AddRange(pscrollList.Select(sk => new Tuple<int, SkillName>(3, sk)));
			}

			if (scrollList.Count > 0)
			{
				for (int i = 0; i < amount; i++)
				{
					Tuple<int, SkillName> random = scrollList[Utility.Random(scrollList.Count)];

					switch (random.Item1)
					{
						case 1: chest.DropItem(new ScrollOfTranscendence(random.Item2, Utility.RandomMinMax(1.0, chest.Map == Map.Felucca ? 7.0 : 5.0) / 10)); break;
						case 2: chest.DropItem(new ScrollOfAlacrity(random.Item2)); break;
						case 3: chest.DropItem(new PowerScroll(random.Item2, 110.0)); break;
					}
				}
			}
		}
		#endregion

		#region Decorations
		switch (level)
		{
			case TreasureLevel.Stash: dropChance = 0.00; break;
			case TreasureLevel.Supply: dropChance = 0.10; break;
			case TreasureLevel.Cache: dropChance = 0.20; break;
			case TreasureLevel.Hoard: dropChance = 0.40; break;
			case TreasureLevel.Trove: dropChance = 0.50; break;
		}

		if (Utility.RandomDouble() < dropChance)
		{
			list = GetDecorativeList(level, package, facet);

			if (list is { Length: > 0 })
			{
				Item deco = Loot.Construct(list[Utility.Random(list.Length)]);

				if (m_DecorativeMinorArtifacts.Any(t => t == deco.GetType()))
				{
					Container pack = new Backpack
					{
						Hue = 1278
					};

					pack.DropItem(deco);
					chest.DropItem(pack);
				}
				else
				{
					chest.DropItem(deco);
				}
			}
		}

		switch (level)
		{
			case TreasureLevel.Stash: dropChance = 0.00; break;
			case TreasureLevel.Supply: dropChance = 0.10; break;
			case TreasureLevel.Cache: dropChance = 0.20; break;
			case TreasureLevel.Hoard: dropChance = 0.50; break;
			case TreasureLevel.Trove: dropChance = 0.75; break;
		}

		if (Utility.RandomDouble() < dropChance)
		{
			list = GetSpecialLootList(level, package);

			if (list is { Length: > 0 })
			{
				Type type = MutateType(list[Utility.Random(list.Length)]);

				var deco = type == null ? LootHelpers.GetRandomRecipe() : Loot.Construct(type);

				/*if (deco is SkullGnarledStaff || deco is SkullLongsword)
			        {
			            if (package == TreasurePackage.Artisan)
			            {
			                ((IQuality)deco).Quality = ItemQuality.Exceptional;
			            }
			            else
			            {
			                int min, max;
			                GetMinMaxBudget(level, deco, out min, out max);
			                RunicReforging.GenerateRandomItem(deco, from is PlayerMobile ? ((PlayerMobile)from).RealLuck : from.Luck, min, max, chest.Map);
			            }
			        }*/

				if (m_FunctionalMinorArtifacts.Any(t => t == type))
				{
					Container pack = new Backpack
					{
						Hue = 1278
					};

					pack.DropItem(deco);
					chest.DropItem(pack);
				}
				else
				{
					chest.DropItem(deco);
				}
			}
		}
		#endregion

		#region Magic Equipment
		amount = GetEquipmentAmount(from, level, package);

		foreach (Type type in GetRandomEquipment(package, facet, amount))
		{
			Item item = Loot.Construct(type);
			GetMinMaxBudget(level, item, out var min, out var max);

			if (item == null)
				continue;

			RunicReforging.GenerateRandomItem(item, from is PlayerMobile mobile ? mobile.RealLuck : from.Luck, min, max, chest.Map);
			chest.DropItem(item);
		}

		#endregion
	}

	private static Type MutateType(Type type)
	{
		/*if (type == typeof(SkullGnarledStaff))
		{
		    type = typeof(GargishSkullGnarledStaff);
		}
		else if (type == typeof(SkullLongsword))
		{
		    type = typeof(GargishSkullLongsword);
		}*/

		return type;
	}
}
