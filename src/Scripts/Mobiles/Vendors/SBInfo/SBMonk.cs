using Server.Items;
using System.Collections.Generic;

namespace Server.Mobiles;

public class SbMonk : SbInfo
{
	public override IShopSellInfo SellInfo { get; } = new InternalSellInfo();

	public override List<GenericBuyInfo> BuyInfo { get; } = new InternalBuyInfo();

	public class InternalBuyInfo : List<GenericBuyInfo>
	{
		public InternalBuyInfo()
		{
			if (Core.AOS) Add(new GenericBuyInfo(typeof(MonkRobe), 136, 20, 0x2687, 0x21E));
		}
	}

	public class InternalSellInfo : GenericSellInfo
	{
	}
}
