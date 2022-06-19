using System.Collections.Generic;

namespace Server.Mobiles;

[TypeAlias("Server.Mobiles.GargoyleAlchemist")]
public class Glassblower : BaseVendor
{
	private readonly List<SbInfo> _mSbInfos = new();
	protected override List<SbInfo> SbInfos => _mSbInfos;

	public override NpcGuild NpcGuild => NpcGuild.MagesGuild;

	[Constructable]
	public Glassblower() : base("the Glassblower")
	{
		Job = JobFragment.glassblower;
		Karma = Utility.RandomMinMax(13, -45);
		SetSkill(SkillName.Alchemy, 85.0, 100.0);
		SetSkill(SkillName.TasteID, 85.0, 100.0);
	}

	public override void InitSbInfo()
	{
		_mSbInfos.Add(new SbGlassblower());
		_mSbInfos.Add(new SbAlchemist());
	}

	public Glassblower(Serial serial) : base(serial)
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
