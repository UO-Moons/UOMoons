using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles;

public class TavernKeeper : BaseVendor
{
	private readonly List<SbInfo> _mSbInfos = new();
	protected override List<SbInfo> SbInfos => _mSbInfos;

	[Constructable]
	public TavernKeeper() : base("the tavern keeper")
	{
		Job = JobFragment.tavkeep;
		Karma = Utility.RandomMinMax(13, -45);
	}

	public override void InitSbInfo()
	{
		_mSbInfos.Add(new SbTavernKeeper());
	}

	public override void InitOutfit()
	{
		base.InitOutfit();

		AddItem(new HalfApron());
	}

	public TavernKeeper(Serial serial) : base(serial)
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
