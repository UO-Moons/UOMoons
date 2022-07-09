using Server.Engines.Craft;
using Server.Engines.Distillation;
using System;
using System.Globalization;

namespace Server.Items
{
    public class LiquorBarrel : BaseItem, ICraftable
    {
        private Liquor _liquor;
        private DateTime _maturationBegin;
        private string _label;
        private bool _isStrong;
        private int _usesRemaining;
        private bool _exceptional;
        private Mobile _distiller;

        [CommandProperty(AccessLevel.GameMaster)]
        public Liquor Liquor { get => _liquor;
	        set { BeginDistillation(value); InvalidateProperties(); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime MaturationBegin { get => _maturationBegin;
	        set { _maturationBegin = value; InvalidateProperties(); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan MutrationDuration { get; private set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public string Label { get => _label;
	        set { _label = value; InvalidateProperties(); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsStrong { get => _isStrong;
	        set { _isStrong = value; InvalidateProperties(); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int UsesRemaining { get => _usesRemaining;
	        set { _usesRemaining = value; InvalidateProperties(); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Exceptional { get => _exceptional;
	        set { _exceptional = value; InvalidateProperties(); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile Distiller { get => _distiller;
	        set { _distiller = value; InvalidateProperties(); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public virtual bool IsMature => _liquor != Liquor.None && (MutrationDuration == TimeSpan.MinValue || _maturationBegin + MutrationDuration < DateTime.UtcNow);

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsEmpty => _liquor == Liquor.None;

        public override int LabelNumber => _usesRemaining == 0 || _liquor == Liquor.None ? 1150816 : 1150807;  // liquor barrel

        public override double DefaultWeight => 5.0;

        [Constructable]
        public LiquorBarrel()
            : base(4014)
        {
            _liquor = Liquor.None;
            _usesRemaining = 0;
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (IsChildOf(from.Backpack) && _usesRemaining > 0)
            {
                if (IsMature)
                {
                    BottleOfLiquor bottle = new(_liquor, _label, _isStrong, _distiller);

                    if (from.Backpack == null || !from.Backpack.TryDropItem(from, bottle, false))
                    {
                        bottle.Delete();
                        from.SendLocalizedMessage(500720); // You don't have enough room in your backpack!
                    }
                    else
                    {
                        from.PlaySound(0x240);
                        from.SendLocalizedMessage(1150815); // You have poured matured liquid into the bottle.
                        UsesRemaining--;
                    }
                }
                else
                {
                    from.SendLocalizedMessage(1150806); // You need to wait until the liquor in the barrel has matured.

                    if (DateTime.UtcNow < _maturationBegin + MutrationDuration)
                    {
                        TimeSpan remaining = (_maturationBegin + MutrationDuration) - DateTime.UtcNow;
                        if (remaining.TotalDays > 0)
                            from.SendLocalizedMessage(1150814,
	                            $"{remaining.Days.ToString()}\t{remaining.Hours.ToString()}");
                        else
                            from.SendLocalizedMessage(1150813, remaining.TotalHours.ToString(CultureInfo.InvariantCulture));
                    }
                }
            }
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (Crafter != null)
                list.Add(1050043, Crafter.Name); // Crafted By: ~1_Name~

            if (_exceptional)
                list.Add(1018303); // Exceptional

            if (!IsEmpty)
            {
                if (IsMature)
                    list.Add(1060584, _usesRemaining.ToString()); // uses remaining: ~1_val~

                list.Add(1150805, _maturationBegin.ToShortDateString()); // start date: ~1_NAME~

                int cliloc = IsMature ? 1150804 : 1150812;  // maturing: ~1_NAME~ / // matured: ~1_NAME~

                list.Add(cliloc, _label ?? $"#{DistillationSystem.GetLabel(_liquor, _isStrong)}");

                list.Add(1150454, $"#{DistillationSystem.GetLabel(_liquor, _isStrong)}"); // Liquor Type: ~1_TYPE~

                if (_distiller != null)
                    list.Add(1150679, _distiller.Name); // Distiller: ~1_NAME~
            }
        }

        public void BeginDistillation(Liquor liquor)
        {
	        var ts = liquor is Liquor.Spirytus or Liquor.Akvavit ? TimeSpan.MinValue : DistillationSystem.MaturationPeriod;

            BeginDistillation(liquor, ts, _label, _isStrong, _distiller);
        }

        public void BeginDistillation(Liquor liquor, TimeSpan duration, string label, bool isStrong, Mobile distiller)
        {
            _liquor = liquor;
            MutrationDuration = duration;
            _label = label;
            _isStrong = isStrong;
            _distiller = distiller;
            _maturationBegin = DateTime.UtcNow;
            _usesRemaining = _exceptional ? 20 : 10;

            InvalidateProperties();
        }

        public virtual int OnCraft(int quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, ITool tool, CraftItem craftItem, int resHue)
        {
            if (quality >= 2)
            {
                _exceptional = true;

                if (makersMark)
	                Crafter = from;
            }

            typeRes ??= craftItem.Resources.GetAt(0).ItemType;

            CraftResource resource = CraftResources.GetFromType(typeRes);
            Hue = CraftResources.GetHue(resource);

            return quality;
        }

        public LiquorBarrel(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);

            writer.Write((int)_liquor);
            writer.Write(_maturationBegin);
            writer.Write(MutrationDuration);
            writer.Write(_label);
            writer.Write(_isStrong);
            writer.Write(_usesRemaining);
            writer.Write(_exceptional);
            writer.Write(_distiller);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            _liquor = (Liquor)reader.ReadInt();
            _maturationBegin = reader.ReadDateTime();
            MutrationDuration = reader.ReadTimeSpan();
            _label = reader.ReadString();
            _isStrong = reader.ReadBool();
            _usesRemaining = reader.ReadInt();
            _exceptional = reader.ReadBool();
            _distiller = reader.ReadMobile();
        }
    }
}
