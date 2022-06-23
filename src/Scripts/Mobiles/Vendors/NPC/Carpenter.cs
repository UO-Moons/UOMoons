using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles;

public class Carpenter : BaseVendor
{
	private readonly List<SbInfo> _mSbInfos = new();
	protected override List<SbInfo> SbInfos => _mSbInfos;

	public override NpcGuild NpcGuild => NpcGuild.TinkersGuild;

	[Constructable]
	public Carpenter() : base("the carpenter")
	{
		Job = JobFragment.carpenter;
		Karma = Utility.RandomMinMax(13, -45);
		SetSkill(SkillName.Carpentry, 85.0, 100.0);
		SetSkill(SkillName.Lumberjacking, 60.0, 83.0);
	}

	public override void InitSbInfo()
	{
		_mSbInfos.Add(new SbStavesWeapon());
		_mSbInfos.Add(new SbCarpenter());
		_mSbInfos.Add(new SbWoodenShields());

		if (IsTokunoVendor)
			_mSbInfos.Add(new SbseCarpenter());
	}

	public override void InitOutfit()
	{
		base.InitOutfit();

		SetWearable(new HalfApron());
	}

	public Carpenter(Serial serial) : base(serial)
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
