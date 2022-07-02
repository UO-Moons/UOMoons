using Server.Items;

namespace Server.Mobiles;

public class Kevin : HumilityQuestMobile
{
	public override int Greeting => 1075759;

	public override bool IsActiveVendor => true;
	public override bool CanTeach => true;

	[Constructable]
	public Kevin()
		: base("Kevin", "the Butcher")
	{
		SetSkill(SkillName.Anatomy, 45.0, 68.0);
	}

	public override void InitSbInfo()
	{
		SbInfos.Add(new SbButcher());
	}

	public Kevin(Serial serial)
		: base(serial)
	{
	}

	public override void InitBody()
	{
		InitStats(100, 100, 25);

		Female = false;
		Race = Race.Human;
		Body = 0x190;

		Hue = Race.RandomSkinHue();
		HairItemId = Race.RandomHair(false);
		HairHue = Race.RandomHairHue();
	}

	public override void InitOutfit()
	{
		base.InitOutfit();

		AddItem(new HalfApron());
		AddItem(new Cleaver());
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		_ = reader.ReadInt();
	}
}