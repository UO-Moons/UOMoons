using System;
using System.Collections;
using Server.Items;

namespace Server.Spells.Bushido;

public class Evasion : SamuraiSpell
{
	private static readonly SpellInfo m_Info = new(
		"Evasion", null,
		-1,
		9002);
	private static readonly Hashtable m_Table = new();
	public Evasion(Mobile caster, Item scroll)
		: base(caster, scroll, m_Info)
	{
	}

	public override TimeSpan CastDelayBase => TimeSpan.FromSeconds(0.25);
	public override double RequiredSkill => 60.0;
	public override int RequiredMana => 10;

	private static bool VerifyCast(Mobile caster, bool messages)
	{
		if (caster == null) // Sanity
			return false;

		if (caster.FindItemOnLayer(Layer.OneHanded) is not BaseWeapon weap)
			weap = caster.FindItemOnLayer(Layer.TwoHanded) as BaseWeapon;

		if (weap != null)
		{
			if (Core.ML && caster.Skills[weap.Skill].Base < 50)
			{
				if (messages)
				{
					caster.SendLocalizedMessage(1076206); // Your skill with your equipped weapon must be 50 or higher to use Evasion.
				}
				return false;
			}
		}
		else if (caster.FindItemOnLayer(Layer.TwoHanded) is not BaseShield)
		{
			if (messages)
			{
				caster.SendLocalizedMessage(1062944); // You must have a weapon or a shield equipped to use this ability!
			}
			return false;
		}

		if (!caster.CanBeginAction(typeof(Evasion)))
		{
			if (messages)
			{
				caster.SendLocalizedMessage(501789); // You must wait before trying again.
			}
			return false;
		}

		return true;
	}

	public static bool CheckSpellEvasion(Mobile defender)
	{
		BaseWeapon weap = defender.FindItemOnLayer(Layer.OneHanded) as BaseWeapon ?? defender.FindItemOnLayer(Layer.TwoHanded) as BaseWeapon;

		if (Core.ML)
		{
			if (defender.Spell is {IsCasting: true})
			{
				return false;
			}

			if (weap != null)
			{
				if (defender.Skills[weap.Skill].Base < 50)
				{
					return false;
				}
			}
			else if (defender.FindItemOnLayer(Layer.TwoHanded) is not BaseShield)
			{
				return false;
			}
		}

		if (IsEvading(defender) && BaseWeapon.CheckParry(defender))
		{
			defender.Emote("*evades*"); // Yes.  Eew.  Blame OSI.
			defender.FixedEffect(0x37B9, 10, 16);

			if (Core.SA)
			{
				defender.Animate(AnimationType.Block, 0);
			}

			return true;
		}

		return false;
	}

	public static bool IsEvading(Mobile m)
	{
		return m_Table.Contains(m);
	}

	private static TimeSpan GetEvadeDuration(Mobile m)
	{
		/* Evasion duration now scales with Bushido skill
		* 
		* If the player has higher than GM Bushido, and GM Tactics and Anatomy, they get a 1 second bonus
		* Evasion duration range:
		* o 3-6 seconds w/o tactics/anatomy
		* o 6-7 seconds w/ GM+ Bushido and GM tactics/anatomy 
		*/
		if (!Core.ML)
			return TimeSpan.FromSeconds(8.0);

		double seconds = 3;

		if (m.Skills.Bushido.Value > 60)
			seconds += (m.Skills.Bushido.Value - 60) / 20;

		if (m.Skills.Anatomy.Value >= 100.0 && m.Skills.Tactics.Value >= 100.0 && m.Skills.Bushido.Value > 100.0)	//Bushido being HIGHER than 100 for bonus is intended
			seconds++;

		return TimeSpan.FromSeconds((int)seconds);
	}

	public static double GetParryScalar(Mobile m)
	{
		/* Evasion modifier to parry now scales with Bushido skill
		* 
		* If the player has higher than GM Bushido, and at least GM Tactics and Anatomy, they get a bonus to their evasion modifier (10% bonus to the evasion modifier to parry NOT 10% to the final parry chance)
		* 
		* Bonus modifier to parry range: (these are the ranges for the evasion modifier)
		* o 16-40% bonus w/o tactics/anatomy
		* o 42-50% bonus w/ GM+ bushido and GM tactics/anatomy
		*/
		if (!Core.ML)
			return 1.5;

		double bonus = 0;

		if (m.Skills.Bushido.Value >= 60)
			bonus += (m.Skills.Bushido.Value - 60) * .004 + 0.16;

		if (m.Skills.Anatomy.Value >= 100 && m.Skills.Tactics.Value >= 100 && m.Skills.Bushido.Value > 100) //Bushido being HIGHER than 100 for bonus is intended
			bonus += 0.10;

		return 1.0 + bonus;
	}

	private static void BeginEvasion(Mobile m)
	{
		Timer t = (Timer)m_Table[m];

		if (t != null)
			t.Stop();

		TimeSpan duration = GetEvadeDuration(m);
		t = new InternalTimer(m, duration);

		BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.Evasion, 1060597, 1153810, duration, m));

		m_Table[m] = t;

		t.Start();
	}

	public static void EndEvasion(Mobile m)
	{
		Timer t = (Timer)m_Table[m];

		t?.Stop();

		m_Table.Remove(m);

		BuffInfo.RemoveBuff(m, BuffIcon.Evasion);

		OnEffectEnd(m, typeof(Evasion));
	}

	public override bool CheckCast()
	{
		return VerifyCast(Caster, true) && base.CheckCast();
	}

	public override void OnBeginCast()
	{
		base.OnBeginCast();

		Caster.FixedEffect(0x37C4, 10, 7, 4, 3);
	}

	public override void OnCast()
	{
		if (CheckSequence())
		{
			Caster.SendLocalizedMessage(1063120); // You feel that you might be able to deflect any attack!
			Caster.FixedParticles(0x376A, 1, 20, 0x7F5, 0x960, 3, EffectLayer.Waist);
			Caster.PlaySound(0x51B);

			OnCastSuccessful(Caster);

			BeginEvasion(Caster);

			Caster.BeginAction(typeof(Evasion));
			Timer.DelayCall(TimeSpan.FromSeconds(20.0), delegate { Caster.EndAction(typeof(Evasion)); });
		}

		FinishSequence();
	}

	private class InternalTimer : Timer
	{
		private readonly Mobile _mobile;
		public InternalTimer(Mobile m, TimeSpan delay)
			: base(delay)
		{
			_mobile = m;
			Priority = TimerPriority.TwoFiftyMs;
		}

		protected override void OnTick()
		{
			EndEvasion(_mobile);
			_mobile.SendLocalizedMessage(1063121); // You no longer feel that you could deflect any attack.
		}
	}
}
