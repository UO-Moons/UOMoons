using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles;

public class Alchemist : BaseVendor
{
	private readonly List<SbInfo> _mSbInfos = new();
	protected override List<SbInfo> SbInfos => _mSbInfos;

	public override NpcGuild NpcGuild => NpcGuild.MagesGuild;

	[Constructable]
	public Alchemist() : base("the alchemist")
	{
		Job = JobFragment.alchemist;
		Karma = Utility.RandomMinMax(13, -45);
		SetSkill(SkillName.Alchemy, 85.0, 100.0);
		SetSkill(SkillName.TasteID, 65.0, 88.0);
	}

	public override void InitSbInfo()
	{
		_mSbInfos.Add(new SbAlchemist());
	}

	public override VendorShoeType ShoeType => Utility.RandomBool() ? VendorShoeType.Shoes : VendorShoeType.Sandals;

	public override void InitOutfit()
	{
		base.InitOutfit();

		AddItem(new Robe(Utility.RandomPinkHue()));
	}

	public Alchemist(Serial serial) : base(serial)
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
