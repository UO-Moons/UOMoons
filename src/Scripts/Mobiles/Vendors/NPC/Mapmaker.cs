using System.Collections.Generic;

namespace Server.Mobiles;

public class Mapmaker : BaseVendor
{
	private readonly List<SbInfo> _mSbInfos = new();
	protected override List<SbInfo> SbInfos => _mSbInfos;

	[Constructable]
	public Mapmaker() : base("the mapmaker")
	{
		Job = JobFragment.mapmaker;
		Karma = Utility.RandomMinMax(13, -45);
		SetSkill(SkillName.Cartography, 90.0, 100.0);
	}

	public override void InitSbInfo()
	{
		_mSbInfos.Add(new SbMapmaker());
	}

	public Mapmaker(Serial serial) : base(serial)
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
