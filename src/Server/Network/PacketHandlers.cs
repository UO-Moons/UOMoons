using Server.Accounting;
using Server.ContextMenus;
using Server.Diagnostics;
using Server.Gumps;
using Server.HuePickers;
using Server.Items;
using Server.Menus;
using Server.Mobiles;
using Server.Prompts;
using Server.Targeting;
using System;
using System.Collections.Generic;
using System.IO;
using CV = Server.ClientVersion;

namespace Server.Network
{
	public enum MessageType
	{
		Regular = 0x00,
		System = 0x01,
		Emote = 0x02,
		Label = 0x06,
		Focus = 0x07,
		Whisper = 0x08,
		Yell = 0x09,
		Spell = 0x0A,

		Guild = 0x0D,
		Alliance = 0x0E,
		Command = 0x0F,

		Encoded = 0xC0
	}

	public static class PacketHandlers
	{
		private static readonly PacketHandler[] m_6017Handlers;

		private static readonly PacketHandler[] m_ExtendedHandlersLow;
		private static readonly Dictionary<int, PacketHandler> m_ExtendedHandlersHigh;

		private static readonly EncodedPacketHandler[] m_EncodedHandlersLow;
		private static readonly Dictionary<int, EncodedPacketHandler> m_EncodedHandlersHigh;

		public static PacketHandler[] Handlers { get; private set; }

		static PacketHandlers()
		{
			Handlers = new PacketHandler[0x100];
			m_6017Handlers = new PacketHandler[0x100];

			m_ExtendedHandlersLow = new PacketHandler[0x100];
			m_ExtendedHandlersHigh = new Dictionary<int, PacketHandler>();

			m_EncodedHandlersLow = new EncodedPacketHandler[0x100];
			m_EncodedHandlersHigh = new Dictionary<int, EncodedPacketHandler>();

			Register(0x00, 104, false, new OnPacketReceive(CreateCharacter));
			Register(0x01, 5, false, new OnPacketReceive(Disconnect));
			Register(0x02, 7, true, new OnPacketReceive(MovementReq));
			Register(0x03, 0, true, new OnPacketReceive(AsciiSpeech));
			Register(0x04, 2, true, new OnPacketReceive(GodModeRequest));
			Register(0x05, 5, true, new OnPacketReceive(AttackReq));
			Register(0x06, 5, true, new OnPacketReceive(UseReq));
			Register(0x07, 7, true, new OnPacketReceive(LiftReq));
			Register(0x08, 14, true, new OnPacketReceive(DropReq));
			Register(0x09, 5, true, new OnPacketReceive(LookReq));
			Register(0x0A, 11, true, new OnPacketReceive(Edit));
			Register(0x12, 0, true, new OnPacketReceive(TextCommand));
			Register(0x13, 10, true, new OnPacketReceive(EquipReq));
			Register(0x14, 6, true, new OnPacketReceive(ChangeZ));
			Register(0x22, 3, true, new OnPacketReceive(Resynchronize));
			Register(0x2C, 2, true, new OnPacketReceive(DeathStatusResponse));
			Register(0x34, 10, true, new OnPacketReceive(MobileQuery));
			Register(0x3A, 0, true, new OnPacketReceive(ChangeSkillLock));
			Register(0x3B, 0, true, new OnPacketReceive(VendorBuyReply));
			Register(0x47, 11, true, new OnPacketReceive(NewTerrain));
			Register(0x48, 73, true, new OnPacketReceive(NewAnimData));
			Register(0x58, 106, true, new OnPacketReceive(NewRegion));
			Register(0x5D, 73, false, new OnPacketReceive(PlayCharacter));
			Register(0x61, 9, true, new OnPacketReceive(DeleteStatic));
			Register(0x6C, 19, true, new OnPacketReceive(TargetResponse));
			Register(0x6F, 0, true, new OnPacketReceive(SecureTrade));
			Register(0x72, 5, true, new OnPacketReceive(SetWarMode));
			Register(0x73, 2, false, new OnPacketReceive(PingReq));
			Register(0x75, 35, true, new OnPacketReceive(RenameRequest));
			Register(0x79, 9, true, new OnPacketReceive(ResourceQuery));
			Register(0x7E, 2, true, new OnPacketReceive(GodviewQuery));
			Register(0x7D, 13, true, new OnPacketReceive(MenuResponse));
			Register(0x80, 62, false, new OnPacketReceive(AccountLogin));
			Register(0x83, 39, false, new OnPacketReceive(DeleteCharacter));
			Register(0x91, 65, false, new OnPacketReceive(GameLogin));
			Register(0x95, 9, true, new OnPacketReceive(HuePickerResponse));
			Register(0x96, 0, true, new OnPacketReceive(GameCentralMoniter));
			Register(0x98, 0, true, new OnPacketReceive(MobileNameRequest));
			Register(0x9A, 0, true, new OnPacketReceive(AsciiPromptResponse));
			Register(0x9B, 258, true, new OnPacketReceive(HelpRequest));
			Register(0x9D, 51, true, new OnPacketReceive(GMSingle));
			Register(0x9F, 0, true, new OnPacketReceive(VendorSellReply));
			Register(0xA0, 3, false, new OnPacketReceive(PlayServer));
			Register(0xA4, 149, false, new OnPacketReceive(SystemInfo));
			Register(0xA7, 4, true, new OnPacketReceive(RequestScrollWindow));
			Register(0xAD, 0, true, new OnPacketReceive(UnicodeSpeech));
			Register(0xB1, 0, true, new OnPacketReceive(DisplayGumpResponse));
			Register(0xB6, 9, true, new OnPacketReceive(ObjectHelpRequest));
			Register(0xB8, 0, true, new OnPacketReceive(ProfileReq));
			Register(0xBB, 9, false, new OnPacketReceive(AccountID));
			Register(0xBD, 0, false, new OnPacketReceive(ClientVersion));
			Register(0xBE, 0, true, new OnPacketReceive(AssistVersion));
			Register(0xBF, 0, true, new OnPacketReceive(ExtendedCommand));
			Register(0xC2, 0, true, new OnPacketReceive(UnicodePromptResponse));
			Register(0xC8, 2, true, new OnPacketReceive(SetUpdateRange));
			Register(0xC9, 6, true, new OnPacketReceive(TripTime));
			Register(0xCA, 6, true, new OnPacketReceive(UTripTime));
			Register(0xCF, 0, false, new OnPacketReceive(AccountLogin));
			Register(0xD0, 0, true, new OnPacketReceive(ConfigurationFile));
			Register(0xD1, 2, true, new OnPacketReceive(LogoutReq));
			Register(0xD6, 0, true, new OnPacketReceive(BatchQueryProperties));
			Register(0xD7, 0, true, new OnPacketReceive(EncodedCommand));
			Register(0xE1, 0, false, new OnPacketReceive(ClientType));
			Register(0xEF, 21, false, new OnPacketReceive(LoginServerSeed));
			Register(0xF4, 0, false, new OnPacketReceive(CrashReport));
			Register(0xF8, 106, false, new OnPacketReceive(CreateCharacter70160));

			Register6017(0x08, 15, true, new OnPacketReceive(DropReq6017));

			RegisterExtended(0x05, false, new OnPacketReceive(ScreenSize));
			RegisterExtended(0x06, true, new OnPacketReceive(PartyMessage));
			RegisterExtended(0x07, true, new OnPacketReceive(QuestArrow));
			RegisterExtended(0x09, true, new OnPacketReceive(DisarmRequest));
			RegisterExtended(0x0A, true, new OnPacketReceive(StunRequest));
			RegisterExtended(0x0B, false, new OnPacketReceive(Language));
			RegisterExtended(0x0C, true, new OnPacketReceive(CloseStatus));
			RegisterExtended(0x0E, true, new OnPacketReceive(Animate));
			RegisterExtended(0x0F, false, new OnPacketReceive(Empty)); // What's this?
			RegisterExtended(0x10, true, new OnPacketReceive(QueryProperties));
			RegisterExtended(0x13, true, new OnPacketReceive(ContextMenuRequest));
			RegisterExtended(0x15, true, new OnPacketReceive(ContextMenuResponse));
			RegisterExtended(0x1A, true, new OnPacketReceive(StatLockChange));
			RegisterExtended(0x1C, true, new OnPacketReceive(CastSpell));
			RegisterExtended(0x24, false, new OnPacketReceive(UnhandledBF));
			RegisterExtended(0x2C, true, new OnPacketReceive(BandageTarget));
			RegisterExtended(0x32, true, new OnPacketReceive(ToggleFlying));

			RegisterEncoded(0x19, true, new OnEncodedPacketReceive(SetAbility));
			RegisterEncoded(0x28, true, new OnEncodedPacketReceive(GuildGumpRequest));

			RegisterEncoded(0x32, true, new OnEncodedPacketReceive(QuestGumpRequest));
		}

		public static void Register(int packetID, int length, bool ingame, OnPacketReceive onReceive)
		{
			Handlers[packetID] = new PacketHandler(packetID, length, ingame, onReceive);

			if (m_6017Handlers[packetID] == null)
				m_6017Handlers[packetID] = new PacketHandler(packetID, length, ingame, onReceive);
		}

		public static PacketHandler GetHandler(int packetID)
		{
			return Handlers[packetID];
		}

		public static void Register6017(int packetID, int length, bool ingame, OnPacketReceive onReceive)
		{
			m_6017Handlers[packetID] = new PacketHandler(packetID, length, ingame, onReceive);
		}

		public static PacketHandler Get6017Handler(int packetID)
		{
			return m_6017Handlers[packetID];
		}

		public static void RegisterExtended(int packetID, bool ingame, OnPacketReceive onReceive)
		{
			if (packetID >= 0 && packetID < 0x100)
				m_ExtendedHandlersLow[packetID] = new PacketHandler(packetID, 0, ingame, onReceive);
			else
				m_ExtendedHandlersHigh[packetID] = new PacketHandler(packetID, 0, ingame, onReceive);
		}

		public static PacketHandler GetExtendedHandler(int packetID)
		{
			if (packetID >= 0 && packetID < 0x100)
				return m_ExtendedHandlersLow[packetID];
			else
			{
				m_ExtendedHandlersHigh.TryGetValue(packetID, out PacketHandler handler);
				return handler;
			}
		}

		public static void RemoveExtendedHandler(int packetID)
		{
			if (packetID >= 0 && packetID < 0x100)
				m_ExtendedHandlersLow[packetID] = null;
			else
				m_ExtendedHandlersHigh.Remove(packetID);
		}

		public static void RegisterEncoded(int packetID, bool ingame, OnEncodedPacketReceive onReceive)
		{
			if (packetID >= 0 && packetID < 0x100)
				m_EncodedHandlersLow[packetID] = new EncodedPacketHandler(packetID, ingame, onReceive);
			else
				m_EncodedHandlersHigh[packetID] = new EncodedPacketHandler(packetID, ingame, onReceive);
		}

		public static EncodedPacketHandler GetEncodedHandler(int packetID)
		{
			if (packetID >= 0 && packetID < 0x100)
				return m_EncodedHandlersLow[packetID];
			else
			{
				m_EncodedHandlersHigh.TryGetValue(packetID, out EncodedPacketHandler handler);
				return handler;
			}
		}

		public static void RemoveEncodedHandler(int packetID)
		{
			if (packetID >= 0 && packetID < 0x100)
				m_EncodedHandlersLow[packetID] = null;
			else
				m_EncodedHandlersHigh.Remove(packetID);
		}

