using System;
using Server.Engines.VeteranRewards;
using Server.Items;
using Server.Multis;
using Server.Network;
using Server.Spells;

namespace Server.Mobiles
{
    public class ChargerOfTheFallen : EtherealMount
    {
        [Constructable]
        public ChargerOfTheFallen()
            : base(0x2D9C, 0x3E92, 0x3E92, DefaultEtherealHue)
        {
            Transparent = false;
        }

        public ChargerOfTheFallen(Serial serial)
            : base(serial)
        { }

        public override int LabelNumber => 1074816;  // Charger of the Fallen Statuette

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version == 0)
            {
                Transparent = false;
            }
        }
    }
}
