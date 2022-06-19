using System.Collections.Generic;

namespace Server.Mobiles;

public class LeatherWorker : BaseVendor
{
	private readonly List<SbInfo> _mSbInfos = new();
	protected override List<SbInfo> SbInfos => _mSbInfos;

	[Constructable]
	public LeatherWorker() : base("the leather worker")
	{
	}
	public override void InitSbInfo()
	{
		Job = JobFragment.tanner;
		Karma = Utility.RandomMinMax(13, -45);
		_mSbInfos.Add(new SbLeatherArmor());
		_mSbInfos.Add(new SbStuddedArmor());
		_mSbInfos.Add(new SbLeatherWorker());
	}
	public LeatherWorker(Serial serial) : base(serial)
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
