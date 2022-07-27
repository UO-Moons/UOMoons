using Server.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Mobiles;

public class LoyaltyTimer : Timer
{
	private static readonly TimeSpan InternalDelay = TimeSpan.FromMinutes(5.0);

	public static void Initialize()
	{
		new LoyaltyTimer().Start();
	}

	public LoyaltyTimer() : base(InternalDelay, InternalDelay)
	{
		m_NextHourlyCheck = DateTime.UtcNow + TimeSpan.FromHours(1.0);
		Priority = TimerPriority.FiveSeconds;
	}

	private DateTime m_NextHourlyCheck;

	protected override void OnTick()
	{
		if (DateTime.UtcNow >= m_NextHourlyCheck)
			m_NextHourlyCheck = DateTime.UtcNow + TimeSpan.FromHours(1.0);
		else
			return;

		List<BaseCreature> toRelease = new();

		// added array for wild creatures in house regions to be removed
		List<BaseCreature> toRemove = new();

		_ = Parallel.ForEach(World.Mobiles.Values, m =>
		{
			switch (m)
			{
				case BaseMount { Rider: { } } mount:
					mount.OwnerAbandonTime = DateTime.MinValue;
					return;
				case BaseCreature c:
				{
					if (c.IsDeadPet)
					{
						Mobile owner = c.ControlMaster;

						if (!c.IsStabled && (owner == null || owner.Deleted || owner.Map != c.Map || !owner.InRange(c, 12) || !c.CanSee(owner) || !c.InLOS(owner)))
						{
							if (c.OwnerAbandonTime == DateTime.MinValue)
							{
								c.OwnerAbandonTime = DateTime.UtcNow;
							}
							else if ((c.OwnerAbandonTime + c.BondingAbandonDelay) <= DateTime.UtcNow)
							{
								lock (toRemove)
									toRemove.Add(c);
							}
						}
						else
						{
							c.OwnerAbandonTime = DateTime.MinValue;
						}
					}
					else if (c.Controlled && c.Commandable)
					{
						c.OwnerAbandonTime = DateTime.MinValue;

						if (c.Map != Map.Internal)
						{
							c.Loyalty -= (BaseCreature.MaxLoyalty / 10);

							if (c.Loyalty < (BaseCreature.MaxLoyalty / 10))
							{
								c.Say(1043270, c.Name); // * ~1_NAME~ looks around desperately *
								c.PlaySound(c.GetIdleSound());
							}

							if (c.Loyalty <= 0)
								lock (toRelease)
									toRelease.Add(c);
						}
					}

					// added lines to check if a wild creature in a house region has to be removed or not
					if (!c.Controlled && !c.IsStabled && ((c.Region.IsPartOf(typeof(HouseRegion)) && c.CanBeDamaged()) || (c.RemoveIfUntamed && c.Spawner == null)))
					{
						c.RemoveStep++;

						if (c.RemoveStep >= 20)
							lock (toRemove)
								toRemove.Add(c);
					}
					else
					{
						c.RemoveStep = 0;
					}

					break;
				}
			}
		});

		foreach (BaseCreature c in toRelease.Where(c => c != null))
		{
			if (c.IsDeadBondedPet)
			{
				c.Delete();
				continue;
			}

			c.Say(1043255, c.Name); // ~1_NAME~ appears to have decided that is better off without a master!
			c.Loyalty = BaseCreature.MaxLoyalty; // Wonderfully Happy
			c.IsBonded = false;
			c.BondingBegin = DateTime.MinValue;
			c.OwnerAbandonTime = DateTime.MinValue;
			c.ControlTarget = null;

			c.AIObject?.DoOrderRelease();
			// this will prevent no release of creatures left alone with AI disabled (and consequent bug of Followers)
			c.DropBackpack();
			c.RemoveOnSave = true;
		}

		// added code to handle removing of wild creatures in house regions
		foreach (BaseCreature c in toRemove)
		{
			c.Delete();
		}

		ColUtility.Free(toRelease);
		ColUtility.Free(toRemove);
	}
}
