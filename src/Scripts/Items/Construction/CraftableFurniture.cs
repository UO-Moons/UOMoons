using Server.Engines.Craft;
using System;

namespace Server.Items
{
    public class CraftableFurniture : Item, IResource/*, IQuality*/
    {
        public virtual bool ShowCrafterName => true;

        private Mobile m_Crafter;
        private CraftResource m_Resource;
        private ItemQuality m_Quality;

        [CommandProperty(AccessLevel.GameMaster)]
        public ItemQuality Quality
        {
            get => m_Quality;
            set
            {
                m_Quality = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public CraftResource Resource
        {
            get => m_Resource;
            set
            {
                if (m_Resource != value)
                {
                    m_Resource = value;
                    Hue = CraftResources.GetHue(m_Resource);

                    InvalidateProperties();
                }
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile Crafter
        {
            get => m_Crafter;
            set
            {
                m_Crafter = value;
                InvalidateProperties();
            }
        }

        public virtual bool PlayerConstructed => true;

        public CraftableFurniture(int itemID)
            : base(itemID)
        {
        }

        public CraftableFurniture(Serial serial)
            : base(serial)
        {
        }

        public override void AddWeightProperty(ObjectPropertyList list)
        {
            base.AddWeightProperty(list);

            //if (ShowCrafterName && m_Crafter != null)
            //{
            //    list.Add(1050043, m_Crafter.TitleName); // crafted by ~1_NAME~
            //}

            if (m_Quality == ItemQuality.Exceptional)
            {
                list.Add(1060636); // exceptional
            }
        }

        //public override void AddCraftedProperties(ObjectPropertyList list)
        //{
        //    CraftResourceInfo info = CraftResources.IsStandard(m_Resource) ? null : CraftResources.GetInfo(m_Resource);

        //    if (info != null && info.Number > 0)
        //    {
        //        list.Add(info.Number);
        //    }
        //}

        /*public override void OnSingleClick(Mobile from)
        {
            base.OnSingleClick(from);

            if (m_Crafter != null)
            {
                LabelTo(from, 1050043, m_Crafter.TitleName); // crafted by ~1_NAME~
            }
        }*/

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version

            writer.Write(m_Crafter);
            writer.Write((int)m_Resource);
            writer.Write((int)m_Quality);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.PeekInt();

            switch (version)
            {
                case 0:
                    reader.ReadInt();
                    m_Crafter = reader.ReadMobile();
                    m_Resource = (CraftResource)reader.ReadInt();
                    m_Quality = (ItemQuality)reader.ReadInt();
                    break;
            }
        }

        #region ICraftable
        public virtual int OnCraft(int quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, ITool tool, CraftItem craftItem, int resHue)
        {
            Quality = (ItemQuality)quality;

            if (makersMark)
            {
                Crafter = from;
            }

            Type resourceType = typeRes;

            if (resourceType == null)
            {
                resourceType = craftItem.Resources.GetAt(0).ItemType;
            }

            Resource = CraftResources.GetFromType(resourceType);

            return quality;
        }
        #endregion
    }
}
