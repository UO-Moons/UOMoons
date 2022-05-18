namespace Server.Items
{
	public class FarmableCabbage : FarmableCrop
	{
		public static int GetCropID() => 3254;
		public override int GetPickedID() => 3254;

		public override Item GetCropObject()
		{
			Cabbage cabbage = new()
			{
				ItemID = Utility.Random(3195, 2)
			};

			return cabbage;
		}

		[Constructable]
		public FarmableCabbage() : base(GetCropID())
		{
		}

		public FarmableCabbage(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.WriteEncodedInt(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadEncodedInt();
		}
	}
}
