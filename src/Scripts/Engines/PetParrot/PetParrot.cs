using Server.Items;
using Server.Multis;
using System;

namespace Server.Mobiles
{
	public sealed class PetParrot : BaseCreature
	{
		[Constructable]
		public PetParrot()
			: this(DateTime.MinValue, null, 0)
		{
		}

		[Constructable]
		public PetParrot(DateTime birth, string name, int hue)
			: base(AIType.AI_Animal, FightMode.None, 10, 1, 0.2, 0.4)
		{
			Name = "a pet parrot";
			Title = "the parrot";
			Body = 0x11A;
			BaseSoundId = 0xBF;

			SetStr(1, 5);
			SetDex(25, 30);
			SetInt(2);

			SetHits(1, Str);
			SetStam(25, Dex);
			SetMana(0);

			SetResistance(ResistanceType.Physical, 2);

			SetSkill(SkillName.MagicResist, 4);
			SetSkill(SkillName.Tactics, 4);
			SetSkill(SkillName.Wrestling, 4);

			CantWalk = true;
			Frozen = true;
			Blessed = true;

			Birth = birth != DateTime.MinValue ? birth : DateTime.UtcNow;

			if (name != null)
				Name = name;

			if (hue > 0)
				Hue = hue;
		}

		public PetParrot(Serial serial)
			: base(serial)
		{
		}

		public override bool NoHouseRestrictions => true;
		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime Birth { get; set; }
		public override FoodType FavoriteFood => FoodType.FruitsAndVegies;

		public static int GetWeeks(DateTime birth)
		{
			TimeSpan span = DateTime.UtcNow - birth;
			return (int)(span.TotalDays / 7);
		}

		public override void OnStatsQuery(Mobile from)
		{
			if (from.Map != Map || !from.InUpdateRange(from) || !from.CanSee(this)) return;
			BaseHouse house = BaseHouse.FindHouseAt(this);

			if (house != null && house.IsCoOwner(from) && from.IsPlayer())
				from.SendLocalizedMessage(1072625); // As the house owner, you may rename this Parrot.

			if (house != null && house.IsCoOwner(from) && from.IsStaff() )
				from.SendAsciiMessage("As the house owner and staff member, you may rename this Parrot.");

			Network.MobileStatus.Send(from.NetState, this);
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			int weeks = GetWeeks(Birth);

			switch (weeks)
			{
				case 1:
					list.Add(1072626); // 1 week old
					break;
				case > 1:
					list.Add(1072627, weeks.ToString()); // ~1_AGE~ weeks old
					break;
			}
		}

		public override bool CanBeRenamedBy(Mobile from)
		{
			if (from.AccessLevel > (int)AccessLevel.Player)
				return true;

			BaseHouse house = BaseHouse.FindHouseAt(this);

			return house != null && house.IsCoOwner(from);
		}

		public override void OnSpeech(SpeechEventArgs e)
		{
			base.OnSpeech(e);

			if (!(Utility.RandomDouble() < 0.05)) return;
			Say(e.Speech);
			PlaySound(0xC0);
		}

		public override bool OnDragDrop(Mobile from, Item dropped)
		{
			if (dropped is not ParrotWafer) return false;
			dropped.Delete();

			switch (Utility.Random(6))
			{
				case 0:
					Say(1072602, "#" + Utility.RandomMinMax(1012003, 1012010));
					break; // I just flew in from ~1_CITYNAME~ and boy are my wings tired!
				case 1:
					Say(1072603);
					break; // Wind in the sails!  Wind in the sails!
				case 2:
					Say(1072604);
					break; // Arrrr, matey!
				case 3:
					Say(1072605);
					break; // Loot and plunder!  Loot and plunder!
				case 4:
					Say(1072606);
					break; // I want a cracker!
				case 5:
					Say(1072607);
					break; // I'm just a house pet!
			}

			PlaySound(Utility.RandomMinMax(0xBF, 0xC3));
			Direction = Utility.GetDirection(this, from);

			return true;

		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);
			writer.Write(Birth);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			_ = reader.ReadInt();

			Birth = reader.ReadDateTime();
			Frozen = true;
		}
	}
}
