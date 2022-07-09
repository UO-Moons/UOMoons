using Server.Items;
using Server.Mobiles;
using Server.Spells.Seventh;
using System;
using System.Collections;

namespace Server.Spells.Fifth;

public class IncognitoSpell : MagerySpell
{
	private static readonly SpellInfo m_Info = new(
		"Incognito", "Kal In Ex",
		206,
		9002,
		Reagent.Bloodmoss,
		Reagent.Garlic,
		Reagent.Nightshade
	);

	public override SpellCircle Circle => SpellCircle.Fifth;
	public override bool RequireTarget => false;

	public IncognitoSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
	{
	}

	public override bool CheckCast()
	{
		if (Factions.Sigil.ExistsOn(Caster))
		{
			Caster.SendLocalizedMessage(1010445); // You cannot incognito if you have a sigil
			return false;
		}

		if (!Caster.CanBeginAction(typeof(IncognitoSpell)))
		{
			Caster.SendLocalizedMessage(1005559); // This spell is already in effect.
			return false;
		}

		if (Caster.BodyMod == 183 || Caster.BodyMod == 184)
		{
			Caster.SendLocalizedMessage(1042402); // You cannot use incognito while wearing body paint
			return false;
		}

		return true;
	}

	public override void OnCast()
	{
		if (Factions.Sigil.ExistsOn(Caster))
		{
			Caster.SendLocalizedMessage(1010445); // You cannot incognito if you have a sigil
		}
		else if (!Caster.CanBeginAction(typeof(IncognitoSpell)))
		{
			Caster.SendLocalizedMessage(1005559); // This spell is already in effect.
		}
		else if (Caster.BodyMod == 183 || Caster.BodyMod == 184)
		{
			Caster.SendLocalizedMessage(1042402); // You cannot use incognito while wearing body paint
		}
		else if (DisguiseTimers.IsDisguised(Caster))
		{
			Caster.SendLocalizedMessage(1061631); // You can't do that while disguised.
		}
		else if (!Caster.CanBeginAction(typeof(PolymorphSpell)) || Caster.IsBodyMod)
		{
			DoFizzle();
		}
		else if (CheckSequence())
		{
			if (Caster.BeginAction(typeof(IncognitoSpell)))
			{
				DisguiseTimers.StopTimer(Caster);

				Caster.HueMod = Caster.Race.RandomSkinHue();
				Caster.NameMod = Caster.Female ? NameList.RandomName("female") : NameList.RandomName("male");

				if (Caster is PlayerMobile pm && pm.Race != null)
				{
					pm.SetHairMods(pm.Race.RandomHair(pm.Female), pm.Race.RandomFacialHair(pm.Female));
					pm.HairHue = pm.Race.RandomHairHue();
					pm.FacialHairHue = pm.Race.RandomHairHue();
				}

				Caster.FixedParticles(0x373A, 10, 15, 5036, EffectLayer.Head);
				Caster.PlaySound(0x3BD);

				BaseEquipment.ValidateMobile(Caster);

				StopTimer(Caster);


				int timeVal = 6 * Caster.Skills.Magery.Fixed / 50 + 1;

				if (timeVal > 144)
					timeVal = 144;

				TimeSpan length = TimeSpan.FromSeconds(timeVal);


				Timer t = new InternalTimer(Caster, length);

				m_Timers[Caster] = t;

				t.Start();

				BuffInfo.AddBuff(Caster, new BuffInfo(BuffIcon.Incognito, 1075819, length, Caster));

			}
			else
			{
				Caster.SendLocalizedMessage(1079022); // You're already incognitoed!
			}
		}

		FinishSequence();
	}

	private static readonly Hashtable m_Timers = new();

	public static bool StopTimer(Mobile m)
	{
		Timer t = (Timer)m_Timers[m];

		if (t != null)
		{
			t.Stop();
			m_Timers.Remove(m);
			BuffInfo.RemoveBuff(m, BuffIcon.Incognito);
		}

		return t != null;
	}

	private static readonly int[] m_HairIDs = {
		0x2044, 0x2045, 0x2046,
		0x203C, 0x203B, 0x203D,
		0x2047, 0x2048, 0x2049,
		0x204A, 0x0000
	};

	private static readonly int[] m_BeardIDs = {
		0x203E, 0x203F, 0x2040,
		0x2041, 0x204B, 0x204C,
		0x204D, 0x0000
	};

	private class InternalTimer : Timer
	{
		private readonly Mobile _owner;

		public InternalTimer(Mobile owner, TimeSpan length) : base(length)
		{
			_owner = owner;

			/*
			int val = ((6 * owner.Skills.Magery.Fixed) / 50) + 1;

			if ( val > 144 )
				val = 144;

			Delay = TimeSpan.FromSeconds( val );
			 * */
			Priority = TimerPriority.OneSecond;
		}

		protected override void OnTick()
		{
			if (!_owner.CanBeginAction(typeof(IncognitoSpell)))
			{
				if (_owner is PlayerMobile mobile)
					mobile.SetHairMods(-1, -1);

				_owner.BodyMod = 0;
				_owner.HueMod = -1;
				_owner.NameMod = null;
				_owner.EndAction(typeof(IncognitoSpell));

				BaseEquipment.ValidateMobile(_owner);
			}
		}
	}
}
