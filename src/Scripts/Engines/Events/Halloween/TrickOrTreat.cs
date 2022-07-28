using Server.Items;
using Server.Mobiles;
using Server.Targeting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Engines.Events;

public class TrickOrTreat
{
	private static readonly DateTime m_Current = DateTime.UtcNow; // returns year
	private static DateTime StartHalloween => new(m_Current.Year, 10, 24);  // YY MM DD
	public static DateTime FinishHalloween => new(m_Current.Year, 11, 15);
	private static Item RandomBeggerItem => (Item)Activator.CreateInstance(m_GmBeggarTreats[Utility.Random(m_GmBeggarTreats.Length)]);
	private static Item RandomTreat => (Item)Activator.CreateInstance(m_Treats[Utility.Random(m_Treats.Length)]);
	private static readonly bool PlayerZombiesEnabled = Settings.Configuration.Get<bool>("Holidays", "PlayerZombies");
	private static readonly bool PumkinPatchEnabled = Settings.Configuration.Get<bool>("Holidays", "PumpkinPatch");
	private static readonly bool TrickorTreatEnabled = Settings.Configuration.Get<bool>("Holidays", "TrickorTreat");

	private static readonly Type[] m_GmBeggarTreats =
	{
		typeof( CreepyCake ),
		typeof( PumpkinPizza ),
		typeof( GrimWarning ),
		typeof( HarvestWine ),
		typeof( MurkyMilk ),
		typeof( MrPlainsCookies ),
		typeof( SkullsOnPike ),
		typeof( ChairInAGhostCostume ),
		typeof( ExcellentIronMaiden ),
		typeof( HalloweenGuillotine ),
		typeof( ColoredSmallWebs )
	};

	private static readonly Type[] m_Treats =
	{
		typeof( Lollipops ),
		typeof( WrappedCandy ),
		typeof( JellyBeans ),
		typeof( Taffy ),
		typeof( NougatSwirl )
	};

	private static readonly Rectangle2D[] m_PumpkinFields =
	{
		new( 4557, 1471, 20, 10 ),
		new( 796, 2152, 36, 24 ),
		new( 816, 2251, 16, 8 ),
		new( 816, 2261, 16, 8 ),
		new( 816, 2271, 16, 8 ),
		new( 816, 2281, 16, 8 ),
		new( 835, 2344, 16, 16 ),
		new( 816, 2344, 16, 24 )
	};

	public static void Initialize()
	{
		DateTime unused = DateTime.UtcNow;

		if (DateTime.UtcNow < StartHalloween || DateTime.UtcNow > FinishHalloween)
			return;

		if (TrickorTreatEnabled)
		{
			EventSink.OnSpeech += EventSink_Speech;
		}

		if (PumkinPatchEnabled)
		{
			Timer.DelayCall(TimeSpan.Zero, TimeSpan.FromMinutes(.50), 0, PumpkinPatchSpawnerCallback);
		}

		if (!PlayerZombiesEnabled)
			return;

		HalloweenHauntings.TotalZombieLimit = 200;
		HalloweenHauntings.DeathQueueLimit = 200;
		HalloweenHauntings.QueueDelaySeconds = 120;
		HalloweenHauntings.QueueClearIntervalSeconds = 1800;

		TimeSpan tick = TimeSpan.FromSeconds(HalloweenHauntings.QueueDelaySeconds);
		TimeSpan clear = TimeSpan.FromSeconds(HalloweenHauntings.QueueClearIntervalSeconds);

		HalloweenHauntings._ReAnimated = new Dictionary<PlayerMobile, ZombieSkeleton>();
		HalloweenHauntings.DeathQueue = new List<PlayerMobile>();

		HalloweenHauntings.Timer = Timer.DelayCall(tick, tick, HalloweenHauntings.Timer_Callback);

		HalloweenHauntings.ClearTimer = Timer.DelayCall(clear, clear, HalloweenHauntings.Clear_Callback);

		EventSink.OnMobileDeath += HalloweenHauntings.EventSink_PlayerDeath;
	}

	private static void EventSink_Speech(SpeechEventArgs e)
	{
		if (!Insensitive.Contains(e.Speech, "trick or treat"))
			return;

		e.Mobile.Target = new TrickOrTreatTarget();

		e.Mobile.SendLocalizedMessage(1076764);  /* Pick someone to Trick or Treat. */
	}

	private class TrickOrTreatTarget : Target
	{
		public TrickOrTreatTarget()
			: base(15, false, TargetFlags.None)
		{
		}

