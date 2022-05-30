using System;
using Server.Items;
using Server.Engines.Quests;

namespace Server.Mobiles
{
    public class AndreasVesalius : MondainQuester
    {
        public override Type[] Quests => new Type[]
                {
                    typeof(KnowThineEnemyQuest)
                };

        [Constructable]
        public AndreasVesalius()
            : base("Andreas Vesalius", "the Anatomy Instructor")
        {
            SetSkill(SkillName.Anatomy, 120.0, 120.0);
            SetSkill(SkillName.Parry, 120.0, 120.0);
            SetSkill(SkillName.Healing, 120.0, 120.0);
            SetSkill(SkillName.Tactics, 120.0, 120.0);
            SetSkill(SkillName.Swords, 120.0, 120.0);
            SetSkill(SkillName.Focus, 120.0, 120.0);
        }

        public AndreasVesalius(Serial serial)
            : base(serial)
        {
        }

        public override void Advertise()
        {
            Say(1078138); // Learning of the body will allow you to excel in combat.
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
            AddItem(new BlackStaff());
            AddItem(new Boots());
            AddItem(new LongPants());
            AddItem(new Tunic(0x66D));
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
