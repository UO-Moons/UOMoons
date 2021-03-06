using Server.Engines.Craft;
using Server.Mobiles;
using Server.Targeting;
using System;

namespace Server.Items;

[TypeAlias("Server.Items.DragonBarding")]
public class DragonBardingDeed : BaseItem, ICraftable, IResource
{
	private bool _mExceptional;

	public override int LabelNumber => _mExceptional ? 1053181 : 1053012;  // dragon barding deed

	[CommandProperty(AccessLevel.GameMaster)]
	public override CraftResource Resource { get => base.Resource; set { base.Resource = value; Hue = CraftResources.GetHue(value); InvalidateProperties(); } }

	public DragonBardingDeed() : base(0x14F0)
	{
		Weight = 1.0;
	}

	public override void GetProperties(ObjectPropertyList list)
	{
		base.GetProperties(list);

		if (Quality == ItemQuality.Exceptional && Crafter != null)
			list.Add(1050043, Crafter.Name); // crafted by ~1_NAME~
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (IsChildOf(from.Backpack))
		{
			from.BeginTarget(6, false, TargetFlags.None, OnTarget);
			from.SendLocalizedMessage(1053024); // Select the swamp dragon you wish to place the barding on.
		}
		else
		{
			from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
		}
	}

	public virtual void OnTarget(Mobile from, object obj)
	{
		if (Deleted)
			return;

		if (obj is not SwampDragon pet || pet.HasBarding)
		{
			from.SendLocalizedMessage(1053025); // That is not an unarmored swamp dragon.
		}
		else if (!pet.Controlled || pet.ControlMaster != from)
		{
			from.SendLocalizedMessage(1053026); // You can only put barding on a tamed swamp dragon that you own.
		}
		else if (!IsChildOf(from.Backpack))
		{
			from.SendLocalizedMessage(1060640); // The item must be in your backpack to use it.
		}
		else
		{
			pet.BardingExceptional = Quality == ItemQuality.Exceptional;
			pet.BardingCrafter = Crafter;
			pet.BardingHP = pet.BardingMaxHP;
			pet.BardingResource = Resource;
			pet.HasBarding = true;
			pet.Hue = Hue;

			Delete();

			from.SendLocalizedMessage(1053027); // You place the barding on your swamp dragon.  Use a bladed item on your dragon to remove the armor.
		}
	}

	public DragonBardingDeed(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
		writer.Write(_mExceptional);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		int version = reader.ReadInt();
		_mExceptional = version switch
		{
			0 => reader.ReadBool(),
			_ => _mExceptional
		};
	}

	#region ICraftable Members
	public int OnCraft(int quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, ITool tool, CraftItem craftItem, int resHue)
	{
		Quality = ItemQuality.Normal;

		if (makersMark)
			Crafter = from;

		Type resourceType = typeRes;

		if (resourceType == null)
			resourceType = craftItem.Resources.GetAt(0).ItemType;

		Resource = CraftResources.GetFromType(resourceType);

		CraftContext context = craftSystem.GetContext(from);

		if (context is {DoNotColor: true})
			Hue = 0;

		return quality;
	}
	#endregion
}