		public static void RegisterThrottler(int packetID, ThrottlePacketCallback t)
		{
			PacketHandler ph = GetHandler(packetID);

			if (ph != null)
				ph.ThrottleCallback = t;

			ph = Get6017Handler(packetID);

			if (ph != null)
				ph.ThrottleCallback = t;
		}

		private static void UnhandledBF(NetState state, PacketReader pvSrc)
		{
		}

		public static void Empty(NetState state, PacketReader pvSrc)
		{
		}

		public static void SetAbility(NetState state, IEntity e, EncodedReader reader)
		{
			EventSink.InvokeSetAbility(state.Mobile, reader.ReadInt32());
		}

		public static void GuildGumpRequest(NetState state, IEntity e, EncodedReader reader)
		{
			EventSink.InvokeGuildGumpRequest(state.Mobile);
		}

		public static void QuestGumpRequest(NetState state, IEntity e, EncodedReader reader)
		{
			EventSink.InvokeQuestGumpRequest(state.Mobile);
		}

		public static void EncodedCommand(NetState state, PacketReader pvSrc)
		{
			IEntity e = pvSrc.ReadEntity();
			int packetID = pvSrc.ReadUInt16();

			EncodedPacketHandler ph = GetEncodedHandler(packetID);

			if (ph != null)
			{
				if (ph.Ingame && state.Mobile == null)
				{
					Console.WriteLine("Client: {0}: Sent ingame packet (0xD7x{1:X2}) before having been attached to a mobile", state, packetID);
					state.Dispose();
				}
				else if (ph.Ingame && state.Mobile.Deleted)
				{
					state.Dispose();
				}
				else
				{
					ph.OnReceive(state, e, new EncodedReader(pvSrc));
				}
			}
			else
			{
				pvSrc.Trace(state);
			}
		}

		public static void RenameRequest(NetState state, PacketReader pvSrc)
		{
			Mobile from = state.Mobile;
			Mobile targ = pvSrc.ReadMobile();

			if (targ != null)
				EventSink.InvokeRenameRequest(from, targ, pvSrc.ReadStringSafe());
		}

		public static void ChatRequest(NetState state, PacketReader pvSrc)
		{
			//EventSink.InvokeChatRequest(new ChatRequestEventArgs(state.Mobile));
		}

		public static void SecureTrade(NetState state, PacketReader pvSrc)
		{
			switch (pvSrc.ReadByte())
			{
				case 1: // Cancel
					{
						var serial = pvSrc.ReadSerial();

						if (World.FindItem(serial) is SecureTradeContainer cont && cont.Trade != null && (cont.Trade.From.Mobile == state.Mobile || cont.Trade.To.Mobile == state.Mobile))
							cont.Trade.Cancel();

						break;
					}
				case 2: // Check
					{
						var serial = pvSrc.ReadSerial();

						if (World.FindItem(serial) is SecureTradeContainer cont)
						{
							SecureTrade trade = cont.Trade;

							bool value = (pvSrc.ReadInt32() != 0);

							if (trade != null && trade.From.Mobile == state.Mobile)
							{
								trade.From.Accepted = value;
								trade.Update();
							}
							else if (trade != null && trade.To.Mobile == state.Mobile)
							{
								trade.To.Accepted = value;
								trade.Update();
							}
						}

						break;
					}
				case 3: // Update Gold
					{
						var serial = pvSrc.ReadSerial();

						if (World.FindItem(serial) is SecureTradeContainer cont)
						{
							int gold = pvSrc.ReadInt32();
							int plat = pvSrc.ReadInt32();

							SecureTrade trade = cont.Trade;

							if (trade != null)
							{
								if (trade.From.Mobile == state.Mobile)
								{
									trade.From.Gold = gold;
									trade.From.Plat = plat;
									trade.UpdateFromCurrency();
								}
								else if (trade.To.Mobile == state.Mobile)
								{
									trade.To.Gold = gold;
									trade.To.Plat = plat;
									trade.UpdateToCurrency();
								}
							}
						}
					}
					break;
			}
		}

		public static void VendorBuyReply(NetState state, PacketReader pvSrc)
		{
			pvSrc.Seek(1, SeekOrigin.Begin);

			int msgSize = pvSrc.ReadUInt16();
			Mobile vendor = pvSrc.ReadMobile();
			byte flag = pvSrc.ReadByte();

			if (vendor == null)
			{
				return;
			}
			else if (vendor.Deleted || !Utility.RangeCheck(vendor.Location, state.Mobile.Location, 10))
			{
				state.Send(new EndVendorBuy(vendor));
				return;
			}

			if (flag == 0x02)
			{
				msgSize -= 1 + 2 + 4 + 1;

				if ((msgSize / 7) > 100)
					return;

				List<BuyItemResponse> buyList = new(msgSize / 7);
				for (; msgSize > 0; msgSize -= 7)
				{
					_ = pvSrc.ReadByte();
					Serial serial = pvSrc.ReadSerial();
					int amount = pvSrc.ReadInt16();

					buyList.Add(new BuyItemResponse(serial, amount));
				}

				if (buyList.Count > 0)
				{
					if (vendor is IVendor v && v.OnBuyItems(state.Mobile, buyList))
						state.Send(new EndVendorBuy(vendor));
				}
			}
			else
			{
				state.Send(new EndVendorBuy(vendor));
			}
		}

		public static void VendorSellReply(NetState state, PacketReader pvSrc)
		{
			Serial serial = pvSrc.ReadSerial();
			Mobile vendor = World.FindMobile(serial);

			if (vendor == null)
			{
				return;
			}
			else if (vendor.Deleted || !Utility.RangeCheck(vendor.Location, state.Mobile.Location, 10))
			{
				state.Send(new EndVendorSell(vendor));
				return;
			}

			int count = pvSrc.ReadUInt16();
			if (count < 100 && pvSrc.Size == (1 + 2 + 4 + 2 + (count * 6)))
			{
				List<SellItemResponse> sellList = new(count);

				for (int i = 0; i < count; i++)
				{
					Item item = pvSrc.ReadItem();
					int Amount = pvSrc.ReadInt16();

					if (item != null && Amount > 0)
						sellList.Add(new SellItemResponse(item, Amount));
				}

				if (sellList.Count > 0)
				{
					if (vendor is IVendor v && v.OnSellItems(state.Mobile, sellList))
						state.Send(new EndVendorSell(vendor));
				}
			}
		}

		public static void DeleteCharacter(NetState state, PacketReader pvSrc)
		{
			pvSrc.Seek(30, SeekOrigin.Current);
			int index = pvSrc.ReadInt32();

			EventSink.InvokeDeleteRequest(state, index);
		}

		public static void ResourceQuery(NetState state, PacketReader pvSrc)
		{
			if (VerifyGC(state))
			{
			}
		}

		public static void GameCentralMoniter(NetState state, PacketReader pvSrc)
		{
			if (VerifyGC(state))
			{
				int type = pvSrc.ReadByte();
				int num1 = pvSrc.ReadInt32();

				Console.WriteLine("God Client: {0}: Game central moniter", state);
				Console.WriteLine(" - Type: {0}", type);
				Console.WriteLine(" - Number: {0}", num1);

				pvSrc.Trace(state);
			}
		}

		public static void GodviewQuery(NetState state, PacketReader pvSrc)
		{
			if (VerifyGC(state))
			{
				Console.WriteLine("God Client: {0}: Godview query 0x{1:X}", state, pvSrc.ReadByte());
			}
		}

		public static void GMSingle(NetState state, PacketReader pvSrc)
		{
			if (VerifyGC(state))
				pvSrc.Trace(state);
		}

		public static void DeathStatusResponse(NetState state, PacketReader pvSrc)
		{
			// Ignored
		}

		public static void ObjectHelpRequest(NetState state, PacketReader pvSrc)
		{
			Mobile from = state.Mobile;

			Serial serial = pvSrc.ReadSerial();
			int unk = pvSrc.ReadByte();
			string lang = pvSrc.ReadString(3);

			if (serial.IsItem)
			{
				Item item = World.FindItem(serial);

				if (item != null && from.Map == item.Map && from.InUpdateRange(item.GetWorldLocation(), from.Location) && from.CanSee(item))
					item.OnHelpRequest(from);
			}
			else if (serial.IsMobile)
			{
				Mobile m = World.FindMobile(serial);

				if (m != null && from.Map == m.Map && from.InUpdateRange(m.Location, from.Location) && from.CanSee(m))
					m.OnHelpRequest(m);
			}
		}

		public static void MobileNameRequest(NetState state, PacketReader pvSrc)
		{
			var m = pvSrc.ReadMobile();

			if (m != null && state.Mobile.InUpdateRange(m) && state.Mobile.CanSee(m))
			{
				state.Send(new MobileName(m));
			}
		}

		public static void RequestScrollWindow(NetState state, PacketReader pvSrc)
		{
			int lastTip = pvSrc.ReadInt16();
			int type = pvSrc.ReadByte();
		}

		public static void AttackReq(NetState state, PacketReader pvSrc)
		{
			Mobile from = state.Mobile;
			Mobile m = pvSrc.ReadMobile();

			if (m != null)
				from.Attack(m);
		}

		public static void HuePickerResponse(NetState state, PacketReader pvSrc)
		{
			int serial = pvSrc.ReadInt32();
			int value = pvSrc.ReadInt16();
			int hue = pvSrc.ReadInt16() & 0x3FFF;

			hue = Utility.ClipDyedHue(hue);

			foreach (HuePicker huePicker in state.HuePickers)
			{
				if (huePicker.Serial == serial)
				{
					state.RemoveHuePicker(huePicker);

					huePicker.OnResponse(hue);

					break;
				}
			}
		}

		public static void TripTime(NetState state, PacketReader pvSrc)
		{
			int unk1 = pvSrc.ReadByte();
			int unk2 = pvSrc.ReadInt32();

			state.Send(new TripTimeResponse(unk1));
		}

		public static void UTripTime(NetState state, PacketReader pvSrc)
		{
			int unk1 = pvSrc.ReadByte();
			int unk2 = pvSrc.ReadInt32();

			state.Send(new UTripTimeResponse(unk1));
		}

		public static void ChangeZ(NetState state, PacketReader pvSrc)
		{
			if (VerifyGC(state))
			{
				int x = pvSrc.ReadInt16();
				int y = pvSrc.ReadInt16();
				int z = pvSrc.ReadSByte();

				Console.WriteLine("God Client: {0}: Change Z ({1}, {2}, {3})", state, x, y, z);
			}
		}

		public static void SystemInfo(NetState state, PacketReader pvSrc)
		{
			int v1 = pvSrc.ReadByte();
			int v2 = pvSrc.ReadUInt16();
			int v3 = pvSrc.ReadByte();
			string s1 = pvSrc.ReadString(32);
			string s2 = pvSrc.ReadString(32);
			string s3 = pvSrc.ReadString(32);
			string s4 = pvSrc.ReadString(32);
			int v4 = pvSrc.ReadUInt16();
			int v5 = pvSrc.ReadUInt16();
			int v6 = pvSrc.ReadInt32();
			int v7 = pvSrc.ReadInt32();
			int v8 = pvSrc.ReadInt32();
		}

		public static void Edit(NetState state, PacketReader pvSrc)
		{
			if (VerifyGC(state))
			{
				int type = pvSrc.ReadByte(); // 10 = static, 7 = npc, 4 = dynamic
				int x = pvSrc.ReadInt16();
				int y = pvSrc.ReadInt16();
				int id = pvSrc.ReadInt16();
				int z = pvSrc.ReadSByte();
				int hue = pvSrc.ReadUInt16();

				Console.WriteLine("God Client: {0}: Edit {6} ({1}, {2}, {3}) 0x{4:X} (0x{5:X})", state, x, y, z, id, hue, type);
			}
		}

