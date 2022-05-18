namespace Server.Engines.TownHouses
{
	public class DecoreItemInfo
	{
		private Point3D c_Location;

		public string TypeString { get; private set; }
		public string Name { get; private set; }
		public int ItemID { get; private set; }
		public int Hue { get; private set; }

		public Point3D Location => c_Location;

		public Map Map { get; private set; }

		public DecoreItemInfo()
		{
		}

		public DecoreItemInfo(string typestring, string name, int itemid, int hue, Point3D loc, Map map)
		{
			TypeString = typestring;
			ItemID = itemid;
			c_Location = loc;
			Map = map;
		}

		public void Save(GenericWriter writer)
		{
			writer.Write(0);
			writer.Write(Hue);
			writer.Write(Name);
			writer.Write(TypeString);
			writer.Write(ItemID);
			writer.Write(c_Location);
			writer.Write(Map);
		}

		public void Load(GenericReader reader)
		{
			_ = reader.ReadInt();
			Hue = reader.ReadInt();
			Name = reader.ReadString();
			TypeString = reader.ReadString();
			ItemID = reader.ReadInt();
			c_Location = reader.ReadPoint3D();
			Map = reader.ReadMap();
		}
	}
}
