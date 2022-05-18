#region References

using Server.Multis;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace Server.Engines.TownHouses
{
	public class General
	{
		public static string Version => "1.0.0.0";

		// This setting determines the suggested gold value for a single square of a home
		//  which then derives price, lockdowns and secures.
		public static int SuggestionFactor => 600;

		// This setting determines if players need License in order to rent out their property
		private static bool RequireRenterLicense => false;

		public static void Configure()
		{
			EventSink.OnWorldSave += EventSink_OnSave;
		}

		public static void Initialize()
		{
			EventSink.OnLogin += EventSink_Login;
			EventSink.OnSpeech += HandleSpeech;
			EventSink.OnServerStarted += OnStarted;
		}

		private static void OnStarted()
		{
			foreach (TownHouse house in TownHouse.AllTownHouses)
			{
				house.InitSectorDefinition();
				VersionCommand.UpdateRegion(house.ForSaleSign);
			}
		}

		private static void EventSink_OnSave()
		{
			foreach (TownHouseSign sign in new List<TownHouseSign>(TownHouseSign.AllSigns))
			{
				sign.ValidateOwnership();
			}

			foreach (TownHouse house in new List<TownHouse>(TownHouse.AllTownHouses).Where(house => house.Deleted))
			{
				TownHouse.AllTownHouses.Remove(house);
			}
		}

		private static void EventSink_Login(Mobile m)
		{
			foreach (TownHouse house in BaseHouse.GetHouses(m).OfType<TownHouse>())
			{
				house.ForSaleSign.CheckDemolishTimer();
			}
		}

		private static void HandleSpeech(SpeechEventArgs e)
		{
			var houses = new List<BaseHouse>(BaseHouse.GetHouses(e.Mobile));

			foreach (BaseHouse house in houses.Where(house => VersionCommand.RegionContains(house.Region, e.Mobile)))
			{
				if (house is TownHouse)
				{
					house.OnSpeech(e);
				}

				if (house.Owner == e.Mobile
					&& e.Speech.ToLower() == "create rental contract"
					&& CanRent(e.Mobile, house, true))
				{
					e.Mobile.AddToBackpack(new RentalContract());
					e.Mobile.SendMessage("A rental contract has been placed in your bag.");
				}

				if (house.Owner != e.Mobile || e.Speech.ToLower() != "check storage")
				{
					continue;
				}
				int count = 0;

				e.Mobile.SendMessage("You have {0} lockdowns and {1} secures available.", RemainingSecures(house),
					RemainingLocks(house));

				if ((count = AllRentalLocks(house)) != 0)
				{
					e.Mobile.SendMessage("Current rentals are using {0} of your lockdowns.", count);
				}
				if ((count = AllRentalSecures(house)) != 0)
				{
					e.Mobile.SendMessage("Current rentals are using {0} of your secures.", count);
				}
			}
		}

		private static bool CanRent(Mobile m, BaseHouse house, bool say)
		{
			if (house is TownHouse && ((TownHouse)house).ForSaleSign.PriceType != "Sale")
			{
				if (say)
				{
					m.SendMessage("You must own your property to rent it.");
				}

				return false;
			}

			if (RequireRenterLicense)
			{
				RentalLicense lic = m.Backpack.FindItemByType(typeof(RentalLicense)) as RentalLicense;

				if (lic != null && lic.Owner == null)
				{
					lic.Owner = m;
				}

				if (lic == null || lic.Owner != m)
				{
					if (say)
					{
						m.SendMessage("You must have a renter's license to rent your property.");
					}

					return false;
				}
			}

			if (EntireHouseContracted(house))
			{
				if (say)
				{
					m.SendMessage("This entire house already has a rental contract.");
				}

				return false;
			}

			if (RemainingSecures(house) >= 0 && RemainingLocks(house) >= 0)
			{
				return true;
			}

			if (say)
			{
				m.SendMessage("You don't have the storage available to rent property.");
			}

			return false;
		}

		#region Rental Info

		public static bool EntireHouseContracted(BaseHouse house)
		{
			return
				TownHouseSign.AllSigns.Where(
					item => item is RentalContract contract && house == contract.ParentHouse)
					.Any(item => ((RentalContract)item).EntireHouse);
		}

		public static bool HasContract(BaseHouse house)
		{
			return
				TownHouseSign.AllSigns.Any(
					item => item is RentalContract contract && house == contract.ParentHouse);
		}

		public static bool HasOtherContract(BaseHouse house, RentalContract contract)
		{
			return
				TownHouseSign.AllSigns.Any(
					item => item is RentalContract contract1 && item != contract && house == contract1.ParentHouse);
		}

		public static int RemainingSecures(BaseHouse house)
		{
			if (house == null)
			{
				return 0;
			}


			return (Core.AOS
				? house.GetAosMaxSecures() - house.GetAosCurSecures(out int a, out int b, out int c, out int d)
				: house.MaxSecures - house.SecureCount) - AllRentalSecures(house);
		}

		public static int RemainingLocks(BaseHouse house)
		{
			if (house == null)
			{
				return 0;
			}

			return (Core.AOS
				? house.GetAosMaxLockdowns() - house.GetAosCurLockdowns()
				: house.MaxLockDowns - house.LockDownCount) - AllRentalLocks(house);
		}

		private static int AllRentalSecures(IEntity house)
		{
			return
				TownHouseSign.AllSigns.Where(
					sign => sign is RentalContract contract && contract.ParentHouse == house)
					.Sum(sign => sign.Secures);
		}

		private static int AllRentalLocks(BaseHouse house)
		{
			return
				TownHouseSign.AllSigns.Where(
					sign => sign is RentalContract contract && contract.ParentHouse == house)
					.Sum(sign => sign.Locks);
		}

		#endregion
	}
}
