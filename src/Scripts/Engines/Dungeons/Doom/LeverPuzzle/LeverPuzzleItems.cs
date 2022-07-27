using Server.Mobiles;
using Server.Network;
using Server.Spells;
using System;

namespace Server.Engines.Doom;

public class LampRoomBox : BaseItem
{
	private LeverPuzzleController _mController;
	private Mobile _mWanderer;

	public LampRoomBox(LeverPuzzleController controller) : base(0xe80)
	{
		_mController = controller;
		ItemId = 0xe80;
		Movable = false;
	}

	public override void OnDoubleClick(Mobile m)
	{
		if (!m.InRange(GetWorldLocation(), 3))
			return;
		if (_mController.Enabled)
			return;

		if (_mWanderer is {Alive: true}) return;
		_mWanderer = new WandererOfTheVoid();
		_mWanderer.MoveToWorld(LeverPuzzleController.LrEnter, Map.Malas);
		_mWanderer.PublicOverheadMessage(MessageType.Regular, 0x3B2, 1060002, ""); // I am the guardian of...
		Timer.DelayCall(TimeSpan.FromSeconds(5.0), CallBackMessage);
	}

	private void CallBackMessage()
	{
		PublicOverheadMessage(MessageType.Regular, 0x3B2, 1060003, ""); // You try to pry the box open...
	}
	public override void OnAfterDelete()
	{
		if (_mController is { Deleted: false })
			_mController.Delete();
	}
	public LampRoomBox(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
		writer.Write(_mController);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadInt();
		_mController = reader.ReadItem() as LeverPuzzleController;
	}
}

public class LeverPuzzleStatue : BaseItem
{
	private LeverPuzzleController _mController;

	public LeverPuzzleStatue(int[] dat, LeverPuzzleController controller) : base(dat[0])
	{
		_mController = controller;
		Hue = 0x44E;
		Movable = false;
	}
	public override void OnAfterDelete()
	{
		if (_mController is { Deleted: false })
			_mController.Delete();
	}
	public LeverPuzzleStatue(Serial serial) : base(serial)
	{
	}
	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
		writer.Write(_mController);
	}
	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadInt();
		_mController = reader.ReadItem() as LeverPuzzleController;
	}
}

public class LeverPuzzleLever : BaseItem
{
	private LeverPuzzleController _mController;

	[CommandProperty(AccessLevel.GameMaster)]
	private ushort Code { get; set; }

	public LeverPuzzleLever(ushort code, LeverPuzzleController controller) : base(0x108E)
	{
		_mController = controller;
		Code = code;
		Hue = 0x66D;
		Movable = false;
	}

	public override void OnDoubleClick(Mobile m)
	{
		if (m != null && _mController.Enabled)
		{
			ItemId ^= 2;
			Effects.PlaySound(Location, Map, 0x3E8);
			_mController.LeverPulled(Code);
		}
		else
		{
			m?.SendLocalizedMessage(1060001); // You throw the switch, but the mechanism cannot be engaged again so soon.
		}
	}

	public override void OnAfterDelete()
	{
		if (_mController is {Deleted: false})
			_mController.Delete();
	}

	public LeverPuzzleLever(Serial serial) : base(serial)
	{
	}
	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
		writer.Write(Code);
		writer.Write(_mController);
	}
	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadInt();
		Code = reader.ReadUShort();
		_mController = reader.ReadItem() as LeverPuzzleController;
	}
}

[TypeAlias("Server.Engines.Doom.LampRoomTelePorter")]
public class LampRoomTeleporter : BaseItem
{
	public LampRoomTeleporter(int[] dat)
	{
		Hue = dat[1];
		ItemId = dat[0];
		Movable = false;
	}

	public override bool HandlesOnMovement => true;
	public override bool OnMoveOver(Mobile m)
	{
		if (m is not PlayerMobile) return true;
		if (SpellHelper.CheckCombat(m))
		{
			m.SendLocalizedMessage(1005564, 0x22); // Wouldst thou flee during the heat of battle??
		}
		else
		{
			BaseCreature.TeleportPets(m, LeverPuzzleController.LrExit, Map.Malas);
			m.MoveToWorld(LeverPuzzleController.LrExit, Map.Malas);
			return false;
		}
		return true;
	}

	public LampRoomTeleporter(Serial serial) : base(serial)
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
