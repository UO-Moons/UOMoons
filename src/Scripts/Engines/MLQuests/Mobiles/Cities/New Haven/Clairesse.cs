using System;
using Server.Engines.Quests;
using Server.Items;

namespace Server.Mobiles
{
	public class Clairesse : MondainQuester
    {
        [Constructable]
        public Clairesse()
            : base("Clairesse", "the Servant")
        {
        }

        public Clairesse(Serial serial)
            : base(serial)
        {
        }

        public override Type[] Quests => new Type[]
                {
                    typeof(StitchInTimeQuest)
                };
        public override void InitBody()
        {
            InitStats(100, 100, 25);

            Female = true;
            CantWalk = true;
            Race = Race.Human;

            Hue = 0x840B;
            HairItemID = 0x203D;
            HairHue = 0x458;
        }

        public override void InitOutfit()
        {
            AddItem(new Backpack());
            AddItem(new PlainDress(0x3C9));
            AddItem(new Shoes(0x740));
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            _ = reader.ReadInt();
        }
    }
}
