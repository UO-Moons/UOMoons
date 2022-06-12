using Server.Engines.Craft;
using System;

namespace Server.Items;

public abstract class BaseHat : BaseClothing, IShipwreckedItem
{
	[CommandProperty(AccessLevel.GameMaster)]
	public bool IsShipwreckedItem { get; set; }

	public BaseHat(int itemID) : this(itemID, 0)
	{
	}

	public BaseHat(int itemID, int hue) : base(itemID, Layer.Helm, hue)
	{
	}

	public BaseHat(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
		writer.Write(IsShipwreckedItem);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		int version = reader.ReadInt();
		switch (version)
		{
			case 0:
				{
					IsShipwreckedItem = reader.ReadBool();
					break;
				}
		}
	}

	public override void AddNameProperties(ObjectPropertyList list)
	{
		base.AddNameProperties(list);
		if (IsShipwreckedItem)
			list.Add(1041645); // recovered from a shipwreck
	}

	public override int OnCraft(int quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, ITool tool, CraftItem craftItem, int resHue)
	{
		Quality = ItemQuality.Normal;

		if (Quality == ItemQuality.Exceptional)
			DistributeBonuses((tool is BaseRunicTool ? 6 : (Core.SE ? 15 : 14)));   //BLAME OSI. (We can't confirm it's an OSI bug yet.)

		return base.OnCraft(quality, makersMark, from, craftSystem, typeRes, tool, craftItem, resHue);
	}
}
