using Server.Gumps;
using Server.Mobiles;
using System.Collections;
using System.Collections.Generic;

namespace Server.Items
{
	[Flipable(0x1E5E, 0x1E5F)]
	public class PlayerQuestBoard : XmlQuestBook
	{

		public override bool IsDecoContainer => false;

		public PlayerQuestBoard(Serial serial) : base(serial)
		{
		}

		[Constructable]
		public PlayerQuestBoard() : base(0x1e5e)
		{
			Movable = false;
			Name = "Player Quest Board";
			LiftOverride = true;    // allow players to store books in it
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
	}

	public class XmlQuestBook : Container
	{
		[CommandProperty(AccessLevel.GameMaster)]
		public PlayerMobile Owner { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Locked { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool IsCompleted
		{
			get
			{
				Item[] questitems = FindItemsByType(typeof(IXmlQuest));

				if (questitems == null || questitems.Length <= 0)
					return false;

				for (int i = 0; i < questitems.Length; ++i)
				{
					// check completion and validity status of all quests held in the book
					if (questitems[i] is not IXmlQuest q || q.Deleted || !q.IsValid || !q.IsCompleted) return false;
				}

				return true;
			}
		}

		public XmlQuestBook(Serial serial) : base(serial)
		{
		}

		[Constructable]
		public XmlQuestBook(int itemid) : this()
		{
			ItemId = itemid;
		}

		[Constructable]
		public XmlQuestBook() : base(0x2259)
		{
			//LootType = LootType.Blessed;
			Name = "QuestBook";
			Hue = 100;
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (from is not PlayerMobile)
				return;

			if (from.AccessLevel >= AccessLevel.GameMaster)
			{
				base.OnDoubleClick(from);
			}

			from.SendGump(new XmlQuestBookGump((PlayerMobile)from, this));
		}

		public override bool OnDragDrop(Mobile from, Item dropped)
		{
			if (dropped is IXmlQuest && !Locked)
			{
				return base.OnDragDrop(from, dropped);
			}
			else
			{
				return false;
			}
		}

		private void CheckOwnerFlag()
		{
			if (Owner != null && !Owner.Deleted)
			{
				// need to check to see if any other questtoken items are owned
				// search the Owners top level pack for an xmlquest
				//List<Item> list = XmlQuest.FindXmlQuest(Owner);
				ArrayList list = XmlQuest.FindXmlQuest(Owner);
				if (list == null || list.Count == 0)
				{
					// if none remain then flag the ower as having none
					Owner.SetFlag(XmlQuest.CarriedXmlQuestFlag, false);
				}

			}
		}

		public virtual void Invalidate()
		{

			if (Owner != null)
			{
				Owner.SendMessage(string.Format("{0} Quests invalidated - '{1}' removed", TotalItems, Name));
			}
			Delete();
		}

		public override void OnItemLifted(Mobile from, Item item)
		{
			base.OnItemLifted(from, item);

			if (from is PlayerMobile && Owner == null)
			{
				Owner = from as PlayerMobile;
				LootType = LootType.Blessed;
				// flag the owner as carrying a questtoken assuming the book contains quests and then confirm it with CheckOwnerFlag
				Owner.SetFlag(XmlQuest.CarriedXmlQuestFlag, true);
				CheckOwnerFlag();
			}
		}

		public override void OnAdded(IEntity parent)
		{
			base.OnAdded(parent);

			if (parent is not null and Container)
			{
				// find the parent of the container
				// note, the only valid additions are to the player pack.  Anything else is invalid.  This is to avoid exploits involving storage or transfer of questtokens
				object from = ((Container)parent).Parent;

				// check to see if it can be added
				if (from != null && from is PlayerMobile)
				{
					// if it was not owned then allow it to go anywhere
					if (Owner == null)
					{
						Owner = from as PlayerMobile;

						LootType = LootType.Blessed;
						// could also bless all of the quests inside as well but not actually necessary since blessed containers retain their
						// contents whether blessed or not, and when dropped the questtokens will be blessed

						// flag the owner as carrying a questtoken
						Owner.SetFlag(XmlQuest.CarriedXmlQuestFlag, true);
						CheckOwnerFlag();
					}
					else
					if (from as PlayerMobile != Owner || parent is BankBox)
					{
						// tried to give it to another player or placed it in the players bankbox. try to return it to the owners pack
						Owner.AddToBackpack(this);
					}
				}
				else
				{
					if (Owner != null)
					{
						// try to return it to the owners pack
						Owner.AddToBackpack(this);
					}
					// allow placement into npcs or drop on their corpses when owner is null
					else
					if (from is not Mobile && parent is not Corpse)
					{
						// in principle this should never be reached

						// invalidate the token

						CheckOwnerFlag();

						Invalidate();
					}
				}
			}
		}

		public override void OnDelete()
		{
			base.OnDelete();

			CheckOwnerFlag();
		}

		public override bool OnDroppedToWorld(Mobile from, Point3D point)
		{
			_ = base.OnDroppedToWorld(from, point);

			from.SendGump(new XmlConfirmDeleteGump(from, this));

			//CheckOwnerFlag();

			//Invalidate();
			return false;
			//return returnvalue;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0); // version

			writer.Write(Owner);
			writer.Write(Locked);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			_ = reader.ReadInt();

			Owner = reader.ReadMobile() as PlayerMobile;
			Locked = reader.ReadBool();
		}
	}
}
