using Server.Items;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Engines.Champions;

public sealed class ChampionSkullBrazier : AddonComponent
{
	private ChampionSkullPlatform _mPlatform;
	private ChampionSkullType _mType;
	private Item _mSkull;
	public ChampionSkullBrazier(ChampionSkullPlatform platform, ChampionSkullType type)
		: base(0x19BB)
	{
		Hue = 0x455;
		Light = LightType.Circle300;

		_mPlatform = platform;
		_mType = type;
	}

	public ChampionSkullBrazier(Serial serial)
		: base(serial)
	{
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public ChampionSkullPlatform Platform => _mPlatform;
	[CommandProperty(AccessLevel.GameMaster)]
	private ChampionSkullType Type
	{
		get => _mType;
		set
		{
			_mType = value;
			InvalidateProperties();
		}
	}
	[CommandProperty(AccessLevel.GameMaster)]
	public Item Skull
	{
		get => _mSkull;
		private set
		{
			_mSkull = value;
			_mPlatform?.Validate();
		}
	}
	public override int LabelNumber => 1049489 + (int)_mType;
	public override void OnDoubleClick(Mobile from)
	{
		_mPlatform?.Validate();

		BeginSacrifice(from);
	}

	private void BeginSacrifice(Mobile from)
	{
		if (Deleted)
			return;

		if (_mSkull is {Deleted: true})
			Skull = null;

		if (from.Map != Map || !from.InRange(GetWorldLocation(), 3))
		{
			from.SendLocalizedMessage(500446); // That is too far away.
		}
		else if (!Harrower.CanSpawn)
		{
			from.SendMessage("The harrower has already been spawned.");
		}
		else if (_mSkull == null)
		{
			from.SendLocalizedMessage(1049485); // What would you like to sacrifice?
			from.Target = new SacrificeTarget(this);
		}
		else
		{
			SendLocalizedMessageTo(from, 1049487, ""); // I already have my champions awakening skull!
		}
	}

	private void EndSacrifice(Mobile from, ChampionSkull skull)
	{
		if (Deleted)
			return;

		if (_mSkull is {Deleted: true})
			Skull = null;

		if (from.Map != Map || !from.InRange(GetWorldLocation(), 3))
		{
			from.SendLocalizedMessage(500446); // That is too far away.
		}
		else if (!Harrower.CanSpawn)
		{
			from.SendMessage("The harrower has already been spawned.");
		}
		else if (skull == null)
		{
			SendLocalizedMessageTo(from, 1049488, ""); // That is not my champions awakening skull!
		}
		else if (_mSkull != null)
		{
			SendLocalizedMessageTo(from, 1049487, ""); // I already have my champions awakening skull!
		}
		else if (!skull.IsChildOf(from.Backpack))
		{
			from.SendLocalizedMessage(1049486); // You can only sacrifice items that are in your backpack!
		}
		else
		{
			if (skull.Type == Type)
			{
				skull.Movable = false;
				skull.MoveToWorld(GetWorldTop(), Map);

				Skull = skull;
			}
			else
			{
				SendLocalizedMessageTo(from, 1049488, ""); // That is not my champions awakening skull!
			}
		}
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
		writer.Write((int)_mType);
		writer.Write(_mPlatform);
		writer.Write(_mSkull);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		var version = reader.ReadInt();

		switch (version)
		{
			case 0:
			{
				_mType = (ChampionSkullType)reader.ReadInt();
				_mPlatform = reader.ReadItem() as ChampionSkullPlatform;
				_mSkull = reader.ReadItem();

				if (_mPlatform == null)
					Delete();

				break;
			}
		}
	}

	private class SacrificeTarget : Target
	{
		private readonly ChampionSkullBrazier _mBrazier;
		public SacrificeTarget(ChampionSkullBrazier brazier)
			: base(12, false, TargetFlags.None)
		{
			_mBrazier = brazier;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			_mBrazier.EndSacrifice(from, targeted as ChampionSkull);
		}
	}
}
