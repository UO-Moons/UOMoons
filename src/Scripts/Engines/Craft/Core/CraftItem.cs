using Server.Commands;
using Server.Engines.Plants;
using Server.Engines.Quests;
using Server.Factions;
using Server.Items;
using Server.Mobiles;
using System;
using System.Collections.Generic;
using System.Linq;
using Server.Targeting;

namespace Server.Engines.Craft;

public enum ConsumeType
{
	All,
	Half,
	None
}

public interface ICraftable
{
	public Mobile Crafter { get; set; }
	int OnCraft(
		int quality,
		bool makersMark,
		Mobile from,
		CraftSystem craftSystem,
		Type typeRes,
		ITool tool,
		CraftItem craftItem,
		int resHue);
}

public class CraftItem
{
	public Action<Mobile, CraftItem, ITool> TryCraft { get; set; }

	/// this func will create complex items that may require args, or other
	/// things to create that Activator may not be able to accomidate.
	public Func<Mobile, CraftItem, ITool, Item> CreateItem { get; set; }
	public Func<Mobile, ConsumeType, int> ConsumeResCallback { get; set; }

	public Type ItemType { get; }
	public string GroupNameString { get; }
	public int GroupNameNumber { get; }
	public string NameString { get; }
	public int NameNumber { get; }
	public CraftResCol Resources { get; }
	public CraftSkillCol Skills { get; }

	public BeverageType RequiredBeverage { get; set; }

	public int ForceSuccessChance { get; set; } = -1;

	public double MinSkillOffset { get; set; }
	public bool ForceNonExceptional { get; set; }
	public bool ForceExceptional { get; set; }
	public Expansion RequiredExpansion { get; set; }
	public ThemePack RequiredThemePack { get; set; }

	public bool RequiresBasketWeaving { get; set; }
	public bool RequiresResTarget { get; set; }
	public bool RequiresMechanicalLife { get; set; }

	public object Data { get; set; }
	public int DisplayId { get; set; }
	public Recipe Recipe { get; set; }

	public int Mana { get; set; }
	public int Hits { get; set; }
	public int Stam { get; set; }
	public bool UseSubRes2 { get; set; }
	public bool UseAllRes { get; set; }
	public bool ForceTypeRes { get; set; }

	public bool NeedHeat { get; set; }
	public bool NeedOven { get; set; }
	public bool NeedMaker { get; set; }
	public bool NeedMill { get; set; }
	public bool NeedWater { get; set; }
	public int ItemHue { get; set; }

	public Action<Mobile, Item, ITool> MutateAction { get; set; }

	public void AddRecipe(int id, CraftSystem system)
	{
		if (Recipe != null)
		{
			Console.WriteLine(
				"Warning: Attempted add of recipe #{0} to the crafting of {1} in CraftSystem {2}.", id, ItemType.Name, system);
			return;
		}

		Recipe = new Recipe(id, system, this);
	}

	public CraftItem(Type type, TextDefinition groupName, TextDefinition name)
	{
		Resources = new CraftResCol();
		Skills = new CraftSkillCol();

		ItemType = type;

		GroupNameString = groupName;
		NameString = name;

		GroupNameNumber = groupName;
		NameNumber = name;

		RequiredBeverage = BeverageType.Water;
	}

	public void AddRes(Type type, TextDefinition name, int amount)
	{
		AddRes(type, name, amount, "");
	}

	public void AddRes(Type type, TextDefinition name, int amount, TextDefinition message)
	{
		CraftRes craftRes = new CraftRes(type, name, amount, message);
		Resources.Add(craftRes);
	}

	public void AddSkill(SkillName skillToMake, double minSkill, double maxSkill)
	{
		CraftSkill craftSkill = new CraftSkill(skillToMake, minSkill, maxSkill);
		Skills.Add(craftSkill);
	}

	public static CraftItem GetCraftItem(Item item, bool subClass = false)
	{
		return GetCraftItem(item.GetType(), subClass);
	}

	public static CraftItem GetCraftItem(Type type, bool subClass = false)
	{
		CraftItem crItem = null;

		for (int i = 0; i < CraftSystem.Systems.Count; i++)
		{
			var system = CraftSystem.Systems[i];

			if (system.CraftItems == null)
			{
				continue;
			}

			crItem = system.CraftItems.SearchFor(type);

			if (crItem == null && subClass)
			{
				crItem = system.CraftItems.SearchForSubclass(type);
			}

			if (crItem != null)
			{
				break;
			}
		}

		return crItem;
	}

	private static readonly Dictionary<Type, int> ItemIds = new();

	public static int ItemIdOf(Type type)
	{
		if (ItemIds.TryGetValue(type, out var itemId)) return itemId;
		if (type == typeof(FactionExplosionTrap))
		{
			itemId = 14034;
		}
		else if (type == typeof(FactionGasTrap))
		{
			itemId = 4523;
		}
		else if (type == typeof(FactionSawTrap))
		{
			itemId = 4359;
		}
		else if (type == typeof(FactionSpikeTrap))
		{
			itemId = 4517;
		}
		else if (type == typeof(ArcaneBookshelfSouthDeed))
		{
			itemId = 0x2DEF;
		}
		else if (type == typeof(ArcaneBookshelfEastDeed))
		{
			itemId = 0x2DF0;
		}
		else if (type == typeof(OrnateElvenChestSouthDeed))
		{
			itemId = 0x2DE9;
		}
		else if (type == typeof(OrnateElvenChestEastDeed))
		{
			itemId = 0x2DEA;
		}
		else if (type == typeof(ElvenWashBasinSouthDeed) || type == typeof(ElvenWashBasinSouthAddonWithDrawer))
		{
			itemId = 0x2D0B;
		}
		else if (type == typeof(ElvenWashBasinEastDeed) || type == typeof(ElvenWashBasinEastAddonWithDrawer))
		{
			itemId = 0x2D0C;
		}
		else if (type == typeof(ElvenDresserSouthDeed))
		{
			itemId = 0x2D09;
		}
		else if (type == typeof(ElvenDresserEastDeed))
		{
			itemId = 0x2D0A;
		}

		if (itemId == 0)
		{
			var attrs = type.GetCustomAttributes(typeof(CraftItemIdAttribute), false);

			if (attrs.Length > 0)
			{
				CraftItemIdAttribute craftItemId = (CraftItemIdAttribute)attrs[0];
				itemId = craftItemId.ItemId;
			}
		}

		if (itemId == 0)
		{
			Item item = null;

			try
			{
				item = Activator.CreateInstance(type) as Item;
			}
			catch
			{
				// ignored
			}

			if (item != null)
			{
				itemId = item.ItemId;
				item.Delete();
			}
		}

		ItemIds[type] = itemId;

		return itemId;
	}

	public bool ConsumeAttributes(Mobile from, ref object message, bool consume)
	{
		if (Hits > 0 && from.Hits < Hits)
		{
			message = "You lack the required hit points to make that.";
			return false;
		}

		//if (Mana > 0)
		//{
		//if (ManaPhasingOrb.IsInManaPhase(from))
		//{
		//	if (consume)
		//	{
		//		ManaPhasingOrb.RemoveFromTable(from);
		//	}

		//	return true;
		//}

		//if (from.Mana < Mana)
		//{
		//	message = "You lack the required mana to make that.";
		//	return false;
		//}
		//else
		//{
		//consumMana = consume;
		//}
		//}


		if (Stam > 0 && from.Stam < Stam)
		{
			message = "You lack the required stamina to make that.";
			return false;
		}

		if (consume)
		{
			from.Hits -= Hits;
		}

		if (consume)
		{
			from.Stam -= Stam;
		}

		return true;
	}

