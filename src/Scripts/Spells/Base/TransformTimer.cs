using System;

namespace Server.Spells;

public class TransformTimer : Timer
{
	private readonly Mobile _mobile;
	private readonly ITransformationSpell _spell;

	public TransformTimer(Mobile from, ITransformationSpell spell)
		: base(TimeSpan.FromSeconds(spell.TickRate), TimeSpan.FromSeconds(spell.TickRate))
	{
		_mobile = from;
		_spell = spell;

		Priority = TimerPriority.TwoFiftyMs;
	}

	protected override void OnTick()
	{
		if (_mobile.Deleted || !_mobile.Alive || _mobile.Body != _spell.Body || _mobile.Hue != _spell.Hue)
		{
			TransformationSpellHelper.RemoveContext(_mobile, true);
			Stop();
		}
		else
		{
			_spell.OnTick(_mobile);
		}
	}
}
