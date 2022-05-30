using System;
using Server.Items;
using Server.Engines.Quests;

namespace Server.Mobiles
{
    public class Sleen : MondainQuester
    {
        [Constructable]
        public Sleen()
            : base("Sleen", "the Trinket Weaver")
        {
            SetSkill(SkillName.Meditation, 60.0, 83.0);
            SetSkill(SkillName.Focus, 60.0, 83.0);
        }

        public Sleen(Serial serial)
            : base(serial)
        {
        }

        public override Type[] Quests => new Type[]
                {
                    typeof(ArchSupportQuest),
                    typeof(StopHarpingOnMeQuest),
                    typeof(TheFarEyeQuest),
                    typeof(NecessitysMotherQuest),
                    typeof(TickTockQuest),
                    typeof(FromTheGaultierCollectionQuest),
                    typeof(ReptilianDentistQuest)
                };
        public override void InitBody()
        {
            InitStats(100, 100, 25);

            Female = true;
            Race = Race.Elf;

            Hue = 0x83E6;
            HairItemID = 0x2FC0;
            HairHue = 0x386;
        }

        public override void InitOutfit()
        {
            AddItem(new ElvenBoots(0x901));
            AddItem(new FullApron(0x1BB));
            AddItem(new ShortPants(0x71));
            AddItem(new Cloak(0x73C));
            AddItem(new ElvenShirt());
            AddItem(new SmithHammer());
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
