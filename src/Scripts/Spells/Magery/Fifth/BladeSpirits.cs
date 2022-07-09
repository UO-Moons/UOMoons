using Server.Mobiles;
using Server.Targeting;
using System;

namespace Server.Spells.Fifth;

public class BladeSpiritsSpell : MagerySpell
{
	private static readonly SpellInfo m_Info = new(
		"Blade Spirits", "In Jux Hur Ylem",
		266,
		9040,
		false,
		Reagent.BlackPearl,
		Reagent.MandrakeRoot,
		Reagent.Nightshade
	);

	public override SpellCircle Circle => SpellCircle.Fifth;
	public override bool CanTargetGround => true;

	public BladeSpiritsSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
	{
	}

	public override TimeSpan GetCastDelay()
	{
		if (Core.AOS)
			return TimeSpan.FromTicks(base.GetCastDelay().Ticks * ((Core.SE) ? 3 : 5));

		return base.GetCastDelay() + TimeSpan.FromSeconds(6.0);
	}

	public override bool CheckCast()
	{
		if (!base.CheckCast())
			return false;

		if (Caster.Followers + (Core.SE ? 2 : 1) > Caster.FollowersMax)
		{
			Caster.SendLocalizedMessage(1049645); // You have too many followers to summon that creature.
			return false;
		}

		return true;
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

	public void Target(IPoint3D p)
	{
		if (p is not null)
		{
			Map map = Caster.Map;

			SpellHelper.GetSurfaceTop(ref p);

			if (map == null || !map.CanSpawnMobile(p.X, p.Y, p.Z))
			{
				Caster.SendLocalizedMessage(501942); // That location is blocked.
			}
			else if (SpellHelper.CheckTown(p, Caster) && CheckSequence())
			{
				var duration = TimeSpan.FromSeconds(Core.AOS ? 120 : Utility.Random(80, 40));

				BaseCreature.Summon(new BladeSpirits(), false, Caster, new Point3D(p), 0x212, duration);
			}
		}

		FinishSequence();
	}

	public class InternalTarget : Target
	{
		private BladeSpiritsSpell _owner;

		public InternalTarget(BladeSpiritsSpell owner) : base(owner.SpellRange, true, TargetFlags.None)
		{
			_owner = owner;
		}

		protected override void OnTarget(Mobile from, object o)
		{
			if (o is IPoint3D d)
				_owner.Target(d);
		}

		protected override void OnTargetOutOfLOS(Mobile from, object o)
		{
			from.SendLocalizedMessage(501943); // Target cannot be seen. Try again.
			from.Target = new InternalTarget(_owner);
			from.Target.BeginTimeout(from, TimeoutTime - DateTime.UtcNow);
			_owner = null;
		}

		protected override void OnTargetFinish(Mobile from)
		{
			_owner?.FinishSequence();
		}
	}
}
