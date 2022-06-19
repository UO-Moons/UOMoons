using Server.Items;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Engines.Craft;

public enum CraftEca
{
	ChanceMinusSixty,
	FiftyPercentChanceMinusTenPercent,
	ChanceMinusSixtyToFourtyFive
}

public abstract class CraftSystem
{
	public static List<CraftSystem> Systems { get; set; }

	public int MinCraftEffect { get; }
	public int MaxCraftEffect { get; }
	public double Delay { get; }

	public CraftItemCol CraftItems { get; }
	public CraftGroupCol CraftGroups { get; }
	public CraftSubResCol CraftSubRes { get; }
	public CraftSubResCol CraftSubRes2 { get; }

	public abstract SkillName MainSkill { get; }

	public virtual int GumpTitleNumber => 0;
	public virtual string GumpTitleString => "";

	public virtual CraftEca Eca => CraftEca.ChanceMinusSixty;

	private readonly Dictionary<Mobile, CraftContext> _mContextTable = new();

	public abstract double GetChanceAtMin(CraftItem item);

	public virtual bool RetainsColorFrom(CraftItem item, Type type)
	{
		return false;
	}

	public void AddContext(Mobile m, CraftContext c)
	{
		if (c == null || m == null || c.System != this)
		{
			return;
		}

		_mContextTable[m] = c;
	}

	public CraftContext GetContext(Mobile m)
	{
		if (m == null)
		{
			return null;
		}

		if (m.Deleted)
		{
			_mContextTable.Remove(m);
			return null;
		}

		_mContextTable.TryGetValue(m, out CraftContext c);

		if (c == null)
		{
			_mContextTable[m] = c = new CraftContext(m, this);
		}

		return c;
	}

	public void OnMade(Mobile m, CraftItem item)
	{
		CraftContext c = GetContext(m);

		c?.OnMade(item);
	}

	public void OnRepair(Mobile m, ITool tool, Item deed, Item addon, IEntity e)
	{
		if (tool is Item)
		{
		}

		//EventSink.InvokeRepairItem(new RepairItemEventArgs(m, source, e));
	}

	public bool Resmelt { get; set; }
	public bool Repair { get; set; }
	public bool MarkOption { get; set; }
	public bool CanEnhance { get; set; }
	public bool QuestOption { get; set; }
	public bool CanAlter { get; set; }

	protected CraftSystem(int minCraftEffect, int maxCraftEffect, double delay)
	{
		MinCraftEffect = minCraftEffect;
		MaxCraftEffect = maxCraftEffect;
		Delay = delay;

		CraftItems = new CraftItemCol();
		CraftGroups = new CraftGroupCol();
		CraftSubRes = new CraftSubResCol();
		CraftSubRes2 = new CraftSubResCol();

		InitCraftList();
		AddSystem(this);
	}

	private static void AddSystem(CraftSystem system)
	{
		Systems ??= new List<CraftSystem>();

		Systems.Add(system);
	}

	private readonly Type[] _globalNoConsume =
	{
		typeof(CapturedEssence), typeof(EyeOfTheTravesty), typeof(DiseasedBark),  typeof(LardOfParoxysmus), typeof(GrizzledBones), typeof(DreadHornMane),

		typeof(Blight), typeof(Corruption), typeof(Muculent), typeof(Scourge), typeof(Putrefaction), typeof(Taint),

		// Tailoring
		typeof(MidnightBracers), typeof(CrimsonCincture), typeof(LeurociansMempoOfFortune),

		// Blacksmithy
		typeof(LeggingsOfBane), typeof(GauntletsOfNobility),

		// Carpentry
		typeof(StaffOfTheMagi),

		// Tinkering
		typeof(Factions.Silver), typeof(RingOfTheElements), typeof(HatOfTheMagi),
	};

	public virtual bool ConsumeOnFailure(Mobile from, Type resourceType, CraftItem craftItem)
	{
		return _globalNoConsume.All(t => t != resourceType);
	}

	public virtual bool ConsumeOnFailure(Mobile from, Type resourceType, CraftItem craftItem, ref MasterCraftsmanTalisman talisman)
	{
		if (!ConsumeOnFailure(from, resourceType, craftItem))
		{
			return false;
		}

		Item item = from.FindItemOnLayer(Layer.Talisman);

		if (item is not MasterCraftsmanTalisman mct) return true;
		if (mct.Charges <= 0) return true;
		talisman = mct;
		return false;

	}

	public void CreateItem(Mobile from, Type type, Type typeRes, ITool tool, CraftItem realCraftItem)
	{
		CraftItem craftItem = CraftItems.SearchFor(type);
		if (craftItem != null)
		{
			realCraftItem.Craft(from, this, typeRes, tool);
		}
	}

	public int AddCraft(Type typeItem, TextDefinition group, TextDefinition name, double minSkill, double maxSkill, Type typeRes, TextDefinition nameRes, int amount)
	{
		return AddCraft(typeItem, group, name, MainSkill, minSkill, maxSkill, typeRes, nameRes, amount, "");
	}

