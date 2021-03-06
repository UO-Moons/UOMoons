using Server.Items;
using System.Collections.Generic;

namespace Server.Mobiles;

public class HolyMage : BaseVendor
{
	private readonly List<SbInfo> _mSbInfos = new();
	protected override List<SbInfo> SbInfos => _mSbInfos;

	[Constructable]
	public HolyMage() : base("the Holy Mage")
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
		_mSbInfos.Add(new SbHolyMage());
	}

	public Item ApplyHue(Item item, int hue)
	{
		item.Hue = hue;

		return item;
	}

	public override void InitOutfit()
	{
		SetWearable(new Robe(), 0x47E);
		SetWearable(ApplyHue(new ThighBoots(), 0x47E));
		SetWearable(ApplyHue(new BlackStaff(), 0x47E));

		if (Female)
		{
			SetWearable(ApplyHue(new LeatherGloves(), 0x47E));
			SetWearable(ApplyHue(new GoldNecklace(), 0x47E));
		}
		else
		{
			SetWearable(ApplyHue(new PlateGloves(), 0x47E));
			SetWearable(ApplyHue(new PlateGorget(), 0x47E));
		}

		HairItemID = Utility.Random(Female ? 2 : 1) switch
		{
			0 => 0x203C,
			1 => 0x203D,
			_ => HairItemID
		};

		HairHue = 0x47E;

		PackGold(100, 200);
	}

	public HolyMage(Serial serial) : base(serial)
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
