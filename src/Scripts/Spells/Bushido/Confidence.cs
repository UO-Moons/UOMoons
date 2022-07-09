using System;
using System.Collections.Generic;

namespace Server.Spells.Bushido;

public class Confidence : SamuraiSpell
{
	private static readonly SpellInfo m_Info = new(
		"Confidence", null,
		-1,
		9002);

	private static readonly Dictionary<Mobile, Timer> m_Table = new();
	private static readonly Dictionary<Mobile, Timer> m_RegenTable = new();

	public Confidence(Mobile caster, Item scroll)
		: base(caster, scroll, m_Info)
	{
	}

	public override TimeSpan CastDelayBase => TimeSpan.FromSeconds(0.25);
	public override double RequiredSkill => 25.0;
	public override int RequiredMana => 10;

	public static bool IsConfident(Mobile m)
	{
		return m_Table.ContainsKey(m);
	}

	public static void BeginConfidence(Mobile m)
	{
		if (m_Table.TryGetValue(m, out Timer t))
			t.Stop();

		t = new InternalTimer(m);

		m_Table[m] = t;

		t.Start();

		double bushido = m.Skills[SkillName.Bushido].Value;
		BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.Confidence, 1060596, 1153809, TimeSpan.FromSeconds(4), m,
			$"{(int) (bushido / 12)}\t{(int) (bushido / 5)}\t100")); // Successful parry will heal for 1-~1_HEAL~ hit points and refresh for 1-~2_STAM~ stamina points.<br>+~3_HP~ hit point regeneration (4 second duration).
	}

	public static void EndConfidence(Mobile m)
	{
		if (!m_Table.ContainsKey(m))
			return;

		Timer t = m_Table[m];

		t.Stop();
		m_Table.Remove(m);

		OnEffectEnd(m, typeof(Confidence));

		BuffInfo.RemoveBuff(m, BuffIcon.Confidence);
		BuffInfo.RemoveBuff(m, BuffIcon.AnticipateHit);
	}

	public static bool IsRegenerating(Mobile m)
	{
		return m_RegenTable.ContainsKey(m);
	}

	public static void BeginRegenerating(Mobile m)
	{
		if (m_RegenTable.TryGetValue(m, out var t))
			t.Stop();

		t = new RegenTimer(m);

		m_RegenTable[m] = t;

		t.Start();
	}

	public static void StopRegenerating(Mobile m)
	{
		if (m_RegenTable.TryGetValue(m, out Timer t))
			t.Stop();

		if (m_RegenTable.ContainsKey(m))
			m_RegenTable.Remove(m);

		BuffInfo.RemoveBuff(m, BuffIcon.AnticipateHit);
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
			Caster.SendLocalizedMessage(1063115); // You exude confidence.

			Caster.FixedParticles(0x375A, 1, 17, 0x7DA, 0x960, 0x3, EffectLayer.Waist);
			Caster.PlaySound(0x51A);

			OnCastSuccessful(Caster);

			BeginConfidence(Caster);
			BeginRegenerating(Caster);
		}

		FinishSequence();
	}

	private class InternalTimer : Timer
	{
		private readonly Mobile _mobile;
		public InternalTimer(Mobile m)
			: base(TimeSpan.FromSeconds(15.0))
		{
			_mobile = m;
			Priority = TimerPriority.TwoFiftyMs;
		}

		protected override void OnTick()
		{
			EndConfidence(_mobile);
			_mobile.SendLocalizedMessage(1063116); // Your confidence wanes.
		}
	}

	private class RegenTimer : Timer
	{
		private readonly Mobile _mobile;
		private int _ticks;

		private int Hits { get; }

		public RegenTimer(Mobile m)
			: base(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0))
		{
			_mobile = m;
			Hits = 15 + (m.Skills.Bushido.Fixed * m.Skills.Bushido.Fixed / 57600);
			Priority = TimerPriority.TwoFiftyMs;
		}

		protected override void OnTick()
		{
			++_ticks;

			if (_ticks >= 5)
			{
				_mobile.Hits += (Hits - (Hits * 4 / 5));
				StopRegenerating(_mobile);
			}

			_mobile.Hits += (Hits / 5);
		}
	}
}
