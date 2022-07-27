using System;
using System.Collections;
using Server.Items;

namespace Server.Spells.Ninjitsu;

public class KiAttack : NinjaMove
{
	private static readonly Hashtable m_Table = new();

	public override int BaseMana => 25;
	public override double RequiredSkill => 80.0;
	public override TextDefinition AbilityMessage => new(1063099);// Your Ki Attack must be complete within 2 seconds for the damage bonus!

	private static double GetBonus(Mobile from)
	{
		if (m_Table[from] is not KiAttackInfo info)
			return 0.0;

		int xDelta = info.Location.X - from.X;
		int yDelta = info.Location.Y - from.Y;

		double bonus = Math.Sqrt((xDelta * xDelta) + (yDelta * yDelta));

		if (bonus > 20.0)
			bonus = 20.0;

		return bonus;
	}

	public override void OnUse(Mobile from)
	{
		if (!Validate(from))
			return;

		KiAttackInfo info = new(from);
		info.Timer = Timer.DelayCall(TimeSpan.FromSeconds(2.0), new TimerStateCallback(EndKiAttack), info);

		m_Table[from] = info;
	}

	public override bool Validate(Mobile from)
	{
		if (from.Hidden && from.AllowedStealthSteps > 0)
		{
			from.SendLocalizedMessage(1063127); // You cannot use this ability while in stealth mode.
			return false;
		}

		if (!Core.ML)
			return base.Validate(from);

		if (from.Weapon is not BaseRanged)
			return base.Validate(from);

		from.SendLocalizedMessage(1075858); // You can only use this with melee attacks.
		return false;

	}

	public override double GetDamageScalar(Mobile attacker, Mobile defender)
	{
		if (attacker.Hidden)
			return 1.0;

		return 1.0 + GetBonus(attacker) / 10;
	}

	public override void OnHit(Mobile attacker, Mobile defender, int damage)
	{
		if (!Validate(attacker) || !CheckMana(attacker, true))
			return;

		if (GetBonus(attacker) == 0.0)
		{
			attacker.SendLocalizedMessage(1063101); // You were too close to your target to cause any additional damage.
		}
		else
		{
			attacker.FixedParticles(0x37BE, 1, 5, 0x26BD, 0x0, 0x1, EffectLayer.Waist);
			attacker.PlaySound(0x510);

			attacker.SendLocalizedMessage(1063100); // Your quick flight to your target causes extra damage as you strike!
			defender.FixedParticles(0x37BE, 1, 5, 0x26BD, 0, 0x1, EffectLayer.Waist);

			CheckGain(attacker);
		}

		ClearCurrentMove(attacker);
	}

	public override void OnClearMove(Mobile from)
	{
		if (m_Table[from] is not KiAttackInfo info)
			return;

		info.Timer?.Stop();

		m_Table.Remove(info.Mobile);
	}

	private static void EndKiAttack(object state)
	{
		KiAttackInfo info = (KiAttackInfo)state;

		info.Timer?.Stop();

		ClearCurrentMove(info.Mobile);
		info.Mobile.SendLocalizedMessage(1063102); // You failed to complete your Ki Attack in time.

		m_Table.Remove(info.Mobile);
	}

	private class KiAttackInfo
	{
		public readonly Mobile Mobile;
		public readonly Point3D Location;
		public Timer Timer;
		public KiAttackInfo(Mobile m)
		{
			Mobile = m;
			Location = m.Location;
		}
	}
}
