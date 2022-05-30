using System;
using Server.Items;
using Server.Engines.Quests;

namespace Server.Mobiles
{
    public class NewHavenEscortable : BaseEscort
    {
        private static readonly Type[] m_Quests = new Type[]
        {
            typeof(NewHavenAlchemistEscortQuest),
            typeof(NewHavenBardEscortQuest),
            typeof(NewHavenWarriorEscortQuest),
            typeof(NewHavenTailorEscortQuest),
            typeof(NewHavenCarpenterEscortQuest),
            typeof(NewHavenMapmakerEscortQuest),
            typeof(NewHavenMageEscortQuest),
            typeof(NewHavenInnEscortQuest),
            typeof(NewHavenFarmEscortQuest),
            typeof(NewHavenDocksEscortQuest),
            typeof(NewHavenBowyerEscortQuest),
            typeof(NewHavenBankEscortQuest)
        };

        private static readonly string[] m_Destinations = new string[]
        {
            "the New Haven Alchemist",
            "the New Haven Bard",
            "the New Haven Warrior",
            "the New Haven Tailor",
            "the New Haven Carpenter",
            "the New Haven Mapmaker",
            "the New Haven Mage",
            "the New Haven Inn",
            "the New Haven Farm",
            "the New Haven Docks",
            "the New Haven Bowyer",
            "the New Haven Bank"
        };

        private int m_Quest;

        public NewHavenEscortable()
            : this(Utility.Random(12))
        {
        }

        public NewHavenEscortable(int quest)
        {
            m_Quest = quest;
        }

        public NewHavenEscortable(Serial serial)
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

            writer.WriteEncodedInt(0); // version

            writer.Write(m_Quest);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();

            m_Quest = reader.ReadInt();
        }
    }
}
