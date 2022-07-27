using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Spells.Chivalry;

public class ConsecrateWeaponSpell : PaladinSpell
{
	private static readonly SpellInfo m_Info = new(
		"Consecrate Weapon", "Consecrus Arma",
		-1,
		9002);

	private static readonly Dictionary<Mobile, ConsecratedWeaponContext> m_Table = new();

	public ConsecrateWeaponSpell(Mobile caster, Item scroll)
		: base(caster, scroll, m_Info)
	{
	}

	public override TimeSpan CastDelayBase => TimeSpan.FromSeconds(0.5);
	public override double RequiredSkill => 15.0;
	public override int RequiredMana => 10;
	public override int RequiredTithing => 10;
	public override int MantraNumber => 1060720;// Consecrus Arma
	public override bool BlocksMovement => false;

	public override void OnCast()
	{
		BaseWeapon weapon = Caster.Weapon as BaseWeapon;

		if (Caster.Player && weapon is null or Fists)
		{
			Caster.SendLocalizedMessage(501078); // You must be holding a weapon.
		}
		else if (CheckSequence())
		{
			/* Temporarily enchants the weapon the caster is currently wielding.
			* The type of damage the weapon inflicts when hitting a target will
			* be converted to the target's worst Resistance type.
			* Duration of the effect is affected by the caster's Karma and lasts for 3 to 11 seconds.
			*/

			if (weapon != null)
			{
				int itemId;
				int soundId;
				switch (weapon.Skill)
				{
					case SkillName.Macing:
						itemId = 0xFB4;
						soundId = 0x232;
						break;
					case SkillName.Archery:
						itemId = 0x13B1;
						soundId = 0x145;
						break;
					default:
						itemId = 0xF5F;
						soundId = 0x56;
						break;
				}

				Caster.PlaySound(0x20C);
				Caster.PlaySound(soundId);
				Caster.FixedParticles(0x3779, 1, 30, 9964, 3, 3, EffectLayer.Waist);

				IEntity from = new Entity(Serial.Zero, new Point3D(Caster.X, Caster.Y, Caster.Z), Caster.Map);
				IEntity to = new Entity(Serial.Zero, new Point3D(Caster.X, Caster.Y, Caster.Z + 50), Caster.Map);
				Effects.SendMovingParticles(from, to, itemId, 1, 0, false, false, 33, 3, 9501, 1, 0, EffectLayer.Head,
					0x100);

				int pkarma = Caster.Karma;

				var seconds = pkarma switch
				{
					> 5000 => 11.0,
					>= 4999 => 10.0,
					>= 3999 => 9.00,
					>= 2999 => 8.0,
					>= 1999 => 7.0,
					>= 999 => 6.0,
					_ => 5.0
				};

				TimeSpan duration = TimeSpan.FromSeconds(seconds);
				ConsecratedWeaponContext context;

				if (IsUnderEffects(Caster))
				{
					context = m_Table[Caster];

					if (context.Timer != null)
					{
						context.Timer.Stop();
						context.Timer = null;
					}

					context.Weapon = weapon;
				}
				else
				{
					context = new ConsecratedWeaponContext(Caster, weapon);
				}

				weapon.ConsecratedContext = context;
				context.Timer = Timer.DelayCall(duration, RemoveEffects, Caster);

				m_Table[Caster] = context;

				BuffInfo.AddBuff(Caster,
					new BuffInfo(BuffIcon.ConsecrateWeapon, 1151385, 1151386, duration, Caster,
						$"{context.ConsecrateProcChance}\t{context.ConsecrateDamageBonus}"));
			}
		}

		FinishSequence();
	}

	private static bool IsUnderEffects(Mobile m)
	{
		return m_Table.ContainsKey(m);
	}

	private static void RemoveEffects(Mobile m)
	{
		if (m_Table.ContainsKey(m))
		{
			var context = m_Table[m];

			context.Expire();

			m_Table.Remove(m);
		}
	}
}

public class ConsecratedWeaponContext
{
	public Mobile Owner { get; }
	public BaseWeapon Weapon { get; set; }

	public Timer Timer { get; set; }

	public int ConsecrateProcChance
	{
		get
		{
			if (!Core.SA || Owner.Skills.Chivalry.Value >= 80)
			{
				return 100;
			}

			return (int)Owner.Skills.Chivalry.Value;
		}
	}

	public int ConsecrateDamageBonus
	{
		get
		{
			if (!Core.SA)
				return 0;

			double value = Owner.Skills.Chivalry.Value;

			if (value >= 90)
			{
				return (int)Math.Truncate((value - 90) / 2);
			}

			return 0;
		}
	}

	public ConsecratedWeaponContext(Mobile owner, BaseWeapon weapon)
	{
		Owner = owner;
		Weapon = weapon;
	}

	public void Expire()
	{
		Weapon.ConsecratedContext = null;

		Effects.PlaySound(Weapon.GetWorldLocation(), Weapon.Map, 0x1F8);

		if (Timer == null)
			return;

		Timer.Stop();
		Timer = null;
	}
}
