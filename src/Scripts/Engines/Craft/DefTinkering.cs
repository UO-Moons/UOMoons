using Server.Factions;
using Server.Items;
using Server.Targeting;
using System;

namespace Server.Engines.Craft;

public class DefTinkering : CraftSystem
{
	public override CraftEca Eca => CraftEca.ChanceMinusSixtyToFourtyFive;

	public override SkillName MainSkill => SkillName.Tinkering;

	public override int GumpTitleNumber => 1044007;

	private static CraftSystem _mCraftSystem;

	public static CraftSystem CraftSystem => _mCraftSystem ??= new DefTinkering();

	public DefTinkering() : base(1, 1, 1.25)// base( 1, 1, 3.0 )
	{
		_mCraftSystem = this;
	}

	public override double GetChanceAtMin(CraftItem item)
	{
		if (item.NameNumber == 1044258 || item.NameNumber == 1046445) // potion keg and faction trap removal kit
			return 0.5; // 50%

		return 0.0; // 0%
	}

	public override int CanCraft(Mobile from, ITool tool, Type itemType)
	{
		int num = 0;

		if (tool == null || tool.Deleted || tool.UsesRemaining <= 0)
		{
			return 1044038; // You have worn out your tool!
		}

		if (!tool.CheckAccessible(from, ref num))
		{
			return num; // The tool must be on your person to use.
		}

		if (itemType != null && (itemType.IsSubclassOf(typeof(BaseFactionTrapDeed)) || itemType == typeof(FactionTrapRemovalKit)) && Faction.Find(from) == null)
		{
			return 1044573; // You have to be in a faction to do that.
		}

		return 0;
	}

	private static readonly Type[] MTinkerColorables = {
		typeof(ForkLeft), typeof(ForkRight),
		typeof(SpoonLeft), typeof(SpoonRight),
		typeof(KnifeLeft), typeof(KnifeRight),
		typeof(Plate),
		typeof(Goblet), typeof(PewterMug),
		typeof(KeyRing),
		typeof(Candelabra), typeof(Scales),
		typeof(Key), typeof(Globe),
		typeof(Spyglass), typeof(Lantern),
		typeof(HeatingStand), typeof(BroadcastCrystal), typeof(RedScales),
		typeof(BlueScales), typeof(BlackScales), typeof(GreenScales), typeof(YellowScales), typeof(WhiteScales),
		typeof(PlantPigment), typeof(SoftenedReeds), typeof(DryReeds), typeof(PlantClippings)
	};

	public override bool RetainsColorFrom(CraftItem item, Type type)
	{
		if (!type.IsSubclassOf(typeof(BaseIngot)))
			return false;

		type = item.ItemType;

		bool contains = false;

		for (var i = 0; !contains && i < MTinkerColorables.Length; ++i)
			contains = (MTinkerColorables[i] == type);

		return contains;
	}

	public override void PlayCraftEffect(Mobile from)
	{
		// no sound
		//from.PlaySound( 0x241 );
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
		// You create an exceptional quality item.// You create the item.
		return quality == 2 ? 1044155 : 1044154;
	}


	public override bool ConsumeOnFailure(Mobile from, Type resourceType, CraftItem craftItem)
	{
		return resourceType != typeof(Silver) && base.ConsumeOnFailure(from, resourceType, craftItem);
	}

	public void AddJewelrySet(GemType gemType, Type itemType)
	{
		var offset = (int)gemType - 1;

		var index = AddCraft(typeof(GoldRing), 1044049, 1044176 + offset, 40.0, 90.0, typeof(IronIngot), 1044036, 2, 1044037);
		AddRes(index, itemType, 1044231 + offset, 1, 1044240);

		index = AddCraft(typeof(SilverBeadNecklace), 1044049, 1044185 + offset, 40.0, 90.0, typeof(IronIngot), 1044036, 2, 1044037);
		AddRes(index, itemType, 1044231 + offset, 1, 1044240);

		index = AddCraft(typeof(GoldNecklace), 1044049, 1044194 + offset, 40.0, 90.0, typeof(IronIngot), 1044036, 2, 1044037);
		AddRes(index, itemType, 1044231 + offset, 1, 1044240);

		index = AddCraft(typeof(GoldEarrings), 1044049, 1044203 + offset, 40.0, 90.0, typeof(IronIngot), 1044036, 2, 1044037);
		AddRes(index, itemType, 1044231 + offset, 1, 1044240);

		index = AddCraft(typeof(GoldBeadNecklace), 1044049, 1044212 + offset, 40.0, 90.0, typeof(IronIngot), 1044036, 2, 1044037);
		AddRes(index, itemType, 1044231 + offset, 1, 1044240);

		index = AddCraft(typeof(GoldBracelet), 1044049, 1044221 + offset, 40.0, 90.0, typeof(IronIngot), 1044036, 2, 1044037);
		AddRes(index, itemType, 1044231 + offset, 1, 1044240);
	}