	public int AddCraft(Type typeItem, TextDefinition group, TextDefinition name, double minSkill, double maxSkill, Type typeRes, TextDefinition nameRes, int amount, TextDefinition message)
	{
		return AddCraft(typeItem, group, name, MainSkill, minSkill, maxSkill, typeRes, nameRes, amount, message);
	}

	public int AddCraft(Type typeItem, TextDefinition group, TextDefinition name, SkillName skillToMake, double minSkill, double maxSkill, Type typeRes, TextDefinition nameRes, int amount)
	{
		return AddCraft(typeItem, group, name, skillToMake, minSkill, maxSkill, typeRes, nameRes, amount, "");
	}

	public int AddCraft(Type typeItem, TextDefinition group, TextDefinition name, SkillName skillToMake, double minSkill, double maxSkill, Type typeRes, TextDefinition nameRes, int amount, TextDefinition message)
	{
		CraftItem craftItem = new(typeItem, group, name);
		craftItem.AddRes(typeRes, nameRes, amount, message);
		craftItem.AddSkill(skillToMake, minSkill, maxSkill);

		DoGroup(group, craftItem);
		return CraftItems.Add(craftItem);
	}

	private void DoGroup(TextDefinition groupName, CraftItem craftItem)
	{
		int index = CraftGroups.SearchFor(groupName);

		if (index == -1)
		{
			CraftGroup craftGroup = new(groupName);
			craftGroup.AddCraftItem(craftItem);
			CraftGroups.Add(craftGroup);
		}
		else
		{
			CraftGroups.GetAt(index).AddCraftItem(craftItem);
		}
	}

	public void SetItemHue(int index, int hue)
	{
		CraftItem craftItem = CraftItems.GetAt(index);
		craftItem.ItemHue = hue;
	}

	public void SetManaReq(int index, int mana)
	{
		CraftItem craftItem = CraftItems.GetAt(index);
		craftItem.Mana = mana;
	}

	public void SetStamReq(int index, int stam)
	{
		CraftItem craftItem = CraftItems.GetAt(index);
		craftItem.Stam = stam;
	}

	public void SetHitsReq(int index, int hits)
	{
		CraftItem craftItem = CraftItems.GetAt(index);
		craftItem.Hits = hits;
	}

	public void SetUseAllRes(int index, bool useAll)
	{
		CraftItem craftItem = CraftItems.GetAt(index);
		craftItem.UseAllRes = useAll;
	}

	public void SetForceTypeRes(int index, bool value)
	{
		CraftItem craftItem = CraftItems.GetAt(index);
		craftItem.ForceTypeRes = value;
	}

	public void SetNeedHeat(int index, bool needHeat)
	{
		CraftItem craftItem = CraftItems.GetAt(index);
		craftItem.NeedHeat = needHeat;
	}

	public void SetNeedOven(int index, bool needOven)
	{
		CraftItem craftItem = CraftItems.GetAt(index);
		craftItem.NeedOven = needOven;
	}

	public void SetNeedMaker(int index, bool needMaker)
	{
		CraftItem craftItem = CraftItems.GetAt(index);
		craftItem.NeedMaker = needMaker;
	}

	public void SetNeedWater(int index, bool needWater)
	{
		CraftItem craftItem = CraftItems.GetAt(index);
		craftItem.NeedWater = needWater;
	}

	public void SetBeverageType(int index, BeverageType requiredBeverage)
	{
		CraftItem craftItem = CraftItems.GetAt(index);
		craftItem.RequiredBeverage = requiredBeverage;
	}

	public void SetNeedMill(int index, bool needMill)
	{
		CraftItem craftItem = CraftItems.GetAt(index);
		craftItem.NeedMill = needMill;
	}

	public void SetNeededExpansion(int index, Expansion expansion)
	{
		CraftItem craftItem = CraftItems.GetAt(index);
		craftItem.RequiredExpansion = expansion;
	}

	public void SetNeededThemePack(int index, ThemePack pack)
	{
		CraftItem craftItem = CraftItems.GetAt(index);
		craftItem.RequiredThemePack = pack;
	}

	public void SetRequiresBasketWeaving(int index)
	{
		CraftItem craftItem = CraftItems.GetAt(index);
		craftItem.RequiresBasketWeaving = true;
	}

	public void SetRequireResTarget(int index)
	{
		CraftItem craftItem = CraftItems.GetAt(index);
		craftItem.RequiresResTarget = true;
	}

	public void SetRequiresMechanicalLife(int index)
	{
		CraftItem craftItem = CraftItems.GetAt(index);
		craftItem.RequiresMechanicalLife = true;
	}

	public void SetData(int index, object data)
	{
		CraftItem craftItem = CraftItems.GetAt(index);
		craftItem.Data = data;
	}

	public void SetDisplayId(int index, int id)
	{
		CraftItem craftItem = CraftItems.GetAt(index);
		craftItem.DisplayId = id;
	}

