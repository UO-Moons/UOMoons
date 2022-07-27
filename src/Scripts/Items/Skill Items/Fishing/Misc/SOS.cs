using Server.Gumps;
using System;
using Server.Misc;

namespace Server.Items;

[Flipable(0x14ED, 0x14EE)]
public class SOS : BaseItem
{
	public override int LabelNumber
	{
		get
		{
			if (IsAncient)
				return 1063450; // an ancient SOS

			return 1041081; // a waterstained SOS
		}
	}

	private int m_Level;
	private Point3D m_TargetLocation;

	[CommandProperty(AccessLevel.GameMaster)]
	public bool IsAncient => m_Level >= 4;

	[CommandProperty(AccessLevel.GameMaster)]
	public int Level
	{
		get => m_Level;
		set
		{
			m_Level = Math.Max(1, Math.Min(value, 4));
			UpdateHue();
			InvalidateProperties();
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	private Map TargetMap { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public Point3D TargetLocation
	{
		get => m_TargetLocation;
		set => m_TargetLocation = value;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	private int MessageIndex { get; set; }

	private void UpdateHue()
	{
		Hue = IsAncient ? 0x481 : 0;
	}

	[Constructable]
	public SOS() : this(Map.Trammel)
	{
	}

	[Constructable]
	private SOS(Map map) : this(map, MessageInABottle.GetRandomLevel())
	{
	}

	[Constructable]
	public SOS(Map map, int level) : base(0x14EE)
	{
		Weight = 1.0;

		m_Level = level;
		MessageIndex = Utility.Random(MessageEntry.Entries.Length);
		TargetMap = map;
		m_TargetLocation = FindLocation(TargetMap);

		UpdateHue();
	}

	public SOS(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version

		writer.Write(m_Level);

		writer.Write(TargetMap);
		writer.Write(m_TargetLocation);
		writer.Write(MessageIndex);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		int version = reader.ReadInt();

		switch (version)
		{
			case 0:
			{
				m_Level = reader.ReadInt();

				TargetMap = reader.ReadMap();
				m_TargetLocation = reader.ReadPoint3D();
				MessageIndex = reader.ReadInt();

				break;
			}
		}
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (IsChildOf(from.Backpack))
		{
			MessageEntry entry;

			if (MessageIndex >= 0 && MessageIndex < MessageEntry.Entries.Length)
				entry = MessageEntry.Entries[MessageIndex];
			else
				entry = MessageEntry.Entries[MessageIndex = Utility.Random(MessageEntry.Entries.Length)];

			//from.CloseGump( typeof( MessageGump ) );
			from.SendGump(new MessageGump(entry, TargetMap, m_TargetLocation));
		}
		else
		{
			from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
		}
	}

	private static readonly Rectangle2D[] m_BritRegions = { new(0, 0, 5120, 4096) };
	private static readonly Rectangle2D[] m_IlshRegions = { new(1472, 272, 304, 240), new(1240, 1000, 312, 160) };
	private static readonly Rectangle2D[] m_MalasRegions = { new(1376, 1520, 464, 280) };

	private static Point3D FindLocation(Map map)
	{
		if (map == null || map == Map.Internal)
			return Point3D.Zero;

		Rectangle2D[] regions;

		if (map == Map.Felucca || map == Map.Trammel)
			regions = m_BritRegions;
		else if (map == Map.Ilshenar)
			regions = m_IlshRegions;
		else if (map == Map.Malas)
			regions = m_MalasRegions;
		else
			regions = new[] { new Rectangle2D(0, 0, map.Width, map.Height) };

		if (regions.Length == 0)
			return Point3D.Zero;

		for (int i = 0; i < 50; ++i)
		{
			Rectangle2D reg = regions[Utility.Random(regions.Length)];
			int x = Utility.Random(reg.X, reg.Width);
			int y = Utility.Random(reg.Y, reg.Height);

			if (!Helpers.ValidateDeepWater(map, x, y))
				continue;

			bool valid = true;

			for (int j = 1, offset = 5; valid && j <= 5; ++j, offset += 5)
			{
				if (!Helpers.ValidateDeepWater(map, x + offset, y + offset))
					valid = false;
				else if (!Helpers.ValidateDeepWater(map, x + offset, y - offset))
					valid = false;
				else if (!Helpers.ValidateDeepWater(map, x - offset, y + offset))
					valid = false;
				else if (!Helpers.ValidateDeepWater(map, x - offset, y - offset))
					valid = false;
			}

			if (valid)
				return new Point3D(x, y, 0);
		}

		return Point3D.Zero;
	}

	private class MessageGump : Gump
	{
		public MessageGump(MessageEntry entry, Map map, Point3D loc) : base(150, 50)
		{
			int xLong = 0, yLat = 0;
			int xMins = 0, yMins = 0;
			bool xEast = false, ySouth = false;

			var fmt = Sextant.Format(loc, map, ref xLong, ref yLat, ref xMins, ref yMins, ref xEast, ref ySouth) ? $"{yLat}°{yMins}'{(ySouth ? "S" : "N")},{xLong}°{xMins}'{(xEast ? "E" : "W")}" : "?????";

			AddPage(0);

			AddBackground(0, 40, 350, 300, 2520);

			AddHtmlLocalized(30, 80, 285, 160, 1018326, true, true); /* This is a message hastily scribbled by a passenger aboard a sinking ship.
																			* While it is probably too late to save the passengers and crew,
																			* perhaps some treasure went down with the ship!
																			* The message gives the ship's last known sextant co-ordinates.
																			*/

			AddHtml(35, 240, 230, 20, fmt, false, false);

			AddButton(35, 265, 4005, 4007, 0, GumpButtonType.Reply, 0);
			AddHtmlLocalized(70, 265, 100, 20, 1011036, false, false); // OKAY
		}
	}

	private class MessageEntry
	{
		private int Width { get; }
		private int Height { get; }
		private string Message { get; }

		private MessageEntry(int width, int height, string message)
		{
			Width = width;
			Height = height;
			Message = message;
		}

		public static MessageEntry[] Entries { get; } = {
			new( 280, 180, "...Ar! {0} and a fair wind! No chance... storms, though--ar! Is that a sea serp...<br><br>uh oh." ),
			new( 280, 215, "...been inside this whale for three days now. I've run out of food I can pick out of his teeth. I took a sextant reading through the blowhole: {0}. I'll never see my treasure again..." ),
			new( 280, 285, "...grand adventure! Captain Quacklebush had me swab down the decks daily...<br>  ...pirates came, I was in the rigging practicing with my sextant. {0} if I am not mistaken...<br>  ....scuttled the ship, and our precious cargo went with her and the screaming pirates, down to the bottom of the sea..." ),
			new( 280, 180, "Help! Ship going dow...n heavy storms...precious cargo...st reach dest...current coordinates {0}...ve any survivors... ease!" ),
			new( 280, 215, "...know that the wreck is near {0} but have not found it. Could the message passed down in my family for generations be wrong? No... I swear on the soul of my grandfather, I will find..." ),
			new( 280, 195, "...never expected an iceberg...silly woman on bow crushed instantly...send help to {0}...ey'll never forget the tragedy of the sinking of the Miniscule..." ),
			new( 280, 265, "...nobody knew I was a girl. They just assumed I was another sailor...then we met the undine. {0}. It was demanded sacrifice...I was youngset, they figured...<br>  ...grabbed the captain's treasure, screamed, 'It'll go down with me!'<br>  ...they took me up on it." ),
			new( 280, 230, "...so I threw the treasure overboard, before the curse could get me too. But I was too late. Now I am doomed to wander these seas, a ghost forever. Join me: seek ye at {0} if thou wishest my company..." ),
			new( 280, 285, "...then the ship exploded. A dragon swooped by. The slime swallowed Bertie whole--he screamed, it was amazing. The sky glowed orange. A sextant reading put us at {0}. Norma was chattering about sailing over the edge of the world. I looked at my hands and saw through them..." ),
			new( 280, 285, "...trapped on a deserted island, with a magic fountain supplying wood, fresh water springs, gorgeous scenery, and my lovely young wife. I know the ship with all our life's earnings sank at {0} but I don't know what our coordinates are... someone has GOT to rescue me before Sunday's finals game or I'll go mad..." ),
			new( 280, 160, "WANTED: divers exp...d in shipwre...overy. Must have own vess...pply at {0}<br>...good benefits, flexible hours..." ),
			new( 280, 250, "...was a cad and a boor, no matter what momma s...rew him overboard! Oh, Anna, 'twas so exciting!<br>  Unfort...y he grabbe...est, and all his riches went with him!<br>  ...sked the captain, and he says we're at {0}<br>...so maybe..." )
		};
	}
}
