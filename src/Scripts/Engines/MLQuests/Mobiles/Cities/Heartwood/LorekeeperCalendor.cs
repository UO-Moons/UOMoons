using System;
using Server.Items;
using Server.Engines.Quests;

namespace Server.Mobiles
{
    public class Calendor : MondainQuester
    {
        [Constructable]
        public Calendor()
            : base("Lorekeeper Calendor", "the Keeper of Tradition")
        {
            SetSkill(SkillName.Meditation, 60.0, 83.0);
            SetSkill(SkillName.Focus, 60.0, 83.0);
        }

        public Calendor(Serial serial)
            : base(serial)
        {
        }

        public override Type[] Quests => new Type[]
                {
                    typeof(DreadhornQuest)
                };

        public override void InitBody()
        {
            InitStats(100, 100, 25);

            Female = true;
            Race = Race.Elf;

            Hue = 0x847E;
            HairItemID = 0x2FD0;
            HairHue = 0x1F2;
        }

        public override void InitOutfit()
        {
            AddItem(new ElvenBoots(0x65A));
            AddItem(new ElvenShirt(0x728));
            AddItem(new Kilt(0x1BB));
            AddItem(new RoyalCirclet());
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
