using Server.Engines.Quests;
using System;

namespace Server.Items
{
    public class SmithsSatchel : BaseContainer, IDyable
    {
        [Constructable]
        public SmithsSatchel()
            : base(0xE75)
        {
            Hue = BaseReward.SatchelHue();
            Layer = Layer.Backpack;
            Weight = 3.0;
            DropItem(new IronIngot(10));
            DropItem(new SmithHammer());
        }

        public SmithsSatchel(Serial serial)
            : base(serial)
        {
        }
        public bool Dye(Mobile from, DyeTub sender)
        {
            if (Deleted)
            {
                return false;
            }

            Hue = sender.DyedHue;

            return true;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}
