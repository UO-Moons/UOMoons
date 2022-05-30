using Server.Items;

namespace Server.Mobiles
{
	public class Aminia : BaseCreature
	{
		[Constructable]
		public Aminia()
			: base(AIType.AI_Melee, FightMode.None, 2, 1, 0.5, 2)
		{
			Name = "Aminia";
			Title = "the Master Weaponsmith's Wife";
			Blessed = true;

			InitStats(100, 100, 25);

			Female = true;
			Race = Race.Human;

			Hue = 0x83ED;
			HairItemID = 0x203B;
			HairHue = 0x454;

			AddItem(new Backpack());
			AddItem(new Sandals(0x75B));
			AddItem(new Tunic(0x4BF));
			AddItem(new Skirt(0x8FD));
		}

		public Aminia(Serial serial)
			: base(serial)
		{
		}

		public override void OnThink()
		{
			Clock.GetTime(Map, Location.X, Location.Y, out int hours);

			if (hours == 21)
			{
				Blessed = false;
				Body = 0x17;
			}
			else
			{
				Blessed = true;
				Body = 0x191;
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			_ = reader.ReadInt();
		}
	}
}
