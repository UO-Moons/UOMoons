using System.Collections.Generic;

namespace Server.Mobiles;

public class Architect : BaseVendor
{
	private readonly List<SbInfo> _mSbInfos = new();
	protected override List<SbInfo> SbInfos => _mSbInfos;

	public override NpcGuild NpcGuild => NpcGuild.TinkersGuild;

	public override double GetBuyDiscountFor(Mobile from)
	{
		return 1.0;
	}

	public override double GetSellDiscountFor(Mobile from)
	{
		return 1.0;
	}

	[Constructable]
	public Architect() : base("the architect")
	{
		Job = JobFragment.architect;
		Karma = Utility.RandomMinMax(13, -45);
		BankAccount = BankRestockAmount = 0x40000000;
	}

	public override void InitSbInfo()
	{
		if (!Core.AOS)
			_mSbInfos.Add(new SbHouseDeed());

		_mSbInfos.Add(new SbArchitect());
	}

	public Architect(Serial serial) : base(serial)
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
