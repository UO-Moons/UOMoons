using Server.Engines.Quests;
using Server.Gumps;
using Server.Network;
using Server.Services.Community_Collections;
using System;
using System.Collections.Generic;

namespace Server.Mobiles;

public abstract class BaseCollectionMobile : BaseVendor, IComunityCollection
{
	protected override List<SbInfo> SbInfos { get; } = new();

	public override bool IsActiveVendor => false;

	#region IComunityCollection
	public abstract Collection CollectionId { get; }
	public abstract int MaxTier { get; }

	public List<CollectionItem> Donations { get; private set; }

	public List<CollectionItem> Rewards { get; private set; }

	private long _mPoints;
	private long _mStartTier;
	private long _mNextTier;
	private long _mDailyDecay;
	private int _mTier;

	[CommandProperty(AccessLevel.GameMaster)]
	public long Points
	{
		get => _mPoints;
		set
		{
			_mPoints = value;

			if (_mPoints < 0)
				_mPoints = 0;

			while (_mTier > 0 && _mPoints < PreviousTier)
				DecreaseTier();

			while (_mTier < MaxTier && _mPoints > CurrentTier)
				IncreaseTier();

			InvalidateProperties();
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public long PreviousTier
	{
		get
		{
			if (_mTier <= 2) return _mStartTier * _mTier;
			long tier = _mStartTier * 2;

			for (var i = 0; i < _mTier - 2; i++)
				tier += (i + 3) * _mNextTier;

			return tier;

		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public long CurrentTier
	{
		get
		{
			if (_mTier > 1)
				return PreviousTier + (_mTier + 1) * _mNextTier;

			return _mStartTier + _mStartTier * _mTier;
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public long StartTier
	{
		get => _mStartTier;
		set
		{
			_mStartTier = value;
			InvalidateProperties();
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public long NextTier
	{
		get => _mNextTier;
		set
		{
			_mNextTier = value;
			InvalidateProperties();
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public long DailyDecay
	{
		get => _mDailyDecay;
		set
		{
			_mDailyDecay = value;
			InvalidateProperties();
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int Tier => _mTier;
	#endregion

	private object _mDonationTitle;

	public List<List<object>> Tiers { get; private set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int DonationLabel
	{
		get => _mDonationTitle is int donationTitle ? donationTitle : 0;
		set => _mDonationTitle = value;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public string DonationString
	{
		get => _mDonationTitle is string donationTitle ? donationTitle : null;
		set => _mDonationTitle = value;
	}

	public BaseCollectionMobile(string name, string title)
		: base(title)
	{
		Name = name;
		Frozen = true;
		CantWalk = true;

		Init();

		CollectionsSystem.RegisterMobile(this);
	}

	public BaseCollectionMobile(Serial serial)
		: base(serial)
	{
	}

	public override void InitSbInfo()
	{
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (!from.Alive) return;
		if (from.NetState == null || !from.NetState.SupportsExpansion(Expansion.ML))
		{
			from.SendLocalizedMessage(1073651); // You must have Mondain's Legacy before proceeding...			
			return;
		}

		if (!MondainsLegacy.PublicDonations && (int)from.AccessLevel < (int)AccessLevel.GameMaster)
		{
			from.SendLocalizedMessage(1042753, "Public donations"); // ~1_SOMETHING~ has been temporarily disabled.
			return;
		}

		if (from.InRange(Location, 2) && from is PlayerMobile mobile && CanDonate(mobile))
		{
			mobile.CloseGump(typeof(CommunityCollectionGump));
			mobile.SendGump(new CommunityCollectionGump(mobile, this, Location));
		}
		else
			from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
	}

	public override void GetProperties(ObjectPropertyList list)
	{
		base.GetProperties(list);

		list.Add(1072819, _mTier.ToString()); // Current Tier: ~1_TIER~
		list.Add(1072820, _mPoints.ToString()); // Current Points: ~1_POINTS~
		list.Add(1072821, _mTier > MaxTier ? 0.ToString() : CurrentTier.ToString()); // Points until next tier: ~1_POINTS~

		if (DonationLabel > 0)
			list.Add(DonationLabel);
		else if (DonationString != null)
			list.Add(DonationString);
	}

	public CollectionData GetData()
	{
		CollectionData ret = new()
		{
			Collection = CollectionId,
			Points = Points,
			StartTier = StartTier,
			NextTier = NextTier,
			DailyDecay = DailyDecay,
			Tier = Tier,
			DonationTitle = _mDonationTitle,
			Tiers = Tiers
		};

		return ret;
	}

	public void SetData(CollectionData data)
	{
		_mPoints = data.Points;
		_mStartTier = data.StartTier;
		_mNextTier = data.NextTier;
		_mDailyDecay = data.DailyDecay;
		_mTier = data.Tier;
		_mDonationTitle = data.DonationTitle;
		Tiers = data.Tiers;
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(1); // version			
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		int version = reader.ReadInt();

		Init();

		if (version == 0)
		{
			_mPoints = reader.ReadLong();
			_mStartTier = reader.ReadLong();
			_mNextTier = reader.ReadLong();
			_mDailyDecay = reader.ReadLong();
			_mTier = reader.ReadInt();

			_mDonationTitle = QuestReader.Object(reader);

			for (var i = reader.ReadInt(); i > 0; i--)
			{
				List<object> list = new();

				for (int j = reader.ReadInt(); j > 0; j--)
					list.Add(QuestReader.Object(reader));

				Tiers.Add(list);
			}
			CollectionsSystem.RegisterMobile(this);
		}

		if (CantWalk)
			Frozen = true;
	}

	#region IComunityCollection
	public virtual void Donate(PlayerMobile player, CollectionItem item, int amount)
	{
		int points = (int)Math.Round(amount * item.Points);

		player.AddCollectionPoints(CollectionId, points);

		player.SendLocalizedMessage(1072816); // Thank you for your donation!
		player.SendLocalizedMessage(1072817, points.ToString()); // You have earned ~1_POINTS~ reward points for this donation.	
		player.SendLocalizedMessage(1072818, points.ToString()); // The Collection has been awarded ~1_POINTS~ points

		Points += points;

		InvalidateProperties();
	}

	public virtual void Reward(PlayerMobile player, CollectionItem reward, int hue)
	{
		Item item = QuestHelper.Construct(reward.Type) as Item;

		if (item != null && player.AddToBackpack(item))
		{
			if (hue > 0)
				item.Hue = hue;

			player.AddCollectionPoints(CollectionId, (int)reward.Points * -1);
			player.SendLocalizedMessage(1073621); // Your reward has been placed in your backpack.
			player.PlaySound(0x5A7);

			if (reward.QuestItem)
				CollectionsObtainObjective.CheckReward(player, item);

			reward.OnGiveReward(player, item, this, hue);
		}
		else if (item != null)
		{
			player.SendLocalizedMessage(1074361); // The reward could not be given.  Make sure you have room in your pack.
			item.Delete();
		}

		player.SendGump(new CommunityCollectionGump(player, this, Location));
	}

	public virtual void DonatePet(PlayerMobile player, BaseCreature pet)
	{
		for (var i = 0; i < Donations.Count; i++)
			if (Donations[i].Type == pet.GetType())
			{
				pet.Delete();
				Donate(player, Donations[i], 1);
				return;
			}

		player.SendLocalizedMessage(1073113); // This Collection is not accepting that type of creature.
	}

	#endregion

	public virtual void IncreaseTier()
	{
		_mTier += 1;
	}

	public virtual void DecreaseTier()
	{
		_mTier -= 1;

		if (Tiers is not {Count: > 0}) return;
		for (var i = 0; i < Tiers[^1].Count; i++)
		{
			switch (Tiers[^1][i])
			{
				case Item:
					((Item)Tiers[^1][i]).Delete();
					break;
				case Mobile:
					((Mobile)Tiers[^1][i]).Delete();
					break;
			}
		}

		Tiers.RemoveAt(Tiers.Count - 1);
	}

	public override void Init()
	{
		Donations ??= new List<CollectionItem>();

		Rewards ??= new List<CollectionItem>();

		Tiers ??= new List<List<object>>();

		// start decay timer
		if (_mDailyDecay <= 0) return;
		DateTime today = DateTime.Today.AddDays(1);


		new CollectionDecayTimer(this, today - DateTime.UtcNow);
	}

	public virtual bool CanDonate(PlayerMobile player)
	{
		return true;
	}

	public override void OnDelete()
	{
		base.OnDelete();

		CollectionsSystem.UnregisterMobile(this);
	}
}
