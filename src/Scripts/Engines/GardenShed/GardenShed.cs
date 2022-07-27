using Server.ContextMenus;
using Server.Engines.VeteranRewards;
using Server.Gumps;
using Server.Network;
using System.Collections.Generic;

namespace Server.Items;

public class GardenShedComponent : AddonContainerComponent
{
	public override int LabelNumber => 1153492;  // garden shed

	public GardenShedComponent(int itemId)
		: base(itemId)
	{
		Weight = 0;
	}

	public override void OnDoubleClick(Mobile from)
	{
	}

	public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
	{
	}

	public GardenShedComponent(Serial serial)
		: base(serial)
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

public class GardenShedAddon : BaseAddonContainer
{
	public override BaseAddonContainerDeed Deed => new GardenShedDeed();
	public override int LabelNumber => 1153492;  // garden shed
	public override int DefaultGumpID => 0x10B;
	public override int DefaultDropSound => 0x42;
	private Point3D _mOffset;

	[CommandProperty(AccessLevel.GameMaster)]
	public BaseAddonContainer SecondContainer { get; private set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public Point3D Offset
	{
		get => _mOffset;
		private init => _mOffset = value;
	}

	[Constructable]
	public GardenShedAddon(bool east) : base(east ? 0x4BEB : 0x4BE7)
	{
		SecondContainer = new GardenShedBarrel(this, east);

		if (east) // East
		{
			AddComponent(new GardenShedComponent(0x4BEA), 0, 1, 0);
			AddComponent(new GardenShedComponent(0x4BEC), 0, -1, 0);
			AddComponent(new GardenShedComponent(0x4BF1), -1, 1, 0);
			AddComponent(new GardenShedComponent(0x4BF0), -1, 0, 0);
			AddComponent(new GardenShedComponent(0x4BEF), -2, -1, 0);
			AddComponent(new GardenShedComponent(0x4BEE), -1, -2, 0);
			AddComponent(new GardenShedComponent(0x4BF5), -2, -2, 0);
			AddComponent(new GardenShedComponent(0x4BF3), -2, 0, 0);
			AddComponent(new GardenShedComponent(0x4BEA), -2, 1, 0);
			Offset = new Point3D(0, -2, 0);
		}
		else    // South
		{
			AddComponent(new GardenShedComponent(0x4BE2), 2, -1, 0);
			AddComponent(new GardenShedComponent(0x4BE5), -1, -1, 0);
			AddComponent(new GardenShedComponent(0x4BDE), -1, -2, 0);
			AddComponent(new GardenShedComponent(0x4BE1), 2, -2, 0);
			AddComponent(new GardenShedComponent(0x4BE8), 1, 0, 0);
			AddComponent(new GardenShedComponent(0x4BE3), 1, -1, 0);
			AddComponent(new GardenShedComponent(0x4BE6), -1, 0, 0);
			AddComponent(new GardenShedComponent(0x4BE0), 1, -2, 0);
			AddComponent(new GardenShedComponent(0x4BE4), 0, -1, 0);
			Offset = new Point3D(2, 0, 0);
		}
	}

	public GardenShedAddon(Serial serial) : base(serial)
	{
	}

	public override void OnLocationChange(Point3D old)
	{
		base.OnLocationChange(old);

		SecondContainer?.MoveToWorld(new Point3D(X + _mOffset.X, Y + _mOffset.Y, Z + _mOffset.Z), Map);
	}

	public override void OnMapChange()
	{
		base.OnMapChange();

		if (Deleted)
			return;

		if (SecondContainer != null)
		{
			SecondContainer.Map = Map;
		}
	}

	public override void OnDelete()
	{
		SecondContainer?.Delete();

		base.OnDelete();
	}

	public override void OnChop(Mobile from)
	{
		if (!SecondContainer.IsSecure)
		{
			SecondContainer.DropItemsToGround();
			base.OnChop(from);
		}
		else
		{
			from.SendLocalizedMessage(1074870); // This item must be unlocked/unsecured before re-deeding it.
		}
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);

		writer.Write(SecondContainer);
		writer.Write(_mOffset);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		_ = reader.ReadInt();

		SecondContainer = reader.ReadItem() as BaseAddonContainer;
		_mOffset = reader.ReadPoint3D();
	}
}

[TypeAlias("Server.Items.GardenShedAddonSecond")]
public class GardenShedBarrel : BaseAddonContainer
{
	private GardenShedAddon m_MMainContainer;

