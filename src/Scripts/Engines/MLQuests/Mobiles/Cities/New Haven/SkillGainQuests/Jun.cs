using System;
using Server.Items;
using Server.Engines.Quests;

namespace Server.Mobiles
{
    public class Jun : MondainQuester
    {
        public override Type[] Quests => new Type[]
                {
                    typeof(WalkingSilentlyQuest)
                };

        [Constructable]
        public Jun()
            : base("Jun", "the Stealth Instructor")
        {
            SetSkill(SkillName.Hiding, 120.0, 120.0);
            SetSkill(SkillName.Tactics, 120.0, 120.0);
            SetSkill(SkillName.Tracking, 120.0, 120.0);
            SetSkill(SkillName.Fencing, 120.0, 120.0);
            SetSkill(SkillName.Stealth, 120.0, 120.0);
            SetSkill(SkillName.Ninjitsu, 120.0, 120.0);
        }

        public Jun(Serial serial)
            : base(serial)
        {
        }

        public override void Advertise()
        {
            Say(1078175); // Walk Silently. Remain unseen. I can teach you.
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
            AddItem(new SamuraiTabi());
            AddItem(new LeatherNinjaPants());
            AddItem(new LeatherNinjaHood());
            AddItem(new LeatherNinjaBelt());
            AddItem(new LeatherNinjaMitts());
            AddItem(new LeatherNinjaJacket());
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
