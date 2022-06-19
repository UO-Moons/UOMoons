namespace Server.Items;

public sealed class HarrowerGate : Moongate
{
	private Mobile _mHarrower;
	public HarrowerGate(Mobile harrower, Point3D loc, Map map, Point3D targLoc, Map targMap)
		: base(targLoc, targMap)
	{
		_mHarrower = harrower;

		Dispellable = false;
		ItemId = 0x1FD4;
		Light = LightType.Circle300;

		MoveToWorld(loc, map);
	}

	public HarrowerGate(Serial serial)
		: base(serial)
	{
	}

	public override int LabelNumber => 1049498;// dark moongate
	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
		writer.Write(_mHarrower);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		var version = reader.ReadInt();

		switch (version)
		{
			case 0:
			{
				_mHarrower = reader.ReadMobile();

				if (_mHarrower == null)
					Delete();

				break;
			}
		}
	}
}