	#region Tables
	private static readonly int[] MHeatSources =
	{
		0x461, 0x48E, // Sandstone oven/fireplace
		0x92B, 0x96C, // Stone oven/fireplace
		0xDE3, 0xDE9, // Campfire
		0xFAC, 0xFAC, // Firepit
		0x184A, 0x184C, // Heating stand (left)
		0x184E, 0x1850, // Heating stand (right)
		0x398C, 0x399F, // Fire field
		0x2DDB, 0x2DDC, //Elven stove
		0x19AA, 0x19BB, // Veteran Reward Brazier
		0x197A, 0x19A9, // Large Forge 
		0x0FB1, 0x0FB1, // Small Forge
		0x2DD8, 0x2DD8, // Elven Forge
		0xA2A4, 0xA2A5, 0xA2A8, 0xA2A9 // Wood Stove
	};

	private static readonly int[] MOvens =
	{
		0x461, 0x46F, // Sandstone oven
		0x92B, 0x93F, // Stone oven
		0x2DDB, 0x2DDC, //Elven stove
	};

	private static readonly int[] MMakers =
	{
		0x9A96, 0x9A96 // steam powered beverage maker
	};

	private static readonly int[] MMills =
	{
		0x1920, 0x1921, 0x1922, 0x1923, 0x1924, 0x1295, 0x1926, 0x1928, 0x192C, 0x192D, 0x192E, 0x129F, 0x1930, 0x1931,
		0x1932, 0x1934
	};

	private static readonly int[] MWaterSources =
	{
		0xB41, 0xB44,
		0xE7B, 0xE7B,
		0xFFA, 0xFFA,
		0x154D, 0x154D,
		0x99CA, 0x99CB,
		0x9A14, 0x9A19,
		0xA2AF, 0xA2B9,
		0x2AC0, 0x2AC5
	};

	private static readonly Type[][] ItemTypesTable =
	{
		new[] {typeof(Board), typeof(Log)},
		new[] {typeof(HeartwoodBoard), typeof(HeartwoodLog)},
		new[] {typeof(BloodwoodBoard), typeof(BloodwoodLog)},
		new[] {typeof(FrostwoodBoard), typeof(FrostwoodLog)},
		new[] {typeof(OakBoard), typeof(OakLog)},
		new[] {typeof(AshBoard), typeof(AshLog)},
		new[] {typeof(YewBoard), typeof(YewLog)},
		new[] {typeof(Leather), typeof(Hides)},
		new[] {typeof(SpinedLeather), typeof(SpinedHides)},
		new[] {typeof(HornedLeather), typeof(HornedHides)},
		new[] {typeof(BarbedLeather), typeof(BarbedHides)},
		new[] {typeof(BlankMap), typeof(BlankScroll)},
		new[] {typeof(Cloth), typeof(UncutCloth)},
		new[] {typeof(CheeseWheel), typeof(CheeseWedge)},
		new[] {typeof(Pumpkin), typeof(SmallPumpkin)},
		new[] {typeof(WoodenBowlOfPeas), typeof(PewterBowlOfPeas)},
		new[] { typeof( CrystallineFragments ) },
		new[] { typeof( RedScales ), typeof( BlueScales ), typeof( BlackScales ), typeof( YellowScales ), typeof( GreenScales ), typeof( WhiteScales ) },
		new[] { typeof(Sausage) },
		new[] { typeof(Lettuce), typeof(FarmableLettuce) },
		new[] { typeof(DarkYarn), typeof(LightYarn) }
	};

	private static readonly Type[] MColoredItemTable =
	{
		#region Mondain's Legacy
		typeof(BaseContainer), typeof(ParrotPerchAddonDeed),
		#endregion

		typeof(BaseWeapon), typeof(BaseArmor), typeof(BaseClothing), typeof(BaseJewel), typeof(DragonBardingDeed),
		typeof(BaseAddonDeed), typeof(BaseAddon),
	};

	private static readonly Type[] MClothColoredItemTable =
	{
		typeof( GozaMatSouthDeed ), typeof( GozaMatEastDeed ),
		typeof( SquareGozaMatSouthDeed ), typeof( SquareGozaMatEastDeed ),
		typeof( BrocadeGozaMatSouthDeed ), typeof( BrocadeGozaMatEastDeed ),
		typeof( BrocadeSquareGozaMatSouthDeed ), typeof( BrocadeSquareGozaMatEastDeed ),
		typeof( Tessen )
	};

	private static readonly Type[] MColoredResourceTable =
	{
		typeof(Board), typeof(Log),
		typeof(BaseIngot), typeof(BaseOre), typeof(BaseLeather), typeof(BaseHides), typeof(UncutCloth), typeof(Cloth),
		typeof(BaseGranite), typeof(BaseScales), typeof(PlantClippings), typeof(DryReeds), typeof(SoftenedReeds),
		typeof(PlantPigment), typeof(BaseContainer)
	};

	private static readonly Type[] MMarkableTable =
	{
		typeof(BlueDiamondRing), typeof(BrilliantAmberBracelet), typeof(DarkSapphireBracelet), typeof(EcruCitrineRing),
		typeof(FireRubyBracelet), typeof(PerfectEmeraldRing), typeof(TurqouiseRing), typeof(WhitePearlBracelet),
		typeof(BaseContainer), typeof(CraftableFurniture),

		typeof(BaseArmor), typeof(BaseWeapon), typeof(BaseClothing), typeof(BaseInstrument), typeof(BaseTool),
		typeof(BaseHarvestTool), typeof(BaseQuiver), typeof(DragonBardingDeed), typeof(Fukiya), typeof(FukiyaDarts),
		typeof(Shuriken), typeof(Spellbook), typeof(Runebook), typeof(ShortMusicStandLeft), typeof(ShortMusicStandRight),
		typeof(TallMusicStandLeft), typeof(TallMusicStandRight), typeof(EasleNorth), typeof(EasleEast), typeof(EasleSouth),
		typeof(RedHangingLantern), typeof(WhiteHangingLantern), typeof(BambooScreen), typeof(ShojiScreen),
		typeof(FishingPole), typeof(Stool), typeof(FootStool), typeof(WoodenBench), typeof(WoodenThrone), typeof(Throne),
		typeof(BambooChair), typeof(WoodenChair), typeof(FancyWoodenChairCushion), typeof(WoodenChairCushion),
		typeof(Nightstand), typeof(LargeTable), typeof(WritingTable), typeof(YewWoodTable), typeof(PlainLowTable),
		typeof(ElegantLowTable), typeof(DressformFront), typeof(DressformSide), typeof(BasePlayerBB), typeof(BarrelStaves),
		typeof(BarrelLid), typeof(Clippers), typeof(Scissors),

		typeof(KeyRing), typeof(Key), typeof(Globe), typeof(Spyglass), typeof(Lantern), typeof(Candelabra), typeof(Scales), typeof(BroadcastCrystal),
		typeof(BaseUtensil), typeof(BaseBeverage),

		typeof(FruitBowl), typeof(SackFlour), typeof(Dough), typeof(SweetDough), typeof(CocoaButter), typeof(CocoaLiquor),
		typeof(Food)
	};

	private static readonly Dictionary<Type, Type> MResourceConversionTable = new()
	{
		{ typeof(Board), typeof(Log) },
		{ typeof(HeartwoodBoard), typeof(HeartwoodLog) },
		{ typeof(BloodwoodBoard), typeof(BloodwoodLog) },
		{ typeof(FrostwoodBoard), typeof(FrostwoodLog) },
		{ typeof(OakBoard), typeof(OakLog) },
		{ typeof(AshBoard), typeof(AshLog) },
		{ typeof(YewBoard), typeof(YewLog) },
		{ typeof(Leather), typeof(Hides) },
		{ typeof(SpinedLeather), typeof(SpinedHides) },
		{ typeof(HornedLeather), typeof(HornedHides) },
		{ typeof(BarbedLeather), typeof(BarbedHides) },
	};

