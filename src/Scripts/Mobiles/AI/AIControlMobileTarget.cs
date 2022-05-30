using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;
using System.Collections.Generic;

namespace Server.Targets
{
	public class AIControlMobileTarget : Target
	{
		private readonly List<BaseAI> m_List;
		private readonly BaseCreature m_Mobile;

		public AIControlMobileTarget(BaseAI ai, OrderType order)
			: base(-1, false, (order == OrderType.Attack ? TargetFlags.Harmful : TargetFlags.None))
		{
			m_List = new List<BaseAI>();
			Order = order;

			AddAI(ai);
			m_Mobile = ai.m_Mobile;
		}

		public OrderType Order { get; }

		public void AddAI(BaseAI ai)
		{
			if (!m_List.Contains(ai))
			{
				m_List.Add(ai);
			}
		}

		protected override void OnTarget(Mobile from, object o)
		{
			if (o is IDamageable)
			{
				var dam = o as IDamageable;

				for (var i = 0; i < m_List.Count; ++i)
				{
					m_List[i].EndPickTarget(from, dam, Order);
				}
			}
			else if (o is MoonglowDonationBox && Order == OrderType.Transfer && from is PlayerMobile)
			{
				var pm = (PlayerMobile)from;
				var box = (MoonglowDonationBox)o;

				pm.SendGump(new ConfirmTransferPetGump(box, from.Location, m_Mobile));
			}
		}
	}
}
