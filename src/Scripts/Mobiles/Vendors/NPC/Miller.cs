using System.Collections.Generic;

namespace Server.Mobiles;

public class Miller : BaseVendor
{
	private readonly List<SbInfo> _mSbInfos = new();
	protected override List<SbInfo> SbInfos => _mSbInfos;

	[Constructable]
	public Miller() : base("the miller")
	{
		Job = JobFragment.miller;
		Karma = Utility.RandomMinMax(13, -45);
	}

	public override void InitSbInfo()
	{
		_mSbInfos.Add(new SbMiller());
	}

	public Miller(Serial serial) : base(serial)
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
