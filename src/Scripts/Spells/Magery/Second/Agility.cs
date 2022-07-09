using Server.Targeting;
using System;

namespace Server.Spells.Second;

public class AgilitySpell : MagerySpell
{
	private static readonly SpellInfo m_Info = new(
		"Agility", "Ex Uus",
		212,
		9061,
		Reagent.Bloodmoss,
		Reagent.MandrakeRoot
	);

	public override SpellCircle Circle => SpellCircle.Second;
	public override TargetFlags SpellTargetFlags => TargetFlags.Beneficial;

	public AgilitySpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
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
		else if (CheckBSequence(m))
		{
			int oldDex = SpellHelper.GetBuffOffset(m, StatType.Dex);
			int newDex = SpellHelper.GetOffset(Caster, m, StatType.Dex, false, true);

			if (newDex < oldDex || newDex == 0)
			{
				DoHurtFizzle();
			}
			else
			{
				SpellHelper.Turn(Caster, m);

				SpellHelper.AddStatBonus(Caster, m, false, StatType.Dex);
				int percentage = (int)(SpellHelper.GetOffsetScalar(Caster, m, false) * 100);
				TimeSpan length = SpellHelper.GetDuration(Caster, m);
				BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.Agility, 1075841, length, m, percentage.ToString()));

				m.FixedParticles(0x375A, 10, 15, 5010, EffectLayer.Waist);
				m.PlaySound(0x1e7);
			}
		}

		FinishSequence();
	}

	private class InternalTarget : Target
	{
		private readonly AgilitySpell _owner;

		public InternalTarget(AgilitySpell owner) : base(owner.SpellRange, false, TargetFlags.Beneficial)
		{
			_owner = owner;
		}

		protected override void OnTarget(Mobile from, object o)
		{
			if (o is Mobile mobile)
			{
				_owner.Target(mobile);
			}
		}

		protected override void OnTargetFinish(Mobile from)
		{
			_owner.FinishSequence();
		}
	}
}
