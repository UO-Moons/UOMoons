using Server.Mobiles;
using System;

namespace Server.Spells.Eighth;

public class SummonDaemonSpell : MagerySpell
{
	private static readonly SpellInfo m_Info = new(
		"Summon Daemon", "Kal Vas Xen Corp",
		269,
		9050,
		false,
		Reagent.Bloodmoss,
		Reagent.MandrakeRoot,
		Reagent.SpidersSilk,
		Reagent.SulfurousAsh
	);

	public override SpellCircle Circle => SpellCircle.Eighth;
	public override bool RequireTarget => false;

	public SummonDaemonSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
	{
	}

	public override bool CheckCast()
	{
		if (!base.CheckCast())
			return false;

		if (Caster.Followers + (Core.SE ? 4 : 5) <= Caster.FollowersMax)
			return true;
		Caster.SendLocalizedMessage(1049645); // You have too many followers to summon that creature.
		return false;

	}

	public override void OnCast()
	{
		if (CheckSequence())
		{
			TimeSpan duration = TimeSpan.FromSeconds(2 * Caster.Skills.Magery.Fixed / 5.0);

			BaseCreature daemon = new SummonedDaemon();
			SpellHelper.Summon(daemon, Caster, 0x216, duration, false, false);
			daemon.FixedParticles(0x3728, 8, 20, 5042, EffectLayer.Head);
		}

		FinishSequence();
	}
}
