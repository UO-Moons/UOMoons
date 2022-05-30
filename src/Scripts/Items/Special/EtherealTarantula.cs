using System;
using Server.Engines.VeteranRewards;
using Server.Items;
using Server.Multis;
using Server.Network;
using Server.Spells;

namespace Server.Mobiles
{
    public class EtherealTarantula : EtherealMount
    {
        public override int LabelNumber => 1157081;  // Tarantula Statuette

        [Constructable]
        public EtherealTarantula()
            : base(0x9DD6, 0x3ECA, 0x3ECA, DefaultEtherealHue)
        {
            Transparent = false;
        }


        public EtherealTarantula(Serial serial)
            : base(serial)
        {
        }

        public override bool Validate(Mobile from)
        {
            #region TOL
            if (from.NetState != null && !from.NetState.SupportsExpansion(Expansion.ML))
            {
                from.SendLocalizedMessage(1156139); // * You must upgrade to the Time of Legends in order to use this. *                               
                return false;
            }
            #endregion

            return base.Validate(from);
        }

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
