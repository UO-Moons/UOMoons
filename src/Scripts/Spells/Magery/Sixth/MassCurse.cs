using Server.Spells.Fourth;
using Server.Targeting;
using System.Collections.Generic;
using System.Linq;

namespace Server.Spells.Sixth;

public class MassCurseSpell : MagerySpell
{
	private static readonly SpellInfo m_Info = new SpellInfo(
		"Mass Curse", "Vas Des Sanct",
		218,
		9031,
		false,
		Reagent.Garlic,
		Reagent.Nightshade,
		Reagent.MandrakeRoot,
		Reagent.SulfurousAsh
	);

	public override SpellCircle Circle => SpellCircle.Sixth;

	public MassCurseSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
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

	private void Target(IPoint3D p)
	{
		if (!Caster.CanSee(p))
		{
			Caster.SendLocalizedMessage(500237); // Target can not be seen.
		}
		else if (SpellHelper.CheckTown(p, Caster) && CheckSequence())
		{
			SpellHelper.Turn(Caster, p);
			SpellHelper.GetSurfaceTop(ref p);

			foreach (var m in AcquireIndirectTargets(p, 2).OfType<Mobile>())
			{
				CurseSpell.DoCurse(Caster, m, true);
			}
		}

		FinishSequence();
	}

	private class InternalTarget : Target
	{
		private readonly MassCurseSpell _owner;

		public InternalTarget(MassCurseSpell owner) : base(owner.SpellRange, true, TargetFlags.None)
		{
			_owner = owner;
		}

		protected override void OnTarget(Mobile from, object o)
		{
			if (o is IPoint3D p)
				_owner.Target(p);
		}

		protected override void OnTargetFinish(Mobile from)
		{
			_owner.FinishSequence();
		}
	}
}
