using System;

namespace Server
{
	public class BounceInfo
	{
		public Map m_Map;
		public Point3D m_Location, m_WorldLoc;
		public IEntity m_Parent;
		public object m_ParentStack;
		public byte m_GridLocation;
		public Mobile m_Mobile;

		public BounceInfo(Mobile from, Item item)
		{
			m_Map = item.Map;
			m_Location = item.Location;
			m_WorldLoc = item.GetWorldLocation();
			m_Parent = item.Parent;
			m_ParentStack = null;
			m_GridLocation = item.GridLocation;
			m_Mobile = from;
		}

		private BounceInfo(Map map, Point3D loc, Point3D worldLoc, IEntity parent)
		{
			m_Map = map;
			m_Location = loc;
			m_WorldLoc = worldLoc;
			m_Parent = parent;
			m_ParentStack = null;
		}

		public static BounceInfo Deserialize(GenericReader reader)
		{
			if (reader.ReadBool())
			{
				Map map = reader.ReadMap();
				Point3D loc = reader.ReadPoint3D();
				Point3D worldLoc = reader.ReadPoint3D();

				IEntity parent;

				Serial serial = reader.ReadInt();

				if (serial.IsItem)
					parent = World.FindItem(serial);
				else if (serial.IsMobile)
					parent = World.FindMobile(serial);
				else
					parent = null;

				return new BounceInfo(map, loc, worldLoc, parent);
			}
			else
			{
				return null;
			}
		}

		public static void Serialize(BounceInfo info, GenericWriter writer)
		{
			if (info == null)
			{
				writer.Write(false);
			}
			else
			{
				writer.Write(true);

				writer.Write(info.m_Map);
				writer.Write(info.m_Location);
				writer.Write(info.m_WorldLoc);

				if (info.m_Parent is Mobile mobile)
					writer.Write(mobile);
				else if (info.m_Parent is Item item)
					writer.Write(item);
				else
					writer.Write((Serial)0);
			}
		}
	}

}
