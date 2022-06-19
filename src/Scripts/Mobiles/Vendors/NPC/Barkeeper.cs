using Server.Items;
using System.Collections.Generic;

namespace Server.Mobiles;

public class Barkeeper : BaseVendor
{
	private readonly List<SbInfo> _mSbInfos = new();
	protected override List<SbInfo> SbInfos => _mSbInfos;

	public override void InitSbInfo()
	{
		_mSbInfos.Add(new SbBarkeeper());
	}

	public override VendorShoeType ShoeType => Utility.RandomBool() ? VendorShoeType.ThighBoots : VendorShoeType.Boots;

	public override void InitOutfit()
	{
		base.InitOutfit();

		AddItem(new HalfApron(Utility.RandomBrightHue()));
	}

	[Constructable]
	public Barkeeper() : base("the barkeeper")
	{
		Job = JobFragment.tavkeep;
		Karma = Utility.RandomMinMax(13, -45);
	}

	public Barkeeper(Serial serial) : base(serial)
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
