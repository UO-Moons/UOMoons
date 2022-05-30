using System;
using Server.Engines.VeteranRewards;
using Server.Items;
using Server.Multis;
using Server.Network;
using Server.Spells;

namespace Server.Mobiles
{
    public class EtherealTiger : EtherealMount
    {
        [Constructable]
        public EtherealTiger()
            : this(false)
        {
        }

        [Constructable]
        public EtherealTiger(bool light = false)
            : base(0x9844, light ? 0x3EC7 : 0x3EC8, light ? 0x3EC7 : 0x3EC8, DefaultEtherealHue)
        {
            Transparent = false;
        }

        public EtherealTiger(Serial serial)
            : base(serial)
        { }

        public override int LabelNumber => 1154589;  // Ethereal Tiger Statuette

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
