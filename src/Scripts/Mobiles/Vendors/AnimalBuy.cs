using System;

namespace Server.Mobiles;

public class AnimalBuyInfo : GenericBuyInfo
{
	public AnimalBuyInfo(int controlSlots, Type type, int price, int amount, int itemId, int hue) : this(controlSlots, null, type, price, amount, itemId, hue)
	{
	}

	public AnimalBuyInfo(int controlSlots, string name, Type type, int price, int amount, int itemId, int hue) : base(name, type, price, amount, itemId, hue)
	{
		ControlSlots = controlSlots;
	}

	public override int ControlSlots { get; }
}
