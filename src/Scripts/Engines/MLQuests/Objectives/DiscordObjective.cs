using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Engines.Quests
{
	public class DiscordObjective : SimpleObjective
	{
		private static readonly Type m_Type = typeof(Goat);

		private readonly List<string> m_Descr = new();
		public override List<string> Descriptions => m_Descr;

		public DiscordObjective()
			: base(5, -1)
		{
			m_Descr.Add("Discord five goats.");
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
					Quest.Owner.SendLocalizedMessage(1115749, true, (MaxProgress - CurProgress).ToString()); // Creatures remaining to be discorded: 
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