		protected override void OnTarget(Mobile from, object targ)
		{
			if (targ == null || !CheckMobile(from))
				return;

			if (targ is not Mobile)
			{
				from.SendLocalizedMessage(1076781); /* There is little chance of getting candy from that! */
				return;
			}
			if (targ is not BaseVendor begged || begged.Deleted)
			{
				from.SendLocalizedMessage(1076765); /* That doesn't look friendly. */
				return;
			}

			DateTime now = DateTime.UtcNow;

			if (!CheckMobile(begged))
				return;

			if (begged.NextTrickOrTreat > now)
			{
				from.SendLocalizedMessage(1076767); /* That doesn't appear to have any more candy. */
				return;
			}

			begged.NextTrickOrTreat = now + TimeSpan.FromMinutes(Utility.RandomMinMax(5, 10));

			if (from.Backpack is not { Deleted: false })
				return;

			if (Utility.RandomDouble() > .10)
			{
				switch (Utility.Random(3))
				{
					case 0: begged.Say(1076768); break; /* Oooooh, aren't you cute! */
					case 1: begged.Say(1076779); break; /* All right...This better not spoil your dinner! */
					case 2: begged.Say(1076778); break; /* Here you go! Enjoy! */
				}

				if (Utility.RandomDouble() <= .01 && from.Skills.Begging.Value >= 100)
				{
					from.AddToBackpack(RandomBeggerItem);

					from.SendLocalizedMessage(1076777); /* You receive a special treat! */
				}
				else
				{
					from.AddToBackpack(RandomTreat);

					from.SendLocalizedMessage(1076769);   /* You receive some candy. */
				}
			}
			else
			{
				begged.Say(1076770); /* TRICK! */

				int action = Utility.Random(4);

				switch (action)
				{
					case 0:
						Timer.DelayCall(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1), 10, Bleeding, from);
						break;
					case 1:
						Timer.DelayCall(TimeSpan.FromSeconds(2), SolidHueMobile, from);
						break;
					default:
						Timer.DelayCall(TimeSpan.FromSeconds(2), MakeTwin, from);
						break;
				}
			}
		}
	}

	private static void Bleeding(Mobile from)
	{
		if (!CheckMobile(from))
			return;

		if (from.Location == Point3D.Zero)
			return;

		int amount = Utility.RandomMinMax(3, 7);

		for (int i = 0; i < amount; i++)
		{
			new Blood(Utility.RandomMinMax(0x122C, 0x122F)).MoveToWorld(RandomPointOneAway(from.X, from.Y, from.Z, from.Map), from.Map);
		}
	}

	private static void RemoveHueMod(Mobile target)
	{
		if (target is { Deleted: false })
		{
			target.SolidHueOverride = -1;
		}
	}

	private static void SolidHueMobile(Mobile target)
	{
		if (CheckMobile(target))
		{
			target.SolidHueOverride = Utility.RandomMinMax(2501, 2644);

			Timer.DelayCall(TimeSpan.FromSeconds(10), RemoveHueMod, target);
		}
	}

	private static void MakeTwin(Mobile from)
	{
		List<Item> items = new();

		if (!CheckMobile(from))
			return;

		Mobile twin = new NaughtyTwin(from);

		if (twin is not { Deleted: false })
			return;

		items.AddRange(from.Items.Where(item => item.Layer != Layer.Backpack && item.Layer != Layer.Mount && item.Layer != Layer.Bank));

		if (items.Count > 0)
		{
			for (int i = 0; i < items.Count; i++) /* dupe exploits start out like this ... */
			{
				twin.AddItem(Mobile.LiftItemDupe(items[i], 1));
			}

			foreach (var item in twin.Items.Where(item => item.Layer != Layer.Backpack && item.Layer != Layer.Mount && item.Layer != Layer.Bank))
			{
				item.Movable = false;
			}
		}

		twin.Hue = from.Hue;
		twin.BodyValue = from.BodyValue;
		twin.Kills = from.Kills;

		Point3D point = RandomPointOneAway(from.X, from.Y, from.Z, from.Map);

		twin.MoveToWorld(from.Map.CanSpawnMobile(point) ? point : from.Location, from.Map);

		Timer.DelayCall(TimeSpan.FromSeconds(5), DeleteTwin, twin);
	}

	private static void DeleteTwin(Mobile twin)
	{
		if (CheckMobile(twin))
		{
			twin.Delete();
		}
	}

	private static Point3D RandomPointOneAway(int x, int y, int z, Map map)
	{
		Point3D loc = new(x + Utility.Random(-1, 3), y + Utility.Random(-1, 3), 0);

		loc.Z = map.CanFit(loc, 0) ? map.GetAverageZ(loc.X, loc.Y) : z;

		return loc;
	}

	public static bool CheckMobile(Mobile mobile)
	{
		return mobile is { Map: { }, Deleted: false, Alive: true } && mobile.Map != Map.Internal;
	}

	private static void PumpkinPatchSpawnerCallback()
	{
		AddPumpkin(Map.Felucca);
		AddPumpkin(Map.Trammel);
	}

	private static void AddPumpkin(Map map)
	{
		for (int i = 0; i < m_PumpkinFields.Length; i++)
		{
			Rectangle2D rect = m_PumpkinFields[i];

			int spawncount = rect.Height * rect.Width / 20;
			int pumpkins = map.GetItemsInBounds(rect).OfType<HalloweenPumpkin>().Count();

			if (spawncount <= pumpkins)
				continue;

			Item item = new HalloweenPumpkin();

			item.MoveToWorld(RandomPointIn(rect, map), map);
		}
	}

	private static Point3D RandomPointIn(Rectangle2D rect, Map map)
	{
		int x = Utility.Random(rect.X, rect.Width);
		int y = Utility.Random(rect.Y, rect.Height);
		int z = map.GetAverageZ(x, y);

		return new Point3D(x, y, z);
	}
}

