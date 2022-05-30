using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles
{
    public class Andric : MondainQuester
    {
        [Constructable]
        public Andric()
            : base("Andric", "the Archer Trainer")
        {
            SetSkill(SkillName.Archery, 65.0, 88.0);
        }

        public Andric(Serial serial)
            : base(serial)
        {
        }

        public override Type[] Quests => new Type[]
                {
                    typeof(SplitEndsQuest)
                };
        public override void InitBody()
        {
            InitStats(100, 100, 25);

            Female = false;
            CantWalk = true;
            Race = Race.Human;

            Hue = 0x8407;
            HairItemID = 0x2049;
            HairHue = 0x6CE;
        }

        public override void InitOutfit()
        {
            AddItem(new Backpack());
            AddItem(new Boots(0x1BB));

            Item item;

            item = new LeatherLegs
            {
                Hue = 0x6C8
            };
            AddItem(item);

            item = new LeatherGloves
            {
                Hue = 0x1BB
            };
            AddItem(item);

            item = new LeatherChest
            {
                Hue = 0x1BB
            };
            AddItem(item);

            item = new LeatherArms
            {
                Hue = 0x4C7
            };
            AddItem(item);

            item = new CompositeBow
            {
                Hue = 0x5DD
            };
            AddItem(item);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}
