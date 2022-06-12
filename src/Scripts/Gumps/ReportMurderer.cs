using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Network;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Server.Gumps
{
	public class ReportMurdererGump : Gump
	{
		public static void Configure()
		{
			PacketHandlers.Register(0xAC, 0, true, new OnPacketReceive(BountyEntryResponse));
		}

		private static readonly TimeSpan ReportExpirationTimeout = TimeSpan.FromMinutes(10);

		private int m_Idx;
		private readonly List<Mobile> m_Killers;
		private readonly Mobile m_Victum;
		public static readonly bool BountyEnabled = Settings.Configuration.Get<bool>("Gameplay", "BountyEnabled");
		public static void Initialize()
		{
			EventSink.OnMobileDeath += EventSink_PlayerDeath;
		}

		public static void EventSink_PlayerDeath(Mobile m, Mobile killer, Container cont)
		{
			List<Mobile> killers = new();
			List<Mobile> toGive = new();

			foreach (AggressorInfo ai in m.Aggressors)
			{
				if (ai.Attacker.Player && ai.CanReportMurder && !ai.Reported)
				{
					if (!Core.SE || !((PlayerMobile)m).RecentlyReported.Contains(ai.Attacker))
					{
						killers.Add(ai.Attacker);
						ai.Reported = true;
						ai.CanReportMurder = false;
					}
				}

				if (ai.Attacker.Player && (DateTime.UtcNow - ai.LastCombatTime) < TimeSpan.FromSeconds(30.0) && !toGive.Contains(ai.Attacker))
					toGive.Add(ai.Attacker);
			}

			foreach (AggressorInfo ai in m.Aggressed)
			{
				if (ai.Defender.Player && (DateTime.UtcNow - ai.LastCombatTime) < TimeSpan.FromSeconds(30.0) && !toGive.Contains(ai.Defender))
					toGive.Add(ai.Defender);
			}

			foreach (Mobile g in toGive)
			{
				int n = Notoriety.Compute(g, m);
				_ = m.Karma;
				int ourKarma = g.Karma;
				bool innocent = n == Notoriety.Innocent;
				bool criminal = n == Notoriety.Criminal || n == Notoriety.Murderer;

				int fameAward = m.Fame / 200;
				int karmaAward = 0;

				if (innocent)
					karmaAward = ourKarma > -2500 ? -850 : -110 - (m.Karma / 100);
				else if (criminal)
					karmaAward = 50;

				Titles.AwardFame(g, fameAward, false);
				Titles.AwardKarma(g, karmaAward, true);
				XmlQuest.RegisterKill(m, g);

				if (killers.Contains(g))
				{
					EventSink.InvokeOnPlayerMurdered(g, m);
				}
			}

			if (m is PlayerMobile mobile && mobile.NpcGuild == NpcGuild.ThievesGuild)
				return;

			if (killers.Count > 0)
				new GumpTimer(m, killers).Start();
		}

		private class BountyEntry : Packet
		{
			public BountyEntry(Mobile killer, int maxGold) : base(0xAB)
			{
				string prompt = $"Do you wish to place a bounty on the head of {killer.Name}?";
				string subText = $"({maxGold}gp max.)";

				EnsureCapacity(1 + 2 + 4 + 1 + 1 + 2 + prompt.Length + 1 + 1 + 1 + 4 + 2 + subText.Length + 1);

				m_Stream.Write((int)killer.Serial); // id
				m_Stream.Write((byte)0); // 'typeid'
				m_Stream.Write((byte)0); // 'index'

				m_Stream.Write((short)(prompt.Length + 1)); // textlen 
				m_Stream.WriteAsciiNull(prompt);

				m_Stream.Write(true); // enable cancel btn
				m_Stream.Write((byte)2); // style, 0=disable, 1=normal, 2=numeric
				m_Stream.Write(maxGold); // 'format' when style=1 format=maxlen, style=2 format=max # val

				m_Stream.Write((short)(subText.Length + 1));
				m_Stream.WriteAsciiNull(subText);
			}
		}

		private static void BountyEntryResponse(NetState ns, PacketReader pvSrc)
		{
			Mobile from = ns.Mobile;
			if (from == null)
				return;
			Mobile killer = World.FindMobile((Serial)pvSrc.ReadInt32());
			_ = pvSrc.ReadByte();
			_ = pvSrc.ReadByte();
			bool cancel = pvSrc.ReadByte() == 0;
			_ = pvSrc.ReadInt16();
			string resp = pvSrc.ReadString();

			if (killer != null && !cancel)
			{
				int bounty = Utility.ToInt32(resp);
				if (bounty > 5000)
					bounty = 5000;
				bounty = from.BankBox.ConsumeUpTo(typeof(Gold), bounty, true);

				killer.Kills++;
				if (killer is PlayerMobile mobile && bounty > 0)
				{
					mobile.Bounty += bounty;
					killer.SendAsciiMessage("{0} has placed a bounty of {1}gp on your head!", from.Name, bounty);
					if (mobile.Bounty >= 5000 && mobile.Kills > 1 && mobile.BankBox.Items.Count > 0 && mobile.Karma <= Notoriety.Murderer)
					{
						killer.SayTo(killer, true, "A bounty hath been issued for thee, and thy worldly goods are hereby confiscated!");
						mobile.Bounty += EmptyAndGetGold(killer.BankBox.Items);
					}
				}
			}
		}

		private static int GetPriceFor(Item item)
		{
			int price = 0;

			if (item is Gold)
			{
				price = item.Amount;
			}
			else if (item is BaseArmor armor)
			{
				if (armor.Quality == ItemQuality.Low)
					price = (int)(price * 0.75);
				else if (armor.Quality == ItemQuality.Exceptional)
					price = (int)(price * 1.25);

				price += 100 * (int)armor.Durability;

				price += 100 * (int)armor.ProtectionLevel;
			}
			else if (item is BaseWeapon weapon)
			{
				if (weapon.Quality == ItemQuality.Low)
					price = (int)(price * 0.60);
				else if (weapon.Quality == ItemQuality.Exceptional)
					price = (int)(price * 1.25);

				price += 100 * (int)weapon.DurabilityLevel;

				price += 100 * (int)weapon.DamageLevel;
			}
			else if (item is BaseBeverage beverage)
			{
				int price1 = price, price2 = price;

				if (item is Pitcher)
				{ price1 = 3; price2 = 5; }
				else if (item is BeverageBottle)
				{ price1 = 3; price2 = 3; }
				else if (item is Jug)
				{ price1 = 6; price2 = 6; }

				BaseBeverage bev = beverage;

				if (bev.IsEmpty || bev.Content == BeverageType.Milk)
					price = price1;
				else
					price = price2;
			}
			else
			{
				price = Utility.RandomMinMax(10, 50);
			}

			if (price < 1)
				price = 1;

			return price;
		}

		private static int EmptyAndGetGold(List<Item> items)
		{
			int gold = 0;
			ArrayList myList = new(items);
			for (int i = 0; i < myList.Count; i++)
			{
				Item item = (Item)myList[i];
				if (item.Items.Count > 0)
					gold += EmptyAndGetGold(item.Items);
				gold += GetPriceFor(item);
				item.Delete();
			}

			return gold;
		}

		private class GumpTimer : Timer
		{
			private readonly Mobile m_Victim;
			private readonly List<Mobile> m_Killers;

			public GumpTimer(Mobile victim, List<Mobile> killers) : base(TimeSpan.FromSeconds(4.0))
			{
				m_Victim = victim;
				m_Killers = killers;
			}

			protected override void OnTick()
			{
				m_Victim.SendGump(new ReportMurdererGump(m_Victim, m_Killers));
			}
		}

		public ReportMurdererGump(Mobile victum, List<Mobile> killers) : this(victum, killers, 0)
		{
		}

		private ReportMurdererGump(Mobile victum, List<Mobile> killers, int idx) : base(0, 0)
		{
			m_Killers = killers;
			m_Victum = victum;
			m_Idx = idx;
			BuildGump();
		}

		private void BuildGump()
		{
			AddBackground(265, 205, 320, 290, 5054);
			Closable = false;
			Resizable = false;

			AddPage(0);

			AddImageTiled(225, 175, 50, 45, 0xCE);   //Top left corner
			AddImageTiled(267, 175, 315, 44, 0xC9);  //Top bar
			AddImageTiled(582, 175, 43, 45, 0xCF);   //Top right corner
			AddImageTiled(225, 219, 44, 270, 0xCA);  //Left side
			AddImageTiled(582, 219, 44, 270, 0xCB);  //Right side
			AddImageTiled(225, 489, 44, 43, 0xCC);   //Lower left corner
			AddImageTiled(267, 489, 315, 43, 0xE9);  //Lower Bar
			AddImageTiled(582, 489, 43, 43, 0xCD);   //Lower right corner

			AddPage(1);

			AddHtml(260, 234, 300, 140, m_Killers[m_Idx].Name, false, false); // Player's Name
			AddHtmlLocalized(260, 254, 300, 140, 1049066, false, false); // Would you like to report...

			AddButton(260, 300, 0xFA5, 0xFA7, 1, GumpButtonType.Reply, 0);
			AddHtmlLocalized(300, 300, 300, 50, 1046362, false, false); // Yes

			AddButton(360, 300, 0xFA5, 0xFA7, 2, GumpButtonType.Reply, 0);
			AddHtmlLocalized(400, 300, 300, 50, 1046363, false, false); // No
		}

		public static void ReportedListExpiry_Callback(object state)
		{
			object[] states = (object[])state;

			PlayerMobile from = (PlayerMobile)states[0];
			Mobile killer = (Mobile)states[1];

			if (from.RecentlyReported.Contains(killer))
			{
				from.RecentlyReported.Remove(killer);
			}
		}

		public override void OnResponse(NetState state, RelayInfo info)
		{
			Mobile from = state.Mobile;

			switch (info.ButtonID)
			{
				case 1:
					{
						Mobile killer = m_Killers[m_Idx];
						if (killer != null && !killer.Deleted)
						{
							killer.Kills++;
							killer.ShortTermMurders++;
							if (BountyEnabled)
							{
								Item[] gold = from.BankBox.FindItemsByType(typeof(Gold), true);
								int total = 0;
								for (int i = 0; i < gold.Length && total < 5000; i++)
									total += gold[i].Amount;
								if (total > 5000)
									total = 5000;

								from.Send(new BountyEntry(killer, total));
							}

							if (Core.SE)
							{
								((PlayerMobile)from).RecentlyReported.Add(killer);
								Timer.DelayCall(ReportExpirationTimeout, new TimerStateCallback(ReportedListExpiry_Callback), new object[] { from, killer });
							}

							if (killer is PlayerMobile pk)
							{
								pk.ResetKillTime();
								pk.SendLocalizedMessage(1049067);//You have been reported for murder!

								if (pk.Kills == Mobile.MurderKills)
								{
									pk.SendLocalizedMessage(502134);//You are now known as a murderer!
								}
								else if (SkillHandlers.Stealing.SuspendOnMurder && pk.Kills == 1 && pk.NpcGuild == NpcGuild.ThievesGuild)
								{
									pk.SendLocalizedMessage(501562); // You have been suspended by the Thieves Guild.
								}
							}

						}
						break;
					}
				case 2:
					{
						break;
					}
			}

			m_Idx++;
			if (m_Idx < m_Killers.Count)
				from.SendGump(new ReportMurdererGump(from, m_Killers, m_Idx));
		}
	}
}