	public override void InitCraftList()
	{

		#region Wooden Items
		AddCraft(typeof(JointingPlane), 1044042, 1024144, 0.0, 50.0, typeof(Log), 1044041, 4, 1044351);
		AddCraft(typeof(MouldingPlane), 1044042, 1024140, 0.0, 50.0, typeof(Log), 1044041, 4, 1044351);
		AddCraft(typeof(SmoothingPlane), 1044042, 1024146, 0.0, 50.0, typeof(Log), 1044041, 4, 1044351);
		AddCraft(typeof(ClockFrame), 1044042, 1024173, 0.0, 50.0, typeof(Log), 1044041, 6, 1044351);
		AddCraft(typeof(Axle), 1044042, 1024187, -25.0, 25.0, typeof(Log), 1044041, 2, 1044351);
		AddCraft(typeof(RollingPin), 1044042, 1024163, 0.0, 50.0, typeof(Log), 1044041, 5, 1044351);

		int index;
		if (Core.SE)
		{
			index = AddCraft(typeof(Nunchaku), 1044042, 1030158, 70.0, 120.0, typeof(IronIngot), 1044036, 3, 1044037);
			AddRes(index, typeof(Log), 1044041, 8, 1044351);
			SetNeededExpansion(index, Expansion.SE);
		}
		#endregion

		#region Tools
		AddCraft(typeof(Scissors), 1044046, 1023998, 5.0, 55.0, typeof(IronIngot), 1044036, 2, 1044037);
		AddCraft(typeof(MortarPestle), 1044046, 1023739, 20.0, 70.0, typeof(IronIngot), 1044036, 3, 1044037);
		AddCraft(typeof(Scorp), 1044046, 1024327, 30.0, 80.0, typeof(IronIngot), 1044036, 2, 1044037);
		AddCraft(typeof(TinkerTools), 1044046, 1044164, 10.0, 60.0, typeof(IronIngot), 1044036, 2, 1044037);
		AddCraft(typeof(Hatchet), 1044046, 1023907, 30.0, 80.0, typeof(IronIngot), 1044036, 4, 1044037);
		AddCraft(typeof(DrawKnife), 1044046, 1024324, 30.0, 80.0, typeof(IronIngot), 1044036, 2, 1044037);
		AddCraft(typeof(SewingKit), 1044046, 1023997, 10.0, 70.0, typeof(IronIngot), 1044036, 2, 1044037);
		AddCraft(typeof(Saw), 1044046, 1024148, 30.0, 80.0, typeof(IronIngot), 1044036, 4, 1044037);
		AddCraft(typeof(DovetailSaw), 1044046, 1024136, 30.0, 80.0, typeof(IronIngot), 1044036, 4, 1044037);
		AddCraft(typeof(Froe), 1044046, 1024325, 30.0, 80.0, typeof(IronIngot), 1044036, 2, 1044037);
		AddCraft(typeof(Shovel), 1044046, 1023898, 40.0, 90.0, typeof(IronIngot), 1044036, 4, 1044037);
		AddCraft(typeof(Hammer), 1044046, 1024138, 30.0, 80.0, typeof(IronIngot), 1044036, 1, 1044037);
		AddCraft(typeof(Tongs), 1044046, 1024028, 35.0, 85.0, typeof(IronIngot), 1044036, 1, 1044037);
		AddCraft(typeof(SmithHammer), 1044046, 1025091, 40.0, 90.0, typeof(IronIngot), 1044036, 4, 1044037);
		AddCraft(typeof(SledgeHammer), 1044046, 1024021, 40.0, 90.0, typeof(IronIngot), 1044036, 4, 1044037);
		AddCraft(typeof(Inshave), 1044046, 1024326, 30.0, 80.0, typeof(IronIngot), 1044036, 2, 1044037);
		AddCraft(typeof(Pickaxe), 1044046, 1023718, 40.0, 90.0, typeof(IronIngot), 1044036, 4, 1044037);
		AddCraft(typeof(Lockpick), 1044046, 1025371, 45.0, 95.0, typeof(IronIngot), 1044036, 1, 1044037);
		AddCraft(typeof(Skillet), 1044046, 1044567, 30.0, 80.0, typeof(IronIngot), 1044036, 4, 1044037);
		AddCraft(typeof(FlourSifter), 1044046, 1024158, 50.0, 100.0, typeof(IronIngot), 1044036, 3, 1044037);
		AddCraft(typeof(FletcherTools), 1044046, 1044166, 35.0, 85.0, typeof(IronIngot), 1044036, 3, 1044037);
		AddCraft(typeof(MapmakersPen), 1044046, 1044167, 25.0, 75.0, typeof(IronIngot), 1044036, 1, 1044037);
		AddCraft(typeof(ScribesPen), 1044046, 1044168, 25.0, 75.0, typeof(IronIngot), 1044036, 1, 1044037);

		if (Core.ML)
		{
			index = AddCraft(typeof(MetalContainerEngraver), 1044046, 1072154, 75.0, 100.0, typeof(IronIngot), 1044036, 4, 1044037);
			AddRes(index, typeof(Springs), 1044171, 1, 1044253);
			AddRes(index, typeof(Gears), 1044254, 2, 1044253);
			AddRes(index, typeof(Diamond), 1062608, 1, 1044240);
			SetNeededExpansion(index, Expansion.ML);
		}

		AddCraft(typeof(Pitchfork), 1044046, 1023719, 40.0, 90.0, typeof(IronIngot), 1044036, 4, 1044037);
		#endregion

		#region Parts
		AddCraft(typeof(Gears), 1044047, 1024179, 5.0, 55.0, typeof(IronIngot), 1044036, 2, 1044037);
		AddCraft(typeof(ClockParts), 1044047, 1024175, 25.0, 75.0, typeof(IronIngot), 1044036, 1, 1044037);
		AddCraft(typeof(BarrelTap), 1044047, 1024100, 35.0, 85.0, typeof(IronIngot), 1044036, 2, 1044037);
		AddCraft(typeof(Springs), 1044047, 1024189, 5.0, 55.0, typeof(IronIngot), 1044036, 2, 1044037);
		AddCraft(typeof(SextantParts), 1044047, 1024185, 30.0, 80.0, typeof(IronIngot), 1044036, 4, 1044037);
		AddCraft(typeof(BarrelHoops), 1044047, 1024321, -15.0, 35.0, typeof(IronIngot), 1044036, 5, 1044037);
		AddCraft(typeof(Hinge), 1044047, 1024181, 5.0, 55.0, typeof(IronIngot), 1044036, 2, 1044037);
		AddCraft(typeof(BolaBall), 1044047, 1023699, 45.0, 95.0, typeof(IronIngot), 1044036, 10, 1044037);

		if (Core.ML)
		{
			index = AddCraft(typeof(JeweledFiligree), 1044047, 1072894, 70.0, 110.0, typeof(IronIngot), 1044036, 2, 1044037);
			AddRes(index, typeof(StarSapphire), 1044231, 1, 1044253);
			AddRes(index, typeof(Ruby), 1044234, 1, 1044253);
			SetNeededExpansion(index, Expansion.ML);
		}
		#endregion

		#region Utensils
		AddCraft(typeof(ButcherKnife), 1044048, 1025110, 25.0, 75.0, typeof(IronIngot), 1044036, 2, 1044037);
		AddCraft(typeof(SpoonLeft), 1044048, 1044158, 0.0, 50.0, typeof(IronIngot), 1044036, 1, 1044037);
		AddCraft(typeof(SpoonRight), 1044048, 1044159, 0.0, 50.0, typeof(IronIngot), 1044036, 1, 1044037);
		AddCraft(typeof(Plate), 1044048, 1022519, 0.0, 50.0, typeof(IronIngot), 1044036, 2, 1044037);
		AddCraft(typeof(ForkLeft), 1044048, 1044160, 0.0, 50.0, typeof(IronIngot), 1044036, 1, 1044037);
		AddCraft(typeof(ForkRight), 1044048, 1044161, 0.0, 50.0, typeof(IronIngot), 1044036, 1, 1044037);
		AddCraft(typeof(Cleaver), 1044048, 1023778, 20.0, 70.0, typeof(IronIngot), 1044036, 3, 1044037);
		AddCraft(typeof(KnifeLeft), 1044048, 1044162, 0.0, 50.0, typeof(IronIngot), 1044036, 1, 1044037);
		AddCraft(typeof(KnifeRight), 1044048, 1044163, 0.0, 50.0, typeof(IronIngot), 1044036, 1, 1044037);
		AddCraft(typeof(Goblet), 1044048, 1022458, 10.0, 60.0, typeof(IronIngot), 1044036, 2, 1044037);
		AddCraft(typeof(PewterMug), 1044048, 1024097, 10.0, 60.0, typeof(IronIngot), 1044036, 2, 1044037);
		AddCraft(typeof(SkinningKnife), 1044048, 1023781, 25.0, 75.0, typeof(IronIngot), 1044036, 2, 1044037);
		#endregion

		#region Misc
		AddCraft(typeof(KeyRing), 1044050, 1024113, 10.0, 60.0, typeof(IronIngot), 1044036, 2, 1044037);
		AddCraft(typeof(Candelabra), 1044050, 1022599, 55.0, 105.0, typeof(IronIngot), 1044036, 4, 1044037);
		AddCraft(typeof(Scales), 1044050, 1026225, 60.0, 110.0, typeof(IronIngot), 1044036, 4, 1044037);
		AddCraft(typeof(Key), 1044050, 1024112, 20.0, 70.0, typeof(IronIngot), 1044036, 3, 1044037);
		AddCraft(typeof(Globe), 1044050, 1024167, 55.0, 105.0, typeof(IronIngot), 1044036, 4, 1044037);
		AddCraft(typeof(Spyglass), 1044050, 1025365, 60.0, 110.0, typeof(IronIngot), 1044036, 4, 1044037);
		AddCraft(typeof(Lantern), 1044050, 1022597, 30.0, 80.0, typeof(IronIngot), 1044036, 2, 1044037);
		AddCraft(typeof(HeatingStand), 1044050, 1026217, 60.0, 110.0, typeof(IronIngot), 1044036, 4, 1044037);

		if (Core.SE)
		{
			index = AddCraft(typeof(ShojiLantern), 1044050, 1029404, 65.0, 115.0, typeof(IronIngot), 1044036, 10, 1044037);
			AddRes(index, typeof(Log), 1044041, 5, 1044351);
			SetNeededExpansion(index, Expansion.SE);

			index = AddCraft(typeof(PaperLantern), 1044050, 1029406, 65.0, 115.0, typeof(IronIngot), 1044036, 10, 1044037);
			AddRes(index, typeof(Log), 1044041, 5, 1044351);
			SetNeededExpansion(index, Expansion.SE);

			index = AddCraft(typeof(RoundPaperLantern), 1044050, 1029418, 65.0, 115.0, typeof(IronIngot), 1044036, 10, 1044037);
			AddRes(index, typeof(Log), 1044041, 5, 1044351);
			SetNeededExpansion(index, Expansion.SE);

			index = AddCraft(typeof(WindChimes), 1044050, 1030290, 80.0, 130.0, typeof(IronIngot), 1044036, 15, 1044037);
			SetNeededExpansion(index, Expansion.SE);

			index = AddCraft(typeof(FancyWindChimes), 1044050, 1030291, 80.0, 130.0, typeof(IronIngot), 1044036, 15, 1044037);
			SetNeededExpansion(index, Expansion.SE);
		}
		#endregion

		#region Assembly
		if (Core.ML)
		{
			index = AddCraft(typeof(HitchingRope), 1044051, 1071124, 60.0, 120.0, typeof(Rope), 1020934, 1, 1044253);
			AddSkill(index, SkillName.AnimalLore, 15.0, 100.0);
			AddRes(index, typeof(ResolvesBridle), 1074761, 1, 1044253);
			SetNeededExpansion(index, Expansion.ML);

			index = AddCraft(typeof(HitchingPost), 1044051, 1071127, 90.0, 160.0, typeof(IronIngot), 1044036, 50, 1044253);
			AddRes(index, typeof(AnimalPheromone), 1071200, 1, 1044253);
			AddRes(index, typeof(HitchingRope), 1071124, 2, 1044253);
			AddRes(index, typeof(PhillipsWoodenSteed), 1063488, 1, 1044253);
			SetNeededExpansion(index, Expansion.ML);
		}
		#endregion

		#region Jewelry

		AddCraft(typeof(GoldRing), 1044049, 1024234, 65.0, 115.0, typeof(IronIngot), 1044036, 3, 1044037);
		AddCraft(typeof(GoldBracelet), 1044049, 1024230, 55.0, 105.0, typeof(IronIngot), 1044036, 3, 1044037);

		AddJewelrySet(GemType.StarSapphire, typeof(StarSapphire));
		AddJewelrySet(GemType.Emerald, typeof(Emerald));
		AddJewelrySet(GemType.Sapphire, typeof(Sapphire));
		AddJewelrySet(GemType.Ruby, typeof(Ruby));
		AddJewelrySet(GemType.Citrine, typeof(Citrine));
		AddJewelrySet(GemType.Amethyst, typeof(Amethyst));
		AddJewelrySet(GemType.Tourmaline, typeof(Tourmaline));
		AddJewelrySet(GemType.Amber, typeof(Amber));
		AddJewelrySet(GemType.Diamond, typeof(Diamond));
		#endregion

		#region Multi-Component Items
		index = AddCraft(typeof(AxleGears), 1044051, 1024177, 0.0, 0.0, typeof(Axle), 1044169, 1, 1044253);
		AddRes(index, typeof(Gears), 1044254, 1, 1044253);

		index = AddCraft(typeof(ClockParts), 1044051, 1024175, 0.0, 0.0, typeof(AxleGears), 1044170, 1, 1044253);
		AddRes(index, typeof(Springs), 1044171, 1, 1044253);

		index = AddCraft(typeof(SextantParts), 1044051, 1024185, 0.0, 0.0, typeof(AxleGears), 1044170, 1, 1044253);
		AddRes(index, typeof(Hinge), 1044172, 1, 1044253);

		index = AddCraft(typeof(ClockRight), 1044051, 1044257, 0.0, 0.0, typeof(ClockFrame), 1044174, 1, 1044253);
		AddRes(index, typeof(ClockParts), 1044173, 1, 1044253);

		index = AddCraft(typeof(ClockLeft), 1044051, 1044256, 0.0, 0.0, typeof(ClockFrame), 1044174, 1, 1044253);
		AddRes(index, typeof(ClockParts), 1044173, 1, 1044253);

		AddCraft(typeof(Sextant), 1044051, 1024183, 0.0, 0.0, typeof(SextantParts), 1044175, 1, 1044253);

		index = AddCraft(typeof(Bola), 1044051, 1046441, 60.0, 80.0, typeof(BolaBall), 1046440, 4, 1042613);
		AddRes(index, typeof(Leather), 1044462, 3, 1044463);

		index = AddCraft(typeof(PotionKeg), 1044051, 1044258, 75.0, 100.0, typeof(Keg), 1044255, 1, 1044253);
		AddRes(index, typeof(EmptyBottle), 1044250, 10, 1044253);
		AddRes(index, typeof(BarrelLid), 1044251, 1, 1044253);
		AddRes(index, typeof(BarrelTap), 1044252, 1, 1044253);

		#endregion

		#region Traps
		// Dart Trap
		index = AddCraft(typeof(DartTrapCraft), 1044052, 1024396, 30.0, 80.0, typeof(IronIngot), 1044036, 1, 1044037);
		AddRes(index, typeof(CrossBowBolt), 1044570, 1, 1044253);

		// Poison Trap
		index = AddCraft(typeof(PoisonTrapCraft), 1044052, 1044593, 30.0, 80.0, typeof(IronIngot), 1044036, 1, 1044037);
		AddRes(index, typeof(BasePoisonPotion), 1044571, 1, 1044253);

		// Explosion Trap
		index = AddCraft(typeof(ExplosionTrapCraft), 1044052, 1044597, 55.0, 105.0, typeof(IronIngot), 1044036, 1, 1044037);
		AddRes(index, typeof(BaseExplosionPotion), 1044569, 1, 1044253);

		// Faction Gas Trap
		index = AddCraft(typeof(FactionGasTrapDeed), 1044052, 1044598, 65.0, 115.0, typeof(Silver), 1044572, Core.AOS ? 250 : 1000, 1044253);
		AddRes(index, typeof(IronIngot), 1044036, 10, 1044037);
		AddRes(index, typeof(BasePoisonPotion), 1044571, 1, 1044253);

		// Faction explosion Trap
		index = AddCraft(typeof(FactionExplosionTrapDeed), 1044052, 1044599, 65.0, 115.0, typeof(Silver), 1044572, Core.AOS ? 250 : 1000, 1044253);
		AddRes(index, typeof(IronIngot), 1044036, 10, 1044037);
		AddRes(index, typeof(BaseExplosionPotion), 1044569, 1, 1044253);

		// Faction Saw Trap
		index = AddCraft(typeof(FactionSawTrapDeed), 1044052, 1044600, 65.0, 115.0, typeof(Silver), 1044572, Core.AOS ? 250 : 1000, 1044253);
		AddRes(index, typeof(IronIngot), 1044036, 10, 1044037);
		AddRes(index, typeof(Gears), 1044254, 1, 1044253);

		// Faction Spike Trap
		index = AddCraft(typeof(FactionSpikeTrapDeed), 1044052, 1044601, 65.0, 115.0, typeof(Silver), 1044572, Core.AOS ? 250 : 1000, 1044253);
		AddRes(index, typeof(IronIngot), 1044036, 10, 1044037);
		AddRes(index, typeof(Springs), 1044171, 1, 1044253);

		// Faction trap removal kit
		index = AddCraft(typeof(FactionTrapRemovalKit), 1044052, 1046445, 90.0, 115.0, typeof(Silver), 1044572, 500, 1044253);
		AddRes(index, typeof(IronIngot), 1044036, 10, 1044037);
		#endregion

		#region Magic Jewlery
		if (Core.ML)
		{
			index = AddCraft(typeof(BrilliantAmberBracelet), 1073107, 1073453, 75.0, 125.0, typeof(IronIngot), 1044036, 5, 1044037);
			AddRes(index, typeof(Amber), 1062607, 20, 1044240);
			AddRes(index, typeof(BrilliantAmber), 1032697, 10, 1044240);
			SetNeededExpansion(index, Expansion.ML);

			index = AddCraft(typeof(FireRubyBracelet), 1073107, 1073454, 75.0, 125.0, typeof(IronIngot), 1044036, 5, 1044037);
			AddRes(index, typeof(Ruby), 1062603, 20, 1044240);
			AddRes(index, typeof(FireRuby), 1032695, 10, 1044240);
			SetNeededExpansion(index, Expansion.ML);

			index = AddCraft(typeof(DarkSapphireBracelet), 1073107, 1073455, 75.0, 125.0, typeof(IronIngot), 1044036, 5, 1044037);
			AddRes(index, typeof(Sapphire), 1062602, 20, 1044240);
			AddRes(index, typeof(DarkSapphire), 1032690, 10, 1044240);
			SetNeededExpansion(index, Expansion.ML);

			index = AddCraft(typeof(WhitePearlBracelet), 1073107, 1073456, 75.0, 125.0, typeof(IronIngot), 1044036, 5, 1044037);
			AddRes(index, typeof(Tourmaline), 1062606, 20, 1044240);
			AddRes(index, typeof(WhitePearl), 1032694, 10, 1044240);
			SetNeededExpansion(index, Expansion.ML);

			index = AddCraft(typeof(EcruCitrineRing), 1073107, 1073457, 75.0, 125.0, typeof(IronIngot), 1044036, 5, 1044037);
			AddRes(index, typeof(Citrine), 1062604, 20, 1044240);
			AddRes(index, typeof(EcruCitrine), 1032693, 10, 1044240);
			SetNeededExpansion(index, Expansion.ML);

			index = AddCraft(typeof(BlueDiamondRing), 1073107, 1073458, 75.0, 125.0, typeof(IronIngot), 1044036, 5, 1044037);
			AddRes(index, typeof(Diamond), 1062608, 20, 1044240);
			AddRes(index, typeof(BlueDiamond), 1032696, 10, 1044240);
			SetNeededExpansion(index, Expansion.ML);

			index = AddCraft(typeof(PerfectEmeraldRing), 1073107, 1073459, 75.0, 125.0, typeof(IronIngot), 1044036, 5, 1044037);
			AddRes(index, typeof(Emerald), 1062601, 20, 1044240);
			AddRes(index, typeof(PerfectEmerald), 1032692, 10, 1044240);
			SetNeededExpansion(index, Expansion.ML);

			index = AddCraft(typeof(TurqouiseRing), 1073107, 1073460, 75.0, 125.0, typeof(IronIngot), 1044036, 5, 1044037);
			AddRes(index, typeof(Amethyst), 1062605, 20, 1044240);
			AddRes(index, typeof(Turquoise), 1032691, 10, 1044240);
			SetNeededExpansion(index, Expansion.ML);

			index = AddCraft(typeof(ResilientBracer), 1073107, 1072933, 100.0, 125.0, typeof(IronIngot), 1044036, 2, 1044037);
			SetMinSkillOffset(index, 25.0);
			AddRes(index, typeof(CapturedEssence), 1032686, 1, 1044253);
			AddRes(index, typeof(BlueDiamond), 1032696, 10, 1044253);
			AddRes(index, typeof(Diamond), 1062608, 50, 1044253);
			AddRecipe(index, (int)TinkerRecipes.ResilientBracer);
			ForceNonExceptional(index);
			SetNeededExpansion(index, Expansion.ML);

			index = AddCraft(typeof(EssenceOfBattle), 1073107, 1072935, 100.0, 125.0, typeof(IronIngot), 1044036, 2, 1044037);
			SetMinSkillOffset(index, 25.0);
			AddRes(index, typeof(CapturedEssence), 1032686, 1, 1044253);
			AddRes(index, typeof(FireRuby), 1032695, 10, 1044253);
			AddRes(index, typeof(Ruby), 1062603, 50, 1044253);
			AddRecipe(index, (int)TinkerRecipes.EssenceOfBattle);
			ForceNonExceptional(index);
			SetNeededExpansion(index, Expansion.ML);


			index = AddCraft(typeof(PendantOfTheMagi), 1073107, 1072937, 100.0, 125.0, typeof(IronIngot), 1044036, 2, 1044037);
			SetMinSkillOffset(index, 25.0);
			AddRes(index, typeof(EyeOfTheTravesty), 1032685, 1, 1044253);
			AddRes(index, typeof(WhitePearl), 1032694, 5, 1044253);
			AddRes(index, typeof(StarSapphire), 1062600, 50, 1044253);
			AddRecipe(index, (int)TinkerRecipes.PendantOfTheMagi);
			ForceNonExceptional(index);
			SetNeededExpansion(index, Expansion.ML);
		}
		#endregion

		// Set the overridable material
		SetSubRes(typeof(IronIngot), 1044022);

		// Add every material you want the player to be able to choose from
		// This will override the overridable material
		AddSubRes(typeof(IronIngot), 1044022, 00.0, 1044036, 1044267);
		AddSubRes(typeof(DullCopperIngot), 1044023, 65.0, 1044036, 1044268);
		AddSubRes(typeof(ShadowIronIngot), 1044024, 70.0, 1044036, 1044268);
		AddSubRes(typeof(CopperIngot), 1044025, 75.0, 1044036, 1044268);
		AddSubRes(typeof(BronzeIngot), 1044026, 80.0, 1044036, 1044268);
		AddSubRes(typeof(GoldIngot), 1044027, 85.0, 1044036, 1044268);
		AddSubRes(typeof(AgapiteIngot), 1044028, 90.0, 1044036, 1044268);
		AddSubRes(typeof(VeriteIngot), 1044029, 95.0, 1044036, 1044268);
		AddSubRes(typeof(ValoriteIngot), 1044030, 99.0, 1044036, 1044268);

		MarkOption = true;
		Repair = true;
		CanEnhance = Core.AOS;
	}
}

