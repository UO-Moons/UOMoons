using System;
using Server.Mobiles;

namespace Server.Items;

public class BedlamAltar : PeerlessAltar
{
	public override int KeyCount => 3;
	public override MasterKey MasterKey => new BedlamKey();

	public override Type[] Keys => new[]
	{
		typeof( LibrariansKey )
	};

	public override BasePeerless Boss => new MonstrousInterredGrizzle();

	[Constructable]
	public BedlamAltar() : base(0x207E)
	{
		BossLocation = new Point3D(106, 1615, 90);
		TeleportDest = new Point3D(101, 1623, 50);
		ExitDest = new Point3D(2068, 1372, -75);
	}

	public override Rectangle2D[] BossBounds { get; } = {
		new(99, 1609, 14, 18),
	};

	public BedlamAltar(Serial serial) : base(serial)
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
