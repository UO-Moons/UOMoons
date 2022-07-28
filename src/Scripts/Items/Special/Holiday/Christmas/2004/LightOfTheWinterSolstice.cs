namespace Server.Items;

[Flipable(0x236E, 0x2371)]
public class LightOfTheWinterSolstice : BaseItem
{
	private static readonly string[] m_StaffNames = {
		"Ando",
		"EvilMantis",
		"GM Rend JB",
		"MrsTroublemaker",
		"Saralah",
		"TTSO",
		"Axiom",   "Excrom",  "GM Snark",    "Jinsol",  "MrTact",  "Sharkbait",   "ValQor",
		"Binky",   "Farmer Farley",   "GM Sowl", "Kalag",   "Mung",    "Sheperd Vex",
		"Cadillac",    "Fenris",  "GM Unicta",   "L.Lantz", "Neojonez",    "Silvani", "Wildcat",
		"Carbon",  "Fertbert",    "GM Wasia",    "LadyLu",  "Niobe",   "Skunky",  "Wilki",
		"Cheap Book",  "Foster",  "GM Zoer", "Leurocian",   "Oaks",   "Spada",   "Willow",
		"Cyrus",   "Galess",  "GunGix",  "LongBow",  "Ogel",  "Sparkle",  "Wraith",
		"Deko",  "GM Blantry",  "Hanse",  "M.Cory",  "Orbeus",  "Speedman",  "Ya-Ssan",
		"Draconis Rex",  "GM Comforl",  "Hugo",  "Malachite",  "Platinum",  "Stormwind",  "Yeti",
		"Echa",  "Firwood",  "GM Licatia",  "Hyacinth",  "Mantisa",  "Purple",  "Sunsword",  "Zilo",
		"Ender",  "GM Marby",  "Imirian Maul",  "Rugen",  "The Intern",
		"Eva",  "GM Prume",  "Inoia",  "MeatShield",  "Ryujin",  "Torikichi"
	};

	[CommandProperty(AccessLevel.GameMaster)]
	private string Dipper { get; set; }

	[Constructable]
	public LightOfTheWinterSolstice() : this(m_StaffNames[Utility.Random(m_StaffNames.Length)])
	{
	}

	[Constructable]
	private LightOfTheWinterSolstice(string dipper) : base(0x236E)
	{
		Dipper = dipper;

		Weight = 1.0;
		LootType = LootType.Blessed;
		Light = LightType.Circle300;
		Hue = Utility.RandomDyedHue();
	}

	public LightOfTheWinterSolstice(Serial serial) : base(serial)
	{
	}

	public override void OnSingleClick(Mobile from)
	{
		base.OnSingleClick(from);

		LabelTo(from, 1070881, Dipper); // Hand Dipped by ~1_name~
		LabelTo(from, 1070880); // Winter 2004
	}

	public override void GetProperties(ObjectPropertyList list)
	{
		base.GetProperties(list);

		list.Add(1070881, Dipper); // Hand Dipped by ~1_name~
		list.Add(1070880); // Winter 2004
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version

		writer.Write(Dipper);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		int version = reader.ReadInt();

		Dipper = version switch
		{
			0 => reader.ReadString(),
			_ => Dipper
		};

		if (Dipper != null)
			Dipper = string.Intern(Dipper);
	}
}