	[Constructable]
	public GardenShedBarrel(GardenShedAddon container, bool east)
		: base(east ? 0x4BED : 0x4BE9)
	{
		m_MMainContainer = container;
	}

	public GardenShedBarrel(Serial serial)
		: base(serial)
	{
	}

	public override BaseAddonContainerDeed Deed => m_MMainContainer.Deed;
	public override int LabelNumber => 1153492;  // garden shed
	public override int DefaultGumpID => 0x3E;
	public override int DefaultDropSound => 0x42;

	public override void OnLocationChange(Point3D old)
	{
		m_MMainContainer.Location = new Point3D(X - m_MMainContainer.Offset.X, Y - m_MMainContainer.Offset.Y, Z - m_MMainContainer.Offset.Z);
	}

	public override void OnMapChange()
	{
		if (m_MMainContainer != null)
			m_MMainContainer.Map = Map;
	}

	public override void OnAfterDelete()
	{
		base.OnAfterDelete();

		m_MMainContainer?.Delete();
	}

	public override void OnChop(Mobile from)
	{
		if (m_MMainContainer == null)
			return;

		if (!m_MMainContainer.IsSecure)
		{
			m_MMainContainer.DropItemsToGround();
			base.OnChop(from);
		}
		else
		{
			from.SendLocalizedMessage(1074870); // This item must be unlocked/unsecured before re-deeding it.
		}
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);

		writer.Write(m_MMainContainer);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		_ = reader.ReadInt();

		m_MMainContainer = reader.ReadItem() as GardenShedAddon;
	}
}

public class GardenShedDeed : BaseAddonContainerDeed, IRewardItem
{
	public override BaseAddonContainer Addon => new GardenShedAddon(_mEast);
	public override int LabelNumber => 1153491;  // Garden Shed Deed

	[CommandProperty(AccessLevel.GameMaster)]
	public bool IsRewardItem { get; set; }

	private bool _mEast;

	[Constructable]
	public GardenShedDeed()
	{
		LootType = LootType.Blessed;
	}

	public GardenShedDeed(Serial serial) : base(serial)
	{
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (IsChildOf(from.Backpack))
		{
			_ = from.CloseGump(typeof(InternalGump));
			_ = from.SendGump(new InternalGump(this));
		}
		else
			from.SendLocalizedMessage(1062334); // This item must be in your backpack to be used.
	}

	public override void GetProperties(ObjectPropertyList list)
	{
		base.GetProperties(list);

		if (IsRewardItem)
			list.Add(1113805); // 15th Year Veteran Reward
	}

	private void SendTarget(Mobile m)
	{
		base.OnDoubleClick(m);
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.WriteEncodedInt(0);
		writer.Write(IsRewardItem);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadEncodedInt();
		IsRewardItem = reader.ReadBool();
	}

	private class InternalGump : Gump
	{
		private readonly GardenShedDeed _mDeed;

		public InternalGump(GardenShedDeed deed) : base(60, 36)
		{
			_mDeed = deed;

			AddPage(0);

			AddBackground(0, 0, 273, 324, 0x13BE);
			AddImageTiled(10, 10, 253, 20, 0xA40);
			AddImageTiled(10, 40, 253, 244, 0xA40);
			AddImageTiled(10, 294, 253, 20, 0xA40);
			AddAlphaRegion(10, 10, 253, 304);
			AddButton(10, 294, 0xFB1, 0xFB2, 0, GumpButtonType.Reply, 0);
			AddHtmlLocalized(45, 296, 450, 20, 1060051, 0x7FFF, false, false); // CANCEL
			AddHtmlLocalized(14, 12, 273, 20, 1071175, 0x7FFF, false, false); // Please select your vanity position.

			AddPage(1);

			AddButton(19, 49, 0x845, 0x846, 1, GumpButtonType.Reply, 0);
			AddHtmlLocalized(44, 47, 213, 20, 1153490, 0x7FFF, false, false); // Garden Shed (South)
			AddButton(19, 73, 0x845, 0x846, 2, GumpButtonType.Reply, 0);
			AddHtmlLocalized(44, 71, 213, 20, 1153489, 0x7FFF, false, false); // Garden Shed (East)
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			if (_mDeed == null || _mDeed.Deleted || info.ButtonID == 0)
				return;

			_mDeed._mEast = info.ButtonID != 1;
			_mDeed.SendTarget(sender.Mobile);
		}
	}
}
