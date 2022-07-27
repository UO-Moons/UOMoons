using System;
using System.Collections;
using Server.SkillHandlers;

namespace Server.Spells.Ninjitsu;

public class SurpriseAttack : NinjaMove
{
	private static readonly Hashtable m_Table = new();

	public override int BaseMana => 20;
	public override double RequiredSkill => Core.ML ? 60.0 : 30.0;
	public override TextDefinition AbilityMessage => new(1063128);// You prepare to surprise your prey.
	public override bool ValidatesDuringHit => false;
	public static bool GetMalus(Mobile target, ref int malus)
	{
		if (m_Table[target] is not SurpriseAttackInfo info)
			return false;

		malus = info.Malus;
		return true;
	}

	public override bool Validate(Mobile from)
	{
		if (!from.Hidden || from.AllowedStealthSteps <= 0)
		{
			from.SendLocalizedMessage(1063087); // You must be in stealth mode to use this ability.
			return false;
		}

		return base.Validate(from);
	}

	public override bool OnBeforeSwing(Mobile attacker, Mobile defender)
	{
		bool valid = Validate(attacker) && CheckMana(attacker, true);

		if (!valid)
			return false;

		attacker.BeginAction(typeof(Stealth));
		Timer.DelayCall(TimeSpan.FromSeconds(5.0), delegate { attacker.EndAction(typeof(Stealth)); });

		return true;
	}

	public override void OnHit(Mobile attacker, Mobile defender, int damage)
	{
		//Validates before swing
		ClearCurrentMove(attacker);

		attacker.SendLocalizedMessage(1063129); // You catch your opponent off guard with your Surprise Attack!
		defender.SendLocalizedMessage(1063130); // Your defenses are lowered as your opponent surprises you!

		defender.FixedParticles(0x37B9, 1, 5, 0x26DA, 0, 3, EffectLayer.Head);

		attacker.RevealingAction();

		SurpriseAttackInfo info;

		if (m_Table.Contains(defender))
		{
			info = (SurpriseAttackInfo)m_Table[defender];

			info?.Timer?.Stop();

			m_Table.Remove(defender);
		}

		int ninjitsu = attacker.Skills[SkillName.Ninjitsu].Fixed;

		int malus = ninjitsu / 60 + (int)Tracking.GetStalkingBonus(attacker, defender);

		info = new SurpriseAttackInfo(defender, malus);
		info.Timer = Timer.DelayCall(TimeSpan.FromSeconds(8.0), new TimerStateCallback(EndSurprise), info);

		m_Table[defender] = info;

		CheckGain(attacker);
	}

	public override void OnMiss(Mobile attacker, Mobile defender)
	{
		ClearCurrentMove(attacker);

		attacker.SendLocalizedMessage(1063161); // You failed to properly use the element of surprise.

		attacker.RevealingAction();
	}

	private static void EndSurprise(object state)
	{
		SurpriseAttackInfo info = (SurpriseAttackInfo)state;

		info.Timer?.Stop();

		info.Target.SendLocalizedMessage(1063131); // Your defenses have returned to normal.

		m_Table.Remove(info.Target);
	}

	private class SurpriseAttackInfo
	{
		public readonly Mobile Target;
		public readonly int Malus;
		public Timer Timer;
		public SurpriseAttackInfo(Mobile target, int effect)
		{
			Target = target;
			Malus = effect;
		}
	}
}
