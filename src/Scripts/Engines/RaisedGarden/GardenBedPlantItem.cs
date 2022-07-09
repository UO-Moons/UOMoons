using Server.Items;
using Server.Multis;
using Server.Network;
using System;

namespace Server.Engines.Plants
{
    [TypeAlias("Server.Engines.Plants.RaisedGardenPlantItem")]
    public class GardenBedPlantItem : PlantItem
    {
        public override bool RequiresUpkeep => false;
        public override int BowlOfDirtID => 2323;
        public override int GreenBowlID
        {
            get
            {
	            return PlantStatus <= PlantStatus.Stage3 ? 0xC7E : 0xC62;
            }
        }

        public override int ContainerLocalization => 1150436;  // mound
        public override int OnPlantLocalization => 1150442;  // You plant the seed in the mound of dirt.
        public override int CantUseLocalization => 1150511;  // That is not your gardening plot.

        public override int LabelNumber
        {
            get
            {
                int label = base.LabelNumber;

                if (label == 1029913)
                    label = 1022321;   // dirt patch

                return label;
            }
        }

        private GardenAddonComponent _component;

        [CommandProperty(AccessLevel.GameMaster)]
        public GardenAddonComponent Component
        {
            get
            {
                if (_component != null)
                {
                    if (_component.X != X || _component.Y != Y || _component.Map != Map || _component.Deleted)
                        _component = null;
                }

                return _component;
            }
            set
            {
                _component = value;

                if (_component != null)
                {
                    if (_component.X != X || _component.Y != Y || _component.Map != Map || _component.Deleted)
                        _component = null;
                }
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public override bool ValidGrowthLocation => RootParent == null && Component != null && !Movable && !Deleted;

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime NextGrowth
        {
            get
            {
	            return PlantSystem?.NextGrowth ?? DateTime.MinValue;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public PlantGrowthIndicator GrowthIndicator
        {
            get
            {
	            return PlantSystem?.GrowthIndicator ?? PlantGrowthIndicator.None;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool ForceGrow
        {
            get => false;
            set
            {
                if (value && PlantSystem != null)
                {
                    PlantSystem.NextGrowth = DateTime.UtcNow;
                    PlantSystem.DoGrowthCheck();
                }
            }
        }

        [Constructable]
        public GardenBedPlantItem()
            : this(false)
        {
        }

        [Constructable]
        public GardenBedPlantItem(bool fertileDirt)
            : base(2323, fertileDirt)
        {
            Movable = false;
        }

        public override void Delete()
        {
            if (_component != null && _component.Plant == this)
                _component.Plant = null;

            base.Delete();
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (PlantStatus >= PlantStatus.DecorativePlant)
                return;

            Point3D loc = GetWorldLocation();

            if (!from.InLOS(loc) || !from.InRange(loc, 4))
            {
                from.LocalOverheadMessage(MessageType.Regular, 0x3E9, 1019045); // I can't reach that.
                return;
            }

            if (!IsUsableBy(from))
            {
                LabelTo(from, 1150327); // Only the house owner and co-owners can use the raised garden bed.

                return;
            }

            from.SendGump(new MainPlantGump(this));
        }

        public override bool IsUsableBy(Mobile from)
        {
            BaseHouse house = BaseHouse.FindHouseAt(this);
            return house != null && house.IsCoOwner(from) && IsAccessibleTo(from);
        }

        public GardenBedPlantItem(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version

            writer.Write(_component);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            _component = reader.ReadItem() as GardenAddonComponent;
        }
    }
}
