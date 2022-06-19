using System.Collections.Generic;

namespace Server.Mobiles;

public class HairStylist : BaseVendor
{
	private readonly List<SbInfo> _mSbInfos = new();
	protected override List<SbInfo> SbInfos => _mSbInfos;

	[Constructable]
	public HairStylist() : base("the hair stylist")
	{
		SetSkill(SkillName.Alchemy, 80.0, 100.0);
		SetSkill(SkillName.Magery, 90.0, 110.0);
		SetSkill(SkillName.TasteID, 85.0, 100.0);
	}

	public override void InitSbInfo()
	{
		_mSbInfos.Add(new SbHairStylist());
	}

	public HairStylist(Serial serial) : base(serial)
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
