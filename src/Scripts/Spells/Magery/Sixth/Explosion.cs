using Server.Targeting;
using System;

namespace Server.Spells.Sixth;

public class ExplosionSpell : MagerySpell
{
	private static readonly SpellInfo m_Info = new(
		"Explosion", "Vas Ort Flam",
		230,
		9041,
		Reagent.Bloodmoss,
		Reagent.MandrakeRoot
	);

	public override SpellCircle Circle => SpellCircle.Sixth;
	public override TargetFlags SpellTargetFlags => TargetFlags.Harmful;

	public ExplosionSpell(Mobile caster, Item scroll)
		: base(caster, scroll, m_Info)
	{
	}

	public override bool DelayedDamageStacking => !Core.AOS;

	public override void OnCast()
	{
		if (Precast)
		{
			Caster.Target = new InternalTarget(this);
		}
		else
		{
			if (SpellTarget is Mobile target)
				Target(target);
			else
				FinishSequence();
		}
	}

	public override bool DelayedDamage => false;

	private void Target(Mobile m)
	{
		if (!Caster.CanSee(m))
		{
			Caster.SendLocalizedMessage(500237); // Target can not be seen.
		}
		else if (Caster.CanBeHarmful(m) && CheckSequence())
		{
			Mobile attacker = Caster, defender = m;

			SpellHelper.Turn(Caster, m);

			SpellHelper.CheckReflect((int)Circle, Caster, ref m);

			InternalTimer t = new(this, attacker, defender, m);
			t.Start();
		}

		FinishSequence();
	}

	private class InternalTimer : Timer
	{
		private readonly MagerySpell _spell;
		private readonly Mobile _target;
		private readonly Mobile _attacker, _defender;

		public InternalTimer(MagerySpell spell, Mobile attacker, Mobile defender, Mobile target)
			: base(TimeSpan.FromSeconds(Core.AOS ? 3.0 : 2.5))
		{
			_spell = spell;
			_attacker = attacker;
			_defender = defender;
			_target = target;

			_spell?.StartDelayedDamageContext(attacker, this);

			Priority = TimerPriority.FiftyMs;
		}

		protected override void OnTick()
		{
			if (_attacker.HarmfulCheck(_defender))
			{
				double damage;

				if (Core.AOS)
				{
					damage = _spell.GetNewAosDamage(40, 1, 5, _defender);
				}
				else
				{
					damage = Utility.Random(23, 22);

					if (_spell.CheckResisted(_target))
					{
						damage *= 0.75;

						_target.SendLocalizedMessage(501783); // You feel yourself resisting magical energy.
					}

					damage *= _spell.GetDamageScalar(_target);
				}

				_target.FixedParticles(0x36BD, 20, 10, 5044, EffectLayer.Head);
				_target.PlaySound(0x307);

				SpellHelper.Damage(_spell, _target, damage, 0, 100, 0, 0, 0);

				_spell?.RemoveDelayedDamageContext(_attacker);
			}
		}
	}

	private class InternalTarget : Target
	{
		private readonly ExplosionSpell _owner;

		public InternalTarget(ExplosionSpell owner) : base(owner.SpellRange, false, TargetFlags.Harmful)
		{
			_owner = owner;
		}

		protected override void OnTarget(Mobile from, object o)
		{
			if (o is Mobile mobile)
				_owner.Target(mobile);
		}

		protected override void OnTargetFinish(Mobile from)
		{
			_owner.FinishSequence();
		}
	}
}
