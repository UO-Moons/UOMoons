using System;

namespace Server.Regions;

public class DemolishedHouseRegion : BaseRegion
{
	readonly Mobile _mOwner;

	public DemolishedHouseRegion(Mobile owner, Map map, params Rectangle3D[] area) : base("", map, DefaultPriority + 2, area)
	{
		_mOwner = owner;
		new Internaltimer(this).Start();
	}

	public override bool AllowHousing(Mobile from, Point3D p)
	{
		return from == _mOwner || (from.Account != null && _mOwner.Account != null && from.Account == _mOwner.Account);
	}

	private class Internaltimer : Timer
	{
		private readonly DemolishedHouseRegion _mOwner;

		public Internaltimer(DemolishedHouseRegion owner) : base(TimeSpan.FromMinutes(30 + Utility.Random(30)))
		{
			_mOwner = owner;
			Priority = TimerPriority.FiveSeconds;
		}

		protected override void OnTick()
		{
			_mOwner.Unregister();
			Stop();
		}
	}
}
