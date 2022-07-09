using Server.ContextMenus;
using Server.Gumps;
using Server.Multis;
using Server.Network;
using System.Collections.Generic;

namespace Server.Items;

public class SingingBall : BaseItem, ISecurable
{
	public override int LabelNumber => 1041245;  // Singing Ball

	private bool _turnedOn;

	[CommandProperty(AccessLevel.GameMaster)]
	public bool TurnedOn
	{
		get => _turnedOn;
		set
		{
			_turnedOn = value;
			InvalidateProperties();
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public SecureLevel Level { get; set; }

	[Constructable]
	public SingingBall()
		: this(0xE2E)
	{
	}

	[Constructable]
	public SingingBall(int itemId)
		: base(itemId)
	{
		Weight = 10.0;
		LootType = LootType.Blessed;

		Light = LightType.Circle300;
	}

	public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
	{
		base.GetContextMenuEntries(from, list);

		SetSecureLevelEntry.AddTo(from, this, list);
	}

	public bool CheckAccessible(Mobile from, Item item)
	{
		if (from.AccessLevel >= AccessLevel.GameMaster)
			return true; // Staff can access anything

		BaseHouse house = BaseHouse.FindHouseAt(item);

		if (house == null)
			return false;

		return Level switch
		{
			SecureLevel.Owner => house.IsOwner(from),
			SecureLevel.CoOwners => house.IsCoOwner(from),
			SecureLevel.Friends => house.IsFriend(from),
			SecureLevel.Anyone => true,
			SecureLevel.Guild => house.IsGuildMember(from),
			_ => false
		};
	}

	public SingingBall(Serial serial)
		: base(serial)
	{
	}

	public override bool HandlesOnMovement => _turnedOn && IsLockedDown;

	public override void OnMovement(Mobile m, Point3D oldLocation)
	{
		if (_turnedOn && IsLockedDown && (!m.Hidden || m.IsPlayer()) && Utility.InRange(m.Location, Location, 2) && !Utility.InRange(oldLocation, Location, 2))
		{
			Effects.PlaySound(Location, Map, SoundList());
		}

		base.OnMovement(m, oldLocation);
	}

	public virtual int SoundList()
	{
		return Utility.RandomMinMax(0, 1338);
	}

	public override void GetProperties(ObjectPropertyList list)
	{
		base.GetProperties(list);

		list.Add(_turnedOn ? 502695 : 502696);
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (CheckAccessible(from, this))
		{
			OnOffGump onOffGump = new(this);
			from.SendGump(onOffGump);
		}
		else
		{
			PublicOverheadMessage(MessageType.Regular, 0x3E9, 1061637); // You are not allowed to access 
		}
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(1); // version

		writer.Write((int)Level);
		writer.Write(_turnedOn);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		int version = reader.ReadInt();

		if (version > 0)
			Level = (SecureLevel)reader.ReadInt();

		_turnedOn = reader.ReadBool();
	}

	private class OnOffGump : Gump
	{
		private readonly SingingBall _singingBall;

		public OnOffGump(SingingBall ball)
			: base(150, 200)
		{
			_singingBall = ball;

			AddBackground(0, 0, 300, 150, 0xA28);

			AddHtmlLocalized(45, 20, 300, 35, ball.TurnedOn ? 1011035 : 1011034, false, false); // [De]Activate this item

			AddButton(40, 53, 0xFA5, 0xFA7, 1, GumpButtonType.Reply, 0);
			AddHtmlLocalized(80, 55, 65, 35, 1011036, false, false); // OKAY

			AddButton(150, 53, 0xFA5, 0xFA7, 0, GumpButtonType.Reply, 0);
			AddHtmlLocalized(190, 55, 100, 35, 1011012, false, false); // CANCEL
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			Mobile from = sender.Mobile;

			if (info.ButtonID == 1)
			{
				bool newValue = !_singingBall.TurnedOn;

				_singingBall.TurnedOn = newValue;

				if (newValue && !_singingBall.IsLockedDown)
					from.SendLocalizedMessage(502693); // Remember, this only works when locked down.
			}
			else
			{
				from.SendLocalizedMessage(502694); // Cancelled action.
			}
		}
	}
}
