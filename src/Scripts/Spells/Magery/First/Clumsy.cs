using Server.Targeting;
using System;
using System.Collections.Generic;

namespace Server.Spells.First;

public class ClumsySpell : MagerySpell
{
	private static readonly SpellInfo m_Info = new(
		"Clumsy", "Uus Jux",
		212,
		9031,
		Reagent.Bloodmoss,
		Reagent.Nightshade
	);

	public ClumsySpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
	{
	}
	public static readonly Dictionary<Mobile, Timer> Table = new();

	public static bool IsUnderEffects(Mobile m)
	{
		return Table.ContainsKey(m);
	}

	public static void RemoveEffects(Mobile m, bool removeMod = true)
	{
		if (Table.ContainsKey(m))
		{
			Timer t = Table[m];

			if (t is {Running: true})
			{
				t.Stop();
			}

			BuffInfo.RemoveBuff(m, BuffIcon.Clumsy);

			if (removeMod)
			{
				m.RemoveStatMod("[Magic] Dex Curse");
			}

			Table.Remove(m);
		}
	}

	public override SpellCircle Circle => SpellCircle.First;

	public override TargetFlags SpellTargetFlags => TargetFlags.Harmful;

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
			int oldOffset = SpellHelper.GetCurseOffset(m, StatType.Dex);
			int newOffset = SpellHelper.GetOffset(Caster, m, StatType.Dex, true, false);
			if (-newOffset > oldOffset || newOffset == 0)
			{
				DoHurtFizzle();
			}
			else
			{
				if (m.Spell != null)
					m.Spell.OnCasterHurt();

				m.Paralyzed = false;

				m.FixedParticles(0x3779, 10, 15, 5002, EffectLayer.Head);
				m.PlaySound(0x1DF);
				HarmfulSpell(m);

				if (-newOffset < oldOffset)
				{
					SpellHelper.AddStatCurse(Caster, m, StatType.Dex, false, newOffset);

					int percentage = (int)(SpellHelper.GetOffsetScalar(Caster, m, true) * 100);
					TimeSpan length = SpellHelper.GetDuration(Caster, m);
					BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.Clumsy, 1075831, length, m, percentage.ToString()));

					if (Table.ContainsKey(m))
					{
						Table[m].Stop();
					}

					Table[m] = Timer.DelayCall(length, () =>
					{
						RemoveEffects(m);
					});
				}
			}
		}

		FinishSequence();
	}

	private class InternalTarget : Target
	{
		private readonly ClumsySpell _owner;

		public InternalTarget(ClumsySpell owner) : base(owner.SpellRange, false, TargetFlags.Harmful)
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
}
