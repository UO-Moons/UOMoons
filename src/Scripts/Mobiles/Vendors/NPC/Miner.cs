using System.Collections.Generic;

namespace Server.Mobiles;

public class Miner : BaseVendor
{
	private readonly List<SbInfo> _mSbInfos = new();
	protected override List<SbInfo> SbInfos => _mSbInfos;

	[Constructable]
	public Miner() : base("the miner")
	{
		Job = JobFragment.miner;
		Karma = Utility.RandomMinMax(13, -45);
		SetSkill(SkillName.Mining, 65.0, 88.0);
	}

	public override void InitSbInfo()
	{
		_mSbInfos.Add(new SbMiner());
	}

	public override void InitOutfit()
	{
		base.InitOutfit();

		SetWearable(new Server.Items.FancyShirt(0x3E4));
		SetWearable(new Server.Items.LongPants(0x192));
		SetWearable(new Server.Items.Pickaxe());
		SetWearable(new Server.Items.ThighBoots(0x283));
	}

	public Miner(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		int version = reader.ReadInt();
	}
}
