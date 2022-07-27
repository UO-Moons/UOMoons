using System;
using System.Collections.Generic;
using System.Linq;
using Server.Network;

namespace Server.Spells.Spellweaving;

public class ThunderstormSpell : ArcanistSpell
{
	private static readonly SpellInfo m_Info = new(
		"Thunderstorm", "Erelonia",
		-1);

	private static readonly Dictionary<Mobile, Timer> m_Table = new();

	public override DamageType SpellDamageType => DamageType.SpellAOE;

	public ThunderstormSpell(Mobile caster, Item scroll)
		: base(caster, scroll, m_Info)
	{
	}

	public override TimeSpan CastDelayBase => TimeSpan.FromSeconds(1.5);
	public override double RequiredSkill => 10.0;
	public override int RequiredMana => 32;
	public static int GetCastRecoveryMalus(Mobile m)
	{
		return m_Table.ContainsKey(m) ? 6 : 0;
	}

	private static void DoExpire(Mobile m)
	{
		if (!m_Table.TryGetValue(m, out var t))
			return;

		t.Stop();
		m_Table.Remove(m);

		BuffInfo.RemoveBuff(m, BuffIcon.Thunderstorm);
		m.Delta(MobileDelta.WeaponDamage);
	}

	public override void OnCast()
	{
		if (CheckSequence())
		{
			Caster.PlaySound(0x5CE);

			double skill = Caster.Skills[SkillName.Spellweaving].Value;

			int damage = Math.Max(11, 10 + (int)(skill / 24)) + FocusLevel;

			int sdiBonus = AosAttributes.GetValue(Caster, AosAttribute.SpellDamage);

			int pvmDamage = damage * (100 + sdiBonus);
			pvmDamage /= 100;

			if (sdiBonus > 15)
				sdiBonus = 15;

			int pvpDamage = damage * (100 + sdiBonus);
			pvpDamage /= 100;

			TimeSpan duration = TimeSpan.FromSeconds(5 + FocusLevel);

			foreach (var m in AcquireIndirectTargets(Caster.Location, 3 + FocusLevel).OfType<Mobile>())
			{
				Caster.DoHarmful(m);

				SpellHelper.Damage(this, m, m.Player && Caster.Player ? pvpDamage : pvmDamage, 0, 0, 0, 0, 100);
				Effects.SendPacket(m.Location, m.Map, new HuedEffect(EffectType.FixedFrom, m.Serial, Serial.Zero, 0x1B6C, m.Location, m.Location, 10, 10, false, false, 0x480, 4));

				if (m.Spell is not Spell oldSpell || oldSpell == m.Spell)
					continue;

				if (CheckResisted(m))
					continue;

				m_Table[m] = Timer.DelayCall(duration, DoExpire, m);

				BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.Thunderstorm, 1075800, duration, m, GetCastRecoveryMalus(m)));
				m.Delta(MobileDelta.WeaponDamage);
			}
		}

		FinishSequence();
	}
}
