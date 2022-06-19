using Server.Targeting;

namespace Server;

public delegate void BoundingBoxCallback(Mobile from, Map map, Point3D start, Point3D end, object state);

public class BoundingBoxPicker
{
	public static void Begin(Mobile from, BoundingBoxCallback callback, object state)
	{
		from.SendMessage("Target the first location of the bounding box.");
		from.Target = new PickTarget(callback, state);
	}

	private class PickTarget : Target
	{
		private readonly Point3D _mStore;
		private readonly bool _mFirst;
		private readonly Map _mMap;
		private readonly BoundingBoxCallback _mCallback;
		private readonly object _mState;

		public PickTarget(BoundingBoxCallback callback, object state) : this(Point3D.Zero, true, null, callback, state)
		{
		}

		public PickTarget(Point3D store, bool first, Map map, BoundingBoxCallback callback, object state) : base(-1, true, TargetFlags.None)
		{
			_mStore = store;
			_mFirst = first;
			_mMap = map;
			_mCallback = callback;
			_mState = state;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			IPoint3D p = targeted as IPoint3D;

			switch (p)
			{
				case null:
					return;
				case Item item:
					p = item.GetWorldTop();
					break;
			}

			if (_mFirst)
			{
				from.SendMessage("Target another location to complete the bounding box.");
				from.Target = new PickTarget(new Point3D(p), false, from.Map, _mCallback, _mState);
			}
			else if (from.Map != _mMap)
			{
				from.SendMessage("Both locations must reside on the same map.");
			}
			else if (_mMap != null && _mMap != Map.Internal && _mCallback != null)
			{
				Point3D start = _mStore;
				Point3D end = new(p);

				Utility.FixPoints(ref start, ref end);

				_mCallback(from, _mMap, start, end, _mState);
			}
		}
	}
}
