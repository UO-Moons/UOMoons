using System.Collections.Generic;

namespace Server.Mobiles;

public class Rancher : BaseVendor
{
	private readonly List<SbInfo> _mSbInfos = new();
	protected override List<SbInfo> SbInfos => _mSbInfos;

	[Constructable]
	public Rancher() : base("the rancher")
	{
		Job = JobFragment.rancher;
		Karma = Utility.RandomMinMax(13, -45);
		SetSkill(SkillName.AnimalLore, 55.0, 78.0);
		SetSkill(SkillName.AnimalTaming, 55.0, 78.0);
		SetSkill(SkillName.Herding, 64.0, 100.0);
		SetSkill(SkillName.Veterinary, 60.0, 83.0);
	}

	public override void InitSbInfo()
	{
		_mSbInfos.Add(new SbRancher());
	}

	public Rancher(Serial serial) : base(serial)
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