public abstract class TrapCraft : CustomCraft
{
	public LockableContainer Container { get; private set; }

	public abstract TrapType TrapType { get; }

	protected TrapCraft(Mobile from, CraftItem craftItem, CraftSystem craftSystem, Type typeRes, ITool tool, int quality)
		: base(from, craftItem, craftSystem, typeRes, tool, quality)
	{
	}

	private int Verify(LockableContainer container)
	{
		if (container == null || container.KeyValue == 0)
		{
			return 1005638; // You can only trap lockable chests.
		}

		if (From.Map != container.Map || !From.InRange(container.GetWorldLocation(), 2))
		{
			return 500446; // That is too far away.
		}

		if (!container.Movable)
		{
			return 502944; // You cannot trap this item because it is locked down.
		}

		if (!container.IsAccessibleTo(From))
		{
			return 502946; // That belongs to someone else.
		}

		if (container.Locked)
		{
			return 502943; // You can only trap an unlocked object.
		}

		return container.TrapType != TrapType.None ? 502945 : 0;
	}

	private bool Acquire(object target, out int message)
	{
		LockableContainer container = target as LockableContainer;

		message = Verify(container);

		if (message > 0)
		{
			return false;
		}

		Container = container;
		return true;
	}

	public override void EndCraftAction()
	{
		From.SendLocalizedMessage(502921); // What would you like to set a trap on?
		From.Target = new ContainerTarget(this);
	}

