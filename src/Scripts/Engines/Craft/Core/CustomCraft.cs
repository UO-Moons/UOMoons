using Server.Items;
using System;

namespace Server.Engines.Craft;

public abstract class CustomCraft
{
	protected CustomCraft(Mobile from, CraftItem craftItem, CraftSystem craftSystem, Type typeRes, ITool tool, int quality)
	{
		From = from;
		CraftItem = craftItem;
		CraftSystem = craftSystem;
		TypeRes = typeRes;
		Tool = tool;
		Quality = quality;
	}

	public Mobile From { get; }
	public CraftItem CraftItem { get; }
	public CraftSystem CraftSystem { get; }
	public Type TypeRes { get; }
	public ITool Tool { get; }
	public int Quality { get; }
	public abstract void EndCraftAction();

	public abstract Item CompleteCraft(out int message);
}
