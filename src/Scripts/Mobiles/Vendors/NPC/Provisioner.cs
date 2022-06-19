using System.Collections.Generic;

namespace Server.Mobiles;

public class Provisioner : BaseVendor
{
	private readonly List<SbInfo> _mSbInfos = new();
	protected override List<SbInfo> SbInfos => _mSbInfos;

	[Constructable]
	public Provisioner() : base("the provisioner")
	{
		Job = JobFragment.cobbler;
		Karma = Utility.RandomMinMax(13, -45);
		SetSkill(SkillName.Camping, 45.0, 68.0);
		SetSkill(SkillName.Tactics, 45.0, 68.0);
	}

	public override void InitSbInfo()
	{
		_mSbInfos.Add(new SbProvisioner());

		if (IsTokunoVendor)
			_mSbInfos.Add(new SbseHats());
	}

	public Provisioner(Serial serial) : base(serial)
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
