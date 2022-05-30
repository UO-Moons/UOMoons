using System;
using Server.Items;
using Server.Engines.Quests;

namespace Server.Mobiles
{
    public class Mulcivikh : MondainQuester
    {
        public override Type[] Quests => new Type[]
                {
                    typeof(TheAllureOfDarkMagicQuest)
                };

        public override void InitSBInfo()
        {
            SBInfos.Add(new SBMage());
        }

        [Constructable]
        public Mulcivikh()
            : base("Mulcivikh", "the Necromancy Instructor")
        {
            SetSkill(SkillName.Magery, 120.0, 120.0);
            SetSkill(SkillName.MagicResist, 120.0, 120.0);
            SetSkill(SkillName.SpiritSpeak, 120.0, 120.0);
            SetSkill(SkillName.Swords, 120.0, 120.0);
            SetSkill(SkillName.Meditation, 120.0, 120.0);
            SetSkill(SkillName.Necromancy, 120.0, 120.0);
        }

        public Mulcivikh(Serial serial)
            : base(serial)
        {
        }

        public override void Advertise()
        {
            Say(1078131); // Allured by dark magic, aren't you?
        }

        public override void OnOfferFailed()
        {
            Say(1077772); // I cannot teach you, for you know all I can teach!
        }

        public override void InitBody()
        {
            Female = false;
            CantWalk = true;
            Race = Race.Human;

            base.InitBody();
        }

        public override void InitOutfit()
        {
            AddItem(new Backpack());
            AddItem(new Sandals(0x8FD));
            AddItem(new BoneHelm());

            Item item;

            item = new LeatherLegs();
            item.Hue = 0x2C3;
            AddItem(item);

            item = new LeatherGloves();
            item.Hue = 0x2C3;
            AddItem(item);

            item = new LeatherGorget();
            item.Hue = 0x2C3;
            AddItem(item);

            item = new LeatherChest();
            item.Hue = 0x2C3;
            AddItem(item);

            item = new LeatherArms();
            item.Hue = 0x2C3;
            AddItem(item);
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