		public static void DeleteStatic(NetState state, PacketReader pvSrc)
		{
			if (VerifyGC(state))
			{
				int x = pvSrc.ReadInt16();
				int y = pvSrc.ReadInt16();
				int z = pvSrc.ReadInt16();
				int id = pvSrc.ReadUInt16();

				Console.WriteLine("God Client: {0}: Delete Static ({1}, {2}, {3}) 0x{4:X}", state, x, y, z, id);
			}
		}

		public static void NewAnimData(NetState state, PacketReader pvSrc)
		{
			if (VerifyGC(state))
			{
				Console.WriteLine("God Client: {0}: New tile animation", state);

				pvSrc.Trace(state);
			}
		}

		public static void NewTerrain(NetState state, PacketReader pvSrc)
		{
			if (VerifyGC(state))
			{
				int x = pvSrc.ReadInt16();
				int y = pvSrc.ReadInt16();
				int id = pvSrc.ReadUInt16();
				int width = pvSrc.ReadInt16();
				int height = pvSrc.ReadInt16();

				Console.WriteLine("God Client: {0}: New Terrain ({1}, {2})+({3}, {4}) 0x{5:X4}", state, x, y, width, height, id);
			}
		}

		public static void NewRegion(NetState state, PacketReader pvSrc)
		{
			if (VerifyGC(state))
			{
				string name = pvSrc.ReadString(40);
				int unk = pvSrc.ReadInt32();
				int x = pvSrc.ReadInt16();
				int y = pvSrc.ReadInt16();
				int width = pvSrc.ReadInt16();
				int height = pvSrc.ReadInt16();
				int zStart = pvSrc.ReadInt16();
				int zEnd = pvSrc.ReadInt16();
				string desc = pvSrc.ReadString(40);
				int soundFX = pvSrc.ReadInt16();
				int music = pvSrc.ReadInt16();
				int nightFX = pvSrc.ReadInt16();
				int dungeon = pvSrc.ReadByte();
				int light = pvSrc.ReadInt16();

				Console.WriteLine("God Client: {0}: New Region '{1}' ('{2}')", state, name, desc);
			}
		}

		public static void AccountID(NetState state, PacketReader pvSrc)
		{
		}

		public static bool VerifyGC(NetState state)
		{
			if (state.Mobile == null || state.Mobile.AccessLevel <= AccessLevel.Counselor)
			{
				if (state.Running)
					Console.WriteLine("Warning: {0}: Player using godclient, disconnecting", state);

				state.Dispose();
				return false;
			}
			else
			{
				return true;
			}
		}

		public static void TextCommand(NetState state, PacketReader pvSrc)
		{
			int type = pvSrc.ReadByte();
			string command = pvSrc.ReadString();

			Mobile m = state.Mobile;

			switch (type)
			{
				case 0x00: // Go
					{
						if (VerifyGC(state))
						{
							try
							{
								string[] split = command.Split(' ');

								int x = Utility.ToInt32(split[0]);
								int y = Utility.ToInt32(split[1]);

								int z;

								if (split.Length >= 3)
									z = Utility.ToInt32(split[2]);
								else if (m.Map != null)
									z = m.Map.GetAverageZ(x, y);
								else
									z = 0;

								m.Location = new Point3D(x, y, z);
							}
							catch
							{
							}
						}

						break;
					}
				case 0xC7: // Animate
					{
						EventSink.InvokeAnimateRequest(m, command);

						break;
					}
				case 0x24: // Use skill
					{
						if (!int.TryParse(command.Split(' ')[0], out int skillIndex))
							break;

						Skills.UseSkill(m, skillIndex);

						break;
					}
				case 0x43: // Open spellbook
					{
						if (!int.TryParse(command, out int booktype))
							booktype = 1;

						EventSink.InvokeOpenSpellbookRequest(m, booktype);

						break;
					}
				case 0x27: // Cast spell from book
					{
						string[] split = command.Split(' ');

						if (split.Length > 0)
						{
							int spellID = Utility.ToInt32(split[0]) - 1;
							var serial = split.Length > 1 ? Utility.ToSerial(split[1]) : Serial.MinusOne;

							EventSink.InvokeCastSpellRequest(m, spellID, World.FindEntity(serial) as ISpellbook);
						}

						break;
					}
				case 0x58: // Open door
					{
						EventSink.InvokeOpenDoorMacroUsed(m);

						break;
					}
				case 0x56: // Cast spell from macro
					{
						int spellID = Utility.ToInt32(command) - 1;

						EventSink.InvokeCastSpellRequest(m, spellID, null);

						break;
					}
				case 0xF4: // Invoke virtues from macro
					{
						int virtueID = Utility.ToInt32(command) - 1;

						EventSink.InvokeVirtueMacroRequest(m, virtueID);

						break;
					}
				case 0x2F: // Old scroll double click
					{
						/*
						 * This command is still sent for items 0xEF3 - 0xEF9
						 *
						 * Command is one of three, depending on the item ID of the scroll:
						 * - [scroll serial]
						 * - [scroll serial] [target serial]
						 * - [scroll serial] [x] [y] [z]
						 */
						break;
					}
				default:
					{
						Console.WriteLine("Client: {0}: Unknown text-command type 0x{1:X2}: {2}", state, type, command);
						break;
					}
			}
		}

		public static void GodModeRequest(NetState state, PacketReader pvSrc)
		{
			if (VerifyGC(state))
			{
				state.Send(new GodModeReply(pvSrc.ReadBoolean()));
			}
		}

		public static void AsciiPromptResponse(NetState state, PacketReader pvSrc)
		{
			int serial = pvSrc.ReadInt32();
			int prompt = pvSrc.ReadInt32();
			int type = pvSrc.ReadInt32();
			string text = pvSrc.ReadStringSafe();

			if (text == null || text.Length > 128)
			{
				return;
			}

			Mobile from = state.Mobile;
			Prompt p = from.Prompt;

			if (from != null && p != null && p.Sender.Serial == serial && p.TypeId == prompt)
			{
				from.Prompt = null;

				if (type == 0)
				{
					p.OnCancel(from);
				}
				else
				{
					p.OnResponse(from, text);
				}
			}
		}

		public static void UnicodePromptResponse(NetState state, PacketReader pvSrc)
		{
			int serial = pvSrc.ReadInt32();
			int prompt = pvSrc.ReadInt32();
			int type = pvSrc.ReadInt32();
			string lang = pvSrc.ReadString(4);
			string text = pvSrc.ReadUnicodeStringLESafe();

			if (text.Length > 128)
			{
				return;
			}

			Mobile from = state.Mobile;
			Prompt p = from.Prompt;

			int promptSerial = (p != null && p.Sender != null) ? p.Sender.Serial.Value : from.Serial.Value;

			if (p != null && promptSerial == serial && p.TypeId == prompt)
			{
				from.Prompt = null;

				if (type == 0)
				{
					p.OnCancel(from);
				}
				else
				{
					p.OnResponse(from, text);
				}
			}
		}

		public static void MenuResponse(NetState state, PacketReader pvSrc)
		{
			int serial = pvSrc.ReadInt32();
			int menuID = pvSrc.ReadInt16(); // unused in our implementation
			int index = pvSrc.ReadInt16();
			int itemID = pvSrc.ReadInt16();
			int hue = pvSrc.ReadInt16();

			index -= 1; // convert from 1-based to 0-based

			foreach (IMenu menu in state.Menus)
			{
				if (menu.Serial == serial)
				{
					state.RemoveMenu(menu);

					if (index >= 0 && index < menu.EntryLength)
					{
						menu.OnResponse(state, index);
					}
					else
					{
						menu.OnCancel(state);
					}

					break;
				}
			}
		}

		public static void ProfileReq(NetState state, PacketReader pvSrc)
		{
			int type = pvSrc.ReadByte();
			Serial serial = pvSrc.ReadSerial();

			Mobile beholder = state.Mobile;
			Mobile beheld = World.FindMobile(serial);

			if (beheld == null)
				return;

			switch (type)
			{
				case 0x00: // display request
					{
						EventSink.InvokeProfileRequest(beholder, beheld);

						break;
					}
				case 0x01: // edit request
					{
						pvSrc.ReadInt16(); // Skip
						int length = pvSrc.ReadUInt16();

						if (length > 511)
							return;

						string text = pvSrc.ReadUnicodeString(length);

						EventSink.InvokeChangeProfileRequest(beholder, beheld, text);

						break;
					}
			}
		}

		public static void Disconnect(NetState state, PacketReader pvSrc)
		{
			int minusOne = pvSrc.ReadInt32();
		}

		public static void LiftReq(NetState state, PacketReader pvSrc)
		{
			Serial serial = pvSrc.ReadSerial();
			int amount = pvSrc.ReadUInt16();
			Item item = World.FindItem(serial);

			state.Mobile.Lift(item, amount, out bool rejected, out LRReason reject);
		}

		public static void EquipReq(NetState state, PacketReader pvSrc)
		{
			Mobile from = state.Mobile;
			Item item = from.Holding;

			bool valid = (item != null && item.HeldBy == from && item.Map == Map.Internal);

			from.Holding = null;

			if (!valid)
			{
				return;
			}

			pvSrc.Seek(5, SeekOrigin.Current);
			var to = pvSrc.ReadMobile() ?? from;

			if (!to.AllowEquipFrom(from) || !to.EquipItem(item))
				item.Bounce(from);

			item.ClearBounce();
		}

		public static void DropReq(NetState state, PacketReader pvSrc)
		{
			var serial = pvSrc.ReadSerial();
			int x = pvSrc.ReadInt16();
			int y = pvSrc.ReadInt16();
			int z = pvSrc.ReadSByte();

			var gridloc = byte.MaxValue;

			if (state.ContainerGridLines)
			{
				gridloc = pvSrc.ReadByte();
			}

			var dropped = World.FindItem(serial);

			if (dropped != null)
			{
				dropped.GridLocation = gridloc;
			}

			var dest = pvSrc.ReadSerial();

			if (!state.ContainerGridLines)
			{
				pvSrc.Slice(); // push remaining data back to the buffer
			}

			var loc = new Point3D(x, y, z);
			var from = state.Mobile;

			if (dest.IsMobile)
			{
				from.Drop(World.FindMobile(dest), loc);
			}
			else if (dest.IsItem)
			{
				var item = World.FindItem(dest);

				if (item is BaseMulti multi && multi.AllowsRelativeDrop)
				{
					loc.m_X += item.X;
					loc.m_Y += item.Y;
					from.Drop(loc);
				}
				else
				{
					from.Drop(item, loc);
				}
			}
			else
			{
				from.Drop(loc);
			}
		}

		public static void DropReq6017(NetState state, PacketReader pvSrc)
		{
			pvSrc.ReadInt32(); // serial, ignored
			int x = pvSrc.ReadInt16();
			int y = pvSrc.ReadInt16();
			int z = pvSrc.ReadSByte();
			pvSrc.ReadByte(); // Grid Location?
			Serial dest = pvSrc.ReadSerial();

			Point3D loc = new(x, y, z);

			Mobile from = state.Mobile;

			if (dest.IsMobile)
			{
				from.Drop(World.FindMobile(dest), loc);
			}
			else if (dest.IsItem)
			{
				Item item = World.FindItem(dest);

				if (item is BaseMulti && ((BaseMulti)item).AllowsRelativeDrop)
				{
					loc.m_X += item.X;
					loc.m_Y += item.Y;
					from.Drop(loc);
				}
				else
				{
					from.Drop(item, loc);
				}
			}
			else
			{
				from.Drop(loc);
			}
		}

