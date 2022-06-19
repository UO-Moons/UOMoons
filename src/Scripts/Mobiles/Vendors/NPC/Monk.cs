using Server.Items;
using System.Collections.Generic;

namespace Server.Mobiles;

public class Monk : BaseVendor
{
	private readonly List<SbInfo> _mSbInfos = new();
	protected override List<SbInfo> SbInfos => _mSbInfos;

	[Constructable]
	public Monk() : base("the Monk")
	{
		Job = JobFragment.monk;
		Karma = Utility.RandomMinMax(13, -45);
		SetSkill(SkillName.EvalInt, 100.0);
		SetSkill(SkillName.Tactics, 70.0, 90.0);
		SetSkill(SkillName.Wrestling, 70.0, 90.0);
		SetSkill(SkillName.MagicResist, 70.0, 90.0);
		SetSkill(SkillName.Macing, 70.0, 90.0);
	}

	public override void InitSbInfo()
	{
		_mSbInfos.Add(new SbMonk());
	}
	public override void InitOutfit()
	{
		AddItem(new Sandals());
		AddItem(new MonkRobe());
	}

	public Monk(Serial serial) : base(serial)
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
