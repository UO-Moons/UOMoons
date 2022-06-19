using Server.Items;
using System.Collections.Generic;

namespace Server.Mobiles;

public class SbStavesWeapon : SbInfo
{
	public override IShopSellInfo SellInfo { get; } = new InternalSellInfo();

	public override List<GenericBuyInfo> BuyInfo { get; } = new InternalBuyInfo();

	public class InternalBuyInfo : List<GenericBuyInfo>
	{
		public InternalBuyInfo()
		{
			Add(new GenericBuyInfo(typeof(BlackStaff), 22, 20, 0xDF1, 0));
			Add(new GenericBuyInfo(typeof(GnarledStaff), 16, 20, 0x13F8, 0));
			Add(new GenericBuyInfo(typeof(QuarterStaff), 19, 20, 0xE89, 0));
			Add(new GenericBuyInfo(typeof(ShepherdsCrook), 20, 20, 0xE81, 0));
		}
	}

	public class InternalSellInfo : GenericSellInfo
	{
		public InternalSellInfo()
		{
			Add(typeof(BlackStaff), 11);
			Add(typeof(GnarledStaff), 8);
			Add(typeof(QuarterStaff), 9);
			Add(typeof(ShepherdsCrook), 10);
		}
	}
}