	private static readonly Type[] MNeverColorTable = {
		typeof(OrcHelm),typeof(DeerMask), typeof(BearMask), typeof(TribalMask), typeof(HornedTribalMask)
	};
	#endregion

	public bool IsMarkable(Type type)
	{
		if (ForceNonExceptional) //Don't even display the stuff for marking if it can't ever be exceptional.
		{
			return false;
		}

		for (int i = 0; i < MMarkableTable.Length; ++i)
		{
			if (type == MMarkableTable[i] || type.IsSubclassOf(MMarkableTable[i]))
			{
				return true;
			}
		}

		return false;
	}

	public static bool RetainsColor(Type type)
	{
		bool neverColor = false;

		for (int i = 0; !neverColor && i < MNeverColorTable.Length; ++i)
		{
			neverColor = type == MNeverColorTable[i] || type.IsSubclassOf(MNeverColorTable[i]);
		}

		if (neverColor)
		{
			return false;
		}

		bool inItemTable = false;

		for (int i = 0; !inItemTable && i < MColoredItemTable.Length; ++i)
		{
			inItemTable = (type == MColoredItemTable[i] || type.IsSubclassOf(MColoredItemTable[i]));
		}

		return inItemTable;
	}

	public bool RetainsColorFrom(CraftSystem system, Type type)
	{
		if (system.RetainsColorFrom(this, type))
		{
			return true;
		}

		bool inItemTable = RetainsColor(type);

		if (!inItemTable)
		{
			return false;
		}

		bool inResourceTable = false;

		//for (int i = 0; !inItemTable && i < MColoredItemTable.Length; ++i)
		//{
		//	inItemTable = (ItemType == MColoredItemTable[i] || ItemType.IsSubclassOf(MColoredItemTable[i]));
		//}

		for (int i = 0; !inResourceTable && i < MColoredResourceTable.Length; ++i)
		{
			inResourceTable = (type == MColoredResourceTable[i] || type.IsSubclassOf(MColoredResourceTable[i]));
		}

		return inResourceTable;
	}

	public bool RetainsColorFromCloth(Item item)
	{
		Type t = item.GetType();

		foreach (var type in MClothColoredItemTable)
		{
			if (type == t)
			{
				return true;
			}
		}

		return false;
	}

	public bool Find(Mobile from, int[] itemIDs)
	{
		Map map = from.Map;

		if (map == null)
		{
			return false;
		}

		IPooledEnumerable eable = map.GetItemsInRange(from.Location, 2);

		if (eable.Cast<Item>().Any(item => item.Z + 16 > from.Z && from.Z + 16 > item.Z && Find(item.ItemId, itemIDs)))
		{
			eable.Free();
			return true;
		}

		eable.Free();

		for (int x = -2; x <= 2; ++x)
		{
			for (int y = -2; y <= 2; ++y)
			{
				int vx = from.X + x;
				int vy = from.Y + y;

				var tiles = map.Tiles.GetStaticTiles(vx, vy, true);

				for (var i = 0; i < tiles.Length; ++i)
				{
					int z = tiles[i].Z;
					int id = tiles[i].ID;

					if (z + 16 > from.Z && from.Z + 16 > z && Find(id, itemIDs))
					{
						return true;
					}
				}
			}
		}

		return false;
	}

	public bool Find(int itemId, int[] itemIDs)
	{
		bool contains = false;

		for (int i = 0; !contains && i < itemIDs.Length; i += 2)
		{
			contains = itemId >= itemIDs[i] && itemId <= itemIDs[i + 1];
		}

		return contains;
	}

	private bool FindWater(Mobile m)
	{
		Map map = m.Map;

		if (map == null)
		{
			return false;
		}

		return false;
	}

	public bool IsQuantityType(Type[][] types)
	{
		for (int i = 0; i < types.Length; ++i)
		{
			var check = types[i];

			for (int j = 0; j < check.Length; ++j)
			{
				if (typeof(IHasQuantity).IsAssignableFrom(check[j]))
				{
					return true;
				}
			}
		}

		return false;
	}

	#region SA
	public bool IsPlantHueType(Type[][] types)
	{
		for (int i = 0; i < types.Length; ++i)
		{
			Type[] check = types[i];

			for (int j = 0; j < check.Length; ++j)
			{
				if (typeof(IPlantHue).IsAssignableFrom(check[j]))
				{
					return true;
				}

				if (typeof(IPigmentHue).IsAssignableFrom(check[j]))
				{
					return true;
				}
			}
		}

		return false;
	}

	public int ConsumeQuantityByPlantHue(Mobile from, CraftSystem craftSystem, Container cont, Type[][] types, int[] amounts)
	{
		if (types.Length != amounts.Length)
		{
			throw new ArgumentException(nameof(amounts));
		}

		CraftContext context = craftSystem.GetContext(from);

		if (context == null)
		{
			return 0;
		}

		Item[][] items = new Item[types.Length][];
		int[] totals = new int[types.Length];

		for (int i = 0; i < types.Length; ++i)
		{
			items[i] = cont.FindItemsByType(types[i], true);

			for (int j = 0; j < items[i].Length; ++j)
			{
				if (items[i][j] is IPlantHue plantHue && plantHue.PlantHue != context.RequiredPlantHue)
				{
					continue;
				}

				if (items[i][j] is IPigmentHue pigmentHue && pigmentHue.PigmentHue != context.RequiredPigmentHue)
				{
					continue;
				}

				totals[i] += items[i][j].Amount;
			}

			if (totals[i] < amounts[i])
			{
				return i;
			}
		}

		for (int i = 0; i < types.Length; ++i)
		{
			int need = amounts[i];

			for (int j = 0; j < items[i].Length; ++j)
			{
				Item item = items[i][j];

				int theirAmount = item.Amount;

				switch (item)
				{
					case IPlantHue ph when ph.PlantHue != context.RequiredPlantHue:
					case IPigmentHue pigh when pigh.PigmentHue != context.RequiredPigmentHue:
						continue;
				}

				if (theirAmount < need)
				{
					OnResourceConsumed(item, theirAmount);

					item.Delete();
					need -= theirAmount;
				}
				else
				{
					OnResourceConsumed(item, need);

					item.Consume(need);
					break;
				}
			}
		}

		return -1;
	}
	#endregion

	public int ConsumeQuantity(Container cont, Type[][] types, int[] amounts)
	{
		if (types.Length != amounts.Length)
		{
			throw new ArgumentException(nameof(amounts));
		}

		var items = new Item[types.Length][];
		var totals = new int[types.Length];

		for (int i = 0; i < types.Length; ++i)
		{
			items[i] = cont.FindItemsByType(types[i], true);

			for (int j = 0; j < items[i].Length; ++j)
			{
				if (items[i][j] is not IHasQuantity hq)
				{
					totals[i] += items[i][j].Amount;
				}
				else
				{
					if (hq is BaseBeverage beverage && beverage.Content != RequiredBeverage)
					{
						continue;
					}

					totals[i] += hq.Quantity;
				}
			}

			if (totals[i] < amounts[i])
			{
				return i;
			}
		}

		for (int i = 0; i < types.Length; ++i)
		{
			int need = amounts[i];

			for (int j = 0; j < items[i].Length; ++j)
			{
				Item item = items[i][j];

				if (item is not IHasQuantity hq)
				{
					int theirAmount = item.Amount;

					if (theirAmount < need)
					{
						item.Delete();
						need -= theirAmount;
					}
					else
					{
						item.Consume(need);
						break;
					}
				}
				else
				{
					if (hq is BaseBeverage beverage && beverage.Content != RequiredBeverage)
					{
						continue;
					}

					int theirAmount = hq.Quantity;

					if (theirAmount < need)
					{
						hq.Quantity -= theirAmount;
						need -= theirAmount;
					}
					else
					{
						hq.Quantity -= need;
						break;
					}
				}
			}
		}

		return -1;
	}

