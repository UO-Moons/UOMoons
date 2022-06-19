using Server.Items;
using System;

namespace Server.Engines.Craft;

public class DefCooking : CraftSystem
{
	public override SkillName MainSkill => SkillName.Cooking;

	public override int GumpTitleNumber => 1044003;

	private static CraftSystem _mCraftSystem;

	public static CraftSystem CraftSystem => _mCraftSystem ??= new DefCooking();

	public override CraftEca Eca => CraftEca.ChanceMinusSixtyToFourtyFive;

	public override double GetChanceAtMin(CraftItem item)
	{
		return 0.0; // 0%
	}

	public DefCooking() : base(1, 1, 1.25)// base( 1, 1, 1.5 )
	{
		_mCraftSystem = this;
	}

	public override int CanCraft(Mobile from, ITool tool, Type itemType)
	{
		int num = 0;

		if (tool == null || tool.Deleted || tool.UsesRemaining <= 0)
		{
			return 1044038; // You have worn out your tool!
		}

		return !tool.CheckAccessible(from, ref num) ? num : 0;
	}

	public override void PlayCraftEffect(Mobile from)
	{
	}

	public override int PlayEndingEffect(Mobile from, bool failed, bool lostMaterial, bool toolBroken, int quality, bool makersMark, CraftItem item)
	{
		if (toolBroken)
		{
			from.SendLocalizedMessage(1044038); // You have worn out your tool
		}

		if (failed)
		{
			return lostMaterial ? 1044043 : 1044157;// You failed to create the item, and some of your materials are lost.// You failed to create the item, but no materials were lost.
		}

		if (quality == 0)
		{
			return 502785; // You were barely able to make this item.  It's quality is below average.
		}

		if (makersMark && quality == 2)
		{
			return 1044156; // You create an exceptional quality item and affix your maker's mark.
		}

		return quality == 2 ? 1044155 : 1044154;// You create an exceptional quality item.// You create the item.
	}

