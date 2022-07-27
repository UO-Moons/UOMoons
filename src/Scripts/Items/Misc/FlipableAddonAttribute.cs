using Server.Multis;
using System;
using System.Reflection;

namespace Server.Items;

[AttributeUsage(AttributeTargets.Class)]
public sealed class FlipableAddonAttribute : Attribute
{
	private static readonly string m_MethodName = "Flip";

	private static readonly Type[] m_Params = {
		typeof( Mobile ), typeof( Direction )
	};

	private Direction[] Directions { get; }

	public FlipableAddonAttribute(params Direction[] directions)
	{
		Directions = directions;
	}

	public void Flip(Mobile from, Item addon)
	{
		if (Directions is { Length: > 1 })
		{
			try
			{
				MethodInfo flipMethod = addon.GetType().GetMethod(m_MethodName, m_Params);

				if (flipMethod != null)
				{
					int index = 0;

					for (int i = 0; i < Directions.Length; i++)
					{
						if (addon.Direction == Directions[i])
						{
							index = i + 1;
							break;
						}
					}

					if (index >= Directions.Length)
						index = 0;

					ClearComponents(addon);

					flipMethod.Invoke(addon, new object[2] { from, Directions[index] });

					BaseHouse house = null;
					AddonFitResult result = AddonFitResult.Valid;

					addon.Map = Map.Internal;

					result = addon switch
					{
						BaseAddon baseAddon => baseAddon.CouldFit(baseAddon.Location, from.Map, from, ref house),
						BaseAddonContainer container => container.CouldFit(container.Location, from.Map, from,
							ref house),
						_ => result
					};

					addon.Map = from.Map;

					if (result != AddonFitResult.Valid)
					{
						if (index == 0)
							index = Directions.Length - 1;
						else
							index -= 1;

						ClearComponents(addon);

						flipMethod.Invoke(addon, new object[2] { from, Directions[index] });

						switch (result)
						{
							case AddonFitResult.Blocked:
								from.SendLocalizedMessage(500269); // You cannot build that there.
								break;
							case AddonFitResult.NotInHouse:
								from.SendLocalizedMessage(500274); // You can only place this in a house that you own!
								break;
							case AddonFitResult.DoorsNotClosed:
								from.SendMessage("You must close all house doors before placing this.");
								break;
							case AddonFitResult.DoorTooClose:
								from.SendLocalizedMessage(500271); // You cannot build near the door.
								break;
							case AddonFitResult.NoWall:
								from.SendLocalizedMessage(500268); // This object needs to be mounted on something.
								break;
						}
					}

					addon.Direction = Directions[index];
				}
			}
			catch
			{
				// ignored
			}
		}
	}

	private void ClearComponents(Item item)
	{
		switch (item)
		{
			case BaseAddon baseAddon:
			{
				foreach (AddonComponent c in baseAddon.Components)
				{
					c.Addon = null;
					c.Delete();
				}

				baseAddon.Components.Clear();
				break;
			}
			case BaseAddonContainer container:
			{
				foreach (AddonContainerComponent c in container.Components)
				{
					c.Addon = null;
					c.Delete();
				}

				container.Components.Clear();
				break;
			}
		}
	}
}
