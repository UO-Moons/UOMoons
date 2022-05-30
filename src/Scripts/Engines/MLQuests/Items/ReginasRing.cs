using Server.Engines.Craft;

namespace Server.Items
{
	public class ReginasRing : BaseRing/*, IRepairable*/
	{
		public override bool IsArtifact => true;
		[Constructable]
		public ReginasRing()
			: base(0x1F09)
		{
			LootType = LootType.Blessed;
		}

		public ReginasRing(Serial serial)
			: base(serial)
		{
		}

		public override int LabelNumber => 1075305;// Regina's Ring
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
