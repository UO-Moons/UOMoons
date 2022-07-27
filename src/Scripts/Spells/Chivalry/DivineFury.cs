using System;
using System.Collections.Generic;

namespace Server.Spells.Chivalry;

public class DivineFurySpell : PaladinSpell
{
	private static readonly SpellInfo m_Info = new(
		"Divine Fury", "Divinum Furis",
		-1,
		9002);

	private static readonly Dictionary<Mobile, Timer> m_Table = new();

	public DivineFurySpell(Mobile caster, Item scroll)
		: base(caster, scroll, m_Info)
	{
	}

	public override TimeSpan CastDelayBase => TimeSpan.FromSeconds(1.0);
	public override double RequiredSkill => 25.0;
	public override int RequiredMana => 15;
	public override int RequiredTithing => 10;
	public override int MantraNumber => 1060722;// Divinum Furis
	public override bool BlocksMovement => false;

	public static bool UnderEffect(Mobile m)
	{
		return m_Table.ContainsKey(m);
	}

	public override void OnCast()
	{
		if (CheckSequence())
		{
			Caster.PlaySound(0x20F);
			Caster.PlaySound(Caster.Female ? 0x338 : 0x44A);
			Caster.FixedParticles(0x376A, 1, 31, 9961, 1160, 0, EffectLayer.Waist);
			Caster.FixedParticles(0x37C4, 1, 31, 9502, 43, 2, EffectLayer.Waist);

			Caster.Stam = Caster.StamMax;

			if (m_Table.ContainsKey(Caster))
			{
				var t = m_Table[Caster];

				t?.Stop();
			}

			int delay = ComputePowerValue(10);

			switch (delay)
			{
				case < 7:
					delay = 7;
					break;
				case > 24:
					delay = 24;
					break;
			}

			m_Table[Caster] = Timer.DelayCall(TimeSpan.FromSeconds(delay), new TimerStateCallback(Expire_Callback), Caster);
			Caster.Delta(MobileDelta.WeaponDamage);

			string args =
				$"{GetAttackBonus(Caster)}\t{GetDamageBonus(Caster)}\t{GetWeaponSpeedBonus(Caster)}\t{GetDefendMalus(Caster)}";

			BuffInfo.AddBuff(Caster, new BuffInfo(BuffIcon.DivineFury, 1060589, 1150218, TimeSpan.FromSeconds(delay), Caster, args));
			// ~1_HCI~% hit chance<br> ~2_DI~% damage<br>~3_SSI~% swing speed increase<br>-~4_DCI~% defense chance
		}

		FinishSequence();
	}

	public static int GetDamageBonus(Mobile m)
	{
		if (m_Table.ContainsKey(m))
		{
			return m.Skills[SkillName.Chivalry].Value >= 120.0 && m.Karma >= 10000 ? 20 : 10;
		}

		return 0;
	}

	public static int GetWeaponSpeedBonus(Mobile m)
	{
		if (m_Table.ContainsKey(m))
		{
			return m.Skills[SkillName.Chivalry].Value >= 120.0 && m.Karma >= 10000 ? 15 : 10;
		}

		return 0;
	}

	public static int GetAttackBonus(Mobile m)
	{
		if (m_Table.ContainsKey(m))
		{
			return m.Skills[SkillName.Chivalry].Value >= 120.0 && m.Karma >= 10000 ? 15 : 10;
		}

		return 0;
	}

	public static int GetDefendMalus(Mobile m)
	{
		if (m_Table.ContainsKey(m))
		{
			return m.Skills[SkillName.Chivalry].Value >= 120.0 && m.Karma >= 10000 ? 10 : 20;
		}

		return 0;
	}

	private static void Expire_Callback(object state)
	{
		Mobile m = (Mobile)state;

		if (m_Table.ContainsKey(m))
			m_Table.Remove(m);

		m.Delta(MobileDelta.WeaponDamage);
		m.PlaySound(0xF8);
	}
}
