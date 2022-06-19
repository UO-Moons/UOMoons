using System;

namespace Server.Engines.Champions;

public class SliceTimer : Timer
{
	private readonly ChampionSpawn _mSpawn;
	public SliceTimer(ChampionSpawn spawn)
		: base(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0))
	{
		_mSpawn = spawn;
		Priority = TimerPriority.OneSecond;
	}

	protected override void OnTick()
	{
		_mSpawn.OnSlice();
	}
}
