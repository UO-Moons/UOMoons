using Server.Items;
using System.Collections.Generic;

namespace Server.Mobiles;

public class KeeperOfChivalry : BaseVendor
{
	private readonly List<SbInfo> _mSbInfos = new();
	protected override List<SbInfo> SbInfos => _mSbInfos;

	[Constructable]
	public KeeperOfChivalry() : base("the Keeper of Chivalry")
	{
		Job = JobFragment.paladin;
		Karma = Utility.RandomMinMax(13, -45);
		SetSkill(SkillName.Fencing, 75.0, 85.0);
		SetSkill(SkillName.Macing, 75.0, 85.0);
		SetSkill(SkillName.Swords, 75.0, 85.0);
		SetSkill(SkillName.Chivalry, 100.0);
	}

	public override void InitSbInfo()
	{
		_mSbInfos.Add(new SbKeeperOfChivalry());
	}

	public override void InitOutfit()
	{
		SetWearable(new PlateArms());
		SetWearable(new PlateChest());
		SetWearable(new PlateGloves());
		SetWearable(new StuddedGorget());
		SetWearable(new PlateLegs());

		switch (Utility.Random(4))
		{
			case 0: SetWearable(new PlateHelm()); break;
			case 1: SetWearable(new NorseHelm()); break;
			case 2: SetWearable(new CloseHelm()); break;
			case 3: SetWearable(new Helmet()); break;
		}

		switch (Utility.Random(3))
		{
			case 0: SetWearable(new BodySash(0x482)); break;
			case 1: SetWearable(new Doublet(0x482)); break;
			case 2: SetWearable(new Tunic(0x482)); break;
		}

		SetWearable(new Broadsword());

		Item shield = new MetalKiteShield
		{
			Hue = Utility.RandomNondyedHue()
		};

		SetWearable(shield);

		switch (Utility.Random(2))
		{
			case 0: SetWearable(new Boots()); break;
			case 1: SetWearable(new ThighBoots()); break;
		}

		PackGold(100, 200);
	}

	public KeeperOfChivalry(Serial serial) : base(serial)
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