	public int GetQuantity(Container cont, Type[] types)
	{
		var items = cont.FindItemsByType(types, true);

		int amount = 0;

		for (int i = 0; i < items.Length; ++i)
		{
			if (items[i] is not IHasQuantity hq)
			{
				amount += items[i].Amount;
			}
			else
			{
				if (hq is BaseBeverage beverage && beverage.Content != RequiredBeverage)
				{
					continue;
				}

				amount += hq.Quantity;
			}
		}

		return amount;
	}

	#region SA
	public int GetPlantHueAmount(Mobile from, CraftSystem craftSystem, Container cont, Type[] types)
	{
		Item[] items = cont.FindItemsByType(types, true);
		CraftContext context = craftSystem.GetContext(from);

		int amount = 0;

		for (int i = 0; i < items.Length; ++i)
		{
			if (context == null || (items[i] is IPlantHue ph && ph.PlantHue != context.RequiredPlantHue))
			{
				continue;
			}

			if (items[i] is IPigmentHue pigh && pigh.PigmentHue != context.RequiredPigmentHue)
			{
				continue;
			}

			amount += items[i].Amount;
		}

		return amount;
	}
	#endregion

	public bool ConsumeRes(
		Mobile from,
		Type typeRes,
		CraftSystem craftSystem,
		ref int resHue,
		ref int maxAmount,
		ConsumeType consumeType,
		ref object message)
	{
		return ConsumeRes(from, typeRes, craftSystem, ref resHue, ref maxAmount, consumeType, ref message, false);
	}

	public bool ConsumeRes(
		Mobile from,
		Type typeRes,
		CraftSystem craftSystem,
		ref int resHue,
		ref int maxAmount,
		ConsumeType consumeType,
		ref object message,
		bool isFailure)
	{
		Container ourPack = from.Backpack;

		if (ourPack == null)
		{
			return false;
		}

		if (ourPack.TotalItems >= ourPack.MaxItems || ourPack.TotalWeight >= ourPack.MaxWeight)
		{
			message = 1048147; // Your backpack can't hold anything else.
			return false;
		}

		if (ConsumeResCallback != null)
		{
			int resMessage = ConsumeResCallback(from, consumeType);

			if (resMessage > 0)
			{
				message = resMessage;
				return false;
			}
		}

		if (NeedHeat && !Find(from, MHeatSources))
		{
			message = 1044487; // You must be near a fire source to cook.
			return false;
		}

		if (NeedOven && !Find(from, MOvens))
		{
			message = 1044493; // You must be near an oven to bake that.
			return false;
		}

		if (NeedMaker && !Find(from, MMakers))
		{
			message = 1155732; // You must be near a steam powered beverage maker to do that.
			return false;
		}

		if (NeedMill && !Find(from, MMills))
		{
			message = 1044491; // You must be near a flour mill to do that.
			return false;
		}

		if (NeedWater && !Find(from, MWaterSources) && !FindWater(from))
		{
			message = 1158882; // You must be near a water source such as a water trough to craft this item.
			return false;
		}

		var types = new Type[Resources.Count][];
		var amounts = new int[Resources.Count];

		maxAmount = int.MaxValue;

		CraftSubResCol resCol = UseSubRes2 ? craftSystem.CraftSubRes2 : craftSystem.CraftSubRes;
		MasterCraftsmanTalisman talisman = null;

		for (int i = 0; i < types.Length; ++i)
		{
			CraftRes craftRes = Resources.GetAt(i);
			Type baseType = craftRes.ItemType;

			if (typeRes != null && ForceTypeRes)
			{
				if (MResourceConversionTable.TryGetValue(typeRes, out Type outType))
				{
					baseType = outType;
				}
			}

			// Resource Mutation
			if ((baseType == resCol.ResType) && (typeRes != null))
			{
				baseType = typeRes;

				CraftSubRes subResource = resCol.SearchFor(baseType);

				if (subResource != null && from.Skills[craftSystem.MainSkill].Base < subResource.RequiredSkill)
				{
					message = subResource.Message;
					return false;
				}
			}
			// ******************

			for (int j = 0; types[i] == null && j < ItemTypesTable.Length; ++j)
			{
				if (ItemTypesTable[j][0] == baseType)
				{
					types[i] = ItemTypesTable[j];
				}
			}

			if (types[i] == null)
			{
				types[i] = new[] { baseType };
			}

			amounts[i] = craftRes.Amount;

			// For stackable items that can ben crafted more than one at a time
			if (UseAllRes)
			{
				int tempAmount = ourPack.GetAmount(types[i]);
				tempAmount /= amounts[i];

				if (tempAmount < maxAmount)
				{
					maxAmount = tempAmount;

					if (maxAmount == 0)
					{
						CraftRes res = Resources.GetAt(i);

						if (res.MessageNumber > 0)
						{
							message = res.MessageNumber;
						}
						else if (!string.IsNullOrEmpty(res.MessageString))
						{
							message = res.MessageString;
						}
						else
						{
							message = 502925; // You don't have the resources required to make that item.
						}

						return false;
					}
				}
			}
			// ****************************

			if (isFailure && (talisman != null || !craftSystem.ConsumeOnFailure(from, types[i][0], this, ref talisman)))
			{
				amounts[i] = 0;
			}
		}

		if (talisman != null)
		{
			talisman.Charges--;
		}

		// We adjust the amount of each resource to consume the max posible
		if (UseAllRes && consumeType != ConsumeType.Half)
		{
			for (int i = 0; i < amounts.Length; ++i)
			{
				amounts[i] *= maxAmount;
			}
		}
		else
		{
			maxAmount = -1;
		}

		Item consumeExtra = null;

		if (NameNumber == 1041267)
		{
			// Runebooks are a special case, they need a blank recall rune
			var runes = ourPack.FindItemsByType<RecallRune>();

			for (int i = 0; i < runes.Count; ++i)
			{
				RecallRune rune = runes[i];

				if (rune != null && !rune.Marked)
				{
					consumeExtra = rune;
					break;
				}
			}

			if (consumeExtra == null)
			{
				message = 1044253; // You don't have the components needed to make that.
				return false;
			}
		}

		int index;

		// Consume ALL
		if (consumeType == ConsumeType.All)
		{
			_mResHue = 0;
			_mResAmount = 0;
			_mSystem = craftSystem;
			CaddelliteCraft = true;

			if (IsQuantityType(types))
			{
				index = ConsumeQuantity(ourPack, types, amounts);
			}
			else if (IsPlantHueType(types))
			{
				index = ConsumeQuantityByPlantHue(from, craftSystem, ourPack, types, amounts);
			}
			else
			{
				index = ourPack.ConsumeTotalGrouped(types, amounts, true, ResourceValidator, OnResourceConsumed, CheckHueGrouping);
			}

			resHue = _mResHue;
		}
		// Consume Half ( for use all resource craft type )
		else if (consumeType == ConsumeType.Half)
		{
			for (int i = 0; i < amounts.Length; i++)
			{
				amounts[i] /= 2;

				if (amounts[i] < 1)
				{
					amounts[i] = 1;
				}
			}

			_mResHue = 0;
			_mResAmount = 0;
			_mSystem = craftSystem;

			if (IsQuantityType(types))
			{
				index = ConsumeQuantity(ourPack, types, amounts);
			}
			else if (IsPlantHueType(types))
			{
				index = ConsumeQuantityByPlantHue(from, craftSystem, ourPack, types, amounts);
			}
			else
			{
				index = ourPack.ConsumeTotalGrouped(types, amounts, true, ResourceValidator, OnResourceConsumed, CheckHueGrouping);
			}

			resHue = _mResHue;
		}
		else
			// ConstumeType.None ( it's basicaly used to know if the crafter has enough resource before starting the process )
		{
			index = -1;

			if (IsQuantityType(types))
			{
				for (int i = 0; i < types.Length; i++)
				{
					if (GetQuantity(ourPack, types[i]) >= amounts[i]) continue;
					index = i;
					break;
				}
			}
			else if (IsPlantHueType(types))
			{
				craftSystem.GetContext(from);

				for (int i = 0; i < types.Length; i++)
				{
					if (GetPlantHueAmount(from, craftSystem, ourPack, types[i]) >= amounts[i]) continue;
					index = i;
					break;
				}
			}
			else
			{
				for (int i = 0; i < types.Length; i++)
				{
					if (ourPack.GetBestGroupAmount(types[i], true, CheckHueGrouping) >= amounts[i]) continue;
					index = i;
					break;
				}
			}
		}

		if (index == -1)
		{
			if (consumeType == ConsumeType.None) return true;
			consumeExtra?.Delete();

			return true;
		}
		else
		{
			CraftRes res = Resources.GetAt(index);

			if (res.MessageNumber > 0)
			{
				message = res.MessageNumber;
			}
			else if (!string.IsNullOrEmpty(res.MessageString))
			{
				message = res.MessageString;
			}
			else
			{
				message = 502925; // You don't have the resources required to make that item.
			}

			return false;
		}
	}

