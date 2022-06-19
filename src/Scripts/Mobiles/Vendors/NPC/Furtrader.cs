using System.Collections.Generic;

namespace Server.Mobiles;

public class Furtrader : BaseVendor
{
	private readonly List<SbInfo> _mSbInfos = new();
	protected override List<SbInfo> SbInfos => _mSbInfos;

	[Constructable]
	public Furtrader() : base("the furtrader")
	{
		Job = JobFragment.furtrader;
		Karma = Utility.RandomMinMax(13, -45);
		SetSkill(SkillName.Camping, 55.0, 78.0);
		//SetSkill( SkillName.Alchemy, 60.0, 83.0 );
		SetSkill(SkillName.AnimalLore, 85.0, 100.0);
		SetSkill(SkillName.Cooking, 45.0, 68.0);
		SetSkill(SkillName.Tracking, 36.0, 68.0);
	}

	public override void InitSbInfo()
	{
		_mSbInfos.Add(new SbFurtrader());
	}

	public Furtrader(Serial serial) : base(serial)
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
