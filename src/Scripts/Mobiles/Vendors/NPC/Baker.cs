using System.Collections.Generic;

namespace Server.Mobiles;

public class Baker : BaseVendor
{
	private readonly List<SbInfo> _mSbInfos = new();
	protected override List<SbInfo> SbInfos => _mSbInfos;

	[Constructable]
	public Baker() : base("the baker")
	{
		Job = JobFragment.baker;
		Karma = Utility.RandomMinMax(13, -45);
		SetSkill(SkillName.Cooking, 75.0, 98.0);
		SetSkill(SkillName.TasteID, 36.0, 68.0);
	}

	public override void InitSbInfo()
	{
		_mSbInfos.Add(new SbBaker());
	}

	public Baker(Serial serial) : base(serial)
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
