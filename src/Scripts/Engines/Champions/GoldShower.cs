using Server.Items;
using System;
using System.Linq;

namespace Server.Engines.Champions;

public class GoldShower
{
	public static void DoForChamp(Point3D center, Map map)
	{
		Do(center, map, ChampionSystem.GoldShowerPiles, ChampionSystem.GoldShowerMinAmount, ChampionSystem.GoldShowerMaxAmount);
	}

	public static void DoForHarrower(Point3D center, Map map)
	{
		Do(center, map, ChampionSystem.HarrowerGoldShowerPiles, ChampionSystem.HarrowerGoldShowerMinAmount, ChampionSystem.HarrowerGoldShowerMaxAmount);
	}

	private static void Do(Point3D center, Map map, int piles, int minAmount, int maxAmount)
	{
		new GoodiesTimer(center, map, piles, minAmount, maxAmount).Start();
	}

	private class GoodiesTimer : Timer
	{
		private readonly Map _mMap;
		private readonly Point3D _mLocation;
		private readonly int _mPilesMax;
		private int _mPilesDone;
		private readonly int _mMinAmount;
		private readonly int _mMaxAmount;
		public GoodiesTimer(Point3D center, Map map, int piles, int minAmount, int maxAmount)
			: base(TimeSpan.FromSeconds(0.25d), TimeSpan.FromSeconds(0.25d))
		{
			_mLocation = center;
			_mMap = map;
			_mPilesMax = piles;
			_mMinAmount = minAmount;
			_mMaxAmount = maxAmount;
		}

		protected override void OnTick()
		{
			if (_mPilesDone >= _mPilesMax)
			{
				Stop();
				return;
			}

			var p = FindGoldLocation(_mMap, _mLocation, _mPilesMax / 8);
			Gold g = new(_mMinAmount, _mMaxAmount);
			g.MoveToWorld(p, _mMap);

			switch (Utility.Random(3))
			{
				case 0: // Fire column
					Effects.SendLocationParticles(EffectItem.Create(g.Location, g.Map, EffectItem.DefaultDuration), 0x3709, 10, 30, 5052);
					Effects.PlaySound(g, g.Map, 0x208);
					break;
				case 1: // Explosion
					Effects.SendLocationParticles(EffectItem.Create(g.Location, g.Map, EffectItem.DefaultDuration), 0x36BD, 20, 10, 5044);
					Effects.PlaySound(g, g.Map, 0x307);
					break;
				case 2: // Ball of fire
					Effects.SendLocationParticles(EffectItem.Create(g.Location, g.Map, EffectItem.DefaultDuration), 0x36FE, 10, 10, 5052);
					break;
			}
			++_mPilesDone;
		}

		private static Point3D FindGoldLocation(Map map, Point3D center, int range)
		{
			var cx = center.X;
			var cy = center.Y;

			for (var i = 0; i < 20; ++i)
			{
				var x = cx + Utility.Random(range * 2) - range;
				var y = cy + Utility.Random(range * 2) - range;
				if ((cx - x) * (cx - x) + (cy - y) * (cy - y) > range * range)
					continue;

				var z = map.GetAverageZ(x, y);
				if (!map.CanFit(x, y, z, 6, false, false))
					continue;

				var topZ = map.GetItemsInRange(new Point3D(x, y, z), 0).Select(item => item.Z + item.ItemData.CalcHeight).Prepend(z).Max();
				return new Point3D(x, y, topZ);
			}
			return center;
		}
	}
}
