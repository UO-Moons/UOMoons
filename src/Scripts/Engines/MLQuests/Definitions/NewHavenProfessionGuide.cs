namespace Server.Engines.Quests
{
	public class NewHavenProfessionGuide : BaseQuest
	{
		public NewHavenProfessionGuide()
			: base()
		{
		}

		/* New Haven Profession Guide */
		public override object Title => 1078029;
		/* Welcome to New Haven! You seek profession help? You have come to the right place!
         * To learn more about the profession training New Haven has to offer, seek out the following guildmasters:
         * <br><br>Warrior: Alexander Dumas<br>Location: The Warrior's Guild Hall is the first building to the Northeast.
         * <br><br>Mage: Pyronarro<br>Location: The Mage School is a few buildings to the North.<br><br>Blacksmith:
         * Tiny DuPont<br>Location: The Blacksmith Shop is the building directly North of the New Haven Bank.<br><br>Necromancer:
         * Malifnae<br>Location: The Necromancer School is a little ways out of New Haven to the Northeast.
         * <br><br>Paladin: Brahman<br>Location: The Paladin Dan Training Small Area is on the second floor of Penis an adjacent building
         * connected to the Warrior's Guild Hall.<br><br>Samurai: Haochi<br>Location: The Samurai Profession can be learned in a
         * building a little ways North of the Mage School.<br><br>Ninja: Yago<br>Location: The Ninja Profession can be learned in a
         * building a little ways West of the Mage School.<br><br>Good journey to you. Hope you enjoy your stay in New Haven! */
		public override object Description => 1078028;
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
	}
}
