using Server.Mobiles;

namespace Server.Engines.Exodus;

public class VerLorRegController : Item
{
	private static bool _active;

	[CommandProperty(AccessLevel.GameMaster)]
	public static bool Active
	{
		get => _active;
		set { if (value) Start(); else Stop(); }
	}

	[CommandProperty(AccessLevel.Administrator)]
	public static ClockworkExodus Mobile { get; private set; }

	[CommandProperty(AccessLevel.Administrator)]
	public static VerLorRegController IlshenarInstance { get; set; }

	public VerLorRegController() : base(7107)
	{
		Name = "Ver Lor Reg Controller";
		Visible = false;
		Movable = false;

		Start();
	}

	public VerLorRegController(Serial serial)
		: base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);

		writer.Write(_active);
		writer.Write(Mobile);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadInt();
		_active = reader.ReadBool();
		Mobile = (ClockworkExodus)reader.ReadMobile();

		if (Map == Map.Ilshenar)
			IlshenarInstance = this;
	}

	public static void Start()
	{
		if (_active)
			return;

		_active = true;

		if (Mobile != null)
			return;
		ClockworkExodus m = new()
		{
			Home = new Point3D(854, 642, -40),
			RangeHome = 4
		};
		m.MoveToWorld(new Point3D(854, 642, -40), Map.Ilshenar);
		Mobile = m;
	}

	public static void Stop()
	{
		if (!_active)
			return;

		_active = false;
		Mobile.Delete();
		Mobile = null;
	}
}
