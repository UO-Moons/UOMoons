using Server.Engines.Plants;
using System.Collections.Generic;
using System.IO;

namespace Server.Engines.Craft
{
    public enum CraftMarkOption
    {
        MarkItem,
        DoNotMark,
        PromptForMark
    }

    #region SA
    public enum CraftQuestOption
    {
        QuestItem,
        NonQuestItem
    }
    #endregion

    public class CraftContext
    {
        public Mobile Owner { get; private set; }
        public CraftSystem System { get; private set; }

        #region Hue State Vars
        /*private bool m_CheckedHues;
        private List<int> m_Hues;
        private Item m_CompareHueTo;

        public bool CheckedHues
        {
            get
            {
                return m_CheckedHues;
            }
            set
            {
                m_CheckedHues = value;
            }
        }
        public List<int> Hues
        {
            get
            {
                return m_Hues;
            }
            set
            {
                m_Hues = value;
            }
        }
        public Item CompareHueTo
        {
            get
            {
                return m_CompareHueTo;
            }
            set
            {
                m_CompareHueTo = value;
            }
        }*/
        #endregion

        public List<CraftItem> Items { get; }
        public int LastResourceIndex { get; set; }
        public int LastResourceIndex2 { get; set; }
        public int LastGroupIndex { get; set; }
        public bool DoNotColor { get; set; }
        public CraftMarkOption MarkOption { get; set; }
        #region SA
        public CraftQuestOption QuestOption { get; set; }

        public int MakeTotal { get; set; }

        public PlantHue RequiredPlantHue { get; set; }

        public PlantPigmentHue RequiredPigmentHue { get; set; }
        #endregion

        public CraftContext(Mobile owner, CraftSystem system)
        {
            Owner = owner;
            System = system;

            Items = new List<CraftItem>();
            LastResourceIndex = -1;
            LastResourceIndex2 = -1;
            LastGroupIndex = -1;

            QuestOption = CraftQuestOption.NonQuestItem;
            RequiredPlantHue = PlantHue.None;
            RequiredPigmentHue = PlantPigmentHue.None;

            Contexts.Add(this);
        }

        public CraftItem LastMade
        {
            get
            {
                if (Items.Count > 0)
                {
                    return Items[0];
                }

                return null;
            }
        }

        public void OnMade(CraftItem item)
        {
            Items.Remove(item);

            if (Items.Count == 10)
            {
                Items.RemoveAt(9);
            }

            Items.Insert(0, item);
        }

        public virtual void Serialize(GenericWriter writer)
        {
            writer.Write(0);

            writer.Write(Owner);
            writer.Write(GetSystemIndex(System));
            writer.Write(LastResourceIndex);
            writer.Write(LastResourceIndex2);
            writer.Write(LastGroupIndex);
            writer.Write(DoNotColor);
            writer.Write((int)MarkOption);
            writer.Write((int)QuestOption);

            writer.Write(MakeTotal);
        }

        public CraftContext(GenericReader reader)
        {
            _ = reader.ReadInt();

            Items = new List<CraftItem>();

            Owner = reader.ReadMobile();
            int sysIndex = reader.ReadInt();
            LastResourceIndex = reader.ReadInt();
            LastResourceIndex2 = reader.ReadInt();
            LastGroupIndex = reader.ReadInt();
            DoNotColor = reader.ReadBool();
            MarkOption = (CraftMarkOption)reader.ReadInt();
            QuestOption = (CraftQuestOption)reader.ReadInt();

            MakeTotal = reader.ReadInt();

            System = GetCraftSystem(sysIndex);

            if (System != null && Owner != null)
            {
                System.AddContext(Owner, this);
                Contexts.Add(this);
            }
        }

        public int GetSystemIndex(CraftSystem system)
        {
            for (int i = 0; i < Systems.Length; i++)
            {
                if (Systems[i] == system)
                {
                    return i;
                }
            }

            return -1;
        }

        public CraftSystem GetCraftSystem(int i)
        {
            if (i >= 0 && i < Systems.Length)
            {
                return Systems[i];
            }

            return null;
        }

        #region Serialize/Deserialize Persistence
        private static readonly string FilePath = Path.Combine("Saves", "CraftContext", "Contexts.bin");

        private static readonly List<CraftContext> Contexts = new List<CraftContext>();

        public static CraftSystem[] Systems { get; } = new CraftSystem[11];

        public static void Configure()
        {
            Systems[0] = DefAlchemy.CraftSystem;
            Systems[1] = DefBlacksmithy.CraftSystem;
            Systems[2] = DefBowFletching.CraftSystem;
            Systems[3] = DefCarpentry.CraftSystem;
            Systems[4] = DefCartography.CraftSystem;
            Systems[5] = DefCooking.CraftSystem;
            Systems[6] = DefGlassblowing.CraftSystem;
            Systems[7] = DefInscription.CraftSystem;
            Systems[8] = DefMasonry.CraftSystem;
            Systems[9] = DefTailoring.CraftSystem;
            Systems[10] = DefTinkering.CraftSystem;

            EventSink.OnWorldSave += OnSave;
            EventSink.OnWorldLoad += OnLoad;
        }

        public static void OnSave()
        {
            Persistence.Serialize(
                FilePath,
                writer =>
                {
                    writer.Write(0); // version

                    writer.Write(Contexts.Count);
                    Contexts.ForEach(c => c.Serialize(writer));
                });
        }

        public static void OnLoad()
        {
            Persistence.Deserialize(
                FilePath,
                reader =>
                {
                    int version = reader.ReadInt();

                    int count = reader.ReadInt();
                    for (int i = 0; i < count; i++)
                    {
                        new CraftContext(reader);
                    }
                });
        }
        #endregion
    }
}
