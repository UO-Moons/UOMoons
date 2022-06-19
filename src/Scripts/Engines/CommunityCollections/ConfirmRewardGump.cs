using Server.Mobiles;

namespace Server.Gumps;

public class ConfirmRewardGump : BaseConfirmGump
{
	private readonly IComunityCollection _mCollection;
	private readonly Point3D _mLocation;
	private readonly CollectionItem _mItem;
	private readonly int _mHue;

	public ConfirmRewardGump(IComunityCollection collection, Point3D location, CollectionItem item)
		: this(collection, location, item, 0)
	{
	}

	public ConfirmRewardGump(IComunityCollection collection, Point3D location, CollectionItem item, int hue)
	{
		_mCollection = collection;
		_mLocation = location;
		_mItem = item;
		_mHue = hue;

		if (_mItem != null)
			AddItem(150, 100, _mItem.ItemId, _mItem.Hue);
	}

	public override int TitleNumber => 1074974;// Confirm Selection
	public override int LabelNumber => 1074975;// Are you sure you wish to select this?
	public override void Confirm(Mobile from)
	{
		if (_mCollection == null || !from.InRange(_mLocation, 2))
			return;

		if (from is not PlayerMobile player) return;
		if (player.GetCollectionPoints(_mCollection.CollectionId) < _mItem.Points)
		{
			player.SendLocalizedMessage(1073122); // You don't have enough points for that!
		}
		else if (_mItem.CanSelect(player))
		{
			_mCollection.Reward(player, _mItem, _mHue);
		}
	}
}
