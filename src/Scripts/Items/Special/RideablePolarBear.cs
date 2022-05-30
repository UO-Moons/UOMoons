using System;
using Server.Engines.VeteranRewards;
using Server.Items;
using Server.Multis;
using Server.Network;
using Server.Spells;

namespace Server.Mobiles
{
    public class RideablePolarBear : EtherealMount
    {
        [Constructable]
        public RideablePolarBear()
            : base(0x20E1, 0x3EC5, 0x3EC5, DefaultEtherealHue)
        {
            Transparent = false;
        }

        public RideablePolarBear(Serial serial)
            : base(serial)
        { }

        public override int LabelNumber => 1076159;  // Rideable Polar Bear

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(2); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();

            if (version == 1)
            {
                Transparent = false;
            }
        }
    }
}
