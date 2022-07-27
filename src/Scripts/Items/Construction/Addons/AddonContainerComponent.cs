using Server.ContextMenus;
using System.Collections.Generic;

namespace Server.Items;

public class AddonContainerComponent : BaseItem, IChopable
{
	public virtual bool NeedsWall => false;
	public virtual Point3D WallPosition => Point3D.Zero;

	private Point3D m_Offset;

	[CommandProperty(AccessLevel.GameMaster)]
	public BaseAddonContainer Addon { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public Point3D Offset
	{
		get => m_Offset;
		set => m_Offset = value;
	}

	[Hue, CommandProperty(AccessLevel.GameMaster)]
	public override int Hue
	{
		get => base.Hue;
		set
		{
			base.Hue = value;

			if (Addon is { ShareHue: true })
				Addon.Hue = value;
		}
	}

	[Constructable]
	public AddonContainerComponent(int itemId) : base(itemId)
	{
		Movable = false;

		AddonComponent.ApplyLightTo(this);
	}

	public AddonContainerComponent(Serial serial) : base(serial)
	{
	}

	public override bool OnDragDrop(Mobile from, Item dropped)
	{
		return Addon != null && Addon.OnDragDrop(from, dropped);
	}

	public override void OnDoubleClick(Mobile from)
	{
		Addon?.OnComponentUsed(this, from);
	}

	public override void OnLocationChange(Point3D old)
	{
		if (Addon != null)
			Addon.Location = new Point3D(X - m_Offset.X, Y - m_Offset.Y, Z - m_Offset.Z);
	}

	public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
	{
		Addon?.GetContextMenuEntries(from, list);
	}

	public override void OnMapChange()
	{
		if (Addon != null)
			Addon.Map = Map;
	}

	public override void OnAfterDelete()
	{
		base.OnAfterDelete();

		Addon?.Delete();
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version

		writer.Write(Addon);
		writer.Write(m_Offset);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadInt();

		Addon = reader.ReadItem() as BaseAddonContainer;
		m_Offset = reader.ReadPoint3D();

		Addon?.OnComponentLoaded(this);

		AddonComponent.ApplyLightTo(this);
	}

	public virtual void OnChop(Mobile from)
	{
		if (Addon != null && from.InRange(GetWorldLocation(), 3))
			Addon.OnChop(from);
		else
			from.SendLocalizedMessage(500446); // That is too far away.
	}
}

public class LocalizedContainerComponent : AddonContainerComponent
{
	private int m_LabelNumber;

	public override int LabelNumber => m_LabelNumber > 0 ? m_LabelNumber : base.LabelNumber;

	public LocalizedContainerComponent(int itemId, int labelNumber) : base(itemId)
	{
		m_LabelNumber = labelNumber;
	}

	public LocalizedContainerComponent(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version

		writer.Write(m_LabelNumber);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadInt();

		m_LabelNumber = reader.ReadInt();
	}
}
