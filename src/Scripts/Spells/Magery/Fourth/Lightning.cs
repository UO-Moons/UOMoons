using Server.Mobiles;
using Server.Targeting;

namespace Server.Spells.Fourth
{
	public class LightningSpell : MagerySpell
	{
		private static readonly SpellInfo m_Info = new(
				"Lightning", "Por Ort Grav",
				239,
				9021,
				Reagent.MandrakeRoot,
				Reagent.SulfurousAsh
			);

		public override SpellCircle Circle => SpellCircle.Fourth;
		public override TargetFlags SpellTargetFlags => TargetFlags.Harmful;

		public LightningSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
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
				if (SpellTarget is IDamageable target)
					Target(target);
				else
					FinishSequence();
			}
		}

		public override bool DelayedDamage => false;

		public void Target(IDamageable m)
		{
			Mobile mob = m as Mobile;
			if (!Caster.CanSee(m))
			{
				Caster.SendLocalizedMessage(500237); // Target can not be seen.
			}
			else if (CheckHSequence(m))
			{
				Mobile source = Caster;
				SpellHelper.Turn(Caster, m.Location);

				SpellHelper.CheckReflect((int)Circle, ref source, ref m);

				double damage = 0;

				if (Core.AOS)
				{
					damage = GetNewAosDamage(23, 1, 4, m);
				}
				else if (mob != null)
				{
					damage = Utility.Random(12, 9);

					if (CheckResisted(mob))
					{
						damage *= 0.75;

						mob.SendLocalizedMessage(501783); // You feel yourself resisting magical energy.
					}

					damage *= GetDamageScalar(mob);
				}

				if (m is Mobile)
				{
					Effects.SendBoltEffect(m, true, 0, false);
				}
				else
				{
					Effects.SendBoltEffect(EffectMobile.Create(m.Location, m.Map, EffectMobile.DefaultDuration), true, 0, false);
				}

				if (damage > 0)
				{
					SpellHelper.Damage(this, m, damage, 0, 0, 0, 0, 100);
				}
			}
			FinishSequence();
		}

		private class InternalTarget : Target
		{
			private readonly LightningSpell m_Owner;

			public InternalTarget(LightningSpell owner) : base(owner.SpellRange, false, TargetFlags.Harmful)
			{
				m_Owner = owner;
			}

			protected override void OnTarget(Mobile from, object o)
			{
				if (o is Mobile)
					m_Owner.Target((Mobile)o);
			}

			protected override void OnTargetFinish(Mobile from)
			{
				m_Owner.FinishSequence();
			}
		}
	}
}
