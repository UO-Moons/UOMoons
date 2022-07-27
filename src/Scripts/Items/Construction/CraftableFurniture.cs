using Server.Engines.Craft;
using System;

namespace Server.Items;

public class CraftableFurniture : BaseItem, IResource, IQuality
{
	public virtual bool ShowCrafterName => true;

	public CraftableFurniture(int itemId)
		: base(itemId)
	{
	}

	public CraftableFurniture(Serial serial)
		: base(serial)
	{
	}

	public override void AddWeightProperty(ObjectPropertyList list)
	{
		base.AddWeightProperty(list);

		if (ShowCrafterName && Crafter != null)
		{
			list.Add(1050043, Crafter.TitleName); // crafted by ~1_NAME~
		}

		if (Quality == ItemQuality.Exceptional)
		{
			list.Add(1060636); // exceptional
		}
	}

	public override void AddCraftedProperties(ObjectPropertyList list)
	{
		CraftResourceInfo info = CraftResources.IsStandard(Resource) ? null : CraftResources.GetInfo(Resource);

		if (info is { Number: > 0 })
		{
			list.Add(info.Number);
		}
	}

	public override void OnSingleClick(Mobile from)
	{
		base.OnSingleClick(from);

		if (Crafter != null)
		{
			LabelTo(from, 1050043, Crafter.TitleName); // crafted by ~1_NAME~
		}
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.PeekInt();
	}

	#region ICraftable
	public virtual int OnCraft(int quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, ITool tool, CraftItem craftItem, int resHue)
	{
		Quality = (ItemQuality)quality;

		if (makersMark)
		{
			Crafter = from;
		}

		PlayerConstructed = true;

		Type resourceType = typeRes ?? craftItem.Resources.GetAt(0).ItemType;

		Resource = CraftResources.GetFromType(resourceType);

		return quality;
	}
	#endregion
}
