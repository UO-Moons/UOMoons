using Server.Items;
using System.Collections.Generic;

namespace Server.Mobiles;

public class SbSamurai : SbInfo
{
	public override IShopSellInfo SellInfo { get; } = new InternalSellInfo();

	public override List<GenericBuyInfo> BuyInfo { get; } = new InternalBuyInfo();

	public class InternalBuyInfo : List<GenericBuyInfo>
	{
		public InternalBuyInfo()
		{
			Add(new GenericBuyInfo(typeof(BookOfBushido), 280, 20, 0x238C, 0));
		}
	}

	public class InternalSellInfo : GenericSellInfo
	{
	}
}