	private int _mResHue;
	private int _mClothHue;
	private int _mResAmount;
	private CraftSystem _mSystem;

	public bool CaddelliteCraft { get; private set; }

	private PlantHue _mPlantHue = PlantHue.None;
	private PlantPigmentHue _mPlantPigmentHue = PlantPigmentHue.None;

	private void OnResourceConsumed(Item item, int amount)
	{
		#region Plant Pigments
		switch (item)
		{
			case IPlantHue hue:
				_mPlantHue = hue.PlantHue;
				break;
			case IPigmentHue hue1:
				_mPlantPigmentHue = hue1.PigmentHue;
				break;
		}
		#endregion

		if (!RetainsColorFrom(_mSystem, item.GetType()))
		{
			return;
		}

		if (item is Cloth or UncutCloth)
		{
			_mClothHue = item.Hue;
		}

		if (amount < _mResAmount) return;
		_mResHue = item.Hue;
		_mResAmount = amount;
	}

	private static int CheckHueGrouping(Item a, Item b)
	{
		return b.Hue.CompareTo(a.Hue);
	}

	public bool ResourceValidator(Item item)
	{
		// VvV items or Faction Items cannot be used as resources
		return item is not IFactionItem factionItem || factionItem.FactionItemState == null;
	}

	public double GetExceptionalChance(CraftSystem system, double chance, Mobile from)
	{
		if (ForceNonExceptional)
		{
			return 0.0;
		}

		if (ForceExceptional)
		{
			bool allRequiredSkills = false;
			GetSuccessChance(from, null, system, false, ref allRequiredSkills);

			if (allRequiredSkills)
			{
				return 100.0;
			}
		}

		double bonus = 0.0;

		if (from.Talisman is BaseTalisman talisman)
		{
			if (talisman.CheckSkill(system))
			{
				bonus = talisman.ExceptionalBonus / 100.0;
			}
		}

		switch (system.Eca)
		{
			default:
			case CraftEca.ChanceMinusSixty:
				chance -= 0.6;
				break;
			case CraftEca.FiftyPercentChanceMinusTenPercent:
				chance = chance * 0.5 - 0.1;
				break;
			case CraftEca.ChanceMinusSixtyToFourtyFive:
			{
				double offset = 0.60 - ((from.Skills[system.MainSkill].Value - 95.0) * 0.03);

				offset = offset switch
				{
					< 0.45 => 0.45,
					> 0.60 => 0.60,
					_ => offset
				};

				chance -= offset;
				break;
			}
		}

		if (chance > 0)
		{
			return chance + bonus;
		}

		return chance;
	}

	public bool CheckSkills(
		Mobile from, Type typeRes, CraftSystem craftSystem, ref int quality, ref bool allRequiredSkills, int maxAmount)
	{
		return CheckSkills(from, typeRes, craftSystem, ref quality, ref allRequiredSkills, true, maxAmount);
	}

	public bool CheckSkills(
		Mobile from, Type typeRes, CraftSystem craftSystem, ref int quality, ref bool allRequiredSkills, bool gainSkills, int maxAmount)
	{
		double chance = GetSuccessChance(from, typeRes, craftSystem, gainSkills, ref allRequiredSkills, maxAmount);

		if (GetExceptionalChance(craftSystem, chance, from) > Utility.RandomDouble())
		{
			quality = 2;
		}

		return (chance > Utility.RandomDouble());
	}

	public double GetSuccessChance(Mobile from, Type typeRes, CraftSystem craftSystem, bool gainSkills, ref bool allRequiredSkills)
	{
		return GetSuccessChance(from, typeRes, craftSystem, gainSkills, ref allRequiredSkills, 1);
	}

	public double GetSuccessChance(Mobile from, Type typeRes, CraftSystem craftSystem, bool gainSkills, ref bool allRequiredSkills, int maxAmount)
	{
		if (ForceSuccessChance > -1)
		{
			return ForceSuccessChance / 100.0;
		}

		double minMainSkill = 0.0;
		double maxMainSkill = 0.0;
		double valMainSkill = 0.0;

		allRequiredSkills = true;

		for (int i = 0; i < Skills.Count; i++)
		{
			CraftSkill craftSkill = Skills.GetAt(i);

			double minSkill = craftSkill.MinSkill - MinSkillOffset;
			double maxSkill = craftSkill.MaxSkill;
			double valSkill = from.Skills[craftSkill.SkillToMake].Value;

			if (valSkill < minSkill)
			{
				allRequiredSkills = false;
			}

			if (craftSkill.SkillToMake == craftSystem.MainSkill)
			{
				minMainSkill = minSkill;
				maxMainSkill = maxSkill;
				valMainSkill = valSkill;
			}

			if (gainSkills && !UseAllRes) // This is a passive check. Success chance is entirely dependant on the main skill
			{
				from.CheckSkill(craftSkill.SkillToMake, minSkill, maxSkill);
			}
		}

		double chance;

		if (allRequiredSkills)
		{
			chance = craftSystem.GetChanceAtMin(this) +
			         (valMainSkill - minMainSkill) / (maxMainSkill - minMainSkill) * (1.0 - craftSystem.GetChanceAtMin(this));
		}
		else
		{
			chance = 0.0;
		}

		if (allRequiredSkills && from.Talisman is BaseTalisman talisman)
		{
			if (talisman.CheckSkill(craftSystem))
			{
				chance += talisman.SuccessBonus / 100.0;
			}
		}

		if (allRequiredSkills && Math.Abs(valMainSkill - maxMainSkill) < 125)
		{
			chance = 1.0;
		}

		return chance;
	}

