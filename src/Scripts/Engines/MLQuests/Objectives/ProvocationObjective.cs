using System;
using Server;
using Server.Items;
using Server.Mobiles;
using System.Collections.Generic;
using Server.Targeting;

namespace Server.Engines.Quests
{
    public class ProvocationObjective : SimpleObjective
    {
        private List<string> m_Descr = new List<string>();
        public override List<string> Descriptions => m_Descr;

        public ProvocationObjective()
            : base(5, -1)
        {
            m_Descr.Add("Incite rabbits into battle with 5 wandering healers.");
        }

        public override bool Update(object obj)
        {
            if (obj is Mobile && (((Mobile)obj).GetType() == typeof(WanderingHealer) || ((Mobile)obj).GetType() == typeof(EvilWanderingHealer)))
            {
                CurProgress++;

                if (Completed)
                    Quest.OnCompleted();
                else
                {
                    Quest.Owner.SendLocalizedMessage(1115748, true, (MaxProgress - CurProgress).ToString()); // Conflicts remaining to be incited: 
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
