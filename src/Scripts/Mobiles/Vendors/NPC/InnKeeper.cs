using System.Collections.Generic;

namespace Server.Mobiles;

public class InnKeeper : BaseVendor
{
	private readonly List<SbInfo> _mSbInfos = new();
	protected override List<SbInfo> SbInfos => _mSbInfos;

	[Constructable]
	public InnKeeper() : base("the innkeeper")
	{
		Job = JobFragment.innkeeper;
		Karma = Utility.RandomMinMax(13, -45);
	}

	public override void InitSbInfo()
	{
		_mSbInfos.Add(new SbInnKeeper());

		if (IsTokunoVendor)
			_mSbInfos.Add(new SbseFood());
	}

	public override VendorShoeType ShoeType => Utility.RandomBool() ? VendorShoeType.Sandals : VendorShoeType.Shoes;

	public InnKeeper(Serial serial) : base(serial)
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
