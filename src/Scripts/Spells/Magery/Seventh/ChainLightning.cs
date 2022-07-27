using Server.Mobiles;
using Server.Targeting;
using System;
using System.Linq;

namespace Server.Spells.Seventh;

public class ChainLightningSpell : MagerySpell
{
	private static readonly SpellInfo m_Info = new(
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

	private void Target(IPoint3D p)
	{
		if (!Caster.CanSee(p))
		{
			Caster.SendLocalizedMessage(500237); // Target can not be seen.
		}
		else if (SpellHelper.CheckTown(p, Caster) && CheckSequence())
		{
			SpellHelper.Turn(Caster, p);

			if (p is Item item)
			{
				p = item.GetWorldLocation();
			}

			var targets = AcquireIndirectTargets(p, 2).ToList();
			var count = Math.Max(1, targets.Count);

			foreach (var dam in targets)
			{
				var id = dam;
				var m = id as Mobile;

				double damage = Core.AOS ? GetNewAosDamage(51, 1, 5, id is PlayerMobile, id) : Utility.Random(27, 22);

				switch (Core.AOS)
				{
					case true when count > 2:
						damage = damage * 2 / count;
						break;
					case false:
						damage /= count;
						break;
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
		private readonly ChainLightningSpell _owner;
		public InternalTarget(ChainLightningSpell owner)
			: base(Core.ML ? 10 : 12, true, TargetFlags.None)
		{
			_owner = owner;
		}

		protected override void OnTarget(Mobile from, object o)
		{
			if (o is IPoint3D p)
			{
				_owner.Target(p);
			}
		}

		protected override void OnTargetFinish(Mobile from)
		{
			_owner.FinishSequence();
		}
	}
}
