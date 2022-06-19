using System.Collections.Generic;

namespace Server.Mobiles;

public class Jeweler : BaseVendor
{
	private readonly List<SbInfo> _mSbInfos = new();
	protected override List<SbInfo> SbInfos => _mSbInfos;

	[Constructable]
	public Jeweler() : base("the jeweler")
	{
		Job = JobFragment.jeweler;
		Karma = Utility.RandomMinMax(13, -45);
		SetSkill(SkillName.ItemID, 64.0, 100.0);
		BankAccount = BankRestockAmount = 0x40000000;
	}

	public override void InitSbInfo()
	{
		_mSbInfos.Add(new SbJewel());
	}

	public Jeweler(Serial serial) : base(serial)
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
