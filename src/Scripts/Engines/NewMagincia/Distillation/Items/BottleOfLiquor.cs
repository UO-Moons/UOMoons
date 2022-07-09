using Server.Items;

namespace Server.Engines.Distillation
{
    public class BottleOfLiquor : BeverageBottle
    {
        private Liquor _liquor;
        private string _label;
        private bool _isStrong;
        private Mobile _distiller;

        [CommandProperty(AccessLevel.GameMaster)]
        public Liquor Liquor { get => _liquor;
	        set { _liquor = value; InvalidateProperties(); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public string Label { get => _label;
	        set { _label = value; InvalidateProperties(); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsStrong { get => _isStrong;
	        set { _isStrong = value; InvalidateProperties(); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile Distiller { get => _distiller;
	        set { _distiller = value; InvalidateProperties(); } }

        public override bool ShowQuantity => false;

        [Constructable]
        public BottleOfLiquor() : this(Liquor.Whiskey, null, false, null)
        {
        }

        [Constructable]
        public BottleOfLiquor(Liquor liquor, string label, bool isstrong, Mobile distiller) : base(BeverageType.Liquor)
        {
            Quantity = MaxQuantity;
            _liquor = liquor;
            _label = label;
            _isStrong = isstrong;
            _distiller = distiller;
        }

        public override void AddNameProperty(ObjectPropertyList list)
        {
	        list.Add(1049519,
		        !string.IsNullOrEmpty(_label) ? _label : $"#{DistillationSystem.GetLabel(_liquor, _isStrong)}");
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (_liquor != Liquor.None)
                list.Add(1150454, $"#{DistillationSystem.GetLabel(_liquor, _isStrong)}"); // Liquor Type: ~1_TYPE~

            if (_distiller != null)
                list.Add(1150679, _distiller.Name); // Distiller: ~1_NAME~

            list.Add(GetQuantityDescription());
        }

        public BottleOfLiquor(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);

            writer.Write(_isStrong);

            writer.Write((int)_liquor);
            writer.Write(_label);
            writer.Write(_distiller);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    _isStrong = reader.ReadBool();
                    _liquor = (Liquor)reader.ReadInt();
                    _label = reader.ReadString();
                    _distiller = reader.ReadMobile();

                    _isStrong = true;
                    break;
            }
        }
    }
}
