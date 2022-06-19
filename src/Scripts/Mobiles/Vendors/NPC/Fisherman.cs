using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles;

public class Fisherman : BaseVendor
{
	private readonly List<SbInfo> _mSbInfos = new();
	protected override List<SbInfo> SbInfos => _mSbInfos;

	public override NpcGuild NpcGuild => NpcGuild.FishermensGuild;

	[Constructable]
	public Fisherman() : base("the fisher")
	{
		Job = JobFragment.fisher;
		Karma = Utility.RandomMinMax(13, -45);
		SetSkill(SkillName.Fishing, 75.0, 98.0);
	}

	public override void InitSbInfo()
	{
		_mSbInfos.Add(new SbFisherman());
	}

	public override void InitOutfit()
	{
		base.InitOutfit();

		AddItem(new FishingPole());
	}

	public Fisherman(Serial serial) : base(serial)
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
