using Server.Mobiles;
using Server.Targeting;
using System;

namespace Server.Spells.Eighth;

public class EnergyVortexSpell : MagerySpell
{
	private static readonly SpellInfo m_Info = new(
		"Energy Vortex", "Vas Corp Por",
		260,
		9032,
		false,
		Reagent.Bloodmoss,
		Reagent.BlackPearl,
		Reagent.MandrakeRoot,
		Reagent.Nightshade
	);

	public override SpellCircle Circle => SpellCircle.Eighth;
	public override bool CanTargetGround => true;

	public EnergyVortexSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
	{
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
			if (SpellTarget is IPoint3D point)
				Target(point);
			else
				FinishSequence();
		}
	}

	private void Target(IPoint3D p)
	{
		Map map = Caster.Map;

		SpellHelper.GetSurfaceTop(ref p);

		if (map == null || !map.CanSpawnMobile(p.X, p.Y, p.Z))
		{
			Caster.SendLocalizedMessage(501942); // That location is blocked.
		}
		else if (SpellHelper.CheckTown(p, Caster) && CheckSequence())
		{
			var duration = Core.AOS ? TimeSpan.FromSeconds(90.0) : TimeSpan.FromSeconds(Utility.Random(80, 40));

			BaseCreature.Summon(new EnergyVortex(), false, Caster, new Point3D(p), 0x212, duration);
		}

		FinishSequence();
	}

	public class InternalTarget : Target
	{
		private EnergyVortexSpell _owner;

		public InternalTarget(EnergyVortexSpell owner) : base(owner.SpellRange, true, TargetFlags.None)
		{
			_owner = owner;
		}

		protected override void OnTarget(Mobile from, object o)
		{
			if (o is IPoint3D point3D)
				_owner.Target(point3D);
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
