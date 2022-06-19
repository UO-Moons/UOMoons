using System.Collections.Generic;

namespace Server.Mobiles;

public class Herbalist : BaseVendor
{
	private readonly List<SbInfo> _mSbInfos = new();
	protected override List<SbInfo> SbInfos => _mSbInfos;

	public override NpcGuild NpcGuild => NpcGuild.MagesGuild;

	[Constructable]
	public Herbalist() : base("the herbalist")
	{
		Job = JobFragment.herbalist;
		Karma = Utility.RandomMinMax(13, -45);
		SetSkill(SkillName.Alchemy, 80.0, 100.0);
		SetSkill(SkillName.Cooking, 80.0, 100.0);
		SetSkill(SkillName.TasteID, 80.0, 100.0);
	}

	public override void InitSbInfo()
	{
		_mSbInfos.Add(new SbHerbalist());
	}

	public override VendorShoeType ShoeType => Utility.RandomBool() ? VendorShoeType.Shoes : VendorShoeType.Sandals;

	public Herbalist(Serial serial) : base(serial)
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