		public static void ConfigurationFile(NetState state, PacketReader pvSrc)
		{
		}

		public static void LogoutReq(NetState state, PacketReader pvSrc)
		{
			state.Send(new LogoutAck());
		}

		public static void ChangeSkillLock(NetState state, PacketReader pvSrc)
		{
			Skill s = state.Mobile.Skills[pvSrc.ReadInt16()];

			if (s != null)
				s.SetLockNoRelay((SkillLock)pvSrc.ReadByte());
		}

		public static void HelpRequest(NetState state, PacketReader pvSrc)
		{
			EventSink.InvokeHelpRequest(state.Mobile);
		}

		public static void TargetResponse(NetState state, PacketReader pvSrc)
		{
			int type = pvSrc.ReadByte();
			var targetId = pvSrc.ReadInt32();
			int flags = pvSrc.ReadByte();
			var serial = pvSrc.ReadSerial();
			int x = pvSrc.ReadInt16();
			int y = pvSrc.ReadInt16();
			int z = pvSrc.ReadInt16();
			int graphic = pvSrc.ReadUInt16();

			if (targetId == unchecked((int)0xDEADBEEF))
			{
				return;
			}

			var from = state.Mobile;
			var t = from.Target;

			if (t != null)
			{
				var prof = TargetProfile.Acquire(t.GetType());

				prof?.Start();

				try
				{
					from.ClearTarget();

					if (x == -1 && y == -1 && !serial.IsValid)
					{
						t.Cancel(from, TargetCancelType.Canceled);
						return;
					}

					if (t.TargetId != targetId)
					{
						t.Cancel(from, TargetCancelType.Invalid);
						return;
					}

					object toTarget;

					if (type == 1)
					{
						if (graphic == 0)
						{
							toTarget = new LandTarget(new Point3D(x, y, z), from.Map);
						}
						else
						{
							var map = from.Map;

							if (map == null || map == Map.Internal)
							{
								t.Cancel(from, TargetCancelType.Invalid);
								return;
							}

							var tiles = map.Tiles.GetStaticTiles(x, y, !t.DisallowMultis);

							var valid = false;
							var hue = 0;

							if (state.HighSeas)
							{
								var id = TileData.ItemTable[graphic & TileData.MaxItemValue];

								if (id.Surface && !id.Flags.HasFlag(TileFlag.Roof))
								{
									z -= id.Height;
								}
							}

							for (var i = 0; !valid && i < tiles.Length; ++i)
							{
								if (tiles[i].Z == z && tiles[i].Id == graphic)
								{
									valid = true;
									hue = tiles[i].Hue;
								}
							}

							if (!valid)
							{
								t.Cancel(from, TargetCancelType.Invalid);
								return;
							}

							toTarget = new StaticTarget(new Point3D(x, y, z), graphic, hue);
						}
					}
					else if (serial.IsMobile)
					{
						toTarget = World.FindMobile(serial);
					}
					else if (serial.IsItem)
					{
						toTarget = World.FindItem(serial);
					}
					else
					{
						t.Cancel(from, TargetCancelType.Invalid);
						return;
					}

					t.Invoke(from, toTarget);
				}
				finally
				{
					prof?.Finish();
				}
			}
		}

		public static void DisplayGumpResponse(NetState state, PacketReader pvSrc)
		{
			var serial = pvSrc.ReadSerial();
			var typeID = pvSrc.ReadInt32();
			var buttonID = pvSrc.ReadInt32();

			var index = state.Gumps.Count;

			Gump gump;

			while (--index >= 0)
			{
				if (index >= state.Gumps.Count)
				{
					continue;
				}

				gump = state.Gumps[index];

				if (gump == null)
				{
					state.Gumps.RemoveAt(index);
					continue;
				}

				if (gump.Serial != serial || gump.TypeId != typeID)
				{
					continue;
				}

				var buttonExists = buttonID == 0; // 0 is always 'close'

				if (!buttonExists)
				{
					foreach (var e in gump.Entries)
					{
						if (e is GumpButton && ((GumpButton)e).ButtonId == buttonID)
						{
							buttonExists = true;
							break;
						}

						if (e is GumpImageTileButton && ((GumpImageTileButton)e).ButtonID == buttonID)
						{
							buttonExists = true;
							break;
						}
					}
				}

				if (!buttonExists)
				{
					Utility.WriteConsole(ConsoleColor.Red, $"Client: {state}: Invalid gump button response for '{gump.GetType()}'");

					state.RemoveGump(gump);
					gump.OnServerClose(state);

					return;
				}

				var switchCount = pvSrc.ReadInt32();

				if (switchCount < 0 || switchCount > gump.MSwitches)
				{
					Utility.WriteConsole(ConsoleColor.Red, $"Client: {state}: Invalid gump switch response for '{gump.GetType()}'");

					state.RemoveGump(gump);
					gump.OnServerClose(state);

					return;
				}

				var switches = new int[switchCount];

				for (var j = 0; j < switches.Length; ++j)
				{
					switches[j] = pvSrc.ReadInt32();
				}

				var textCount = pvSrc.ReadInt32();

				if (textCount < 0 || textCount > gump.MTextEntries)
				{
					Utility.WriteConsole(ConsoleColor.Red, $"Client: {state}: Invalid gump text response for '{gump.GetType()}'");

					state.RemoveGump(gump);
					gump.OnServerClose(state);

					return;
				}

				var textEntries = new TextRelay[textCount];

				for (var j = 0; j < textEntries.Length; ++j)
				{
					int entryID = pvSrc.ReadUInt16();
					int textLength = pvSrc.ReadUInt16();

					if (textLength > 239)
					{
						Utility.WriteConsole(ConsoleColor.Red, $"Client: {state}: Invalid gump text response for '{gump.GetType()}' entry '{entryID}'");

						state.RemoveGump(gump);
						gump.OnServerClose(state);

						return;
					}

					var text = pvSrc.ReadUnicodeStringSafe(textLength);

					textEntries[j] = new TextRelay(entryID, text);
				}

				state.RemoveGump(gump);

				var prof = GumpProfile.Acquire(gump.GetType());

				if (prof != null)
				{
					prof.Start();
				}

				gump.OnResponse(state, new RelayInfo(buttonID, switches, textEntries));

				if (prof != null)
				{
					prof.Finish();
				}

				return;
			}

			if (typeID == 461)
			{ // Virtue gump
				int switchCount = pvSrc.ReadInt32();

				if (buttonID == 1 && switchCount > 0)
				{
					Mobile beheld = pvSrc.ReadMobile();

					if (beheld != null)
					{
						EventSink.InvokeVirtueGumpRequest(state.Mobile, beheld);
					}
				}
				else
				{
					Mobile beheld = World.FindMobile(serial);

					if (beheld != null)
					{
						EventSink.InvokeVirtueItemRequest(state.Mobile, beheld, buttonID);
					}
				}
			}
		}

		public static void SetWarMode(NetState state, PacketReader pvSrc)
		{
			state.Mobile.DelayChangeWarmode(pvSrc.ReadBoolean());
		}

		/*public static void Resynchronize(NetState state, PacketReader pvSrc)
		{
			Mobile m = state.Mobile;

			if (state.StygianAbyss)
			{
				state.Send(new MobileUpdate(m));
			}
			else
			{
				state.Send(new MobileUpdateOld(m));
			}

			state.Send(MobileIncoming.Create(state, m, m));

			m.SendEverything();

			state.Sequence = 0;

			m.ClearFastwalkStack();
		}*/

		public static void Resynchronize(NetState state, PacketReader pvSrc)
		{
			state.Mobile?.SendMapUpdates(false, false);
		}

		private static readonly int[] m_EmptyInts = Array.Empty<int>();

		public static void AsciiSpeech(NetState state, PacketReader pvSrc)
		{
			Mobile from = state.Mobile;

			MessageType type = (MessageType)pvSrc.ReadByte();
			int hue = pvSrc.ReadInt16();
			pvSrc.ReadInt16(); // font
			string text = pvSrc.ReadStringSafe().Trim();

			if (text.Length <= 0 || text.Length > 128)
				return;

			if (!Enum.IsDefined(typeof(MessageType), type))
				type = MessageType.Regular;

			from.DoSpeech(text, m_EmptyInts, type, Utility.ClipDyedHue(hue));
		}

		private static readonly KeywordList m_KeywordList = new();

		public static void UnicodeSpeech(NetState state, PacketReader pvSrc)
		{
			Mobile from = state.Mobile;

			MessageType type = (MessageType)pvSrc.ReadByte();
			int hue = pvSrc.ReadInt16();
			pvSrc.ReadInt16(); // font
			string lang = pvSrc.ReadString(4);
			string text;

			bool isEncoded = (type & MessageType.Encoded) != 0;
			int[] keywords;

			if (isEncoded)
			{
				int value = pvSrc.ReadInt16();
				int count = (value & 0xFFF0) >> 4;
				int hold = value & 0xF;

				if (count < 0 || count > 50)
					return;

				KeywordList keyList = m_KeywordList;

				for (int i = 0; i < count; ++i)
				{
					int speechID;

					if ((i & 1) == 0)
					{
						hold <<= 8;
						hold |= pvSrc.ReadByte();
						speechID = hold;
						hold = 0;
					}
					else
					{
						value = pvSrc.ReadInt16();
						speechID = (value & 0xFFF0) >> 4;
						hold = value & 0xF;
					}

					if (!keyList.Contains(speechID))
						keyList.Add(speechID);
				}

				text = pvSrc.ReadUTF8StringSafe();

				keywords = keyList.ToArray();
			}
			else
			{
				text = pvSrc.ReadUnicodeStringSafe();

				keywords = m_EmptyInts;
			}

			text = text.Trim();

			if (text.Length <= 0 || text.Length > 128)
				return;

			type &= ~MessageType.Encoded;

			if (!Enum.IsDefined(typeof(MessageType), type))
				type = MessageType.Regular;

			from.Language = lang;
			from.DoSpeech(text, keywords, type, Utility.ClipDyedHue(hue));
		}

		public static void UseReq(NetState state, PacketReader pvSrc)
		{
			Mobile from = state.Mobile;

			if (from.IsStaff() || Core.TickCount - from.NextActionTime >= 0)
			{
				var value = pvSrc.ReadSerial();

				if ((value & ~0x7FFFFFFF) != 0)
				{
					from.OnPaperdollRequest();
				}
				else
				{
					if (value.IsMobile)
					{
						var m = World.FindMobile(value);

						if (m != null && !m.Deleted)
						{
							from.Use(m);
						}
					}
					else if (value.IsItem)
					{
						var item = World.FindItem(value);

						if (item != null && !item.Deleted)
						{
							from.Use(item);
						}
					}
				}

				from.NextActionTime = Core.TickCount + Mobile.ActionDelay;
			}
			else
			{
				from.SendActionMessage();
			}
		}

		public static bool SingleClickProps { get; set; }
		public static Func<Mobile, Mobile, bool> MobileClickOverride;
		public static Func<Mobile, Item, bool> ItemClickOverride;

