using System;

namespace Server.Mobiles;

public class DamageStore : IComparable
{
	public readonly Mobile Mobile;
	public int Damage;
	public bool HasRight;
	public double DamagePercent;

	public DamageStore(Mobile m, int damage)
	{
		Mobile = m;
		Damage = damage;
	}

	public int CompareTo(object obj)
	{
		DamageStore ds = (DamageStore)obj;

		return ds.Damage - Damage;
	}
}
