using Server.Gumps;
using Server.Multis;
using Server.Network;

namespace Server.Items;

public class BaseStatuette : BaseItem
{
	private bool _mTurnedOn;
	[Constructable]
	public BaseStatuette(int itemId)
		: base(itemId)
	{
		LootType = LootType.Blessed;
	}

	public BaseStatuette(Serial serial)
		: base(serial)
	{
	}

	public override bool HandlesOnMovement => _mTurnedOn && IsLockedDown;
	[CommandProperty(AccessLevel.GameMaster)]
	public bool TurnedOn
	{
		get => _mTurnedOn;
		set
		{
			_mTurnedOn = value;
			InvalidateProperties();
		}
	}
	public override double DefaultWeight => 1.0;
	public override void OnMovement(Mobile m, Point3D oldLocation)
	{
		if (_mTurnedOn && IsLockedDown && (!m.Hidden || m.IsPlayer()) && Utility.InRange(m.Location, Location, 2) && !Utility.InRange(oldLocation, Location, 2))
		{
			PlaySound(m);
		}

		base.OnMovement(m, oldLocation);
	}

	public virtual void PlaySound(Mobile to)
	{
	}

	public override void GetProperties(ObjectPropertyList list)
	{
		base.GetProperties(list);

		list.Add(_mTurnedOn ? 502695 : 502696);// turned on : turned off
	}

	public bool IsOwner(Mobile mob)
	{
		BaseHouse house = BaseHouse.FindHouseAt(this);

		return house != null && house.IsOwner(mob);
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (IsOwner(from))
		{
			OnOffGump onOffGump = new(this);
			_ = from.SendGump(onOffGump);
		}
		else
		{
			from.SendLocalizedMessage(502691); // You must be the owner to use this.
		}
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0);

		writer.Write(_mTurnedOn);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		int version = reader.ReadInt();

		switch (version)
		{
			case 0:
			{
				_mTurnedOn = reader.ReadBool();
				break;
			}
		}
	}

	private class OnOffGump : Gump
	{
		private readonly BaseStatuette _mStatuette;
		public OnOffGump(BaseStatuette statuette)
			: base(150, 200)
		{
			_mStatuette = statuette;

			AddBackground(0, 0, 300, 150, 0xA28);

			AddHtmlLocalized(45, 20, 300, 35, statuette.TurnedOn ? 1011035 : 1011034, false, false); // [De]Activate this item

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
				bool newValue = !_mStatuette.TurnedOn;
				_mStatuette.TurnedOn = newValue;

				if (newValue && !_mStatuette.IsLockedDown)
					from.SendLocalizedMessage(502693); // Remember, this only works when locked down.
			}
			else
			{
				from.SendLocalizedMessage(502694); // Cancelled action.
			}
		}
	}
}
