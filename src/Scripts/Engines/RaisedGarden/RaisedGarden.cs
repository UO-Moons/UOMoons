using System.Linq;
using Server.Engines.Plants;
using Server.Gumps;
using Server.Network;

namespace Server.Items
{
	public enum GardenBedDirection
	{
		South = 1,
		East,
		Large,
		Small
	}

	public class RaisedGardenAddon : BaseAddon
	{
		public override BaseAddonDeed Deed => new RaisedGardenDeed();
		public override bool ForceShowProperties => true;

		[Constructable]
		public RaisedGardenAddon(GardenBedDirection direction)
		{
			switch (direction)
			{
				case GardenBedDirection.Large:
					{
						AddComponent(new GardenAddonComponent(19234), 0, 0, 0);
						AddComponent(new GardenAddonComponent(19240), 1, 0, 0);
						AddComponent(new GardenAddonComponent(19235), 2, 0, 0);
						AddComponent(new GardenAddonComponent(19237), 2, 1, 0);
						AddComponent(new GardenAddonComponent(19239), 2, 2, 0);
						AddComponent(new GardenAddonComponent(19242), 1, 2, 0);
						AddComponent(new GardenAddonComponent(19238), 0, 2, 0);
						AddComponent(new GardenAddonComponent(19236), 0, 1, 0);
						AddComponent(new GardenAddonComponent(19241), 1, 1, 0);

						break;
					}
				case GardenBedDirection.East:
					{
						AddComponent(new GardenAddonComponent(19234), 0, 0, 0);
						AddComponent(new GardenAddonComponent(19235), 1, 0, 0);
						AddComponent(new GardenAddonComponent(19237), 1, 1, 0);
						AddComponent(new GardenAddonComponent(19239), 1, 2, 0);
						AddComponent(new GardenAddonComponent(19238), 0, 2, 0);
						AddComponent(new GardenAddonComponent(19236), 0, 1, 0);

						break;
					}
				case GardenBedDirection.South:
					{
						AddComponent(new GardenAddonComponent(19234), 0, 0, 0);
						AddComponent(new GardenAddonComponent(19240), 1, 0, 0);
						AddComponent(new GardenAddonComponent(19235), 2, 0, 0);
						AddComponent(new GardenAddonComponent(19239), 2, 1, 0);
						AddComponent(new GardenAddonComponent(19242), 1, 1, 0);
						AddComponent(new GardenAddonComponent(19238), 0, 1, 0);

						break;
					}
				case GardenBedDirection.Small:
					{
						AddComponent(new GardenAddonComponent(19234), 0, 0, 0);
						AddComponent(new GardenAddonComponent(19235), 1, 0, 0);
						AddComponent(new GardenAddonComponent(19239), 1, 1, 0);
						AddComponent(new GardenAddonComponent(19238), 0, 1, 0);

						break;
					}
			}
		}

		public override void OnChop(Mobile from)
		{
			if (Components.Any(comp => comp is GardenAddonComponent {Plant: { }}))
			{
				from.SendLocalizedMessage(1150383); // You need to remove all plants through the plant menu before destroying this.
				return;
			}

			base.OnChop(from);
		}

		public RaisedGardenAddon(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			reader.ReadInt();
		}
	}

	public class RaisedGardenDeed : BaseAddonDeed, IRewardOption
	{
		public override int LabelNumber => 1150359;  // Raised Garden Bed

		public override BaseAddon Addon => new RaisedGardenAddon(m_Direction);
		public GardenBedDirection m_Direction;

		[Constructable]
		public RaisedGardenDeed()
		{
			LootType = LootType.Blessed;
		}

		public RaisedGardenDeed(Serial serial)
			: base(serial)
		{
		}

		public void GetOptions(RewardOptionList list)
		{
			list.Add(1, 1150381); // Garden Bed (South) 
			list.Add(2, 1150382); // Garden Bed (East)
			list.Add(3, 1150620); // Garden Bed (Large)
			list.Add(4, 1150621); // Garden Bed (Small)
		}

		public void OnOptionSelected(Mobile from, int choice)
		{
			m_Direction = (GardenBedDirection)choice;

			if (!Deleted)
				base.OnDoubleClick(from);
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (IsChildOf(from.Backpack))
			{
				from.CloseGump(typeof(RewardOptionGump));
				from.SendGump(new RewardOptionGump(this, 1076170)); // Choose Direction
			}
			else
				from.SendLocalizedMessage(1062334); // This item must be in your backpack to be used.       	
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			reader.ReadEncodedInt();
		}
	}

	public class GardenAddonComponent : AddonComponent
	{
		private PlantItem _plant;

		[CommandProperty(AccessLevel.GameMaster)]
		public PlantItem Plant
		{
			get
			{
				if (_plant != null)
				{
					if (_plant.X != X || _plant.Y != Y || _plant.Map != Map || _plant.Deleted)
						_plant = null;
				}

				return _plant;
			}
			set
			{
				_plant = value;

				if (_plant != null)
				{
					if (_plant.X != X || _plant.Y != Y || _plant.Map != Map || _plant.Deleted)
						_plant = null;
				}
			}
		}

		public override int LabelNumber => Addon is RaisedGardenAddon ? 1150359 : 1159056; // Raised Garden Bed - Field Garden Bed

		public GardenAddonComponent(int itemId)
			: base(itemId)
		{
		}

		public int ZLocation()
		{
			return Addon is RaisedGardenAddon ? 5 : 1;
		}

		public override void Delete()
		{
			base.Delete();

			if (Plant != null)
				_plant.Z -= ZLocation();
		}

		public GardenAddonComponent(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0); // version

			writer.Write(_plant);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			reader.ReadInt();

			_plant = reader.ReadItem() as PlantItem;

			if (_plant is GardenBedPlantItem {Component: null} item)
				item.Component = this;
		}
	}
}
