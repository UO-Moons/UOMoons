using System;
using Server;
using Server.Targeting;
using System.Collections.Generic;
using Server.Network;

namespace Server.Spells.Mysticism;

public class SleepSpell : MysticSpell
{
	public override SpellCircle Circle => SpellCircle.Third;

	private static readonly SpellInfo m_Info = new(
		"Sleep", "In Zu",
		230,
		9022,
		Reagent.Nightshade,
		Reagent.SpidersSilk,
		Reagent.BlackPearl
	);

	public SleepSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
	{
	}

	public override void OnCast()
	{
		Caster.Target = new InternalTarget(this);
	}

	private void OnTarget(object o)
	{
		if (o is not Mobile target)
		{
			return;
		}

		if (target.Paralyzed)
		{
			Caster.SendLocalizedMessage(1080134); //Your target is already immobilized and cannot be slept.
		}
		else if (m_ImmunityList.Contains(target))
		{
			Caster.SendLocalizedMessage(1080135); //Your target cannot be put to sleep.
		}
		else if (CheckHSequence(target))
		{
			SpellHelper.CheckReflect((int)Circle, Caster, ref target);

			double duration = ((Caster.Skills[CastSkill].Value + Caster.Skills[DamageSkill].Value) / 20) + 2;
			duration -= GetResistSkill(target) / 10;

			if (duration <= 0 || StoneFormSpell.CheckImmunity(target))
			{
				Caster.SendLocalizedMessage(1080136); //Your target resists sleep.
				target.SendLocalizedMessage(1080137); //You resist sleep.
			}
			else
				DoSleep(Caster, target, TimeSpan.FromSeconds(duration));
		}

		FinishSequence();
	}

	public static readonly Dictionary<Mobile, SleepTimer> Table = new();
	private static readonly List<Mobile> m_ImmunityList = new();

	public static void DoSleep(Mobile caster, Mobile target, TimeSpan duration)
	{
		target.Combatant = null;
		target.SendSpeedControl(SpeedControlType.WalkSpeed);

		if (Table.ContainsKey(target))
			Table[target].Stop();

		Table[target] = new SleepTimer(target, duration);

		BuffInfo.AddBuff(target, new BuffInfo(BuffIcon.Sleep, 1080139, 1080140, duration, target));

		target.Delta(MobileDelta.WeaponDamage);
	}

	public static void AddToSleepTable(Mobile from, TimeSpan duration)
	{
		Table.Add(from, new SleepTimer(from, duration));
	}

	public static bool IsUnderSleepEffects(Mobile from)
	{
		return Table.ContainsKey(from);
	}

	public static void OnDamage(Mobile from)
	{
		if (Table.ContainsKey(from))
			EndSleep(from);
	}

	public class SleepTimer : Timer
	{
		private readonly Mobile m_Target;
		private readonly DateTime m_EndTime;

		public SleepTimer(Mobile target, TimeSpan duration)
			: base(TimeSpan.Zero, TimeSpan.FromSeconds(0.5))
		{
			m_EndTime = DateTime.UtcNow + duration;
			m_Target = target;
			Start();
		}

		protected override void OnTick()
		{
			if (m_EndTime < DateTime.UtcNow)
			{
				EndSleep(m_Target);
				Stop();
			}
			else
			{
				Effects.SendTargetParticles(m_Target, 0x3779, 1, 32, 0x13BA, EffectLayer.Head);
			}
		}
	}

	public static void EndSleep(Mobile target)
	{
		if (Table.ContainsKey(target))
		{
			target.SendSpeedControl(SpeedControlType.Disable);

			Table[target].Stop();
			Table.Remove(target);

			BuffInfo.RemoveBuff(target, BuffIcon.Sleep);

			double immduration = target.Skills[SkillName.MagicResist].Value / 10;

			m_ImmunityList.Add(target);
			Timer.DelayCall(TimeSpan.FromSeconds(immduration), new TimerStateCallback(RemoveImmunity_Callback), target);

			target.Delta(MobileDelta.WeaponDamage);
		}
	}

	private static void RemoveImmunity_Callback(object state)
	{
		Mobile m = (Mobile)state;

		if (m_ImmunityList.Contains(m))
			m_ImmunityList.Remove(m);
	}

	private class InternalTarget : Target
	{
		private SleepSpell Owner { get; }

		public InternalTarget(SleepSpell owner, bool allowland = false)
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
