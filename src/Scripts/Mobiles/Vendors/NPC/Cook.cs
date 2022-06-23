using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles;

public class Cook : BaseVendor
{
	private readonly List<SbInfo> _mSbInfos = new();
	protected override List<SbInfo> SbInfos => _mSbInfos;

	[Constructable]
	public Cook() : base("the cook")
	{
		Job = JobFragment.cook;
		Karma = Utility.RandomMinMax(13, -45);
		SetSkill(SkillName.Cooking, 90.0, 100.0);
		SetSkill(SkillName.TasteID, 75.0, 98.0);
	}

	public override void InitSbInfo()
	{
		_mSbInfos.Add(new SbCook());

		if (IsTokunoVendor)
			_mSbInfos.Add(new SbseCook());
	}

	public override VendorShoeType ShoeType => Utility.RandomBool() ? VendorShoeType.Sandals : VendorShoeType.Shoes;

	public override void InitOutfit()
	{
		base.InitOutfit();

		SetWearable(new HalfApron());
	}

	public Cook(Serial serial) : base(serial)
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
