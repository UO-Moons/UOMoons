using System;
using Server.Mobiles;
using System.Collections.Generic;

namespace Server.Spells.Chivalry;

public class EnemyOfOneSpell : PaladinSpell
{
	private static readonly SpellInfo m_Info = new(
		"Enemy of One", "Forul Solum",
		-1,
		9002);

	public EnemyOfOneSpell(Mobile caster, Item scroll)
		: base(caster, scroll, m_Info)
	{
	}

	public override TimeSpan CastDelayBase => TimeSpan.FromSeconds(0.5);

	public override double RequiredSkill => 45.0;
	public override int RequiredMana => 20;
	public override int RequiredTithing => 10;
	public override int MantraNumber => 1060723;  // Forul Solum
	public override bool BlocksMovement => false;

	public override TimeSpan GetCastDelay()
	{
		TimeSpan delay = base.GetCastDelay();

		if (!Core.SA || !UnderEffect(Caster))
			return delay;

		double milliseconds = delay.TotalMilliseconds / 2;

		delay = TimeSpan.FromMilliseconds(milliseconds);

		return delay;
	}

	public override void OnCast()
	{
		if (Core.SA && UnderEffect(Caster))
		{
			PlayEffects();

			// As per Pub 71, Enemy of one has now been changed to a Spell Toggle. You can remove the effect
			// before the duration expires by recasting the spell.
			RemoveEffect(Caster);
		}
		else if (CheckSequence())
		{
			PlayEffects();

			// TODO: validate formula
			var seconds = ComputePowerValue(1);
			Utility.FixMinMax(ref seconds, 67, 228);

			var delay = TimeSpan.FromSeconds(seconds);

			var timer = Timer.DelayCall(delay, RemoveEffect, Caster);

			var expire = DateTime.UtcNow + delay;

			var context = new EnemyOfOneContext(Caster, timer, expire);
			context.OnCast();
			m_Table[Caster] = context;
		}

		FinishSequence();
	}

	private void PlayEffects()
	{
		Caster.PlaySound(0x0F5);
		Caster.PlaySound(0x1ED);

		Caster.FixedParticles(0x375A, 1, 30, 9966, 33, 2, EffectLayer.Head);
		Caster.FixedParticles(0x37B9, 1, 30, 9502, 43, 3, EffectLayer.Head);
	}

	private static readonly Dictionary<Mobile, EnemyOfOneContext> m_Table = new();

	public static EnemyOfOneContext GetContext(Mobile m)
	{
		return !m_Table.ContainsKey(m) ? null : m_Table[m];
	}

	public static bool UnderEffect(Mobile m)
	{
		return m_Table.ContainsKey(m);
	}

	public static void RemoveEffect(Mobile m)
	{
		if (!m_Table.ContainsKey(m))
			return;

		var context = m_Table[m];

		m_Table.Remove(m);

		context.OnRemoved();

		m.PlaySound(0x1F8);
	}

	private static Dictionary<Type, string> NameCache { get; set; }

	public static void Configure()
	{
		NameCache ??= new Dictionary<Type, string>();
	}

	public static string GetTypeName(Mobile defender)
	{
		if (defender is PlayerMobile || (defender is BaseCreature && ((BaseCreature)defender).GetMaster() is PlayerMobile))
		{
			return defender.Name;
		}

		Type t = defender.GetType();

		return NameCache.ContainsKey(t) ? NameCache[t] : AddNameToCache(t);
	}

	private static string AddNameToCache(Type t)
	{
		string name = t.Name;

		for (int i = 0; i < name.Length; i++)
		{
			if (i <= 0 || !char.IsUpper(name[i]))
				continue;

			name = name.Insert(i, " ");
			i++;
		}

		if (name.EndsWith("y"))
		{
			name = name.Substring(0, name.Length - 1);
			name += "ies";
		}
		else if (!name.EndsWith("s"))
		{
			name += "s";
		}


		NameCache[t] = name.ToLower();

		return name;
	}
}

public class EnemyOfOneContext
{
	private DateTime m_Expire;

	private Mobile m_PlayerOrPet;

	private Mobile Owner { get; }

	private Timer Timer { get; set; }

	private Type TargetType { get; set; }

	public int DamageScalar { get; private set; }

	private string TypeName { get; set; }

	public EnemyOfOneContext(Mobile owner, Timer timer, DateTime expire)
	{
		Owner = owner;
		Timer = timer;
		m_Expire = expire;
		TargetType = null;
		DamageScalar = 50;
	}

	public bool IsWaitingForEnemy => TargetType == null;

	public bool IsEnemy(Mobile m)
	{
		if (m is BaseCreature creature && creature.GetMaster() == Owner)
		{
			return false;
		}

		if (m_PlayerOrPet != null)
		{
			if (m_PlayerOrPet == m)
			{
				return true;
			}
		}
		else if (TargetType == m.GetType())
		{
			return true;
		}

		return false;
	}

	public void OnCast()
	{
		UpdateBuffInfo();
	}

	private void UpdateDamage()
	{
		var chivalry = (int)Owner.Skills.Chivalry.Value;
		DamageScalar = 10 + (chivalry - 40) * 9 / 10;

		if (m_PlayerOrPet != null)
			DamageScalar /= 2;
	}

	private void UpdateBuffInfo()
	{
		BuffInfo.AddBuff(Owner,
			TypeName == null
				? new BuffInfo(BuffIcon.EnemyOfOne, 1075653, 1075902, m_Expire - DateTime.UtcNow, Owner,
					$"{DamageScalar}\t{"100"}", true)
				: new BuffInfo(BuffIcon.EnemyOfOne, 1075653, 1075654, m_Expire - DateTime.UtcNow, Owner,
					$"{DamageScalar}\t{TypeName}\t.\t100", true));
	}

	public void OnHit(Mobile defender)
	{
		if (TargetType == null)
		{
			TypeName = EnemyOfOneSpell.GetTypeName(defender);

			if (defender is PlayerMobile || (defender is BaseCreature && ((BaseCreature)defender).GetMaster() is PlayerMobile))
			{
				m_PlayerOrPet = defender;
				TimeSpan duration = TimeSpan.FromSeconds(8);

				if (DateTime.UtcNow + duration < m_Expire)
				{
					m_Expire = DateTime.UtcNow + duration;
				}

				if (Timer != null)
				{
					Timer.Stop();
					Timer = null;
				}

				Timer = Timer.DelayCall(duration, EnemyOfOneSpell.RemoveEffect, Owner);
			}
			else
			{
				TargetType = defender.GetType();
			}

			UpdateDamage();
			DeltaEnemies();
			UpdateBuffInfo();
		}
		else if (Core.SA)
		{
			UpdateDamage();
		}
	}

	public void OnRemoved()
	{
		Timer?.Stop();

		DeltaEnemies();

		BuffInfo.RemoveBuff(Owner, BuffIcon.EnemyOfOne);
	}

	private void DeltaEnemies()
	{
		IPooledEnumerable eable = Owner.GetMobilesInRange(18);

		foreach (Mobile m in eable)
		{
			if (m_PlayerOrPet != null)
			{
				if (m == m_PlayerOrPet)
				{
					m.Delta(MobileDelta.Noto);
				}
			}
			else if (m.GetType() == TargetType)
			{
				m.Delta(MobileDelta.Noto);
			}
		}

		eable.Free();
	}
}
