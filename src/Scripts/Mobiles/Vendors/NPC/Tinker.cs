using System.Collections.Generic;

namespace Server.Mobiles;

public class Tinker : BaseVendor
{
	private readonly List<SbInfo> _mSbInfos = new();
	protected override List<SbInfo> SbInfos => _mSbInfos;

	public override NpcGuild NpcGuild => NpcGuild.TinkersGuild;

	[Constructable]
	public Tinker() : base("the tinker")
	{
		Job = JobFragment.tinker;
		Karma = Utility.RandomMinMax(13, -45);
		SetSkill(SkillName.Lockpicking, 60.0, 83.0);
		SetSkill(SkillName.RemoveTrap, 75.0, 98.0);
		SetSkill(SkillName.Tinkering, 64.0, 100.0);
	}

	public override void InitSbInfo()
	{
		_mSbInfos.Add(new SbTinker(this));
	}

	public Tinker(Serial serial) : base(serial)
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
