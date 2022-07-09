using Server.Accounting;
using Server.Items;
using Server.Mobiles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Engines.NewMagincia;

[PropertyObject]
public class MaginciaPlotAuction
{
	public Dictionary<Mobile, BidEntry> Auctioners { get; } = new();

	[CommandProperty(AccessLevel.GameMaster)]
	public MaginciaBazaarPlot Plot { get; }

	[CommandProperty(AccessLevel.GameMaster)]
	public DateTime AuctionEnd { get; private set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public bool EndCurrentAuction
	{
		get => false;
		set => EndAuction();
	}

	public MaginciaPlotAuction(MaginciaBazaarPlot plot) : this(plot, MaginciaBazaar.GetShortAuctionTime)
	{
	}

	public MaginciaPlotAuction(MaginciaBazaarPlot plot, TimeSpan auctionDuration)
	{
		Plot = plot;
		AuctionEnd = DateTime.UtcNow + auctionDuration;
	}

	public override string ToString()
	{
		return "...";
	}

	public void MakeBid(Mobile bidder, int amount)
	{
		Auctioners[bidder] = new BidEntry(bidder, amount, BidType.Specific);
	}

	public bool RetractBid(Mobile from)
	{
		if (from.Account is Account acct)
		{
			for (int i = 0; i < acct.Length; i++)
			{
				Mobile m = acct[i];

				if (m == null)
					continue;

				if (Auctioners.ContainsKey(m))
				{
					BidEntry entry = Auctioners[m];

					if (entry != null && Banker.Deposit(m, entry.Amount))
					{
						Auctioners.Remove(m);
						return true;
					}
				}
			}
		}

		return false;
	}

	public void RemoveBid(Mobile from)
	{
		if (Auctioners.ContainsKey(from))
			Auctioners.Remove(from);
	}

	public int GetHighestBid()
	{
		int highest = -1;
		foreach (var entry in Auctioners.Values.Where(entry => entry.Amount >= highest))
		{
			highest = entry.Amount;
		}
		return highest;
	}

	public void OnTick()
	{
		if (AuctionEnd < DateTime.UtcNow)
			EndAuction();
	}

	public void EndAuction()
	{
		if (Plot == null)
			return;

		if (Plot.HasTempMulti())
			Plot.RemoveTempPlot();

		Mobile winner = null;

		Dictionary<Mobile, BidEntry> combined = new(Auctioners);

		//Combine auction bids with the bids for next available plot
		foreach (KeyValuePair<Mobile, BidEntry> kvp in MaginciaBazaar.NextAvailable)
			combined.Add(kvp.Key, kvp.Value);

		//Get highest bid
		int highest = combined.Values.Select(entry => entry.Amount).Prepend(0).Max();

		// Check for owner, and if the owner has a match bad AND hasn't bidded on another plot!
		if (Plot.Owner != null && MaginciaBazaar.Reserve.ContainsKey(Plot.Owner) && MaginciaBazaar.Instance != null && !MaginciaBazaar.Instance.HasActiveBid(Plot.Owner))
		{
			int matching = MaginciaBazaar.GetBidMatching(Plot.Owner);

			if (matching >= highest)
			{
				MaginciaBazaar.DeductReserve(Plot.Owner, highest);
				int newreserve = MaginciaBazaar.GetBidMatching(Plot.Owner);
				winner = Plot.Owner;

				/*You extended your lease on Stall ~1_STALLNAME~ at the ~2_FACET~ New Magincia 
				 *Bazaar. You matched the top bid of ~3_BIDAMT~gp. That amount has been deducted 
				 *from your Match Bid of ~4_MATCHAMT~gp. Your Match Bid balance is now 
				 *~5_NEWMATCH~gp. You may reclaim any additional match bid funds or adjust 
				 *your match bid for next week at the bazaar.*/
				MaginciaLottoSystem.SendMessageTo(Plot.Owner, new NewMaginciaMessage(null, new TextDefinition(1150427),
					$"@{Plot.PlotDef.Id}@{Plot.PlotDef.Map}@{highest:N0}@{matching:N0}@{newreserve:N0}"));
			}
			else
			{
				/*You lost the bid to extend your lease on Stall ~1_STALLNAME~ at the ~2_FACET~ 
				 *New Magincia Bazaar. Your match bid amount of ~3_BIDAMT~gp is held in escrow 
				 *at the Bazaar. You may obtain a full refund there at any time. Your hired 
				 *merchant, if any, has deposited your proceeds and remaining inventory at the 
				 *Warehouse in New Magincia. You must retrieve these within one week or they 
				 *will be destroyed.*/
				MaginciaLottoSystem.SendMessageTo(Plot.Owner, new NewMaginciaMessage(null, new TextDefinition(1150528),
					$"@{Plot.PlotDef.Id}@{Plot.PlotDef.Map}@{matching:N0}"));
			}
		}
		else if (Plot.Owner != null)
		{
			/*Your lease has expired on Stall ~1_STALLNAME~ at the ~2_FACET~ New Magincia Bazaar.*/
			MaginciaLottoSystem.SendMessageTo(Plot.Owner, new NewMaginciaMessage(null, new TextDefinition(1150430),
				$"@{Plot.PlotDef.Id}@{Plot.PlotDef.Map}"));
		}

		if (winner == null)
		{
			//Get list of winners
			List<BidEntry> winners = (from kvp in combined where kvp.Value.Amount >= highest select kvp.Value).ToList();

			// One winner!
			if (winners.Count == 1)
				winner = winners[0].Bidder;
			else
			{
				// get a list of specific type (as opposed to next available)
				List<BidEntry> specifics = winners.Where(bid => bid.BidType == BidType.Specific).ToList();

				switch (specifics.Count)
				{
					// one 1 specific!
					case 1:
						winner = specifics[0].Bidder;
						break;
					case > 1:
					{
						//gets oldest specific
						BidEntry oldest = null;
						foreach (var entry in specifics.Where(entry => oldest == null || entry.DatePlaced < oldest.DatePlaced))
						{
							oldest = entry;
						}

						if (oldest != null)
							winner = oldest.Bidder;
						break;
					}
					default:
					{
						//no specifics! gets oldest of list of winners
						BidEntry oldest = null;
						foreach (var entry in winners.Where(entry => oldest == null || entry.DatePlaced < oldest.DatePlaced))
						{
							oldest = entry;
						}

						if (oldest != null)
							winner = oldest.Bidder;
						break;
					}
				}
			}
		}

		//Give back gold
		foreach (KeyValuePair<Mobile, BidEntry> kvp in Auctioners)
		{
			Mobile m = kvp.Key;

			if (m != winner)
			{
				if (!Banker.Deposit(m, kvp.Value.Amount, true) && m.Backpack != null)
				{
					int total = kvp.Value.Amount;

					while (total > 60000)
					{
						m.Backpack.DropItem(new BankCheck(60000));
						total -= 60000;
					}

					if (total > 0)
						m.Backpack.DropItem(new BankCheck(total));
				}
			}
		}
		//Does actual changes to plots
		if (winner != null)
			MaginciaBazaar.AwardPlot(this, winner, highest);
		else
		{
			Plot.Reset(); // lease expires
			Plot.NewAuction(MaginciaBazaar.GetShortAuctionTime);
		}
	}

	public int GetBidAmount(Mobile from)
	{
		return !Auctioners.ContainsKey(from) ? 0 : Auctioners[from].Amount;
	}

	public void ChangeAuctionTime(TimeSpan ts)
	{
		AuctionEnd = DateTime.UtcNow + ts;

		if (Plot is {Sign: { }})
			Plot.Sign.InvalidateProperties();
	}

	public MaginciaPlotAuction(GenericReader reader, MaginciaBazaarPlot plot)
	{
		reader.ReadInt();

		Plot = plot;
		AuctionEnd = reader.ReadDateTime();

		int c = reader.ReadInt();
		for (int i = 0; i < c; i++)
		{
			Mobile m = reader.ReadMobile();
			BidEntry entry = new BidEntry(reader);

			if (m != null)
				Auctioners[m] = entry;
		}
	}

	public void Serialize(GenericWriter writer)
	{
		writer.Write(0);

		writer.Write(AuctionEnd);

		writer.Write(Auctioners.Count);
		foreach (KeyValuePair<Mobile, BidEntry> kvp in Auctioners)
		{
			writer.Write(kvp.Key);
			kvp.Value.Serialize(writer);
		}
	}
}

public enum BidType
{
	Specific,
	NextAvailable
}

public class BidEntry : IComparable
{
	public Mobile Bidder { get; }

	public int Amount { get; }

	public BidType BidType { get; }

	public DateTime DatePlaced { get; }

	public BidEntry(Mobile bidder, int amount, BidType type)
	{
		Bidder = bidder;
		Amount = amount;
		BidType = type;
		DatePlaced = DateTime.UtcNow;
	}

	public int CompareTo(object obj)
	{
		return ((BidEntry)obj).Amount - Amount;
	}

	public BidEntry(GenericReader reader)
	{
		reader.ReadInt();
		Bidder = reader.ReadMobile();
		Amount = reader.ReadInt();
		BidType = (BidType)reader.ReadInt();
		DatePlaced = reader.ReadDateTime();
	}

	public void Serialize(GenericWriter writer)
	{
		writer.Write(0);
		writer.Write(Bidder);
		writer.Write(Amount);
		writer.Write((int)BidType);
		writer.Write(DatePlaced);
	}
}