	public override void InitCraftList()
	{
		/* Begin Ingredients */
		int index = AddCraft(typeof(SackFlour), 1044495, 1024153, 0.0, 100.0, typeof(WheatSheaf), 1044489, 2, 1044490);
		SetNeedMill(index, true);

		index = AddCraft(typeof(Dough), 1044495, 1024157, 0.0, 100.0, typeof(SackFlour), 1044468, 1, 1044253);
		AddRes(index, typeof(BaseBeverage), 1046458, 1, 1044253);

		index = AddCraft(typeof(SweetDough), 1044495, 1041340, 0.0, 100.0, typeof(Dough), 1044469, 1, 1044253);
		AddRes(index, typeof(JarHoney), 1044472, 1, 1044253);

		index = AddCraft(typeof(CakeMix), 1044495, 1041002, 0.0, 100.0, typeof(SackFlour), 1044468, 1, 1044253);
		AddRes(index, typeof(SweetDough), 1044475, 1, 1044253);

		index = AddCraft(typeof(CookieMix), 1044495, 1024159, 0.0, 100.0, typeof(JarHoney), 1044472, 1, 1044253);
		AddRes(index, typeof(SweetDough), 1044475, 1, 1044253);

		if (Core.ML)
		{
			index = AddCraft(typeof(CocoaButter), 1044495, 1079998, 0.0, 100.0, typeof(CocoaPulp), 1080530, 1, 1044253);
			SetItemHue(index, 0x457);
			SetNeededExpansion(index, Expansion.ML);
			SetNeedOven(index, true);

			index = AddCraft(typeof(CocoaLiquor), 1044495, 1079999, 0.0, 100.0, typeof(CocoaPulp), 1080530, 1, 1044253);
			AddRes(index, typeof(EmptyPewterBowl), 1025629, 1, 1044253);
			SetItemHue(index, 0x46A);
			SetNeededExpansion(index, Expansion.ML);
			SetNeedOven(index, true);
		}
		/* End Ingredients */

		/* Begin Preparations */
		index = AddCraft(typeof(UnbakedQuiche), 1044496, 1041339, 0.0, 100.0, typeof(Dough), 1044469, 1, 1044253);
		AddRes(index, typeof(Eggs), 1044477, 1, 1044253);

		// TODO: This must also support chicken and lamb legs
		index = AddCraft(typeof(UnbakedMeatPie), 1044496, 1041338, 0.0, 100.0, typeof(Dough), 1044469, 1, 1044253);
		AddRes(index, typeof(RawRibs), 1044482, 1, 1044253);

		index = AddCraft(typeof(UncookedSausagePizza), 1044496, 1041337, 0.0, 100.0, typeof(Dough), 1044469, 1, 1044253);
		AddRes(index, typeof(Sausage), 1044483, 1, 1044253);

		index = AddCraft(typeof(UncookedCheesePizza), 1044496, 1041341, 0.0, 100.0, typeof(Dough), 1044469, 1, 1044253);
		AddRes(index, typeof(CheeseWheel), 1044486, 1, 1044253);

		index = AddCraft(typeof(UnbakedFruitPie), 1044496, 1041334, 0.0, 100.0, typeof(Dough), 1044469, 1, 1044253);
		AddRes(index, typeof(Pear), 1044481, 1, 1044253);

		index = AddCraft(typeof(UnbakedPeachCobbler), 1044496, 1041335, 0.0, 100.0, typeof(Dough), 1044469, 1, 1044253);
		AddRes(index, typeof(Peach), 1044480, 1, 1044253);

		index = AddCraft(typeof(UnbakedApplePie), 1044496, 1041336, 0.0, 100.0, typeof(Dough), 1044469, 1, 1044253);
		AddRes(index, typeof(Apple), 1044479, 1, 1044253);

		index = AddCraft(typeof(UnbakedPumpkinPie), 1044496, 1041342, 0.0, 100.0, typeof(Dough), 1044469, 1, 1044253);
		AddRes(index, typeof(Pumpkin), 1044484, 1, 1044253);

		if (Core.SE)
		{
			index = AddCraft(typeof(GreenTea), 1044496, 1030315, 80.0, 130.0, typeof(GreenTeaBasket), 1030316, 1, 1044253);
			AddRes(index, typeof(BaseBeverage), 1046458, 1, 1044253);
			SetNeededExpansion(index, Expansion.SE);
			SetNeedOven(index, true);

			index = AddCraft(typeof(WasabiClumps), 1044496, 1029451, 70.0, 120.0, typeof(BaseBeverage), 1046458, 1, 1044253);
			AddRes(index, typeof(WoodenBowlOfPeas), 1025633, 3, 1044253);
			SetNeededExpansion(index, Expansion.SE);

			index = AddCraft(typeof(SushiRolls), 1044496, 1030303, 90.0, 120.0, typeof(BaseBeverage), 1046458, 1, 1044253);
			AddRes(index, typeof(RawFishSteak), 1044476, 10, 1044253);
			SetNeededExpansion(index, Expansion.SE);

			index = AddCraft(typeof(SushiPlatter), 1044496, 1030305, 90.0, 120.0, typeof(BaseBeverage), 1046458, 1, 1044253);
			AddRes(index, typeof(RawFishSteak), 1044476, 10, 1044253);
			SetNeededExpansion(index, Expansion.SE);
		}

		index = AddCraft(typeof(TribalPaint), 1044496, 1040000, Core.ML ? 55.0 : 80.0, Core.ML ? 105.0 : 80.0, typeof(SackFlour), 1044468, 1, 1044253);
		AddRes(index, typeof(TribalBerry), 1046460, 1, 1044253);

		if (Core.SE)
		{
			index = AddCraft(typeof(EggBomb), 1044496, 1030249, 90.0, 120.0, typeof(Eggs), 1044477, 1, 1044253);
			AddRes(index, typeof(SackFlour), 1044468, 3, 1044253);
			SetNeededExpansion(index, Expansion.SE);
		}
		/* End Preparations */

		/* Begin Baking */
		index = AddCraft(typeof(BreadLoaf), 1044497, 1024156, 0.0, 100.0, typeof(Dough), 1044469, 1, 1044253);
		SetNeedOven(index, true);

		index = AddCraft(typeof(Cookies), 1044497, 1025643, 0.0, 100.0, typeof(CookieMix), 1044474, 1, 1044253);
		SetNeedOven(index, true);

		index = AddCraft(typeof(Cake), 1044497, 1022537, 0.0, 100.0, typeof(CakeMix), 1044471, 1, 1044253);
		SetNeedOven(index, true);

		index = AddCraft(typeof(Muffins), 1044497, 1022539, 0.0, 100.0, typeof(SweetDough), 1044475, 1, 1044253);
		SetNeedOven(index, true);

		index = AddCraft(typeof(Quiche), 1044497, 1041345, 0.0, 100.0, typeof(UnbakedQuiche), 1044518, 1, 1044253);
		SetNeedOven(index, true);

		index = AddCraft(typeof(MeatPie), 1044497, 1041347, 0.0, 100.0, typeof(UnbakedMeatPie), 1044519, 1, 1044253);
		SetNeedOven(index, true);

		index = AddCraft(typeof(SausagePizza), 1044497, 1044517, 0.0, 100.0, typeof(UncookedSausagePizza), 1044520, 1, 1044253);
		SetNeedOven(index, true);

		index = AddCraft(typeof(CheesePizza), 1044497, 1044516, 0.0, 100.0, typeof(UncookedCheesePizza), 1044521, 1, 1044253);
		SetNeedOven(index, true);

		index = AddCraft(typeof(FruitPie), 1044497, 1041346, 0.0, 100.0, typeof(UnbakedFruitPie), 1044522, 1, 1044253);
		SetNeedOven(index, true);

		index = AddCraft(typeof(PeachCobbler), 1044497, 1041344, 0.0, 100.0, typeof(UnbakedPeachCobbler), 1044523, 1, 1044253);
		SetNeedOven(index, true);

		index = AddCraft(typeof(ApplePie), 1044497, 1041343, 0.0, 100.0, typeof(UnbakedApplePie), 1044524, 1, 1044253);
		SetNeedOven(index, true);

		index = AddCraft(typeof(PumpkinPie), 1044497, 1041348, 0.0, 100.0, typeof(UnbakedPumpkinPie), 1046461, 1, 1044253);
		SetNeedOven(index, true);

		if (Core.SE)
		{
			index = AddCraft(typeof(MisoSoup), 1044497, 1030317, 60.0, 110.0, typeof(RawFishSteak), 1044476, 1, 1044253);
			AddRes(index, typeof(BaseBeverage), 1046458, 1, 1044253);
			SetNeededExpansion(index, Expansion.SE);
			SetNeedOven(index, true);

			index = AddCraft(typeof(WhiteMisoSoup), 1044497, 1030318, 60.0, 110.0, typeof(RawFishSteak), 1044476, 1, 1044253);
			AddRes(index, typeof(BaseBeverage), 1046458, 1, 1044253);
			SetNeededExpansion(index, Expansion.SE);
			SetNeedOven(index, true);

			index = AddCraft(typeof(RedMisoSoup), 1044497, 1030319, 60.0, 110.0, typeof(RawFishSteak), 1044476, 1, 1044253);
			AddRes(index, typeof(BaseBeverage), 1046458, 1, 1044253);
			SetNeededExpansion(index, Expansion.SE);
			SetNeedOven(index, true);

			index = AddCraft(typeof(AwaseMisoSoup), 1044497, 1030320, 60.0, 110.0, typeof(RawFishSteak), 1044476, 1, 1044253);
			AddRes(index, typeof(BaseBeverage), 1046458, 1, 1044253);
			SetNeededExpansion(index, Expansion.SE);
			SetNeedOven(index, true);
		}
		/* End Baking */

		/* Begin Barbecue */
		index = AddCraft(typeof(CookedBird), 1044498, 1022487, 0.0, 100.0, typeof(RawBird), 1044470, 1, 1044253);
		SetNeedHeat(index, true);
		SetUseAllRes(index, true);

		index = AddCraft(typeof(ChickenLeg), 1044498, 1025640, 0.0, 100.0, typeof(RawChickenLeg), 1044473, 1, 1044253);
		SetNeedHeat(index, true);
		SetUseAllRes(index, true);

		index = AddCraft(typeof(FishSteak), 1044498, 1022427, 0.0, 100.0, typeof(RawFishSteak), 1044476, 1, 1044253);
		SetNeedHeat(index, true);
		SetUseAllRes(index, true);

		index = AddCraft(typeof(FriedEggs), 1044498, 1022486, 0.0, 100.0, typeof(Eggs), 1044477, 1, 1044253);
		SetNeedHeat(index, true);
		SetUseAllRes(index, true);

		index = AddCraft(typeof(LambLeg), 1044498, 1025642, 0.0, 100.0, typeof(RawLambLeg), 1044478, 1, 1044253);
		SetNeedHeat(index, true);
		SetUseAllRes(index, true);

		index = AddCraft(typeof(Ribs), 1044498, 1022546, 0.0, 100.0, typeof(RawRibs), 1044485, 1, 1044253);
		SetNeedHeat(index, true);
		SetUseAllRes(index, true);
		/* End Barbecue */

		/* Begin Chocolatiering */
		if (!Core.ML) return;
		index = AddCraft(typeof(DarkChocolate), 1080001, 1079994, 15.0, 100.0, typeof(SackOfSugar), 1079997, 1, 1044253);
		AddRes(index, typeof(CocoaButter), 1079998, 1, 1044253);
		AddRes(index, typeof(CocoaLiquor), 1079999, 1, 1044253);
		SetItemHue(index, 0x465);
		SetNeededExpansion(index, Expansion.ML);

		index = AddCraft(typeof(MilkChocolate), 1080001, 1079995, 32.5, 107.5, typeof(SackOfSugar), 1079997, 1, 1044253);
		AddRes(index, typeof(CocoaButter), 1079998, 1, 1044253);
		AddRes(index, typeof(CocoaLiquor), 1079999, 1, 1044253);
		AddRes(index, typeof(BaseBeverage), 1022544, 1, 1044253);
		SetBeverageType(index, BeverageType.Milk);
		SetItemHue(index, 0x461);
		SetNeededExpansion(index, Expansion.ML);

		index = AddCraft(typeof(WhiteChocolate), 1080001, 1079996, 52.5, 127.5, typeof(SackOfSugar), 1079997, 1, 1044253);
		AddRes(index, typeof(CocoaButter), 1079998, 1, 1044253);
		AddRes(index, typeof(Vanilla), 1080000, 1, 1044253);
		AddRes(index, typeof(BaseBeverage), 1022544, 1, 1044253);
		SetBeverageType(index, BeverageType.Milk);
		SetItemHue(index, 0x47E);
		SetNeededExpansion(index, Expansion.ML);
		/* End Chocolatiering */
	}
}
