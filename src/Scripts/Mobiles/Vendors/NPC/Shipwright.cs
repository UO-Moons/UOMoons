using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles;

public class Shipwright : BaseVendor
{
	private readonly List<SbInfo> _mSbInfos = new();
	protected override List<SbInfo> SbInfos => _mSbInfos;

	[Constructable]
	public Shipwright() : base("the shipwright")
	{
		Job = JobFragment.shipwright;
		Karma = Utility.RandomMinMax(13, -45);
		SetSkill(SkillName.Carpentry, 60.0, 83.0);
		SetSkill(SkillName.Macing, 36.0, 68.0);
		BankAccount = BankRestockAmount = 0x40000000;
	}

	public override void InitSbInfo()
	{
		_mSbInfos.Add(new SbShipwright());
	}

	public override void InitOutfit()
	{
		base.InitOutfit();

		AddItem(new SmithHammer());
	}

	public Shipwright(Serial serial) : base(serial)
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
