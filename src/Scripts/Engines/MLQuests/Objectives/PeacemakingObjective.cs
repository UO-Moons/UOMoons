using System;
using Server;
using Server.Items;
using Server.Mobiles;
using System.Collections.Generic;
using Server.Targeting;

namespace Server.Engines.Quests
{
    public class PeacemakingObjective : SimpleObjective
    {
        private static readonly Type m_Type = typeof(Mongbat);

        private List<string> m_Descr = new List<string>();
        public override List<string> Descriptions => m_Descr;

        public PeacemakingObjective()
            : base(5, -1)
        {
            m_Descr.Add("Calm five mongbats.");
        }

        public override bool Update(object obj)
        {
            if (obj is Mobile && ((Mobile)obj).GetType() == m_Type)
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

            int version = reader.ReadInt();
        }
    }
}
