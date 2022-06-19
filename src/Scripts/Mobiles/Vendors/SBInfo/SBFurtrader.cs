using Server.Items;
using System.Collections.Generic;

namespace Server.Mobiles;

public class SbFurtrader : SbInfo
{
	public override IShopSellInfo SellInfo { get; } = new InternalSellInfo();

	public override List<GenericBuyInfo> BuyInfo { get; } = new InternalBuyInfo();

	public class InternalBuyInfo : List<GenericBuyInfo>
	{
		public InternalBuyInfo()
		{
			Add(new GenericBuyInfo(typeof(Hides), 3, 40, 0x1079, 0));
		}
	}

	public class InternalSellInfo : GenericSellInfo
	{
		public InternalSellInfo()
		{
			Add(typeof(Hides), 2);
		}
	}
}
