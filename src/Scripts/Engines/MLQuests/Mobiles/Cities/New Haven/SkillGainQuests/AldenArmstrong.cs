using System;
using Server.Items;
using Server.Engines.Quests;

namespace Server.Mobiles
{
    public class AldenArmstrong : MondainQuester
    {
        public override Type[] Quests => new Type[]
                {
                    typeof(TheArtOfWarQuest)
                };

        [Constructable]
        public AldenArmstrong()
            : base("Alden Armstrong", "the Tactics Instructor")
        {
            SetSkill(SkillName.Anatomy, 120.0, 120.0);
            SetSkill(SkillName.Parry, 120.0, 120.0);
            SetSkill(SkillName.Healing, 120.0, 120.0);
            SetSkill(SkillName.Tactics, 120.0, 120.0);
            SetSkill(SkillName.Swords, 120.0, 120.0);
            SetSkill(SkillName.Focus, 120.0, 120.0);
        }

        public AldenArmstrong(Serial serial)
            : base(serial)
        {
        }

        public override void Advertise()
        {
            Say(1078136); // There is an art to slaying your enemies swiftly. It's called tactics, and I can teach it to you.
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
            AddItem(new Shoes());
            AddItem(new StuddedLegs());
            AddItem(new StuddedGloves());
            AddItem(new StuddedGorget());
            AddItem(new StuddedChest());
            AddItem(new StuddedArms());
            AddItem(new Katana());
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
