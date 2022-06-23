using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles;

public class Butcher : BaseVendor
{
	private readonly List<SbInfo> _mSbInfos = new();
	protected override List<SbInfo> SbInfos => _mSbInfos;

	[Constructable]
	public Butcher() : base("the butcher")
	{
		Job = JobFragment.farmer;
		Karma = Utility.RandomMinMax(13, -45);
		SetSkill(SkillName.Anatomy, 45.0, 68.0);
	}

	public override void InitSbInfo()
	{
		_mSbInfos.Add(new SbButcher());
	}

	public override void InitOutfit()
	{
		base.InitOutfit();

		SetWearable(new HalfApron());
		SetWearable(new Cleaver());
	}

	public Butcher(Serial serial) : base(serial)
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
