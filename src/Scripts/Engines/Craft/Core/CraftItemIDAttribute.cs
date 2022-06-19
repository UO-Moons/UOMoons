using System;

namespace Server.Engines.Craft;

[AttributeUsage(AttributeTargets.Class)]
public class CraftItemIdAttribute : Attribute
{
	public CraftItemIdAttribute(int itemId)
	{
		ItemId = itemId;
	}

	public int ItemId { get; }
}
