using Server.Items;
using Server.Spells;
using System;
using System.Collections.Generic;

namespace Server.Engines.Craft;

public class DefInscription : CraftSystem
{
	private readonly Dictionary<Type, int> _buffer = new();

	public override SkillName MainSkill => SkillName.Inscribe;

	public override int GumpTitleNumber => 1044009;

	private static CraftSystem _mCraftSystem;

	public static CraftSystem CraftSystem => _mCraftSystem ??= new DefInscription();

	public override double GetChanceAtMin(CraftItem item)
	{
		return 0.0; // 0%
	}

	public DefInscription() : base(1, 1, 1.25) // base( 1, 1, 3.0 )
	{
		_mCraftSystem = this;
	}

	public override int CanCraft(Mobile from, ITool tool, Type typeItem)
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

		if (typeItem == null || !typeItem.IsSubclassOf(typeof(SpellScroll))) return 0;
		if (!_buffer.ContainsKey(typeItem))
		{
			object o = Activator.CreateInstance(typeItem);

			switch (o)
			{
				case SpellScroll spellScroll:
				{
					_buffer[typeItem] = spellScroll.SpellID;
					spellScroll.Delete();
					break;
				}
				case IEntity entity:
					entity.Delete();
					return 1042404; // You don't have that spell!
			}
		}

		int id = _buffer[typeItem];
		Spellbook book = Spellbook.Find(from, id);

		if (book == null || !book.HasSpell(id))
		{
			return 1042404; // You don't have that spell!
		}

