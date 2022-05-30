namespace Server.Items
{
    public class SquareGozaMatAddon : BaseAddon
    {
        [Constructable]
        public SquareGozaMatAddon()
            : this(0)
        {
        }

        [Constructable]
        public SquareGozaMatAddon(int hue)
        {
            AddComponent(new LocalizedAddonComponent(0x2A3C, 1113621), 0, 0, 0);
            Hue = hue;
        }

        public SquareGozaMatAddon(Serial serial)
            : base(serial)
        {
        }

        public override BaseAddonDeed Deed => new SquareGozaMatDeed();
        public override bool RetainDeedHue => true;

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

    public class SquareGozaMatDeed : BaseAddonDeed
    {
        public override bool UseCraftResource => false;

        [Constructable]
        public SquareGozaMatDeed()
        {
        }

        public SquareGozaMatDeed(Serial serial)
            : base(serial)
        {
        }

        public override BaseAddon Addon => new SquareGozaMatAddon(Hue);
        public override int LabelNumber => 1030410;// brocade square goza (south)
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
