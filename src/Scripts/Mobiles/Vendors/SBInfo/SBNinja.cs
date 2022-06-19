using Server.Items;
using System.Collections.Generic;

namespace Server.Mobiles;

public class SbNinja : SbInfo
{
	public override IShopSellInfo SellInfo { get; } = new InternalSellInfo();

	public override List<GenericBuyInfo> BuyInfo { get; } = new InternalBuyInfo();

	public class InternalBuyInfo : List<GenericBuyInfo>
	{
		public InternalBuyInfo()
		{
			Add(new GenericBuyInfo(typeof(BookOfNinjitsu), 335, 20, 0x23A0, 0));
		}
	}

	public class InternalSellInfo : GenericSellInfo
	{
	}
}
