using Server.Mobiles;
using System;

namespace Server.Spells;

public class UnsummonTimer : Timer
{
	private readonly BaseCreature _creature;
	private readonly Mobile _caster;

	public UnsummonTimer(Mobile caster, BaseCreature creature, TimeSpan delay) : base(delay)
	{
		_caster = caster;
		_creature = creature;
		Priority = TimerPriority.OneSecond;
	}

	protected override void OnStop()
	{
		//Clear the timer
		_creature.UnsummonTimer = null;

		if (!_creature.Deleted)
			_creature.Delete();

	}
}
