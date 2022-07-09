using Server.Gumps;
using System;

namespace Server.Engines.NewMagincia;

public class WritOfLease : BaseItem
{
	public override int LabelNumber => 1150489;  // a writ of lease

	[CommandProperty(AccessLevel.GameMaster)]
	public MaginciaHousingPlot Plot { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public DateTime Expires { get; private set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public bool Expired { get; private set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public Map Facet { get; private set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public string Identifier { get; private set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public Point3D RecallLoc { get; private set; }

	public WritOfLease(MaginciaHousingPlot plot)
		: base(5358)
	{
		Hue = 0x9A;
		Plot = plot;
		Expires = plot.Expires;
		Expired = false;

		Facet = plot.Map;
		Identifier = plot.Identifier;
		RecallLoc = plot.RecallLoc;
	}

	public override void GetProperties(ObjectPropertyList list)
	{
		base.GetProperties(list);

		list.Add(1150547, Identifier ?? "Unkonwn"); // Lot: ~1_LOTNAME~

		list.Add(Facet == Map.Trammel ? 1150549 : 1150548); // Facet: Felucca

		list.Add(1150546, Misc.ServerList.ServerName); // Shard: ~1_SHARDNAME~

		if (Expired)
			list.Add(1150487); // [Expired]
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (from.InRange(GetWorldLocation(), 2))
		{
			from.CloseGump(typeof(WritNoteGump));
			from.SendGump(new WritNoteGump(this));
		}
	}

	public void CheckExpired()
	{
		if (DateTime.UtcNow > Expires)
			OnExpired();
	}

	public void OnExpired()
	{
		MaginciaLottoSystem.UnregisterPlot(Plot);
		Plot = null;
		Expired = true;
		Expires = DateTime.MinValue;
		InvalidateProperties();
	}

	public override void Delete()
	{
		if (!Expired)
			OnExpired();

		base.Delete();
	}

	public WritOfLease(Serial serial)
		: base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0); // version

		writer.Write(Expired);
		writer.Write(Expires);
		writer.Write(Facet);
		writer.Write(Identifier);
		writer.Write(RecallLoc);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadInt();
		Expired = reader.ReadBool();
		Expires = reader.ReadDateTime();
		Facet = reader.ReadMap();
		Identifier = reader.ReadString();
		RecallLoc = reader.ReadPoint3D();
	}

	private class WritNoteGump : Gump
	{
		private readonly WritOfLease _lease;

		public WritNoteGump(WritOfLease lease)
			: base(100, 100)
		{
			_lease = lease;

			AddImage(0, 0, 9380);
			AddImage(114, 0, 9381);
			AddImage(171, 0, 9382);
			AddImage(0, 140, 9386);
			AddImage(114, 140, 9387);
			AddImage(171, 140, 9388);

			AddHtmlLocalized(90, 5, 200, 16, 1150484, 1, false, false); // WRIT OF LEASE

			string args;

			if (lease.Expired)
			{
				args =
					$"{lease.Identifier}\t{lease.Facet}\t{Misc.ServerList.ServerName}\t\t{lease.RecallLoc.X} {lease.RecallLoc.Y}";
				AddHtmlLocalized(38, 55, 215, 178, 1150488, args, 1, false, true);
				//This deed once entitled the bearer to build a house on the plot of land designated "~1_PLOT~" (located at ~5_SEXTANT~) in the City of New Magincia on the ~2_FACET~ facet of the ~3_SHARD~ shard.<br><br>The deed has expired, and now the indicated plot of land is subject to normal house construction rules.<br><br>This deed was won by lottery, and while it is no longer valid for land ownership it does serve to commemorate the winning of land during the Rebuilding of Magincia.<br><br>This deed functions as a recall rune marked for the location of the plot it represents.
			}
			else
			{
				args =
					$"{lease.Identifier}\t{lease.Facet}\t{Misc.ServerList.ServerName}\t{MaginciaLottoSystem.WritExpirePeriod}\t{lease.RecallLoc.X} {lease.RecallLoc.Y}";
				AddHtmlLocalized(38, 55, 215, 178, 1150483, args, 1, false, true);
				//This deed entitles the bearer to build a house on the plot of land designated "~1_PLOT~" (located at ~5_SEXTANT~) in the City of New Magincia on the ~2_FACET~ facet of the ~3_SHARD~ shard.<br><br>The deed will expire once it is used to construct a house, and thereafter the indicated plot of land will be subject to normal house construction rules. The deed will expire after ~4_DAYS~ more days have passed, and at that time the right to place a house reverts to normal house construction rules.<br><br>This deed functions as a recall rune marked for the location of the plot it represents.<br><br>To place a house on the deeded plot, you must simply have this deed in your backpack or bank box when using a House Placement Tool there.
			}
		}
	}
}
