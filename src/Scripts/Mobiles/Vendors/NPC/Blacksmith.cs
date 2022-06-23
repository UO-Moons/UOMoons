using Server.Engines.BulkOrders;
using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles;

public class Blacksmith : BaseVendor
{
	private readonly List<SbInfo> _mSbInfos = new();
	protected override List<SbInfo> SbInfos => _mSbInfos;

	public override NpcGuild NpcGuild => NpcGuild.BlacksmithsGuild;

	[Constructable]
	public Blacksmith() : base("the blacksmith")
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
		/*m_SBInfos.Add( new SBSmithTools() );

		m_SBInfos.Add( new SBMetalShields() );
		m_SBInfos.Add( new SBWoodenShields() );

		m_SBInfos.Add( new SBPlateArmor() );

		m_SBInfos.Add( new SBHelmetArmor() );
		m_SBInfos.Add( new SBChainmailArmor() );
		m_SBInfos.Add( new SBRingmailArmor() );
		m_SBInfos.Add( new SBAxeWeapon() );
		m_SBInfos.Add( new SBPoleArmWeapon() );
		m_SBInfos.Add( new SBRangedWeapon() );

		m_SBInfos.Add( new SBKnifeWeapon() );
		m_SBInfos.Add( new SBMaceWeapon() );
		m_SBInfos.Add( new SBSpearForkWeapon() );
		m_SBInfos.Add( new SBSwordWeapon() );*/

		_mSbInfos.Add(new SbBlacksmith());
		if (IsTokunoVendor)
		{
			_mSbInfos.Add(new SbseArmor());
			_mSbInfos.Add(new SbseWeapons());
		}
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

		if (item == null)
			SetWearable(new FullApron());

		SetWearable(new Bascinet());
		SetWearable(new SmithHammer());
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

		if (theirSkill >= 70.1 && (theirSkill - 40.0) / 300.0 > Utility.RandomDouble())
			return new LargeSmithBOD();

		return SmallSmithBOD.CreateRandomFor(from);

	}

	public override bool IsValidBulkOrder(Item item)
	{
		return item is SmallSmithBOD || item is LargeSmithBOD;
	}

	public override bool SupportsBulkOrders(Mobile from)
	{
		return from is PlayerMobile && from.Skills[SkillName.Blacksmith].Base > 0;
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

	public Blacksmith(Serial serial) : base(serial)
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
