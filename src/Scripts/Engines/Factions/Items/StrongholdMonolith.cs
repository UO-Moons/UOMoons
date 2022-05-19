namespace Server.Factions
{
	public class StrongholdMonolith : BaseMonolith
	{
		public override int DefaultLabelNumber => 1041042;  // A Faction Sigil Monolith

		public override void OnTownChanged()
		{
			AssignName(Town?.Definition.StrongholdMonolithName);
		}

		public StrongholdMonolith() : this(null, null)
		{
		}

		public StrongholdMonolith(Town town, Faction faction) : base(town, faction)
		{
		}

		public StrongholdMonolith(Serial serial) : base(serial)
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
}
