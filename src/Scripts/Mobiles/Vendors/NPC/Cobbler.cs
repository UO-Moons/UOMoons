using System.Collections.Generic;

namespace Server.Mobiles;

public class Cobbler : BaseVendor
{
	private readonly List<SbInfo> _mSbInfos = new();
	protected override List<SbInfo> SbInfos => _mSbInfos;

	[Constructable]
	public Cobbler() : base("the cobbler")
	{
		Job = JobFragment.cobbler;
		Karma = Utility.RandomMinMax(13, -45);
		SetSkill(SkillName.Tailoring, 60.0, 83.0);
	}

	public override void InitSbInfo()
	{
		_mSbInfos.Add(new SbCobbler());
	}

	public override VendorShoeType ShoeType => Utility.RandomBool() ? VendorShoeType.Sandals : VendorShoeType.Shoes;

	public Cobbler(Serial serial) : base(serial)
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
