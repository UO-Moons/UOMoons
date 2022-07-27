using System;
using System.Collections.Generic;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Spells.Spellweaving;

public class GiftOfLifeSpell : ArcanistSpell
{
	private static readonly SpellInfo m_Info = new(
		"Gift of Life", "Illorae",
		-1);

	private static readonly Dictionary<Mobile, ExpireTimer> m_Table = new();

	public GiftOfLifeSpell(Mobile caster, Item scroll)
		: base(caster, scroll, m_Info)
	{
	}

	public override TimeSpan CastDelayBase => TimeSpan.FromSeconds(4.0);
	public override double RequiredSkill => 38.0;
	public override int RequiredMana => 70;
	private double HitsScalar => (Caster.Skills.Spellweaving.Value / 2.4 + FocusLevel) / 100;
	public static void Initialize()
	{
		EventSink.OnMobileDeath += EventSink_HandleDeath;
		EventSink.OnLogin += Login;
	}

	private static void EventSink_HandleDeath(Mobile m, Mobile killer, Container cont)
	{
		HandleDeath(m);
	}

	public static void HandleDeath(Mobile m)
	{
		if (m_Table.ContainsKey(m))
			Timer.DelayCall(TimeSpan.FromSeconds(Utility.RandomMinMax(2, 4)), HandleDeath_OnCallback, m);
	}

	private static void Login(Mobile m)
	{
		if (!m_Table.ContainsKey(m))
			return;

		var timer = m_Table[m];

		if (timer.EndTime > DateTime.UtcNow)
		{
			BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.GiftOfLife, 1031615, 1075807, timer.EndTime - DateTime.UtcNow, m, null, true));
		}
	}

	public static void OnLogin(Mobile m)
	{
		if (m == null || m.Alive || m_Table[m] == null)
			return;

		HandleDeath_OnCallback(m);
	}

	public override void OnCast()
	{
		Caster.Target = new InternalTarget(this);
	}

	private void Target(Mobile m)
	{
		BaseCreature bc = m as BaseCreature;

		if (!Caster.CanSee(m))
		{
			Caster.SendLocalizedMessage(500237); // Target can not be seen.
		}
		else if (m.IsDeadBondedPet || !m.Alive)
		{
			// As per Osi: Nothing happens.
		}
		else if (m != Caster && (bc == null || !bc.IsBonded || bc.ControlMaster != Caster))
		{
			Caster.SendLocalizedMessage(1072077); // You may only cast this spell on yourself or a bonded pet.
		}
		else if (m_Table.ContainsKey(m))
		{
			Caster.SendLocalizedMessage(501775); // This spell is already in effect.
		}
		else if (CheckBSequence(m))
		{
			if (Caster == m)
			{
				Caster.SendLocalizedMessage(1074774); // You weave powerful magic, protecting yourself from death.
			}
			else
			{
				Caster.SendLocalizedMessage(1074775); // You weave powerful magic, protecting your pet from death.
				SpellHelper.Turn(Caster, m);
			}

			m.PlaySound(0x244);
			m.FixedParticles(0x3709, 1, 30, 0x26ED, 5, 2, EffectLayer.Waist);
			m.FixedParticles(0x376A, 1, 30, 0x251E, 5, 3, EffectLayer.Waist);

			double skill = Caster.Skills[SkillName.Spellweaving].Value;

			TimeSpan duration = TimeSpan.FromMinutes((int)(skill / 24) * 2 + FocusLevel);

			ExpireTimer t = new(m, duration, this);
			t.Start();

			m_Table[m] = t;

			BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.GiftOfLife, 1031615, 1075807, duration, m, null, true));
		}

		FinishSequence();
	}

	private static void HandleDeath_OnCallback(Mobile m)
	{
		if (m_Table.TryGetValue(m, out var timer))
		{
			double hitsScalar = timer.Spell.HitsScalar;

			if (m is BaseCreature { IsDeadBondedPet: true } pet)
			{
				Mobile master = pet.GetMaster();

				if (master is { NetState: { } } && master.InUpdateRange(pet))
				{
					master.CloseGump(typeof(PetResurrectGump));
					master.SendGump(new PetResurrectGump(master, pet, hitsScalar));
				}
				else
				{
					List<Mobile> friends = pet.Friends;

					for (int i = 0; friends != null && i < friends.Count; i++)
					{
						Mobile friend = friends[i];

						if (friend.NetState == null || !friend.InUpdateRange(pet))
							continue;

						friend.CloseGump(typeof(PetResurrectGump));
						friend.SendGump(new PetResurrectGump(friend, pet));
						break;
					}
				}
			}
			else
			{
				m.CloseGump(typeof(ResurrectGump));
				m.SendGump(new ResurrectGump(m, hitsScalar));
			}

			timer.DoExpire();
		}
	}

	private class InternalTarget : Target
	{
		private readonly GiftOfLifeSpell m_Owner;

		public InternalTarget(GiftOfLifeSpell owner)
			: base(10, false, TargetFlags.Beneficial)
		{
			m_Owner = owner;
		}

		protected override void OnTarget(Mobile m, object o)
		{
			if (o is Mobile mobile)
			{
				m_Owner.Target(mobile);
			}
			else
			{
				m.SendLocalizedMessage(1072077); // You may only cast this spell on yourself or a bonded pet.
			}
		}

		protected override void OnTargetFinish(Mobile m)
		{
			m_Owner.FinishSequence();
		}
	}

	private class ExpireTimer : Timer
	{
		private readonly Mobile m_Mobile;

		public DateTime EndTime { get; }

		public ExpireTimer(Mobile m, TimeSpan delay, GiftOfLifeSpell spell)
			: base(delay)
		{
			m_Mobile = m;
			Spell = spell;

			EndTime = DateTime.UtcNow + delay;
		}

		public GiftOfLifeSpell Spell { get; }

		public void DoExpire()
		{
			Stop();

			m_Mobile.SendLocalizedMessage(1074776); // You are no longer protected with Gift of Life.
			m_Table.Remove(m_Mobile);

			BuffInfo.RemoveBuff(m_Mobile, BuffIcon.GiftOfLife);
		}

		protected override void OnTick()
		{
			DoExpire();
		}
	}
}
