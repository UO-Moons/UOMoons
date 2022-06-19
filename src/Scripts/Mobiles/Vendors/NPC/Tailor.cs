using Server.Engines.BulkOrders;
using System;
using System.Collections.Generic;

namespace Server.Mobiles;

public class Tailor : BaseVendor
{
	private readonly List<SbInfo> _mSbInfos = new();
	protected override List<SbInfo> SbInfos => _mSbInfos;

	public override NpcGuild NpcGuild => NpcGuild.TailorsGuild;

	[Constructable]
	public Tailor() : base("the tailor")
	{
		Job = JobFragment.tailor;
		Karma = Utility.RandomMinMax(13, -45);
		SetSkill(SkillName.Tailoring, 64.0, 100.0);
	}

	public override void InitSbInfo()
	{
		_mSbInfos.Add(new SbTailor());
	}

	public override VendorShoeType ShoeType => Utility.RandomBool() ? VendorShoeType.Sandals : VendorShoeType.Shoes;

	#region Bulk Orders
	public override Item CreateBulkOrder(Mobile from, bool fromContextMenu)
	{
		if (from is not PlayerMobile pm || pm.NextTailorBulkOrder != TimeSpan.Zero ||
		    (!fromContextMenu && !(0.2 > Utility.RandomDouble()))) return null;
		double theirSkill = pm.Skills[SkillName.Tailoring].Base;

		pm.NextTailorBulkOrder = theirSkill switch
		{
			>= 70.1 => TimeSpan.FromHours(6.0),
			>= 50.1 => TimeSpan.FromHours(2.0),
			_ => TimeSpan.FromHours(1.0)
		};

		if (theirSkill >= 70.1 && (theirSkill - 40.0) / 300.0 > Utility.RandomDouble())
			return new LargeTailorBOD();

		return SmallTailorBOD.CreateRandomFor(from);

	}

	public override bool IsValidBulkOrder(Item item)
	{
		return item is SmallTailorBOD or LargeTailorBOD;
	}

	public override bool SupportsBulkOrders(Mobile from)
	{
		return from is PlayerMobile && from.Skills[SkillName.Tailoring].Base > 0;
	}

	public override TimeSpan GetNextBulkOrder(Mobile from)
	{
		if (from is PlayerMobile mobile)
			return mobile.NextTailorBulkOrder;

		return TimeSpan.Zero;
	}

	public override void OnSuccessfulBulkOrderReceive(Mobile from)
	{
		if (Core.SE && from is PlayerMobile mobile)
			mobile.NextTailorBulkOrder = TimeSpan.Zero;
	}
	#endregion

	public Tailor(Serial serial) : base(serial)
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