		return 0;
	}

	public override void PlayCraftEffect(Mobile from)
	{
		from.PlaySound(0x249);
	}

	private static readonly Type TypeofSpellScroll = typeof(SpellScroll);

	public override int PlayEndingEffect(Mobile from, bool failed, bool lostMaterial, bool toolBroken, int quality, bool makersMark, CraftItem item)
	{
		if (toolBroken)
		{
			from.SendLocalizedMessage(1044038); // You have worn out your tool
		}

		if (!TypeofSpellScroll.IsAssignableFrom(item.ItemType)) //  not a scroll
		{
			if (failed)
			{
				if (lostMaterial)
				{
					return 1044043; // You failed to create the item, and some of your materials are lost.
				}

				return 1044157; // You failed to create the item, but no materials were lost.
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
		// You fail to inscribe the scroll, and the scroll is ruined.// You inscribe the spell and put the scroll in your backpack.
		return failed ? 501630 : 501629;
	}

	private int _mCircle, _mMana;

	private int _mIndex;

	private void AddSpell(Type type, params Reg[] regs)
	{
		double minSkill, maxSkill;

		switch (_mCircle)
		{
			default:
				minSkill = -25.0;
				maxSkill = 25.0;
				break;
			case 1:
				minSkill = -10.8;
				maxSkill = 39.2;
				break;
			case 2:
				minSkill = 03.5;
				maxSkill = 53.5;
				break;
			case 3:
				minSkill = 17.8;
				maxSkill = 67.8;
				break;
			case 4:
				minSkill = 32.1;
				maxSkill = 82.1;
				break;
			case 5:
				minSkill = 46.4;
				maxSkill = 96.4;
				break;
			case 6:
				minSkill = 60.7;
				maxSkill = 110.7;
				break;
			case 7:
				minSkill = 75.0;
				maxSkill = 125.0;
				break;
		}


		int index = AddCraft(type, 1044369 + _mCircle, 1044381 + _mIndex++, minSkill, maxSkill, Reagent.Types[(int)regs[0]], 1044353 + (int)regs[0], 1, 1044361 + (int)regs[0]);

		for (var i = 1; i < regs.Length; ++i)
			AddRes(index, Reagent.Types[(int)regs[i]], 1044353 + (int)regs[i], 1, 1044361 + (int)regs[i]);

		AddRes(index, typeof(BlankScroll), 1044377, 1, 1044378);

		SetManaReq(index, _mMana);
	}

	private void AddNecroSpell(int spell, int mana, double minSkill, Type type, params Reg[] regs)
	{
		int id = Reagent.GetRegLocalization(regs[0]);
		int index = AddCraft(type, 1061677, 1060509 + spell, minSkill, minSkill + 1.0, Reagent.Types[(int)regs[0]], id, 1, 501627);

		for (int i = 1; i < regs.Length; ++i)
		{
			id = Reagent.GetRegLocalization(regs[i]);
			AddRes(index, Reagent.Types[(int)regs[i]], id, 1, 501627);
		}

		AddRes(index, typeof(BlankScroll), 1044377, 1, 1044378);

		SetManaReq(index, mana);
	}

	private void AddMysticismSpell(int spell, int mana, double minSkill, Type type, params Reg[] regs)
	{
		int id = Reagent.GetRegLocalization(regs[0]);
		int index = AddCraft(type, 1111671, spell, minSkill, minSkill + 1.0, Reagent.Types[(int)regs[0]], id, 1, 501627);    //Yes, on OSI it's only 1.0 skill diff'.  Don't blame me, blame OSI.

		for (int i = 1; i < regs.Length; ++i)
		{
			id = Reagent.GetRegLocalization(regs[i]);
			AddRes(index, Reagent.Types[(int)regs[i]], id, 1, 501627);
		}

		AddRes(index, typeof(BlankScroll), 1044377, 1, 1044378);

		SetManaReq(index, mana);
	}

	public override void InitCraftList()
	{
		_mCircle = 0;
		_mMana = 4;

		AddSpell(typeof(ReactiveArmorScroll), Reg.Garlic, Reg.SpidersSilk, Reg.SulfurousAsh);
		AddSpell(typeof(ClumsyScroll), Reg.Bloodmoss, Reg.Nightshade);
		AddSpell(typeof(CreateFoodScroll), Reg.Garlic, Reg.Ginseng, Reg.MandrakeRoot);
		AddSpell(typeof(FeeblemindScroll), Reg.Nightshade, Reg.Ginseng);
		AddSpell(typeof(HealScroll), Reg.Garlic, Reg.Ginseng, Reg.SpidersSilk);
		AddSpell(typeof(MagicArrowScroll), Reg.SulfurousAsh);
		AddSpell(typeof(NightSightScroll), Reg.SpidersSilk, Reg.SulfurousAsh);
		AddSpell(typeof(WeakenScroll), Reg.Garlic, Reg.Nightshade);

		_mCircle = 1;
		_mMana = 6;

		AddSpell(typeof(AgilityScroll), Reg.Bloodmoss, Reg.MandrakeRoot);
		AddSpell(typeof(CunningScroll), Reg.Nightshade, Reg.MandrakeRoot);
		AddSpell(typeof(CureScroll), Reg.Garlic, Reg.Ginseng);
		AddSpell(typeof(HarmScroll), Reg.Nightshade, Reg.SpidersSilk);
		AddSpell(typeof(MagicTrapScroll), Reg.Garlic, Reg.SpidersSilk, Reg.SulfurousAsh);
		AddSpell(typeof(MagicUnTrapScroll), Reg.Bloodmoss, Reg.SulfurousAsh);
		AddSpell(typeof(ProtectionScroll), Reg.Garlic, Reg.Ginseng, Reg.SulfurousAsh);
		AddSpell(typeof(StrengthScroll), Reg.Nightshade, Reg.MandrakeRoot);

		_mCircle = 2;
		_mMana = 9;

		AddSpell(typeof(BlessScroll), Reg.Garlic, Reg.MandrakeRoot);
		AddSpell(typeof(FireballScroll), Reg.BlackPearl);
		AddSpell(typeof(MagicLockScroll), Reg.Bloodmoss, Reg.Garlic, Reg.SulfurousAsh);
		AddSpell(typeof(PoisonScroll), Reg.Nightshade);
		AddSpell(typeof(TelekinisisScroll), Reg.Bloodmoss, Reg.MandrakeRoot);
		AddSpell(typeof(TeleportScroll), Reg.Bloodmoss, Reg.MandrakeRoot);
		AddSpell(typeof(UnlockScroll), Reg.Bloodmoss, Reg.SulfurousAsh);
		AddSpell(typeof(WallOfStoneScroll), Reg.Bloodmoss, Reg.Garlic);

		_mCircle = 3;
		_mMana = 11;

		AddSpell(typeof(ArchCureScroll), Reg.Garlic, Reg.Ginseng, Reg.MandrakeRoot);
		AddSpell(typeof(ArchProtectionScroll), Reg.Garlic, Reg.Ginseng, Reg.MandrakeRoot, Reg.SulfurousAsh);
		AddSpell(typeof(CurseScroll), Reg.Garlic, Reg.Nightshade, Reg.SulfurousAsh);
		AddSpell(typeof(FireFieldScroll), Reg.BlackPearl, Reg.SpidersSilk, Reg.SulfurousAsh);
		AddSpell(typeof(GreaterHealScroll), Reg.Garlic, Reg.SpidersSilk, Reg.MandrakeRoot, Reg.Ginseng);
		AddSpell(typeof(LightningScroll), Reg.MandrakeRoot, Reg.SulfurousAsh);
		AddSpell(typeof(ManaDrainScroll), Reg.BlackPearl, Reg.SpidersSilk, Reg.MandrakeRoot);
		AddSpell(typeof(RecallScroll), Reg.BlackPearl, Reg.Bloodmoss, Reg.MandrakeRoot);

		_mCircle = 4;
		_mMana = 14;

		AddSpell(typeof(BladeSpiritsScroll), Reg.BlackPearl, Reg.Nightshade, Reg.MandrakeRoot);
		AddSpell(typeof(DispelFieldScroll), Reg.BlackPearl, Reg.Garlic, Reg.SpidersSilk, Reg.SulfurousAsh);
		AddSpell(typeof(IncognitoScroll), Reg.Bloodmoss, Reg.Garlic, Reg.Nightshade);
		AddSpell(typeof(MagicReflectScroll), Reg.Garlic, Reg.MandrakeRoot, Reg.SpidersSilk);
		AddSpell(typeof(MindBlastScroll), Reg.BlackPearl, Reg.MandrakeRoot, Reg.Nightshade, Reg.SulfurousAsh);
		AddSpell(typeof(ParalyzeScroll), Reg.Garlic, Reg.MandrakeRoot, Reg.SpidersSilk);
		AddSpell(typeof(PoisonFieldScroll), Reg.BlackPearl, Reg.Nightshade, Reg.SpidersSilk);
		AddSpell(typeof(SummonCreatureScroll), Reg.Bloodmoss, Reg.MandrakeRoot, Reg.SpidersSilk);

		_mCircle = 5;
		_mMana = 20;

		AddSpell(typeof(DispelScroll), Reg.Garlic, Reg.MandrakeRoot, Reg.SulfurousAsh);
		AddSpell(typeof(EnergyBoltScroll), Reg.BlackPearl, Reg.Nightshade);
		AddSpell(typeof(ExplosionScroll), Reg.Bloodmoss, Reg.MandrakeRoot);
		AddSpell(typeof(InvisibilityScroll), Reg.Bloodmoss, Reg.Nightshade);
		AddSpell(typeof(MarkScroll), Reg.Bloodmoss, Reg.BlackPearl, Reg.MandrakeRoot);
		AddSpell(typeof(MassCurseScroll), Reg.Garlic, Reg.MandrakeRoot, Reg.Nightshade, Reg.SulfurousAsh);
		AddSpell(typeof(ParalyzeFieldScroll), Reg.BlackPearl, Reg.Ginseng, Reg.SpidersSilk);
		AddSpell(typeof(RevealScroll), Reg.Bloodmoss, Reg.SulfurousAsh);

		_mCircle = 6;
		_mMana = 40;

		AddSpell(typeof(ChainLightningScroll), Reg.BlackPearl, Reg.Bloodmoss, Reg.MandrakeRoot, Reg.SulfurousAsh);
		AddSpell(typeof(EnergyFieldScroll), Reg.BlackPearl, Reg.MandrakeRoot, Reg.SpidersSilk, Reg.SulfurousAsh);
		AddSpell(typeof(FlamestrikeScroll), Reg.SpidersSilk, Reg.SulfurousAsh);
		AddSpell(typeof(GateTravelScroll), Reg.BlackPearl, Reg.MandrakeRoot, Reg.SulfurousAsh);
		AddSpell(typeof(ManaVampireScroll), Reg.BlackPearl, Reg.Bloodmoss, Reg.MandrakeRoot, Reg.SpidersSilk);
		AddSpell(typeof(MassDispelScroll), Reg.BlackPearl, Reg.Garlic, Reg.MandrakeRoot, Reg.SulfurousAsh);
		AddSpell(typeof(MeteorSwarmScroll), Reg.Bloodmoss, Reg.MandrakeRoot, Reg.SulfurousAsh, Reg.SpidersSilk);
		AddSpell(typeof(PolymorphScroll), Reg.Bloodmoss, Reg.MandrakeRoot, Reg.SpidersSilk);

		_mCircle = 7;
		_mMana = 50;

		AddSpell(typeof(EarthquakeScroll), Reg.Bloodmoss, Reg.MandrakeRoot, Reg.Ginseng, Reg.SulfurousAsh);
		AddSpell(typeof(EnergyVortexScroll), Reg.BlackPearl, Reg.Bloodmoss, Reg.MandrakeRoot, Reg.Nightshade);
		AddSpell(typeof(ResurrectionScroll), Reg.Bloodmoss, Reg.Garlic, Reg.Ginseng);
		AddSpell(typeof(SummonAirElementalScroll), Reg.Bloodmoss, Reg.MandrakeRoot, Reg.SpidersSilk);
		AddSpell(typeof(SummonDaemonScroll), Reg.Bloodmoss, Reg.MandrakeRoot, Reg.SpidersSilk, Reg.SulfurousAsh);
		AddSpell(typeof(SummonEarthElementalScroll), Reg.Bloodmoss, Reg.MandrakeRoot, Reg.SpidersSilk);
		AddSpell(typeof(SummonFireElementalScroll), Reg.Bloodmoss, Reg.MandrakeRoot, Reg.SpidersSilk, Reg.SulfurousAsh);
		AddSpell(typeof(SummonWaterElementalScroll), Reg.Bloodmoss, Reg.MandrakeRoot, Reg.SpidersSilk);

		if (Core.SE)
		{
			AddNecroSpell(0, 23, 39.6, typeof(AnimateDeadScroll), Reg.GraveDust, Reg.DaemonBlood);
			AddNecroSpell(1, 13, 19.6, typeof(BloodOathScroll), Reg.DaemonBlood);
			AddNecroSpell(2, 11, 19.6, typeof(CorpseSkinScroll), Reg.BatWing, Reg.GraveDust);
			AddNecroSpell(3, 7, 19.6, typeof(CurseWeaponScroll), Reg.PigIron);
			AddNecroSpell(4, 11, 19.6, typeof(EvilOmenScroll), Reg.BatWing, Reg.NoxCrystal);
			AddNecroSpell(5, 11, 39.6, typeof(HorrificBeastScroll), Reg.BatWing, Reg.DaemonBlood);
			AddNecroSpell(6, 23, 69.6, typeof(LichFormScroll), Reg.GraveDust, Reg.DaemonBlood, Reg.NoxCrystal);
			AddNecroSpell(7, 17, 29.6, typeof(MindRotScroll), Reg.BatWing, Reg.DaemonBlood, Reg.PigIron);
			AddNecroSpell(8, 5, 19.6, typeof(PainSpikeScroll), Reg.GraveDust, Reg.PigIron);
			AddNecroSpell(9, 17, 49.6, typeof(PoisonStrikeScroll), Reg.NoxCrystal);
			AddNecroSpell(10, 29, 64.6, typeof(StrangleScroll), Reg.DaemonBlood, Reg.NoxCrystal);
			AddNecroSpell(11, 17, 29.6, typeof(SummonFamiliarScroll), Reg.BatWing, Reg.GraveDust, Reg.DaemonBlood);
			AddNecroSpell(12, 23, 98.6, typeof(VampiricEmbraceScroll), Reg.BatWing, Reg.NoxCrystal, Reg.PigIron);
			AddNecroSpell(13, 41, 79.6, typeof(VengefulSpiritScroll), Reg.BatWing, Reg.GraveDust, Reg.PigIron);
			AddNecroSpell(14, 23, 59.6, typeof(WitherScroll), Reg.GraveDust, Reg.NoxCrystal, Reg.PigIron);
			AddNecroSpell(15, 17, 19.6, typeof(WraithFormScroll), Reg.NoxCrystal, Reg.PigIron);
			AddNecroSpell(16, 40, 79.6, typeof(ExorcismScroll), Reg.NoxCrystal, Reg.GraveDust);
		}

		int index;

		if (Core.ML)
		{
			index = AddCraft(typeof(EnchantedSwitch), 1044294, 1072893, 45.0, 95.0, typeof(BlankScroll), 1044377, 1, 1044378);
			AddRes(index, typeof(SpidersSilk), 1044360, 1, 1044253);
			AddRes(index, typeof(BlackPearl), 1044353, 1, 1044253);
			AddRes(index, typeof(SwitchItem), 1073464, 1, 1044253);
			ForceNonExceptional(index);
			SetNeededExpansion(index, Expansion.ML);

			index = AddCraft(typeof(RunedPrism), 1044294, 1073465, 45.0, 95.0, typeof(BlankScroll), 1044377, 1, 1044378);
			AddRes(index, typeof(SpidersSilk), 1044360, 1, 1044253);
			AddRes(index, typeof(BlackPearl), 1044353, 1, 1044253);
			AddRes(index, typeof(HollowPrism), 1072895, 1, 1044253);
			ForceNonExceptional(index);
			SetNeededExpansion(index, Expansion.ML);
		}

		// Runebook
		index = AddCraft(typeof(Runebook), 1044294, 1041267, 45.0, 95.0, typeof(BlankScroll), 1044377, 8, 1044378);
		AddRes(index, typeof(RecallScroll), 1044445, 1, 1044253);
		AddRes(index, typeof(GateTravelScroll), 1044446, 1, 1044253);

		if (Core.AOS)
		{
			AddCraft(typeof(BulkOrders.BulkOrderBook), 1044294, 1028793, 65.0, 115.0, typeof(BlankScroll), 1044377, 10, 1044378);
		}

		if (Core.SE)
		{
			AddCraft(typeof(Spellbook), 1044294, 1023834, 50.0, 126, typeof(BlankScroll), 1044377, 10, 1044378);
		}

		if (Core.ML)
		{
			index = AddCraft(typeof(ScrappersCompendium), 1044294, 1072940, 75.0, 125.0, typeof(BlankScroll), 1044377, 100, 1044378);
			AddRes(index, typeof(DreadHornMane), 1032682, 1, 1044253);
			AddRes(index, typeof(Taint), 1032679, 10, 1044253);
			AddRes(index, typeof(Corruption), 1032676, 10, 1044253);
			AddRecipe(index, (int)TinkerRecipes.ScrappersCompendium);
			ForceNonExceptional(index);

			index = AddCraft(typeof(SpellbookEngraver), 1044294, 1072151, 75.0, 100.0, typeof(Feather), 1044562, 1, 1044563);
			AddRes(index, typeof(BlackPearl), 1015001, 7, 1044253);


			AddCraft(typeof(NecromancerSpellbook), 1044294, 1074909, 50.0, 100.0, typeof(BlankScroll), 1044377, 10, 1044378);

			AddCraft(typeof(MysticBook), 1044294, 1031677, 50.0, 100.0, typeof(BlankScroll), 1044377, 10, 1044378);
		}

		if (Core.SA)
		{
			AddCraft(typeof(MysticBook), 1044294, 1031677, 50.0, 150.0, typeof(BlankScroll), 1044377, 10, 1044378);

			AddMysticismSpell(1031678, 4, 0.0, typeof(NetherBoltScroll), Reg.SulfurousAsh, Reg.BlackPearl);
			AddMysticismSpell(1031679, 4, 0.0, typeof(HealingStoneScroll), Reg.Bone, Reg.Garlic, Reg.Ginseng, Reg.SpidersSilk);
			AddMysticismSpell(1031680, 6, 0.0, typeof(PurgeMagicScroll), Reg.FertileDirt, Reg.Garlic, Reg.MandrakeRoot, Reg.SulfurousAsh);
			AddMysticismSpell(1031681, 6, 0.0, typeof(EnchantScroll), Reg.SpidersSilk, Reg.MandrakeRoot, Reg.SulfurousAsh);
			AddMysticismSpell(1031682, 9, 3.5, typeof(SleepScroll), Reg.SpidersSilk, Reg.BlackPearl, Reg.Nightshade);
			AddMysticismSpell(1031683, 9, 3.5, typeof(EagleStrikeScroll), Reg.SpidersSilk, Reg.Bloodmoss, Reg.MandrakeRoot, Reg.Bone);
			AddMysticismSpell(1031684, 11, 17.8, typeof(AnimatedWeaponScroll), Reg.Bone, Reg.BlackPearl, Reg.MandrakeRoot, Reg.Nightshade);
			AddMysticismSpell(1031685, 11, 17.8, typeof(StoneFormScroll), Reg.Bloodmoss, Reg.FertileDirt, Reg.Garlic);
			AddMysticismSpell(1031686, 14, 32.1, typeof(SpellTriggerScroll), Reg.SpidersSilk, Reg.MandrakeRoot, Reg.Garlic, Reg.DragonBlood);
			AddMysticismSpell(1031687, 14, 32.1, typeof(MassSleepScroll), Reg.SpidersSilk, Reg.Nightshade, Reg.Ginseng);
			AddMysticismSpell(1031688, 20, 46.4, typeof(CleansingWindsScroll), Reg.Ginseng, Reg.Garlic, Reg.DragonBlood, Reg.MandrakeRoot);
			AddMysticismSpell(1031689, 20, 46.4, typeof(BombardScroll), Reg.Garlic, Reg.DragonBlood, Reg.SulfurousAsh, Reg.Bloodmoss);
			AddMysticismSpell(1031690, 40, 60.7, typeof(SpellPlagueScroll), Reg.DaemonBone, Reg.DragonBlood, Reg.MandrakeRoot, Reg.Nightshade, Reg.SulfurousAsh, Reg.DaemonBone);
			AddMysticismSpell(1031691, 40, 60.7, typeof(HailStormScroll), Reg.DragonBlood, Reg.BlackPearl, Reg.MandrakeRoot, Reg.Bloodmoss);
			AddMysticismSpell(1031692, 50, 75.0, typeof(NetherCycloneScroll), Reg.Bloodmoss, Reg.Nightshade, Reg.SulfurousAsh, Reg.MandrakeRoot);
			AddMysticismSpell(1031693, 50, 75.0, typeof(RisingColossusScroll), Reg.DaemonBone, Reg.FertileDirt, Reg.DragonBlood, Reg.Nightshade, Reg.MandrakeRoot);
		}

		MarkOption = true;
	}
}
