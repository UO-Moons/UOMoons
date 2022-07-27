using Server.Items;

namespace Server.Spells.Necromancy;

public abstract class NecromancerSpell : Spell
{
	public NecromancerSpell(Mobile caster, Item scroll, SpellInfo info)
		: base(caster, scroll, info)
	{
	}

	public abstract double RequiredSkill { get; }
	public abstract int RequiredMana { get; }

	public override SkillName CastSkill => SkillName.Necromancy;
	public override SkillName DamageSkill => SkillName.SpiritSpeak;
	public override bool ClearHandsOnCast => false;
	public override double CastDelayFastScalar => (Core.SE ? base.CastDelayFastScalar : 0);// Necromancer spells are not affected by fast cast items, though they are by fast cast recovery

	public override int ComputeKarmaAward()
	{
		//TODO: Verify this formula being that Necro spells don't HAVE a circle.
		//int karma = -(70 + (10 * (int)Circle));
		int karma = -(40 + (int)(10 * (CastDelayBase.TotalSeconds / CastDelaySecondsPerTick)));

		if (Core.ML) // Pub 36: "Added a new property called Increased Karma Loss which grants higher karma loss for casting necromancy spells."
			karma += AOS.Scale(karma, AosAttributes.GetValue(Caster, AosAttribute.IncreasedKarmaLoss));

		return karma;
	}

	public override void GetCastSkills(out double min, out double max)
	{
		min = RequiredSkill;
		max = Scroll != null ? min : RequiredSkill + 40.0;
	}

	public override bool ConsumeReagents()
	{
		if (base.ConsumeReagents())
			return true;

		if (ArcaneGem.ConsumeCharges(Caster, 1))
			return true;

		return false;
	}

	public override int GetMana()
	{
		return RequiredMana;
	}
}
