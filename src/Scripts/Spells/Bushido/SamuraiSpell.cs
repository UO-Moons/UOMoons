using System;
using Server.Mobiles;
using Server.Network;

namespace Server.Spells.Bushido;

public abstract class SamuraiSpell : Spell
{
	public SamuraiSpell(Mobile caster, Item scroll, SpellInfo info)
		: base(caster, scroll, info)
	{
	}

	public abstract double RequiredSkill { get; }
	public abstract int RequiredMana { get; }
	public override SkillName CastSkill => SkillName.Bushido;
	public override SkillName DamageSkill => SkillName.Bushido;
	public override bool ClearHandsOnCast => false;
	public override bool BlocksMovement => false;
	public override bool ShowHandMovement => false;
	//public override int CastDelayBase{ get{ return 1; } }
	public override double CastDelayFastScalar => 0;
	public override int CastRecoveryBase => 7;
	public static bool CheckExpansion(Mobile from)
	{
		if (!Core.SE)
			return false;

		if (from is not PlayerMobile)
			return true;

		return from.NetState != null && from.NetState.SupportsExpansion(Expansion.SE);
	}

	public static void OnEffectEnd(Mobile caster, Type type)
	{
		int spellId = SpellRegistry.GetRegistryNumber(type);

		if (spellId > 0)
			caster.Send(new ToggleSpecialAbility(spellId + 1, false));
	}

	public override bool CheckCast()
	{
		int mana = ScaleMana(RequiredMana);

		if (!base.CheckCast())
			return false;

		if (!CheckExpansion(Caster))
		{
			Caster.SendLocalizedMessage(1063456); // You must upgrade to Samurai Empire in order to use that ability.
			return false;
		}

		if (Caster.Skills[CastSkill].Value < RequiredSkill)
		{
			string args = $"{RequiredSkill:F1}\t{CastSkill.ToString()}\t ";
			Caster.SendLocalizedMessage(1063013, args); // You need at least ~1_SKILL_REQUIREMENT~ ~2_SKILL_NAME~ skill to use that ability.
			return false;
		}

		if (Caster.Mana < mana)
		{
			Caster.SendLocalizedMessage(1060174, mana.ToString()); // You must have at least ~1_MANA_REQUIREMENT~ Mana to use this ability.
			return false;
		}

		return true;
	}

	public override bool CheckFizzle()
	{
		int mana = ScaleMana(RequiredMana);

		if (Caster.Skills[CastSkill].Value < RequiredSkill)
		{
			Caster.SendLocalizedMessage(1070768, RequiredSkill.ToString("F1")); // You need ~1_SKILL_REQUIREMENT~ Bushido skill to perform that attack!
			return false;
		}

		if (Caster.Mana < mana)
		{
			Caster.SendLocalizedMessage(1060174, mana.ToString()); // You must have at least ~1_MANA_REQUIREMENT~ Mana to use this ability.
			return false;
		}

		if (!base.CheckFizzle())
			return false;

		Caster.Mana -= mana;

		return true;
	}

	public override void GetCastSkills(out double min, out double max)
	{
		min = RequiredSkill - 12.5;	//per 5 on friday, 2/16/07
		max = RequiredSkill + 37.5;
	}

	public override int GetMana()
	{
		return 0;
	}

	public virtual void OnCastSuccessful(Mobile caster)
	{
		if (Evasion.IsEvading(caster))
			Evasion.EndEvasion(caster);

		if (Confidence.IsConfident(caster))
			Confidence.EndConfidence(caster);

		if (CounterAttack.IsCountering(caster))
			CounterAttack.StopCountering(caster);

		int spellId = SpellRegistry.GetRegistryNumber(this);

		if (spellId > 0)
			caster.Send(new ToggleSpecialAbility(spellId + 1, true));
	}
}
