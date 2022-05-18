using Server.ContextMenus;
using Server.Engines.XmlSpawner2;
using Server.Items;
using Server.Network;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Server.Mobiles
{
	public abstract class TalkingBaseVendor : BaseVendor
	{

		public TalkingBaseVendor(string title) : base(title)
		{
			// add the XmlDialog attachment
			DialogAttachment = new XmlDialog((string)null);
			XmlAttach.AttachTo(this, DialogAttachment);
		}

		public TalkingBaseVendor(Serial serial) : base(serial)
		{
		}

		public static void Initialize()
		{
			// reestablish the DialogAttachment assignment
			foreach (Mobile m in World.Mobiles.Values)
			{
				if (m is TalkingBaseVendor vendor)
				{
					XmlDialog xa = XmlAttach.FindAttachment(m, typeof(XmlDialog)) as XmlDialog;
					vendor.DialogAttachment = xa;
				}
			}
		}

		public XmlDialog DialogAttachment { get; set; }

		private DateTime lasteffect;
		private Point3D m_Offset = new(0, 0, 20); // overhead

		[CommandProperty(AccessLevel.GameMaster)]
		public int EItemID { get; set; } = 0;// 0 = disable, 14202 = sparkle, 6251 = round stone, 7885 = light pyramid

		[CommandProperty(AccessLevel.GameMaster)]
		public Point3D EOffset
		{
			get { return m_Offset; }
			set
			{
				m_Offset = value;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int EDuration { get; set; } = 70;

		[CommandProperty(AccessLevel.GameMaster)]
		public int EHue { get; set; } = 68;

		private void DisplayHighlight()
		{
			if (EItemID > 0)
			{
				Effects.SendLocationEffect(new Point3D(Location.X + EOffset.X, Location.Y + EOffset.Y, Location.Z + EOffset.Z), Map, EItemID, EDuration, EHue, 0);
				lasteffect = DateTime.Now;
			}
		}

		public override void OnThink()
		{
			base.OnThink();

			if (lasteffect + TimeSpan.FromSeconds(1) < DateTime.Now)
			{
				DisplayHighlight();
			}
		}

		public override bool Move(Direction d)
		{
			bool didmove = base.Move(d);

			DisplayHighlight();

			return didmove;
		}

		private string m_TalkText;

		[CommandProperty(AccessLevel.GameMaster)]
		public string TalkText { get { return m_TalkText; } set { m_TalkText = value; } }

		// properties below are modified to access the equivalent XmlDialog properties
		// this is largely for backward compatibility, but it does also add some convenience

		public Mobile ActivePlayer
		{
			get => DialogAttachment?.ActivePlayer;
			set
			{
				if (DialogAttachment != null)
					DialogAttachment.ActivePlayer = value;
			}
		}

		public ArrayList SpeechEntries
		{
			get => DialogAttachment?.SpeechEntries;
			set
			{
				if (DialogAttachment != null)
					DialogAttachment.SpeechEntries = value;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public TimeSpan GameTOD
		{
			get
			{
				Clock.GetTime(Map, Location.X, Location.Y, out int hours, out int minutes);
				return (new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hours, minutes, 0).TimeOfDay);
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public static TimeSpan RealTOD => DateTime.Now.TimeOfDay;

		[CommandProperty(AccessLevel.GameMaster)]
		public static int RealDay => DateTime.Now.Day;

		[CommandProperty(AccessLevel.GameMaster)]
		public static int RealMonth => DateTime.Now.Month;

		[CommandProperty(AccessLevel.GameMaster)]
		public static DayOfWeek RealDayOfWeek => DateTime.Now.DayOfWeek;

		[CommandProperty(AccessLevel.GameMaster)]
		public MoonPhase MoonPhase => Clock.GetMoonPhase(Map, Location.X, Location.Y);

		[CommandProperty(AccessLevel.GameMaster)]
		public AccessLevel TriggerAccessLevel
		{
			get => DialogAttachment != null ? DialogAttachment.TriggerAccessLevel : AccessLevel.Player;
			set
			{
				if (DialogAttachment != null)
					DialogAttachment.TriggerAccessLevel = value;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime LastInteraction
		{
			get => DialogAttachment != null ? DialogAttachment.LastInteraction : DateTime.MinValue;
			set
			{
				if (DialogAttachment != null)
					DialogAttachment.LastInteraction = value;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool DoReset
		{
			get => false;
			set
			{
				if (DialogAttachment != null)
					DialogAttachment.DoReset = value;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool IsActive
		{
			get => DialogAttachment != null ? DialogAttachment.IsActive : false;
			set
			{
				if (DialogAttachment != null)
					DialogAttachment.IsActive = value;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool AllowGhostTrig
		{
			get => DialogAttachment != null ? DialogAttachment.AllowGhostTrig : false;
			set
			{
				if (DialogAttachment != null)
					DialogAttachment.AllowGhostTrig = value;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Running
		{
			get => DialogAttachment != null ? DialogAttachment.Running : false;
			set
			{
				if (DialogAttachment != null)
					DialogAttachment.Running = value;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public TimeSpan ResetTime
		{
			get => DialogAttachment != null ? DialogAttachment.ResetTime : TimeSpan.Zero;
			set
			{
				if (DialogAttachment != null)
					DialogAttachment.ResetTime = value;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int SpeechPace
		{
			get => DialogAttachment != null ? DialogAttachment.SpeechPace : 0;
			set
			{
				if (DialogAttachment != null)
					DialogAttachment.SpeechPace = value;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public string Keywords
		{
			get => DialogAttachment != null && DialogAttachment.CurrentEntry != null ? DialogAttachment.CurrentEntry.Keywords : null;
			set
			{
				if (DialogAttachment != null && DialogAttachment.CurrentEntry != null)
					DialogAttachment.CurrentEntry.Keywords = value;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public string Action
		{
			get => DialogAttachment != null && DialogAttachment.CurrentEntry != null ? DialogAttachment.CurrentEntry.Action : null;
			set
			{
				if (DialogAttachment != null && DialogAttachment.CurrentEntry != null)
					DialogAttachment.CurrentEntry.Action = value;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public string Condition
		{
			get => DialogAttachment != null && DialogAttachment.CurrentEntry != null ? DialogAttachment.CurrentEntry.Condition : null;
			set
			{
				if (DialogAttachment != null && DialogAttachment.CurrentEntry != null)
					DialogAttachment.CurrentEntry.Condition = value;
			}

		}

		[CommandProperty(AccessLevel.GameMaster)]
		public string Text
		{
			get => DialogAttachment != null && DialogAttachment.CurrentEntry != null ? DialogAttachment.CurrentEntry.Text : null;
			set
			{
				if (DialogAttachment != null && DialogAttachment.CurrentEntry != null)
					DialogAttachment.CurrentEntry.Text = value;
			}
		}



		[CommandProperty(AccessLevel.GameMaster)]
		public string DependsOn
		{
			get => DialogAttachment != null && DialogAttachment.CurrentEntry != null ? DialogAttachment.CurrentEntry.DependsOn : "-1";
			set
			{
				if (DialogAttachment != null && DialogAttachment.CurrentEntry != null)
					DialogAttachment.CurrentEntry.DependsOn = value;
			}

		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool LockConversation
		{
			get => DialogAttachment != null && DialogAttachment.CurrentEntry != null ? DialogAttachment.CurrentEntry.LockConversation : false;
			set
			{
				if (DialogAttachment != null && DialogAttachment.CurrentEntry != null)
					DialogAttachment.CurrentEntry.LockConversation = value;
			}

		}

		[CommandProperty(AccessLevel.GameMaster)]
		public MessageType SpeechStyle
		{

			get => DialogAttachment != null && DialogAttachment.CurrentEntry != null
					? DialogAttachment.CurrentEntry.SpeechStyle
					: MessageType.Regular;
			set
			{
				if (DialogAttachment != null && DialogAttachment.CurrentEntry != null)
					DialogAttachment.CurrentEntry.SpeechStyle = value;
			}

		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool AllowNPCTrigger
		{
			get => DialogAttachment != null && DialogAttachment.CurrentEntry != null ? DialogAttachment.CurrentEntry.AllowNPCTrigger : false;
			set
			{
				if (DialogAttachment != null && DialogAttachment.CurrentEntry != null)
					DialogAttachment.CurrentEntry.AllowNPCTrigger = value;
			}

		}


		[CommandProperty(AccessLevel.GameMaster)]
		public int Pause
		{

			get => DialogAttachment != null && DialogAttachment.CurrentEntry != null ? DialogAttachment.CurrentEntry.Pause : -1;
			set
			{
				if (DialogAttachment != null && DialogAttachment.CurrentEntry != null)
					DialogAttachment.CurrentEntry.Pause = value;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int PrePause
		{
			get => DialogAttachment != null && DialogAttachment.CurrentEntry != null ? DialogAttachment.CurrentEntry.PrePause : -1;
			set
			{
				if (DialogAttachment != null && DialogAttachment.CurrentEntry != null)
					DialogAttachment.CurrentEntry.PrePause = value;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int ID
		{
			get => DialogAttachment != null && DialogAttachment.CurrentEntry != null ? DialogAttachment.CurrentEntry.ID : -1;
			set
			{
				if (DialogAttachment != null && DialogAttachment.CurrentEntry != null)
					DialogAttachment.CurrentEntry.ID = value;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int EntryNumber
		{
			get => DialogAttachment != null ? DialogAttachment.EntryNumber : -1;
			set
			{
				if (DialogAttachment != null)
				{
					DialogAttachment.EntryNumber = value;
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int ProximityRange
		{
			get => DialogAttachment != null ? DialogAttachment.ProximityRange : -1;
			set
			{
				if (DialogAttachment != null)
				{
					DialogAttachment.ProximityRange = value;
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public string ConfigFile
		{
			get => DialogAttachment?.ConfigFile;
			set
			{
				if (DialogAttachment != null)
				{
					DialogAttachment.ConfigFile = value;
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool LoadConfig
		{
			get => false;
			set { if (value && DialogAttachment != null) DialogAttachment.DoLoadNPC(null, ConfigFile); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool SaveConfig
		{
			get => false;
			set
			{
				if (value && DialogAttachment != null)
					DialogAttachment.DoSaveNPC(null, ConfigFile, false);
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public string TriggerOnCarried
		{
			get => DialogAttachment?.TriggerOnCarried;
			set
			{
				if (DialogAttachment != null)
				{
					DialogAttachment.TriggerOnCarried = value;
				}
			}

		}
		[CommandProperty(AccessLevel.GameMaster)]
		public string NoTriggerOnCarried
		{
			get => DialogAttachment?.NoTriggerOnCarried;
			set
			{
				if (DialogAttachment != null)
				{
					DialogAttachment.NoTriggerOnCarried = value;
				}
			}

		}

		public XmlDialog.SpeechEntry CurrentEntry
		{
			get => DialogAttachment?.CurrentEntry;
			set
			{
				if (DialogAttachment != null)
				{
					DialogAttachment.CurrentEntry = value;
				}
			}

		}

		public override bool OnDragDrop(Mobile from, Item item)
		{

			return XmlQuest.RegisterGive(from, this, item);

			//return base.OnDragDrop(from, item);
		}

		private class TalkEntry : ContextMenuEntry
		{
			private readonly TalkingBaseVendor m_NPC;

			public TalkEntry(TalkingBaseVendor npc) : base(6146)
			{
				m_NPC = npc;
			}

			public override void OnClick()
			{
				Mobile from = Owner.From;

				if (m_NPC == null || m_NPC.Deleted || !from.CheckAlive() || m_NPC.DialogAttachment == null)
					return;

				// process the talk text
				//m_NPC.DialogAttachment.ProcessSpeech(from, m_NPC.TalkText);
				from.DoSpeech(m_NPC.TalkText, Array.Empty<int>(), MessageType.Regular, from.SpeechHue);
			}
		}

		public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
		{
			if (from.Alive)
			{
				if (TalkText != null && TalkText.Length > 0 && DialogAttachment != null)
				{
					list.Add(new TalkEntry(this));
				}
			}

			base.GetContextMenuEntries(from, list);
		}



		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)7); // version

			// version 7
			writer.Write(EItemID);
			writer.Write(EDuration);
			writer.Write(m_Offset);
			writer.Write(EHue);

			// version 6
			writer.Write(m_TalkText);

			// Version 5
			// all serialized data now handled by the XmlDialog attachment

		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			if (version < 5)
			{
				// have to add the XmlDialog attachment
				DialogAttachment = new XmlDialog((string)null);
				XmlAttach.AttachTo(this, DialogAttachment);
			}

			switch (version)
			{
				case 7:
					EItemID = reader.ReadInt();
					EDuration = reader.ReadInt();
					m_Offset = reader.ReadPoint3D();
					EHue = reader.ReadInt();
					goto case 6;
				case 6:
					TalkText = reader.ReadString();
					break;
				case 5:
					{
						break;
					}
				case 4:
					{
						int count = reader.ReadInt();

						SpeechEntries = new ArrayList();
						for (int i = 0; i < count; i++)
						{
							XmlDialog.SpeechEntry newentry = new()
							{
								Condition = reader.ReadString()
							};

							SpeechEntries.Add(newentry);
						}

						goto case 3;
					}
				case 3:
					{
						TriggerOnCarried = reader.ReadString();
						NoTriggerOnCarried = reader.ReadString();
						goto case 2;
					}
				case 2:
					{
						SpeechPace = reader.ReadInt();

						int count = reader.ReadInt();
						if (version < 4)
						{
							SpeechEntries = new ArrayList();
						}
						for (int i = 0; i < count; i++)
						{
							if (version < 4)
							{
								XmlDialog.SpeechEntry newentry = new()
								{
									PrePause = reader.ReadInt(),
									LockConversation = reader.ReadBool(),
									AllowNPCTrigger = reader.ReadBool(),
									SpeechStyle = (MessageType)reader.ReadInt()
								};

								SpeechEntries.Add(newentry);
							}
							else
							{
								XmlDialog.SpeechEntry newentry = (XmlDialog.SpeechEntry)SpeechEntries[i];

								newentry.PrePause = reader.ReadInt();
								newentry.LockConversation = reader.ReadBool();
								newentry.AllowNPCTrigger = reader.ReadBool();
								newentry.SpeechStyle = (MessageType)reader.ReadInt();
							}
						}
						goto case 1;
					}
				case 1:
					{
						ActivePlayer = reader.ReadMobile();
						goto case 0;
					}
				case 0:
					{
						IsActive = reader.ReadBool();
						ResetTime = reader.ReadTimeSpan();
						LastInteraction = reader.ReadDateTime();
						AllowGhostTrig = reader.ReadBool();
						ProximityRange = reader.ReadInt();
						Running = reader.ReadBool();
						ConfigFile = reader.ReadString();
						int count = reader.ReadInt();
						if (version < 2)
						{
							SpeechEntries = new ArrayList();
						}
						for (int i = 0; i < count; i++)
						{

							if (version < 2)
							{
								XmlDialog.SpeechEntry newentry = new()
								{
									EntryNumber = reader.ReadInt(),
									ID = reader.ReadInt(),
									Text = reader.ReadString(),
									Keywords = reader.ReadString(),
									Action = reader.ReadString(),
									DependsOn = reader.ReadInt().ToString(),
									Pause = reader.ReadInt()
								};

								SpeechEntries.Add(newentry);
							}
							else
							{
								XmlDialog.SpeechEntry newentry = (XmlDialog.SpeechEntry)SpeechEntries[i];

								newentry.EntryNumber = reader.ReadInt();
								newentry.ID = reader.ReadInt();
								newentry.Text = reader.ReadString();
								newentry.Keywords = reader.ReadString();
								newentry.Action = reader.ReadString();
								newentry.DependsOn = reader.ReadInt().ToString();
								newentry.Pause = reader.ReadInt();
							}
						}
						// read in the current entry number. Note this will also set the current entry
						EntryNumber = reader.ReadInt();
						// restart the timer if it was active
						bool isrunning = reader.ReadBool();
						if (isrunning)
						{
							Mobile trigmob = reader.ReadMobile();
							TimeSpan delay = reader.ReadTimeSpan();
							if (DialogAttachment != null)
								DialogAttachment.DoTimer(delay, trigmob);
						}
						break;
					}
			}
		}
	}
}
