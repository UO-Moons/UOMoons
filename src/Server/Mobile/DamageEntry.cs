using System;
using System.Collections.Generic;

namespace Server
{
	public class DamageEntry
	{
		public Mobile Damager { get; }
		public int DamageGiven { get; set; }
		public DateTime LastDamage { get; set; }
		public bool HasExpired => (DateTime.UtcNow > (LastDamage + m_ExpireDelay));
		public List<DamageEntry> Responsible { get; set; }

		private static TimeSpan m_ExpireDelay = TimeSpan.FromMinutes(2.0);
		public static TimeSpan ExpireDelay { get => m_ExpireDelay; set => m_ExpireDelay = value; }

		public DamageEntry(Mobile damager)
		{
			Damager = damager;
		}
	}
}
