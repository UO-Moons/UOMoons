using Server.Mobiles;

namespace Server.Items
{
	public class XmlQuestMaker : Item
	{

		public XmlQuestMaker(Serial serial) : base(serial)
		{
		}

		[Constructable]
		public XmlQuestMaker() : base(0xED4)
		{
			Name = "XmlQuestMaker";
			Movable = false;
			Visible = true;
		}


		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			_ = reader.ReadInt();
		}

		public override void OnDoubleClick(Mobile from)
		{
			base.OnDoubleClick(from);

			if (from is not PlayerMobile)
				return;

			// make a quest note
			QuestHolder newquest = new()
			{
				PlayerMade = true,
				Creator = from as PlayerMobile,
				Hue = 500
			};
			from.AddToBackpack(newquest);
			from.SendMessage("A blank quest has been added to your pack!");

		}

	}
}
