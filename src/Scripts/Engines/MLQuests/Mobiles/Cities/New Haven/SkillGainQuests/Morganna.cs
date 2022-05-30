using System;
using Server.Items;
using Server.Engines.Quests;

namespace Server.Mobiles
{
    public class Morganna : MondainQuester
    {
        public override Type[] Quests => new Type[]
                {
                    typeof(ChannelingTheSupernaturalQuest)
                };

        public override void InitSBInfo()
        {
            SBInfos.Add(new SBMage());
        }

        [Constructable]
        public Morganna()
            : base("Morganna", "the Spirit Speak Instructor")
        {
            SetSkill(SkillName.Magery, 120.0, 120.0);
            SetSkill(SkillName.MagicResist, 120.0, 120.0);
            SetSkill(SkillName.SpiritSpeak, 120.0, 120.0);
            SetSkill(SkillName.Swords, 120.0, 120.0);
            SetSkill(SkillName.Meditation, 120.0, 120.0);
            SetSkill(SkillName.Necromancy, 120.0, 120.0);
        }

        public Morganna(Serial serial)
            : base(serial)
        {
        }

        public override void Advertise()
        {
            Say(1078132); // Want to learn how to channel the supernatural?
        }

        public override void OnOfferFailed()
        {
            Say(1077772); // I cannot teach you, for you know all I can teach!
        }

        public override void InitBody()
        {
            Female = true;
            CantWalk = true;
            Race = Race.Human;

            base.InitBody();
        }

        public override void InitOutfit()
        {
            AddItem(new Backpack());
            AddItem(new Robe(0x47D));
            AddItem(new SkullCap(0x455));
            AddItem(new Sandals());
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
