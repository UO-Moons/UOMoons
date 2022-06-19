using Server.Items;

namespace Server.Mobiles;

public class PresetMapBuyInfo : GenericBuyInfo
{
	private readonly PresetMapEntry _mEntry;

	public override bool CanCacheDisplay => false;

	public PresetMapBuyInfo(PresetMapEntry entry, int price, int amount) : base(entry.Name.ToString(), null, price, amount, 0x14EC, 0)
	{
		_mEntry = entry;
	}

	public override IEntity GetEntity()
	{
		return new PresetMap(_mEntry);
	}
}
