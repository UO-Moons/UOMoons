using System;
using System.Xml;
using Server;

namespace Server.Regions
{
	public class DemolishedHouseRegion : BaseRegion
	{
		Mobile m_owner;

		public DemolishedHouseRegion(Mobile owner, Map map, params Rectangle3D[] area) : base("", map, DefaultPriority + 2, area)
		{
			m_owner = owner;
			new Internaltimer(this).Start();
		}

		public override bool AllowHousing(Mobile from, Point3D p)
		{
			return from == m_owner || (from.Account != null && m_owner.Account != null && from.Account == m_owner.Account);
		}

		private class Internaltimer : Timer
		{
			private readonly DemolishedHouseRegion m_owner;

			public Internaltimer(DemolishedHouseRegion owner) : base(TimeSpan.FromMinutes(30 + Utility.Random(30)))
			{
				m_owner = owner;
				Priority = TimerPriority.FiveSeconds;
			}

			protected override void OnTick()
			{
				m_owner.Unregister();
				Stop();
			}
		}
	}
}
