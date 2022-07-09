using System.Collections;

namespace Server.Spells.First;

public class ReactiveArmorSpell : MagerySpell
{
	private static readonly SpellInfo m_Info = new(
		"Reactive Armor", "Flam Sanct",
		236,
		9011,
		Reagent.Garlic,
		Reagent.SpidersSilk,
		Reagent.SulfurousAsh
	);

	public override SpellCircle Circle => SpellCircle.First;
	public override bool RequireTarget => false;

	public ReactiveArmorSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
	{
	}

	public static void EndArmor(Mobile m)
	{
		if (m_Table.Contains(m))
		{
			ResistanceMod[] mods = (ResistanceMod[])m_Table[m];

			if (mods != null)
			{
				for (var i = 0; i < mods.Length; ++i)
				{
					m.RemoveResistanceMod(mods[i]);
				}
			}

			m_Table.Remove(m);
			BuffInfo.RemoveBuff(m, BuffIcon.ReactiveArmor);
		}
	}

	public override bool CheckCast()
	{
		if (Core.AOS)
			return true;

		if (Caster.MeleeDamageAbsorb > 0)
		{
			Caster.SendLocalizedMessage(1005559); // This spell is already in effect.
			return false;
		}

		if (!Caster.CanBeginAction(typeof(DefensiveSpell)))
		{
			Caster.SendLocalizedMessage(1005385); // The spell will not adhere to you at this time.
			return false;
		}

		return true;
	}

	private static readonly Hashtable m_Table = new();

	public override void OnCast()
	{
		if (Core.AOS)
		{
			/* The reactive armor spell increases the caster's physical resistance, while lowering the caster's elemental resistances.
			 * 15 + (Inscription/20) Physcial bonus
			 * -5 Elemental
			 * The reactive armor spell has an indefinite duration, becoming active when cast, and deactivated when re-cast.
			 * Reactive Armor, Protection, and Magic Reflection will stay oneven after logging out, even after dyinguntil you turn them off by casting them again.
			 * (+20 physical -5 elemental at 100 Inscription)
			 */

			if (CheckSequence())
			{
				Mobile targ = Caster;

				ResistanceMod[] mods = (ResistanceMod[])m_Table[targ];

				if (mods == null)
				{
					targ.PlaySound(0x1E9);
					targ.FixedParticles(0x376A, 9, 32, 5008, EffectLayer.Waist);

					mods = new ResistanceMod[]
					{
						new( ResistanceType.Physical, 15 + (int)(targ.Skills[SkillName.Inscribe].Value / 20) ),
						new( ResistanceType.Fire, -5 ),
						new( ResistanceType.Cold, -5 ),
						new( ResistanceType.Poison, -5 ),
						new( ResistanceType.Energy, -5 )
					};

					m_Table[targ] = mods;

					for (int i = 0; i < mods.Length; ++i)
						targ.AddResistanceMod(mods[i]);

					int physresist = 15 + (int)(targ.Skills[SkillName.Inscribe].Value / 20);
					string args = $"{physresist}\t{5}\t{5}\t{5}\t{5}";

					BuffInfo.AddBuff(Caster, new BuffInfo(BuffIcon.ReactiveArmor, 1075812, 1075813, args));
				}
				else
				{
					targ.PlaySound(0x1ED);
					targ.FixedParticles(0x376A, 9, 32, 5008, EffectLayer.Waist);

					m_Table.Remove(targ);

					for (var i = 0; i < mods.Length; ++i)
						targ.RemoveResistanceMod(mods[i]);

					BuffInfo.RemoveBuff(Caster, BuffIcon.ReactiveArmor);
				}
			}

			FinishSequence();
		}
		else
		{
			if (Caster.MeleeDamageAbsorb > 0)
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
					int value = (int)(Caster.Skills[SkillName.Magery].Value + Caster.Skills[SkillName.Meditation].Value + Caster.Skills[SkillName.Inscribe].Value);
					value /= 3;

					value = value switch
					{
						< 0 => 1,
						> 75 => 75,
						_ => value
					};

					Caster.MeleeDamageAbsorb = value;

					Caster.FixedParticles(0x376A, 9, 32, 5008, EffectLayer.Waist);
					Caster.PlaySound(0x1F2);
				}
				else
				{
					Caster.SendLocalizedMessage(1005385); // The spell will not adhere to you at this time.
				}
			}

			FinishSequence();
		}
	}

	public static bool HasArmor(Mobile m)
	{
		return m_Table.ContainsKey(m);
	}
}
