using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles;

public class Armorer : BaseVendor
{
	private readonly List<SbInfo> _mSbInfos = new();
	protected override List<SbInfo> SbInfos => _mSbInfos;

	[Constructable]
	public Armorer() : base("the armorer")
	{
		Job = JobFragment.armourer;
		Karma = Utility.RandomMinMax(13, -45);
		SetSkill(SkillName.ArmsLore, 64.0, 100.0);
		SetSkill(SkillName.Blacksmith, 60.0, 83.0);
	}

	public override void InitSbInfo()
	{
		switch (Utility.Random(4))
		{
			case 0:
			{
				_mSbInfos.Add(new SbLeatherArmor());
				_mSbInfos.Add(new SbStuddedArmor());
				_mSbInfos.Add(new SbMetalShields());
				_mSbInfos.Add(new SbPlateArmor());
				_mSbInfos.Add(new SbHelmetArmor());
				_mSbInfos.Add(new SbChainmailArmor());
				_mSbInfos.Add(new SbRingmailArmor());
				break;
			}
			case 1:
			{
				_mSbInfos.Add(new SbStuddedArmor());
				_mSbInfos.Add(new SbLeatherArmor());
				_mSbInfos.Add(new SbMetalShields());
				_mSbInfos.Add(new SbHelmetArmor());
				break;
			}
			case 2:
			{
				_mSbInfos.Add(new SbMetalShields());
				_mSbInfos.Add(new SbPlateArmor());
				_mSbInfos.Add(new SbHelmetArmor());
				_mSbInfos.Add(new SbChainmailArmor());
				_mSbInfos.Add(new SbRingmailArmor());
				break;
			}
			case 3:
			{
				_mSbInfos.Add(new SbMetalShields());
				_mSbInfos.Add(new SbHelmetArmor());
				break;
			}
		}
		if (IsTokunoVendor)
		{
			_mSbInfos.Add(new SbseLeatherArmor());
			_mSbInfos.Add(new SbseArmor());
		}
	}

	public override VendorShoeType ShoeType => VendorShoeType.Boots;

	public override void InitOutfit()
	{
		base.InitOutfit();

		AddItem(new HalfApron(Utility.RandomYellowHue()));
		AddItem(new Bascinet());
	}

	public Armorer(Serial serial) : base(serial)
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
