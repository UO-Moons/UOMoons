using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles;

public class Mage : BaseVendor
{
	private readonly List<SbInfo> _mSbInfos = new();
	protected override List<SbInfo> SbInfos => _mSbInfos;

	public override NpcGuild NpcGuild => NpcGuild.MagesGuild;

	[Constructable]
	public Mage() : base("the mage")
	{
		Job = JobFragment.mage;
		Karma = Utility.RandomMinMax(13, -45);
		SetSkill(SkillName.EvalInt, 65.0, 88.0);
		SetSkill(SkillName.Inscribe, 60.0, 83.0);
		SetSkill(SkillName.Magery, 64.0, 100.0);
		SetSkill(SkillName.Meditation, 60.0, 83.0);
		SetSkill(SkillName.MagicResist, 65.0, 88.0);
		SetSkill(SkillName.Wrestling, 36.0, 68.0);
	}

	public override void InitSbInfo()
	{
		_mSbInfos.Add(new SbMage());
	}

	public override VendorShoeType ShoeType => Utility.RandomBool() ? VendorShoeType.Shoes : VendorShoeType.Sandals;

	public override void InitOutfit()
	{
		base.InitOutfit();

		SetWearable(new Robe(Utility.RandomBlueHue()));
	}

	public Mage(Serial serial) : base(serial)
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
