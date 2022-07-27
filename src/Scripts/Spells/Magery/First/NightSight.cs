using Server.Targeting;

namespace Server.Spells.First;

public class NightSightSpell : MagerySpell
{
	private static readonly SpellInfo m_Info = new(
		"Night Sight", "In Lor",
		236,
		9031,
		Reagent.SulfurousAsh,
		Reagent.SpidersSilk
	);

	public override SpellCircle Circle => SpellCircle.First;
	public override TargetFlags SpellTargetFlags => TargetFlags.Beneficial;

	public NightSightSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
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

	private void Target(Mobile targeted)
	{
		if (CheckBSequence(targeted))
		{
			SpellHelper.Turn(Caster, targeted);

			if (targeted.BeginAction(typeof(LightCycle)))
			{
				new LightCycle.NightSightTimer(targeted).Start();
				int level = (int)(LightCycle.DungeonLevel * ((Core.AOS ? targeted.Skills[SkillName.Magery].Value : Caster.Skills[SkillName.Magery].Value) / 100));

				if (level < 0)
					level = 0;

				targeted.LightLevel = level;

				targeted.FixedParticles(0x376A, 9, 32, 5007, EffectLayer.Waist);
				targeted.PlaySound(0x1E3);

				BuffInfo.AddBuff(targeted, new BuffInfo(BuffIcon.NightSight, 1075643)); //Night Sight/You ignore lighting effects
			}
			else
			{
				Caster.SendMessage("{0} already have nightsight.", Caster == targeted ? "You" : "They");
			}
		}

		FinishSequence();
	}

	private class InternalTarget : Target
	{
		private readonly NightSightSpell _owner;

		public InternalTarget(NightSightSpell owner) : base(12, false, TargetFlags.Beneficial)
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
