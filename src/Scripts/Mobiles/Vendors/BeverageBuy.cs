using Server.Items;
using System;

namespace Server.Mobiles;

public class BeverageBuyInfo : GenericBuyInfo
{
	private readonly BeverageType _mContent;

	public override bool CanCacheDisplay => false;

	public BeverageBuyInfo(Type type, BeverageType content, int price, int amount, int itemId, int hue) : this(null, type, content, price, amount, itemId, hue)
	{
	}

	public BeverageBuyInfo(string name, Type type, BeverageType content, int price, int amount, int itemId, int hue) : base(name, type, price, amount, itemId, hue)
	{
		_mContent = content;

		if (type == typeof(Pitcher))
			Name = (1048128 + (int)content).ToString();
		else if (type == typeof(BeverageBottle))
			Name = (1042959 + (int)content).ToString();
		else if (type == typeof(Jug))
			Name = (1042965 + (int)content).ToString();
	}

	public override IEntity GetEntity()
	{
		return (IEntity)Activator.CreateInstance(Type, _mContent);
	}
}
