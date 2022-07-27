using System;

namespace Server.Items
{
    public class TombOfKingsSecretDoor : Item
    {
        public override int LabelNumber => 1020233;  // secret door

        [CommandProperty(AccessLevel.GameMaster)]
        private int ClosedId { get; set; }

        [Constructable]
        public TombOfKingsSecretDoor(int closedId)
            : base(closedId)
        {
            Movable = false;

            ClosedId = closedId;
        }

        public override void OnDoubleClickDead(Mobile from)
        {
            Open(from);
        }

        public override void OnDoubleClick(Mobile from)
        {
            Open(from);
        }

        private void Open(Mobile from)
        {
            if (!from.InRange(this, 1))
                return;

            if (ItemId != ClosedId)
	            return;
            ItemId = 1; // no draw

            Timer.DelayCall(TimeSpan.FromSeconds(120.0), delegate
            {
	            ItemId = ClosedId;
            });
        }

        public TombOfKingsSecretDoor(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version

            writer.Write(ClosedId);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            ClosedId = reader.ReadInt();

			// make sure we don't get stuck at opened state before deserialize
			ItemId = ClosedId;
        }
    }
}
