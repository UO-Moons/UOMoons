namespace Server.Engines.NewMagincia
{
    public class MaginciaPlotStone : BaseItem
    {
        public override bool ForceShowProperties => true;

        [CommandProperty(AccessLevel.GameMaster)]
        public MaginciaHousingPlot Plot { get; set; }

        [Constructable]
        public MaginciaPlotStone() : base(3805)
        {
            Movable = false;
        }

        public override void AddNameProperty(ObjectPropertyList list)
        {
            list.Add(1150494, Plot != null ? Plot.Identifier : "Unknown"); // lot ~1_PLOTID~
        }

        public override void OnDoubleClick(Mobile from)
        {
            MaginciaLottoSystem system = MaginciaLottoSystem.Instance;

            if (system is not {Enabled: true} || Plot == null)
                return;

            if (from.InRange(Location, 4))
            {
                if (Plot.LottoOngoing)
                {
                    from.CloseGump(typeof(MaginciaLottoGump));
                    from.SendGump(new MaginciaLottoGump(from, Plot));
                }
                else if (!Plot.IsAvailable)
                    from.SendMessage("The lottory for this lot has ended.");
                else
                    from.SendMessage("The lottory for this lot has expired.  Check back soon!");
            }
        }

        public override void OnAfterDelete()
        {
            if (Plot != null)
                MaginciaLottoSystem.UnregisterPlot(Plot);

            base.OnAfterDelete();
        }

        public MaginciaPlotStone(Serial serial)
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

	        reader.ReadInt();
        }
    }
}
