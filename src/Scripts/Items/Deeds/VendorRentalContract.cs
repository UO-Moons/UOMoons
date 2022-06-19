using Server.ContextMenus;
using Server.Gumps;
using Server.Mobiles;
using Server.Multis;
using Server.Targeting;
using System;
using System.Collections.Generic;

namespace Server.Items;

public class VendorRentalContract : BaseItem
{
	public override int LabelNumber => 1062332;  // a vendor rental contract

	private VendorRentalDuration _mDuration;
	private Mobile _mOfferee;
	private Timer _mOfferExpireTimer;

	public VendorRentalDuration Duration
	{
		get => _mDuration;
		set
		{
			if (value != null)
				_mDuration = value;
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int Price { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public bool LandlordRenew { get; set; }

	public Mobile Offeree
	{
		get => _mOfferee;
		set
		{
			if (_mOfferExpireTimer != null)
			{
				_mOfferExpireTimer.Stop();
				_mOfferExpireTimer = null;
			}

			_mOfferee = value;

			if (value != null)
			{
				_mOfferExpireTimer = new OfferExpireTimer(this);
				_mOfferExpireTimer.Start();
			}

			InvalidateProperties();
		}
	}

	[Constructable]
	public VendorRentalContract() : base(0x14F0)
	{
		Weight = 1.0;
		Hue = 0x672;

		_mDuration = VendorRentalDuration.Instances[0];
		Price = 1500;
	}

	public VendorRentalContract(Serial serial) : base(serial)
	{
	}

	public override void GetProperties(ObjectPropertyList list)
	{
		base.GetProperties(list);

		if (Offeree != null)
			list.Add(1062368, Offeree.Name); // Being Offered To ~1_NAME~
	}

	public bool IsLandlord(Mobile m)
	{
		if (!IsLockedDown) return false;
		BaseHouse house = BaseHouse.FindHouseAt(this);

		if (house != null && house.DecayType != DecayType.Condemned)
			return house.IsOwner(m);

		return false;
	}

	public bool IsUsableBy(Mobile from, bool byLandlord, bool byBackpack, bool noOfferee, bool sendMessage)
	{
		if (Deleted || !from.CheckAlive(sendMessage))
			return false;

		if (noOfferee && Offeree != null)
		{
			if (sendMessage)
				from.SendLocalizedMessage(1062343); // That item is currently in use.

			return false;
		}

		if (byBackpack && IsChildOf(from.Backpack))
			return true;

		if (byLandlord && IsLandlord(from))
		{
			if (from.Map != Map || !from.InRange(this, 5))
			{
				if (sendMessage)
					from.SendLocalizedMessage(501853); // Target is too far away.

				return false;
			}

			return true;
		}

		return false;
	}

	public override void OnDelete()
	{
		if (IsLockedDown)
		{
			BaseHouse house = BaseHouse.FindHouseAt(this);

			if (house != null)
			{
				house.VendorRentalContracts.Remove(this);
			}
		}
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (Offeree != null)
		{
			from.SendLocalizedMessage(1062343); // That item is currently in use.
		}
		else if (!IsLockedDown)
		{
			if (!IsChildOf(from.Backpack))
			{
				from.SendLocalizedMessage(1062334); // This item must be in your backpack to be used.
				return;
			}

			BaseHouse house = BaseHouse.FindHouseAt(from);

			if (house == null || !house.IsOwner(from))
			{
				from.SendLocalizedMessage(1062333); // You must be standing inside of a house that you own to make use of this contract.
			}
			else if (!house.IsAosRules)
			{
				from.SendMessage("Rental contracts can only be placed in AOS-enabled houses.");
			}
			else if (!house.Public)
			{
				from.SendLocalizedMessage(1062335); // Rental contracts can only be placed in public houses.
			}
			else if (!house.CanPlaceNewVendor())
			{
				from.SendLocalizedMessage(1062352); // You do not have enought storage available to place this contract.
			}
			else
			{
				from.SendLocalizedMessage(1062337); // Target the exact location you wish to rent out.
				from.Target = new RentTarget(this);
			}
		}
		else if (IsLandlord(from))
		{
			if (from.InRange(this, 5))
			{
				from.CloseGump(typeof(VendorRentalContractGump));
				from.SendGump(new VendorRentalContractGump(this, from));
			}
			else
			{
				from.SendLocalizedMessage(501853); // Target is too far away.
			}
		}
	}

	public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
	{
		base.GetContextMenuEntries(from, list);

		if (IsUsableBy(from, true, true, true, false))
		{
			list.Add(new ContractOptionEntry(this));
		}
	}

	private class ContractOptionEntry : ContextMenuEntry
	{
		private readonly VendorRentalContract _mContract;

		public ContractOptionEntry(VendorRentalContract contract) : base(6209)
		{
			_mContract = contract;
		}

		public override void OnClick()
		{
			Mobile from = Owner.From;

			if (_mContract.IsUsableBy(from, true, true, true, true))
			{
				from.CloseGump(typeof(VendorRentalContractGump));
				from.SendGump(new VendorRentalContractGump(_mContract, from));
			}
		}
	}

	private class RentTarget : Target
	{
		private readonly VendorRentalContract _mContract;

		public RentTarget(VendorRentalContract contract) : base(-1, false, TargetFlags.None)
		{
			_mContract = contract;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (!_mContract.IsUsableBy(from, false, true, true, true))
				return;

			if (targeted is not IPoint3D location)
				return;

			Point3D pLocation = new(location);
			Map map = from.Map;

			BaseHouse house = BaseHouse.FindHouseAt(pLocation, map, 0);

			if (house == null || !house.IsOwner(from))
			{
				from.SendLocalizedMessage(1062338); // The location being rented out must be inside of your house.
			}
			else if (BaseHouse.FindHouseAt(from) != house)
			{
				from.SendLocalizedMessage(1062339); // You must be located inside of the house in which you are trying to place the contract.
			}
			else if (!house.IsAosRules)
			{
				from.SendMessage("Rental contracts can only be placed in AOS-enabled houses.");
			}
			else if (!house.Public)
			{
				from.SendLocalizedMessage(1062335); // Rental contracts can only be placed in public houses.
			}
			else if (house.DecayType == DecayType.Condemned)
			{
				from.SendLocalizedMessage(1062468); // You cannot place a contract in a condemned house.
			}
			else if (!house.CanPlaceNewVendor())
			{
				from.SendLocalizedMessage(1062352); // You do not have enough storage available to place this contract.
			}
			else if (!map.CanFit(pLocation, 16, false, false))
			{
				from.SendLocalizedMessage(1062486); // A vendor cannot exist at that location.  Please try again.
			}
			else
			{
				BaseHouse.IsThereVendor(pLocation, map, out bool vendor, out bool contract);

				if (vendor)
				{
					from.SendLocalizedMessage(1062342); // You may not place a rental contract at this location while other beings occupy it.
				}
				else if (contract)
				{
					from.SendLocalizedMessage(1062341); // That location is cluttered.  Please clear out any objects there and try again.
				}
				else
				{
					_mContract.MoveToWorld(pLocation, map);

					if (!house.LockDown(from, _mContract))
					{
						from.AddToBackpack(_mContract);
					}
				}
			}
		}

		protected override void OnTargetCancel(Mobile from, TargetCancelType cancelType)
		{
			from.SendLocalizedMessage(1062336); // You decide not to place the contract at this time.
		}
	}

	private class OfferExpireTimer : Timer
	{
		private readonly VendorRentalContract _mContract;

		public OfferExpireTimer(VendorRentalContract contract) : base(TimeSpan.FromSeconds(30.0))
		{
			_mContract = contract;

			Priority = TimerPriority.OneSecond;
		}

		protected override void OnTick()
		{
			Mobile offeree = _mContract.Offeree;

			if (offeree == null) return;
			offeree.CloseGump(typeof(VendorRentalOfferGump));

			_mContract.Offeree = null;
		}
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.WriteEncodedInt(0);
		writer.WriteEncodedInt(_mDuration.Id);
		writer.Write(Price);
		writer.Write(LandlordRenew);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		_ = reader.ReadEncodedInt();
		int durationId = reader.ReadEncodedInt();
		_mDuration = durationId < VendorRentalDuration.Instances.Length ? VendorRentalDuration.Instances[durationId] : VendorRentalDuration.Instances[0];

		Price = reader.ReadInt();
		LandlordRenew = reader.ReadBool();
	}
}
