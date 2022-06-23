using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles;

public class Waiter : BaseVendor
{
	private readonly List<SbInfo> _mSbInfos = new();
	protected override List<SbInfo> SbInfos => _mSbInfos;

	[Constructable]
	public Waiter() : base("the waiter")
	{
		Job = JobFragment.waiter;
		Karma = Utility.RandomMinMax(13, -45);
		SetSkill(SkillName.Discordance, 36.0, 68.0);
	}

	public override void InitSbInfo()
	{
		_mSbInfos.Add(new SbWaiter());
	}

	public override void InitOutfit()
	{
		base.InitOutfit();

		SetWearable(new HalfApron());
	}

	public Waiter(Serial serial) : base(serial)
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
