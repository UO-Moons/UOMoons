using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles;

public class Ranger : BaseVendor
{
	private readonly List<SbInfo> _mSbInfos = new();
	protected override List<SbInfo> SbInfos => _mSbInfos;

	[Constructable]
	public Ranger() : base("the ranger")
	{
		Job = JobFragment.ranger;
		Karma = Utility.RandomMinMax(13, -45);
		SetSkill(SkillName.Camping, 55.0, 78.0);
		SetSkill(SkillName.DetectHidden, 65.0, 88.0);
		SetSkill(SkillName.Hiding, 45.0, 68.0);
		SetSkill(SkillName.Archery, 65.0, 88.0);
		SetSkill(SkillName.Tracking, 65.0, 88.0);
		SetSkill(SkillName.Veterinary, 60.0, 83.0);
	}

	public override void InitSbInfo()
	{
		_mSbInfos.Add(new SbRanger());
	}

	public override void InitOutfit()
	{
		base.InitOutfit();

		SetWearable(new Shirt(Utility.RandomNeutralHue()));
		SetWearable(new LongPants(Utility.RandomNeutralHue()));
		SetWearable(new Bow());
		SetWearable(new ThighBoots(Utility.RandomNeutralHue()));
	}

	public Ranger(Serial serial) : base(serial)
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
