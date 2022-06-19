using System.Collections.Generic;

namespace Server.Mobiles;

public abstract class SbInfo
{
	public static readonly List<SbInfo> Empty = new();

	public abstract IShopSellInfo SellInfo { get; }
	public abstract List<GenericBuyInfo> BuyInfo { get; }
}
