namespace Server.Items
{
    [Flipable(0x19BC, 0x19BD)]
    public partial class BaseCostume : BaseShield
    {
        public bool m_Transformed;
        private int _saveHueMod = -1;

        public virtual string CreatureName { get; }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Transformed
        {
            get => m_Transformed;
            set => m_Transformed = value;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int CostumeBody { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int CostumeHue { get; set; } = -1;

        public BaseCostume(string creatureName)
            : base(0x19BC)
        {
	        CreatureName = creatureName;
	        Resource = CraftResource.None;
            Attributes.SpellChanneling = 1;
            Layer = Layer.FirstValid;
            Weight = 4.0;
            StrRequirement = 10;
        }

        public BaseCostume(Serial serial, string creatureName)
            : base(serial)
        {
	        CreatureName = creatureName;
        }

        private bool EnMask(Mobile from)
        {
            if (from.Mounted || from.Flying) // You cannot use this while mounted or flying. 
            {
                from.SendLocalizedMessage(1010097);
            }
            else if (from.IsBodyMod || from.HueMod > -1)
            {
                from.SendLocalizedMessage(1158010); // You cannot use that item in this form.
            }
            else
            {
                from.BodyMod = CostumeBody;
                from.HueMod = CostumeHue;
                Transformed = true;

                return true;
            }

            return false;
        }

        private void DeMask(Mobile from)
        {
            from.BodyMod = 0;
            from.HueMod = -1;
            Transformed = false;
        }

        public virtual bool Dye(Mobile from, DyeTub sender)
        {
            if (Deleted)
                return false;

            else if (RootParent is Mobile && from != RootParent)
                return false;

            Hue = sender.DyedHue;
            return true;
        }

        public override bool OnEquip(Mobile from)
        {
            if (!Transformed)
            {
                if (EnMask(from))
                    return true;

                return false;
            }

            return base.OnEquip(from);
        }

        public override void OnRemoved(IEntity parent)
		{
            if (parent is Mobile mobile && Transformed)
            {
                DeMask(mobile);
            }

            base.OnRemoved(parent);
        }

        public static void OnDamaged(Mobile m)
        {
	        if (m.FindItemOnLayer(Layer.FirstValid) is BaseCostume costume)
            {
                m.AddToBackpack(costume);
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(3);
            writer.Write(CostumeBody);
            writer.Write(CostumeHue);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 3:
                    CostumeBody = reader.ReadInt();
                    CostumeHue = reader.ReadInt();
                    break;
                case 2:
                    CostumeBody = reader.ReadInt();
                    CostumeHue = reader.ReadInt();
                    reader.ReadInt();
                    break;
                case 1:
                    CostumeBody = reader.ReadInt();
                    CostumeHue = reader.ReadInt();
                    reader.ReadInt();
                    reader.ReadBool();

                    _saveHueMod = reader.ReadInt();
                    reader.ReadInt();
                    break;
            }

            if (RootParent is Mobile mobile && mobile.Items.Contains(this))
            {
                EnMask(mobile);
            }
        }
    }
}
