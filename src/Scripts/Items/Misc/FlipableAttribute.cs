using Server.Commands;
using Server.Targeting;
using System;
using System.Reflection;

namespace Server.Items;

public class FlipCommandHandlers
{
	public static void Initialize()
	{
		CommandSystem.Register("Flip", AccessLevel.GameMaster, Flip_OnCommand);
	}

	[Usage("Flip")]
	[Description("Turns an item.")]
	private static void Flip_OnCommand(CommandEventArgs e)
	{
		e.Mobile.Target = new FlipTarget();
	}

	private class FlipTarget : Target
	{
		public FlipTarget()
			: base(-1, false, TargetFlags.None)
		{
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (targeted is not Item item)
				return;

			if (item.Movable == false && from.AccessLevel == AccessLevel.Player)
				return;

			Type type = item.GetType();

			FlipableAttribute[] attributeArray = (FlipableAttribute[])type.GetCustomAttributes(typeof(FlipableAttribute), false);

			if (attributeArray.Length == 0)
			{
				return;
			}

			FlipableAttribute fa = attributeArray[0];

			fa.Flip(item);
		}
	}
}

[AttributeUsage(AttributeTargets.Class)]
public class DynamicFlipingAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class FlipableAttribute : Attribute
{
	public int[] ItemIDs { get; }

	public FlipableAttribute()
		: this(null)
	{
	}

	public FlipableAttribute(params int[] itemIDs)
	{
		ItemIDs = itemIDs;
	}

	public void Flip(Item item)
	{
		if (ItemIDs == null)
		{
			try
			{
				MethodInfo flipMethod = item.GetType().GetMethod("Flip", Type.EmptyTypes);
				if (flipMethod != null)
					flipMethod.Invoke(item, Array.Empty<object>());
			}
			catch
			{
				// ignored
			}
		}
		else
		{
			int index = 0;
			for (int i = 0; i < ItemIDs.Length; i++)
			{
				if (item.ItemId == ItemIDs[i])
				{
					index = i + 1;
					break;
				}
			}

			if (index > ItemIDs.Length - 1)
				index = 0;

			item.ItemId = ItemIDs[index];
		}
	}
}
