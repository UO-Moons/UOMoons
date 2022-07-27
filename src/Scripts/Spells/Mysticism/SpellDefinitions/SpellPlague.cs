using System;
using Server.Mobiles;
using Server.Targeting;
using System.Collections.Generic;
using System.Linq;

namespace Server.Spells.Mysticism;

public class SpellPlagueSpell : MysticSpell
{
	private static readonly SpellInfo m_Info = new(
		"Spell Plague", "Vas Rel Jux Ort",
		230,
		9022,
		Reagent.DaemonBone,
		Reagent.DragonBlood,
		Reagent.Nightshade,
		Reagent.SulfurousAsh
	);

	public override SpellCircle Circle => SpellCircle.Seventh;

	public SpellPlagueSpell(Mobile caster, Item scroll)
		: base(caster, scroll, m_Info)
	{
	}

	public override void OnCast()
	{
		Caster.Target = new InternalTarget(this);
	}

	private void OnTarget(object o)
	{
		if (o is not Mobile m)
			return;

		if (!(m is PlayerMobile || m is BaseCreature))
		{
			Caster.SendLocalizedMessage(1080194); // Your target cannot be affected by spell plague.
		}
		else if (CheckResisted(m))
		{
			m.SendLocalizedMessage(1080199); //You resist spell plague.
			Caster.SendLocalizedMessage(1080200); //Your target resists spell plague.
		}
		else if (CheckHSequence(m))
		{
			SpellHelper.CheckReflect((int)Circle, Caster, ref m);

			SpellHelper.Turn(Caster, m);

			Caster.PlaySound(0x658);

			m.FixedParticles(0x375A, 1, 17, 9919, 1161, 7, EffectLayer.Waist);
			m.FixedParticles(0x3728, 1, 13, 9502, 1161, 7, (EffectLayer)255);

			if (!m_Table.ContainsKey(m) || m_Table[m] == null)
				m_Table.Add(m, new List<SpellPlagueTimer>());

			m_Table[m].Add(new SpellPlagueTimer(Caster, m, TimeSpan.FromSeconds(8)));

			BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.SpellPlague, 1031690, 1080167, TimeSpan.FromSeconds(8), m));

			DoExplosion(m, Caster, 1);
		}

		FinishSequence();
	}

	private static readonly Dictionary<Mobile, List<SpellPlagueTimer>> m_Table = new();

	public static bool HasSpellPlague(Mobile from)
	{
		return m_Table.Where(kvp => kvp.Value != null).SelectMany(kvp => kvp.Value).Any(timer => timer.Caster == from);
	}

	public static void OnMobileDamaged(Mobile from)
	{
		if (!m_Table.ContainsKey(from) || m_Table[from].Count <= 0 ||
		    m_Table[from][0].NextUse >= DateTime.UtcNow)
			return;

		int amount = m_Table[from][0].Amount;
		bool doExplosion = false;
		double mod = from.Skills[SkillName.MagicResist].Value >= 70.0 ? (from.Skills[SkillName.MagicResist].Value / 1000 * 3) : 0.0;

		if (mod < 0)
			mod = .01;

		if (amount == 0 && .90 - mod > Utility.RandomDouble())
			doExplosion = true;
		else if (amount == 1 && .60 - mod > Utility.RandomDouble())
			doExplosion = true;
		else if (amount == 2 && .30 - mod > Utility.RandomDouble())
			doExplosion = true;

		if (!doExplosion)
			return;

		SpellPlagueTimer timer = m_Table[from][0];

		timer.NextUse = DateTime.UtcNow + TimeSpan.FromSeconds(1.5);

		DoExplosion(from, timer.Caster, amount);
		timer.Amount++;
	}

	private static void DoExplosion(Mobile from, Mobile caster, int amount)
	{
		double prim = caster.Skills[SkillName.Mysticism].Value;
		double sec = caster.Skills[SkillName.Imbuing].Value;

		if (caster.Skills[SkillName.Focus].Value > sec)
			sec = caster.Skills[SkillName.Focus].Value;

		int damage = (int)((prim + sec) / 12) + Utility.RandomMinMax(1, 6);

		if (amount > 1)
			damage /= amount;

		from.PlaySound(0x658);

		from.FixedParticles(0x375A, 1, 17, 9919, 1161, 7, EffectLayer.Waist);
		from.FixedParticles(0x3728, 1, 13, 9502, 1161, 7, (EffectLayer)255);

		int sdiBonus = SpellHelper.GetSpellDamageBonus(caster, from, SkillName.Mysticism, from is PlayerMobile);

		damage *= 100 + sdiBonus;
		damage /= 100;

		SpellHelper.Damage(null, TimeSpan.Zero, from, caster, damage, 0, 0, 0, 0, 0, DfAlgorithm.Standard, 100);
	}

	public static void RemoveFromList(Mobile from)
	{
		if (!m_Table.ContainsKey(from) || m_Table[from].Count <= 0)
			return;

		Mobile caster = m_Table[from][0].Caster;

		m_Table[from].Remove(m_Table[from][0]);

		if (m_Table[from].Count == 0)
		{
			m_Table.Remove(from);
			BuffInfo.RemoveBuff(from, BuffIcon.SpellPlague);
		}

		if (m_Table.SelectMany(kvp => kvp.Value).Any(ttimer => ttimer.Caster == caster))
		{
			return;
		}

		BuffInfo.RemoveBuff(caster, BuffIcon.SpellPlague);
	}

	private class InternalTarget : Target
	{
		private SpellPlagueSpell Owner { get; }

		public InternalTarget(SpellPlagueSpell owner, bool allowland = false)
			: base(12, allowland, TargetFlags.Harmful)
		{
			Owner = owner;
		}

		protected override void OnTarget(Mobile from, object o)
		{
			if (o == null)
				return;

			if (!from.CanSee(o))
				from.SendLocalizedMessage(500237); // Target can not be seen.
			else
			{
				SpellHelper.Turn(from, o);
				Owner.OnTarget(o);
			}
		}

		protected override void OnTargetFinish(Mobile from)
		{
			Owner.FinishSequence();
		}
	}
}

public class SpellPlagueTimer : Timer
{
	private readonly Mobile m_Owner;
	private int m_Amount;

	public Mobile Caster { get; }

	public int Amount
	{
		get => m_Amount;
		set
		{
			m_Amount = value;

			if (m_Amount >= 3)
				EndTimer();
		}
	}

	public DateTime NextUse { get; set; }

	public SpellPlagueTimer(Mobile caster, Mobile owner, TimeSpan duration)
		: base(duration)
	{
		Caster = caster;
		m_Owner = owner;
		m_Amount = 0;
		NextUse = DateTime.UtcNow;
		Start();
	}

	protected override void OnTick()
	{
		EndTimer();
	}

	private void EndTimer()
	{
		Stop();
		SpellPlagueSpell.RemoveFromList(m_Owner);
	}
}
