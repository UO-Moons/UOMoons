using Server.Engines.Plants;
using Server.Multis;
using Server.Targeting;

namespace Server.Items;

public class NaturalDye : Item, IPigmentHue
{
	private PlantPigmentHue _mHue;
	private int _mUsesRemaining;
	[Constructable]
	public NaturalDye()
		: this(PlantPigmentHue.None)
	{
	}

	[Constructable]
	public NaturalDye(PlantPigmentHue hue)
		: base(0x182B)
	{
		Weight = 1.0;
		PigmentHue = hue;
		_mUsesRemaining = 5;
	}

	public NaturalDye(Serial serial)
		: base(serial)
	{
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public PlantPigmentHue PigmentHue
	{
		get => _mHue;
		set
		{
			_mHue = value;
			// set any invalid pigment hue to Plain
			if (_mHue != PlantPigmentHueInfo.GetInfo(_mHue).PlantPigmentHue)
				_mHue = PlantPigmentHue.Plain;
			Hue = PlantPigmentHueInfo.GetInfo(_mHue).Hue;
			InvalidateProperties();
		}
	}
	[CommandProperty(AccessLevel.GameMaster)]
	public int UsesRemaining
	{
		get => _mUsesRemaining;
		set
		{
			_mUsesRemaining = value;
			InvalidateProperties();
		}
	}
	public override int LabelNumber => 1112136;// natural dye
	public static bool RetainsColorFrom => true;
	public override bool ForceShowProperties => ObjectPropertyList.Enabled;
	public override void GetProperties(ObjectPropertyList list)
	{
		base.GetProperties(list);

		list.Add(1060584, _mUsesRemaining.ToString()); // uses remaining: ~1_val~
	}

	public override void AddNameProperty(ObjectPropertyList list)
	{
		PlantPigmentHueInfo hueInfo = PlantPigmentHueInfo.GetInfo(_mHue);

		if (Amount > 1)
			list.Add(PlantPigmentHueInfo.IsBright(_mHue) ? 1113277 : 1113276, "{0}\t{1}", Amount, "#" + hueInfo.Name);  // ~1_COLOR~ Softened Reeds
		else
			list.Add(hueInfo.IsBright() ? 1112138 : 1112137, "#" + hueInfo.Name);  // ~1_COLOR~ natural dye
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
		writer.Write((int)_mHue);
		writer.Write(_mUsesRemaining);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		var version = reader.ReadInt();

		switch (version)
		{
			case 0:
				_mHue = (PlantPigmentHue)reader.ReadInt();
				_mUsesRemaining = reader.ReadInt();
				break;
		}
	}

	public override void OnDoubleClick(Mobile from)
	{
		from.SendLocalizedMessage(1112139); // Select the item you wish to dye.
		from.Target = new InternalTarget(this);
	}

	private class InternalTarget : Target
	{
		private readonly NaturalDye _mItem;
		public InternalTarget(NaturalDye item)
			: base(1, false, TargetFlags.None)
		{
			_mItem = item;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (_mItem.Deleted)
				return;

			if (targeted is Item item)
			{
				bool valid = item is IDyable or BaseTalisman or BaseBook or BaseClothing or BaseJewel or BaseStatuette or BaseWeapon or Runebook or Spellbook || item.IsArtifact || BasePigmentsOfTokuno.IsValidItem(item);

				if (item is HoodedShroudOfShadows or MonkRobe)
				{
					from.SendLocalizedMessage(1042083); // You cannot dye that.
					return;
				}

				if (!valid && FurnitureAttribute.Check(item))
				{
					if (!from.InRange(_mItem.GetWorldLocation(), 1) || !from.InRange(item.GetWorldLocation(), 1))
					{
						from.SendLocalizedMessage(500446); // That is too far away.
						return;
					}

					BaseHouse house = BaseHouse.FindHouseAt(item);

					if (house == null || (!house.IsLockedDown(item) && !house.IsSecure(item)))
					{
						from.SendLocalizedMessage(501022); // Furniture must be locked down to paint it.
						return;
					}

					if (!house.IsCoOwner(from))
					{
						from.SendLocalizedMessage(501023); // You must be the owner to use this item.
						return;
					}

					valid = true;
				}
				else if (!item.IsChildOf(from.Backpack))
				{
					from.SendLocalizedMessage(1060640); // The item must be in your backpack to use it.
					return;
				}

				if (!valid && item is BaseArmor armor)
				{
					CraftResourceType restype = CraftResources.GetType(armor.Resource);
					if (restype is CraftResourceType.Leather or CraftResourceType.Metal &&
					    ArmorMaterialType.Bone != armor.MaterialType)
					{
						valid = true;
					}
				}
				// need to add any bags, chests, boxes, crates not IDyable but dyable by natural dyes
				if (valid)
				{
					item.Hue = PlantPigmentHueInfo.GetInfo(_mItem.PigmentHue).Hue;
					from.PlaySound(0x23E);

					if (--_mItem.UsesRemaining > 0)
						_mItem.InvalidateProperties();
					else
						_mItem.Delete();

					return;
				}
			}

			from.SendLocalizedMessage(1042083); // You cannot dye that.
		}
	}
}
