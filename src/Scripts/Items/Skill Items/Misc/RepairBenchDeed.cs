using Server.Engines.VeteranRewards;
using Server.Gumps;
using System.Collections.Generic;
using System.Linq;

namespace Server.Items
{
    public class RepairBenchDeed : BaseAddonDeed, IRewardItem, IRewardOption
    {
        public override int LabelNumber => 1158860;  // Repair Bench

        public override BaseAddon Addon
        {
            get
            {
                RepairBenchAddon addon = new RepairBenchAddon(_Direction, Tools)
                {
                    IsRewardItem = m_IsRewardItem
                };

                return addon;
            }
        }

        private DirectionType _Direction;

        private bool m_IsRewardItem;

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsRewardItem
        {
            get => m_IsRewardItem;
            set
            {
                m_IsRewardItem = value;
                InvalidateProperties();
            }
        }

        public List<RepairBenchDefinition> Tools;

        [Constructable]
        public RepairBenchDeed()
            : this(null)
        {
        }

        [Constructable]
        public RepairBenchDeed(List<RepairBenchDefinition> tools)
            : base()
        {
            Tools = tools;
            LootType = LootType.Blessed;
        }

        public RepairBenchDeed(Serial serial)
            : base(serial)
        {
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (m_IsRewardItem)
            {
                list.Add(1076221); // 5th Year Veteran Reward
            }

            if (Tools != null)
            {
                int[] value = Tools.Select(x => x.Charges).ToArray();
                list.Add(1158899, $"{value[0]}\t{value[1]}\t{value[2]}\t{value[3]}\t{value[4]}\t{value[5]}\t{value[6]}"); // Tinkering: ~1_CHARGES~<br>Blacksmithing: ~2_CHARGES~<br>Carpentry: ~3_CHARGES~<br>Tailoring: ~4_CHARGES~<br>Fletching: ~5_CHARGES~<br>Masonry: ~6_CHARGES~<br>Glassblowing: ~7_CHARGES~
            }
        }

        public void GetOptions(RewardOptionList list)
        {
            list.Add((int)DirectionType.South, 1075386); // South
            list.Add((int)DirectionType.East, 1075387); // East
        }

        public void OnOptionSelected(Mobile from, int choice)
        {
            _Direction = (DirectionType)choice;

            if (!Deleted)
            {
                base.OnDoubleClick(from);
            }
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (IsChildOf(from.Backpack))
            {
                from.CloseGump(typeof(AddonOptionGump));
                from.SendGump(new AddonOptionGump(this, 1154194)); // Choose a Facing:
            }
            else
            {
                from.SendLocalizedMessage(1062334); // This item must be in your backpack to be used.
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);

            writer.Write(m_IsRewardItem);

            writer.Write(Tools == null ? 0 : Tools.Count);

            if (Tools != null)
            {
                Tools.ForEach(x =>
                {
                    writer.Write((int)x.Skill);
                    writer.Write((int)x.SkillValue);
                    writer.Write(x.Charges);
                });
            }
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            _ = reader.ReadInt();

            m_IsRewardItem = reader.ReadBool();

            int toolcount = reader.ReadInt();

            if (toolcount != 0)
            {
                Tools = new List<RepairBenchDefinition>();
            }

            for (int x = 0; x < toolcount; x++)
            {
                RepairSkillType skill = (RepairSkillType)reader.ReadInt();
                int skillvalue = reader.ReadInt();
                int charge = reader.ReadInt();

                Tools.Add(new RepairBenchDefinition(RepairBenchAddon.GetInfo(skill).System, skill, RepairBenchAddon.GetInfo(skill).Cliloc, skillvalue, charge));
            }
        }
    }
}
