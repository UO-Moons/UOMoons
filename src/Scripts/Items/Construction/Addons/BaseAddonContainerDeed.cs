using Server.Engines.Craft;
using Server.Multis;
using Server.Targeting;
using System;
using Server.Spells;

namespace Server.Items;

[Flipable(0x14F0, 0x14EF)]
public abstract class BaseAddonContainerDeed : BaseItem, ICraftable, IResource
{
	public abstract BaseAddonContainer Addon { get; }

	/*[CommandProperty(AccessLevel.GameMaster)]
	public Mobile Crafter { get; set; }

	private CraftResource m_Resource;

	[CommandProperty(AccessLevel.GameMaster)]
	public CraftResource Resource
	{
		get => m_Resource;
		set
		{
			if (m_Resource != value)
			{
				m_Resource = value;
				Hue = CraftResources.GetHue(m_Resource);

				InvalidateProperties();
			}
		}
	}*/

	public BaseAddonContainerDeed() : base(0x14F0)
	{
		Weight = 1.0;

		if (!Core.AOS)
			LootType = LootType.Newbied;
	}

	public BaseAddonContainerDeed(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version

		//writer.Write((int)m_Resource);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadInt();

		/*switch (version)
		{
			case 0:
				m_Resource = (CraftResource)reader.ReadInt();
				break;
		}*/
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (IsChildOf(from.Backpack))
			from.Target = new InternalTarget(this);
		else
			from.SendLocalizedMessage(1062334); // This item must be in your backpack to be used.
	}

	public override void GetProperties(ObjectPropertyList list)
	{
		base.GetProperties(list);

		if (!CraftResources.IsStandard(Resource))
			list.Add(CraftResources.GetLocalizationNumber(Resource));
	}

	#region ICraftable
	public virtual int OnCraft(int quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, ITool tool, CraftItem craftItem, int resHue)
	{
		Type resourceType = typeRes ?? craftItem.Resources.GetAt(0).ItemType;

		Resource = CraftResources.GetFromType(resourceType);

		CraftContext context = craftSystem.GetContext(from);

		if (context is { DoNotColor: true })
			Hue = 0;

		return quality;
	}
	#endregion

	private class InternalTarget : Target
	{
		private readonly BaseAddonContainerDeed m_Deed;

		public InternalTarget(BaseAddonContainerDeed deed) : base(-1, true, TargetFlags.None)
		{
			m_Deed = deed;

			CheckLos = false;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			Map map = from.Map;

			if (targeted is not IPoint3D p || map == null || m_Deed.Deleted)
				return;

			if (m_Deed.IsChildOf(from.Backpack))
			{
				BaseAddonContainer addon = m_Deed.Addon;
				addon.Resource = m_Deed.Resource;

				SpellHelper.GetSurfaceTop(ref p);

				BaseHouse house = null;

				AddonFitResult res = addon.CouldFit(p, map, from, ref house);

				switch (res)
				{
					case AddonFitResult.Valid:
						addon.MoveToWorld(new Point3D(p), map);
						break;
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

				if (res == AddonFitResult.Valid)
				{
					m_Deed.Delete();
					house.Addons[addon] = from;

					if (addon is GardenShedAddon ad)
					{
						house.Addons[ad.SecondContainer] = from;
					}

					if (addon.Security)
					{
						house.AddSecure(from, addon);
					}
				}
				else
				{
					addon.Delete();
				}
			}
			else
			{
				from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
			}
		}
	}
}
