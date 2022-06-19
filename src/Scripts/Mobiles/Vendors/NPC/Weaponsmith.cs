using Server.Engines.BulkOrders;
using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles;

public class Weaponsmith : BaseVendor
{
	private readonly List<SbInfo> _mSbInfos = new();
	protected override List<SbInfo> SbInfos => _mSbInfos;

	[Constructable]
	public Weaponsmith() : base("the weaponsmith")
	{
		Job = JobFragment.weaponsmith;
		Karma = Utility.RandomMinMax(13, -45);
		SetSkill(SkillName.ArmsLore, 64.0, 100.0);
		SetSkill(SkillName.Blacksmith, 65.0, 88.0);
		SetSkill(SkillName.Fencing, 45.0, 68.0);
		SetSkill(SkillName.Macing, 45.0, 68.0);
		SetSkill(SkillName.Swords, 45.0, 68.0);
		SetSkill(SkillName.Tactics, 36.0, 68.0);
	}

	public override void InitSbInfo()
	{
		_mSbInfos.Add(new SbWeaponSmith());

		if (IsTokunoVendor)
			_mSbInfos.Add(new SbseWeapons());
	}

	public override VendorShoeType ShoeType => Utility.RandomBool() ? VendorShoeType.Boots : VendorShoeType.ThighBoots;

	public override int GetShoeHue()
	{
		return 0;
	}

	public override void InitOutfit()
	{
		base.InitOutfit();

		AddItem(new HalfApron());
	}

	#region Bulk Orders
	public override Item CreateBulkOrder(Mobile from, bool fromContextMenu)
	{
		if (from is not PlayerMobile pm || pm.NextSmithBulkOrder != TimeSpan.Zero ||
		    (!fromContextMenu && !(0.2 > Utility.RandomDouble()))) return null;
		double theirSkill = pm.Skills[SkillName.Blacksmith].Base;

		pm.NextSmithBulkOrder = theirSkill switch
		{
			>= 70.1 => TimeSpan.FromHours(6.0),
			>= 50.1 => TimeSpan.FromHours(2.0),
			_ => TimeSpan.FromHours(1.0)
		};

		if (theirSkill >= 70.1 && ((theirSkill - 40.0) / 300.0) > Utility.RandomDouble())
			return new LargeSmithBOD();

		return SmallSmithBOD.CreateRandomFor(from);

	}

	public override bool IsValidBulkOrder(Item item)
	{
		return item is SmallSmithBOD or LargeSmithBOD;
	}

	public override bool SupportsBulkOrders(Mobile from)
	{
		return from is PlayerMobile && Core.AOS && from.Skills[SkillName.Blacksmith].Base > 0;
	}

	public override TimeSpan GetNextBulkOrder(Mobile from)
	{
		if (from is PlayerMobile mobile)
			return mobile.NextSmithBulkOrder;

		return TimeSpan.Zero;
	}

	public override void OnSuccessfulBulkOrderReceive(Mobile from)
	{
		if (Core.SE && from is PlayerMobile mobile)
			mobile.NextSmithBulkOrder = TimeSpan.Zero;
	}
	#endregion

	public Weaponsmith(Serial serial) : base(serial)
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
