using Server.Items;
using Server.Multis.Deeds;
using Server.Network;
using Server.Targeting;
using System;
using System.Collections.Generic;

namespace Server.Mobiles;

public class RealEstateBroker : BaseVendor
{
	private readonly List<SbInfo> _mSbInfos = new();
	protected override List<SbInfo> SbInfos => _mSbInfos;

	[Constructable]
	public RealEstateBroker() : base("the real estate broker")
	{
		Job = JobFragment.architect;
		Karma = Utility.RandomMinMax(13, -45);
	}

	public override bool HandlesOnSpeech(Mobile from)
	{
		if (from.Alive && from.InRange(this, 3))
			return true;

		return base.HandlesOnSpeech(from);
	}

	private DateTime _mNextCheckPack;

	public override void OnMovement(Mobile m, Point3D oldLocation)
	{
		if (DateTime.UtcNow > _mNextCheckPack && InRange(m, 4) && !InRange(oldLocation, 4) && m.Player)
		{
			Container pack = m.Backpack;

			if (pack != null)
			{
				_mNextCheckPack = DateTime.UtcNow + TimeSpan.FromSeconds(2.0);

				Item deed = pack.FindItemByType(typeof(HouseDeed), false);

				if (deed != null)
				{
					// If you have a deed, I can appraise it or buy it from you...
					PrivateOverheadMessage(MessageType.Regular, 0x3B2, 500605, m.NetState);

					// Simply hand me a deed to sell it.
					PrivateOverheadMessage(MessageType.Regular, 0x3B2, 500606, m.NetState);
				}
			}
		}

		base.OnMovement(m, oldLocation);
	}

	public override void OnSpeech(SpeechEventArgs e)
	{
		if (!e.Handled && e.Mobile.Alive && e.HasKeyword(0x38)) // *appraise*
		{
			PublicOverheadMessage(MessageType.Regular, 0x3B2, 500608); // Which deed would you like appraised?
			e.Mobile.BeginTarget(12, false, TargetFlags.None, Appraise_OnTarget);
			e.Handled = true;
		}

		base.OnSpeech(e);
	}

	public override bool OnDragDrop(Mobile from, Item dropped)
	{
		if (dropped is HouseDeed deed)
		{
			int price = ComputePriceFor(deed);

			if (price > 0)
			{
				if (Banker.Deposit(from, price))
				{
					// For the deed I have placed gold in your bankbox :
					PublicOverheadMessage(MessageType.Regular, 0x3B2, 1008000, AffixType.Append, price.ToString(), "");

					deed.Delete();
					return true;
				}
				else
				{
					PublicOverheadMessage(MessageType.Regular, 0x3B2, 500390); // Your bank box is full.
					return false;
				}
			}
			else
			{
				PublicOverheadMessage(MessageType.Regular, 0x3B2, 500607); // I'm not interested in that.
				return false;
			}
		}

		return base.OnDragDrop(from, dropped);
	}

	public void Appraise_OnTarget(Mobile from, object obj)
	{
		if (obj is HouseDeed deed)
		{
			int price = ComputePriceFor(deed);

			if (price > 0)
			{
				// I will pay you gold for this deed :
				PublicOverheadMessage(MessageType.Regular, 0x3B2, 1008001, AffixType.Append, price.ToString(), "");

				PublicOverheadMessage(MessageType.Regular, 0x3B2, 500610); // Simply hand me the deed if you wish to sell it.
			}
			else
			{
				PublicOverheadMessage(MessageType.Regular, 0x3B2, 500607); // I'm not interested in that.
			}
		}
		else
		{
			PublicOverheadMessage(MessageType.Regular, 0x3B2, 500609); // I can't appraise things I know nothing about...
		}
	}

	public static int ComputePriceFor(HouseDeed deed)
	{
		int price = 0;

		switch (deed)
		{
			case SmallBrickHouseDeed:
			case StonePlasterHouseDeed:
			case FieldStoneHouseDeed:
			case WoodHouseDeed:
			case WoodPlasterHouseDeed:
			case ThatchedRoofCottageDeed:
				price = 43800;
				break;
			case BrickHouseDeed:
				price = 144500;
				break;
			case TwoStoryWoodPlasterHouseDeed:
			case TwoStoryStonePlasterHouseDeed:
				price = 192400;
				break;
			case TowerDeed:
				price = 433200;
				break;
			case KeepDeed:
				price = 665200;
				break;
			case CastleDeed:
				price = 1022800;
				break;
			case LargePatioDeed:
				price = 152800;
				break;
			case LargeMarbleDeed:
				price = 192800;
				break;
			case SmallTowerDeed:
				price = 88500;
				break;
			case LogCabinDeed:
				price = 97800;
				break;
			case SandstonePatioDeed:
				price = 90900;
				break;
			case VillaDeed:
				price = 136500;
				break;
			case StoneWorkshopDeed:
				price = 60600;
				break;
			case MarbleWorkshopDeed:
				price = 60300;
				break;
		}

		return AOS.Scale(price, 80); // refunds 80% of the purchase price
	}

	public override void InitSbInfo()
	{
		_mSbInfos.Add(new SbRealEstateBroker());
	}

	public RealEstateBroker(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadInt();
	}
}
