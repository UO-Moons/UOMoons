using System;
using Server.Engines.VeteranRewards;
using Server.Items;
using Server.Multis;
using Server.Network;
using Server.Spells;

namespace Server.Mobiles
{
    public class EtherealAncientHellHound : EtherealMount
    {
        public override int LabelNumber => 1155723;  // Ancient Hell Hound Statuette

        [Constructable]
        public EtherealAncientHellHound()
            : base(0x3FFD, 0x3EC9, 0x3EC9, DefaultEtherealHue)
        {
            Transparent = false;
        }

        public EtherealAncientHellHound(Serial serial)
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
