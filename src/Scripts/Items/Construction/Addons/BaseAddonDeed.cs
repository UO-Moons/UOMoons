using Server.Engines.Craft;
using Server.Multis;
using Server.Targeting;
using System;
using Server.Spells;

namespace Server.Items;

[Flipable(0x14F0, 0x14EF)]
public abstract class BaseAddonDeed : BaseItem, ICraftable, IResource
{
	//private CraftResource m_Resource;
	private bool m_ReDeed;

	public BaseAddonDeed()
		: base(0x14F0)
	{
		Weight = 1.0;
		LootType = Core.AOS ? LootType.Regular : LootType.Newbied;
	}

	public BaseAddonDeed(Serial serial)
		: base(serial)
	{
	}

	public abstract BaseAddon Addon { get; }

	public virtual bool UseCraftResource => true;

	public virtual bool ExcludeDeedHue => false;

	/*[CommandProperty(AccessLevel.GameMaster)]
	public CraftResource Resource
	{
		get => m_Resource;
		set
		{
			if (UseCraftResource && m_Resource != value)
			{
				m_Resource = value;
				Hue = CraftResources.GetHue(m_Resource);

				InvalidateProperties();
			}
		}
	}*/

	[CommandProperty(AccessLevel.GameMaster)]
	public bool IsReDeed
	{
		get => m_ReDeed;
		set
		{
			m_ReDeed = value;

			if (UseCraftResource)
			{
				if (m_ReDeed && ItemId == 0x14F0)
				{
					ItemId = 0x14EF;
				}
				else if (!m_ReDeed && ItemId == 0x14EF)
				{
					ItemId = 0x14F0;
				}
			}
		}
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0);
		writer.Write(m_ReDeed);
		//writer.Write((int)m_Resource);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		int version = reader.ReadInt();

		switch (version)
		{
			case 0:
			{
				m_ReDeed = reader.ReadBool();
				break;
			}
			//case 1:
			//{
			//	m_Resource = (CraftResource)reader.ReadInt();
			//	break;
			//}
		}

		//if (version == 1 && UseCraftResource && Hue == 0 && Resource != CraftResource.None)
		//{
		//	Hue = CraftResources.GetHue(Resource);
		//}
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (IsChildOf(from.Backpack))
		{
			from.Target = new InternalTarget(this);
		}
		else
		{
			from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
		}
	}

	public virtual void DeleteDeed()
	{
		Delete();
	}

	public override void GetProperties(ObjectPropertyList list)
	{
		base.GetProperties(list);

		if (!CraftResources.IsStandard(Resource))
		{
			list.Add(CraftResources.GetLocalizationNumber(Resource));
		}
	}

	public virtual int OnCraft(int quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, ITool tool, CraftItem craftItem, int resHue)
	{
		Type resourceType = typeRes ?? craftItem.Resources.GetAt(0).ItemType;

		Resource = CraftResources.GetFromType(resourceType);

		CraftContext context = craftSystem.GetContext(from);

		if (context is { DoNotColor: true })
		{
			Hue = 0;
		}
		else if (Hue == 0)
		{
			Hue = resHue;
		}

		return quality;
	}

	private class InternalTarget : Target
	{
		private readonly BaseAddonDeed m_Deed;
		public InternalTarget(BaseAddonDeed deed)
			: base(-1, true, TargetFlags.None)
		{
			m_Deed = deed;

			CheckLos = false;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			Map map = from.Map;

			if (targeted is not IPoint3D p || map == null || m_Deed.Deleted)
			{
				return;
			}

			if (m_Deed.IsChildOf(from.Backpack))
			{
				BaseAddon addon = m_Deed.Addon;

				SpellHelper.GetSurfaceTop(ref p);

				BaseHouse house = null;

				AddonFitResult res = addon.CouldFit(p, map, from, ref house);

				switch (res)
				{
					case AddonFitResult.Valid:
					{
						addon.Resource = m_Deed.Resource;

						if (!m_Deed.ExcludeDeedHue)
						{
							if (addon.RetainDeedHue || (m_Deed.Hue != 0 && CraftResources.GetHue(m_Deed.Resource) != m_Deed.Hue))
							{
								addon.Hue = m_Deed.Hue;
							}
						}

						addon.MoveToWorld(new Point3D(p), map);

						if (house != null)
						{
							house.Addons[addon] = from;
						}

						m_Deed.DeleteDeed();
						break;
					}
					case AddonFitResult.Blocked:
						from.SendLocalizedMessage(500269); // You cannot build that there.
						break;
					case AddonFitResult.NotInHouse:
						from.SendLocalizedMessage(500274); // You can only place this in a house that you own!
						break;
					case AddonFitResult.DoorTooClose:
						from.SendLocalizedMessage(500271); // You cannot build near the door.
						break;
					case AddonFitResult.NoWall:
						from.SendLocalizedMessage(500268); // This object needs to be mounted on something.
						break;
				}

				if (res != AddonFitResult.Valid)
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
