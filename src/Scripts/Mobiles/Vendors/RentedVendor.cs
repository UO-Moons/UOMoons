using Server.ContextMenus;
using Server.Gumps;
using Server.Misc;
using Server.Multis;
using Server.Prompts;
using System;
using System.Collections.Generic;

namespace Server.Mobiles;

public class VendorRentalDuration
{
	public static readonly VendorRentalDuration[] Instances = {
		new( TimeSpan.FromDays(  7.0 ), 1062361 ),	// 1 Week
		new( TimeSpan.FromDays( 14.0 ), 1062362 ),	// 2 Weeks
		new( TimeSpan.FromDays( 21.0 ), 1062363 ),	// 3 Weeks
		new( TimeSpan.FromDays( 28.0 ), 1062364 )	// 1 Month
	};

	public TimeSpan Duration { get; }
	public int Name { get; }

	public int Id
	{
		get
		{
			for (var i = 0; i < Instances.Length; i++)
			{
				if (Instances[i] == this)
					return i;
			}

			return 0;
		}
	}

	private VendorRentalDuration(TimeSpan duration, int name)
	{
		Duration = duration;
		Name = name;
	}
}

public class RentedVendor : PlayerVendor
{
	private Timer _mRentalExpireTimer;

	public RentedVendor(Mobile owner, BaseHouse house, VendorRentalDuration duration, int rentalPrice, bool landlordRenew, int rentalGold) : base(owner, house)
	{
		RentalDuration = duration;
		RentalPrice = RenewalPrice = rentalPrice;
		LandlordRenew = landlordRenew;
		RenterRenew = false;

		RentalGold = rentalGold;

		RentalExpireTime = DateTime.UtcNow + duration.Duration;
		_mRentalExpireTimer = new RentalExpireTimer(this, duration.Duration);
		_mRentalExpireTimer.Start();
	}

	public RentedVendor(Serial serial) : base(serial)
	{
	}