		private static void HandleSingleClick(Mobile m, IEntity target)
		{
			if (m == null || target == null || target.Deleted || !m.CanSee(target))
			{
				return;
			}

			if (target is Item ti)
			{
				if (m.InUpdateRange(ti))
				{
					if (ItemClickOverride == null || !ItemClickOverride(m, ti))
					{
						if (ObjectPropertyList.Enabled && m.ViewOpl)
						{
							ti.OnAosSingleClick(m);
						}
						else if (m.Region.OnSingleClick(m, ti))
						{
							if (ti.Parent is Item tip)
							{
								tip.OnSingleClickContained(m, ti);
							}

							ti.OnSingleClick(m);
						}
					}
				}
			}
			else if (target is Mobile tm)
			{
				if (m.InUpdateRange(tm))
				{
					if (MobileClickOverride == null || !MobileClickOverride(m, tm))
					{
						if (ObjectPropertyList.Enabled && m.ViewOpl)
						{
							tm.OnAosSingleClick(m);
						}
						else if (m.Region.OnSingleClick(m, tm))
						{
							tm.OnSingleClick(m);
						}
					}
				}
			}
		}

		public static void LookReq(NetState state, PacketReader pvSrc)
		{
			if (state.Mobile != null && state.Mobile.ViewOpl)
			{
				HandleSingleClick(state.Mobile, pvSrc.ReadEntity());
			}
		}

		public static void PingReq(NetState state, PacketReader pvSrc)
		{
			state.Send(PingAck.Instantiate(pvSrc.ReadByte()));
		}

		public static void SetUpdateRange(NetState state, PacketReader pvSrc)
		{
			//            min   max  default
			/* 640x480    5     18   15
             * 800x600    5     18   18
             * 1024x768   5     24   24
             * 1152x864   5     24   24 
             * 1280x720   5     24   24
             */

			int range = pvSrc.ReadByte();

			// Don't let range drop below the minimum standard.
			range = Math.Max(Map.GlobalUpdateRange, range);

			var old = state.UpdateRange;

			if (old == range)
			{
				return;
			}

			state.UpdateRange = range;

			ChangeUpdateRange.Send(state);

			if (state.Mobile != null)
			{
				state.Mobile.OnUpdateRangeChanged(old, state.UpdateRange);
			}
			//state.Send(ChangeUpdateRange.Instantiate(Map.GlobalUpdateRange));
		}

		private const int BadFood = unchecked((int)0xBAADF00D);
		private const int BadUOTD = unchecked((int)0xFFCEFFCE);

		public static void MovementReq(NetState state, PacketReader pvSrc)
		{
			Direction dir = (Direction)pvSrc.ReadByte();
			int seq = pvSrc.ReadByte();
			int key = pvSrc.ReadInt32();

			Mobile m = state.Mobile;

			if ((state.Sequence == 0 && seq != 0) || !m.Move(dir))
			{
				state.Send(new MovementRej(seq, m));
				state.Sequence = 0;

				m.ClearFastwalkStack();
			}
			else
			{
				++seq;

				if (seq == 256)
					seq = 1;

				state.Sequence = seq;
			}
		}

		public static void NewMovementReq(NetState state, PacketReader pvSrc)
		{
			/*byte count = pvSrc.ReadByte();

            while (--count >= 0)
            {
                long date1 = pvSrc.ReadUInt32();
                long date2 = pvSrc.ReadUInt32();

                byte seq = pvSrc.ReadByte();
                Direction dir = (Direction)pvSrc.ReadByte();

                int type = pvSrc.ReadUInt16();
                int x = pvSrc.ReadInt16();
                int y = pvSrc.ReadInt16();
                int z = pvSrc.ReadInt16();
            }*/
		}

		public static int[] m_ValidAnimations = new int[]
			{
				6, 21, 32, 33,
				100, 101, 102,
				103, 104, 105,
				106, 107, 108,
				109, 110, 111,
				112, 113, 114,
				115, 116, 117,
				118, 119, 120,
				121, 123, 124,
				125, 126, 127,
				128
			};

		public static int[] ValidAnimations { get => m_ValidAnimations; set => m_ValidAnimations = value; }

		public static void Animate(NetState state, PacketReader pvSrc)
		{
			Mobile from = state.Mobile;
			int action = pvSrc.ReadInt32();

			bool ok = false;

			for (int i = 0; !ok && i < m_ValidAnimations.Length; ++i)
				ok = (action == m_ValidAnimations[i]);

			if (from != null && ok && from.Alive && from.Body.IsHuman && !from.Mounted)
				from.Animate(action, 7, 1, true, false, 0);
		}

		public static void QuestArrow(NetState state, PacketReader pvSrc)
		{
			bool rightClick = pvSrc.ReadBoolean();
			Mobile from = state.Mobile;

			if (from != null && from.QuestArrow != null)
				from.QuestArrow.OnClick(rightClick);
		}

		public static void ExtendedCommand(NetState state, PacketReader pvSrc)
		{
			int packetID = pvSrc.ReadUInt16();

			PacketHandler ph = GetExtendedHandler(packetID);

			if (ph != null)
			{
				if (ph.Ingame && state.Mobile == null)
				{
					Console.WriteLine("Client: {0}: Sent ingame packet (0xBFx{1:X2}) before having been attached to a mobile", state, packetID);
					state.Dispose();
				}
				else if (ph.Ingame && state.Mobile.Deleted)
				{
					state.Dispose();
				}
				else
				{
					ph.OnReceive(state, pvSrc);
				}
			}
			else
			{
				pvSrc.Trace(state);
			}
		}

		public static void CastSpell(NetState state, PacketReader pvSrc)
		{
			Mobile from = state.Mobile;

			if (from == null)
				return;

			ISpellbook spellbook = null;

			if (pvSrc.ReadInt16() == 1)
			{
				spellbook = pvSrc.ReadEntity() as ISpellbook;
			}

			int spellID = pvSrc.ReadInt16() - 1;

			EventSink.InvokeCastSpellRequest(from, spellID, spellbook);
		}

		public static void BandageTarget(NetState state, PacketReader pvSrc)
		{
			Mobile from = state.Mobile;

			if (from == null)
				return;

			if (from.IsStaff() || Core.TickCount - from.NextActionTime >= 0)
			{
				var bandage = pvSrc.ReadItem();

				if (bandage == null)
					return;

				var target = pvSrc.ReadMobile();

				if (target == null)
					return;

				EventSink.InvokeBandageTargetRequest(from, bandage, target);

				from.NextActionTime = Core.TickCount + Mobile.ActionDelay;
			}
			else
			{
				from.SendActionMessage();
			}
		}

		public static void ToggleFlying(NetState state, PacketReader pvSrc)
		{
			state.Mobile.ToggleFlying();
		}
		public static void BatchQueryProperties(NetState state, PacketReader pvSrc)
		{
			if (!ObjectPropertyList.Enabled)
				return;

			Mobile from = state.Mobile;

			int length = pvSrc.Size - 3;

			if (length < 0 || (length % 4) != 0)
				return;

			int count = length / 4;

			for (int i = 0; i < count; ++i)
			{
				var s = pvSrc.ReadSerial();

				if (s.IsMobile)
				{
					Mobile m = World.FindMobile(s);

					if (m != null && from.CanSee(m) && from.InUpdateRange(m))
						m.SendPropertiesTo(from);
				}
				else if (s.IsItem)
				{
					Item item = World.FindItem(s);

					if (item != null && !item.Deleted && from.CanSee(item) && from.InUpdateRange(from.Location, item.GetWorldLocation()))
						item.SendPropertiesTo(from);
				}
			}
		}

		public static void QueryProperties(NetState state, PacketReader pvSrc)
		{
			if (!ObjectPropertyList.Enabled)
				return;

			Mobile from = state.Mobile;

			var s = pvSrc.ReadSerial();

			if (s.IsMobile)
			{
				Mobile m = World.FindMobile(s);

				if (m != null && from.CanSee(m) && from.InUpdateRange(m))
					m.SendPropertiesTo(from);
			}
			else if (s.IsItem)
			{
				Item item = World.FindItem(s);

				if (item != null && !item.Deleted && from.CanSee(item) && from.InUpdateRange(from.Location, item.GetWorldLocation()))
					item.SendPropertiesTo(from);
			}
		}

		public static void PartyMessage(NetState state, PacketReader pvSrc)
		{
			if (state.Mobile == null)
				return;

			switch (pvSrc.ReadByte())
			{
				case 0x01: PartyMessage_AddMember(state, pvSrc); break;
				case 0x02: PartyMessage_RemoveMember(state, pvSrc); break;
				case 0x03: PartyMessage_PrivateMessage(state, pvSrc); break;
				case 0x04: PartyMessage_PublicMessage(state, pvSrc); break;
				case 0x06: PartyMessage_SetCanLoot(state, pvSrc); break;
				case 0x08: PartyMessage_Accept(state, pvSrc); break;
				case 0x09: PartyMessage_Decline(state, pvSrc); break;
				default: pvSrc.Trace(state); break;
			}
		}

		public static void PartyMessage_AddMember(NetState state, PacketReader pvSrc)
		{
			if (PartyCommands.Handler != null)
				PartyCommands.Handler.OnAdd(state.Mobile);
		}

		public static void PartyMessage_RemoveMember(NetState state, PacketReader pvSrc)
		{
			if (PartyCommands.Handler != null)
				PartyCommands.Handler.OnRemove(state.Mobile, pvSrc.ReadMobile());
		}

		public static void PartyMessage_PrivateMessage(NetState state, PacketReader pvSrc)
		{
			if (PartyCommands.Handler != null)
				PartyCommands.Handler.OnPrivateMessage(state.Mobile, pvSrc.ReadMobile(), pvSrc.ReadUnicodeStringSafe());
		}

		public static void PartyMessage_PublicMessage(NetState state, PacketReader pvSrc)
		{
			if (PartyCommands.Handler != null)
				PartyCommands.Handler.OnPublicMessage(state.Mobile, pvSrc.ReadUnicodeStringSafe());
		}

		public static void PartyMessage_SetCanLoot(NetState state, PacketReader pvSrc)
		{
			if (PartyCommands.Handler != null)
				PartyCommands.Handler.OnSetCanLoot(state.Mobile, pvSrc.ReadBoolean());
		}

		public static void PartyMessage_Accept(NetState state, PacketReader pvSrc)
		{
			if (PartyCommands.Handler != null)
				PartyCommands.Handler.OnAccept(state.Mobile, pvSrc.ReadMobile());
		}

		public static void PartyMessage_Decline(NetState state, PacketReader pvSrc)
		{
			if (PartyCommands.Handler != null)
				PartyCommands.Handler.OnDecline(state.Mobile, pvSrc.ReadMobile());
		}

		public static void StunRequest(NetState state, PacketReader pvSrc)
		{
			EventSink.InvokeStunRequest(state.Mobile);
		}

		public static void DisarmRequest(NetState state, PacketReader pvSrc)
		{
			EventSink.InvokeDisarmRequest(state.Mobile);
		}

		public static void StatLockChange(NetState state, PacketReader pvSrc)
		{
			int stat = pvSrc.ReadByte();
			int lockValue = pvSrc.ReadByte();

			if (lockValue > 2) lockValue = 0;

			Mobile m = state.Mobile;

			if (m != null)
			{
				switch (stat)
				{
					case 0: m.StrLock = (StatLockType)lockValue; break;
					case 1: m.DexLock = (StatLockType)lockValue; break;
					case 2: m.IntLock = (StatLockType)lockValue; break;
				}
			}
		}

		public static void ScreenSize(NetState state, PacketReader pvSrc)
		{
			int width = pvSrc.ReadInt32();
			int unk = pvSrc.ReadInt32();
		}

