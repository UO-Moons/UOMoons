using Server.Mobiles;

namespace Server.Items;

public class HalloweenPumpkin : BaseItem
{
	private static readonly string[] m_Staff =
	{
		"Adida",
		"Adolescence",
		"Ando",
		"Argain",
		"Arrenai",
		"Baron Mind",
		"Cadillac",
		"CatHat",
		"Cerulean",
		"Co",
		"Coelacanth",
		"Comforl",
		"Coolio",
		"Cyrus",
		"Czarzane",
		"Dark Hanako",
		"Darkscribe",
		"Draconi",
		"Draconis Rex",
		"DragonHead",
		"Drake",
		"Elendrik",
		"Ender",
		"Fenris",
		"Fylwyn",
		"Glamdring",
		"Goto",
		"GrumpyMartyr",
		"Gustus",
		"Hannel",
		"Hazel",
		"HoppyGirl",
		"Inoia",
		"JIB",
		"Jyrra",
		"Kalag",
		"LagMan",
		"Leto",
		"Leurocian",
		"Licatia",
		"Marby",
		"Masara",
		"Mesanna",
		"Mostly Harmless",
		"MrsTroubleMaker",
		"MrTact",
		"Mythfire",
		"Nina",
		"Nyssa",
		"Petrucchio",
		"Prume",
		"PurpleTurtle",
		"RabidFuzzle",
		"Reico",
		"Rend",
		"Runna",
		"Sameerah",
		"Serado",
		"Sienna",
		"Silvani",
		"Skunky",
		"Snark",
		"Sorif",
		"Sowl",
		"Spada",
		"Stormwind",
		"Supreem",
		"TheGrimmOmen",
		"Theowulf",
		"Towein",
		"Tulkas",
		"Uril",
		"Vou",
		"Wasia",
		"Wilki",
		"Willow",
		"Wolf",
		"Wraith",
		"Xena",
		"Yamada",
		"Ya-Ssan",
		"Yeti",
		"Zilo",
		"Zoer"
	};

	[Constructable]
	public HalloweenPumpkin()
	{
		Weight = Utility.RandomMinMax(3, 20);
		ItemId = Utility.RandomDouble() <= .02 ? Utility.RandomList(0x4694, 0x4698) : Utility.RandomList(0xc6a, 0xc6b, 0xc6c);
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (!from.InRange(GetWorldLocation(), 2))
			return;

		bool douse = false;

		switch (ItemId)
		{
			case 0x4694: ItemId = 0x4691; break;
			case 0x4691: ItemId = 0x4694; douse = true; break;
			case 0x4698: ItemId = 0x4695; break;
			case 0x4695: ItemId = 0x4698; douse = true; break;
			default: return;
		}

		from.SendLocalizedMessage(douse ? 1113988 : 1113987); // You extinguish/light the Jack-O-Lantern
		Effects.PlaySound(GetWorldLocation(), Map, douse ? 0x3be : 0x47);
	}

	private void AssignRandomName()
	{
		Name = $"{m_Staff[Utility.Random(m_Staff.Length)]}'s Jack-O-Lantern";
	}

	public override bool OnDragLift(Mobile from)
	{
		if (Name == null && ItemId is 0x4694 or 0x4691 or 0x4698 or 0x4695)
		{
			if (Utility.RandomBool())
			{
				new PumpkinHead().MoveToWorld(GetWorldLocation(), Map);

				Delete();
				return false;
			}

			AssignRandomName();
		}

		return true;
	}

	public HalloweenPumpkin(Serial serial)
		: base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadInt();
	}
}
