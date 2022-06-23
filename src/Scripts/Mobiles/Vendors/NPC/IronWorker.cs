using Server.Items;
using System.Collections.Generic;

namespace Server.Mobiles;

public class IronWorker : BaseVendor
{
	private readonly List<SbInfo> _mSbInfos = new();
	protected override List<SbInfo> SbInfos => _mSbInfos;

	[Constructable]
	public IronWorker() : base("the iron worker")
	{
		Job = JobFragment.blacksmith;
		Karma = Utility.RandomMinMax(13, -45);
		SetSkill(SkillName.ArmsLore, 36.0, 68.0);
		SetSkill(SkillName.Blacksmith, 65.0, 88.0);
		SetSkill(SkillName.Fencing, 60.0, 83.0);
		SetSkill(SkillName.Macing, 61.0, 93.0);
		SetSkill(SkillName.Swords, 60.0, 83.0);
		SetSkill(SkillName.Tactics, 60.0, 83.0);
		SetSkill(SkillName.Parry, 61.0, 93.0);
	}

	public override void InitSbInfo()
	{
		_mSbInfos.Add(new SbAxeWeapon());
		_mSbInfos.Add(new SbKnifeWeapon());
		_mSbInfos.Add(new SbMaceWeapon());
		_mSbInfos.Add(new SbSmithTools());
		_mSbInfos.Add(new SbPoleArmWeapon());
		_mSbInfos.Add(new SbSpearForkWeapon());
		_mSbInfos.Add(new SbSwordWeapon());

		_mSbInfos.Add(new SbMetalShields());

		_mSbInfos.Add(new SbHelmetArmor());
		_mSbInfos.Add(new SbPlateArmor());
		_mSbInfos.Add(new SbChainmailArmor());
		_mSbInfos.Add(new SbRingmailArmor());
		_mSbInfos.Add(new SbStuddedArmor());
		_mSbInfos.Add(new SbLeatherArmor());
	}

	public override VendorShoeType ShoeType => VendorShoeType.None;

	public override void InitOutfit()
	{
		base.InitOutfit();

		Item item = Utility.RandomBool() ? null : new RingmailChest();

		if (item != null && !EquipItem(item))
		{
			item.Delete();
			item = null;
		}

		switch (Utility.Random(3))
		{
			case 0:
			case 1: SetWearable(new JesterHat(Utility.RandomBrightHue())); break;
			case 2: SetWearable(new Bandana(Utility.RandomBrightHue())); break;
		}

		if (item == null)
			SetWearable(new FullApron(Utility.RandomBrightHue()));

		SetWearable(new Bascinet());
		SetWearable(new SmithHammer());

		item = FindItemOnLayer(Layer.Pants);

		if (item != null)
			item.Hue = Utility.RandomBrightHue();

		item = FindItemOnLayer(Layer.OuterLegs);

		if (item != null)
			item.Hue = Utility.RandomBrightHue();

		item = FindItemOnLayer(Layer.InnerLegs);

		if (item != null)
			item.Hue = Utility.RandomBrightHue();

		item = FindItemOnLayer(Layer.OuterTorso);

		if (item != null)
			item.Hue = Utility.RandomBrightHue();

		item = FindItemOnLayer(Layer.InnerTorso);

		if (item != null)
			item.Hue = Utility.RandomBrightHue();

		item = FindItemOnLayer(Layer.Shirt);

		if (item != null)
			item.Hue = Utility.RandomBrightHue();
	}

	public IronWorker(Serial serial) : base(serial)
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
