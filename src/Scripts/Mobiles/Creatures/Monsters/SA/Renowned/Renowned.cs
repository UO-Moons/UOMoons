using System;
using Server.Items;

namespace Server.Mobiles;

public class Renowned
{
	public static readonly double InfectedChestChance = .10;

	public static readonly Map[] Maps = {
		Map.Felucca,
		Map.Trammel
	};

	private class RenownedStamRegen : Timer
	{
		private readonly BaseCreature m_Owner;

		public RenownedStamRegen(IEntity m)
			: base(TimeSpan.FromSeconds(.5), TimeSpan.FromSeconds(2))
		{
			Priority = TimerPriority.FiftyMs;
			m_Owner = m as BaseCreature;
		}

		protected override void OnTick()
		{
			if (!m_Owner.Deleted && m_Owner.IsRenowned && m_Owner.Map != Map.Internal)
			{
				m_Owner.Stam++;

				Delay = Interval = m_Owner.Stam < m_Owner.StamMax * .75 ? TimeSpan.FromSeconds(.5) : TimeSpan.FromSeconds(2);
			}
			else
			{
				Stop();
			}
		}
	}

	// Buffs
	private const double HitsBuff = 4.0;
	private const double StrBuff = 4.05;
	private const double IntBuff = 4.20;
	private const double DexBuff = 4.20;
	private const double SkillsBuff = 4.20;
	private const double SpeedBuff = 4.20;
	private const double FameBuff = 4.40;
	private const double KarmaBuff = 4.40;
	private const int DamageBuff = 20;
	private const int PhysicalBuff = 20;
	private const int FireBuff = 20;
	private const int ColdBuff = 20;
	private const int PoisonBuff = 20;
	private const int EnergyBuff = 20;

	public static void Convert(BaseCreature bc)
	{
		if (bc.IsRenowned || !bc.CanBeRenowned)
		{
			return;
		}

		if (bc.HitsMaxSeed >= 0)
		{
			bc.HitsMaxSeed = (int)(bc.HitsMaxSeed * HitsBuff);
		}

		bc.RawStr = (int)(bc.RawStr * StrBuff);
		bc.RawInt = (int)(bc.RawInt * IntBuff);
		bc.RawDex = (int)(bc.RawDex * DexBuff);

		bc.Hits = bc.HitsMax;
		bc.Mana = bc.ManaMax;
		bc.Stam = bc.StamMax;

		for (int i = 0; i < bc.Skills.Length; i++)
		{
			Skill skill = bc.Skills[i];

			if (skill.Base > 0.0)
			{
				skill.Base *= SkillsBuff;
			}
		}

		bc.PassiveSpeed /= SpeedBuff;
		bc.ActiveSpeed /= SpeedBuff;
		bc.CurrentSpeed = bc.PassiveSpeed;

		bc.DamageMin += DamageBuff;
		bc.DamageMax += DamageBuff;

		if (bc.Fame > 0)
		{
			bc.Fame = (int)(bc.Fame * FameBuff);
		}

		if (bc.Fame > 32000)
		{
			bc.Fame = 32000;
		}

		if (bc.PhysicalResistanceSeed >= 0)
		{
			bc.PhysicalResistanceSeed *= PhysicalBuff;
		}

		if (bc.FireResistSeed >= 0)
		{
			bc.FireResistSeed *= FireBuff;
		}

		if (bc.ColdResistSeed >= 0)
		{
			bc.ColdResistSeed *= ColdBuff;
		}

		if (bc.PoisonResistSeed >= 0)
		{
			bc.PoisonResistSeed *= PoisonBuff;
		}

		if (bc.EnergyResistSeed >= 0)
		{
			bc.EnergyResistSeed *= EnergyBuff;
		}

		if (bc.Karma != 0)
		{
			bc.Karma = (int)(bc.Karma * KarmaBuff);

			if (Math.Abs(bc.Karma) > 32000)
			{
				bc.Karma = 32000 * Math.Sign(bc.Karma);
			}
		}

		new RenownedStamRegen(bc).Start();
	}

	public static void UnConvert(BaseCreature bc)
	{
		if (!bc.IsRenowned)
		{
			return;
		}

		if (bc.HitsMaxSeed >= 0)
		{
			bc.HitsMaxSeed = (int)(bc.HitsMaxSeed / HitsBuff);
		}

		bc.RawStr = (int)(bc.RawStr / StrBuff);
		bc.RawInt = (int)(bc.RawInt / IntBuff);
		bc.RawDex = (int)(bc.RawDex / DexBuff);

		bc.Hits = bc.HitsMax;
		bc.Mana = bc.ManaMax;
		bc.Stam = bc.StamMax;

		for (int i = 0; i < bc.Skills.Length; i++)
		{
			Skill skill = bc.Skills[i];

			if (skill.Base > 0.0)
			{
				skill.Base /= SkillsBuff;
			}
		}

		bc.PassiveSpeed *= SpeedBuff;
		bc.ActiveSpeed *= SpeedBuff;

		bc.DamageMin -= DamageBuff;
		bc.DamageMax -= DamageBuff;

		if (bc.Fame > 0)
		{
			bc.Fame = (int)(bc.Fame / FameBuff);
		}

		if (bc.Karma != 0)
		{
			bc.Karma = (int)(bc.Karma / KarmaBuff);
		}
	}

	public static bool CheckConvert(BaseCreature bc)
	{
		return CheckConvert(bc, bc.Location, bc.Map);
	}

	public static bool CheckConvert(BaseCreature bc, Point3D location, Map m)
	{
		if (!Core.SA)
		{
			return false;
		}

		if (Array.IndexOf(Maps, m) == -1)
		{
			return false;
		}

		if (bc is BaseChampion or Harrower or BaseVendor or BaseEscortable || bc.Summoned || bc.Controlled || bc is MirrorImageClone || bc.IsRenowned)
		{
			return false;
		}

		if (bc is DreadHorn or MonstrousInterredGrizzle or Travesty or ChiefParoxysmus or LadyMelisande or ShimmeringEffusion || bc.IsParagon || bc.IsBlackRock)
		{
			return false;
		}

		int fame = bc.Fame;

		if (fame > 32000)
		{
			fame = 32000;
		}

		double chance = 1 / Math.Round(20.0 - fame / 3200);

		return chance > Utility.RandomDouble();
	}
}