	private void MultipleSkillCheck(Mobile from, int amount)
	{
		for (int i = 0; i < Skills.Count; i++)
		{
			CraftSkill craftSkill = Skills.GetAt(i);

			Misc.SkillCheck.CheckSkill(from, craftSkill.SkillToMake, craftSkill.MinSkill - MinSkillOffset, craftSkill.MaxSkill, amount);
		}
	}

	public void Craft(Mobile from, CraftSystem craftSystem, Type typeRes, ITool tool)
	{
		if (from.BeginAction(typeof(CraftSystem)))
		{
			if (RequiredExpansion == Expansion.None ||
			    (from.NetState != null && from.NetState.SupportsExpansion(RequiredExpansion)))
			{
				bool allRequiredSkills = true;
				double chance = GetSuccessChance(from, typeRes, craftSystem, false, ref allRequiredSkills);

				if (allRequiredSkills && chance >= 0.0)
				{
					if (Recipe == null || !(from is PlayerMobile mobile) || mobile.HasRecipe(Recipe))
					{
						if (!RequiresBasketWeaving || from is PlayerMobile {BasketWeaving: true})
						{
							if (!RequiresMechanicalLife || from is PlayerMobile {MechanicalLife: true})
							{
								int badCraft = craftSystem.CanCraft(from, tool, ItemType);

								if (badCraft <= 0)
								{
									if (RequiresResTarget && NeedsResTarget(from, craftSystem))
									{
										from.Target = new ChooseResTarget(from, this, craftSystem, typeRes, tool);
										from.SendMessage("Choose the resource you would like to use.");
										return;
									}

									int resHue = 0;
									int maxAmount = 0;
									object message = null;

									if (ConsumeRes(from, typeRes, craftSystem, ref resHue, ref maxAmount, ConsumeType.None, ref message))
									{
										message = null;

										if (ConsumeAttributes(from, ref message, false))
										{
											CraftContext context = craftSystem.GetContext(from);

											context?.OnMade(this);

											int iMin = craftSystem.MinCraftEffect;
											int iMax = (craftSystem.MaxCraftEffect - iMin) + 1;
											int iRandom = Utility.Random(iMax);
											iRandom += iMin + 1;
											new InternalTimer(from, craftSystem, this, typeRes, tool, iRandom).Start();
											return;
										}
										else
										{
											from.EndAction(typeof(CraftSystem));
											from.SendGump(new CraftGump(from, craftSystem, tool, message));
										}
									}
									else
									{
										from.EndAction(typeof(CraftSystem));
										from.SendGump(new CraftGump(from, craftSystem, tool, message));
									}
								}
								else
								{
									from.EndAction(typeof(CraftSystem));
									from.SendGump(new CraftGump(from, craftSystem, tool, badCraft));
								}
							}
							else
							{
								from.EndAction(typeof(CraftSystem));
								from.SendGump(new CraftGump(from, craftSystem, tool, 1113034)); // You haven't read the Mechanical Life Manual. Talking to Sutek might help!
							}
						}
						else
						{
							from.EndAction(typeof(CraftSystem));
							from.SendGump(new CraftGump(from, craftSystem, tool, 1112253)); // You haven't learned basket weaving. Perhaps studying a book would help!
						}
					}
					else
					{
						mobile.EndAction(typeof(CraftSystem));
						mobile.SendGump(new CraftGump(mobile, craftSystem, tool, 1072847)); // You must learn that recipe from a scroll.
					}
				}
				else
				{
					from.EndAction(typeof(CraftSystem));
					from.SendGump(new CraftGump(from, craftSystem, tool, 1044153));
					// You don't have the required skills to attempt this item.
				}
			}
			else
			{
				from.EndAction(typeof(CraftSystem));
				from.SendGump(new CraftGump(from, craftSystem, tool, RequiredExpansionMessage(RequiredExpansion)));
				//The {0} expansion is required to attempt this item.
			}
		}
		else
		{
			from.SendLocalizedMessage(500119); // You must wait to perform another action
		}

		AutoCraftTimer.EndTimer(from);
	}

	private static object RequiredExpansionMessage(Expansion expansion)
	{
		return expansion switch
		{
			Expansion.SE => 1063307 // The "Samurai Empire" expansion is required to attempt this item.
			,
			Expansion.ML => 1072650 // The "Mondain's Legacy" expansion is required to attempt this item.
			,
			Expansion.SA => 1094731 // You must have the Stygian Abyss expansion pack to use this feature.
			,
			Expansion.HS => 1116295 // You must have the High Seas booster pack to use this feature
			,
			Expansion.TOL => 1155875 // You must have the Time of Legends expansion to use this feature.
			,
			_ => $"The \"{ExpansionInfo.GetInfo(expansion).Name}\" expansion is required to attempt this item."
		};
	}

