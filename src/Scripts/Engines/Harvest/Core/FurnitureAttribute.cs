using System;
using System.Linq;
using Server.Items;

namespace Server;

[AttributeUsage(AttributeTargets.Class)]
public class FurnitureAttribute : Attribute
{
	private static bool IsNotChoppables(Item item)
	{
		return NotChoppables.Any(t => t == item.GetType());
	}

	private static readonly Type[] NotChoppables = {
		typeof(CommodityDeedBox), typeof(JewelryBox)
	};

	public static bool Check(Item item)
	{
		if (item == null)
		{
			return false;
		}

		if (IsNotChoppables(item))
		{
			return false;
		}

		if (item.GetType().IsDefined(typeof(FurnitureAttribute), false))
		{
			return true;
		}

		if (item is AddonComponent {Addon: { }} component && component.Addon.GetType().IsDefined(typeof(FurnitureAttribute), false))
		{
			return true;
		}

		return false;
	}
}
