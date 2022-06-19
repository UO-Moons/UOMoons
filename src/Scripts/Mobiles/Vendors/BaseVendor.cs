using Server.ContextMenus;
using Server.Engines.BulkOrders;
using Server.Factions;
using Server.Gumps;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Network;
using Server.Regions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.Mobiles
{
	public enum VendorShoeType
	{
		None,
		Shoes,
		Boots,
		Sandals,
		ThighBoots
	}

	public abstract class BaseVendor : BaseConvo, IVendor
	{
		public static List<BaseVendor> AllVendors { get; }

		static BaseVendor()
		{
			AllVendors = new List<BaseVendor>(0x4000);
		}

		private const int MaxSell = 500;
		public override bool ShowFameTitle => false;
		public override bool IsInvulnerable => true;
		public override bool CanTeach => true;
		public override bool BardImmune => true;
		public override bool PlayerRangeSensitive => true;
		public virtual bool IsActiveVendor => true;
		public virtual double GetMoveDelay => Utility.RandomMinMax(1, 2);
		public virtual bool ChangeRace => true;
		public virtual TimeSpan RestockDelay => TimeSpan.FromHours(1);
		private int _mBankAccount, _mBankRestock;
		public virtual bool IsTokunoVendor => Map == Map.Tokuno;
		public DateTime LastRestock { get; set; }
		public virtual bool IsActiveBuyer => IsActiveVendor;  // response to vendor SELL
		public virtual bool IsActiveSeller => IsActiveVendor;  // response to vendor BUY
		protected abstract List<SbInfo> SbInfos { get; }
		private readonly ArrayList _mArmorBuyInfo = new();
		private readonly ArrayList _mArmorSellInfo = new();
		public virtual NpcGuild NpcGuild => NpcGuild.None;
		public virtual DateTime NextTrickOrTreat { get; set; }
		public virtual bool IsValidBulkOrder(Item item) => false;
		public virtual Item CreateBulkOrder(Mobile from, bool fromContextMenu) => null;
		public virtual bool SupportsBulkOrders(Mobile from) => false;
		public virtual TimeSpan GetNextBulkOrder(Mobile from) => TimeSpan.Zero;
		public virtual void OnSuccessfulBulkOrderReceive(Mobile from) {}

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int BankAccount
		{
			get => _mBankAccount;
			set => _mBankAccount = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int BankRestockAmount
		{
			get => _mBankRestock;
			set => _mBankRestock = value;
		}

		protected override void GetConvoFragments(ArrayList list)
		{
			list.Add((int)JobFragment.shopkeep);
			base.GetConvoFragments(list);
		}

		#region Faction
		public virtual int GetPriceScalar()
		{
			Town town = Town.FromRegion(Region);

			if (town != null)
				return 100 + town.Tax;

			return 100;
		}

		public void UpdateBuyInfo()
		{
			int priceScalar = GetPriceScalar();

			IBuyItemInfo[] buyinfo = (IBuyItemInfo[])_mArmorBuyInfo.ToArray(typeof(IBuyItemInfo));

			foreach (IBuyItemInfo info in buyinfo)
				info.PriceScalar = priceScalar;
		}
		#endregion

		public BaseVendor(string title)
			: base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
		{
			LoadSbInfo();

			Title = title;
			InitBody();
			InitOutfit();

			Container pack;
			//these packs MUST exist, or the client will crash when the packets are sent
			pack = new Backpack
			{
				Layer = Layer.ShopBuy,
				Movable = false,
				Visible = false
			};
			AddItem(pack);

			pack = new Backpack
			{
				Layer = Layer.ShopResale,
				Movable = false,
				Visible = false
			};
			AddItem(pack);
			_mBankAccount = _mBankRestock = 1000;
			LastRestock = DateTime.UtcNow;
		}

		public BaseVendor(Serial serial)
			: base(serial)
		{
		}

		public Container BuyPack
		{
			get
			{
				if (FindItemOnLayer(Layer.ShopBuy) is Container pack) return pack;
				pack = new Backpack
				{
					Layer = Layer.ShopBuy,
					Visible = false
				};
				AddItem(pack);

				return pack;
			}
		}

		public abstract void InitSbInfo();
		protected void LoadSbInfo()
		{
			LastRestock = DateTime.UtcNow;

			for (var i = 0; i < _mArmorBuyInfo.Count; ++i)
			{
				if (_mArmorBuyInfo[i] is GenericBuyInfo buy)
					buy.DeleteDisplayEntity();
			}

			SbInfos.Clear();

			InitSbInfo();

			_mArmorBuyInfo.Clear();
			_mArmorSellInfo.Clear();

			for (var i = 0; i < SbInfos.Count; i++)
			{
				SbInfo sbInfo = SbInfos[i];
				_mArmorBuyInfo.AddRange(sbInfo.BuyInfo);
				_mArmorSellInfo.Add(sbInfo.SellInfo);
			}
		}

		public virtual bool GetGender() => Utility.RandomBool();

		public virtual void InitBody()
		{
			InitStats(100, 100, 25);

			SpeechHue = Utility.RandomDyedHue();
			Hue = Utility.RandomSkinHue();

			if (Female == GetGender())
			{
				Body = 0x191;
				Name = NameList.RandomName("female");
			}
			else
			{
				Body = 0x190;
				Name = NameList.RandomName("male");
			}
		}

		public virtual int GetRandomHue()
		{
			return Utility.Random(5) switch
			{
				1 => Utility.RandomGreenHue(),
				2 => Utility.RandomRedHue(),
				3 => Utility.RandomYellowHue(),
				4 => Utility.RandomNeutralHue(),
				_ => Utility.RandomBlueHue(),
			};
		}

		public virtual int GetShoeHue() => 0.1 > Utility.RandomDouble() ? 0 : Utility.RandomNeutralHue();

		public virtual VendorShoeType ShoeType => VendorShoeType.Shoes;

		public virtual void CheckMorph()
		{
			Map map = Map;

			if (Map == Map.Tokuno)
			{
				var n = NameList.GetNameList(Female ? "tokuno female" : "tokuno male");

				if (!n.ContainsName(Name))
					TurnToTokuno();
			}

			if (map == Map.Ilshenar || Region.IsPartOf("Gargoyle City") && Body != 0x2F6 || (Hue & 0x8000) == 0)
				TurnToGargoyle();

			if (map == Map.Malas || Region.IsPartOf("Umbra") && Hue != 0x83E8)
				TurnToNecromancer();


			//if (CheckGargoyle() || CheckNecromancer())
			//{
			//	return;
			//}

			//CheckTokuno();
		}

		public virtual bool CheckTokuno()
		{
			if (Map != Map.Tokuno)
				return false;

			NameList n;

			if (Female)
				n = NameList.GetNameList("tokuno female");
			else
				n = NameList.GetNameList("tokuno male");

			if (!n.ContainsName(Name))
				TurnToTokuno();

			return true;
		}

		public virtual void TurnToTokuno()
		{
			if (Female)
				Name = NameList.RandomName("tokuno female");
			else
				Name = NameList.RandomName("tokuno male");
		}

		public virtual bool CheckGargoyle()
		{
			Map map = Map;

			if (map != Map.Ilshenar || !Region.IsPartOf("Gargoyle City"))
				return false;

			if (Body != 0x2F6 || (Hue & 0x8000) == 0)
				TurnToGargoyle();

			return true;
		}

		public virtual bool CheckNecromancer()
		{
			Map map = Map;

			if (map != Map.Malas || !Region.IsPartOf("Umbra"))
				return false;

			if (Hue != 0x83E8)
				TurnToNecromancer();

			return true;
		}

		public override void OnAfterSpawn()
		{
			CheckMorph();
		}

		protected override void OnMapChange(Map oldMap)
		{
			base.OnMapChange(oldMap);

			CheckMorph();

			LoadSbInfo();
		}

		public virtual int GetRandomNecromancerHue()
		{
			return Utility.Random(20) switch
			{
				0 => 0,
				1 => 0x4E9,
				_ => Utility.RandomList(0x485, 0x497),
			};
		}

		public virtual void TurnToNecromancer()
		{
			for (var i = 0; i < Items.Count; ++i)
			{
				Item item = Items[i];

				switch (item)
				{
					case Hair:
					case Beard:
						item.Hue = 0;
						break;
					case BaseClothing:
					case BaseWeapon:
					case BaseArmor:
					case BaseTool:
						item.Hue = GetRandomNecromancerHue();
						break;
				}
			}

			HairHue = 0;
			FacialHairHue = 0;

			Hue = 0x83E8;
		}

		public virtual void TurnToGargoyle()
		{
			for (var i = 0; i < Items.Count; ++i)
			{
				Item item = Items[i];

				if (item is BaseClothing or Hair or Beard)
					item.Delete();
			}

			HairItemID = 0;
			FacialHairItemID = 0;

			Body = 0x2F6;
			Hue = Utility.RandomBrightHue() | 0x8000;
			Name = NameList.RandomName("gargoyle vendor");

			CapitalizeTitle();
		}

		public virtual void CapitalizeTitle()
		{
			string title = Title;

			if (title == null)
				return;

			string[] split = title.Split(' ');

			for (var i = 0; i < split.Length; ++i)
			{
				if (Insensitive.Equals(split[i], "the"))
					continue;

				split[i] = split[i].Length switch
				{
					> 1 => char.ToUpper(split[i][0]) + split[i][1..],
					> 0 => char.ToUpper(split[i][0]).ToString(),
					_ => split[i]
				};
			}

			Title = string.Join(" ", split);
		}

		public virtual int GetHairHue() => Utility.RandomHairHue();

		public virtual void InitOutfit()
		{
			switch (Utility.Random(3))
			{
				case 0: AddItem(new FancyShirt(GetRandomHue())); break;
				case 1: AddItem(new Doublet(GetRandomHue())); break;
				case 2: AddItem(new Shirt(GetRandomHue())); break;
			}

			switch (ShoeType)
			{
				case VendorShoeType.Shoes: AddItem(new Shoes(GetShoeHue())); break;
				case VendorShoeType.Boots: AddItem(new Boots(GetShoeHue())); break;
				case VendorShoeType.Sandals: AddItem(new Sandals(GetShoeHue())); break;
				case VendorShoeType.ThighBoots: AddItem(new ThighBoots(GetShoeHue())); break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			int hairHue = GetHairHue();

			Utility.AssignRandomHair(this, hairHue);
			Utility.AssignRandomFacialHair(this, hairHue);

			if (Female)
			{
				switch (Utility.Random(6))
				{
					case 0: AddItem(new ShortPants(GetRandomHue())); break;
					case 1:
					case 2: AddItem(new Kilt(GetRandomHue())); break;
					case 3:
					case 4:
					case 5: AddItem(new Skirt(GetRandomHue())); break;
				}
			}
			else
			{
				switch (Utility.Random(2))
				{
					case 0: AddItem(new LongPants(GetRandomHue())); break;
					case 1: AddItem(new ShortPants(GetRandomHue())); break;
				}
			}

			PackGold(100, 200);
		}

		public virtual void Restock()
		{
			LastRestock = DateTime.UtcNow;
			_mBankAccount = (int)(_mBankRestock * 0.75 + Utility.Random(_mBankRestock / 4));

			if (Home != Point3D.Zero && !InRange(Home, RangeHome + 1))
			{
				Say("I do not have my goods with me here, I must return to my shop.");
				Location = Home;
			}

			IBuyItemInfo[] buyInfo = GetBuyInfo();

			foreach (var bii in buyInfo)
				bii.OnRestock();
		}

		private static readonly TimeSpan InventoryDecayTime = TimeSpan.FromHours(1.0);

		public virtual double GetSellDiscountFor(Mobile from)
		{
			return 0.75;
		}

		public virtual double GetBuyDiscountFor(Mobile from)
		{
			var scale = from.Karma / Titles.MaxKarma * 0.1;

			// inverse discounts on red npcs (bucs den)
			if (Notoriety.Compute(this, this) == Notoriety.Murderer)
				scale = -(scale / 2);

			scale = 1.0 - scale;

			if (NpcGuild != NpcGuild.None && NpcGuild != NpcGuild.MerchantsGuild && from is PlayerMobile mobile && NpcGuild == mobile.NpcGuild)
				scale -= 0.1;

			scale = scale switch
			{
				< 0.85 => 0.85,
				> 1.15 => 1.15,
				_ => scale
			};

			return scale;
		}

		public virtual void VendorBuy(Mobile from)
		{
			if (!IsActiveSeller || !from.CheckAlive())
				return;

			if (Home != Point3D.Zero && !InRange(Home, RangeHome + 5))
			{
				Say("Please allow me to return to my shop so that I might assist thee.");
				Location = Home;
				return;
			}

			if (!CheckVendorAccess(from))
			{
				Say(501522); // I shall not treat with scum like thee!
				return;
			}

			if (DateTime.UtcNow - LastRestock > RestockDelay)
				Restock();

			double discount = GetBuyDiscountFor(from);

			UpdateBuyInfo();

			IBuyItemInfo[] buyInfo = GetBuyInfo();
			IShopSellInfo[] sellInfo = GetSellInfo();

			var list = new List<BuyItemState>(buyInfo.Length);
			Container cont = BuyPack;

			List<ObjectPropertyList> opls = null;

			for (var idx = 0; idx < buyInfo.Length; idx++)
			{
				IBuyItemInfo buyItem = buyInfo[idx];

				if (buyItem.Amount <= 0 || list.Count >= 250)
					continue;

				// NOTE: Only GBI supported; if you use another implementation of IBuyItemInfo, this will crash
				GenericBuyInfo gbi = (GenericBuyInfo)buyItem;
				IEntity disp = gbi.GetDisplayEntity();

				list.Add(new BuyItemState(buyItem.Name, cont.Serial, disp?.Serial ?? 0x7FC0FFEE, buyItem.Price, buyItem.Amount, buyItem.ItemId, buyItem.Hue));

				opls ??= new List<ObjectPropertyList>();

				switch (disp)
				{
					case Item item:
						opls.Add(item.PropertyList);
						break;
					case Mobile mobile:
						opls.Add(mobile.PropertyList);
						break;
				}
			}

			List<Item> playerItems = cont.Items;

			for (var i = playerItems.Count - 1; i >= 0; --i)
			{
				if (i >= playerItems.Count)
					continue;

				Item item = playerItems[i];

				if (item.LastMoved + InventoryDecayTime <= DateTime.UtcNow)
					item.Delete();
			}

			for (var i = 0; i < playerItems.Count; ++i)
			{
				Item item = playerItems[i];

				int price = 0;
				string name = null;

				foreach (IShopSellInfo ssi in sellInfo)
				{
					if (!ssi.IsSellable(item)) continue;
					if (Core.AOS)
					{
						price = ssi.GetBuyPriceFor(item);
					}
					else
					{
						price = (int)Math.Round(ssi.GetBuyPriceFor(item) * discount);
					}
					name = ssi.GetNameFor(item);
					break;
				}

				if (name == null || list.Count >= 250) continue;
				if (price < 1)
					price = 1;

				list.Add(new BuyItemState(name, cont.Serial, item.Serial, price, item.Amount, item.ItemId, item.Hue));

				opls ??= new List<ObjectPropertyList>();

				opls.Add(item.PropertyList);
			}

			//one (not all) of the packets uses a byte to describe number of items in the list.  Osi = dumb.
			//if ( list.Count > 255 )
			//	Console.WriteLine( "Vendor Warning: Vendor {0} has more than 255 buy items, may cause client errors!", this );

			if (list.Count > 0)
			{
				list.Sort(new BuyItemStateComparer());

				SendPacksTo(from);

				NetState ns = from.NetState;

				if (ns == null)
					return;

				if (ns.ContainerGridLines)
					from.Send(new VendorBuyContent6017(list));
				else
					from.Send(new VendorBuyContent(list));

				from.Send(new VendorBuyList(this, list));

				if (ns.HighSeas)
					from.Send(new DisplayBuyListHS(this));
				else
					from.Send(new DisplayBuyList(this));

				from.Send(new MobileStatusExtended(from));//make sure their gold amount is sent
				if (Core.AOS)
				{
					if (opls != null)
					{
						for (var i = 0; i < opls.Count; ++i)
						{
							from.Send(opls[i]);
						}
					}
				}
				else
				{
					foreach (BuyItemState bis in list)
					{
						int loc;
						try { loc = Utility.ToInt32(bis.Description); }
						catch { loc = 0; }

						from.Send(loc > 500000
							? new FakeOpl(bis.MySerial, loc)
							: new FakeOpl(bis.MySerial, bis.Description));
					}
				}

				SayTo(from, 500186); // Greetings.  Have a look around.
			}
		}

		private class FakeOpl : Packet
		{
			public FakeOpl(Serial serial, int locNum) : base(0xD6)
			{
				EnsureCapacity(1 + 2 + 2 + 4 + 1 + 1 + 4 + 4 + 2 + 4);

				int hash = locNum & 0x3FFFFFF;
				hash ^= (locNum >> 26) & 0x3F;

				m_Stream.Write((short)1);
				m_Stream.Write(serial);
				m_Stream.Write((byte)0);
				m_Stream.Write((byte)0);
				m_Stream.Write(hash);

				m_Stream.Write(locNum);
				m_Stream.Write((short)0);

				m_Stream.Write(0); // terminator
			}

			private static byte[] _mBuffer = Array.Empty<byte>();
			private static readonly Encoding MEncoding = Encoding.Unicode;

			public FakeOpl(Serial serial, string desc) : base(0xD6)
			{
				int byteCount = MEncoding.GetByteCount(desc);
				if (byteCount > _mBuffer.Length)
					_mBuffer = new byte[byteCount];

				byteCount = MEncoding.GetBytes(desc, 0, desc.Length, _mBuffer, 0);

				EnsureCapacity(1 + 2 + 2 + 4 + 1 + 1 + 4 + 4 + 2 + byteCount + 4);

				int hash = 1042971 & 0x3FFFFFF;
				hash ^= (1042971 >> 26) & 0x3F;

				int code = desc.GetHashCode();
				hash ^= (code & 0x3FFFFFF);
				hash ^= (code >> 26) & 0x3F;

				m_Stream.Write((short)1);
				m_Stream.Write(serial);
				m_Stream.Write((byte)0);
				m_Stream.Write((byte)0);
				m_Stream.Write(hash);

				m_Stream.Write(1042971);

				m_Stream.Write((short)byteCount);
				m_Stream.Write(_mBuffer, 0, byteCount);

				m_Stream.Write(0); // terminator
			}
		}

		public virtual void SendPacksTo(Mobile from)
		{
			Item pack = FindItemOnLayer(Layer.ShopBuy);

			if (pack == null)
			{
				pack = new Backpack
				{
					Layer = Layer.ShopBuy,
					Movable = false,
					Visible = false
				};
				AddItem(pack);
			}

			from.Send(new EquipUpdate(pack));

			pack = FindItemOnLayer(Layer.ShopSell);

			if (pack != null)
				from.Send(new EquipUpdate(pack));

			pack = FindItemOnLayer(Layer.ShopResale);

			if (pack == null)
			{
				pack = new Backpack
				{
					Layer = Layer.ShopResale,
					Movable = false,
					Visible = false
				};
				AddItem(pack);
			}

			from.Send(new EquipUpdate(pack));
		}

		public virtual void VendorSell(Mobile from)
		{
			if (!IsActiveBuyer || !from.CheckAlive())
				return;

			if (Home != Point3D.Zero && !InRange(Home, RangeHome + 5))
			{
				Say("Please allow me to return to my shop so that I might assist thee.");
				Location = Home;
				return;
			}

			double discount = GetSellDiscountFor(from);

			if (!CheckVendorAccess(from))
			{
				Say(501522); // I shall not treat with scum like thee!
				return;
			}

			if (DateTime.UtcNow - LastRestock > RestockDelay)
				Restock();// restocks the bank account too so must do it on sell also

			Container pack = from.Backpack;

			bool noMoney = false;

			if (pack == null) return;
			IShopSellInfo[] info = GetSellInfo();

			Dictionary<Item, SellItemState> table = new();

			foreach (IShopSellInfo ssi in info)
			{
				Item[] items = pack.FindItemsByType(ssi.Types);

				foreach (Item item in items)
				{
					if (item is Container container && container.Items.Count != 0)
						continue;

					if (!item.IsStandardLoot() || !item.Movable || !ssi.IsSellable(item)) continue;
					if (Core.AOS)
					{
						table[item] = new SellItemState(item, ssi.GetSellPriceFor(item), ssi.GetNameFor(item));
					}
					else
					{
						int price = (int)Math.Round(ssi.GetSellPriceFor(item) * discount);

						if (price < 1)
							price = 1;

						if (price <= _mBankAccount)
							table[item] = new SellItemState(item, price, ssi.GetNameFor(item));
						else
							noMoney = true;
					}
				}
			}
			if (Core.AOS)
			{
				if (table.Count > 0)
				{
					SendPacksTo(from);
					from.Send(new VendorSellList(this, table.Values));
				}
				else
				{
					Say(true, "You have nothing I would be interested in.");
				}
			}
			else
			{
				if (table.Count > 0)
				{
					SendPacksTo(from);

					from.Send(new VendorSellList(this, table.Values));

					foreach (SellItemState sis in table.Values)
					{
						int loc;
						try { loc = Utility.ToInt32(sis.Name); }
						catch { loc = 0; }

						if (loc > 500000)
							from.Send(new FakeOpl(sis.Item.Serial, loc));
						else
							from.Send(new FakeOpl(sis.Item.Serial, sis.Name));
					}
				}
				else
				{
					Say(true,
						noMoney
							? "I don't have enough money to buy anything right now."
							: "You have nothing I would be interested in.");
				}

			}
		}

		public override bool OnDragDrop(Mobile from, Item dropped)
		{
			#region Honesty Item Check
			var honestySocket = dropped.GetSocket<HonestyItemSocket>();

			if (honestySocket != null)
			{
				bool gainedPath = false;

				if (honestySocket.HonestyOwner == this)
				{
					VirtueHelper.Award(from, VirtueName.Honesty, 120, ref gainedPath);
					from.SendMessage(gainedPath ? "You have gained a path in Honesty!" : "You have gained in Honesty.");
					SayTo(from, 1074582); //Ah!  You found my property.  Thank you for your honesty in returning it to me.
					dropped.Delete();
					return true;
				}

				SayTo(from, 501550, 0x3B2); // I am not interested in this.
				return false;
			}
			#endregion
			/* TODO: Thou art giving me? and fame/karma for gold gifts */
			if (ConvertsMageArmor && dropped is BaseArmor armor && CheckConvertArmor(from, armor))
			{
				return false;
			}

			if (dropped is not SmallBOD && dropped is not LargeBOD) return base.OnDragDrop(from, dropped);
			PlayerMobile pm = from as PlayerMobile;

			if (Core.ML && pm != null && pm.NextBodTurnInTime > DateTime.UtcNow)
			{
				SayTo(from, 1079976); // You'll have to wait a few seconds while I inspect the last order.
				return false;
			}

			if (!IsValidBulkOrder(dropped))
			{
				SayTo(from, 1045130); // That order is for some other shopkeeper.
				return false;
			}

			if (dropped is SmallBOD {Complete: false} or LargeBOD {Complete: false})
			{
				SayTo(from, 1045131); // You have not completed the order yet.
				return false;
			}

			Item reward;
			int gold, fame;

			if (dropped is SmallBOD bOd)
				bOd.GetRewards(out reward, out gold, out fame);
			else
				((LargeBOD)dropped).GetRewards(out reward, out gold, out fame);

			from.SendSound(0x3D);

			SayTo(from, 1045132); // Thank you so much!  Here is a reward for your effort.

			if (reward != null)
				from.AddToBackpack(reward);

			switch (gold)
			{
				case > 1000:
					from.AddToBackpack(new BankCheck(gold));
					break;
				case > 0:
					from.AddToBackpack(new Gold(gold));
					break;
			}

			Titles.AwardFame(from, fame, true);

			OnSuccessfulBulkOrderReceive(from);

			if (Core.ML && pm != null)
				pm.NextBodTurnInTime = DateTime.UtcNow + TimeSpan.FromSeconds(10.0);

			dropped.Delete();
			return true;

		}

		private GenericBuyInfo LookupDisplayObject(object obj)
		{
			IBuyItemInfo[] buyInfo = GetBuyInfo();

			return buyInfo.Cast<GenericBuyInfo>().FirstOrDefault(gbi => gbi.GetDisplayEntity() == obj);
		}

		private static void ProcessSinglePurchase(BuyItemResponse buy, IBuyItemInfo bii, List<BuyItemResponse> validBuy, ref int controlSlots, ref bool fullPurchase, ref int totalCost, ref double discount)
		{
			int amount = buy.Amount;

			if (amount > bii.Amount)
				amount = bii.Amount;

			if (amount <= 0)
				return;

			int slots = bii.ControlSlots * amount;

			if (controlSlots >= slots)
			{
				controlSlots -= slots;
			}
			else
			{
				fullPurchase = false;
				return;
			}

			if (Core.AOS)
			{
				totalCost += bii.Price * amount;
				validBuy.Add(buy);
			}
			else
			{
				int price = (int)Math.Round(bii.Price * discount);
				if (price < 1)
					price = 1;
				totalCost += price * amount;
				validBuy.Add(buy);
			}
		}

		private static void ProcessValidPurchase(int amount, IBuyItemInfo bii, Mobile buyer, Container cont)
		{
			if (amount > bii.Amount)
				amount = bii.Amount;

			if (amount < 1)
				return;

			bii.Amount -= amount;

			IEntity o = bii.GetEntity();

			switch (o)
			{
				case Item item when item.Stackable:
				{
					item.Amount = amount;

					if (cont == null || !cont.TryDropItem(buyer, item, false))
						item.MoveToWorld(buyer.Location, buyer.Map);
					break;
				}
				case Item item:
				{
					item.Amount = 1;

					if (cont == null || !cont.TryDropItem(buyer, item, false))
						item.MoveToWorld(buyer.Location, buyer.Map);

					for (var i = 1; i < amount; i++)
					{
						item = bii.GetEntity() as Item;

						if (item == null) continue;
						item.Amount = 1;

						if (cont == null || !cont.TryDropItem(buyer, item, false))
							item.MoveToWorld(buyer.Location, buyer.Map);
					}

					break;
				}
				case Mobile m:
				{
					m.Direction = (Direction)Utility.Random(8);
					m.MoveToWorld(buyer.Location, buyer.Map);
					m.PlaySound(m.GetIdleSound());

					if (m is BaseCreature creature)
					{
						creature.SetControlMaster(buyer);
						creature.ControlOrder = OrderType.Stop;
					}

					for (var i = 1; i < amount; ++i)
					{
						m = bii.GetEntity() as Mobile;

						if (m == null) continue;
						m.Direction = (Direction)Utility.Random(8);
						m.MoveToWorld(buyer.Location, buyer.Map);

						if (m is not BaseCreature creature1) continue;
						creature1.SetControlMaster(buyer);
						creature1.ControlOrder = OrderType.Stop;
					}

					break;
				}
			}
		}

		public virtual bool OnBuyItems(Mobile buyer, List<BuyItemResponse> list)
		{
			if (!IsActiveSeller)
				return false;

			if (!buyer.CheckAlive())
				return false;

			if (!CheckVendorAccess(buyer))
			{
				Say(501522); // I shall not treat with scum like thee!
				return false;
			}

			UpdateBuyInfo();
			_ = GetBuyInfo();
			IShopSellInfo[] info = GetSellInfo();
			int totalCost = 0;
			List<BuyItemResponse> validBuy = new(list.Count);
			bool fromBank = false;
			bool fullPurchase = true;
			int controlSlots = buyer.FollowersMax - buyer.Followers;
			double discount = GetBuyDiscountFor(buyer);

			foreach (BuyItemResponse buy in list)
			{
				Serial ser = buy.Serial;
				int amount = buy.Amount;

				if (ser.IsItem)
				{
					Item item = World.FindItem(ser);

					if (item == null)
						continue;

					GenericBuyInfo gbi = LookupDisplayObject(item);

					if (gbi != null)
					{
						ProcessSinglePurchase(buy, gbi, validBuy, ref controlSlots, ref fullPurchase, ref totalCost, ref discount);
					}
					else if (item != BuyPack && item.IsChildOf(BuyPack))
					{
						if (amount > item.Amount)
							amount = item.Amount;

						if (amount <= 0)
							continue;

						foreach (IShopSellInfo ssi in info)
						{
							if (!ssi.IsSellable(item)) continue;
							if (!ssi.IsResellable(item)) continue;
							if (Core.AOS)
							{
								totalCost += ssi.GetBuyPriceFor(item) * amount;
								validBuy.Add(buy);
								break;
							}
							else
							{
								int price = (int)Math.Round(ssi.GetBuyPriceFor(item) * discount);
								if (price < 1)
									price = 1;
								totalCost += price * amount;
								validBuy.Add(buy);
								break;
							}
						}
					}
				}
				else if (ser.IsMobile)
				{
					Mobile mob = World.FindMobile(ser);

					if (mob == null)
						continue;

					GenericBuyInfo gbi = LookupDisplayObject(mob);

					if (gbi != null)
						ProcessSinglePurchase(buy, gbi, validBuy, ref controlSlots, ref fullPurchase, ref totalCost, ref discount);
				}
			}//foreach

			if (fullPurchase && validBuy.Count == 0)
				SayTo(buyer, 500190); // Thou hast bought nothing!
			else if (validBuy.Count == 0)
				SayTo(buyer, 500187); // Your order cannot be fulfilled, please try again.

			if (validBuy.Count == 0)
				return false;

			bool bought = buyer.AccessLevel >= AccessLevel.GameMaster;

			var cont = buyer.Backpack;
			if (!bought && cont != null)
			{
				if (cont.ConsumeTotal(typeof(Gold), totalCost))
					bought = true;
				else if (totalCost < 2000)
					SayTo(buyer, 500192); // Begging thy pardon, but thou canst not afford that.
			}

			if (!bought && totalCost >= 2000)
			{
				cont = buyer.FindBankNoCreate();
				if (cont != null && cont.ConsumeTotal(typeof(Gold), totalCost))
				{
					bought = true;
					fromBank = true;
				}
				else
				{
					SayTo(buyer, 500191); //Begging thy pardon, but thy bank account lacks these funds.
				}
			}

			if (!bought)
				return false;
			else
				buyer.PlaySound(0x32);

			if (buyer.AccessLevel < AccessLevel.GameMaster) // dont count free purchases
				_mBankAccount += (int)(totalCost * 0.9); // gets back 90%

			cont = buyer.Backpack;
			if (cont == null)
				cont = buyer.BankBox;

			foreach (BuyItemResponse buy in validBuy)
			{
				Serial ser = buy.Serial;
				int amount = buy.Amount;

				if (amount < 1)
					continue;

				if (ser.IsItem)
				{
					Item item = World.FindItem(ser);

					if (item == null)
						continue;

					GenericBuyInfo gbi = LookupDisplayObject(item);

					if (gbi != null)
					{
						ProcessValidPurchase(amount, gbi, buyer, cont);
					}
					else
					{
						if (amount > item.Amount)
							amount = item.Amount;

						foreach (IShopSellInfo ssi in info)
						{
							if (!ssi.IsSellable(item)) continue;
							if (!ssi.IsResellable(item)) continue;
							Item buyItem;
							if (amount >= item.Amount)
							{
								buyItem = item;
							}
							else
							{
								buyItem = LiftItemDupe(item, item.Amount - amount) ?? item;
							}

							if (cont == null || !cont.TryDropItem(buyer, buyItem, false))
								buyItem.MoveToWorld(buyer.Location, buyer.Map);

							break;
						}
					}
				}
				else if (ser.IsMobile)
				{
					Mobile mob = World.FindMobile(ser);

					if (mob == null)
						continue;

					GenericBuyInfo gbi = LookupDisplayObject(mob);

					if (gbi != null)
						ProcessValidPurchase(amount, gbi, buyer, cont);
				}
			}//foreach

			if (fullPurchase)
			{
				if (buyer.IsStaff())
					SayTo(buyer, true, "I would not presume to charge thee anything.  Here are the goods you requested.");
				else if (fromBank)
					SayTo(buyer, 1151638, totalCost.ToString());//The total of your purchase is ~1_val~ gold, which has been drawn from your bank account.  My thanks for the patronage.
				else
					SayTo(buyer, 1151639, totalCost.ToString());//The total of your purchase is ~1_val~ gold.  My thanks for the patronage.
			}
			else
			{
				if (buyer.IsStaff())
					SayTo(buyer, true, "I would not presume to charge thee anything.  Unfortunately, I could not sell you all the goods you requested.");
				else if (fromBank)
					SayTo(buyer, true, "The total of thy purchase is {0} gold, which has been withdrawn from your bank account.  My thanks for the patronage.  Unfortunately, I could not sell you all the goods you requested.", totalCost);
				else
					SayTo(buyer, true, "The total of thy purchase is {0} gold.  My thanks for the patronage.  Unfortunately, I could not sell you all the goods you requested.", totalCost);
			}

			return true;
		}

		public virtual bool CheckVendorAccess(Mobile from)
		{
			GuardedRegion reg = (GuardedRegion)Region.GetRegion(typeof(GuardedRegion));

			if (reg != null && !reg.CheckVendorAccess(this, from))
				return false;

			if (Region == from.Region) return true;
			reg = (GuardedRegion)from.Region.GetRegion(typeof(GuardedRegion));

			return reg == null || reg.CheckVendorAccess(this, from);
		}

		public virtual bool OnSellItems(Mobile seller, List<SellItemResponse> list)
		{
			if (!IsActiveBuyer)
				return false;

			if (!seller.CheckAlive())
				return false;

			if (!CheckVendorAccess(seller))
			{
				Say(501522); // I shall not treat with scum like thee!
				return false;
			}

			seller.PlaySound(0x32);

			IShopSellInfo[] info = GetSellInfo();
			IBuyItemInfo[] buyInfo = GetBuyInfo();
			int giveGold = 0;
			int sold = 0;
			double discount = GetSellDiscountFor(seller);
			foreach (SellItemResponse resp in list)
			{
				if (resp.Item.RootParent != seller || resp.Amount <= 0 || !resp.Item.IsStandardLoot() || !resp.Item.Movable || (resp.Item is Container container && container.Items.Count != 0))
					continue;

				if (info.Any(ssi => ssi.IsSellable(resp.Item)))
				{
					sold++;
				}
			}

			if (sold > MaxSell)
			{
				SayTo(seller, true, "You may only sell {0} items at a time!", MaxSell);
				return false;
			}

			if (sold == 0)
			{
				return true;
			}

			bool lowMoney = false;
			foreach (SellItemResponse resp in list)
			{
				if (giveGold >= _mBankAccount)
				{
					lowMoney = true;
					break;
				}

				if (resp.Item.RootParent != seller || resp.Amount <= 0 || !resp.Item.IsStandardLoot() || !resp.Item.Movable || (resp.Item is Container container && container.Items.Count != 0))
					continue;

				foreach (IShopSellInfo ssi in info)
				{
					Container cont;
					if (Core.AOS && ssi.IsSellable(resp.Item))
					{
						int amount = resp.Amount;

						if (amount > resp.Item.Amount)
							amount = resp.Item.Amount;

						if (ssi.IsResellable(resp.Item))
						{
							bool found = false;

							foreach (IBuyItemInfo bii in buyInfo)
							{
								if (bii.Restock(resp.Item, amount))
								{
									resp.Item.Consume(amount);
									found = true;

									break;
								}
							}

							if (!found)
							{
								cont = BuyPack;

								if (amount < resp.Item.Amount)
								{
									Item item = LiftItemDupe(resp.Item, resp.Item.Amount - amount);

									if (item != null)
									{
										item.SetLastMoved();
										cont.DropItem(item);
									}
									else
									{
										resp.Item.SetLastMoved();
										cont.DropItem(resp.Item);
									}
								}
								else
								{
									resp.Item.SetLastMoved();
									cont.DropItem(resp.Item);
								}
							}
						}
						else
						{
							if (amount < resp.Item.Amount)
								resp.Item.Amount -= amount;
							else
								resp.Item.Delete();
						}

						giveGold += ssi.GetSellPriceFor(resp.Item) * amount;
						break;
					}
					else if (ssi.IsSellable(resp.Item))
					{
						int sellPrice = (int)Math.Round(ssi.GetSellPriceFor(resp.Item) * discount);
						if (sellPrice < 1)
							sellPrice = 1;

						int amount = resp.Amount;
						int maxAfford = (_mBankAccount - giveGold) / sellPrice;

						if (maxAfford <= 0)
						{
							lowMoney = true;
							break;
						}
						if (amount > resp.Item.Amount)
							amount = resp.Item.Amount;

						if (amount > maxAfford)
						{
							lowMoney = true;
							amount = maxAfford;
						}

						if (ssi.IsResellable(resp.Item))
						{
							bool found = false;

							if (buyInfo.Any(bii => bii.Restock(resp.Item, amount)))
							{
								resp.Item.Consume(amount);
								found = true;
							}

							if (!found)
							{
								cont = BuyPack;

								if (amount < resp.Item.Amount)
								{
									Item item = LiftItemDupe(resp.Item, resp.Item.Amount - amount);

									if (item != null)
									{
										item.SetLastMoved();
										cont.DropItem(item);
									}
									else
									{
										resp.Item.SetLastMoved();
										cont.DropItem(resp.Item);
									}
								}
								else
								{
									resp.Item.SetLastMoved();
									cont.DropItem(resp.Item);
								}
							}
						}
						else
						{
							if (amount < resp.Item.Amount)
								resp.Item.Amount -= amount;
							else
								resp.Item.Delete();
						}

						giveGold += ssi.GetSellPriceFor(resp.Item) * amount;
						break;
					}
				}
			}
			if (lowMoney)
				SayTo(seller, true, "Sorry, I cannot afford to buy all of that right now.");

			if (giveGold <= 0) return true;
			while (giveGold > 60000)
			{
				seller.AddToBackpack(new Gold(60000));
				giveGold -= 60000;
			}

			seller.AddToBackpack(new Gold(giveGold));

			seller.PlaySound(0x0037);//Gold dropping sound

			if (!BodSystem.Enabled || !SupportsBulkOrders(seller)) return true;
			Item bulkOrder = CreateBulkOrder(seller, false);

			switch (bulkOrder)
			{
				case LargeBOD largeBod:
					seller.SendGump(new LargeBODAcceptGump(seller, largeBod));
					break;
				case SmallBOD smallBod:
					seller.SendGump(new SmallBODAcceptGump(seller, smallBod));
					break;
			}
			//no cliloc for this?
			//SayTo( seller, true, "Thank you! I bought {0} item{1}. Here is your {2}gp.", Sold, (Sold > 1 ? "s" : ""), GiveGold );

			return true;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
			writer.Write(_mBankRestock);
			/*
			List<SBInfo> sbInfos = SBInfos;

			for (int i = 0; sbInfos != null && i < sbInfos.Count; ++i)
			{
				SBInfo sbInfo = sbInfos[i];
				List<GenericBuyInfo> buyInfo = sbInfo.BuyInfo;

				for (int j = 0; buyInfo != null && j < buyInfo.Count; ++j)
				{
					GenericBuyInfo gbi = buyInfo[j];

					int maxAmount = gbi.MaxAmount;
					int doubled = 0;

					switch (maxAmount)
					{
						case 40: doubled = 1; break;
						case 80: doubled = 2; break;
						case 160: doubled = 3; break;
						case 320: doubled = 4; break;
						case 640: doubled = 5; break;
						case 999: doubled = 6; break;
					}

					if (doubled > 0)
					{
						writer.WriteEncodedInt(1 + ((j * sbInfos.Count) + i));
						writer.WriteEncodedInt(doubled);
					}
				}
			}*/

			writer.WriteEncodedInt(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			LoadSbInfo();

			List<SbInfo> sbInfos = SbInfos;

			switch (version)
			{
				case 0:
					{
						_mBankAccount = _mBankRestock = reader.ReadInt();
						int index;

						while ((index = reader.ReadEncodedInt()) > 0)
						{
							int doubled = reader.ReadEncodedInt();

							/*if (sbInfos != null)
							{
								index -= 1;
								int sbInfoIndex = index % sbInfos.Count;
								int buyInfoIndex = index / sbInfos.Count;

								if (sbInfoIndex >= 0 && sbInfoIndex < sbInfos.Count)
								{
									SBInfo sbInfo = sbInfos[sbInfoIndex];
									List<GenericBuyInfo> buyInfo = sbInfo.BuyInfo;

									if (buyInfo != null && buyInfoIndex >= 0 && buyInfoIndex < buyInfo.Count)
									{
										GenericBuyInfo gbi = buyInfo[buyInfoIndex];

										int amount = 20;

										switch (doubled)
										{
											case 1: amount = 40; break;
											case 2: amount = 80; break;
											case 3: amount = 160; break;
											case 4: amount = 320; break;
											case 5: amount = 640; break;
											case 6: amount = 999; break;
										}

										gbi.Amount = gbi.MaxAmount = amount;
									}
								}
							}*/
						}

						break;
					}
			}

			if (IsParagon)
				IsParagon = false;
			Timer.DelayCall(TimeSpan.Zero, AfterLoad);

			if (_mBankRestock <= 0)
				_mBankRestock = 1000;
			_mBankAccount = _mBankRestock;

			if (RestockDelay.TotalMinutes >= 2)
				LastRestock += TimeSpan.FromMinutes(Utility.Random((int)RestockDelay.TotalMinutes));

			Timer.DelayCall(TimeSpan.Zero, CheckMorph);
		}

		private void AfterLoad()
		{
			if (Backpack != null)
			{
				if (Backpack.GetAmount(typeof(Gold), true) < 15)
					PackGold(15, 50);
			}
		}

		public override void AddCustomContextEntries(Mobile from, List<ContextMenuEntry> list)
		{
			if (Core.AOS && from.Alive && IsActiveVendor)
			{
				if (BodSystem.Enabled && SupportsBulkOrders(from))
					list.Add(new BulkOrderInfoEntry(from, this));

				if (IsActiveSeller)
					list.Add(new VendorBuyEntry(from, this));

				if (IsActiveBuyer)
					list.Add(new VendorSellEntry(from, this));
			}

			base.AddCustomContextEntries(from, list);
		}

		public virtual IShopSellInfo[] GetSellInfo()
		{
			return (IShopSellInfo[])_mArmorSellInfo.ToArray(typeof(IShopSellInfo));
		}

		public virtual IBuyItemInfo[] GetBuyInfo()
		{
			return (IBuyItemInfo[])_mArmorBuyInfo.ToArray(typeof(IBuyItemInfo));
		}

		#region Mage Armor Conversion
		public virtual bool ConvertsMageArmor => false;

		private readonly List<PendingConvert> _pendingConvertEntries = new();

		private bool CheckConvertArmor(Mobile from, BaseArmor armor)
		{
			var convert = GetConvert(from, armor);

			if (convert == null || from is not PlayerMobile)
				return false;

			object state = convert.Armor;

			RemoveConvertEntry(convert);
			from.CloseGump(typeof(ConfirmCallbackGump));

			from.SendGump(new ConfirmCallbackGump((PlayerMobile)from, 1049004, 1154115, state, null,
				(m, obj) =>
				{
					if (!Deleted && obj is BaseArmor ar && armor.IsChildOf(m.Backpack) && CanConvertArmor(m, ar))
					{
						if (!InRange(m.Location, 3))
						{
							m.SendLocalizedMessage(1149654); // You are too far away.
						}
						else if (!Banker.Withdraw(m, 250000, true))
						{
							m.SendLocalizedMessage(1019022); // You do not have enough gold.
						}
						else
						{
							ConvertMageArmor(m, ar);
						}
					}
				},
				(m, obj) =>
				{
					var con = GetConvert(m, armor);

					if (con != null)
					{
						RemoveConvertEntry(con);
					}
				}));

			return true;
		}

		protected virtual bool CanConvertArmor(Mobile from, BaseArmor armor)
		{
			if (armor != null && armor is not BaseShield) return true;
			from.SendLocalizedMessage(1113044); // You can't convert that.
			return false;

			//if (armor.ArmorAttributes.MageArmor == 0 &&
			//	Server.SkillHandlers.Imbuing.GetTotalMods(armor) > 4)
			//{
			//	from.SendLocalizedMessage(1154119); // This action would exceed a stat cap
			//	return false;
			//}

		}

		public void TryConvertArmor(Mobile from, BaseArmor armor)
		{
			if (!CanConvertArmor(from, armor)) return;
			from.SendLocalizedMessage(1154117); // Ah yes, I will convert this piece of armor but it's gonna cost you 250,000 gold coin. Payment is due immediately. Just hand me the armor.

			var convert = GetConvert(from, armor);

			if (convert != null)
			{
				convert.ResetTimer();
			}
			else
			{
				_pendingConvertEntries.Add(new PendingConvert(from, armor, this));
			}
		}

		public virtual void ConvertMageArmor(Mobile from, BaseArmor armor)
		{
			armor.ArmorAttributes.MageArmor = armor.ArmorAttributes.MageArmor > 0 ? 0 : 1;

			from.SendLocalizedMessage(1154118); // Your armor has been converted.
		}

		private void RemoveConvertEntry(PendingConvert convert)
		{
			_pendingConvertEntries.Remove(convert);

			convert.Timer?.Stop();
		}

		private PendingConvert GetConvert(IEntity from, BaseArmor armor)
		{
			return _pendingConvertEntries.FirstOrDefault(c => c.From == from && c.Armor == armor);
		}

		protected class PendingConvert
		{
			public Mobile From { get; set; }
			public BaseArmor Armor { get; set; }
			public BaseVendor Vendor { get; set; }

			public Timer Timer { get; set; }
			public DateTime Expires { get; set; }

			public bool Expired => DateTime.UtcNow > Expires;

			public PendingConvert(Mobile from, BaseArmor armor, BaseVendor vendor)
			{
				From = from;
				Armor = armor;
				Vendor = vendor;

				ResetTimer();
			}

			public void ResetTimer()
			{
				if (Timer != null)
				{
					Timer.Stop();
					Timer = null;
				}

				Expires = DateTime.UtcNow + TimeSpan.FromSeconds(120);

				Timer = Timer.DelayCall(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1), OnTick);
				Timer.Start();
			}

			public void OnTick()
			{
				if (Expired)
				{
					Vendor.RemoveConvertEntry(this);
				}
			}
		}
		#endregion
	}
}

namespace Server.ContextMenus
{
	public class VendorBuyEntry : ContextMenuEntry
	{
		private readonly BaseVendor _mVendor;

		public VendorBuyEntry(Mobile from, BaseVendor vendor)
			: base(6103, 8)
		{
			_mVendor = vendor;
			Enabled = vendor.CheckVendorAccess(from);
		}

		public override void OnClick()
		{
			_mVendor.VendorBuy(Owner.From);
		}
	}

	public class VendorSellEntry : ContextMenuEntry
	{
		private readonly BaseVendor _mVendor;

		public VendorSellEntry(Mobile from, BaseVendor vendor)
			: base(6104, 8)
		{
			_mVendor = vendor;
			Enabled = vendor.CheckVendorAccess(from);
		}

		public override void OnClick()
		{
			_mVendor.VendorSell(Owner.From);
		}
	}
}

namespace Server
{
	public interface IShopSellInfo
	{
		//get display name for an item
		string GetNameFor(Item item);

		//get price for an item which the player is selling
		int GetSellPriceFor(Item item);

		//get price for an item which the player is buying
		int GetBuyPriceFor(Item item);

		//can we sell this item to this vendor?
		bool IsSellable(Item item);

		//What do we sell?
		Type[] Types { get; }

		//does the vendor resell this item?
		bool IsResellable(Item item);
	}

	public interface IBuyItemInfo
	{
		//get a new instance of an object (we just bought it)
		IEntity GetEntity();

		int ControlSlots { get; }

		int PriceScalar { get; set; }

		//display price of the item
		int Price { get; }

		//display name of the item
		string Name { get; }

		//display hue
		int Hue { get; }

		//display id
		int ItemId { get; }

		//amount in stock
		int Amount { get; set; }

		//max amount in stock
		int MaxAmount { get; }

		//Attempt to restock with item, (return true if restock sucessful)
		bool Restock(Item item, int amount);

		//called when its time for the whole shop to restock
		void OnRestock();
	}
}
