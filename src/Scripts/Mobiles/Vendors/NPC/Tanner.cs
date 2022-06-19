using System.Collections.Generic;

namespace Server.Mobiles;

public class Tanner : BaseVendor
{
	private readonly List<SbInfo> _mSbInfos = new();
	protected override List<SbInfo> SbInfos => _mSbInfos;

	[Constructable]
	public Tanner() : base("the tanner")
	{
		SetSkill(SkillName.Tailoring, 36.0, 68.0);
	}

	public override void InitSbInfo()
	{
		_mSbInfos.Add(new SbTanner());
	}

	public Tanner(Serial serial) : base(serial)
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
