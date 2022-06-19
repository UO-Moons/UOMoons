using Server.ContextMenus;
using Server.Items;
using System.Collections.Generic;

namespace Server.Mobiles;

public class TinkerGuildmaster : BaseGuildmaster
{
	public override NpcGuild NpcGuild => NpcGuild.TinkersGuild;

	[Constructable]
	public TinkerGuildmaster() : base("tinker")
	{
		SetSkill(SkillName.Lockpicking, 65.0, 88.0);
		SetSkill(SkillName.Tinkering, 90.0, 100.0);
		SetSkill(SkillName.RemoveTrap, 85.0, 100.0);
	}

	public TinkerGuildmaster(Serial serial) : base(serial)
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

	public override void AddCustomContextEntries(Mobile from, List<ContextMenuEntry> list)
	{
		if (Core.ML && from.Alive)
		{
			RechargeEntry entry = new(from, this);

			if (WeaponEngravingTool.Find(from) == null)
				entry.Enabled = false;

			list.Add(entry);
		}

		base.AddCustomContextEntries(from, list);
	}

	private class RechargeEntry : ContextMenuEntry
	{
		private readonly Mobile _mFrom;
		private readonly Mobile _mVendor;

		public RechargeEntry(Mobile from, Mobile vendor) : base(6271, 6)
		{
			_mFrom = from;
			_mVendor = vendor;
		}

		public override void OnClick()
		{
			if (!Core.ML || _mVendor == null || _mVendor.Deleted)
				return;

			WeaponEngravingTool tool = WeaponEngravingTool.Find(_mFrom);

			if (tool is {UsesRemaining: <= 0})
			{
				if (Banker.GetBalance(_mFrom) >= 100000)
					_mFrom.SendGump(new WeaponEngravingTool.ConfirmGump(tool, _mVendor));
				else
					_mVendor.Say(1076167); // You need a 100,000 gold and a blue diamond to recharge the weapon engraver.
			}
			else
				_mVendor.Say(1076164); // I can only help with this if you are carrying an engraving tool that needs repair.
		}
	}
}
