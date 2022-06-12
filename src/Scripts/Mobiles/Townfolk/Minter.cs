namespace Server.Mobiles
{
	public class Minter : Banker
	{
		public override NpcGuild NpcGuild => NpcGuild.MerchantsGuild;

		[Constructable]
		public Minter()
		{
			Title = "the minter";
			Job = JobFragment.minter;
			Karma = Utility.RandomMinMax(13, -45);
		}

		public Minter(Serial serial) : base(serial)
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

			int version = reader.ReadInt();
		}
	}
}
