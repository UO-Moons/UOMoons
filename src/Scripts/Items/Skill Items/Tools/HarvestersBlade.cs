namespace Server.Items
{
    public class HarvestersBlade : ElvenSpellblade
    {
        public override int LabelNumber => 1114096;  // Harvester's Blade

        [Constructable]
        public HarvestersBlade()
        {
            Hue = 1191;
            Attributes.SpellChanneling = 1;
        }

        public HarvestersBlade(Serial serial) : base(serial)
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
            int v = reader.ReadInt();

            if (v == 0)
            {
                Hue = 1191;
            }
        }
    }
}
