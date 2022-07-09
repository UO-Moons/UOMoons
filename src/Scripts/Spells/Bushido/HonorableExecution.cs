using System;
using System.Collections;

namespace Server.Spells.Bushido;

public class HonorableExecution : SamuraiMove
{
	private static readonly Hashtable m_Table = new();

	public override int BaseMana => 0;
	public override double RequiredSkill => 25.0;
	public override TextDefinition AbilityMessage => new(1063122);// You better kill your enemy with your next hit or you'll be rather sorry...
	public static int GetSwingBonus(Mobile target)
	{
		return m_Table[target] is not HonorableExecutionInfo info ? 0 : info.SwingBonus;
	}

	public static bool IsUnderPenalty(Mobile target)
	{
		return m_Table[target] is HonorableExecutionInfo {Penalty: true};
	}

	public static void RemovePenalty(Mobile target)
	{
		if (m_Table[target] is not HonorableExecutionInfo info || (info.SwingBonus == 0 && !info.Penalty))
			return;

		info.Clear();

		info.Timer?.Stop();

		m_Table.Remove(target);
	}

	public override double GetDamageScalar(Mobile attacker, Mobile defender)
	{
		double bushido = attacker.Skills[SkillName.Bushido].Value;

		// TODO: 20 -> Perfection
		return 1.0 + bushido * 20 / 10000;
	}

	public override void OnHit(Mobile attacker, Mobile defender, int damage)
	{
		if (!Validate(attacker) || !CheckMana(attacker, true))
			return;

		ClearCurrentMove(attacker);

		if (m_Table[attacker] is HonorableExecutionInfo info)
		{
			info.Clear();

			if (info.Timer != null)
				info.Timer.Stop();
		}

		if (!defender.Alive)
		{
			attacker.FixedParticles(0x373A, 1, 17, 0x7E2, EffectLayer.Waist);

			double bushido = attacker.Skills[SkillName.Bushido].Value;

			attacker.Hits += 20 + (int)(bushido * bushido / 480.0);

			int swingBonus = Math.Max(1, (int)(bushido * bushido / 720.0));

			info = new HonorableExecutionInfo(attacker, swingBonus);
			info.Timer = Timer.DelayCall(TimeSpan.FromSeconds(20.0), new TimerStateCallback(EndEffect), info);

			BuffInfo.AddBuff(attacker, new BuffInfo(BuffIcon.HonorableExecution, 1060595, 1153807, TimeSpan.FromSeconds(20.0), attacker,
				$"{swingBonus}"));

			m_Table[attacker] = info;
		}
		else
		{
			ArrayList mods = new()
			{
				new ResistanceMod(ResistanceType.Physical, -40),
				new ResistanceMod(ResistanceType.Fire, -40),
				new ResistanceMod(ResistanceType.Cold, -40),
				new ResistanceMod(ResistanceType.Poison, -40),
				new ResistanceMod(ResistanceType.Energy, -40)
			};

			double resSpells = attacker.Skills[SkillName.MagicResist].Value;

			if (resSpells > 0.0)
				mods.Add(new DefaultSkillMod(SkillName.MagicResist, true, -resSpells));

			info = new HonorableExecutionInfo(attacker, mods);
			info.Timer = Timer.DelayCall(TimeSpan.FromSeconds(7.0), new TimerStateCallback(EndEffect), info);

			BuffInfo.AddBuff(attacker, new BuffInfo(BuffIcon.HonorableExecution, 1060595, 1153808, TimeSpan.FromSeconds(7.0), attacker,
				$"{resSpells}\t40\t40\t40\t40\t40"));

			m_Table[attacker] = info;
		}

		attacker.Delta(MobileDelta.WeaponDamage);
		CheckGain(attacker);
	}

	public void EndEffect(object state)
	{
		HonorableExecutionInfo info = (HonorableExecutionInfo)state;

		info.Mobile?.Delta(MobileDelta.WeaponDamage);

		RemovePenalty(info.Mobile);
	}

	private class HonorableExecutionInfo
	{
		public readonly Mobile Mobile;
		public readonly int SwingBonus;
		private readonly ArrayList _mods;
		public readonly bool Penalty;
		public Timer Timer;

		public HonorableExecutionInfo(Mobile from, ArrayList mods)
			: this(from, 0, mods, true)
		{
		}

		public HonorableExecutionInfo(Mobile from, int swingBonus, ArrayList mods = null, bool penalty = false)
		{
			Mobile = from;
			SwingBonus = swingBonus;
			_mods = mods;
			Penalty = penalty;

			Apply();
		}

		private void Apply()
		{
			if (_mods == null)
				return;

			for (int i = 0; i < _mods.Count; ++i)
			{
				object mod = _mods[i];

				switch (mod)
				{
					case ResistanceMod resistanceMod:
						Mobile.AddResistanceMod(resistanceMod);
						break;
					case SkillMod skillMod:
						Mobile.AddSkillMod(skillMod);
						break;
				}
			}
		}

		public void Clear()
		{
			if (_mods == null)
				return;

			for (int i = 0; i < _mods.Count; ++i)
			{
				object mod = _mods[i];

				switch (mod)
				{
					case ResistanceMod resistanceMod:
						Mobile.RemoveResistanceMod(resistanceMod);
						break;
					case SkillMod skillMod:
						Mobile.RemoveSkillMod(skillMod);
						break;
				}
			}
		}
	}
}
