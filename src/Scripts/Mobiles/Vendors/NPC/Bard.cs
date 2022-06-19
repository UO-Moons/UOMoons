using System.Collections.Generic;

namespace Server.Mobiles;

public class Bard : BaseVendor
{
	private readonly List<SbInfo> _mSbInfos = new();
	protected override List<SbInfo> SbInfos => _mSbInfos;

	public override NpcGuild NpcGuild => NpcGuild.BardsGuild;

	[Constructable]
	public Bard() : base("the bard")
	{
		Job = JobFragment.bard;
		Karma = Utility.RandomMinMax(13, -45);
		SetSkill(SkillName.Discordance, 64.0, 100.0);
		SetSkill(SkillName.Musicianship, 64.0, 100.0);
		SetSkill(SkillName.Peacemaking, 65.0, 88.0);
		SetSkill(SkillName.Provocation, 60.0, 83.0);
		SetSkill(SkillName.Archery, 36.0, 68.0);
		SetSkill(SkillName.Swords, 36.0, 68.0);
	}

	public override void InitSbInfo()
	{
		_mSbInfos.Add(new SbBard());
	}

	public Bard(Serial serial) : base(serial)
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