	public void CompleteCraft(
		int quality,
		bool makersMark,
		Mobile from,
		CraftSystem craftSystem,
		Type typeRes,
		ITool tool,
		CustomCraft customCraft)
	{
		int badCraft = craftSystem.CanCraft(from, tool, ItemType);

		if (badCraft > 0)
		{
			if (tool is {Deleted: false, UsesRemaining: > 0})
			{
				from.SendGump(new CraftGump(from, craftSystem, tool, badCraft));
			}
			else
			{
				from.SendLocalizedMessage(badCraft);
			}

			AutoCraftTimer.EndTimer(from);

			return;
		}

		int checkResHue = 0, checkMaxAmount = 0;
		object checkMessage = null;

		// Not enough resource to craft it
		if (!ConsumeRes(from, typeRes, craftSystem, ref checkResHue, ref checkMaxAmount, ConsumeType.None, ref checkMessage))
		{
			if (tool is {Deleted: false, UsesRemaining: > 0})
			{
				from.SendGump(new CraftGump(from, craftSystem, tool, checkMessage));
			}
			else switch (checkMessage)
			{
				case int message and > 0:
					from.SendLocalizedMessage(message);
					break;
				case string message:
					from.SendMessage(message);
					break;
			}

			AutoCraftTimer.EndTimer(from);

			return;
		}

		if (!ConsumeAttributes(from, ref checkMessage, false))
		{
			if (tool is {Deleted: false, UsesRemaining: > 0})
			{
				from.SendGump(new CraftGump(from, craftSystem, tool, checkMessage));
			}
			else switch (checkMessage)
			{
				case int message and > 0:
					from.SendLocalizedMessage(message);
					break;
				case string message2:
					from.SendMessage(message2);
					break;
			}

			AutoCraftTimer.EndTimer(from);

			return;
		}

		bool toolBroken = false;

		int ignored = 1;
		int endquality = 1;

		bool allRequiredSkills = true;

		if (CheckSkills(from, typeRes, craftSystem, ref ignored, ref allRequiredSkills, checkMaxAmount))
		{
			// Resource
			int resHue = 0;
			int maxAmount = 0;

			object message = null;

			// Not enough resource to craft it
			if (!ConsumeRes(from, typeRes, craftSystem, ref resHue, ref maxAmount, ConsumeType.All, ref message))
			{
				if (tool is {Deleted: false, UsesRemaining: > 0})
				{
					from.SendGump(new CraftGump(from, craftSystem, tool, message));
				}
				else switch (message)
				{
					case int i and > 0:
						from.SendLocalizedMessage(i);
						break;
					case string s:
						from.SendMessage(s);
						break;
				}

				AutoCraftTimer.EndTimer(from);

				return;
			}
			else if (!ConsumeAttributes(from, ref message, true))
			{
				if (tool is {Deleted: false, UsesRemaining: > 0})
				{
					from.SendGump(new CraftGump(from, craftSystem, tool, message));
				}
				else switch (message)
				{
					case int i and > 0:
						from.SendLocalizedMessage(i);
						break;
					case string s:
						from.SendMessage(s);
						break;
				}

				AutoCraftTimer.EndTimer(from);

				return;
			}

			if (UseAllRes && maxAmount > 0)
			{
				MultipleSkillCheck(from, maxAmount);
			}

			if (craftSystem is DefBlacksmithy)
			{
				if (from.FindItemOnLayer(Layer.OneHanded) is AncientSmithyHammer hammer && hammer != tool)
				{
					if (Core.ML && hammer is HammerOfHephaestus)
					{
						if (hammer.UsesRemaining > 0)
						{
							hammer.UsesRemaining--;
						}

						if (hammer.UsesRemaining < 1)
						{
							from.PlaceInBackpack(hammer);
						}
					}
					else
					{
						hammer.UsesRemaining--;

						if (hammer.UsesRemaining < 1)
						{
							hammer.Delete();
						}
					}
				}
			}

			int num = 0;

			Item item;
			if (customCraft != null)
			{
				item = customCraft.CompleteCraft(out num);
			}
			else if (!Core.SA && typeof(MapItem).IsAssignableFrom(ItemType) && from.Map != Map.Trammel && from.Map != Map.Felucca)
			{
				item = new IndecipherableMap();
				from.SendLocalizedMessage(1070800); // The map you create becomes mysteriously indecipherable.
			}
			else if (CreateItem != null)
			{
				item = CreateItem(from, this, tool);
			}
			else
			{
				item = Activator.CreateInstance(ItemType) as Item;
			}

			if (item != null)
			{
				if (item is Board)
				{
					Type resourceType = typeRes;

					if (resourceType == null)
					{
						resourceType = Resources.GetAt(0).ItemType;
					}

					CraftResource thisResource = CraftResources.GetFromType(resourceType);
					Item oldItem = item;

					item = thisResource switch
					{
						CraftResource.OakWood => new OakBoard(),
						CraftResource.AshWood => new AshBoard(),
						CraftResource.YewWood => new YewBoard(),
						CraftResource.Heartwood => new HeartwoodBoard(),
						CraftResource.Bloodwood => new BloodwoodBoard(),
						CraftResource.Frostwood => new FrostwoodBoard(),
						_ => new Board()
					};

					if (item != oldItem)
					{
						oldItem.Delete();
					}
				}

				CraftContext context = craftSystem.GetContext(from);
				int originalHue = item.Hue;

				if (item is ICraftable craftable)
				{
					endquality = craftable.OnCraft(quality, makersMark, from, craftSystem, typeRes, tool, this, resHue);
				}
				else if (item.Hue == 0)
				{
					item.Hue = resHue;
				}

				if (item.Hue == 0 && RetainsColorFromCloth(item) && _mClothHue != 0)
				{
					item.Hue = _mClothHue;
				}

				// This takes into account for natural hues, ie plant hues
				if (item.Hue != originalHue && context.DoNotColor)
				{
					item.Hue = originalHue;
				}

				if (maxAmount > 0)
				{
					if (!item.Stackable && item is IUsesRemaining)
					{
						((IUsesRemaining)item).UsesRemaining *= maxAmount;
					}
					else
					{
						item.Amount = maxAmount;
					}
				}

				#region Plant Pigments
				if (_mPlantHue != PlantHue.None)
				{
					switch (item)
					{
						case IPlantHue hue:
							hue.PlantHue = _mPlantHue;
							break;
						case IPigmentHue hue2:
							hue2.PigmentHue = PlantPigmentHueInfo.HueFromPlantHue(_mPlantHue);
							break;
					}
				}
				else if (_mPlantPigmentHue != PlantPigmentHue.None && item is IPigmentHue)
				{
					((IPigmentHue)item).PigmentHue = _mPlantPigmentHue;
				}

				if (context.QuestOption == CraftQuestOption.QuestItem)
				{
					PlayerMobile px = from as PlayerMobile;

					if (!QuestHelper.CheckItem(px, item))
					{
						from.SendLocalizedMessage(1072355, null, 0x23); // That item does not match any of your quest criteria	
					}
				}

				context.RequiredPigmentHue = PlantPigmentHue.None;
				context.RequiredPlantHue = PlantHue.None;

				_mPlantHue = PlantHue.None;
				_mPlantPigmentHue = PlantPigmentHue.None;
				#endregion

				MutateAction?.Invoke(from, item, tool);

				if ((tool as Item)?.Parent is Container)
				{
					Container cntnr = (Container)((Item)tool).Parent;

					if (!cntnr.TryDropItem(from, item, false))
					{
						if (cntnr != from.Backpack)
						{
							from.AddToBackpack(item);
						}
						else
						{
							item.MoveToWorld(from.Location, from.Map);
						}
					}
				}
				else
				{
					from.AddToBackpack(item);
				}

				//EventSink.OnInvokeCraftSuccess(new CraftSuccessEventArgs(from, item, tool is Item ? (Item)tool : null));

				if (from.IsStaff())
				{
					CommandLogging.WriteLine(from, "Crafting {0} with craft system {1}", CommandLogging.Format(item), craftSystem.GetType().Name);
				}
			}

			tool.UsesRemaining--;

			#region Mondain's Legacy
			if (tool is HammerOfHephaestus)
			{
				if (tool.UsesRemaining < 1)
				{
					tool.UsesRemaining = 0;
				}
			}
			#endregion
			else
			{
				if (tool.UsesRemaining < 1 && tool.BreakOnDepletion)
				{
					toolBroken = true;
				}

				if (toolBroken)
				{
					tool.Delete();
				}
			}

			if (num == 0)
			{
				num = craftSystem.PlayEndingEffect(from, false, true, toolBroken, endquality, makersMark, this);
			}

			bool queryFactionImbue = false;
			int availableSilver = 0;
			FactionItemDefinition def = null;
			Faction faction = null;

			if (item is IFactionItem)
			{
				def = FactionItemDefinition.Identify(item);

				if (def != null)
				{
					faction = Faction.Find(from);

					if (faction != null)
					{
						Town town = Town.FromRegion(from.Region);

						if (town != null && town.Owner == faction)
						{
							Container pack = from.Backpack;

							if (pack != null)
							{
								availableSilver = pack.GetAmount(typeof(Silver));

								if (availableSilver >= def.SilverCost)
								{
									queryFactionImbue = Faction.IsNearType(from, def.VendorType, 12);
								}
							}
						}
					}
				}
			}

			// TODO: Scroll imbuing

			if (queryFactionImbue)
			{
				from.SendGump(new FactionImbueGump(quality, item, from, craftSystem, tool, num, availableSilver, faction, def));
			}
			else if (tool is {Deleted: false, UsesRemaining: > 0})
			{
				from.SendGump(new CraftGump(from, craftSystem, tool, num));
			}
			else if (num > 0)
			{
				from.SendLocalizedMessage(num);
			}
		}
		else if (!allRequiredSkills)
		{
			if (tool is {Deleted: false, UsesRemaining: > 0})
			{
				from.SendGump(new CraftGump(from, craftSystem, tool, 1044153));
			}
			else
			{
				from.SendLocalizedMessage(1044153); // You don't have the required skills to attempt this item.
			}

			AutoCraftTimer.EndTimer(from);
		}
		else
		{
			ConsumeType consumeType = UseAllRes ? ConsumeType.Half : ConsumeType.All;
			int resHue = 0;
			int maxAmount = 0;

			object message = null;

			// Not enough resource to craft it
			if (!ConsumeRes(from, typeRes, craftSystem, ref resHue, ref maxAmount, consumeType, ref message, true))
			{
				if (tool is {Deleted: false, UsesRemaining: > 0})
				{
					from.SendGump(new CraftGump(from, craftSystem, tool, message));
				}
				else switch (message)
				{
					case int i when i > 0:
						from.SendLocalizedMessage(i);
						break;
					case string s:
						from.SendMessage(s);
						break;
				}

				AutoCraftTimer.EndTimer(from);

				return;
			}

			tool.UsesRemaining--;

			if (tool.UsesRemaining < 1 && tool.BreakOnDepletion)
			{
				toolBroken = true;
			}

			if (toolBroken)
			{
				tool.Delete();
			}

			if (UseAllRes)
			{
				MultipleSkillCheck(from, 1);
			}

			// SkillCheck failed.
			int num = craftSystem.PlayEndingEffect(from, true, true, toolBroken, endquality, false, this);

			if (!tool.Deleted && tool.UsesRemaining > 0)
			{
				from.SendGump(new CraftGump(from, craftSystem, tool, num));
			}
			else if (num > 0)
			{
				from.SendLocalizedMessage(num);
			}
		}
	}

