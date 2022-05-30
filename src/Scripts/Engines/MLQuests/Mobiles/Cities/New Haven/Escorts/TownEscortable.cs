using System;
using Server.Items;
using Server.Engines.Quests;

namespace Server.Mobiles
{
    public class TownEscortable : BaseEscort
    {
        private static readonly Type[] m_Quests = new Type[]
        {
            typeof(EscortToYewQuest),
            typeof(EscortToVesperQuest),
            typeof(EscortToTrinsicQuest),
            typeof(EscortToSkaraQuest),
            typeof(EscortToSerpentsHoldQuest),
            typeof(EscortToNujelmQuest),
            typeof(EscortToMoonglowQuest),
            typeof(EscortToMinocQuest),
            typeof(EscortToMaginciaQuest),
            typeof(EscortToJhelomQuest),
            typeof(EscortToCoveQuest),
            typeof(EscortToBritainQuest)
        };

        private static readonly string[] m_Destinations = new string[]
        {
            "Yew",
            "Vesper",
            "Trinsic",
            "Skara Brae",
            "Serpent's Hold",
            "Nujel'm",
            "Moonglow",
            "Minoc",
            "Magincia",
            "Jhelom",
            "Cove",
            "Britain"
        };

        private int m_Quest;

        public TownEscortable()
            : base()
        {
            m_Quest = Utility.Random(m_Quests.Length);
        }

        protected override void OnMapChange(Map oldMap)
        {
            base.OnMapChange(oldMap);

            if (m_Destinations[m_Quest] == Region.Name)
            {
                m_Quest = RandomDestination();
            }
        }

        private int RandomDestination()
        {
            int random;

            do
            {
                random = Utility.Random(m_Destinations.Length);
            }
            while (m_Destinations[random] == Region.Find(Location, Map).Name);

            return random;
        }

        public TownEscortable(Serial serial)
            : base(serial)
        {
        }

        public override Type[] Quests => new Type[] { m_Quests[m_Quest] };
        public override void Advertise()
        {
            Say(Utility.RandomMinMax(1072301, 1072303));
        }

        public override Region GetDestination()
        {
            return QuestHelper.FindRegion(m_Destinations[m_Quest]);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(1); // version

            writer.Write(m_Quest);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();

            m_Quest = reader.ReadInt();

            if (version == 0 && m_Destinations[m_Quest] == Region.Name)
            {
                m_Quest = RandomDestination();
                Console.WriteLine("Adjusting escort destination.");
            }
        }
    }
}
