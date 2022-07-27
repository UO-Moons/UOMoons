using System.Collections;

namespace Server.Spells.Fifth;

public class MagicReflectSpell : MagerySpell
{
	private static readonly SpellInfo m_Info = new(
		"Magic Reflection", "In Jux Sanct",
		242,
		9012,
		Reagent.Garlic,
		Reagent.MandrakeRoot,
		Reagent.SpidersSilk
	);

	public override SpellCircle Circle => SpellCircle.Fifth;
	public override bool RequireTarget => false;

	public MagicReflectSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
	{
	}

	public override bool CheckCast()
	{
		if (Core.AOS)
			return true;

		if (Caster.MagicDamageAbsorb > 0)
		{
			Caster.SendLocalizedMessage(1005559); // This spell is already in effect.
			return false;
		}

		if (Caster.CanBeginAction(typeof(DefensiveSpell)))
			return true;

		Caster.SendLocalizedMessage(1005385); // The spell will not adhere to you at this time.
		return false;

	}

	private static readonly Hashtable m_Table = new();

	public override void OnCast()
	{
		if (Core.AOS)
		{
			/* The magic reflection spell decreases the caster's physical resistance, while increasing the caster's elemental resistances.
			 * Physical decrease = 25 - (Inscription/20).
			 * Elemental resistance = +10 (-20 physical, +10 elemental at GM Inscription)
			 * The magic reflection spell has an indefinite duration, becoming active when cast, and deactivated when re-cast.
			 * Reactive Armor, Protection, and Magic Reflection will stay oneven after logging out, even after dyinguntil you turn them off by casting them again.
			 */

			if (CheckSequence())
			{
				ResistanceMod[] mods = (ResistanceMod[])m_Table[Caster];

				if (mods == null)
				{
					Caster.PlaySound(0x1E9);
					Caster.FixedParticles(0x375A, 10, 15, 5037, EffectLayer.Waist);

					int physiMod = -25 + (int)(Caster.Skills[SkillName.Inscribe].Value / 20);
					const int otherMod = 10;

					mods = new ResistanceMod[]
					{
						new( ResistanceType.Physical, physiMod ),
						new( ResistanceType.Fire,     otherMod ),
						new( ResistanceType.Cold,     otherMod ),
						new( ResistanceType.Poison,   otherMod ),
						new( ResistanceType.Energy,   otherMod )
					};

					m_Table[Caster] = mods;

					for (int i = 0; i < mods.Length; ++i)
						Caster.AddResistanceMod(mods[i]);

					string buffFormat = string.Format("{0}\t+{1}\t+{1}\t+{1}\t+{1}", physiMod, otherMod);

					BuffInfo.AddBuff(Caster, new BuffInfo(BuffIcon.MagicReflection, 1075817, buffFormat, true));
				}
				else
				{
					Caster.PlaySound(0x1ED);
					Caster.FixedParticles(0x375A, 10, 15, 5037, EffectLayer.Waist);

					m_Table.Remove(Caster);

					for (int i = 0; i < mods.Length; ++i)
						Caster.RemoveResistanceMod(mods[i]);

					BuffInfo.RemoveBuff(Caster, BuffIcon.MagicReflection);
				}
			}

			FinishSequence();
		}
		else
		{
			if (Caster.MagicDamageAbsorb > 0)
			{
				Caster.SendLocalizedMessage(1005559); // This spell is already in effect.
			}
			else if (!Caster.CanBeginAction(typeof(DefensiveSpell)))
			{
				Caster.SendLocalizedMessage(1005385); // The spell will not adhere to you at this time.
			}
			else if (CheckSequence())
			{
				if (Caster.BeginAction(typeof(DefensiveSpell)))
				{
					int value = (int)(Caster.Skills[SkillName.Magery].Value + Caster.Skills[SkillName.Inscribe].Value);
					value = (int)(8 + value / 200.0 * 7.0);//absorb from 8 to 15 "circles"

					Caster.MagicDamageAbsorb = value;

					Caster.FixedParticles(0x375A, 10, 15, 5037, EffectLayer.Waist);
					Caster.PlaySound(0x1E9);
				}
				else
				{
					Caster.SendLocalizedMessage(1005385); // The spell will not adhere to you at this time.
				}
			}

			FinishSequence();
		}
	}

	public static void EndReflect(Mobile m)
	{
		if (!m_Table.Contains(m))
			return;

		ResistanceMod[] mods = (ResistanceMod[])m_Table[m];

		if (mods != null)
		{
			for (int i = 0; i < mods.Length; ++i)
				m.RemoveResistanceMod(mods[i]);
		}

		m_Table.Remove(m);
		BuffInfo.RemoveBuff(m, BuffIcon.MagicReflection);
	}

	public static bool HasReflect(Mobile m)
	{
		return m_Table.ContainsKey(m);
	}
}
