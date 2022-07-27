using System;
using Server.Spells.First;
using Server.Targeting;

namespace Server.Spells.Mysticism;

public class NetherBoltSpell : MysticSpell
{
	public override SpellCircle Circle => SpellCircle.First;

	private static readonly SpellInfo m_Info = new(
		"Nether Bolt", "In Corp Ylem",
		230,
		9022,
		Reagent.BlackPearl,
		Reagent.SulfurousAsh
	);

	public NetherBoltSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
	{
	}

	public override bool DelayedDamage => true;
	public override bool DelayedDamageStacking => false;
	public override Type[] DelayDamageFamily => new[] { typeof(MagicArrowSpell) };

	public override void OnCast()
	{
		Caster.Target = new InternalTarget(this);
	}

	private void OnTarget(IDamageable d)
	{
		if (d == null)
		{
			return;
		}

		if (CheckHSequence(d))
		{
			IDamageable target = d;
			IDamageable source = Caster;

			SpellHelper.Turn(Caster, target);

			if (Core.SA && HasDelayContext(target))
			{
				DoHurtFizzle();
				return;
			}

			if (SpellHelper.CheckReflect((int)Circle, ref source, ref target))
			{
				Timer.DelayCall(TimeSpan.FromSeconds(.5), () =>
				{
					source.MovingParticles(target, 0x36D4, 7, 0, false, true, 0x49A, 0, 0, 9502, 4019, 0x160);
					source.PlaySound(0x211);
				});
			}

			double damage = GetNewAosDamage(10, 1, 4, target);

			SpellHelper.Damage(this, target, damage, 0, 0, 0, 0, 0, 100, 0);

			Caster.MovingParticles(d, 0x36D4, 7, 0, false, true, 0x49A, 0, 0, 9502, 4019, 0x160);
			Caster.PlaySound(0x211);
		}

		FinishSequence();
	}

	private class InternalTarget : Target
	{
		private NetherBoltSpell Owner { get; }

		public InternalTarget(NetherBoltSpell owner, bool allowland = false)
			: base(12, allowland, TargetFlags.Harmful)
		{
			Owner = owner;
		}

		protected override void OnTarget(Mobile from, object o)
		{
			if (o == null)
				return;

			if (!from.CanSee(o))
				from.SendLocalizedMessage(500237); // Target can not be seen.
			else if (o is IDamageable damageable)
			{
				SpellHelper.Turn(from, damageable);
				Owner.OnTarget(damageable);
			}
		}

		protected override void OnTargetFinish(Mobile from)
		{
			Owner.FinishSequence();
		}
	}
}
