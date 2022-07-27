using System;
using System.Collections.Generic;
using Server.Mobiles;
using Server.Spells.Necromancy;

namespace Server.Spells.Ninjitsu;

public class MirrorImage : NinjaSpell
{
	private static readonly Dictionary<Mobile, int> m_CloneCount = new();
	private static readonly SpellInfo m_Info = new(
		"Mirror Image", null,
		-1,
		9002);
	public MirrorImage(Mobile caster, Item scroll)
		: base(caster, scroll, m_Info)
	{
	}

	public override TimeSpan CastDelayBase => TimeSpan.FromSeconds(1.5);
	public override double RequiredSkill => Core.ML ? 20.0 : 40.0;
	public override int RequiredMana => 10;
	public override bool BlockedByAnimalForm => false;

	private static bool HasClone(Mobile m)
	{
		return m_CloneCount.ContainsKey(m);
	}

	public static void AddClone(Mobile m)
	{
		if (m == null)
			return;

		if (m_CloneCount.ContainsKey(m))
			m_CloneCount[m]++;
		else
			m_CloneCount[m] = 1;
	}

	public static void RemoveClone(Mobile m)
	{
		if (m == null)
			return;

		if (!m_CloneCount.ContainsKey(m)) return;
		m_CloneCount[m]--;

		if (m_CloneCount[m] == 0)
			m_CloneCount.Remove(m);
	}

	public override bool CheckCast()
	{
		if (Caster.Mounted)
		{
			Caster.SendLocalizedMessage(1063132); // You cannot use this ability while mounted.
			return false;
		}

		if (Caster.Followers + 1 > Caster.FollowersMax)
		{
			Caster.SendLocalizedMessage(1063133); // You cannot summon a mirror image because you have too many followers.
			return false;
		}

		if (TransformationSpellHelper.UnderTransformation(Caster, typeof(HorrificBeastSpell)))
		{
			Caster.SendLocalizedMessage(1061091); // You cannot cast that spell in this form.
			return false;
		}

		if (!Caster.Flying)
			return base.CheckCast();

		Caster.SendLocalizedMessage(1113415); // You cannot use this ability while flying.
		return false;

	}

	public override bool CheckDisturb(DisturbType type, bool resistable)
	{
		return false;
	}

	public override void OnBeginCast()
	{
		base.OnBeginCast();

		Caster.SendLocalizedMessage(1063134); // You begin to summon a mirror image of yourself.
	}

	public override void OnCast()
	{
		if (Caster.Mounted)
		{
			Caster.SendLocalizedMessage(1063132); // You cannot use this ability while mounted.
		}
		else if (Caster.Followers + 1 > Caster.FollowersMax)
		{
			Caster.SendLocalizedMessage(1063133); // You cannot summon a mirror image because you have too many followers.
		}
		else if (TransformationSpellHelper.UnderTransformation(Caster, typeof(HorrificBeastSpell)))
		{
			Caster.SendLocalizedMessage(1061091); // You cannot cast that spell in this form.
		}
		else if (CheckSequence())
		{
			Caster.FixedParticles(0x376A, 1, 14, 0x13B5, EffectLayer.Waist);
			Caster.PlaySound(0x511);

			new MirrorImageClone(Caster).MoveToWorld(Caster.Location, Caster.Map);
		}

		FinishSequence();
	}

	public static MirrorImageClone GetDeflect(Mobile attacker, Mobile defender)
	{
		MirrorImageClone clone = null;

		if (!HasClone(defender) || !((defender.Skills.Ninjitsu.Value / 150.0) > Utility.RandomDouble()))
			return null;
		IPooledEnumerable eable = defender.GetMobilesInRange(4);

		foreach (Mobile m in eable)
		{
			clone = m as MirrorImageClone;

			if (clone is { Summoned: true } && clone.SummonMaster == defender)
			{
				attacker.SendLocalizedMessage(1063141); // Your attack has been diverted to a nearby mirror image of your target!
				defender.SendLocalizedMessage(1063140); // You manage to divert the attack onto one of your nearby mirror images.
				break;
			}
		}

		eable.Free();

		return clone;
	}
}
