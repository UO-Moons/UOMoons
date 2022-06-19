using System.Collections.Generic;

namespace Server.Mobiles;

public class Beekeeper : BaseVendor
{
	private readonly List<SbInfo> _mSbInfos = new();
	protected override List<SbInfo> SbInfos => _mSbInfos;

	[Constructable]
	public Beekeeper() : base("the beekeeper")
	{
		Job = JobFragment.beekeeper;
		Karma = Utility.RandomMinMax(13, -45);
	}

	public override void InitSbInfo()
	{
		_mSbInfos.Add(new SbBeekeeper());
	}

	public override VendorShoeType ShoeType => VendorShoeType.Boots;

	public Beekeeper(Serial serial) : base(serial)
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
