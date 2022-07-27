using System;
using System.Collections.Generic;
using Server.Network;

namespace Server.Spells.Mysticism;

public class StoneFormSpell : MysticTransformationSpell
{
	private static readonly HashSet<Mobile> m_Effected = new();
	public static bool IsEffected(Mobile m)
	{
		return m_Effected.Contains(m);
	}

	private static readonly SpellInfo m_Info = new(
		"Stone Form", "In Rel Ylem",
		230,
		9022,
		Reagent.Bloodmoss,
		Reagent.FertileDirt,
		Reagent.Garlic
	);

	private int m_ResisMod;

	public override SpellCircle Circle => SpellCircle.Fourth;

	public override TimeSpan CastDelayBase => TimeSpan.FromSeconds(2.0);

	public override int Body => 705;
	public override int PhysResistOffset => m_ResisMod;
	public override int FireResistOffset => m_ResisMod;
	public override int ColdResistOffset => m_ResisMod;
	public override int PoisResistOffset => m_ResisMod;
	public override int NrgyResistOffset => m_ResisMod;

	public StoneFormSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
	{
	}

	public override bool CheckCast()
	{
		bool doCast = base.CheckCast();

		if (doCast && Caster.Flying)
		{
			Caster.SendLocalizedMessage(1112567); // You are flying.
			doCast = false;
		}

		if (doCast)
			m_ResisMod = GetResBonus(Caster);

		return doCast;
	}

	public override void DoEffect(Mobile m)
	{
		m.PlaySound(0x65A);
		m.FixedParticles(0x3728, 1, 13, 9918, 92, 3, EffectLayer.Head);

		Timer.DelayCall(MobileDelta_Callback);
		m_Effected.Add(m);


		if (!Core.HS)
		{
			m.SendSpeedControl(SpeedControlType.WalkSpeed);
		}

		string args =
			$"-10\t-2\t{GetResBonus(m)}\t{GetMaxResistance(m)}\t{GetDamBonus(m)}";
		BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.StoneForm, 1080145, 1080146, args));
		BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.PoisonImmunity, 1153785, 1153814));
	}

	private void MobileDelta_Callback()
	{
		Caster.Delta(MobileDelta.WeaponDamage);
		Caster.UpdateResistances();
	}

	public override void RemoveEffect(Mobile m)
	{
		if (!Core.HS)
		{
			m.SendSpeedControl(SpeedControlType.Disable);
		}

		m.Delta(MobileDelta.WeaponDamage);
		m_Effected.Remove(m);
		BuffInfo.RemoveBuff(m, BuffIcon.StoneForm);
		BuffInfo.RemoveBuff(m, BuffIcon.PoisonImmunity);
	}

	public static void Initialize()
	{
		if (!Core.HS)
		{
			EventSink.OnLogin += OnLogin;
		}
	}

	private static void OnLogin(Mobile m)
	{
		TransformContext context = TransformationSpellHelper.GetContext(m);

		if (context != null && context.Type == typeof(StoneFormSpell))
			m.SendSpeedControl(SpeedControlType.WalkSpeed);
	}

	public static int GetMaxResistBonus(Mobile m)
	{
		return TransformationSpellHelper.UnderTransformation(m, typeof(StoneFormSpell)) ? GetMaxResistance(m) : 0;
	}

	public static int GetResistanceBonus(Mobile m)
	{
		return TransformationSpellHelper.UnderTransformation(m, typeof(StoneFormSpell)) ? GetResBonus(m) : 0;
	}

	public static int GetDamageBonus(Mobile m)
	{
		return TransformationSpellHelper.UnderTransformation(m, typeof(StoneFormSpell)) ? GetDamBonus(m) : 0;
	}

	public static double StatOffsetReduction(Mobile caster, Mobile m)
	{
		if (!TransformationSpellHelper.UnderTransformation(m, typeof(StoneFormSpell)))
			return 1.0;

		int prim = (int)m.Skills[SkillName.Mysticism].Value;
		int sec = (int)m.Skills[SkillName.Imbuing].Value;

		if (m.Skills[SkillName.Focus].Value > sec)
			sec = (int)m.Skills[SkillName.Focus].Value;

		caster.SendLocalizedMessage(1080192); // Your target resists your ability reduction magic.

		return (double)(prim + sec) / 480;

	}

	private static int GetResBonus(Mobile m)
	{
		int prim = (int)m.Skills[SkillName.Mysticism].Value;
		int sec = (int)m.Skills[SkillName.Imbuing].Value;

		if (m.Skills[SkillName.Focus].Value > sec)
			sec = (int)m.Skills[SkillName.Focus].Value;

		return Math.Max(2, (prim + sec) / 24);
	}

	private static int GetMaxResistance(Mobile m)
	{
		int prim = (int)m.Skills[SkillName.Mysticism].Value;
		int sec = (int)m.Skills[SkillName.Imbuing].Value;

		if (m.Skills[SkillName.Focus].Value > sec)
			sec = (int)m.Skills[SkillName.Focus].Value;

		return Math.Max(2, (prim + sec) / 48);
	}

	private static int GetDamBonus(Mobile m)
	{
		int prim = (int)m.Skills[SkillName.Mysticism].Value;
		int sec = (int)m.Skills[SkillName.Imbuing].Value;

		if (m.Skills[SkillName.Focus].Value > sec)
			sec = (int)m.Skills[SkillName.Focus].Value;

		return Math.Max(0, (prim + sec) / 12);
	}

	public static bool CheckImmunity(Mobile from)
	{
		if (!TransformationSpellHelper.UnderTransformation(from, typeof(StoneFormSpell)))
			return false;

		int prim = (int)from.Skills[SkillName.Mysticism].Value;
		int sec = (int)from.Skills[SkillName.Imbuing].Value;

		if (from.Skills[SkillName.Focus].Value > sec)
			sec = (int)from.Skills[SkillName.Focus].Value;

		int immunity = (int)(((double)(prim + sec) / 480) * 100);

		if (Necromancy.EvilOmenSpell.TryEndEffect(from))
			immunity -= 30;

		return immunity > Utility.Random(100);

	}
}
