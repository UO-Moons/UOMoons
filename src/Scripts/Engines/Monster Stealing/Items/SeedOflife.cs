using Server.Mobiles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Items;

public class SeedOfLife : Item
{
	private static readonly Dictionary<PlayerMobile, DateTime> SeedUsageList = new();
	private static readonly TimeSpan Cooldown = TimeSpan.FromMinutes(10);

	public static void Initialize()
	{
		EventSink.AfterWorldSave += CheckCleanup;
	}

	public override int LabelNumber => 1094937;  // seed of life

	[Constructable]
	public SeedOfLife()
		: base(0x1727)
	{
		Hue = 0x491;
		Weight = 1.0;
		Stackable = true;
	}

	private static void CheckCleanup(AfterWorldSaveEventArgs e)
	{
		DoCleanup();
		ManaDraught.DoCleanup();
	}

	private static void DoCleanup()
	{
		List<PlayerMobile> toRemove = SeedUsageList.Keys.Where(pm => SeedUsageList[pm] < DateTime.Now + Cooldown).ToList();

		foreach (PlayerMobile pm in toRemove)
		{
			SeedUsageList.Remove(pm);
		}

		toRemove.Clear();

	}

	private static bool CheckUse(PlayerMobile pm)
	{
		if (SeedUsageList.ContainsKey(pm))
		{
			return SeedUsageList[pm] + Cooldown < DateTime.Now;
		}

		return true;
	}

	private void OnUsed(PlayerMobile by)
	{
		if (CheckUse(by))
		{
			DoHeal(by);
		}
		else
		{
			by.SendLocalizedMessage(1079263, ((int)(SeedUsageList[by] + Cooldown - DateTime.Now).TotalSeconds).ToString());
		}
	}

	private void DoHeal(PlayerMobile pm)
	{
		int toHeal = Utility.RandomMinMax(25, 40);

		int diff = pm.HitsMax - pm.Hits;
		if (diff == 0)
		{
			pm.SendLocalizedMessage(1049547); //You are already at full health 
			return;
		}
		toHeal = Math.Min(toHeal, diff);

		pm.Hits += toHeal;
		Consume();

		if (!SeedUsageList.ContainsKey(pm))
		{
			SeedUsageList.Add(pm, DateTime.Now);
		}
		else
		{
			SeedUsageList[pm] = DateTime.Now;
		}
		pm.SendLocalizedMessage(1095126);//The bitter seed instantly restores some of your health!
	}

	public SeedOfLife(Serial serial)
		: base(serial)
	{
	}


	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadInt();
	}
}
