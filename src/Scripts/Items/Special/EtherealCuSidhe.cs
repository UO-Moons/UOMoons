using System;
using Server.Engines.VeteranRewards;
using Server.Items;
using Server.Multis;
using Server.Network;
using Server.Spells;

namespace Server.Mobiles
{
    public class EtherealCuSidhe : EtherealMount
    {
        [Constructable]
        public EtherealCuSidhe()
            : base(0x2D96, 0x3E91, 0x3E91, DefaultEtherealHue)
        {
            Transparent = false;
        }

        public EtherealCuSidhe(Serial serial)
            : base(serial)
        { }

        public override int LabelNumber => 1080386;  // Ethereal Cu Sidhe Statuette
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();

            if (version == 0)
            {
                Transparent = false;
            }
        }
    }
}
