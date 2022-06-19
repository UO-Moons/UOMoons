namespace Server.Engines.Harvest;

public class HarvestSoundTimer : Timer
{
	private readonly Mobile _mFrom;
	private readonly Item _mTool;
	private readonly HarvestSystem _mSystem;
	private readonly HarvestDefinition _mDefinition;
	private readonly object _mToHarvest;
	private readonly object _mLocked;
	private readonly bool _mLast;
	public HarvestSoundTimer(Mobile from, Item tool, HarvestSystem system, HarvestDefinition def, object toHarvest, object locked, bool last)
		: base(def.EffectSoundDelay)
	{
		_mFrom = from;
		_mTool = tool;
		_mSystem = system;
		_mDefinition = def;
		_mToHarvest = toHarvest;
		_mLocked = locked;
		_mLast = last;
	}

	protected override void OnTick()
	{
		_mSystem.DoHarvestingSound(_mFrom, _mTool, _mDefinition, _mToHarvest);

		if (_mLast)
		{
			_mSystem.FinishHarvesting(_mFrom, _mTool, _mDefinition, _mToHarvest, _mLocked);
		}
	}
}
