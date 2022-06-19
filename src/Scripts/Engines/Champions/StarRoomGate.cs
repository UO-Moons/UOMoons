using System;

namespace Server.Items;

public sealed class StarRoomGate : Moongate
{
	private bool _mDecays;
	private DateTime _mDecayTime;
	private Timer _mTimer;
	[Constructable]
	public StarRoomGate()
		: this(false)
	{
	}

	[Constructable]
	public StarRoomGate(bool decays, Point3D loc, Map map)
		: this(decays)
	{
		MoveToWorld(loc, map);
		Effects.PlaySound(loc, map, 0x20E);
	}

	[Constructable]
	public StarRoomGate(bool decays)
		: base(new Point3D(5143, 1774, 0), Map.Felucca)
	{
		Dispellable = false;
		ItemId = 0x1FD4;

		if (!decays)
			return;

		_mDecays = true;
		_mDecayTime = DateTime.UtcNow + TimeSpan.FromMinutes(2.0);

		_mTimer = new InternalTimer(this, _mDecayTime);
		_mTimer.Start();
	}

	public StarRoomGate(Serial serial)
		: base(serial)
	{
	}

	public override int LabelNumber => 1049498;// dark moongate
	public override void OnAfterDelete()
	{
		_mTimer?.Stop();

		base.OnAfterDelete();
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0);

		writer.Write(_mDecays);

		if (_mDecays)
			writer.WriteDeltaTime(_mDecayTime);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		int version = reader.ReadInt();

		switch (version)
		{
			case 0:
			{
				_mDecays = reader.ReadBool();

				if (_mDecays)
				{
					_mDecayTime = reader.ReadDeltaTime();

					_mTimer = new InternalTimer(this, _mDecayTime);
					_mTimer.Start();
				}

				break;
			}
		}
	}

	private class InternalTimer : Timer
	{
		private readonly Item _mItem;
		public InternalTimer(Item item, DateTime end)
			: base(end - DateTime.UtcNow)
		{
			_mItem = item;
		}

		protected override void OnTick()
		{
			_mItem.Delete();
		}
	}
}
