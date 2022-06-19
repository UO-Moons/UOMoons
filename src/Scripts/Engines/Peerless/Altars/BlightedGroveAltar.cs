using System;
using Server.Mobiles;

namespace Server.Items;

public class BlightedGroveAltar : PeerlessAltar
{
	public override int KeyCount => 3;
	public override MasterKey MasterKey => new BlightedGroveKey();

	public override Type[] Keys => new[]
	{
		typeof( DryadsBlessing )
	};

	public override BasePeerless Boss => new LadyMelisande();

	[Constructable]
	public BlightedGroveAltar() : base(0x207B)
	{
		BossLocation = new Point3D(6483, 947, 23);
		TeleportDest = new Point3D(6518, 946, 36);
		ExitDest = new Point3D(587, 1641, -1);
	}

	public override Rectangle2D[] BossBounds { get; } = {
		new(6456, 922, 84, 47),
	};

	public BlightedGroveAltar(Serial serial) : base(serial)
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
