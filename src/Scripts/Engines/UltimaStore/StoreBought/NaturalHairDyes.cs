using Server.Gumps;
using Server.Mobiles;
using System.Linq;

namespace Server.Items;

public enum HairDyeType
{
	None,
	LemonLime,
	YewBrown,
	BloodwoodRed,
	VividBlue,
	AshBlonde,
	HeartwoodGreen,
	OakBlonde,
	SacredWhite,
	FrostwoodIceGreen,
	FieryBlonde,
	BitterBrown,
	GnawsTwistedBlue,
	DuskBlack,
}

public class NaturalHairDye : BaseItem
{
	private HairDyeType _type;
	private TextDefinition _label;

	[Constructable]
	public NaturalHairDye(HairDyeType type)
		: base(0xEFE)
	{
		Weight = 1.0;
		Type = type;
	}

	public NaturalHairDye(Serial serial)
		: base(serial)
	{
	}

	protected TextDefinition Label
	{
		get => _label;
		set
		{
			_label = value;
			InvalidateProperties();
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public HairDyeType Type
	{
		get => _type;
		set
		{
			_type = value;

			HairDyeInfo info = Table.FirstOrDefault(x => x.Type == _type);

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

	public override int LabelNumber => 1071387;  // Natural Hair Dye

	public override void GetProperties(ObjectPropertyList list)
	{
		base.GetProperties(list);

		if (_label > 0)
			TextDefinition.AddTo(list, _label);
	}

	public override void OnDoubleClick(Mobile m)
	{
		if (IsChildOf(m.Backpack))
		{
			BaseGump.SendGump(new HairDyeConfirmGump(m as PlayerMobile, Hue, this));
		}
		else
		{
			m.SendLocalizedMessage(1042010); //You must have the object in your backpack to use it.
		}
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0);

		writer.WriteEncodedInt((int)_type);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadInt();

		Type = (HairDyeType)reader.ReadEncodedInt();
	}

	public static HairDyeInfo[] Table { get; } =
	{
		// Hue, Label
		new( HairDyeType.None, 0, -1 ),
		new( HairDyeType.LemonLime, 1167, 1071439 ), // NOT Confirmed
		new( HairDyeType.YewBrown, 1192, 1071470 ), // Confirmed
		new( HairDyeType.BloodwoodRed, 1194, 1071471 ), // NOT Confirmed
		new( HairDyeType.VividBlue, 1152, 1071438 ), // NOT Confirmed
		new( HairDyeType.AshBlonde, 1191, 1071469 ), // Confirmed
		new( HairDyeType.HeartwoodGreen, 1193, 1071472 ), // NOT Confirmed
		new( HairDyeType.OakBlonde, 2010, 1071468 ), // Confirmed
		new( HairDyeType.SacredWhite, 1153, 1071474 ), // NOT Confirmed
		new( HairDyeType.FrostwoodIceGreen, 1151, 1071473 ), // NOT Confirmed
		new( HairDyeType.FieryBlonde, 1174, 1071440 ), // NOT Confirmed
		new( HairDyeType.BitterBrown, 1149, 1071437 ), // NOT Confirmed
		new( HairDyeType.GnawsTwistedBlue, 1195, 1071442 ), // Confirmed
		new( HairDyeType.DuskBlack, 1175, 1071441 ), // NOT Confirmed
	};

	public class HairDyeInfo
	{
		public HairDyeType Type { get; }
		public int Hue { get; }
		public int Localization { get; }

		public HairDyeInfo(HairDyeType type, int hue, int loc)
		{
			Type = type;
			Hue = hue;
			Localization = loc;
		}
	}
}

public class HairDyeConfirmGump : BaseGump
{
	public int Hue { get; }
	public Item Dye { get; }

	public HairDyeConfirmGump(PlayerMobile pm, int hue, Item dye)
		: base(pm, 200, 200)
	{
		Hue = hue;
		Dye = dye;

		pm.CloseGump(typeof(HairDyeConfirmGump));
	}

	public override void AddGumpLayout()
	{
		AddBackground(0, 0, 340, 200, 3600);
		AddBackground(0, 0, 100, 100, 3600);
		AddBackground(100, 140, 227, 50, 9270);

		AddImage(-39, -23, 50702, Dye.Hue - 1);

		AddHtmlLocalized(110, 25, 205, 80, 1074396, C32216(0x0080FF), false, false); // This special hair dye is made of a unique mixture of leaves, permanently changing one's hair color until another dye is used.
		AddHtmlLocalized(120, 155, 160, 20, 1074395, C32216(0x00FFFF), false, false); // <div align=right>Use Permanent Hair Dye</div>

		AddButton(290, 157, 0x7538, 0x7539, 1, GumpButtonType.Reply, 0);
	}

	public override void OnResponse(RelayInfo info)
	{
		if (info.ButtonID == 1)
		{
			if (!Dye.Deleted && Dye.IsChildOf(User.Backpack))
			{
				if (User.HairItemId != 0)
				{
					User.HairHue = Hue;

					User.SendLocalizedMessage(501199);  // You dye your hair
					Dye.Delete();
					User.PlaySound(0x4E);
				}
				else
				{
					User.SendLocalizedMessage(502623); // You have no hair to dye and you cannot use this.
				}
			}
			else
			{
				User.SendLocalizedMessage(1042010); //You must have the object in your backpack to use it.
			}
		}
	}
}