	private class InternalTimer : Timer
	{
		private readonly Mobile _mFrom;
		private int _mICount;
		private readonly int _mICountMax;
		private readonly CraftItem _mCraftItem;
		private readonly CraftSystem _mCraftSystem;
		private readonly Type _itemTypeRes;
		private readonly ITool _mTool;
		private readonly bool _mAutoCraft;

		public InternalTimer(
			Mobile from, CraftSystem craftSystem, CraftItem craftItem, Type typeRes, ITool tool, int iCountMax)
			: base(TimeSpan.Zero, TimeSpan.FromSeconds(craftSystem.Delay), iCountMax)
		{
			_mFrom = from;
			_mCraftItem = craftItem;
			_mICount = 0;
			_mICountMax = iCountMax;
			_mCraftSystem = craftSystem;
			_itemTypeRes = typeRes;
			_mTool = tool;
			_mAutoCraft = AutoCraftTimer.HasTimer(from);
		}

		protected override void OnTick()
		{
			_mICount++;

			_mFrom.DisruptiveAction();

			if (_mICount < _mICountMax)
			{
				_mCraftSystem.PlayCraftEffect(_mFrom);
			}
			else
			{
				_mFrom.EndAction(typeof(CraftSystem));

				int badCraft = _mCraftSystem.CanCraft(_mFrom, _mTool, _mCraftItem.ItemType);

				if (badCraft > 0)
				{
					if (_mTool is {Deleted: false, UsesRemaining: > 0})
					{
						_mFrom.SendGump(new CraftGump(_mFrom, _mCraftSystem, _mTool, badCraft));
					}
					else
					{
						_mFrom.SendLocalizedMessage(badCraft);
					}

					AutoCraftTimer.EndTimer(_mFrom);

					return;
				}

				int quality = 1;
				bool allRequiredSkills = true;

				_mCraftItem.CheckSkills(_mFrom, _itemTypeRes, _mCraftSystem, ref quality, ref allRequiredSkills, false, 1);

				CraftContext context = _mCraftSystem.GetContext(_mFrom);

				if (context == null)
				{
					return;
				}

				if (typeof(CustomCraft).IsAssignableFrom(_mCraftItem.ItemType))
				{
					CustomCraft cc = null;

					try
					{
						cc =
							Activator.CreateInstance(
									_mCraftItem.ItemType, _mFrom, _mCraftItem, _mCraftSystem, _itemTypeRes, _mTool, quality) as
								CustomCraft;
					}
					catch
					{
						// ignored
					}

					cc?.EndCraftAction();

					return;
				}

				bool makersMark = false;

				if (quality == 2 && _mFrom.Skills[_mCraftSystem.MainSkill].Base >= 100.0)
				{
					makersMark = _mCraftItem.IsMarkable(_mCraftItem.ItemType);
				}

				if (makersMark && context.MarkOption == CraftMarkOption.PromptForMark && !_mAutoCraft)
				{
					_mFrom.SendGump(new QueryMakersMarkGump(quality, _mFrom, _mCraftItem, _mCraftSystem, _itemTypeRes, _mTool));
				}
				else
				{
					if (context.MarkOption == CraftMarkOption.DoNotMark)
					{
						makersMark = false;
					}

					_mCraftItem.CompleteCraft(quality, makersMark, _mFrom, _mCraftSystem, _itemTypeRes, _mTool, null);
				}
			}
		}
	}

	#region SA
	public static void RemoveResTarget(Mobile from)
	{
		if (MHasTarget.Contains(from))
		{
			MHasTarget.Remove(from);
		}
	}

	public static void AddResTarget(Mobile from)
	{
		if (!MHasTarget.Contains(from))
		{
			MHasTarget.Add(from);
		}
	}

	public static bool HasResTarget(Mobile from)
	{
		return MHasTarget.Contains(from);
	}

	private static readonly List<Mobile> MHasTarget = new();

	public bool NeedsResTarget(Mobile from, CraftSystem craftSystem)
	{
		CraftContext context = craftSystem.GetContext(from);

		if (context == null || HasResTarget(from))
		{
			return false;
		}

		Type[][] types = new Type[Resources.Count][];
		Container pack = from.Backpack;
		PlantHue hue = PlantHue.None;
		PlantPigmentHue phue = PlantPigmentHue.None;

		for (int i = 0; i < types.Length; ++i)
		{
			CraftRes craftRes = Resources.GetAt(i);
			Type type = craftRes.ItemType;

			if (pack != null)
			{
				Item[] items = pack.FindItemsByType(type);

				if (items == null) continue;
				switch (items.Length)
				{
					case > 0 when items[0] is IPlantHue:
						hue = ((IPlantHue)items[0]).PlantHue;
						break;
					case > 0 when items[0] is IPigmentHue:
						phue = ((IPigmentHue)items[0]).PigmentHue;
						break;
				}

				foreach (Item item in items)
				{
					switch (item)
					{
						case IPlantHue plantHue when plantHue.PlantHue != hue:
						case IPigmentHue pigmentHue when pigmentHue.PigmentHue != phue:
							return true;
					}
				}

				if (hue != PlantHue.None)
				{
					context.RequiredPlantHue = hue;
				}
				else if (phue != PlantPigmentHue.None)
				{
					context.RequiredPigmentHue = phue;
				}
			}
		}

		return false;
	}

	public class ChooseResTarget : Target
	{
		private readonly CraftItem _mCraftItem;
		private readonly CraftSystem _mCraftSystem;
		private readonly Type _itemTypeRes;
		private readonly ITool _mTool;

		public ChooseResTarget(Mobile from, CraftItem craftitem, CraftSystem craftSystem, Type typeRes, ITool tool)
			: base(-1, false, TargetFlags.None)
		{
			_mCraftItem = craftitem;
			_mCraftSystem = craftSystem;
			_itemTypeRes = typeRes;
			_mTool = tool;

			AddResTarget(from);
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			CraftContext context = _mCraftSystem.GetContext(from);

			if (context != null && targeted is IPlantHue hue)
			{
				context.RequiredPlantHue = hue.PlantHue;
			}
			else if (context != null && targeted is IPigmentHue pigmentHue)
			{
				context.RequiredPigmentHue = pigmentHue.PigmentHue;
			}

			from.EndAction(typeof(CraftSystem));
			_mCraftItem.Craft(from, _mCraftSystem, _itemTypeRes, _mTool);
		}

		protected override void OnTargetCancel(Mobile from, TargetCancelType cancelType)
		{
			from.EndAction(typeof(CraftSystem));
			from.SendGump(new CraftGump(from, _mCraftSystem, _mTool, null));
		}

		protected override void OnTargetFinish(Mobile from)
		{
			RemoveResTarget(from);
		}
	}
	#endregion
}
