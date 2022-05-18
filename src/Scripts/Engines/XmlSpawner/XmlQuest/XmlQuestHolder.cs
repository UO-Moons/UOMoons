#define CLIENT6017

using Server.Engines.XmlSpawner2;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace Server.Items
{
	public abstract class XmlQuestHolder : Container, IXmlQuest
	{
		// public const PlayerFlag CarriedXmlQuestFlag = (PlayerFlag)0x00100000;

		private double m_ExpirationDuration;
		private bool m_Completed1 = false;
		private bool m_Completed2 = false;
		private bool m_Completed3 = false;
		private bool m_Completed4 = false;
		private bool m_Completed5 = false;
		private string m_TitleString;
		private readonly string m_SkillTrigger = null;
		private bool m_Repeatable = true;
		private TimeSpan m_NextRepeatable;
		private Item m_RewardItem;
		private XmlAttachment m_RewardAttachment;
		private int m_RewardAttachmentSerialNumber;
		private string m_status_str;
		public static int JournalNotifyColor;
		public static int JournalEchoColor = 6;

		public XmlQuestHolder(Serial serial)
			: base(serial)
		{
		}

		public XmlQuestHolder()
			: this(3643)
		{
		}

		public XmlQuestHolder(int itemID)
			: base(itemID)
		{
			Weight = 0;
			Hue = 500;
			//LootType = LootType.Blessed;
			TimeCreated = DateTime.UtcNow;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(7); // version
							 // version 7
			writer.Write(RewardAction);
			// version 6
			if (Journal == null || Journal.Count == 0)
			{
				writer.Write(0);
			}
			else
			{
				writer.Write(Journal.Count);
				foreach (XmlQuest.JournalEntry e in Journal)
				{
					writer.Write(e.EntryID);
					writer.Write(e.EntryText);
				}
			}
			// version 5
			writer.Write(m_Repeatable);
			// version 4
			writer.Write(Difficulty);
			// version 3
			writer.Write(AttachmentString);
			// version 2
			writer.Write(m_NextRepeatable);
			// version 1
			if (m_RewardAttachment != null)
				writer.Write(m_RewardAttachment.Serial.Value);
			else
				writer.Write(0);
			// version 0
			writer.Write(ReturnContainer);
			writer.Write(m_RewardItem);
			writer.Write(AutoReward);
			writer.Write(CanSeeReward);
			writer.Write(PlayerMade);
			writer.Write(Creator);
			writer.Write(Description1);
			writer.Write(Description2);
			writer.Write(Description3);
			writer.Write(Description4);
			writer.Write(Description5);
			writer.Write(Owner);
			writer.Write(RewardString);
			writer.Write(ConfigFile);
			writer.Write(NoteString);    // moved from the QuestNote class
			writer.Write(m_TitleString);   // moved from the QuestNote class
			writer.Write(PartyEnabled);
			writer.Write(PartyRange);
			writer.Write(State1);
			writer.Write(State2);
			writer.Write(State3);
			writer.Write(State4);
			writer.Write(State5);
			writer.Write(m_ExpirationDuration);
			writer.Write(TimeCreated);
			writer.Write(Objective1);
			writer.Write(Objective2);
			writer.Write(Objective3);
			writer.Write(Objective4);
			writer.Write(Objective5);
			writer.Write(m_Completed1);
			writer.Write(m_Completed2);
			writer.Write(m_Completed3);
			writer.Write(m_Completed4);
			writer.Write(m_Completed5);
		}

		public override bool CheckLift(Mobile from, Item item, ref LRReason reject)
		{
			if (item == this)
			{
				return base.CheckLift(from, item, ref reject);
			}
			reject = LRReason.CannotLift;
			return false;
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
			switch (version)
			{
				case 7:
					{
						RewardAction = reader.ReadString();
						goto case 6;
					}
				case 6:
					{
						int nentries = reader.ReadInt();

						if (nentries > 0)
						{
							Journal = new List<XmlQuest.JournalEntry>();
							for (int i = 0; i < nentries; i++)
							{
								string entryID = reader.ReadString();
								string entryText = reader.ReadString();
								Journal.Add(new XmlQuest.JournalEntry(entryID, entryText));
							}
						}

						goto case 5;
					}
				case 5:
					{
						m_Repeatable = reader.ReadBool();

						goto case 4;
					}
				case 4:
					{
						Difficulty = reader.ReadInt();

						goto case 3;
					}
				case 3:
					{
						AttachmentString = reader.ReadString();

						goto case 2;
					}
				case 2:
					{
						m_NextRepeatable = reader.ReadTimeSpan();

						goto case 1;
					}
				case 1:
					{
						m_RewardAttachmentSerialNumber = reader.ReadInt();

						goto case 0;
					}
				case 0:
					{
						ReturnContainer = (Container)reader.ReadItem();
						m_RewardItem = reader.ReadItem();
						AutoReward = reader.ReadBool();
						CanSeeReward = reader.ReadBool();
						PlayerMade = reader.ReadBool();
						Creator = reader.ReadMobile() as PlayerMobile;
						Description1 = reader.ReadString();
						Description2 = reader.ReadString();
						Description3 = reader.ReadString();
						Description4 = reader.ReadString();
						Description5 = reader.ReadString();
						Owner = reader.ReadMobile() as PlayerMobile;
						RewardString = reader.ReadString();
						ConfigFile = reader.ReadString();
						NoteString = reader.ReadString();
						m_TitleString = reader.ReadString();
						PartyEnabled = reader.ReadBool();
						PartyRange = reader.ReadInt();
						State1 = reader.ReadString();
						State2 = reader.ReadString();
						State3 = reader.ReadString();
						State4 = reader.ReadString();
						State5 = reader.ReadString();
						Expiration = reader.ReadDouble();
						TimeCreated = reader.ReadDateTime();
						Objective1 = reader.ReadString();
						Objective2 = reader.ReadString();
						Objective3 = reader.ReadString();
						Objective4 = reader.ReadString();
						Objective5 = reader.ReadString();
						m_Completed1 = reader.ReadBool();
						m_Completed2 = reader.ReadBool();
						m_Completed3 = reader.ReadBool();
						m_Completed4 = reader.ReadBool();
						m_Completed5 = reader.ReadBool();
					}
					break;
			}
		}

		private static Item PlaceHolderItem = null;

		public static void Initialize()
		{
			// create a temporary placeholder item used to force allocation empty Items lists used to hold hidden rewards.
			PlaceHolderItem = new Item(1);

			foreach (Item item in World.Items.Values)
			{
				if (item is XmlQuestHolder)
				{
					XmlQuestHolder t = item as XmlQuestHolder;

					t.UpdateWeight();

					t.RestoreRewardAttachment();
				}
			}

			// remove the temporary placeholder item
			PlaceHolderItem.Delete();
		}

		private void HideRewards()
		{
			if (m_RewardItem != null)
			{
				// remove the item from the containers item list
				if (Items.Contains(m_RewardItem))
				{
					Items.Remove(m_RewardItem);
				}
			}
		}

		private void UnHideRewards()
		{
			if (m_RewardItem == null) return;

			Item tmpitem = null;

			if (Items == Item.EmptyItems)
			{
				tmpitem = PlaceHolderItem;

				if (tmpitem == null || tmpitem.Deleted)
				{
					tmpitem = new Item(1);
				}

				// need to get it to allocate a new list by adding an item
				DropItem(tmpitem);
			}

			if (!Items.Contains(m_RewardItem))
			{
				m_RewardItem.Parent = this;
				m_RewardItem.Map = Map;

				// restore the item to the containers item list
				Items.Add(m_RewardItem);

			}

			// remove the placeholder
			if (tmpitem != null && Items.Contains(tmpitem))
			{
				Items.Remove(tmpitem);
				tmpitem.Map = Map.Internal;
			}

			if (tmpitem != null && tmpitem != PlaceHolderItem)
			{
				tmpitem.Delete();
			}
		}

		public override bool CheckItemUse(Mobile from, Item item)
		{
			if (item is not Container)
				return false;
			else
				return base.CheckItemUse(from, item);
		}

		public override void DisplayTo(Mobile to)
		{
			if (to == null)
				return;

			// add the reward item back into the container list for display
			UnHideRewards();

			to.Send(new ContainerDisplay(this));

#if (CLIENT6017)
			// add support for new client container packets
			if (to.NetState != null && to.NetState.ContainerGridLines)
				to.Send(new ContainerContent6017(to, this));
			else
#endif
				to.Send(new ContainerContent(to, this));

			if (ObjectPropertyList.Enabled)
			{
				List<Item> items = Items;

				for (int i = 0; i < items.Count; ++i)
					to.Send(items[i].OPLPacket);
			}
			// move the reward item out of container to protect it from use
			HideRewards();
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			list.Add(Name);
			if (LootType == LootType.Blessed)
			{
				list.Add(1038021);
			}
			if (PlayerMade && Owner != null && RootParent is not PlayerVendor)
			{
				list.Add(1050044, "{0}\t{1}", TotalItems, TotalWeight); // ~1_COUNT~items,~2_WEIGHT~stones
			}

			// add any playervendor price/description information
			if (RootParent is PlayerVendor vendor)
			{
				vendor.GetChildProperties(list, this);
			}
		}

		public override bool CheckHold(Mobile m, Item item, bool message, bool checkItems, int plusItems, int plusWeight) => false;

		public override bool TryDropItem(Mobile from, Item dropped, bool sendFullMessage) => false;

		public override bool OnDragDrop(Mobile from, Item dropped) => false;

		public override bool OnDragDropInto(Mobile from, Item item, Point3D p) => false;

		public override bool CheckTarget(Mobile from, Target targ, object targeted)
		{
			return from.AccessLevel != AccessLevel.Player;
		}


		public override void OnDoubleClick(Mobile from)
		{
			//base.OnDoubleClick(from);

			if (from is not PlayerMobile)
				return;

			if (PlayerMade && (from == Creator) && (from == Owner))
			{
				from.SendGump(new XmlPlayerQuestGump((PlayerMobile)from, this));
			}
		}

		public override bool OnDroppedToWorld(Mobile from, Point3D point)
		{
			_ = base.OnDroppedToWorld(from, point);

			from.SendGump(new XmlConfirmDeleteGump(from, this));

			return false;
		}

		public override bool OnStackAttempt(Mobile from, Item stack, Item dropped) => false;

		public override void OnDelete()
		{

			// remove any temporary quest attachments associated with this quest and quest owner
			XmlQuest.RemoveTemporaryQuestObjects(Owner, Name);

			base.OnDelete();

			// remove any reward items that might be attached to this
			ReturnReward();

			// determine whether the owner needs to be flagged with a quest attachment indicating completion of this quest
			QuestCompletionAttachment();


			CheckOwnerFlag();
		}

		public override void OnItemLifted(Mobile from, Item item)
		{
			base.OnItemLifted(from, item);

			if (from is PlayerMobile && PlayerMade && (Owner != null) && (Owner == Creator))
			{
				LootType = LootType.Regular;
			}
			else if (from is PlayerMobile && Owner == null)
			{
				Owner = from as PlayerMobile;

				LootType = LootType.Blessed;
				// flag the owner as carrying a questtoken
				Owner.SetFlag(XmlQuest.CarriedXmlQuestFlag, true);
			}
		}

		public override void OnAdded(IEntity target)
		{
			base.OnAdded(target);

			if ((target != null) && target is Container container)
			{
				// find the parent of the container
				// note, the only valid additions are to the player pack or a questbook.  Anything else is invalid.  
				// This is to avoid exploits involving storage or transfer of questtokens
				// make an exception for playermade quests that can be put on playervendors
				object parentOfTarget = container.Parent;

				// if this is a QuestBook then allow additions if it is in a players pack or it is a player quest
				if ((parentOfTarget != null) && parentOfTarget is Container container1 && target is XmlQuestBook)
				{
					parentOfTarget = container1.Parent;
				}

				// check to see if it can be added.
				// allow playermade quests to be placed in playervendors or in xmlquestbooks that are in the world (supports the playerquestboards)
				if (PlayerMade && (((parentOfTarget != null) && parentOfTarget is PlayerVendor) || ((parentOfTarget == null) && target is XmlQuestBook)))
				{
					CheckOwnerFlag();
					Owner = null;
					LootType = LootType.Regular;
				}
				else if ((parentOfTarget != null) && (parentOfTarget is PlayerMobile) && PlayerMade && (Owner != null) && ((Owner == Creator) || (Creator == null)))
				{
					// check the old owner
					CheckOwnerFlag();

					Owner = parentOfTarget as PlayerMobile;

					// first owner will become creator by default
					if (Creator == null)
						Creator = Owner;

					LootType = LootType.Blessed;

					// flag the new owner as carrying a questtoken
					Owner.SetFlag(XmlQuest.CarriedXmlQuestFlag, true);

				}
				else if ((parentOfTarget != null) && (parentOfTarget is PlayerMobile))
				{
					if (Owner == null)
					{
						Owner = parentOfTarget as PlayerMobile;
						LootType = LootType.Blessed;

						// flag the owner as carrying a questtoken
						Owner.SetFlag(XmlQuest.CarriedXmlQuestFlag, true);
					}
					else if ((parentOfTarget as PlayerMobile != Owner) || (target is BankBox))
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
					// allow placement into containers in the world, npcs or drop on their corpses when owner is null
					else if (parentOfTarget is not Mobile && target is not Corpse && parentOfTarget != null)
					{
						// invalidate the token
						CheckOwnerFlag();
						Invalidate();
					}
				}
			}
		}

		public List<XmlQuest.JournalEntry> Journal { get; set; }
		private static readonly char[] colondelim = new char[1] { ':' };

		public string EchoAddJournalEntry
		{
			set
			{
				// notify and echo journal text
				VerboseAddJournalEntry(value, true, true);
			}
		}

		public string NotifyAddJournalEntry
		{
			set
			{
				// notify
				VerboseAddJournalEntry(value, true, false);
			}
		}

		public string AddJournalEntry
		{
			set
			{
				// silent
				VerboseAddJournalEntry(value, false, false);
			}
		}

		private void VerboseAddJournalEntry(string entrystring, bool notify, bool echo)
		{
			if (entrystring == null)
				return;

			// parse the value
			string[] args = entrystring.Split(colondelim, 2);

			if (args == null)
				return;

			string entryID = null;
			string entryText = null;
			if (args.Length > 0)
			{
				entryID = args[0].Trim();
			}

			if (entryID == null || entryID.Length == 0)
				return;

			if (args.Length > 1)
			{
				entryText = args[1].Trim();
			}

			// allocate a new journal if none exists
			if (Journal == null) Journal = new List<XmlQuest.JournalEntry>();

			// go through the existing journal to find a matching ID
			XmlQuest.JournalEntry foundEntry = null;

			foreach (XmlQuest.JournalEntry e in Journal)
			{
				if (e.EntryID == entryID)
				{
					foundEntry = e;
					break;
				}
			}

			if (foundEntry != null)
			{
				// modify an existing entry
				if (entryText == null || entryText.Length == 0)
				{
					// delete the entry
					Journal.Remove(foundEntry);
				}
				else
				{
					// just replace the text
					foundEntry.EntryText = entryText;

					if (RootParent is Mobile holder)
					{
						if (notify)
						{
							// notify the player holding the questholder                       
							holder.SendMessage(JournalNotifyColor, "Journal entry '{0}' of quest '{1}' has been modified.", entryID, Name);
						}
						if (echo)
						{
							// echo the journal text to the player holding the questholder                       
							holder.SendMessage(JournalEchoColor, "{0}", entryText);
						}
					}
				}
			}
			else
			{
				// add a new entry
				if (entryText != null && entryText.Length != 0)
				{
					// add the new entry
					Journal.Add(new XmlQuest.JournalEntry(entryID, entryText));

					if (RootParent is Mobile holder)
					{
						if (notify)
						{
							// notify the player holding the questholder                       
							holder.SendMessage(JournalNotifyColor, "Journal entry '{0}' has been added to quest '{1}'.", entryID, Name);
						}
						if (echo)
						{
							// echo the journal text to the player holding the questholder                       
							holder.SendMessage(JournalEchoColor, "{0}", entryText);
						}
					}
				}
			}
		}



		private void QuestCompletionAttachment()
		{
			bool complete = IsCompleted;

			// is this quest repeatable
			if ((!Repeatable || NextRepeatable > TimeSpan.Zero) && complete)
			{
				double expiresin = Repeatable ? NextRepeatable.TotalMinutes : 0;

				// then add an attachment indicating that it has already been done
				XmlAttach.AttachTo(Owner, new XmlQuestAttachment(Name, expiresin));
			}

			// have quest points been enabled?
			if (XmlQuest.QuestPointsEnabled && complete && !PlayerMade)
			{
				XmlQuestPoints.GiveQuestPoints(Owner, this);
			}
		}

		private void PackItem(Item item)
		{
			if (item != null)
			{
				DropItem(item);
			}

			PackItemsMovable(this, false);

			// make sure the weight and gold of the questtoken is updated to reflect the weight of added rewards in playermade quests to avoid
			// exploits where quests are used as zero weight containers

			UpdateWeight();
		}


		private void CalculateWeight(Item target)
		{
			if (target is Container container)
			{
				int gold = 0;
				int weight = 0;
				int nitems = 0;

				foreach (Item i in container.Items)
				{
					// make sure gold amount is consistent with totalgold
					if (i is Gold)
					{
						UpdateTotal(i, TotalType.Gold, i.Amount);
					}

					if (i is Container)
					{
						CalculateWeight(i);
						weight += i.TotalWeight + (int)i.Weight;
						gold += i.TotalGold;
						nitems += i.TotalItems + 1;
					}
					else
					{
						weight += (int)(i.Weight * i.Amount);
						gold += i.TotalGold;
						nitems += 1;
					}
				}

				UpdateTotal(container, TotalType.Weight, weight);
				UpdateTotal(container, TotalType.Gold, gold);
				UpdateTotal(container, TotalType.Items, nitems);
			}
		}


		private void UpdateWeight()
		{


			// decide whether to hide the weight, gold, and number of the reward from the totals calculation

			if (PlayerMade)
			{
				UnHideRewards();
			}
			else
			{
				HideRewards();
			}

			// update the container totals
			UpdateTotals();

			// and the parent totals
			if (RootParent is Mobile mobile)
			{
				mobile.UpdateTotals();
			}

			// hide the reward item
			HideRewards();

		}

		private void ReturnReward()
		{
			if (m_RewardItem != null)
			{
				CheckRewardItem();

				// if this was player made, then return the item to the creator
				if (PlayerMade && (Creator != null) && !Creator.Deleted)
				{
					m_RewardItem.Movable = true;

					// make sure all of the items in the pack are movable as well
					PackItemsMovable(this, true);

					bool returned = false;

					if ((ReturnContainer != null) && !ReturnContainer.Deleted)
					{
						returned = ReturnContainer.TryDropItem(Creator, m_RewardItem, false);
						//ReturnContainer.DropItem(m_RewardItem);
					}
					if (!returned)
					{
						returned = Creator.AddToBackpack(m_RewardItem);
					}
					if (returned)
					{
						Creator.SendMessage("Your reward {0} was returned from quest {1}", m_RewardItem.GetType().Name, Name);
						//AddMobileWeight(Creator, m_RewardItem);
					}
					else
					{
						Creator.SendMessage("Attempted to return reward {0} from quest {1} : containers full.", m_RewardItem.GetType().Name, Name);
					}
				}
				else
				{
					// just delete it
					m_RewardItem.Delete();
				}
				m_RewardItem = null;
				UpdateWeight();
			}
			if (m_RewardAttachment != null)
			{
				// delete any remaining attachments
				m_RewardAttachment.Delete();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public PlayerMobile Owner { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public new string Name
		{
			get => PlayerMade ? "PQ: " + base.Name : base.Name;
			set
			{
				base.Name = value;
				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public PlayerMobile Creator { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int Difficulty { get; set; } = 1;

		[CommandProperty(AccessLevel.GameMaster)]
		public string Status
		{
			get => m_status_str;
			set => m_status_str = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public string NoteString { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool AutoReward { get; set; } = false;

		[CommandProperty(AccessLevel.GameMaster)]
		public bool CanSeeReward { get; set; } = true;

		[CommandProperty(AccessLevel.GameMaster)]
		public bool PlayerMade { get; set; } = false;

		[CommandProperty(AccessLevel.GameMaster)]
		public Container ReturnContainer { get; set; }

		private void PackItemsMovable(Container pack, bool canmove)
		{
			if (pack == null)
				return;
			UnHideRewards();
			Item[] itemlist = pack.FindItemsByType(typeof(Item), true);
			if (itemlist != null)
			{
				for (int i = 0; i < itemlist.Length; i++)
				{
					itemlist[i].Movable = canmove;
				}
			}

		}

		private void RestoreRewardAttachment()
		{
			m_RewardAttachment = XmlAttach.FindAttachmentBySerial(m_RewardAttachmentSerialNumber);
		}

		public XmlAttachment RewardAttachment
		{
			get
			{
				// if the reward item is not set, and the reward string is specified, then use the reward string to construct and assign the
				// reward item
				// dont allow player made quests to use the rewardstring creation feature
				if (m_RewardAttachment != null && m_RewardAttachment.Deleted) m_RewardAttachment = null;

				if ((m_RewardAttachment == null || m_RewardAttachment.Deleted) &&
					(AttachmentString != null) && !PlayerMade)
				{
					object o = XmlQuest.CreateItem(this, AttachmentString, out m_status_str, typeof(XmlAttachment));
					if (o is Item item)
					{
						item.Delete();
					}
					else if (o is XmlAttachment)
					{
						m_RewardAttachment = o as XmlAttachment;
						m_RewardAttachment.OwnedBy = this;
					}
				}

				return m_RewardAttachment;
			}
			set
			{
				// get rid of any existing attachment
				if (m_RewardAttachment != null && !m_RewardAttachment.Deleted)
				{
					m_RewardAttachment.Delete();
				}

				m_RewardAttachment = value;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public Item RewardItem
		{
			get
			{
				// if the reward item is not set, and the reward string is specified, then use the reward string to construct and assign the
				// reward item
				// dont allow player made quests to use the rewardstring creation feature
				if ((m_RewardItem == null || m_RewardItem.Deleted) &&
					(RewardString != null) && !PlayerMade)
				{
					object o = XmlQuest.CreateItem(this, RewardString, out m_status_str, typeof(Item));
					if (o is Item)
					{
						m_RewardItem = o as Item;
						PackItem(m_RewardItem);
					}
					else if (o is XmlAttachment attachment)
					{
						attachment.Delete();
					}
				}

				return m_RewardItem;
			}
			set
			{
				// get rid of any existing reward item if it has been assigned
				if (m_RewardItem != null && !m_RewardItem.Deleted)
				{

					ReturnReward();
				}

				// and assign the new item
				m_RewardItem = value;

				/*
				// is this currently carried by a mobile?
				if(m_RewardItem.RootParent != null && m_RewardItem.RootParent is Mobile)
				{
					// if so then remove it
					((Mobile)(m_RewardItem.RootParent)).RemoveItem(m_RewardItem);

				}
				*/

				// and put it in the pack
				if (m_RewardItem != null && !m_RewardItem.Deleted)
				{
					PackItem(m_RewardItem);
				}


			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public string TitleString
		{
			get => m_TitleString;
			set { m_TitleString = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public string RewardAction { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public string RewardString { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public string AttachmentString { get; set; }


		[CommandProperty(AccessLevel.GameMaster)]
		public string ConfigFile { get; set; }
		[CommandProperty(AccessLevel.GameMaster)]
		public bool LoadConfig
		{
			get { return false; }
			set { if (value == true) LoadXmlConfig(ConfigFile); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool PartyEnabled { get; set; } = false;
		[CommandProperty(AccessLevel.GameMaster)]
		public int PartyRange { get; set; } = -1;
		[CommandProperty(AccessLevel.GameMaster)]
		public string State1 { get; set; }
		[CommandProperty(AccessLevel.GameMaster)]
		public string State2 { get; set; }
		[CommandProperty(AccessLevel.GameMaster)]
		public string State3 { get; set; }
		[CommandProperty(AccessLevel.GameMaster)]
		public string State4 { get; set; }
		[CommandProperty(AccessLevel.GameMaster)]
		public string State5 { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public string Description1 { get; set; }
		[CommandProperty(AccessLevel.GameMaster)]
		public string Description2 { get; set; }
		[CommandProperty(AccessLevel.GameMaster)]
		public string Description3 { get; set; }
		[CommandProperty(AccessLevel.GameMaster)]
		public string Description4 { get; set; }
		[CommandProperty(AccessLevel.GameMaster)]
		public string Description5 { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public string Objective1 { get; set; }
		[CommandProperty(AccessLevel.GameMaster)]
		public string Objective2 { get; set; }
		[CommandProperty(AccessLevel.GameMaster)]
		public string Objective3 { get; set; }
		[CommandProperty(AccessLevel.GameMaster)]
		public string Objective4 { get; set; }
		[CommandProperty(AccessLevel.GameMaster)]
		public string Objective5 { get; set; }
		[CommandProperty(AccessLevel.GameMaster)]
		public bool Completed1
		{
			get => m_Completed1;
			set
			{
				m_Completed1 = value;
				CheckAutoReward();
			}
		}
		[CommandProperty(AccessLevel.GameMaster)]
		public bool Completed2
		{
			get => m_Completed2;
			set
			{
				m_Completed2 = value;
				CheckAutoReward();
			}
		}
		[CommandProperty(AccessLevel.GameMaster)]
		public bool Completed3
		{
			get => m_Completed3;
			set
			{
				m_Completed3 = value;
				CheckAutoReward();
			}
		}
		[CommandProperty(AccessLevel.GameMaster)]
		public bool Completed4
		{
			get => m_Completed4;
			set
			{
				m_Completed4 = value;
				CheckAutoReward();
			}
		}
		[CommandProperty(AccessLevel.GameMaster)]
		public bool Completed5
		{
			get => m_Completed5;
			set
			{
				m_Completed5 = value;
				CheckAutoReward();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime TimeCreated { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public double Expiration
		{
			get => m_ExpirationDuration;
			set
			{
				// cap the max value at 100 years
				if (value > 876000)
				{
					m_ExpirationDuration = 876000;
				}
				else
				{
					m_ExpirationDuration = value;
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public TimeSpan ExpiresIn
		{
			get
			{
				if (m_ExpirationDuration > 0)
				{
					// if this is a player created quest, then refresh the expiration time until it is in someone elses possession
					/*
					 if(PlayerMade && ((Owner == Creator) || (Owner == null)))
					 {
						 m_TimeCreated = DateTime.UtcNow;
					 }
					 */
					return TimeCreated + TimeSpan.FromHours(m_ExpirationDuration) - DateTime.UtcNow;
				}
				else
				{
					return TimeSpan.FromHours(0);
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual bool IsExpired => (m_ExpirationDuration > 0) && (ExpiresIn <= TimeSpan.FromHours(0));

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual bool Repeatable
		{
			get => m_Repeatable;
			set
			{
				m_Repeatable = value;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual TimeSpan NextRepeatable
		{
			get => m_NextRepeatable;
			set
			{
				m_NextRepeatable = value;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual bool AlreadyDone =>
				// look for a quest attachment with the current quest name
				XmlAttach.FindAttachment(Owner, typeof(XmlQuestAttachment), Name) != null;

		public virtual string ExpirationString
		{
			get
			{
				if (AlreadyDone)
				{
					return "Already done";
				}
				else if (m_ExpirationDuration <= 0)
				{
					return "Never expires";
				}
				else if (IsExpired)
				{
					return "Expired";
				}
				else
				{
					TimeSpan ts = ExpiresIn;

					int days = (int)ts.TotalDays;
					int hours = (int)(ts - TimeSpan.FromDays(days)).TotalHours;
					int minutes = (int)(ts - TimeSpan.FromHours(hours)).TotalMinutes;
					int seconds = (int)(ts - TimeSpan.FromMinutes(minutes)).TotalSeconds;

					if (days > 0)
					{
						return $"Expires in {days} days {hours} hrs";
					}
					else if (hours > 0)
					{
						return $"Expires in {hours} hrs {minutes} mins";
					}
					else
					{
						return $"Expires in {minutes} mins {seconds} secs";
					}
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual bool IsValid
		{
			get
			{
				if (IsExpired)
				{
					// eliminate reward definitions
					RewardString = null;
					AttachmentString = null;

					// return any reward items
					ReturnReward();

					return false;
				}
				else
					if (AlreadyDone)
				{
					return false;
				}
				else
					return true;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual bool IsCompleted
		{
			get
			{
				if (IsValid &&
					(Completed1 || Objective1 == null || (Objective1.Length == 0)) &&
					(Completed2 || Objective2 == null || (Objective2.Length == 0)) &&
					(Completed3 || Objective3 == null || (Objective3.Length == 0)) &&
					(Completed4 || Objective4 == null || (Objective4.Length == 0)) &&
					(Completed5 || Objective5 == null || (Objective5.Length == 0))
					)
					return true;
				else
					return false;
			}
		}

		public Container Pack => this;

		// this is the handler for skill use
		// not yet implemented, just a hook for now
		public void OnSkillUse(Mobile m, Skill skill, bool success)
		{
			if (m == Owner && IsValid)
			{
				//m_skillTriggerActivated  = false;

				// do a location test for the skill use
				/*
				if ( !Utility.InRange( m.Location, this.Location, m_ProximityRange ) )
					return;
				*/
				int testskill = -1;

				// check the skill trigger conditions, Skillname,min,max
				try
				{
					testskill = (int)Enum.Parse(typeof(SkillName), m_SkillTrigger);
				}
				catch { }

				if (m_SkillTrigger != null && (int)skill.SkillName == testskill)
				{
					// have a skill trigger so flag it and test it
					//m_skillTriggerActivated  = true;
				}

			}
		}

		public bool HandlesOnSkillUse => (IsValid && m_SkillTrigger != null && m_SkillTrigger.Length > 0);

		private void CheckOwnerFlag()
		{
			if (Owner != null && !Owner.Deleted)
			{
				// need to check to see if any other questtoken items are owned
				// search the Owners top level pack for an xmlquest
				List<Item> list = XmlQuest.FindXmlQuest(Owner);

				if (list == null || list.Count == 0)
				{

					// if none remain then flag the ower as having none
					Owner.SetFlag(XmlQuest.CarriedXmlQuestFlag, false);
				}
			}


		}

		public virtual void Invalidate()
		{
			//Hue = 32;
			//LootType = LootType.Regular;
			if (Owner != null)
			{
				Owner.SendMessage(string.Format("Quest invalidated - '{0}' removed", Name));
			}
			Delete();
		}

		public void CheckRewardItem()
		{
			// go through all reward items and delete anything that is movable.  This blocks any exploits where players might
			// try to add items themselves
			if (m_RewardItem != null && !m_RewardItem.Deleted && m_RewardItem is Container container)
			{
				foreach (Item i in container.FindItemsByType(typeof(Item), true))
				{
					if (i.Movable)
					{
						i.Delete();
					}
				}
			}

		}

		public void CheckAutoReward()
		{
			if (!Deleted && AutoReward && IsCompleted && Owner != null &&
				((RewardItem != null && !m_RewardItem.Deleted) || (RewardAttachment != null && !m_RewardAttachment.Deleted) || RewardAction != null))
			{
				if (RewardItem != null)
				{
					// make sure nothing has been added to the pack other than the original reward items
					CheckRewardItem();

					m_RewardItem.Movable = true;

					// make sure all of the items in the pack are movable as well
					PackItemsMovable(this, true);

					Owner.AddToBackpack(m_RewardItem);
					//AddMobileWeight(Owner,m_RewardItem);

					m_RewardItem = null;
				}

				if (RewardAttachment != null)
				{
					Timer.DelayCall(new TimerStateCallback(AttachToCallback), new object[] { Owner, m_RewardAttachment });

					m_RewardAttachment = null;
				}

				if (RewardAction != null)
				{
					BaseXmlSpawner.ExecuteActions(Owner, Owner, RewardAction);
				}

				Owner.SendMessage($"{Name} completed. You receive the quest reward!");
				Delete();
			}
		}

		public static void AttachToCallback(object state)
		{
			object[] args = (object[])state;

			XmlAttach.AttachTo(args[0], (XmlAttachment)args[1]);
		}




		private const string XmlTableName = "Properties";
		private const string XmlDataSetName = "XmlQuestHolder";

		public void LoadXmlConfig(string filename)
		{
			if (filename == null || filename.Length <= 0)
				return;
			// Check if the file exists
			if (System.IO.File.Exists(filename) == true)
			{
				FileStream fs = null;
				try
				{
					fs = File.Open(filename, FileMode.Open, FileAccess.Read);
				}
				catch { }

				if (fs == null)
				{
					Status = $"Unable to open {filename} for loading";
					return;
				}

				// Create the data set
				DataSet ds = new(XmlDataSetName);

				// Read in the file
				//ds.ReadXml( e.Arguments[0].ToString() );
				bool fileerror = false;
				try
				{
					ds.ReadXml(fs);
				}
				catch { fileerror = true; }

				// close the file
				fs.Close();
				if (fileerror)
				{
					Console.WriteLine("XmlQuestHolder: Error in XML config file '{0}'", filename);
					return;
				}
				// Check that at least a single table was loaded
				if (ds.Tables != null && ds.Tables.Count > 0)
				{
					if (ds.Tables[XmlTableName] != null && ds.Tables[XmlTableName].Rows.Count > 0)
					{
						foreach (DataRow dr in ds.Tables[XmlTableName].Rows)
						{
							bool valid_entry;
							string strEntry = null;
							valid_entry = true;
							try { strEntry = (string)dr["Name"]; }
							catch { valid_entry = false; }
							if (valid_entry)
							{
								Name = strEntry;
							}

							valid_entry = true;
							strEntry = null;
							try { strEntry = (string)dr["Title"]; }
							catch { valid_entry = false; }
							if (valid_entry)
							{
								TitleString = strEntry;
							}

							valid_entry = true;
							strEntry = null;
							try { strEntry = (string)dr["Note"]; }
							catch { valid_entry = false; }
							if (valid_entry)
							{
								NoteString = strEntry;
							}

							valid_entry = true;
							strEntry = null;
							try { strEntry = (string)dr["Reward"]; }
							catch { valid_entry = false; }
							if (valid_entry)
							{
								RewardString = strEntry;
							}

							valid_entry = true;
							strEntry = null;
							try { strEntry = (string)dr["Attachment"]; }
							catch { valid_entry = false; }
							if (valid_entry)
							{
								AttachmentString = strEntry;
							}

							valid_entry = true;
							strEntry = null;
							try { strEntry = (string)dr["Objective1"]; }
							catch { valid_entry = false; }
							if (valid_entry)
							{
								Objective1 = strEntry;
							}

							valid_entry = true;
							strEntry = null;
							try { strEntry = (string)dr["Objective2"]; }
							catch { valid_entry = false; }
							if (valid_entry)
							{
								Objective2 = strEntry;
							}

							valid_entry = true;
							strEntry = null;
							try { strEntry = (string)dr["Objective3"]; }
							catch { valid_entry = false; }
							if (valid_entry)
							{
								Objective3 = strEntry;
							}

							valid_entry = true;
							strEntry = null;
							try { strEntry = (string)dr["Objective4"]; }
							catch { valid_entry = false; }
							if (valid_entry)
							{
								Objective4 = strEntry;
							}

							valid_entry = true;
							strEntry = null;
							try { strEntry = (string)dr["Objective5"]; }
							catch { valid_entry = false; }
							if (valid_entry)
							{
								Objective5 = strEntry;
							}

							valid_entry = true;
							strEntry = null;
							try { strEntry = (string)dr["Description1"]; }
							catch { valid_entry = false; }
							if (valid_entry)
							{
								Description1 = strEntry;
							}

							valid_entry = true;
							strEntry = null;
							try { strEntry = (string)dr["Description2"]; }
							catch { valid_entry = false; }
							if (valid_entry)
							{
								Description2 = strEntry;
							}

							valid_entry = true;
							strEntry = null;
							try { strEntry = (string)dr["Description3"]; }
							catch { valid_entry = false; }
							if (valid_entry)
							{
								Description3 = strEntry;
							}

							valid_entry = true;
							strEntry = null;
							try { strEntry = (string)dr["Description4"]; }
							catch { valid_entry = false; }
							if (valid_entry)
							{
								Description4 = strEntry;
							}

							valid_entry = true;
							strEntry = null;
							try { strEntry = (string)dr["Description5"]; }
							catch { valid_entry = false; }
							if (valid_entry)
							{
								Description5 = strEntry;
							}

							valid_entry = true;
							bool boolEntry = false;
							try { boolEntry = bool.Parse((string)dr["PartyEnabled"]); }
							catch { valid_entry = false; }
							if (valid_entry)
							{
								PartyEnabled = boolEntry;
							}

							valid_entry = true;
							boolEntry = false;
							try { boolEntry = bool.Parse((string)dr["AutoReward"]); }
							catch { valid_entry = false; }
							if (valid_entry)
							{
								AutoReward = boolEntry;
							}

							valid_entry = true;
							boolEntry = true;
							try { boolEntry = bool.Parse((string)dr["CanSeeReward"]); }
							catch { valid_entry = false; }
							if (valid_entry)
							{
								CanSeeReward = boolEntry;
							}

							valid_entry = true;
							boolEntry = true;
							try { boolEntry = bool.Parse((string)dr["Repeatable"]); }
							catch { valid_entry = false; }
							if (valid_entry)
							{
								m_Repeatable = boolEntry;
							}

							valid_entry = true;
							TimeSpan timespanEntry = TimeSpan.Zero;
							try { timespanEntry = TimeSpan.Parse((string)dr["NextRepeatable"]); }
							catch { valid_entry = false; }
							if (valid_entry)
							{
								m_NextRepeatable = timespanEntry;
							}

							valid_entry = true;
							boolEntry = false;
							try { boolEntry = bool.Parse((string)dr["PlayerMade"]); }
							catch { valid_entry = false; }
							if (valid_entry)
							{
								PlayerMade = boolEntry;
							}

							valid_entry = true;
							int intEntry = 0;
							try { intEntry = int.Parse((string)dr["PartyRange"]); }
							catch { valid_entry = false; }
							if (valid_entry)
							{
								PartyRange = intEntry;
							}

							valid_entry = true;
							double doubleEntry = 0;
							try { doubleEntry = double.Parse((string)dr["Expiration"]); }
							catch { valid_entry = false; }
							if (valid_entry)
							{
								Expiration = doubleEntry;
							}
						}
					}
				}
			}
		}
	}
}
