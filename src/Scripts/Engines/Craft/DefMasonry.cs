using Server.Items;
using Server.Mobiles;
using System;

namespace Server.Engines.Craft;

public class DefMasonry : CraftSystem
{
	public override SkillName MainSkill => SkillName.Carpentry;

	public override int GumpTitleNumber => 1044500;

	private static CraftSystem _mCraftSystem;

	public static CraftSystem CraftSystem => _mCraftSystem ??= new DefMasonry();

	public override double GetChanceAtMin(CraftItem item)
	{
		return 0.0; // 0%
	}

	public DefMasonry() : base(1, 1, 1.25)// base( 1, 2, 1.7 )
	{
		_mCraftSystem = this;
	}

	public override bool RetainsColorFrom(CraftItem item, Type type)
	{
		return true;
	}

	public override int CanCraft(Mobile from, ITool tool, Type itemType)
	{
		int num = 0;

		if (tool == null || tool.Deleted || tool.UsesRemaining <= 0)
		{
			return 1044038; // You have worn out your tool!
		}

		if (tool is Item item && !BaseTool.CheckTool(item, from))
		{
			return 1048146; // If you have a tool equipped, you must use that tool.
		}

		if (!(from is PlayerMobile mobile && mobile.Masonry && mobile.Skills[SkillName.Carpentry].Base >= 100.0))
		{
			return 1044633; // You haven't learned stonecraft.
		}

		return !tool.CheckAccessible(from, ref num) ? num : 0;
	}

	public override void PlayCraftEffect(Mobile from)
	{
		// no effects
		if ( from.Body.Type == BodyType.Human && !from.Mounted )
			from.Animate( 9, 5, 1, true, false, 0 );
		new InternalTimer( from ).Start();
	}

	// Delay to synchronize the sound with the hit on the anvil
	private class InternalTimer : Timer
	{
		private readonly Mobile _mFrom;

		public InternalTimer(Mobile from) : base(TimeSpan.FromSeconds(0.7))
		{
			_mFrom = from;
		}

		protected override void OnTick()
		{
			_mFrom.PlaySound(0x23D);
		}
	}

	public override int PlayEndingEffect(Mobile from, bool failed, bool lostMaterial, bool toolBroken, int quality, bool makersMark, CraftItem item)
	{
		if (toolBroken)
		{
			from.SendLocalizedMessage(1044038); // You have worn out your tool 
		}

		if (failed)
		{
			return lostMaterial ? 1044043 : 1044157; // You failed to create the item, and some of your materials are lost.  // You failed to create the item, but no materials were lost.
		}

		if (quality == 0)
		{
			return 502785; // You were barely able to make this item.  It's quality is below average. 
		}

		if (makersMark && quality == 2)
		{
			return 1044156; // You create an exceptional quality item and affix your maker's mark. 
		}
		// You create an exceptional quality item. // You create the item. 
		return quality == 2 ? 1044155 : 1044154;
	}

	public override void InitCraftList()
	{
		// Decorations
		AddCraft(typeof(Vase), 1044501, 1022888, 52.5, 102.5, typeof(Granite), 1044514, 1, 1044513);
		AddCraft(typeof(LargeVase), 1044501, 1022887, 52.5, 102.5, typeof(Granite), 1044514, 3, 1044513);

		if (Core.SE)
		{
			int index = AddCraft(typeof(SmallUrn), 1044501, 1029244, 82.0, 132.0, typeof(Granite), 1044514, 3, 1044513);
			SetNeededExpansion(index, Expansion.SE);

			index = AddCraft(typeof(SmallTowerSculpture), 1044501, 1029242, 82.0, 132.0, typeof(Granite), 1044514, 3, 1044513);
			SetNeededExpansion(index, Expansion.SE);
		}

		if (Core.ML)
		{
			int index = AddCraft(typeof(StoneAnvilSouthDeed), 1044290, 1072876, 78.0, 128.0, typeof(Granite), 1044514, 20, 1044513);
			AddRecipe(index, (int)CarpRecipes.StoneAnvilSouth);
			SetNeededExpansion(index, Expansion.ML);

			index = AddCraft(typeof(StoneAnvilEastDeed), 1044290, 1073392, 78.0, 128.0, typeof(Granite), 1044514, 20, 1044513);
			AddRecipe(index, (int)CarpRecipes.StoneAnvilEast);
			SetNeededExpansion(index, Expansion.ML);
		}

		// Furniture
		AddCraft(typeof(StoneChair), 1044502, 1024635, 55.0, 105.0, typeof(Granite), 1044514, 4, 1044513);
		AddCraft(typeof(MediumStoneTableEastDeed), 1044502, 1044508, 65.0, 115.0, typeof(Granite), 1044514, 6, 1044513);
		AddCraft(typeof(MediumStoneTableSouthDeed), 1044502, 1044509, 65.0, 115.0, typeof(Granite), 1044514, 6, 1044513);
		AddCraft(typeof(LargeStoneTableEastDeed), 1044502, 1044511, 75.0, 125.0, typeof(Granite), 1044514, 9, 1044513);
		AddCraft(typeof(LargeStoneTableSouthDeed), 1044502, 1044512, 75.0, 125.0, typeof(Granite), 1044514, 9, 1044513);

		// Statues
		AddCraft(typeof(StatueSouth), 1044503, 1044505, 60.0, 120.0, typeof(Granite), 1044514, 3, 1044513);
		AddCraft(typeof(StatueNorth), 1044503, 1044506, 60.0, 120.0, typeof(Granite), 1044514, 3, 1044513);
		AddCraft(typeof(StatueEast), 1044503, 1044507, 60.0, 120.0, typeof(Granite), 1044514, 3, 1044513);
		AddCraft(typeof(StatuePegasus), 1044503, 1044510, 70.0, 130.0, typeof(Granite), 1044514, 4, 1044513);

		MarkOption = true;
		Repair = Core.SA;
		CanEnhance = Core.SA;

		SetSubRes(typeof(Granite), 1044525);

		AddSubRes(typeof(Granite), 1044525, 00.0, 1044514, 1044526);
		AddSubRes(typeof(DullCopperGranite), 1044023, 65.0, 1044514, 1044527);
		AddSubRes(typeof(ShadowIronGranite), 1044024, 70.0, 1044514, 1044527);
		AddSubRes(typeof(CopperGranite), 1044025, 75.0, 1044514, 1044527);
		AddSubRes(typeof(BronzeGranite), 1044026, 80.0, 1044514, 1044527);
		AddSubRes(typeof(GoldGranite), 1044027, 85.0, 1044514, 1044527);
		AddSubRes(typeof(AgapiteGranite), 1044028, 90.0, 1044514, 1044527);
		AddSubRes(typeof(VeriteGranite), 1044029, 95.0, 1044514, 1044527);
		AddSubRes(typeof(ValoriteGranite), 1044030, 99.0, 1044514, 1044527);
	}
}
