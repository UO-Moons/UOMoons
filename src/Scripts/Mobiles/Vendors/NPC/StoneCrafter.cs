using System.Collections.Generic;

namespace Server.Mobiles;

[TypeAlias("Server.Mobiles.GargoyleStonecrafter")]
public class StoneCrafter : BaseVendor
{
	private readonly List<SbInfo> _mSbInfos = new();
	protected override List<SbInfo> SbInfos => _mSbInfos;

	public override NpcGuild NpcGuild => NpcGuild.TinkersGuild;

	[Constructable]
	public StoneCrafter() : base("the stone crafter")
	{
		Job = JobFragment.sculptor;
		Karma = Utility.RandomMinMax(13, -45);
		SetSkill(SkillName.Carpentry, 85.0, 100.0);
	}

	public override void InitSbInfo()
	{
		_mSbInfos.Add(new SbStoneCrafter());
		_mSbInfos.Add(new SbStavesWeapon());
		_mSbInfos.Add(new SbCarpenter());
		_mSbInfos.Add(new SbWoodenShields());
	}

	public StoneCrafter(Serial serial) : base(serial)
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
