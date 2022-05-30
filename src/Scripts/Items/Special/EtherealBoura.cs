using System;
using Server.Engines.VeteranRewards;
using Server.Items;
using Server.Multis;
using Server.Network;
using Server.Spells;

namespace Server.Mobiles
{
    public class EtherealBoura : EtherealMount
    {
        public override int LabelNumber => 1150006;  // Rideable Boura Statuette

        [Constructable]
        public EtherealBoura()
            : base(0x46F8, 0x3EC6, 0x3EC6, DefaultEtherealHue)
        {
            Transparent = false;
        }

        public EtherealBoura(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(2); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version == 1)
            {
                Transparent = false;
            }
        }
    }
}