		public static void ContextMenuResponse(NetState state, PacketReader pvSrc)
		{
			var user = state.Mobile;

			if (user == null)
			{
				return;
			}

			using (var menu = user.ContextMenu)
			{
				user.ContextMenu = null;

				if (menu != null && user == menu.From)
				{
					var entity = pvSrc.ReadEntity();

					if (entity != null && entity == menu.Target && user.CanSee(entity))
					{
						Point3D p;

						if (entity is Mobile)
						{
							p = entity.Location;
						}
						else if (entity is Item)
						{
							p = ((Item)entity).GetWorldLocation();
						}
						else
						{
							return;
						}

						int index = pvSrc.ReadUInt16();

						if (state.IsEnhancedClient && index > 0x64)
						{
							index = menu.GetIndexEC(index);
						}

						if (index >= 0 && index < menu.Entries.Length)
						{
							using (var e = menu.Entries[index])
							{
								var range = e.Range;

								if (range == -1)
								{
									if (user.NetState != null && user.NetState.UpdateRange > 0)
									{
										range = user.NetState.UpdateRange;
									}
									else
									{
										range = Map.GlobalUpdateRange;
									}
								}

								if (user.InRange(p, range))
								{
									if (e.Enabled)
									{
										e.OnClick();
									}
									else
									{
										e.OnClickDisabled();
									}
								}
							}
						}
					}
				}
			}
		}

		public static void ContextMenuRequest(NetState state, PacketReader pvSrc)
		{
			var target = pvSrc.ReadEntity();

			if (target != null && ObjectPropertyList.Enabled && !state.Mobile.ViewOpl)
			{
				HandleSingleClick(state.Mobile, target);
			}

			ContextMenu.Display(state.Mobile, target);
		}

		public static void CloseStatus(NetState state, PacketReader pvSrc)
		{
			pvSrc.ReadSerial();
		}

		public static void Language(NetState state, PacketReader pvSrc)
		{
			string lang = pvSrc.ReadString(4);

			if (state.Mobile != null)
				state.Mobile.Language = lang;
		}

		public static void AssistVersion(NetState state, PacketReader pvSrc)
		{
			int unk = pvSrc.ReadInt32();
			string av = pvSrc.ReadString();
		}

		public static void ClientVersion(NetState state, PacketReader pvSrc)
		{
			CV version = state.Version = new CV(pvSrc.ReadString());

			EventSink.InvokeClientVersionReceived(state, version);
		}

		public static void ClientType(NetState state, PacketReader pvSrc)
		{
			pvSrc.ReadUInt16();

			int type = pvSrc.ReadUInt16();
			CV version = state.Version = new CV(pvSrc.ReadString());

			//EventSink.InvokeClientVersionReceived( new ClientVersionReceivedArgs( state, version ) );//todo
		}

		public static void MobileQuery(NetState state, PacketReader pvSrc)
		{
			EntityQuery(state, pvSrc);
		}

		public static void ItemQuery(NetState state, PacketReader pvSrc)
		{
			EntityQuery(state, pvSrc);
		}

		public static void EntityQuery(NetState state, PacketReader pvSrc)
		{
			var from = state.Mobile;

			pvSrc.ReadInt32(); // 0xEDEDEDED

			int type = pvSrc.ReadByte();

			var serial = pvSrc.ReadSerial();

			if (serial.IsMobile)
			{
				var m = World.FindMobile(serial);

				if (m != null)
				{
					switch (type)
					{
						case 0x00: // Unknown, sent by godclient
							{
								if (VerifyGC(state))
								{
									Console.WriteLine("God Client: {0}: Query 0x{1:X2} on {2} '{3}'", state, type, serial, m.Name);
								}

								break;
							}
						case 0x04: // Stats
							{
								m.OnStatsQuery(from);
								break;
							}
						case 0x05:
							{
								m.OnSkillsQuery(from);
								break;
							}
						default:
							{
								pvSrc.Trace(state);
								break;
							}
					}
				}
			}
			else if (serial.IsItem)
			{
				var item = World.FindItem(serial) as IDamageable;

				if (item != null)
				{
					switch (type)
					{
						case 0x00:
							{
								if (VerifyGC(state))
								{
									Console.WriteLine("God Client: {0}: Query 0x{1:X2} on {2} '{3}'", state, type, serial, item.Name);
								}

								break;
							}
						case 0x04: // Stats
							{
								item.OnStatsQuery(from);
								break;
							}
						case 0x05:
							{
								break;
							}
						default:
							{
								pvSrc.Trace(state);
								break;
							}
					}
				}
			}
		}

		public delegate void PlayCharCallback(NetState state, bool val);

		public static PlayCharCallback ThirdPartyAuthCallback, ThirdPartyHackedCallback;

		private static readonly byte[] m_ThirdPartyAuthKey = new byte[]
			{
				0x9, 0x11, 0x83, (byte)'+', 0x4, 0x17, 0x83,
				0x5, 0x24, 0x85,
				0x7, 0x17, 0x87,
				0x6, 0x19, 0x88,
			};

		private class LoginTimer : Timer
		{
			private int m_Ticks;

			private NetState m_State;

			public LoginTimer(NetState state)
				: base(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0))
			{
				m_State = state;
			}

			protected override void OnTick()
			{
				if (m_State == null || !m_State.Running)
				{
					Stop();

					m_State = null;
				}
				else if (m_State.Version != null)
				{
					Stop();

					m_State.BlockAllPackets = false;

					DoLogin(m_State);

					m_State = null;
				}
				else if (++m_Ticks % 10 == 0)
				{
					Stop();

					AccountLogin_ReplyRej(m_State, ALRReason.BadComm);

					m_State = null;
				}
			}
		}

		public static void PlayCharacter(NetState state, PacketReader pvSrc)
		{
			pvSrc.ReadInt32(); // 0xEDEDEDED

			string name = pvSrc.ReadString(30);

			pvSrc.Seek(2, SeekOrigin.Current);
			int flags = pvSrc.ReadInt32();

			if (FeatureProtection.DisabledFeatures != 0 && ThirdPartyAuthCallback != null)
			{
				bool authOK = false;

				ulong razorFeatures = (((ulong)pvSrc.ReadUInt32()) << 32) | pvSrc.ReadUInt32();

				if (razorFeatures == (ulong)FeatureProtection.DisabledFeatures)
				{
					bool match = true;
					for (int i = 0; match && i < m_ThirdPartyAuthKey.Length; i++)
						match = match && pvSrc.ReadByte() == m_ThirdPartyAuthKey[i];

					if (match)
						authOK = true;
				}
				else
				{
					pvSrc.Seek(16, SeekOrigin.Current);
				}

				ThirdPartyAuthCallback(state, authOK);
			}
			else
			{
				pvSrc.Seek(24, SeekOrigin.Current);
			}

			if (ThirdPartyHackedCallback != null)
			{
				pvSrc.Seek(-2, SeekOrigin.Current);
				if (pvSrc.ReadUInt16() == 0xDEAD)
					ThirdPartyHackedCallback(state, true);
			}

			if (!state.Running)
				return;

			int charSlot = pvSrc.ReadInt32();
			int clientIP = pvSrc.ReadInt32();

			IAccount a = state.Account;

			if (a == null || charSlot < 0 || charSlot >= a.Length)
			{
				Utility.WriteConsole(ConsoleColor.Red, $"Login: {state}: Invalid Character Selection.");
				state.Dispose();
			}
			else
			{
				Mobile m = a[charSlot];

				// Check if anyone is using this account
				for (int i = 0; i < a.Length; ++i)
				{
					Mobile check = a[i];

					if (check != null && check.Map != Map.Internal && check != m)
					{
						Console.WriteLine("Login: {0}: Account in use", state);
						PopupMessage.Send(state, PMMessage.CharInWorld);
						return;
					}
				}

				if (m == null)
				{
					Utility.WriteConsole(ConsoleColor.Red, $"Login:{state}:InvalidCharacterSelection.");
					state.Dispose();
				}
				else
				{
					if (m.NetState != null)
					{
						m.NetState.Dispose();
					}

					NetState.ProcessDisposedQueue();

					state.Flags = (ClientFlags)flags;
					state.Mobile = m;

					m.NetState = state;

					if (state.Version == CV.Zero)
					{
						ClientVersionReq.Send(state);

						state.BlockAllPackets = true;

						new LoginTimer(state).Start();
					}
					else
					{
						DoLogin(state);
					}
				}
			}
		}

		/*public static void DoLogin(NetState state)
		{
			var m = state.Mobile;

			state.Send(new LoginConfirm(m));

			if (m.Map != null)
				state.Send(new MapChange(m));

			state.Send(new MapPatches());

			state.Send(SeasonChange.Instantiate(m.GetSeason(), true));

			state.Send(SupportedFeatures.Instantiate(state));

			state.Sequence = 0;

			if (state.NewMobileIncoming)
			{
				state.Send(new MobileUpdate(m));
				state.Send(new MobileUpdate(m));

				m.CheckLightLevels(true);

				state.Send(new MobileUpdate(m));

				state.Send(new MobileIncoming(m, m));
				//state.Send( new MobileAttributes( m ) );
				state.Send(new MobileStatus(m, m));
				state.Send(Server.Network.SetWarMode.Instantiate(m.Warmode));

				m.SendEverything();

				state.Send(SupportedFeatures.Instantiate(state));
				state.Send(new MobileUpdate(m));
				//state.Send( new MobileAttributes( m ) );
				state.Send(new MobileStatus(m, m));
				state.Send(Server.Network.SetWarMode.Instantiate(m.Warmode));
				state.Send(new MobileIncoming(m, m));
			}
			else if (state.StygianAbyss)
			{
				state.Send(new MobileUpdate(m));
				state.Send(new MobileUpdate(m));

				m.CheckLightLevels(true);

				state.Send(new MobileUpdate(m));

				state.Send(new MobileIncomingSA(m, m));
				//state.Send( new MobileAttributes( m ) );
				state.Send(new MobileStatus(m, m));
				state.Send(Server.Network.SetWarMode.Instantiate(m.Warmode));

				m.SendEverything();

				state.Send(SupportedFeatures.Instantiate(state));
				state.Send(new MobileUpdate(m));
				//state.Send( new MobileAttributes( m ) );
				state.Send(new MobileStatus(m, m));
				state.Send(Server.Network.SetWarMode.Instantiate(m.Warmode));
				state.Send(new MobileIncomingSA(m, m));
			}
			else
			{
				state.Send(new MobileUpdateOld(m));
				state.Send(new MobileUpdateOld(m));

				m.CheckLightLevels(true);

				state.Send(new MobileUpdateOld(m));

				state.Send(new MobileIncomingOld(m, m));
				//state.Send( new MobileAttributes( m ) );
				state.Send(new MobileStatus(m, m));
				state.Send(Server.Network.SetWarMode.Instantiate(m.Warmode));

				m.SendEverything();

				state.Send(SupportedFeatures.Instantiate(state));
				state.Send(new MobileUpdateOld(m));
				//state.Send( new MobileAttributes( m ) );
				state.Send(new MobileStatus(m, m));
				state.Send(Server.Network.SetWarMode.Instantiate(m.Warmode));
				state.Send(new MobileIncomingOld(m, m));
			}

			state.Send(LoginComplete.Instance);
			state.Send(new CurrentTime());
			state.Send(SeasonChange.Instantiate(m.GetSeason(), true));
			state.Send(new MapChange(m));

			EventSink.InvokeLogin(m);

			m.ClearFastwalkStack();
		}*/
		public static void DoLogin(NetState state)
		{
			var m = state.Mobile;

			state.BlockAllPackets = false;

			state.Send(new LoginConfirm(m));

			m.SendMapUpdates(false, true);

			state.Send(LoginComplete.Instance);

			MobileStatus.Send(state, m);

			//Network.SetWarMode.Send(state);
			state.Send(Server.Network.SetWarMode.Instantiate(m.Warmode));

			state.Send(new CurrentTime());

			EventSink.InvokeLogin(m);

			Utility.WriteConsole(ConsoleColor.Green, "Client: {0}: Entered World ({1})", state, m);
		}

