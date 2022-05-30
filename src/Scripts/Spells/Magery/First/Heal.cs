using Server.Mobiles;
using Server.Network;
using Server.Targeting;

namespace Server.Spells.First
{
	public class HealSpell : MagerySpell
	{
		private static readonly SpellInfo m_Info = new SpellInfo(
				"Heal", "In Mani",
				224,
				9061,
				Reagent.Garlic,
				Reagent.Ginseng,
				Reagent.SpidersSilk
			);

		public override SpellCircle Circle => SpellCircle.First;
		public override TargetFlags SpellTargetFlags => TargetFlags.Beneficial;

		public HealSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
		{
		}

		public override bool CheckCast()
		{
			if (Engines.ConPVP.DuelContext.CheckSuddenDeath(Caster))
			{
				Caster.SendMessage(0x22, "You cannot cast this spell when in sudden death.");
				return false;
			}

			return base.CheckCast();
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

		public void Target(Mobile m)
		{
			if (!Caster.CanSee(m))
			{
				Caster.SendLocalizedMessage(500237); // Target can not be seen.
			}
			else if (m.IsDeadBondedPet)
			{
				Caster.SendLocalizedMessage(1060177); // You cannot heal a creature that is already dead!
			}
			else if (m is BaseCreature creature && creature.IsAnimatedDead)
			{
				Caster.SendLocalizedMessage(1061654); // You cannot heal that which is not alive.
			}
			else if (m is Golem)
			{
				Caster.LocalOverheadMessage(MessageType.Regular, 0x3B2, 500951); // You cannot heal that.
			}
			else if (m.Poisoned || Server.Items.MortalStrike.IsWounded(m))
			{
				Caster.LocalOverheadMessage(MessageType.Regular, 0x22, (Caster == m) ? 1005000 : 1010398);
			}
			else if (CheckBSequence(m))
			{
				SpellHelper.Turn(Caster, m);

				int toHeal;

				if (Core.AOS)
				{
					toHeal = Caster.Skills.Magery.Fixed / 120;
					toHeal += Utility.RandomMinMax(1, 4);

					if (Core.SE && Caster != m)
						toHeal = (int)(toHeal * 1.5);
				}
				else
				{
					toHeal = (int)(Caster.Skills[SkillName.Magery].Value * 0.1);
					toHeal += Utility.Random(1, 5);
				}

				//m.Heal( toHeal, Caster );
				SpellHelper.Heal(toHeal, m, Caster);

				m.FixedParticles(0x376A, 9, 32, 5005, EffectLayer.Waist);
				m.PlaySound(0x1F2);
			}
			FinishSequence();
		}

		public class InternalTarget : Target
		{
			private readonly HealSpell m_Owner;

			public InternalTarget(HealSpell owner) : base(owner.SpellRange, false, TargetFlags.Beneficial)
			{
				m_Owner = owner;
			}

			protected override void OnTarget(Mobile from, object o)
			{
				if (o is Mobile mobile)
				{
					m_Owner.Target(mobile);
				}
			}

			protected override void OnTargetFinish(Mobile from)
			{
				m_Owner.FinishSequence();
			}
		}
	}
}
