using Server.Items;
using Server.Misc;
using System;

namespace Server.Mobiles;

[CorpseName("Tikitavi [Renowned] corpse")]
public class TikitaviRenowned : BaseRenowned
{
	[Constructable]
	public TikitaviRenowned()
		: base(AIType.AI_Melee)
	{
		Name = "Tikitavi";
		Body = 42;
		BaseSoundId = 437;

		SetStr(315, 354);
		SetDex(139, 177);
		SetInt(243, 288);

		SetHits(50000);
		SetMana(243, 288);
		SetStam(139, 177);

		SetDamage(7, 9);

		SetDamageType(ResistanceType.Physical, 100);

		SetResistance(ResistanceType.Physical, 26, 28);
		SetResistance(ResistanceType.Fire, 22, 25);
		SetResistance(ResistanceType.Cold, 30, 38);
		SetResistance(ResistanceType.Poison, 14, 17);
		SetResistance(ResistanceType.Energy, 15, 18);

		SetSkill(SkillName.MagicResist, 40.4);
		SetSkill(SkillName.Tactics, 73.6);
		SetSkill(SkillName.Wrestling, 66.5);

		Fame = 1500;
		Karma = -1500;
	}

	public TikitaviRenowned(Serial serial)
		: base(serial)
	{
	}
	public override bool AllureImmunity => true;

	protected override Type[] UniqueSaList => new[] { typeof(BasiliskHideBreastplate) };
	protected override Type[] SharedSaList => new[] { typeof(LegacyOfDespair), typeof(MysticsGarb) };
	public override InhumanSpeech SpeechType => InhumanSpeech.Ratman;
	public override bool CanRummageCorpses => true;
	public override int Hides => 8;
	public override HideType HideType => HideType.Spined;
	public override void GenerateLoot()
	{
		AddLoot(LootPack.FilthyRich, 3);
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
