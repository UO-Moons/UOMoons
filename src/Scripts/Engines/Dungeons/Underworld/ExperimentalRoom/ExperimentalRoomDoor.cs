using System.Linq;

namespace Server.Items
{

    public class ExperimentalRoomDoor : MetalDoor2
    {

        public override string DefaultName => "a door";

        [CommandProperty(AccessLevel.GameMaster)]
        private Room Room { get; set; }

        [Constructable]
        public ExperimentalRoomDoor(Room room, DoorFacing facing) : base(facing)
        {
            Room = room;
        }

        public ExperimentalRoomDoor(Serial serial) : base(serial)
        {

        }

        public override void Use(Mobile from)
        {
            if (from.AccessLevel > AccessLevel.Player)
            {
                from.SendMessage("You open the door with your godly powers.");
                base.Use(from);
                return;
            }

            Container pack = from.Backpack;
            bool hasGem = false;

            if (pack != null)
            {
                Item[] items = pack.FindItemsByType(typeof(ExperimentalGem));

                if (items is { Length: > 0 })
                {
	                hasGem = true;

	                if (items.Cast<ExperimentalGem>().Any(gem => gem.Active && (gem.CurrentRoom > Room || Room == Room.RoomZero)))
	                {
		                base.Use(from);
		                return;
	                }
                }
                else
                    from.SendLocalizedMessage(1113410); // You must have an active Experimental Gem to enter that room.
            }

            if (hasGem)
                from.SendLocalizedMessage(1113411); // You have not yet earned access to that room!
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // ver
            writer.Write((int)Room);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
            Room = (Room)reader.ReadInt();
        }
    }
}

namespace Server.Items
{
	public class ExperimentalRoomBlocker : Item
    {
	    [CommandProperty(AccessLevel.GameMaster)]
	    private Room Room { get; set; }

        [Constructable]
        public ExperimentalRoomBlocker(Room room) : base(7107)
        {
            Room = room;

            Visible = false;
            Movable = false;
        }

        public ExperimentalRoomBlocker(Serial serial) : base(serial)
        {

        }

        public override bool OnMoveOver(Mobile from)
        {
            if (from.AccessLevel > AccessLevel.Player)
                return true;

            Container pack = from.Backpack;

            Item[] items = pack?.FindItemsByType(typeof(ExperimentalGem));

            return items != null && items.Cast<ExperimentalGem>().Any(gem => gem.Active && (gem.CurrentRoom > Room || Room == Room.RoomZero));
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // ver
            writer.Write((int)Room);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
            Room = (Room)reader.ReadInt();
        }
    }
}
