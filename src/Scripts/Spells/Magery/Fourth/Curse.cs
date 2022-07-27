using Server.Spells.First;
using Server.Targeting;
using System;
using System.Collections.Generic;

namespace Server.Spells.Fourth;

public class CurseSpell : MagerySpell
{
	private static readonly SpellInfo m_Info = new(
		"Curse", "Des Sanct",
		227,
		9031,
		Reagent.Nightshade,
		Reagent.Garlic,
		Reagent.SulfurousAsh
	);

	public override SpellCircle Circle => SpellCircle.Fourth;
	public override TargetFlags SpellTargetFlags => TargetFlags.Harmful;

	public CurseSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
	{
	}
	private static readonly Dictionary<Mobile, Timer> m_UnderEffect = new();

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

	public static void AddEffect(Mobile m, TimeSpan duration, int strOffset, int dexOffset, int intOffset)
	{
		if (m == null)
		{
			return;
		}

		if (m_UnderEffect.ContainsKey(m))
		{
			m_UnderEffect[m].Stop();
			m_UnderEffect[m] = null;
		}

		// my spell is stronger, so lets remove the lesser spell
		if (WeakenSpell.IsUnderEffects(m) && SpellHelper.GetCurseOffset(m, StatType.Str) <= strOffset)
		{
			WeakenSpell.RemoveEffects(m, false);
		}

		if (ClumsySpell.IsUnderEffects(m) && SpellHelper.GetCurseOffset(m, StatType.Dex) <= dexOffset)
		{
			ClumsySpell.RemoveEffects(m, false);
		}

		if (FeeblemindSpell.IsUnderEffects(m) && SpellHelper.GetCurseOffset(m, StatType.Int) <= intOffset)
		{
			FeeblemindSpell.RemoveEffects(m, false);
		}

		m_UnderEffect[m] = Timer.DelayCall(duration, RemoveEffect, m); //= new CurseTimer(m, duration, strOffset, dexOffset, intOffset);
		m.UpdateResistances();
	}

	public static void RemoveEffect(Mobile m)
	{
		if (!WeakenSpell.IsUnderEffects(m))
		{
			m.RemoveStatMod("[Magic] Str Curse");
		}

		if (!ClumsySpell.IsUnderEffects(m))
		{
			m.RemoveStatMod("[Magic] Dex Curse");
		}

		if (!FeeblemindSpell.IsUnderEffects(m))
		{
			m.RemoveStatMod("[Magic] Int Curse");
		}

		BuffInfo.RemoveBuff(m, BuffIcon.Curse);

		if (m_UnderEffect.ContainsKey(m))
		{
			Timer t = m_UnderEffect[m];

			t?.Stop();

			m_UnderEffect.Remove(m);
		}

		m.UpdateResistances();
	}

	public static bool UnderEffect(Mobile m)
	{
		return m_UnderEffect.ContainsKey(m);
	}

	private void Target(Mobile m)
	{
		if (!Caster.CanSee(m))
		{
			Caster.SendLocalizedMessage(500237); // Target can not be seen.
		}
		else if (CheckHSequence(m))
		{
			SpellHelper.Turn(Caster, m);

			SpellHelper.CheckReflect((int)Circle, Caster, ref m);

			if (DoCurse(Caster, m, false))
			{
				HarmfulSpell(m);
			}
			else
			{
				DoHurtFizzle();
			}
		}

		FinishSequence();
	}

	public static bool DoCurse(Mobile caster, Mobile m, bool masscurse)
	{

		int oldStr = SpellHelper.GetCurseOffset(m, StatType.Str);
		int oldDex = SpellHelper.GetCurseOffset(m, StatType.Dex);
		int oldInt = SpellHelper.GetCurseOffset(m, StatType.Int);

		int newStr = SpellHelper.GetOffset(caster, m, StatType.Str, true, true);
		int newDex = SpellHelper.GetOffset(caster, m, StatType.Dex, true, true);
		int newInt = SpellHelper.GetOffset(caster, m, StatType.Int, true, true);

		if ((-newStr > oldStr && -newDex > oldDex && -newInt > oldInt) ||
		    (newStr == 0 && newDex == 0 && newInt == 0))
		{
			return false;
		}

		SpellHelper.AddStatCurse(caster, m, StatType.Str, false);
		SpellHelper.AddStatCurse(caster, m, StatType.Dex);
		SpellHelper.AddStatCurse(caster, m, StatType.Int);

		int percentage = (int)(SpellHelper.GetOffsetScalar(caster, m, true) * 100);
		TimeSpan length = SpellHelper.GetDuration(caster, m);
		string args;

		if (masscurse)
		{
			args = string.Format("{0}\t{0}\t{0}", percentage);
			BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.MassCurse, 1075839, length, m, args));
		}
		else
		{
			args = $"{percentage}\t{percentage}\t{percentage}\t{10}\t{10}\t{10}\t{10}";
			BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.Curse, 1075835, 1075836, length, m, args));
		}

		AddEffect(m, SpellHelper.GetDuration(caster, m), oldStr, oldDex, oldInt);

		m.Spell?.OnCasterHurt();

		m.Paralyzed = false;

		m.FixedParticles(0x374A, 10, 15, 5028, EffectLayer.Waist);
		m.PlaySound(0x1E1);

		return true;
	}

	private class InternalTarget : Target
	{
		private readonly CurseSpell _owner;

		public InternalTarget(CurseSpell owner) : base(owner.SpellRange, false, TargetFlags.Harmful)
		{
			_owner = owner;
		}

		protected override void OnTarget(Mobile from, object o)
		{
			if (o is Mobile mobile)
				_owner.Target(mobile);
		}

		protected override void OnTargetFinish(Mobile from)
		{
			_owner.FinishSequence();
		}
	}
}
