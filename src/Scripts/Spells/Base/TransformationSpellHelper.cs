using System;
using System.Collections.Generic;
using Server.Items;
using Server.Spells.Fifth;
using Server.Spells.Ninjitsu;
using Server.Spells.Seventh;

namespace Server.Spells;

public class TransformationSpellHelper
{
	#region Context Stuff
	private static readonly Dictionary<Mobile, TransformContext> m_Table = new();

	public static void AddContext(Mobile m, TransformContext context)
	{
		m_Table[m] = context;
	}

	public static void RemoveContext(Mobile m, bool resetGraphics)
	{
		TransformContext context = GetContext(m);

		if (context != null)
			RemoveContext(m, context, resetGraphics);
	}

	public static void RemoveContext(Mobile m, TransformContext context, bool resetGraphics)
	{
		if (!m_Table.ContainsKey(m)) return;
		m_Table.Remove(m);

		List<ResistanceMod> mods = context.Mods;

		for (int i = 0; i < mods.Count; ++i)
			m.RemoveResistanceMod(mods[i]);

		if (resetGraphics)
		{
			m.HueMod = -1;
			m.BodyMod = 0;
		}

		context.Timer.Stop();
		context.Spell.RemoveEffect(m);
	}

	public static TransformContext GetContext(Mobile m)
	{
		m_Table.TryGetValue(m, out var context);

		return context;
	}

	public static bool UnderTransformation(Mobile m)
	{
		return GetContext(m) != null;
	}

	public static bool UnderTransformation(Mobile m, Type type)
	{
		TransformContext context = GetContext(m);

		return context != null && context.Type == type;
	}

	public static void CheckCastSkill(Mobile m, TransformContext context)
	{
		if (context.Spell is Spell spell)
		{
			spell.GetCastSkills(out double min, out _);

			if (m.Skills[spell.CastSkill].Value < min)
			{
				RemoveContext(m, context, true);
			}
		}
	}
	#endregion

	public static bool CheckCast(Mobile caster, Spell spell)
	{
		if (Factions.Sigil.ExistsOn(caster))
		{
			caster.SendLocalizedMessage(1061632); // You can't do that while carrying the sigil.
			return false;
		}

		if (!caster.CanBeginAction(typeof(PolymorphSpell)))
		{
			caster.SendLocalizedMessage(1061628); // You can't do that while polymorphed.
			return false;
		}

		if (AnimalForm.UnderTransformation(caster))
		{
			caster.SendLocalizedMessage(1061091); // You cannot cast that spell in this form.
			return false;
		}

		return true;
	}

	public static bool OnCast(Mobile caster, Spell spell)
	{
		if (spell is not ITransformationSpell transformSpell)
			return false;

		if (Factions.Sigil.ExistsOn(caster))
		{
			caster.SendLocalizedMessage(1061632); // You can't do that while carrying the sigil.
		}
		else if (!caster.CanBeginAction(typeof(PolymorphSpell)))
		{
			caster.SendLocalizedMessage(1061628); // You can't do that while polymorphed.
		}
		else if (DisguiseTimers.IsDisguised(caster))
		{
			caster.SendLocalizedMessage(1061631); // You can't do that while disguised.
			return false;
		}
		else if (AnimalForm.UnderTransformation(caster))
		{
			caster.SendLocalizedMessage(1061091); // You cannot cast that spell in this form.
		}
		else if (!caster.CanBeginAction(typeof(IncognitoSpell)) || (caster.IsBodyMod && GetContext(caster) == null))
		{
			spell.DoFizzle();
		}
		else if (spell.CheckSequence())
		{
			TransformContext context = GetContext(caster);
			Type ourType = spell.GetType();

			bool wasTransformed = context != null;
			bool ourTransform = wasTransformed && context.Type == ourType;

			if (wasTransformed)
			{
				RemoveContext(caster, context, ourTransform);

				if (ourTransform)
				{
					caster.PlaySound(0xFA);
					caster.FixedParticles(0x3728, 1, 13, 5042, EffectLayer.Waist);
				}
			}

			if (!ourTransform)
			{
				List<ResistanceMod> mods = new();

				if (transformSpell.PhysResistOffset != 0)
					mods.Add(new ResistanceMod(ResistanceType.Physical, transformSpell.PhysResistOffset));

				if (transformSpell.FireResistOffset != 0)
					mods.Add(new ResistanceMod(ResistanceType.Fire, transformSpell.FireResistOffset));

				if (transformSpell.ColdResistOffset != 0)
					mods.Add(new ResistanceMod(ResistanceType.Cold, transformSpell.ColdResistOffset));

				if (transformSpell.PoisResistOffset != 0)
					mods.Add(new ResistanceMod(ResistanceType.Poison, transformSpell.PoisResistOffset));

				if (transformSpell.NrgyResistOffset != 0)
					mods.Add(new ResistanceMod(ResistanceType.Energy, transformSpell.NrgyResistOffset));

				if (!((Body)transformSpell.Body).IsHuman)
				{
					var mt = caster.Mount;

					if (mt != null)
						mt.Rider = null;
				}

				caster.BodyMod = transformSpell.Body;
				caster.HueMod = transformSpell.Hue;

				for (int i = 0; i < mods.Count; ++i)
					caster.AddResistanceMod(mods[i]);

				transformSpell.DoEffect(caster);

				Timer timer = new TransformTimer(caster, transformSpell);
				timer.Start();

				AddContext(caster, new TransformContext(timer, mods, ourType, transformSpell));
				return true;
			}
		}

		return false;
	}
}
