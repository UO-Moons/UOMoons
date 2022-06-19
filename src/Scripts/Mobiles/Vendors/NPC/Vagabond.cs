using Server.Items;
using System.Collections.Generic;

namespace Server.Mobiles;

public class Vagabond : BaseVendor
{
	private readonly List<SbInfo> _mSbInfos = new();
	protected override List<SbInfo> SbInfos => _mSbInfos;

	[Constructable]
	public Vagabond() : base("the vagabond")
	{
		Job = JobFragment.cobbler;
		Karma = Utility.RandomMinMax(13, -45);
		SetSkill(SkillName.ItemID, 60.0, 83.0);
	}

	public override void InitSbInfo()
	{
		_mSbInfos.Add(new SbTinker(this));
		_mSbInfos.Add(new SbVagabond());
	}

	public override void InitOutfit()
	{
		AddItem(new FancyShirt(Utility.RandomBrightHue()));
		AddItem(new Shoes(GetShoeHue()));
		AddItem(new LongPants(GetRandomHue()));

		if (Utility.RandomBool())
			AddItem(new Cloak(Utility.RandomBrightHue()));

		switch (Utility.Random(2))
		{
			case 0: AddItem(new SkullCap(Utility.RandomNeutralHue())); break;
			case 1: AddItem(new Bandana(Utility.RandomNeutralHue())); break;
		}

		Utility.AssignRandomHair(this);
		Utility.AssignRandomFacialHair(this, HairHue);

		PackGold(100, 200);
	}

	public Vagabond(Serial serial) : base(serial)
	{
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
