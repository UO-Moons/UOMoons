namespace Server.Items
{
	#region Reward Clothing
	public class LibraryFriendSkirt : Kilt
	{
		public override int LabelNumber => 1073352; // Friends of the Library Kilt

		[Constructable]
		public LibraryFriendSkirt()
			: this(0)
		{
		}

		[Constructable]
		public LibraryFriendSkirt(int hue)
			: base(hue)
		{
		}

		public LibraryFriendSkirt(Serial serial)
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
			_ = reader.ReadInt();
		}
	}

	public class LibraryFriendPants : LongPants
	{
		public override int LabelNumber => 1073349; // Friends of the Library Pants

		[Constructable]
		public LibraryFriendPants()
			: this(0)
		{
		}

		[Constructable]
		public LibraryFriendPants(int hue)
			: base(hue)
		{
		}

		public LibraryFriendPants(Serial serial)
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
			_ = reader.ReadInt();
		}
	}

	public class MalabellesDress : Skirt
	{
		public override int LabelNumber => 1073251; // Malabelle's Dress - Museum of Vesper Replica

		[Constructable]
		public MalabellesDress()
			: this(0)
		{
		}

		[Constructable]
		public MalabellesDress(int hue)
			: base(hue)
		{
		}

		public MalabellesDress(Serial serial)
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
			_ = reader.ReadInt();
		}
	}
	#endregion
}