	public VendorRentalDuration RentalDuration { get; private set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int RentalPrice { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public bool LandlordRenew { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public bool RenterRenew { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public bool Renew => LandlordRenew && RenterRenew && House != null && House.DecayType != DecayType.Condemned;

	[CommandProperty(AccessLevel.GameMaster)]
	public int RenewalPrice { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int RentalGold { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public DateTime RentalExpireTime { get; private set; }

	public override bool IsOwner(Mobile m)
	{
		return m == Owner || m.AccessLevel >= AccessLevel.GameMaster || (Core.ML && AccountHandler.CheckAccount(m, Owner));
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public Mobile Landlord
	{
		get
		{
			if (House != null)
				return House.Owner;

			return null;
		}
	}

	public bool IsLandlord(Mobile m)
	{
		return House != null && House.IsOwner(m);
	}

	public void ComputeRentalExpireDelay(out int days, out int hours)
	{
		TimeSpan delay = RentalExpireTime - DateTime.UtcNow;

		if (delay <= TimeSpan.Zero)
		{
			days = 0;
			hours = 0;
		}
		else
		{
			days = delay.Days;
			hours = delay.Hours;
		}
	}

	public void SendRentalExpireMessage(Mobile to)
	{
		ComputeRentalExpireDelay(out var days, out var hours);

		to.SendLocalizedMessage(1062464, days + "\t" + hours); // The rental contract on this vendor will expire in ~1_DAY~ day(s) and ~2_HOUR~ hour(s).
	}

	public override void OnAfterDelete()
	{
		base.OnAfterDelete();

		_mRentalExpireTimer.Stop();
	}

	public override void Destroy(bool toBackpack)
	{
		if (RentalGold > 0 && House is {IsAosRules: true})
		{
			House.MovingCrate ??= new MovingCrate(House);

			Banker.Deposit(House.MovingCrate, RentalGold);
			RentalGold = 0;
		}

		base.Destroy(toBackpack);
	}

	public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
	{
		if (from.Alive)
		{
			if (IsOwner(from))
			{
				list.Add(new ContractOptionsEntry(this));
			}
			else if (IsLandlord(from))
			{
				if (RentalGold > 0)
					list.Add(new CollectRentEntry(this));

				list.Add(new TerminateContractEntry(this));
				list.Add(new ContractOptionsEntry(this));
			}
		}

		base.GetContextMenuEntries(from, list);
	}

	private class ContractOptionsEntry : ContextMenuEntry
	{
		private readonly RentedVendor _mVendor;

		public ContractOptionsEntry(RentedVendor vendor) : base(6209)
		{
			_mVendor = vendor;
		}

		public override void OnClick()
		{
			Mobile from = Owner.From;

			if (_mVendor.Deleted || !from.CheckAlive())
				return;

			if (_mVendor.IsOwner(from))
			{
				from.CloseGump(typeof(RenterVendorRentalGump));
				from.SendGump(new RenterVendorRentalGump(_mVendor));

				_mVendor.SendRentalExpireMessage(from);
			}
			else if (_mVendor.IsLandlord(from))
			{
				from.CloseGump(typeof(LandlordVendorRentalGump));
				from.SendGump(new LandlordVendorRentalGump(_mVendor));

				_mVendor.SendRentalExpireMessage(from);
			}
		}
	}

	private class CollectRentEntry : ContextMenuEntry
	{
		private readonly RentedVendor _mVendor;

		public CollectRentEntry(RentedVendor vendor) : base(6212)
		{
			_mVendor = vendor;
		}

		public override void OnClick()
		{
			Mobile from = Owner.From;

			if (_mVendor.Deleted || !from.CheckAlive() || !_mVendor.IsLandlord(from))
				return;

			if (_mVendor.RentalGold > 0)
			{
				int depositedGold = Banker.DepositUpTo(from, _mVendor.RentalGold);
				_mVendor.RentalGold -= depositedGold;

				if (depositedGold > 0)
					from.SendLocalizedMessage(1060397, depositedGold.ToString()); // ~1_AMOUNT~ gold has been deposited into your bank box.

				if (_mVendor.RentalGold > 0)
					from.SendLocalizedMessage(500390); // Your bank box is full.
			}
		}
	}

	private class TerminateContractEntry : ContextMenuEntry
	{
		private readonly RentedVendor _mVendor;

		public TerminateContractEntry(RentedVendor vendor) : base(6218)
		{
			_mVendor = vendor;
		}

		public override void OnClick()
		{
			Mobile from = Owner.From;

			if (_mVendor.Deleted || !from.CheckAlive() || !_mVendor.IsLandlord(from))
				return;

			from.SendLocalizedMessage(1062503); // Enter the amount of gold you wish to offer the renter in exchange for immediate termination of this contract?
			from.Prompt = new RefundOfferPrompt(_mVendor);
		}
	}

	private class RefundOfferPrompt : Prompt
	{
		private readonly RentedVendor _mVendor;

		public RefundOfferPrompt(RentedVendor vendor)
		{
			_mVendor = vendor;
		}

		public override void OnResponse(Mobile from, string text)
		{
			if (!_mVendor.CanInteractWith(from, false) || !_mVendor.IsLandlord(from))
				return;

			text = text.Trim();


			if (!int.TryParse(text, out int amount))
				amount = -1;

			Mobile owner = _mVendor.Owner;
			if (owner == null)
				return;

			if (amount < 0)
			{
				from.SendLocalizedMessage(1062506); // You did not enter a valid amount.  Offer canceled.
			}
			else if (Banker.GetBalance(from) < amount)
			{
				from.SendLocalizedMessage(1062507); // You do not have that much money in your bank account.
			}
			else if (owner.Map != _mVendor.Map || !owner.InRange(_mVendor, 5))
			{
				from.SendLocalizedMessage(1062505); // The renter must be closer to the vendor in order for you to make this offer.
			}
			else
			{
				from.SendLocalizedMessage(1062504); // Please wait while the renter considers your offer.

				owner.CloseGump(typeof(VendorRentalRefundGump));
				owner.SendGump(new VendorRentalRefundGump(_mVendor, from, amount));
			}
		}
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.WriteEncodedInt(0); // version

		writer.WriteEncodedInt(RentalDuration.Id);

		writer.Write(RentalPrice);
		writer.Write(LandlordRenew);
		writer.Write(RenterRenew);
		writer.Write(RenewalPrice);

		writer.Write(RentalGold);

		writer.WriteDeltaTime(RentalExpireTime);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadEncodedInt();

		int durationId = reader.ReadEncodedInt();
		RentalDuration = durationId < VendorRentalDuration.Instances.Length ? VendorRentalDuration.Instances[durationId] : VendorRentalDuration.Instances[0];

		RentalPrice = reader.ReadInt();
		LandlordRenew = reader.ReadBool();
		RenterRenew = reader.ReadBool();
		RenewalPrice = reader.ReadInt();

		RentalGold = reader.ReadInt();

		RentalExpireTime = reader.ReadDeltaTime();

		TimeSpan delay = RentalExpireTime - DateTime.UtcNow;
		_mRentalExpireTimer = new RentalExpireTimer(this, delay > TimeSpan.Zero ? delay : TimeSpan.Zero);
		_mRentalExpireTimer.Start();
	}

	private class RentalExpireTimer : Timer
	{
		private readonly RentedVendor _mVendor;

		public RentalExpireTimer(RentedVendor vendor, TimeSpan delay) : base(delay, vendor.RentalDuration.Duration)
		{
			_mVendor = vendor;

			Priority = TimerPriority.OneMinute;
		}

		protected override void OnTick()
		{
			int renewalPrice = _mVendor.RenewalPrice;

			if (_mVendor.Renew && _mVendor.HoldGold >= renewalPrice)
			{
				_mVendor.HoldGold -= renewalPrice;
				_mVendor.RentalGold += renewalPrice;

				_mVendor.RentalPrice = renewalPrice;

				_mVendor.RentalExpireTime = DateTime.UtcNow + _mVendor.RentalDuration.Duration;
			}
			else
			{
				_mVendor.Destroy(false);
			}
		}
	}
}
