using Server.Items;
using System;

namespace Server.Mobiles;

[CorpseName("Devourer of Souls [Renowned] corpse")]
public class DevourerRenowned : BaseRenowned
{
	[Constructable]
	public DevourerRenowned()
		: base(AIType.AI_NecroMage)
	{
		Name = "Devourer of Souls";
		Body = 303;
		BaseSoundId = 357;

		SetStr(861, 910);
		SetDex(132, 137);
		SetInt(230, 239);

		SetHits(1892, 1962);

		SetDamage(22, 26);

		SetDamageType(ResistanceType.Physical, 60);
		SetDamageType(ResistanceType.Cold, 20);
		SetDamageType(ResistanceType.Energy, 20);

		SetResistance(ResistanceType.Physical, 49, 51);
		SetResistance(ResistanceType.Fire, 25, 32);
		SetResistance(ResistanceType.Cold, 18, 20);
		SetResistance(ResistanceType.Poison, 68);
		SetResistance(ResistanceType.Energy, 46, 47);

		SetSkill(SkillName.EvalInt, 92.1, 98.0);
		SetSkill(SkillName.Magery, 98.1, 99.0);
		SetSkill(SkillName.Meditation, 91.1, 95.0);
		SetSkill(SkillName.MagicResist, 94.1, 103.0);
		SetSkill(SkillName.Tactics, 76.1, 83.0);
		SetSkill(SkillName.Wrestling, 80.1, 97.0);

		Fame = 9500;
		Karma = -9750;
	}

	public DevourerRenowned(Serial serial)
		: base(serial)
	{
	}

	protected override Type[] UniqueSaList => Array.Empty<Type>();
	protected override Type[] SharedSaList => new[] { typeof(AnimatedLegsoftheInsaneTinker), typeof(StormCaller), typeof(PillarOfStrength) };
	public override Poison PoisonImmune => Poison.Lethal;
	public override int Meat => 3;

	public override void GenerateLoot()
	{
		AddLoot(LootPack.FilthyRich, 2);
		AddLoot(LootPack.NecroRegs, 24, 45);
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadInt();
	}
}
