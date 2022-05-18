using Server.Commands;
using Server.Multis;
using System.Collections;
using System.Collections.Generic;

namespace Server.Engines.TownHouses
{
	public class VersionCommand
	{
		private static readonly Hashtable s_Commands = new();

		public static void AddCommand(string com, AccessLevel acc, TownHouseCommandHandler cch)
		{
			s_Commands[com.ToLower()] = cch;
			CommandSystem.Register(com, acc, OnCommand);
		}

		private static void OnCommand(CommandEventArgs e)
		{
			if (s_Commands[e.Command.ToLower()] == null)
			{
				return;
			}

			((TownHouseCommandHandler)s_Commands[e.Command.ToLower()])(new CommandInfo(e.Mobile, e.Command, e.ArgString,
				e.Arguments));
		}

		public static void UpdateRegion(TownHouseSign sign)
		{
			if (sign.House == null)
			{
				return;
			}

			sign.House.UpdateRegion();
			Rectangle3D rect = new(Point3D.Zero, Point3D.Zero);

			for (int i = 0; i < sign.House.Region.Area.Length; ++i)
			{
				rect = sign.House.Region.Area[i];
				rect = new Rectangle3D(
					new Point3D(rect.Start.X - sign.House.X, rect.Start.Y - sign.House.Y, sign.MinZ),
					new Point3D(rect.End.X - sign.House.X, rect.End.Y - sign.House.Y, sign.MaxZ));
				sign.House.Region.Area[i] = rect;
			}

			sign.House.Region.Unregister();
			sign.House.Region.Register();
			sign.House.Region.GoLocation = sign.BanLoc;
		}

		public static bool RegionContains(Region region, Mobile m)
		{
			return region.GetMobiles().Contains(m);
		}

		public static IEnumerable<Rectangle3D> RegionArea(Region region)
		{
			return region.Area;
		}
	}

	public class VersionHouse : BaseHouse
	{
		protected VersionHouse(int id, Mobile m, int locks, int secures)
			: base(id, m, locks, secures)
		{
		}

		public override Rectangle2D[] Area
		{
			get { return new Rectangle2D[5]; }
		}

		public override Point3D BaseBanLocation
		{
			get { return Point3D.Zero; }
		}

		public VersionHouse(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
		}
	}
}
