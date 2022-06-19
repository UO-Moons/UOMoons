using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles;

public class Farmer : BaseVendor
{
	private readonly List<SbInfo> _mSbInfos = new();
	protected override List<SbInfo> SbInfos => _mSbInfos;

	[Constructable]
	public Farmer() : base("the farmer")
	{
		Job = JobFragment.farmer;
		Karma = Utility.RandomMinMax(13, -45);
		SetSkill(SkillName.Lumberjacking, 36.0, 68.0);
		SetSkill(SkillName.TasteID, 36.0, 68.0);
		SetSkill(SkillName.Cooking, 36.0, 68.0);
	}

	public override void InitSbInfo()
	{
		_mSbInfos.Add(new SbFarmer());
	}

	public override VendorShoeType ShoeType => VendorShoeType.ThighBoots;

	public override int GetShoeHue()
	{
		return 0;
	}

	public override void InitOutfit()
	{
		base.InitOutfit();

		AddItem(new WideBrimHat(Utility.RandomNeutralHue()));
	}

	public Farmer(Serial serial) : base(serial)
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