	public void SetMutateAction(int index, Action<Mobile, Item, ITool> action)
	{
		CraftItem craftItem = CraftItems.GetAt(index);
		craftItem.MutateAction = action;
	}

	public void SetForceSuccess(int index, int success)
	{
		CraftItem craftItem = CraftItems.GetAt(index);
		craftItem.ForceSuccessChance = success;
	}

	public void AddRes(int index, Type type, TextDefinition name, int amount)
	{
		AddRes(index, type, name, amount, "");
	}

	public void AddRes(int index, Type type, TextDefinition name, int amount, TextDefinition message)
	{
		CraftItem craftItem = CraftItems.GetAt(index);
		craftItem.AddRes(type, name, amount, message);
	}

	public void AddResCallback(int index, Func<Mobile, ConsumeType, int> func)
	{
		CraftItem craftItem = CraftItems.GetAt(index);
		craftItem.ConsumeResCallback = func;
	}

	public void AddSkill(int index, SkillName skillToMake, double minSkill, double maxSkill)
	{
		CraftItem craftItem = CraftItems.GetAt(index);
		craftItem.AddSkill(skillToMake, minSkill, maxSkill);
	}

	public void SetUseSubRes2(int index, bool val)
	{
		CraftItem craftItem = CraftItems.GetAt(index);
		craftItem.UseSubRes2 = val;
	}

	public void AddRecipe(int index, int id)
	{
		CraftItem craftItem = CraftItems.GetAt(index);
		craftItem.AddRecipe(id, this);
	}

	public void ForceNonExceptional(int index)
	{
		CraftItem craftItem = CraftItems.GetAt(index);
		craftItem.ForceNonExceptional = true;
	}

	public void ForceExceptional(int index)
	{
		CraftItem craftItem = CraftItems.GetAt(index);
		craftItem.ForceExceptional = true;
	}

	public void SetMinSkillOffset(int index, double skillOffset)
	{
		CraftItem craftItem = CraftItems.GetAt(index);
		craftItem.MinSkillOffset = skillOffset;
	}

	public void AddCraftAction(int index, Action<Mobile, CraftItem, ITool> action)
	{
		CraftItem craftItem = CraftItems.GetAt(index);
		craftItem.TryCraft = action;
	}

	public void AddCreateItem(int index, Func<Mobile, CraftItem, ITool, Item> func)
	{
		CraftItem craftItem = CraftItems.GetAt(index);
		craftItem.CreateItem = func;
	}

	public void SetSubRes(Type type, string name)
	{
		CraftSubRes.ResType = type;
		CraftSubRes.NameString = name;
		CraftSubRes.Init = true;
	}

	public void SetSubRes(Type type, int name)
	{
		CraftSubRes.ResType = type;
		CraftSubRes.NameNumber = name;
		CraftSubRes.Init = true;
	}

	public void AddSubRes(Type type, int name, double reqSkill, object message)
	{
		CraftSubRes craftSubRes = new(type, name, reqSkill, message);
		CraftSubRes.Add(craftSubRes);
	}

	public void AddSubRes(Type type, int name, double reqSkill, int genericName, object message)
	{
		CraftSubRes craftSubRes = new(type, name, reqSkill, genericName, message);
		CraftSubRes.Add(craftSubRes);
	}

	public void AddSubRes(Type type, string name, double reqSkill, object message)
	{
		CraftSubRes craftSubRes = new(type, name, reqSkill, message);
		CraftSubRes.Add(craftSubRes);
	}

	public void SetSubRes2(Type type, string name)
	{
		CraftSubRes2.ResType = type;
		CraftSubRes2.NameString = name;
		CraftSubRes2.Init = true;
	}

	public void SetSubRes2(Type type, int name)
	{
		CraftSubRes2.ResType = type;
		CraftSubRes2.NameNumber = name;
		CraftSubRes2.Init = true;
	}

	public void AddSubRes2(Type type, int name, double reqSkill, object message)
	{
		CraftSubRes craftSubRes = new(type, name, reqSkill, message);
		CraftSubRes2.Add(craftSubRes);
	}

	public void AddSubRes2(Type type, int name, double reqSkill, int genericName, object message)
	{
		CraftSubRes craftSubRes = new(type, name, reqSkill, genericName, message);
		CraftSubRes2.Add(craftSubRes);
	}

	public void AddSubRes2(Type type, string name, double reqSkill, object message)
	{
		CraftSubRes craftSubRes = new(type, name, reqSkill, message);
		CraftSubRes2.Add(craftSubRes);
	}

	public abstract void InitCraftList();
	public abstract void PlayCraftEffect(Mobile from);
	public abstract int PlayEndingEffect(Mobile from, bool failed, bool lostMaterial, bool toolBroken, int quality, bool makersMark, CraftItem item);
	public abstract int CanCraft(Mobile from, ITool tool, Type itemType);
}
