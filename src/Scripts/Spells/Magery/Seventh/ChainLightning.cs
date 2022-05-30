using Server.Mobiles;
using Server.Targeting;
using System;
using System.Linq;

namespace Server.Spells.Seventh
{
	public class ChainLightningSpell : MagerySpell
	{
		private static readonly SpellInfo m_Info = new SpellInfo(
				"Chain Lightning", "Vas Ort Grav",
				209,
				9022,
				false,
				Reagent.BlackPearl,
				Reagent.Bloodmoss,
				Reagent.MandrakeRoot,
				Reagent.SulfurousAsh
			);

		public override SpellCircle Circle => SpellCircle.Seventh;

		public ChainLightningSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
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
				if (SpellTarget is IPoint3D target)
					Target(target);
				else
					FinishSequence();
			}
		}

		public override bool DelayedDamage => true;

		public void Target(IPoint3D p)
		{
			if (!Caster.CanSee(p))
			{
				Caster.SendLocalizedMessage(500237); // Target can not be seen.
			}
			else if (SpellHelper.CheckTown(p, Caster) && CheckSequence())
			{
				SpellHelper.Turn(Caster, p);

				if (p is Item)
				{
					p = ((Item)p).GetWorldLocation();
				}

				var targets = AcquireIndirectTargets(p, 2).ToList();
				var count = Math.Max(1, targets.Count);

				foreach (var dam in targets)
				{
					var id = dam;
					var m = id as Mobile;
					double damage;

					if (Core.AOS)
					{
						damage = GetNewAosDamage(51, 1, 5, id is PlayerMobile, id);
					}
					else
					{
						damage = Utility.Random(27, 22);
					}

					if (Core.AOS && count > 2)
					{
						damage = (damage * 2) / count;
					}
					else if (!Core.AOS)
					{
						damage /= count;
					}

					if (!Core.AOS && m != null && CheckResisted(m))
					{
						damage *= 0.5;

						m.SendLocalizedMessage(501783); // You feel yourself resisting magical energy.
					}

					Mobile source = Caster;
					SpellHelper.CheckReflect((int)Circle, ref source, ref id, SpellDamageType);

					if (m != null)
					{
						damage *= GetDamageScalar(m);
					}

					Effects.SendBoltEffect(id, true, 0, false);

					Caster.DoHarmful(id);
					SpellHelper.Damage(this, id, damage, 0, 0, 0, 0, 100);
				}

				ColUtility.Free(targets);
			}

			FinishSequence();
		}

		private class InternalTarget : Target
		{
			private readonly ChainLightningSpell m_Owner;
			public InternalTarget(ChainLightningSpell owner)
				: base(Core.ML ? 10 : 12, true, TargetFlags.None)
			{
				m_Owner = owner;
			}

			protected override void OnTarget(Mobile from, object o)
			{
				IPoint3D p = o as IPoint3D;

				if (p != null)
				{
					m_Owner.Target(p);
				}
			}

			protected override void OnTargetFinish(Mobile from)
			{
				m_Owner.FinishSequence();
			}
		}
	}
}