public class NaughtyTwin : BaseCreature
{
	private readonly Mobile m_From;

	private static readonly Point3D[] m_FeluccaLocations =
	{
		new( 4467, 1283, 5 ), // Moonglow
		new( 1336, 1997, 5 ), // Britain
		new( 1499, 3771, 5 ), // Jhelom
		new(  771,  752, 5 ), // Yew
		new( 2701,  692, 5 ), // Minoc
		new( 1828, 2948,-20), // Trinsic
		new(  643, 2067, 5 ), // Skara Brae
		new( 3563, 2139, Map.Trammel.GetAverageZ( 3563, 2139 ) ), // (New) Magincia
	};

	private static readonly Point3D[] m_MalasLocations =
	{
		new(1015, 527, -65), // Luna
		new(1997, 1386, -85) // Umbra
	};

	private static readonly Point3D[] m_IlshenarLocations =
	{
		new( 1215,  467, -13 ), // Compassion
		new(  722, 1366, -60 ), // Honesty
		new(  744,  724, -28 ), // Honor
		new(  281, 1016,   0 ), // Humility
		new(  987, 1011, -32 ), // Justice
		new( 1174, 1286, -30 ), // Sacrifice
		new( 1532, 1340, - 3 ), // Spirituality
		new(  528,  216, -45 ), // Valor
		new( 1721,  218,  96 )  // Chaos
	};

	private static readonly Point3D[] m_TokunoLocations =
	{
		new( 1169,  998, 41 ), // Isamu-Jima
		new(  802, 1204, 25 ), // Makoto-Jima
		new(  270,  628, 15 )  // Homare-Jima
	};

	public NaughtyTwin(Mobile from)
		: base(AIType.AI_Melee, FightMode.None, 10, 1, 0.2, 0.4)
	{
		if (!TrickOrTreat.CheckMobile(from))
			return;

		Body = from.Body;

		m_From = from;
		Name = $"{from.Name}\'s Naughty Twin";

		Timer.DelayCall(TimeSpan.FromSeconds(1), Utility.RandomBool() ? StealCandy : new TimerStateCallback<Mobile>(ToGate), m_From);
	}

	public override void OnThink()
	{
		if (m_From == null || m_From.Deleted)
		{
			Delete();
		}
	}

	private static Item FindCandyTypes(Mobile target)
	{
		Type[] types = { typeof(WrappedCandy), typeof(Lollipops), typeof(NougatSwirl), typeof(Taffy), typeof(JellyBeans) };

		return TrickOrTreat.CheckMobile(target) ? types.Select(target.Backpack.FindItemByType).FirstOrDefault(item => item != null) : null;
	}

	private static void StealCandy(Mobile target)
	{
		if (!TrickOrTreat.CheckMobile(target))
			return;

		Item item = FindCandyTypes(target);

		target.SendLocalizedMessage(1113967); /* Your naughty twin steals some of your candy. */

		if (item is { Deleted: false })
		{
			item.Delete();
		}
	}

	private static void ToGate(Mobile target)
	{
		if (!TrickOrTreat.CheckMobile(target))
			return;

		target.SendLocalizedMessage(1113972); /* Your naughty twin teleports you away with a naughty laugh! */
		target.MoveToWorld(RandomMoongate(target), target.Map);
	}

	private static Point3D RandomMoongate(IEntity target)
	{
		return target.Map.MapID switch
		{
			2 => m_IlshenarLocations[Utility.Random(m_IlshenarLocations.Length)],
			3 => m_MalasLocations[Utility.Random(m_MalasLocations.Length)],
			4 => m_TokunoLocations[Utility.Random(m_TokunoLocations.Length)],
			_ => m_FeluccaLocations[Utility.Random(m_FeluccaLocations.Length)]
		};
	}

	public NaughtyTwin(Serial serial)
		: base(serial)
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
