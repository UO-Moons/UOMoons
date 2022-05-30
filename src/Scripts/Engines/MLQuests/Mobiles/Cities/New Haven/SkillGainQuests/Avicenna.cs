using System;
using Server.Items;
using Server.Engines.Quests;

namespace Server.Mobiles
{
    public class Avicenna : MondainQuester
    {
        public override Type[] Quests => new Type[]
                {
                    typeof(BruisesBandagesAndBloodQuest)
                };

        [Constructable]
        public Avicenna()
            : base("Avicenna", "the Healing Instructor")
        {
            SetSkill(SkillName.Anatomy, 120.0, 120.0);
            SetSkill(SkillName.Parry, 120.0, 120.0);
            SetSkill(SkillName.Healing, 120.0, 120.0);
            SetSkill(SkillName.Tactics, 120.0, 120.0);
            SetSkill(SkillName.Swords, 120.0, 120.0);
            SetSkill(SkillName.Focus, 120.0, 120.0);
        }

        public Avicenna(Serial serial)
            : base(serial)
        {
        }

        public override void Advertise()
        {
            Say(1078137); // A warrior needs to learn how to apply bandages to wounds.
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
            AddItem(new Robe(0x66D));
            AddItem(new Boots());
            AddItem(new GnarledStaff());
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