	private class ContainerTarget : Target
	{
		private readonly TrapCraft _mTrapCraft;

		public ContainerTarget(TrapCraft trapCraft)
			: base(-1, false, TargetFlags.None)
		{
			_mTrapCraft = trapCraft;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{

			if (_mTrapCraft.Acquire(targeted, out int message))
			{
				_mTrapCraft.CraftItem.CompleteCraft(_mTrapCraft.Quality, false, _mTrapCraft.From, _mTrapCraft.CraftSystem, _mTrapCraft.TypeRes, _mTrapCraft.Tool, _mTrapCraft);
			}
			else
			{
				Failure(message);
			}
		}

		protected override void OnTargetCancel(Mobile from, TargetCancelType cancelType)
		{
			if (cancelType == TargetCancelType.Canceled)
			{
				Failure(0);
			}
		}

		private void Failure(int message)
		{
			Mobile from = _mTrapCraft.From;
			ITool tool = _mTrapCraft.Tool;

			if (tool is {Deleted: false, UsesRemaining: > 0})
			{
				from.SendGump(new CraftGump(from, _mTrapCraft.CraftSystem, tool, message));
			}
			else if (message > 0)
			{
				from.SendLocalizedMessage(message);
			}
		}
	}

	public override Item CompleteCraft(out int message)
	{
		message = Verify(Container);

		if (message != 0) return null;
		int trapLevel = (int)(From.Skills.Tinkering.Value / 10);

		Container.TrapType = TrapType;
		Container.TrapPower = trapLevel * 9;
		Container.TrapLevel = trapLevel;
		Container.TrapOnLockpick = true;

		message = 1005639; // Trap is disabled until you lock the chest.

		return null;
	}
}

[CraftItemId(0x1BFC)]
public class DartTrapCraft : TrapCraft
{
	public override TrapType TrapType => TrapType.DartTrap;

	public DartTrapCraft(Mobile from, CraftItem craftItem, CraftSystem craftSystem, Type typeRes, ITool tool, int quality)
		: base(from, craftItem, craftSystem, typeRes, tool, quality)
	{
	}
}

[CraftItemId(0x113E)]
public class PoisonTrapCraft : TrapCraft
{
	public override TrapType TrapType => TrapType.PoisonTrap;

	public PoisonTrapCraft(Mobile from, CraftItem craftItem, CraftSystem craftSystem, Type typeRes, ITool tool, int quality)
		: base(from, craftItem, craftSystem, typeRes, tool, quality)
	{
	}
}

[CraftItemId(0x370C)]
public class ExplosionTrapCraft : TrapCraft
{
	public override TrapType TrapType => TrapType.ExplosionTrap;

	public ExplosionTrapCraft(Mobile from, CraftItem craftItem, CraftSystem craftSystem, Type typeRes, ITool tool, int quality)
		: base(from, craftItem, craftSystem, typeRes, tool, quality)
	{
	}
}
