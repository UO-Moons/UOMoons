namespace Server.Menus
{
	public class ItemListEntry
	{
		public string Name { get; }
		public int ItemID { get; }
		public int Hue { get; }
		public int CraftIndex { get; }

		public ItemListEntry(string name, int itemID) : this(name, itemID, 0)
		{
		}

		public ItemListEntry(string name, int itemID, int hue) : this(name, itemID, 0, 0)
		{
		}

		public ItemListEntry(string name, int itemID, int hue, int craftIndex)
		{
			Name = name;
			ItemID = itemID;
			Hue = hue;
			CraftIndex = craftIndex;
		}
	}
}
