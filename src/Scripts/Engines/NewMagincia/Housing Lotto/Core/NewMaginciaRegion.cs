using Server.Regions;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Server.Engines.NewMagincia;

public class NewMaginciaRegion : TownRegion
{
	public NewMaginciaRegion(XmlElement xml, Map map, Region parent) : base(xml, map, parent)
	{
	}

	public override bool AllowHousing(Mobile from, Point3D p)
	{
		MaginciaLottoSystem system = MaginciaLottoSystem.Instance;

		if (system is {Enabled: true} && from.Backpack != null)
		{
			List<Item> items = new();

			Item[] packItems = from.Backpack.FindItemsByType(typeof(WritOfLease));
			Item[] bankItems = from.BankBox.FindItemsByType(typeof(WritOfLease));

			if (packItems is {Length: > 0})
				items.AddRange(packItems);

			if (bankItems is {Length: > 0})
				items.AddRange(bankItems);

			if (items.Select(item => item as WritOfLease).Any(lease => lease is {Expired: false, Plot: { }} && lease.Plot.Bounds.Contains(p) && from.Map == lease.Plot.Map))
			{
				return true;
			}
		}

		return MaginciaLottoSystem.IsFreeHousingZone(p, Map);
	}
}
