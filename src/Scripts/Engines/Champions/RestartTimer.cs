using System;

namespace Server.Engines.Champions;

public class RestartTimer : Timer
{
	private readonly ChampionSpawn _mSpawn;
	public RestartTimer(ChampionSpawn spawn, TimeSpan delay)
		: base(delay)
	{
		_mSpawn = spawn;
		Priority = TimerPriority.FiveSeconds;
	}

	protected override void OnTick()
	{
		_mSpawn.EndRestart();
	}
}
