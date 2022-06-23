using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles;

[TypeAlias("Server.Mobiles.Bower")]
public class Bowyer : BaseVendor
{
	private readonly List<SbInfo> _mSbInfos = new();
	protected override List<SbInfo> SbInfos => _mSbInfos;

	[Constructable]
	public Bowyer() : base("the bowyer")
	{
		Job = JobFragment.bowyer;
		Karma = Utility.RandomMinMax(13, -45);
		SetSkill(SkillName.Fletching, 80.0, 100.0);
		SetSkill(SkillName.Archery, 80.0, 100.0);
	}

	public override VendorShoeType ShoeType => Female ? VendorShoeType.ThighBoots : VendorShoeType.Boots;

	public override int GetShoeHue()
	{
		return 0;
	}

	public override void InitOutfit()
	{
		base.InitOutfit();

		SetWearable(new Bow());
		SetWearable(new LeatherGorget());
	}

	public override void InitSbInfo()
	{
		_mSbInfos.Add(new SbBowyer());
		_mSbInfos.Add(new SbRangedWeapon());

		if (IsTokunoVendor)
			_mSbInfos.Add(new SbseBowyer());
	}

	public Bowyer(Serial serial) : base(serial)
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
