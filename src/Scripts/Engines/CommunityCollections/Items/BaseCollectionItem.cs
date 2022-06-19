using Server.Engines.Quests;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using System;
using System.Collections.Generic;

namespace Server.Items;

public abstract class BaseCollectionItem : BaseItem, IComunityCollection
{
	#region IComunityCollection		
	public abstract Collection CollectionId { get; }
	public abstract int MaxTier { get; }

	public List<CollectionItem> Donations { get; private set; }

	public List<CollectionItem> Rewards { get; private set; }

	private long _mPoints;
	private long _mStartTier;
	private long _mNextTier;
	private long _mDailyDecay;

	[CommandProperty(AccessLevel.GameMaster)]
	public long Points
	{
		get => _mPoints;
		set
		{
			_mPoints = value;

			if (_mPoints < 0)
				_mPoints = 0;

			while (Tier > 0 && _mPoints < PreviousTier)
				DecreaseTier();

			while (Tier < MaxTier && _mPoints > CurrentTier)
				IncreaseTier();

			InvalidateProperties();
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public long PreviousTier
	{
		get
		{
			if (Tier <= 2) return _mStartTier * Tier;
			long tier = _mStartTier * 2;

			for (var i = 0; i < Tier - 2; i++)
				tier += (i + 3) * _mNextTier;

			return tier;

		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public long CurrentTier
	{
		get
		{
			if (Tier > 1)
				return PreviousTier + (Tier + 1) * _mNextTier;

			return _mStartTier + _mStartTier * Tier;
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
	public int Tier { get; private set; }

	#endregion

	public List<List<object>> Tiers { get; private set; }

	public BaseCollectionItem(int itemId)
		: base(itemId)
	{
		Movable = false;

		Init();
	}

	public BaseCollectionItem(Serial serial)
		: base(serial)
	{
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (from.Alive)
		{
			if (from.NetState == null || !from.NetState.SupportsExpansion(Expansion.ML))
			{
				from.SendLocalizedMessage(1073651); // You must have Mondain's Legacy before proceeding...			
				return;
			}
			else if (!MondainsLegacy.PublicDonations && (int)from.AccessLevel < (int)AccessLevel.GameMaster)
			{
				from.SendLocalizedMessage(1042753, "Public donations"); // ~1_SOMETHING~ has been temporarily disabled.
				return;
			}

			if (from.InRange(Location, 2) && from is PlayerMobile mobile && CanDonate(mobile))
			{
				from.CloseGump(typeof(CommunityCollectionGump));
				from.SendGump(new CommunityCollectionGump(mobile, this, Location));
			}
			else
				from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
		}
	}

	public override void GetProperties(ObjectPropertyList list)
	{
		AddNameProperty(list);

		list.Add(1072819, Tier.ToString()); // Current Tier: ~1_TIER~
		list.Add(1072820, _mPoints.ToString()); // Current Points: ~1_POINTS~
		list.Add(1072821, Tier > MaxTier ? 0.ToString() : CurrentTier.ToString()); // Points until next tier: ~1_POINTS~
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version

		writer.Write(_mPoints);
		writer.Write(_mStartTier);
		writer.Write(_mNextTier);
		writer.Write(_mDailyDecay);
		writer.Write(Tier);

		writer.Write(Tiers.Count);

		for (var i = 0; i < Tiers.Count; i++)
		{
			writer.Write(Tiers[i].Count);

			for (var j = 0; j < Tiers[i].Count; j++)
				QuestWriter.Object(writer, Tiers[i][j]);
		}
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		_ = reader.ReadInt();

		_mPoints = reader.ReadLong();
		_mStartTier = reader.ReadLong();
		_mNextTier = reader.ReadLong();
		_mDailyDecay = reader.ReadLong();
		Tier = reader.ReadInt();

		Init();

		for (int i = reader.ReadInt(); i > 0; i--)
		{
			List<object> list = new();

			for (var j = reader.ReadInt(); j > 0; j--)
				list.Add(QuestReader.Object(reader));

			Tiers.Add(list);
		}
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

		player.CloseGump(typeof(CommunityCollectionGump));
		player.SendGump(new CommunityCollectionGump(player, this, Location));
	}

	public virtual void DonatePet(PlayerMobile player, BaseCreature pet)
	{
		for (var i = 0; i < Donations.Count; i++)
		{
			if (Donations[i].Type != pet.GetType() &&
			    !MoonglowDonationBox.HasGroup(pet.GetType(), Donations[i].Type)) continue;
			pet.Delete();
			Donate(player, Donations[i], 1);
			return;
		}

		player.SendLocalizedMessage(1073113); // This Collection is not accepting that type of creature.
	}

	#endregion

	public virtual void IncreaseTier()
	{
		Tier += 1;
	}

	public virtual void DecreaseTier()
	{
		Tier -= 1;

		if (Tiers is not {Count: > 0}) return;
		for (var i = 0; i < Tiers[^1].Count; i++)
		{
			switch (Tiers[^1][i])
			{
				case Item item:
					item.Delete();
					break;
				case Mobile:
					((Mobile)Tiers[^1][i]).Delete();
					break;
			}
		}

		Tiers.RemoveAt(Tiers.Count - 1);
	}

	public virtual void Init()
	{
		Donations ??= new List<CollectionItem>();

		Rewards ??= new List<CollectionItem>();

		Tiers ??= new List<List<object>>();

		// start decay timer
		if (_mDailyDecay <= 0) return;
		DateTime today = DateTime.Today.AddDays(1);

		_ = new CollectionDecayTimer(this, today - DateTime.UtcNow);
	}

	public virtual bool CanDonate(PlayerMobile player)
	{
		return true;
	}
}
