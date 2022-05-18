using Server.Items;
using System;

namespace Server.Engines.XmlSpawner2
{
	// When this attachment is deleted, the object that it is attached to will be deleted as well.
	// The quest system will automatically delete these attachments after a quest is completed.
	// Specifying an expiration time will also allow you to give objects limited lifetimes.
	public class TemporaryQuestObject : XmlAttachment, ITemporaryQuestAttachment
	{
		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile QuestOwner { get; set; }

		// These are the various ways in which the message attachment can be constructed.  
		// These can be called via the [addatt interface, via scripts, via the spawner ATTACH keyword.
		// Other overloads could be defined to handle other types of arguments

		// a serial constructor is REQUIRED
		public TemporaryQuestObject(ASerial serial) : base(serial)
		{
		}

		[Attachable]
		public TemporaryQuestObject(string questname)
		{
			Name = questname;
		}

		[Attachable]
		public TemporaryQuestObject(string questname, double expiresin)
		{
			Name = questname;
			Expiration = TimeSpan.FromMinutes(expiresin);

		}

		[Attachable]
		public TemporaryQuestObject(string questname, double expiresin, Mobile questowner)
		{
			Name = questname;
			Expiration = TimeSpan.FromMinutes(expiresin);
			QuestOwner = questowner;

		}

		public override void OnDelete()
		{
			base.OnDelete();

			// delete the object that it is attached to
			if (AttachedTo is Mobile mobile)
			{
				// dont allow deletion of players
				if (!mobile.Player)
				{
					SafeMobileDelete(mobile);
					//((Mobile)AttachedTo).Delete();
				}
			}
			else if (AttachedTo is Item item)
			{
				SafeItemDelete(item);
				//((Item)AttachedTo).Delete();
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);
			writer.Write(QuestOwner);

		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			_ = reader.ReadInt();
			QuestOwner = reader.ReadMobile();
		}

		public override string OnIdentify(Mobile from)
		{
			if (from == null || from.AccessLevel == AccessLevel.Player) return null;

			if (Expiration > TimeSpan.Zero)
			{
				return $"{Name} expires in {Expiration.TotalMinutes} mins";
			}
			else
			{
				return $"{Name}: QuestOwner {QuestOwner}";
			}
		}
	}
}
