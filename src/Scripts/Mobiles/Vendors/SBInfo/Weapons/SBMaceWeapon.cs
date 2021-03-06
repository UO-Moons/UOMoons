using Server.Items;
using System.Collections.Generic;

namespace Server.Mobiles;

public class SbMaceWeapon : SbInfo
{
	public override IShopSellInfo SellInfo { get; } = new InternalSellInfo();

	public override List<GenericBuyInfo> BuyInfo { get; } = new InternalBuyInfo();

	public class InternalBuyInfo : List<GenericBuyInfo>
	{
		public InternalBuyInfo()
		{
			Add(new GenericBuyInfo(typeof(HammerPick), 26, 20, 0x143D, 0));
			Add(new GenericBuyInfo(typeof(Club), 16, 20, 0x13B4, 0));
			Add(new GenericBuyInfo(typeof(Mace), 28, 20, 0xF5C, 0));
			Add(new GenericBuyInfo(typeof(Maul), 21, 20, 0x143B, 0));
			Add(new GenericBuyInfo(typeof(WarHammer), 25, 20, 0x1439, 0));
			Add(new GenericBuyInfo(typeof(WarMace), 31, 20, 0x1407, 0));
		}
	}

	public class InternalSellInfo : GenericSellInfo
	{
		public InternalSellInfo()
		{
			Add(typeof(Club), 8);
			Add(typeof(HammerPick), 13);
			Add(typeof(Mace), 14);
			Add(typeof(Maul), 10);
			Add(typeof(WarHammer), 12);
			Add(typeof(WarMace), 15);
		}
	}
}
