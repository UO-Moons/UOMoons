using System;
using Server.Mobiles;
using System.Collections.Generic;

namespace Server.Items;

public class PrismOfLightAltar : PeerlessAltar
{
	private int _mId;
	public override int KeyCount => 3;
	public override MasterKey MasterKey => new PrismOfLightKey();
	public List<Item> Pedestals = new();

	public override Type[] Keys => new[]
	{
		typeof(JaggedCrystals), typeof(BrokenCrystals), typeof(PiecesOfCrystal),
		typeof(CrushedCrystals), typeof(ScatteredCrystals), typeof(ShatteredCrystals)
	};

	public override BasePeerless Boss => new ShimmeringEffusion();

	[Constructable]
	public PrismOfLightAltar() : base(0x2206)
	{
		Visible = false;

		BossLocation = new Point3D(6520, 122, -20);
		TeleportDest = new Point3D(6520, 139, -20);
		ExitDest = new Point3D(3785, 1107, 20);

		_mId = 0;
	}

	public override void ClearContainer()
	{
		base.ClearContainer();

		Pedestals.ForEach(x => x.Hue = ((PrismOfLightPillar)x).OrgHue);
	}

	public override Rectangle2D[] BossBounds { get; } = {
		new(6500, 111, 45, 35),
	};

	public PrismOfLightAltar(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(1); // version

		writer.Write(Pedestals, true);

		writer.Write(_mId);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		int version = reader.ReadInt();

		switch (version)
		{
			case 1:
			{
				Pedestals = reader.ReadStrongItemList();
				goto case 0;
			}
			case 0:
			{
				_mId = reader.ReadInt();
				break;
			}
		}
	}

	public int GetId()
	{
		int id = _mId;
		_mId += 1;
		return id;
	}
}

public class PrismOfLightPillar : Container
{
	public override int LabelNumber => 1024643;  // pedestal

	private PrismOfLightAltar _mAltar;
	private int _mOrgHue;

	[CommandProperty(AccessLevel.GameMaster)]
	public PrismOfLightAltar Altar
	{
		get => _mAltar;
		set
		{
			_mAltar = value;

			if (!_mAltar.Pedestals.Contains(this))
				_mAltar.Pedestals.Add(this);
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int ID { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int OrgHue
	{
		get => _mOrgHue;
		set
		{
			_mOrgHue = value;
			Hue = _mOrgHue;
			InvalidateProperties();
		}
	}

	public PrismOfLightPillar(PrismOfLightAltar altar, int hue)
		: base(0x207D)
	{
		OrgHue = hue;
		Movable = false;

		_mAltar = altar;

		if (_mAltar != null)
		{
			ID = _mAltar.GetId();
			_mAltar.Pedestals.Add(this);
		}
	}

	public PrismOfLightPillar(Serial serial) : base(serial)
	{
	}

	public override bool OnDragDrop(Mobile from, Item dropped)
	{
		if (_mAltar == null)
			return false;

		if (dropped.GetType() == _mAltar.Keys[ID])
		{
			if (_mAltar.OnDragDrop(from, dropped))
			{
				Hue = 36;
				return true;
			}
		}
		else
		{
			from.SendLocalizedMessage(1072682); // This is not the proper key.
		}

		return false;
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(1); // version

		writer.Write(_mOrgHue);

		writer.Write(ID);
		writer.Write(_mAltar);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		int version = reader.ReadInt();

		switch (version)
		{
			case 1:
			{
				_mOrgHue = reader.ReadInt();
				goto case 0;
			}
			case 0:
			{
				ID = reader.ReadInt();
				_mAltar = reader.ReadItem() as PrismOfLightAltar;

				break;
			}
		}

		if (version < 1)
		{
			if (_mOrgHue == 0)
				_mOrgHue = Hue;

			if (_mAltar != null && !_mAltar.Pedestals.Contains(this))
				_mAltar.Pedestals.Add(this);
		}
	}
}