		public static void CreateCharacter(NetState state, PacketReader pvSrc)
		{
			var isEC = pvSrc.ID == 0x8D;
			var is70160 = !isEC && pvSrc.ID == 0xF8;

			pvSrc.ReadUInt32(); // preamble 0xEDEDEDED
			pvSrc.ReadUInt32(); // preamble 0xFFFFFFFF (EC: character index)

			if (!isEC)
			{
				pvSrc.ReadByte(); // preamble terminator 0x0
			}

			var name = pvSrc.ReadString(30);

			if (isEC)
			{
				pvSrc.Skip(30); // password?
			}
			else
			{
				pvSrc.ReadUInt16(); // unknown

				state.Flags = (ClientFlags)pvSrc.ReadInt32();

				pvSrc.ReadUInt32(); // unknown
				pvSrc.ReadUInt32(); // login count
			}

			var prof = (Profession)pvSrc.ReadByte();

			if (!Enum.IsDefined(typeof(Profession), prof))
			{
				prof = Profession.Advanced;
			}

			bool female;
			var raceID = 0;

			if (isEC)
			{
				state.Flags = (ClientFlags)pvSrc.ReadByte();

				female = pvSrc.ReadByte() != 0;
				raceID = pvSrc.ReadByte();
			}
			else
			{
				pvSrc.Skip(15); // unknown

				var genderRace = pvSrc.ReadByte();

				female = genderRace % 2 != 0;

				if (genderRace >= 4)
				{
					raceID = (genderRace / 2) - 1;
				}
			}

			var stats = new[]
			{
				new StatNameValue(StatType.Str, pvSrc.ReadByte()),
				new StatNameValue(StatType.Dex, pvSrc.ReadByte()),
				new StatNameValue(StatType.Int, pvSrc.ReadByte())
			};

			int hairVal, beardVal, faceVal = 0;
			int hairHue, beardHue, faceHue = 0;
			int shirtHue, pantsHue, skinHue = 0;

			if (isEC)
			{
				skinHue = pvSrc.ReadUInt16();

				pvSrc.Skip(8); // unknown
			}

			var skills = new SkillNameValue[isEC || is70160 ? 4 : 3];

			for (var i = 0; i < skills.Length; i++)
			{
				skills[i] = new SkillNameValue((SkillName)pvSrc.ReadByte(), pvSrc.ReadByte());
			}

			var cityIndex = 0;

			if (isEC)
			{
				pvSrc.Skip(26); // unknown

				hairHue = pvSrc.ReadUInt16();
				hairVal = pvSrc.ReadUInt16();

				pvSrc.Skip(6); // unknown

				shirtHue = pvSrc.ReadUInt16();
				pantsHue = pvSrc.ReadUInt16();

				pvSrc.Skip(1); // unknown

				faceHue = pvSrc.ReadUInt16();
				faceVal = pvSrc.ReadUInt16();

				pvSrc.Skip(1); // unknown

				beardHue = pvSrc.ReadUInt16();
				beardVal = pvSrc.ReadUInt16();
			}
			else
			{
				skinHue = pvSrc.ReadUInt16();

				hairVal = pvSrc.ReadUInt16();
				hairHue = pvSrc.ReadUInt16();

				beardVal = pvSrc.ReadUInt16();
				beardHue = pvSrc.ReadUInt16();

				pvSrc.Skip(1); // unknown

				cityIndex = pvSrc.ReadByte();

				pvSrc.ReadInt32(); // character slot
				pvSrc.ReadInt32(); // ipv4 address

				shirtHue = pvSrc.ReadInt16();
				pantsHue = pvSrc.ReadInt16();
			}

			var info = state.CityInfo;

			if (info == null || cityIndex < 0 || cityIndex >= info.Length)
			{
				Utility.WriteConsole(ConsoleColor.Red, $"Login: {state}: Invalid city");

				PopupMessage.Send(state, PMMessage.LoginSyncError);

				state.Dispose();

				return;
			}

			var acc = state.Account;

			if (acc == null)
			{
				Utility.WriteConsole(ConsoleColor.Red, $"Login: {state}: Invalid account");

				PopupMessage.Send(state, PMMessage.LoginSyncError);

				state.Dispose();

				return;
			}

			// Check if anyone is using this account
			for (var i = 0; i < acc.Length; ++i)
			{
				var check = acc[i];

				if (check != null && check.Map != Map.Internal)
				{
					if (acc.AccessLevel > AccessLevel.Player)
					{
						EventSink.InvokeLogout(check);

						check.LogoutMap = check.Map;
						check.LogoutLocation = check.Location;

						check.Internalize();
						continue;
					}

					Utility.WriteConsole(ConsoleColor.Red, $"Login: {state}: Account in use");

					PopupMessage.Send(state, PMMessage.CharInWorld);

					return;
				}
			}

			var race = Race.Races[raceID] ?? Race.DefaultRace;

			skinHue = race.ClipSkinHue(skinHue);
			hairHue = race.ClipHairHue(hairHue);
			beardHue = race.ClipHairHue(beardHue);

			Utility.Clamp(ref shirtHue, 0, 1001);
			Utility.Clamp(ref pantsHue, 0, 1001);

			if (state.Version == CV.Zero)
			{
				ClientVersionReq.Send(state);

				state.BlockAllPackets = true;
			}

			CharacterCreatedEventArgs args = new(state, name, info[cityIndex], race, prof, stats, skills, female, skinHue, hairVal, hairHue, beardVal, beardHue, faceVal, faceHue, shirtHue, pantsHue);

			EventSink.InvokeCharacterCreated(args);

			var m = args.Mobile;

			if (m == null)
			{
				state.BlockAllPackets = false;
				state.Dispose();
				return;
			}

			state.Mobile = m;

			m.NetState = state;
			//EventSink.InvokeCharacterCreated(args);
			//EventSink.InvokeCharacterCreated(new CharacterCreatedEventArgs(state, m));

			if (state.Version == CV.Zero)
			{
				new LoginTimer(state).Start();
			}
			else
			{
				DoLogin(state);
			}
			/*
			int unk1 = pvSrc.ReadInt32();
			int unk2 = pvSrc.ReadInt32();
			int unk3 = pvSrc.ReadByte();
			string name = pvSrc.ReadString(30);

			pvSrc.Seek(2, SeekOrigin.Current);
			int flags = pvSrc.ReadInt32();
			pvSrc.Seek(8, SeekOrigin.Current);
			int prof = pvSrc.ReadByte();
			pvSrc.Seek(15, SeekOrigin.Current);

			//bool female = pvSrc.ReadBoolean();

			int genderRace = pvSrc.ReadByte();

			int str = pvSrc.ReadByte();
			int dex = pvSrc.ReadByte();
			int intl = pvSrc.ReadByte();
			int is1 = pvSrc.ReadByte();
			int vs1 = pvSrc.ReadByte();
			int is2 = pvSrc.ReadByte();
			int vs2 = pvSrc.ReadByte();
			int is3 = pvSrc.ReadByte();
			int vs3 = pvSrc.ReadByte();
			int hue = pvSrc.ReadUInt16();
			int hairVal = pvSrc.ReadInt16();
			int hairHue = pvSrc.ReadInt16();
			int hairValf = pvSrc.ReadInt16();
			int hairHuef = pvSrc.ReadInt16();
			pvSrc.ReadByte();
			int cityIndex = pvSrc.ReadByte();
			int charSlot = pvSrc.ReadInt32();
			int clientIP = pvSrc.ReadInt32();
			int shirtHue = pvSrc.ReadInt16();
			int pantsHue = pvSrc.ReadInt16();

			/*
			Pre-7.0.0.0:
			0x00, 0x01 -> Human Male, Human Female
			0x02, 0x03 -> Elf Male, Elf Female

			Post-7.0.0.0:
			0x00, 0x01
			0x02, 0x03 -> Human Male, Human Female
			0x04, 0x05 -> Elf Male, Elf Female
			0x05, 0x06 -> Gargoyle Male, Gargoyle Female
			*/
			/*
			bool female = ((genderRace % 2) != 0);

			Race race = null;

			if (state.StygianAbyss)
			{
				byte raceID = (byte)(genderRace < 4 ? 0 : ((genderRace / 2) - 1));
				race = Race.Races[raceID];
			}
			else
			{
				race = Race.Races[(byte)(genderRace / 2)];
			}

			if (race == null)
				race = Race.DefaultRace;

			CityInfo[] info = state.CityInfo;
			IAccount a = state.Account;

			if (info == null || a == null || cityIndex < 0 || cityIndex >= info.Length)
			{
				state.Dispose();
			}
			else
			{
				// Check if anyone is using this account
				for (int i = 0; i < a.Length; ++i)
				{
					Mobile check = a[i];

					if (check != null && check.Map != Map.Internal)
					{
						Console.WriteLine("Login: {0}: Account in use", state);
						state.Send(new PopupMessage(PMMessage.CharInWorld));
						return;
					}
				}

				state.Flags = (ClientFlags)flags;

				CharacterCreatedEventArgs args = new(
					state, a,
					name, female, hue,
					str, dex, intl,
					info[cityIndex],
					new SkillNameValue[3]
					{
						new SkillNameValue( (SkillName)is1, vs1 ),
						new SkillNameValue( (SkillName)is2, vs2 ),
						new SkillNameValue( (SkillName)is3, vs3 ),
					},
					shirtHue, pantsHue,
					hairVal, hairHue,
					hairValf, hairHuef,
					prof, race
					);

				state.Send(new ClientVersionReq());

				state.BlockAllPackets = true;

				EventSink.InvokeCharacterCreated(args);

				Mobile m = args.Mobile;

				if (m != null)
				{
					state.Mobile = m;
					m.NetState = state;
					new LoginTimer(state).Start();
				}
				else
				{
					state.BlockAllPackets = false;
					state.Dispose();
				}
			}*/
		}

