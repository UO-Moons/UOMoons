using Server.Targeting;
using System;
using System.Collections.Generic;

namespace Server.Spells.Third;

public class BlessSpell : MagerySpell
{
	private static readonly SpellInfo m_Info = new(
		"Bless", "Rel Sanct",
		203,
		9061,
		Reagent.Garlic,
		Reagent.MandrakeRoot
	);

	public override SpellCircle Circle => SpellCircle.Third;
	public override TargetFlags SpellTargetFlags => TargetFlags.Beneficial;
	private static Dictionary<Mobile, InternalTimer> _table;

	public BlessSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
	{
	}

	public static bool IsBlessed(Mobile m)
	{
		return _table != null && _table.ContainsKey(m);
	}

	public static void AddBless(Mobile m, TimeSpan duration)
	{
		_table ??= new Dictionary<Mobile, InternalTimer>();

		if (_table.ContainsKey(m))
		{
			_table[m].Stop();
		}

		_table[m] = new InternalTimer(m, duration);
	}

	public static void RemoveBless(Mobile m, bool early = false)
	{
		if (_table != null && _table.ContainsKey(m))
		{
			_table[m].Stop();
			m.Delta(MobileDelta.Stat);

			_table.Remove(m);
		}
	}

	public override bool CheckCast()
	{
		if (Engines.ConPVP.DuelContext.CheckSuddenDeath(Caster))
		{
			Caster.SendMessage(0x22, "You cannot cast this spell when in sudden death.");
			return false;
		}

		return base.CheckCast();
	}

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

	public void Target(Mobile m)
	{
		if (!Caster.CanSee(m))
		{
			Caster.SendLocalizedMessage(500237); // Target can not be seen.
		}
		else if (CheckBSequence(m))
		{
			SpellHelper.Turn(Caster, m);

			int oldStr = SpellHelper.GetBuffOffset(m, StatType.Str);
			int oldDex = SpellHelper.GetBuffOffset(m, StatType.Dex);
			int oldInt = SpellHelper.GetBuffOffset(m, StatType.Int);

			int newStr = SpellHelper.GetOffset(Caster, m, StatType.Str, false, true);
			int newDex = SpellHelper.GetOffset(Caster, m, StatType.Dex, false, true);
			int newInt = SpellHelper.GetOffset(Caster, m, StatType.Int, false, true);

			if ((newStr < oldStr && newDex < oldDex && newInt < oldInt) ||
			    (newStr == 0 && newDex == 0 && newInt == 0))
			{
				DoHurtFizzle();
			}
			else
			{
				SpellHelper.AddStatBonus(Caster, m, false, StatType.Str);
				SpellHelper.AddStatBonus(Caster, m, true, StatType.Dex);
				SpellHelper.AddStatBonus(Caster, m, true, StatType.Int);

				int percentage = (int)(SpellHelper.GetOffsetScalar(Caster, m, false) * 100);
				TimeSpan length = SpellHelper.GetDuration(Caster, m);
				string args = $"{percentage}\t{percentage}\t{percentage}";
				BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.Bless, 1075847, 1075848, length, m, args));

				m.FixedParticles(0x373A, 10, 15, 5018, EffectLayer.Waist);
				m.PlaySound(0x1EA);

				AddBless(Caster, length + TimeSpan.FromMilliseconds(50));
			}
		}

		FinishSequence();
	}

	private class InternalTarget : Target
	{
		private readonly BlessSpell _owner;

		public InternalTarget(BlessSpell owner) : base(owner.SpellRange, false, TargetFlags.Beneficial)
		{
			_owner = owner;
		}

		protected override void OnTarget(Mobile from, object o)
		{
			if (o is Mobile mobile)
			{
				_owner.Target(mobile);
			}
		}

		protected override void OnTargetFinish(Mobile from)
		{
			_owner.FinishSequence();
		}
	}

	private class InternalTimer : Timer
	{
		public Mobile Mobile { get; }

		public InternalTimer(Mobile m, TimeSpan duration)
			: base(duration)
		{
			Mobile = m;
			Start();
		}

		protected override void OnTick()
		{
			RemoveBless(Mobile);
		}
	}
}
