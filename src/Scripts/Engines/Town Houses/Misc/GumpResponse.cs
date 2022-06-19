using Server.Gumps;
using Server.Network;
using System;
using System.Linq;

namespace Server.Engines.TownHouses
{
	public class GumpResponse
	{
		public static void Initialize()
		{
			Timer.DelayCall(TimeSpan.Zero, AfterInit);
		}

		private static void AfterInit()
		{
			PacketHandlers.Register(0xB1, 0, true, DisplayGumpResponse);
		}

		private static void DisplayGumpResponse(NetState state, PacketReader pvSrc)
		{
			var serial = pvSrc.ReadInt32();
			var typeId = pvSrc.ReadInt32();
			var buttonId = pvSrc.ReadInt32();

			var gumps = state.Gumps.ToList();

			for (var i = 0; i < gumps.Count; ++i)
			{
				var gump = gumps[i];
				if (gump == null)
				{
					continue;
				}

				if (gump.Serial != serial || gump.TypeId != typeId)
				{
					continue;
				}

				var switchCount = pvSrc.ReadInt32();

				if (switchCount < 0)
				{
					Console.WriteLine("Client: {0}: Invalid gump response, disconnecting...", state);
					state.Dispose();
					return;
				}

				var switches = new int[switchCount];

				for (int j = 0; j < switches.Length; ++j)
				{
					switches[j] = pvSrc.ReadInt32();
				}

				var textCount = pvSrc.ReadInt32();

				if (textCount < 0)
				{
					Console.WriteLine("Client: {0}: Invalid gump response, disconnecting...", state);
					state.Dispose();
					return;
				}

				var textEntries = new TextRelay[textCount];

				for (var j = 0; j < textEntries.Length; ++j)
				{
					int entryId = pvSrc.ReadUInt16();
					int textLength = pvSrc.ReadUInt16();

					if (textLength > 239)
					{
						return;
					}

					string text = pvSrc.ReadUnicodeStringSafe(textLength);
					textEntries[j] = new TextRelay(entryId, text);
				}

				state.RemoveGump(i);

				if (!CheckResponse(gump, state.Mobile, buttonId))
				{
					return;
				}

				gump.OnResponse(state, new RelayInfo(buttonId, switches, textEntries));

				return;
			}

			if (typeId == 461) // Virtue gump
			{
				var switchCount = pvSrc.ReadInt32();

				if (buttonId == 1 && switchCount > 0)
				{
					var beheld = World.FindMobile(pvSrc.ReadInt32());

					if (beheld != null)
					{
						EventSink.InvokeVirtueGumpRequest(state.Mobile, beheld);
					}
				}
				else
				{
					var beheld = World.FindMobile(serial);

					if (beheld != null)
					{
						EventSink.InvokeVirtueItemRequest(state.Mobile, beheld, buttonId);
					}
				}
			}
		}

		private static bool CheckResponse(Gump gump, Mobile m, int id)
		{
			if (m is not {Player: true})
			{
				return true;
			}

			var list = m.GetItemsInRange(20).OfType<TownHouse>().Cast<Item>().ToList();

			var th = list.Cast<TownHouse>().FirstOrDefault(t => t != null && t.Owner == m);

			if (th?.ForSaleSign == null)
			{
				return true;
			}

			if (gump is HouseGumpAOS)
			{
				var val = id - 1;

				if (val < 0)
				{
					return true;
				}

				var type = val % 15;
				var index = val / 15;

				if (th.ForSaleSign.ForcePublic && type == 3 && index == 12 && th.Public)
				{
					m.SendMessage("This house cannot be private.");
					m.SendGump(gump);
					return false;
				}

				if (th.ForSaleSign.ForcePrivate && type == 3 && index == 13 && !th.Public)
				{
					m.SendMessage("This house cannot be public.");
					m.SendGump(gump);
					return false;
				}

				if (/*!th.ForSaleSign.NoTrade ||*/ type != 6 || index != 1)
				{
					return true;
				}
				m.SendMessage("This house cannot be traded.");
				m.SendGump(gump);
				return false;
			}

			if (gump is not HouseGump)
			{
				return true;
			}

			if (th.ForSaleSign.ForcePublic && id == 17 && th.Public)
			{
				m.SendMessage("This house cannot be private.");
				m.SendGump(gump);
				return false;
			}

			if (th.ForSaleSign.ForcePrivate && id == 17 && !th.Public)
			{
				m.SendMessage("This house cannot be public.");
				m.SendGump(gump);
				return false;
			}

			if (/*!th.ForSaleSign.NoTrade ||*/ id != 14)
			{
				return true;
			}
			m.SendMessage("This house cannot be traded.");
			m.SendGump(gump);
			return false;
		}
	}
}