		public static void CreateCharacter70160(NetState state, PacketReader pvSrc)
		{
			var isEC = pvSrc.ID == 0x8D;
			var is70160 = !isEC && pvSrc.ID == 0xF8;

			pvSrc.ReadUInt32(); // preamble 0xEDEDEDED
			pvSrc.ReadUInt32(); // preamble 0xFFFFFFFF (EC: character index)

			if (!isEC)
			{
				pvSrc.ReadByte(); // preamble terminator 0x0
			}

			var name = pvSrc.ReadString(30);

			if (isEC)
			{
				pvSrc.Skip(30); // password?
			}
			else
			{
				pvSrc.ReadUInt16(); // unknown

				state.Flags = (ClientFlags)pvSrc.ReadInt32();

				pvSrc.ReadUInt32(); // unknown
				pvSrc.ReadUInt32(); // login count
			}

			var prof = (Profession)pvSrc.ReadByte();

			if (!Enum.IsDefined(typeof(Profession), prof))
			{
				prof = Profession.Advanced;
			}

			bool female;
			var raceID = 0;

			if (isEC)
			{
				state.Flags = (ClientFlags)pvSrc.ReadByte();

				female = pvSrc.ReadByte() != 0;
				raceID = pvSrc.ReadByte();
			}
			else
			{
				pvSrc.Skip(15); // unknown

				var genderRace = pvSrc.ReadByte();

				female = genderRace % 2 != 0;

				if (genderRace >= 4)
				{
					raceID = (genderRace / 2) - 1;
				}
			}

			var stats = new[]
			{
				new StatNameValue(StatType.Str, pvSrc.ReadByte()),
				new StatNameValue(StatType.Dex, pvSrc.ReadByte()),
				new StatNameValue(StatType.Int, pvSrc.ReadByte())
			};

			int hairVal, beardVal, faceVal = 0;
			int hairHue, beardHue, faceHue = 0;
			int shirtHue, pantsHue, skinHue = 0;

			if (isEC)
			{
				skinHue = pvSrc.ReadUInt16();

				pvSrc.Skip(8); // unknown
			}

			var skills = new SkillNameValue[isEC || is70160 ? 4 : 3];

			for (var i = 0; i < skills.Length; i++)
			{
				skills[i] = new SkillNameValue((SkillName)pvSrc.ReadByte(), pvSrc.ReadByte());
			}

			var cityIndex = 0;

			if (isEC)
			{
				pvSrc.Skip(26); // unknown

				hairHue = pvSrc.ReadUInt16();
				hairVal = pvSrc.ReadUInt16();

				pvSrc.Skip(6); // unknown

				shirtHue = pvSrc.ReadUInt16();
				pantsHue = pvSrc.ReadUInt16();

				pvSrc.Skip(1); // unknown

				faceHue = pvSrc.ReadUInt16();
				faceVal = pvSrc.ReadUInt16();

				pvSrc.Skip(1); // unknown

				beardHue = pvSrc.ReadUInt16();
				beardVal = pvSrc.ReadUInt16();
			}
			else
			{
				skinHue = pvSrc.ReadUInt16();

				hairVal = pvSrc.ReadUInt16();
				hairHue = pvSrc.ReadUInt16();

				beardVal = pvSrc.ReadUInt16();
				beardHue = pvSrc.ReadUInt16();

				pvSrc.Skip(1); // unknown

				cityIndex = pvSrc.ReadByte();

				pvSrc.ReadInt32(); // character slot
				pvSrc.ReadInt32(); // ipv4 address

				shirtHue = pvSrc.ReadInt16();
				pantsHue = pvSrc.ReadInt16();
			}

			var info = state.CityInfo;

			if (info == null || cityIndex < 0 || cityIndex >= info.Length)
			{
				Utility.WriteConsole(ConsoleColor.Red, $"Login: {state}: Invalid city");

				PopupMessage.Send(state, PMMessage.LoginSyncError);

				state.Dispose();

				return;
			}

			var acc = state.Account;

			if (acc == null)
			{
				Utility.WriteConsole(ConsoleColor.Red, $"Login: {state}: Invalid account");

				PopupMessage.Send(state, PMMessage.LoginSyncError);

				state.Dispose();

				return;
			}

			// Check if anyone is using this account
			for (var i = 0; i < acc.Length; ++i)
			{
				var check = acc[i];

				if (check != null && check.Map != Map.Internal)
				{
					if (acc.AccessLevel > AccessLevel.Player)
					{
						EventSink.InvokeLogout(check);

						check.LogoutMap = check.Map;
						check.LogoutLocation = check.Location;

						check.Internalize();
						continue;
					}

					Utility.WriteConsole(ConsoleColor.Red, $"Login: {state}: Account in use");

					PopupMessage.Send(state, PMMessage.CharInWorld);

					return;
				}
			}

			var race = Race.Races[raceID] ?? Race.DefaultRace;

			skinHue = race.ClipSkinHue(skinHue);
			hairHue = race.ClipHairHue(hairHue);
			beardHue = race.ClipHairHue(beardHue);

			Utility.Clamp(ref shirtHue, 0, 1001);
			Utility.Clamp(ref pantsHue, 0, 1001);

			if (state.Version == CV.Zero)
			{
				ClientVersionReq.Send(state);

				state.BlockAllPackets = true;
			}

			CharacterCreatedEventArgs args = new(state, name, info[cityIndex], race, prof, stats, skills, female, skinHue, hairVal, hairHue, beardVal, beardHue, faceVal, faceHue, shirtHue, pantsHue);

			EventSink.InvokeCharacterCreated(args);

			var m = args.Mobile;

			if (m == null)
			{
				state.BlockAllPackets = false;
				state.Dispose();
				return;
			}

			state.Mobile = m;

			m.NetState = state;

			if (state.Version == CV.Zero)
			{
				new LoginTimer(state).Start();
			}
			else
			{
				DoLogin(state);
			}
		}

		public static void PublicHouseContent(NetState state, PacketReader pvSrc)
		{
			int value = pvSrc.ReadByte();
			state.Mobile.PublicHouseContent = Convert.ToBoolean(value);
		}

		public static bool ClientVerification { get; set; } = true;

		internal struct AuthIDPersistence
		{
			public DateTime Age;
			public ClientVersion Version;

			public AuthIDPersistence(ClientVersion v)
			{
				Age = DateTime.UtcNow;
				Version = v;
			}
		}

		private const int m_AuthIDWindowSize = 128;
		private static readonly Dictionary<int, AuthIDPersistence> m_AuthIDWindow = new(m_AuthIDWindowSize);

		private static int GenerateAuthID(NetState state)
		{
			if (m_AuthIDWindow.Count == m_AuthIDWindowSize)
			{
				int oldestID = 0;
				DateTime oldest = DateTime.MaxValue;

				foreach (KeyValuePair<int, AuthIDPersistence> kvp in m_AuthIDWindow)
				{
					if (kvp.Value.Age < oldest)
					{
						oldestID = kvp.Key;
						oldest = kvp.Value.Age;
					}
				}

				m_AuthIDWindow.Remove(oldestID);
			}

			int authID;

			do
			{
				authID = Utility.Random(1, int.MaxValue - 1);

				if (Utility.RandomBool())
					authID |= 1 << 31;
			} while (m_AuthIDWindow.ContainsKey(authID));

			m_AuthIDWindow[authID] = new AuthIDPersistence(state.Version);

			return authID;
		}

		public static void GameLogin(NetState state, PacketReader pvSrc)
		{
			if (state.SentFirstPacket)
			{
				state.Dispose();
				return;
			}

			state.SentFirstPacket = true;

			int authID = pvSrc.ReadInt32();

			if (m_AuthIDWindow.ContainsKey(authID))
			{
				AuthIDPersistence ap = m_AuthIDWindow[authID];
				m_AuthIDWindow.Remove(authID);

				state.Version = ap.Version;
			}
			else if (ClientVerification)
			{
				Console.WriteLine("Login: {0}: Invalid client detected, disconnecting", state);
				state.Dispose();
				return;
			}

			if (state.m_AuthID != 0 && authID != state.m_AuthID)
			{
				Console.WriteLine("Login: {0}: Invalid client detected, disconnecting", state);
				state.Dispose();
				return;
			}
			else if (state.m_AuthID == 0 && authID != state.m_Seed)
			{
				Console.WriteLine("Login: {0}: Invalid client detected, disconnecting", state);
				state.Dispose();
				return;
			}

			string username = pvSrc.ReadString(30);
			string password = pvSrc.ReadString(30);

			GameLoginEventArgs e = new(state, username, password);

			EventSink.InvokeGameLogin(e);

			if (e.Accepted)
			{
				state.CityInfo = e.CityInfo;
				state.CompressionEnabled = true;

				state.Send(SupportedFeatures.Instantiate(state));

				if (state.NewCharacterList)
				{
					state.Send(new CharacterList(state.Account, state.CityInfo, state.IsEnhancedClient));
				}
				else
				{
					state.Send(new CharacterListOld(state.Account, state.CityInfo));
				}
			}
			else
			{
				state.Dispose();
			}
		}

		public static void PlayServer(NetState state, PacketReader pvSrc)
		{
			int index = pvSrc.ReadInt16();
			ServerInfo[] info = state.ServerInfo;
			IAccount a = state.Account;

			if (info == null || a == null || index < 0 || index >= info.Length)
			{
				state.Dispose();
			}
			else
			{
				ServerInfo si = info[index];

				state.m_AuthID = PlayServerAck.m_AuthID = GenerateAuthID(state);

				state.SentFirstPacket = false;
				state.Send(new PlayServerAck(si));
			}
		}

		public static void LoginServerSeed(NetState state, PacketReader pvSrc)
		{
			state.m_Seed = pvSrc.ReadInt32();
			state.Seeded = true;

			if (state.m_Seed == 0)
			{
				Console.WriteLine("Login: {0}: Invalid client detected, disconnecting", state);
				state.Dispose();
				return;
			}

			int clientMaj = pvSrc.ReadInt32();
			int clientMin = pvSrc.ReadInt32();
			int clientRev = pvSrc.ReadInt32();
			int clientPat = pvSrc.ReadInt32();

			state.Version = new ClientVersion(clientMaj, clientMin, clientRev, clientPat);
		}

		public static void CrashReport(NetState state, PacketReader pvSrc)
		{
			byte clientMaj = pvSrc.ReadByte();
			byte clientMin = pvSrc.ReadByte();
			byte clientRev = pvSrc.ReadByte();
			byte clientPat = pvSrc.ReadByte();

			ushort x = pvSrc.ReadUInt16();
			ushort y = pvSrc.ReadUInt16();
			sbyte z = pvSrc.ReadSByte();
			byte map = pvSrc.ReadByte();

			string account = pvSrc.ReadString(32);
			string character = pvSrc.ReadString(32);
			string ip = pvSrc.ReadString(15);

			int unk1 = pvSrc.ReadInt32();
			int exception = pvSrc.ReadInt32();

			string process = pvSrc.ReadString(100);
			string report = pvSrc.ReadString(100);

			pvSrc.ReadByte(); // 0x00

			int offset = pvSrc.ReadInt32();

			int count = pvSrc.ReadByte();

			for (int i = 0; i < count; i++)
			{
				int address = pvSrc.ReadInt32();
			}
		}

		public static void AccountLogin(NetState state, PacketReader pvSrc)
		{
			if (state.SentFirstPacket)
			{
				state.Dispose();
				return;
			}

			state.SentFirstPacket = true;

			string username = pvSrc.ReadString(30);
			string password = pvSrc.ReadString(30);

			AccountLoginEventArgs e = new(state, username, password);

			EventSink.InvokeAccountLogin(e);

			if (e.Accepted)
				AccountLogin_ReplyAck(state);
			else
				AccountLogin_ReplyRej(state, e.RejectReason);
		}

		public static void AccountLogin_ReplyAck(NetState state)
		{
			ServerListEventArgs e = new(state, state.Account);

			EventSink.InvokeServerList(e);

			if (e.Rejected)
			{
				state.Account = null;
				state.Send(new AccountLoginRej(ALRReason.BadComm));
				state.Dispose();
			}
			else
			{
				ServerInfo[] info = e.Servers.ToArray();

				state.ServerInfo = info;

				state.Send(new AccountLoginAck(info));
			}
		}

		public static void AccountLogin_ReplyRej(NetState state, ALRReason reason)
		{
			state.Send(new AccountLoginRej(reason));
			state.Dispose();
		}
	}
}
