using Server.Mobiles;

namespace Server.Items
{
	public class KhaldunPitTeleporter : BaseItem
	{
		[CommandProperty(AccessLevel.GameMaster)]
		private bool Active { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		private Point3D PointDest { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		private Map MapDest { get; set; }

		public override int LabelNumber => 1016511;  // the floor of the cavern seems to have collapsed here - a faint light is visible at the bottom of the pit

		[Constructable]
		public KhaldunPitTeleporter() : this(new Point3D(5451, 1374, 0), Map.Felucca)
		{
		}

		[Constructable]
		public KhaldunPitTeleporter(Point3D pointDest, Map mapDest) : base(0x053B)
		{
			Movable = false;
			Hue = 1;

			Active = true;
			PointDest = pointDest;
			MapDest = mapDest;
		}

		public KhaldunPitTeleporter(Serial serial) : base(serial)
		{
		}

		public override void OnDoubleClick(Mobile m)
		{
			if (!Active)
				return;

			Map map = MapDest;

			if (map == null || map == Map.Internal)
				_ = m.Map;

			Point3D p = PointDest;

			if (p == Point3D.Zero)
				_ = m.Location;

			if (m.InRange(this, 3))
			{
				BaseCreature.TeleportPets(m, PointDest, MapDest);

				m.MoveToWorld(PointDest, MapDest);
			}
			else
			{
				m.SendLocalizedMessage(1019045); // I can't reach that.
			}
		}

		public override void OnDoubleClickDead(Mobile m)
		{
			OnDoubleClick(m);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);

			writer.Write(Active);
			writer.Write(PointDest);
			writer.Write(MapDest);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			reader.ReadInt();

			Active = reader.ReadBool();
			PointDest = reader.ReadPoint3D();
			MapDest = reader.ReadMap();
		}
	}
}
