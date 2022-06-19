using Server.Multis;
using Server.Targeting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Engines.TownHouses
{
	public class TownHouse : VersionHouse
	{
		public static List<TownHouse> AllTownHouses { get; } = new();

		private Item m_CHanger;
		private readonly List<Sector> m_CSectors = new();

		public TownHouseSign ForSaleSign { get; private set; }

		public Item Hanger
		{
			get => m_CHanger ??= new Item(0xB98) { Movable = false, Location = Sign.Location, Map = Sign.Map };
			set => m_CHanger = value;
		}

		public TownHouse(Mobile m, TownHouseSign sign, int locks, int secures)
			: base(0x1DD6 | 0x4000, m, locks, secures)
		{
			ForSaleSign = sign;

			SetSign(0, 0, 0);

			AllTownHouses.Add(this);
		}

		public void InitSectorDefinition()
		{
			if (ForSaleSign == null || ForSaleSign.Blocks.Count == 0)
			{
				return;
			}

			int minX = ForSaleSign.Blocks[0].Start.X;
			int minY = ForSaleSign.Blocks[0].Start.Y;
			int maxX = ForSaleSign.Blocks[0].End.X;
			int maxY = ForSaleSign.Blocks[0].End.Y;

			foreach (Rectangle2D rect in ForSaleSign.Blocks)
			{
				if (rect.Start.X < minX)
				{
					minX = rect.Start.X;
				}
				if (rect.Start.Y < minY)
				{
					minY = rect.Start.Y;
				}
				if (rect.End.X > maxX)
				{
					maxX = rect.End.X;
				}
				if (rect.End.Y > maxY)
				{
					maxY = rect.End.Y;
				}
			}

			foreach (Sector sector in m_CSectors)
			{
				sector.OnMultiLeave(this);
			}

			m_CSectors.Clear();
			for (int x = minX; x < maxX; ++x)
			{
				for (int y = minY; y < maxY; ++y)
				{
					if (!m_CSectors.Contains(Map.GetSector(new Point2D(x, y))))
					{
						m_CSectors.Add(Map.GetSector(new Point2D(x, y)));
					}
				}
			}

			foreach (Sector sector in m_CSectors)
			{
				sector.OnMultiEnter(this);
			}

			Components.Resize(maxX - minX, maxY - minY);
			Components.Add(0x520, Components.Width - 1, Components.Height - 1, -5);
		}

		public override Rectangle2D[] Area
		{
			get
			{
				if (ForSaleSign == null)
				{
					return new Rectangle2D[100];
				}

				var rects = new Rectangle2D[ForSaleSign.Blocks.Count];

				for (int i = 0; i < ForSaleSign.Blocks.Count && i < rects.Length; ++i)
				{
					rects[i] = ForSaleSign.Blocks[i];
				}

				return rects;
			}
		}

		public override bool IsInside(Point3D p, int height)
		{
			if (ForSaleSign == null)
			{
				return false;
			}

			if (Map == null || Region == null)
			{
				Delete();
				return false;
			}

			Sector sector = null;

			try
			{
				if (ForSaleSign is RentalContract && Region.Contains(p))
				{
					return true;
				}

				sector = Map.GetSector(p);

				return !sector.Multis.Any(m => m != null &&
				                               m != this
				                               && m is TownHouse {ForSaleSign: RentalContract} house && house.IsInside(p, height)) && Region.Contains(p);
			}
			catch (Exception e)
			{
				Errors.Report("Error occured in IsInside().  More information on the console.");
				Console.WriteLine($"Info:{Map}, {sector}, {Region}");
				Console.WriteLine(e.Source);
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
				return false;
			}
		}

		public override int GetNewVendorSystemMaxVendors()
		{
			return 50;
		}

		public override int GetAosMaxSecures()
		{
			return MaxSecures;
		}

		public override int GetAosMaxLockdowns()
		{
			return MaxLockDowns;
		}

		public override void OnMapChange()
		{
			base.OnMapChange();

			if (m_CHanger != null)
			{
				m_CHanger.Map = Map;
			}
		}

		public override void OnLocationChange(Point3D oldLocation)
		{
			base.OnLocationChange(oldLocation);

			if (m_CHanger != null)
			{
				m_CHanger.Location = Sign.Location;
			}
		}

		public override void OnSpeech(SpeechEventArgs e)
		{
			if (e.Mobile != Owner || !IsInside(e.Mobile))
			{
				return;
			}

			if (e.Speech.ToLower() == "check house rent")
			{
				ForSaleSign.CheckRentTimer();
			}

			Timer.DelayCall(TimeSpan.Zero, new TimerStateCallback(AfterSpeech), e.Mobile);
		}

		private void AfterSpeech(object o)
		{
			if (o is not Mobile mobile)
			{
				return;
			}

			if (mobile.Target is not HouseBanTarget || ForSaleSign is not {NoBanning: true})
			{
				return;
			}

			mobile.Target.Cancel(mobile, TargetCancelType.Canceled);
			mobile.SendMessage(0x161, "You cannot ban people from this house.");
		}

		public override void OnDelete()
		{
			if (m_CHanger != null)
			{
				m_CHanger.Delete();
			}

			foreach (Item item in Sign.GetItemsInRange(0).Where(item => item != null && item != Sign))
			{
				item.Visible = true;
			}

			ForSaleSign.ClearHouse();
			Doors.Clear();

			AllTownHouses.Remove(this);

			base.OnDelete();
		}

		public TownHouse(Serial serial)
			: base(serial)
		{
			AllTownHouses.Add(this);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(3);

			// Version 2

			writer.Write(m_CHanger);

			// Version 1

			writer.Write(ForSaleSign);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			if (version >= 2)
			{
				m_CHanger = reader.ReadItem();
			}

			ForSaleSign = (TownHouseSign)reader.ReadItem();

			if (version <= 2)
			{
				var count = reader.ReadInt();
				for (var i = 0; i < count; ++i)
				{
					reader.ReadRect2D();
				}
			}

			if (Price == 0)
			{
				Price = 1;
			}

			ItemId = 0x1DD6 | 0x4000;
		}
	}
}
