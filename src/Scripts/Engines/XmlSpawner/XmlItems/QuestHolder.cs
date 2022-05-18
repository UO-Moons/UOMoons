using Server.Gumps;

namespace Server.Items
{
	public class QuestHolder : XmlQuestHolder
	{
		[Constructable]
		public QuestHolder()
			: base()
		{
			Name = "A quest";
			TitleString = "A quest";
		}

		public QuestHolder(Serial serial)
			: base(serial)
		{
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
			from.CloseGump(typeof(XmlQuestStatusGump));

			from.SendGump(new XmlQuestStatusGump(this, TitleString));
		}

		public override void OnSnoop(Mobile from)
		{
			if (from.AccessLevel > AccessLevel.Player)
			{
				from.CloseGump(typeof(XmlQuestStatusGump));

				from.SendGump(new XmlQuestStatusGump(this, TitleString));
			}
		}

	}
}
