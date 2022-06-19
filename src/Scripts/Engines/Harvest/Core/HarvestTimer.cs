using System;

namespace Server.Engines.Harvest;

public class HarvestTimer : Timer
{
	private readonly Mobile _mFrom;
	private readonly Item _mTool;
	private readonly HarvestSystem _mSystem;
	private readonly HarvestDefinition _mDefinition;
	private readonly object _mToHarvest;
	private readonly object _mLocked;
	private readonly int _mCount;
	private int _mIndex;
	public HarvestTimer(Mobile from, Item tool, HarvestSystem system, HarvestDefinition def, object toHarvest, object locked)
		: base(TimeSpan.Zero, def.EffectDelay)
	{
		_mFrom = from;
		_mTool = tool;
		_mSystem = system;
		_mDefinition = def;
		_mToHarvest = toHarvest;
		_mLocked = locked;
		_mCount = Utility.RandomList(def.EffectCounts);
	}

	protected override void OnTick()
	{
		if (!_mSystem.OnHarvesting(_mFrom, _mTool, _mDefinition, _mToHarvest, _mLocked, ++_mIndex == _mCount))
		{
			Stop();
		}
	}
}
