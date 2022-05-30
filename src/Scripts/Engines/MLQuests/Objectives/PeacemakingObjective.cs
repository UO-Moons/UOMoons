using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Engines.Quests
{
	public class PeacemakingObjective : SimpleObjective
	{
		private static readonly Type m_Type = typeof(Mongbat);

		private readonly List<string> m_Descr = new();
		public override List<string> Descriptions => m_Descr;

		public PeacemakingObjective()
			: base(5, -1)
		{
			m_Descr.Add("Calm five mongbats.");
		}

		public override bool Update(object obj)
		{
			if (obj is Mobile mobile && mobile.GetType() == m_Type)
			{
				CurProgress++;

				if (Completed)
					Quest.OnCompleted();
				else
				{
					Quest.Owner.SendLocalizedMessage(1115747, true, (MaxProgress - CurProgress).ToString()); // Creatures remaining to be calmed:   ~1_val~.
					Quest.Owner.PlaySound(Quest.UpdateSound);
				}

				return true;
			}

			return false;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			_ = reader.ReadInt();
		}
	}
}
