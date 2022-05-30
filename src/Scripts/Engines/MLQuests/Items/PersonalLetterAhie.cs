using Server.Engines.Quests;

namespace Server.Items
{
	public class PersonalLetterAhie : BaseQuestItem
	{
		[Constructable]
		public PersonalLetterAhie()
			: base(0x14ED)
		{
		}

		public PersonalLetterAhie(Serial serial)
			: base(serial)
		{
		}

		public override int LabelNumber => 1073128;// A personal letter addressed to: Ahie
		public override int Lifespan => 1800;
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
