using Server.Items;
namespace Server.Engines.Quests
{
    public class IronChain : BaseItem
    {
        public override int LabelNumber => 1075788;  // Iron Chain

        [Constructable]
        public IronChain() : base(0x1A07)
        {
        }

        public IronChain(Serial serial)
            : base(serial)
        {
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

    [Flipable]
    public class GreyCloak : BaseCloak, IArcaneEquip
    {
        public override int LabelNumber => 1075789;  // A Plain Grey Cloak
        #region Arcane Impl
        private int m_MaxArcaneCharges, m_CurArcaneCharges;

        [CommandProperty(AccessLevel.GameMaster)]
        public int MaxArcaneCharges
        {
            get => m_MaxArcaneCharges;
            set
            {
                m_MaxArcaneCharges = value;
                InvalidateProperties();
                Update();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int CurArcaneCharges
        {
            get => m_CurArcaneCharges;
            set
            {
                m_CurArcaneCharges = value;
                InvalidateProperties();
                Update();
            }
        }
        public int TempHue { get; set; }


        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsArcane => m_MaxArcaneCharges > 0 && m_CurArcaneCharges >= 0;

        public void Update()
        {
            if (IsArcane)
            {
                ItemID = 0x26AD;
            }
            else if (ItemID == 0x26AD)
            {
                ItemID = 0x1515;
            }

            if (IsArcane && CurArcaneCharges == 0)
            {
                TempHue = Hue;
                Hue = 0;
            }
        }

        //public override void AddCraftedProperties(ObjectPropertyList list)
        //{
        //    base.AddCraftedProperties(list);

        //    if (IsArcane)
        //    {
        //        list.Add(1061837, "{0}\t{1}", m_CurArcaneCharges, m_MaxArcaneCharges); // arcane charges: ~1_val~ / ~2_val~
        //    }
        //}

        public override void OnSingleClick(Mobile from)
        {
            base.OnSingleClick(from);

            if (IsArcane)
            {
                LabelTo(from, 1061837, string.Format("{0}\t{1}", m_CurArcaneCharges, m_MaxArcaneCharges));
            }
        }

        public void Flip()
        {
            if (ItemID == 0x1515)
            {
                ItemID = 0x1530;
            }
            else if (ItemID == 0x1530)
            {
                ItemID = 0x1515;
            }
        }

        #endregion

        [Constructable]
        public GreyCloak()
            : this(0)
        {
        }

        [Constructable]
        public GreyCloak(int hue)
            : base(0x1515, hue)
        {
            Weight = 5.0;
        }

        public GreyCloak(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(1);

            if (IsArcane)
            {
                writer.Write(true);
                writer.Write(m_CurArcaneCharges);
                writer.Write(m_MaxArcaneCharges);
            }
            else
            {
                writer.Write(false);
            }
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 1:
                    {
                        if (reader.ReadBool())
                        {
                            m_CurArcaneCharges = reader.ReadInt();
                            m_MaxArcaneCharges = reader.ReadInt();

                            if (Hue == 2118)
                            {
                                Hue = ArcaneGem.DefaultArcaneHue;
                            }
                        }

                        break;
                    }
            }

            if (Weight == 4.0)
            {
                Weight = 5.0;
            }
        }
    }

    public class SeasonedSkillet : BaseItem
    {
        public override int LabelNumber => 1075774;  // Seasoned Skillet

        [Constructable]
        public SeasonedSkillet() : base(0x097F)
        {
        }

        public SeasonedSkillet(Serial serial)
            : base(serial)
        {
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

    public class VillageCauldron : BaseItem
	{
        public override int LabelNumber => 1075775;  // Village Cauldron

        [Constructable]
        public VillageCauldron()
            : base(Utility.RandomMinMax(0x0974, 0x0975))
        {
        }

        public VillageCauldron(Serial serial)
            : base(serial)
        {
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

    public class ShortStool : CraftableFurniture
    {
        public override int LabelNumber => 1075776;  // Short Stool

        [Constructable]
        public ShortStool()
            : base(0xA2A)
        {
            Weight = 10.0;
        }

        public ShortStool(Serial serial)
            : base(serial)
        {
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

    public class FriendshipMug : BaseBeverage
    {
        public override int LabelNumber => 1075777;  // Friendship Mug
        public override int MaxQuantity => 1;

        public override int ComputeItemID()
        {
            if (ItemID >= 0x995 && ItemID <= 0x999)
            {
                return ItemID;
            }
            else if (ItemID == 0x9CA)
            {
                return ItemID;
            }

            return 0x995;
        }

        [Constructable]
        public FriendshipMug()
        {
            Weight = 1.0;
        }

        [Constructable]
        public FriendshipMug(BeverageType type)
            : base(type)
        {
            Weight = 1.0;
        }

        public FriendshipMug(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            _ = reader.ReadInt();
        }
    }

    public class BrassRing : BaseRing
    {
        public override int LabelNumber => 1075778;  // Brass Ring

        [Constructable]
        public BrassRing()
            : base(0x108a)
        {
        }

        public BrassRing(Serial serial)
            : base(serial)
        {
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

    public class WornHammer : BaseItem
	{
        public override int LabelNumber => 1075779;  // Worn Hammer

        [Constructable]
        public WornHammer() : base(0x102A)
        {
        }

        public WornHammer(Serial serial)
            : base(serial)
        {
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
    [Flipable]
    public class PairOfWorkGloves : BaseArmor, IArcaneEquip
    {
        public override int LabelNumber => 1075780;  // Pair of Work Gloves
        public override int BasePhysicalResistance => 2;
        public override int BaseFireResistance => 4;
        public override int BaseColdResistance => 3;
        public override int BasePoisonResistance => 3;
        public override int BaseEnergyResistance => 3;

        public override int InitMinHits => 30;
        public override int InitMaxHits => 40;

		public override int StrReq => Core.AOS ? 20 : 10;

		public override int ArmorBase => 13;

        public override ArmorMaterialType MaterialType => ArmorMaterialType.Leather;
        public override CraftResource DefaultResource => CraftResource.RegularLeather;

        public override ArmorMeditationAllowance DefMedAllowance => ArmorMeditationAllowance.All;

        [Constructable]
        public PairOfWorkGloves()
            : base(0x13C6)
        {
            Weight = 1.0;
        }

        public PairOfWorkGloves(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(2); // version

            if (IsArcane)
            {
                writer.Write(true);
                writer.Write(TempHue);
                writer.Write(m_CurArcaneCharges);
                writer.Write(m_MaxArcaneCharges);
            }
            else
            {
                writer.Write(false);
            }
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            switch (version)
            {
                case 2:
                    {
                        if (reader.ReadBool())
                        {
                            TempHue = reader.ReadInt();
                            m_CurArcaneCharges = reader.ReadInt();
                            m_MaxArcaneCharges = reader.ReadInt();
                        }

                        break;
                    }
                case 1:
                    {
                        if (reader.ReadBool())
                        {
                            m_CurArcaneCharges = reader.ReadInt();
                            m_MaxArcaneCharges = reader.ReadInt();
                        }

                        break;
                    }
            }
        }

        #region Arcane Impl
        private int m_MaxArcaneCharges, m_CurArcaneCharges;

        [CommandProperty(AccessLevel.GameMaster)]
        public int MaxArcaneCharges
        {
            get => m_MaxArcaneCharges;
            set
            {
                m_MaxArcaneCharges = value;
                InvalidateProperties();
                Update();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int CurArcaneCharges
        {
            get => m_CurArcaneCharges;
            set
            {
                m_CurArcaneCharges = value;
                InvalidateProperties();
                Update();
            }
        }

        public int TempHue { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsArcane => m_MaxArcaneCharges > 0 && m_CurArcaneCharges >= 0;

        public void Update()
        {
            if (IsArcane)
            {
                ItemID = 0x26B0;
            }
            else if (ItemID == 0x26B0)
            {
                ItemID = 0x13C6;
            }

            if (IsArcane && CurArcaneCharges == 0)
            {
                TempHue = Hue;
                Hue = 0;
            }
        }

        /*public override void AddCraftedProperties(ObjectPropertyList list)
        {
            base.AddCraftedProperties(list);

            if (IsArcane)
            {
                list.Add(1061837, "{0}\t{1}", m_CurArcaneCharges, m_MaxArcaneCharges); // arcane charges: ~1_val~ / ~2_val~
            }
        }*/

        public void Flip()
        {
            if (ItemID == 0x13C6)
            {
                ItemID = 0x13CE;
            }
            else if (ItemID == 0x13CE)
            {
                ItemID = 0x13C6;
            }
        }
        #endregion
    }
}
