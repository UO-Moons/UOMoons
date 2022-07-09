using System;

namespace Server.Engines;

public class MacroCheckTimer : Timer
{
	private readonly CheckPlayer _check;

	public MacroCheckTimer(CheckPlayer check) : base(TimeSpan.FromMinutes(1))
	{
		Priority = TimerPriority.FiveSeconds;
		_check = check;
	}

	protected override void OnTick()
	{
		_check.TimeOut();
	}
}
