using Server.Targeting;
using System;

namespace Server.Spells.Third;

public class FireballSpell : MagerySpell
{
	private static readonly SpellInfo m_Info = new(
		"Fireball", "Vas Flam",
		203,
		9041,
		Reagent.BlackPearl
	);

	public override SpellCircle Circle => SpellCircle.Third;
	public override TargetFlags SpellTargetFlags => TargetFlags.Harmful;
	public override bool DelayedDamage => true;

	public FireballSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
	{
	}

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

	public void Target(IDamageable m)
	{
		if (!Caster.CanSee(m))
		{
			Caster.SendLocalizedMessage(500237); // Target can not be seen.
		}
		else if (CheckHSequence(m))
		{
			IDamageable source = Caster;
			IDamageable target = m;

			SpellHelper.Turn(Caster, m);

			if (SpellHelper.CheckReflect((int)Circle, ref source, ref target))
			{
				Timer.DelayCall(TimeSpan.FromSeconds(.5), () =>
				{
					source.MovingParticles(target, 0x36D4, 7, 0, false, true, 9502, 4019, 0x160);
					source.PlaySound(Core.AOS ? 0x15E : 0x44B);
				});
			}

			double damage = 0;

			if (Core.AOS)
			{
				damage = GetNewAosDamage(19, 1, 5, m);
			}
			else if (m is Mobile mobile)
			{
				damage = Utility.Random(10, 7);

				if (CheckResisted(mobile))
				{
					damage *= 0.75;

					mobile.SendLocalizedMessage(501783); // You feel yourself resisting magical energy.
				}

				damage *= GetDamageScalar(mobile);
			}

			if (damage > 0)
			{
				Caster.MovingParticles(m, 0x36D4, 7, 0, false, true, 9502, 4019, 0x160);
				Caster.PlaySound(Core.AOS ? 0x15E : 0x44B);

				SpellHelper.Damage(this, target, damage, 0, 100, 0, 0, 0);
			}
		}

		FinishSequence();
	}

	private class InternalTarget : Target
	{
		private readonly FireballSpell _owner;

		public InternalTarget(FireballSpell owner) : base(owner.SpellRange, false, TargetFlags.Harmful)
		{
			_owner = owner;
		}

		protected override void OnTarget(Mobile from, object o)
		{
			if (o is IDamageable damageable)
			{
				_owner.Target(damageable);
			}
		}

		protected override void OnTargetFinish(Mobile from)
		{
			_owner.FinishSequence();
		}
	}
}
