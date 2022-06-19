using Server.Items;

namespace Server.ContextMenus;

public class EatEntry : ContextMenuEntry
{
	private readonly Mobile _mFrom;
	private readonly Food _mFood;

	public EatEntry(Mobile from, Food food) : base(6135, 1)
	{
		_mFrom = from;
		_mFood = food;
	}

	public override void OnClick()
	{
		if (_mFood.Deleted || !_mFood.Movable || !_mFrom.CheckAlive() || !_mFood.CheckItemUse(_mFrom))
			return;

		_mFood.Eat(_mFrom);
	}
}
