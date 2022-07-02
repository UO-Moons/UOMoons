namespace Server.Menus;

public class ItemListEntry
{
	public string Name { get; }
	public int ItemId { get; }
	public int Hue { get; }
	public int CraftIndex { get; }

	public ItemListEntry(string name, int itemId) : this(name, itemId, 0)
	{
	}

	public ItemListEntry(string name, int itemId, int hue) : this(name, itemId, 0, 0)
	{
	}

	public ItemListEntry(string name, int itemId, int hue, int craftIndex)
	{
		Name = name;
		ItemId = itemId;
		Hue = hue;
		CraftIndex = craftIndex;
	}
}
