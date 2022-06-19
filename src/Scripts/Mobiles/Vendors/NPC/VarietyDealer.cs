using System.Collections.Generic;

namespace Server.Mobiles;

public class VarietyDealer : BaseVendor
{
	private readonly List<SbInfo> _mSbInfos = new();
	protected override List<SbInfo> SbInfos => _mSbInfos;

	[Constructable]
	public VarietyDealer() : base("the variety dealer")
	{
		Job = JobFragment.cobbler;
		Karma = Utility.RandomMinMax(13, -45);
	}

	public override void InitSbInfo()
	{
		_mSbInfos.Add(new SbVarietyDealer());
	}

	public VarietyDealer(Serial serial) : base(serial)
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
