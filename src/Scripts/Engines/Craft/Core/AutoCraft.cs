using Server.Items;
using Server.Prompts;
using System;
using System.Collections.Generic;

namespace Server.Engines.Craft;

public class MakeNumberCraftPrompt : Prompt
{
	private readonly Mobile _mFrom;
	private readonly CraftSystem _mCraftSystem;
	private readonly CraftItem _mCraftItem;
	private readonly ITool _mTool;

	public MakeNumberCraftPrompt(Mobile from, CraftSystem system, CraftItem item, ITool tool)
	{
		_mFrom = from;
		_mCraftSystem = system;
		_mCraftItem = item;
		_mTool = tool;
	}

	public override void OnCancel(Mobile from)
	{
		_mFrom.SendLocalizedMessage(501806); //Request cancelled.
		from.SendGump(new CraftGump(_mFrom, _mCraftSystem, _mTool, null));
	}

	public override void OnResponse(Mobile from, string text)
	{
		int amount = Utility.ToInt32(text);

		if (amount < 1 || amount > 100)
		{
			from.SendLocalizedMessage(1112587); // Invalid Entry.
			ResendGump();
		}
		else
		{
			AutoCraftTimer.EndTimer(from);
			var unused = new AutoCraftTimer(_mFrom, _mCraftSystem, _mCraftItem, _mTool, amount, TimeSpan.FromSeconds(_mCraftSystem.Delay * _mCraftSystem.MaxCraftEffect + 1.0), TimeSpan.FromSeconds(_mCraftSystem.Delay * _mCraftSystem.MaxCraftEffect + 1.0));

			CraftContext context = _mCraftSystem.GetContext(from);

			if (context != null)
			{
				context.MakeTotal = amount;
			}
		}
	}

	public void ResendGump()
	{
		_mFrom.SendGump(new CraftGump(_mFrom, _mCraftSystem, _mTool, null));
	}
}

public class AutoCraftTimer : Timer
{
	public static Dictionary<Mobile, AutoCraftTimer> AutoCraftTable { get; } = new();

	private readonly Mobile _mFrom;
	private readonly CraftSystem _mCraftSystem;
	private readonly CraftItem _mCraftItem;

	private readonly ITool _mTool;
	private int _mTicks;
	private readonly Type _mTypeRes;

	public int Amount { get; }
	public int Attempts { get; private set; }

	public AutoCraftTimer(Mobile from, CraftSystem system, CraftItem item, ITool tool, int amount, TimeSpan delay, TimeSpan interval)
		: base(delay, interval)
	{
		_mFrom = from;
		_mCraftSystem = system;
		_mCraftItem = item;
		_mTool = tool;
		Amount = amount;
		_mTicks = 0;
		Attempts = 0;

		CraftContext context = _mCraftSystem.GetContext(_mFrom);

		if (context != null)
		{
			CraftSubResCol res = _mCraftItem.UseSubRes2 ? _mCraftSystem.CraftSubRes2 : _mCraftSystem.CraftSubRes;
			int resIndex = _mCraftItem.UseSubRes2 ? context.LastResourceIndex2 : context.LastResourceIndex;

			if (resIndex > -1)
			{
				_mTypeRes = res.GetAt(resIndex).ItemType;
			}
		}

		AutoCraftTable[from] = this;

		Start();
	}

	public AutoCraftTimer(Mobile from, CraftSystem system, CraftItem item, ITool tool, int amount)
		: this(from, system, item, tool, amount, TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3))
	{
	}

	protected override void OnTick()
	{
		_mTicks++;

		if (_mFrom.NetState == null)
		{
			EndTimer(_mFrom);
			return;
		}

		CraftItem();

		if (_mTicks >= Amount)
		{
			EndTimer(_mFrom);
		}
	}

	private void CraftItem()
	{
		if (_mFrom.HasGump(typeof(CraftGump)))
		{
			_mFrom.CloseGump(typeof(CraftGump));
		}

		if (_mFrom.HasGump(typeof(CraftGumpItem)))
		{
			_mFrom.CloseGump(typeof(CraftGumpItem));
		}

		Attempts++;

		if (_mCraftItem.TryCraft != null)
		{
			_mCraftItem.TryCraft(_mFrom, _mCraftItem, _mTool);
		}
		else
		{
			_mCraftSystem.CreateItem(_mFrom, _mCraftItem.ItemType, _mTypeRes, _mTool, _mCraftItem);
		}
	}

	public static void EndTimer(Mobile from)
	{
		if (!AutoCraftTable.ContainsKey(from)) return;
		AutoCraftTable[from].Stop();
		AutoCraftTable.Remove(from);
	}

	public static bool HasTimer(Mobile from)
	{
		return from != null && AutoCraftTable.ContainsKey(from);
	}
}
