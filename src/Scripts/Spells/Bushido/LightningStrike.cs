using System;
using Server.Mobiles;

namespace Server.Spells.Bushido;

public class LightningStrike : SamuraiMove
{
	public override int BaseMana => Core.SA ? 10 : 5;
	public override double RequiredSkill => 50.0;
	public override TextDefinition AbilityMessage => new(1063167);// You prepare to strike quickly.
	public override bool DelayedContext => true;
	public override bool ValidatesDuringHit => false;
	public override int GetAccuracyBonus(Mobile attacker)
	{
		return 50;
	}

	public override bool Validate(Mobile from)
	{
		bool isValid = base.Validate(from);
		if (isValid)
		{
			if (from is PlayerMobile thePlayer)
			{
				thePlayer.ExecutesLightningStrike = BaseMana;
			}
		}
		return isValid;
	}

	public override bool IgnoreArmor(Mobile attacker)
	{
		double bushido = attacker.Skills[SkillName.Bushido].Value;
		double criticalChance = bushido * bushido / 72000.0;
		return criticalChance >= Utility.RandomDouble();
	}

	public override bool OnBeforeSwing(Mobile attacker, Mobile defender)
	{
		/* no mana drain before actual hit */
		bool enoughMana = CheckMana(attacker, false);
		return Validate(attacker);
	}

	public override bool OnBeforeDamage(Mobile attacker, Mobile defender)
	{
		ClearCurrentMove(attacker);

		if (CheckMana(attacker, true))
		{
			attacker.SendLocalizedMessage(1063168); // You attack with lightning precision!
			defender.SendLocalizedMessage(1063169); // Your opponent's quick strike causes extra damage!
			defender.FixedParticles(0x3818, 1, 11, 0x13A8, 0, 0, EffectLayer.Waist);
			defender.PlaySound(0x51D);

			CheckGain(attacker);
			SetContext(attacker);
		}

		return base.OnBeforeDamage(attacker, defender);
	}

	public override void CheckGain(Mobile m)
	{
		// Lightning strike will gain to 120, albeit slow
		if (Core.SA && m.Skills[MoveSkill].Value >= 87.5)
		{
			if (0.25 > Utility.RandomDouble())
			{
				m.CheckSkill(MoveSkill, 0, m.Skills[MoveSkill].Cap);
			}
		}
		else
		{
			base.CheckGain(m);
		}
	}

	public override void OnUse(Mobile m)
	{
		base.OnUse(m);

		double bushido = m.Skills[SkillName.Bushido].Value;
		int criticalChance = (int)(bushido * bushido / 720.0);

		m.Delta(MobileDelta.WeaponDamage);

		BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.LightningStrike, 1060599, 1153811, $"50\t{criticalChance}"));
	}

	public override void OnClearMove(Mobile attacker)
	{
		if (attacker is PlayerMobile thePlayer)
		{
			thePlayer.ExecutesLightningStrike = 0;
		}

		attacker.Delta(MobileDelta.WeaponDamage);

		BuffInfo.RemoveBuff(attacker, BuffIcon.LightningStrike);
	}
}
