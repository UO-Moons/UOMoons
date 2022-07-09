using System.Linq;

namespace Server.Items;

public enum HaochisPigmentType
{
	None,
	HeartwoodSienna,
	CampionWhite,
	YewishPine,
	MinocianFire,
	NinjaBlack,
	Olive,
	DarkReddishBrown,
	Yellow,
	PrettyPink,
	MidnightBlue,
	Emerald,
	SmokyGold,
	GhostsGrey,
	OceanBlue,
	CelticLime
}

public class HaochisPigment : BasePigmentsOfTokuno
{
	private HaochisPigmentType _type;

	[Constructable]
	public HaochisPigment()
		: this(HaochisPigmentType.None, 50)
	{
	}

	[Constructable]
	public HaochisPigment(HaochisPigmentType type)
		: this(type, 50)
	{
	}

	[Constructable]
	public HaochisPigment(HaochisPigmentType type, int uses)
		: base(uses)
	{
		Weight = 1.0;
		Type = type;
	}

	public HaochisPigment(Serial serial)
		: base(serial)
	{
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public HaochisPigmentType Type
	{
		get => _type;
		set
		{
			_type = value;

			HoachisPigmentInfo info = Table.FirstOrDefault(x => x.Type == _type);

			if (info != null)
			{
				Hue = info.Hue;
				Label = info.Localization;
			}
			else
			{
				Hue = 0;
				Label = -1;
			}
		}
	}

	public override int LabelNumber => 1071249;  // Haochi's Pigments

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0);
		writer.Write((int)_type);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadInt();
		Type = (HaochisPigmentType)reader.ReadInt();
	}

	public static HoachisPigmentInfo[] Table { get; } =
	{
		new( HaochisPigmentType.None, 0, -1 ),
		new( HaochisPigmentType.HeartwoodSienna, 2739, 1157275 ), // Verified
		new( HaochisPigmentType.CampionWhite, 2738, 1157274 ), // Verified
		new( HaochisPigmentType.YewishPine, 2737, 1157273 ), // Verified
		new( HaochisPigmentType.MinocianFire, 2736, 1157272 ), // Verified
		new( HaochisPigmentType.NinjaBlack, 1108, 1071246 ), // Verified
		new( HaochisPigmentType.Olive, 1196, 1018352 ), // Verified
		new( HaochisPigmentType.DarkReddishBrown, 1148, 1071247 ), // Verified
		new( HaochisPigmentType.Yellow, 1169, 1071245 ), // Verified
		new( HaochisPigmentType.PrettyPink, 1168, 1071244 ), // Verified
		new( HaochisPigmentType.MidnightBlue, 1156, 1071248 ), // Verified
		new( HaochisPigmentType.Emerald, 1173, 1023856 ), // Verified
		new( HaochisPigmentType.SmokyGold, 1801, 1115467 ), // Verified
		new( HaochisPigmentType.GhostsGrey, 1000, 1115468 ), // Verified
		new( HaochisPigmentType.OceanBlue, 1195, 1115471 ), // Verified
		new( HaochisPigmentType.CelticLime, 2733, 1157269 ), // Verified
	};

	public class HoachisPigmentInfo
	{
		public HaochisPigmentType Type { get; }
		public int Hue { get; }
		public int Localization { get; }

		public HoachisPigmentInfo(HaochisPigmentType type, int hue, int loc)
		{
			Type = type;
			Hue = hue;
			Localization = loc;
		}
	}
}
