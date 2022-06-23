using Server.Items;
using Server.Mobiles;
using Server.Targeting;
using System;

namespace Server.SkillHandlers;

public class Provocation
{
	public static void Initialize()
	{
		SkillInfo.Table[(int)SkillName.Provocation].Callback = OnUse;
	}

	public static TimeSpan OnUse(Mobile m)
	{
		m.RevealingAction();

		BaseInstrument.PickInstrument(m, OnPickedInstrument);

		return TimeSpan.FromSeconds(1.0); // Cannot use another skill for 1 second
	}

	public static void OnPickedInstrument(Mobile from, BaseInstrument instrument)
	{
		from.RevealingAction();
		from.SendLocalizedMessage(501587); // Whom do you wish to incite?
		from.Target = new InternalFirstTarget(from, instrument);
	}

	public class InternalFirstTarget : Target
	{
		private readonly BaseInstrument _mInstrument;

		public InternalFirstTarget(Mobile from, BaseInstrument instrument) : base(BaseInstrument.GetBardRange(from, SkillName.Provocation), false, TargetFlags.None)
		{
			_mInstrument = instrument;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			from.RevealingAction();

			if (targeted is BaseCreature creature && from.CanBeHarmful(creature, true))
			{
				if (!_mInstrument.IsChildOf(from.Backpack))
				{
					from.SendLocalizedMessage(1062488); // The instrument you are trying to play is no longer in your backpack!
				}
				else if (creature.Controlled)
				{
					from.SendLocalizedMessage(501590); // They are too loyal to their master to be provoked.
				}
				else if (creature.IsParagon && BaseInstrument.GetBaseDifficulty(creature) >= 160.0)
				{
					from.SendLocalizedMessage(1049446); // You have no chance of provoking those creatures.
				}
				else
				{
					from.RevealingAction();
					_mInstrument.PlayInstrumentWell(from);
					from.SendLocalizedMessage(1008085); // You play your music and your target becomes angered.  Whom do you wish them to attack?
					from.Target = new InternalSecondTarget(from, _mInstrument, creature);
				}
			}
			else
			{
				from.SendLocalizedMessage(501589); // You can't incite that!
			}
		}
	}

	public class InternalSecondTarget : Target
	{
		private readonly BaseCreature _mCreature;
		private readonly BaseInstrument _mInstrument;

		public InternalSecondTarget(Mobile from, BaseInstrument instrument, BaseCreature creature) : base(BaseInstrument.GetBardRange(from, SkillName.Provocation), false, TargetFlags.None)
		{
			_mInstrument = instrument;
			_mCreature = creature;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			from.RevealingAction();

			if (targeted is BaseCreature creature)
			{
				if (!_mInstrument.IsChildOf(from.Backpack))
				{
					from.SendLocalizedMessage(1062488); // The instrument you are trying to play is no longer in your backpack!
				}
				else if (_mCreature.Unprovokable)
				{
					from.SendLocalizedMessage(1049446); // You have no chance of provoking those creatures.
				}
				else if (creature.Unprovokable && !(creature is DemonKnight))
				{
					from.SendLocalizedMessage(1049446); // You have no chance of provoking those creatures.
				}
				else if (_mCreature.Map != creature.Map || !_mCreature.InRange(creature, BaseInstrument.GetBardRange(from, SkillName.Provocation)))
				{
					from.SendLocalizedMessage(1049450); // The creatures you are trying to provoke are too far away from each other for your music to have an effect.
				}
				else if (_mCreature != creature)
				{
					from.NextSkillTime = Core.TickCount + 10000;

					double diff = ((_mInstrument.GetDifficultyFor(_mCreature) + _mInstrument.GetDifficultyFor(creature)) * 0.5) - 5.0;
					double music = from.Skills[SkillName.Musicianship].Value;

					if (music > 100.0)
						diff -= (music - 100.0) * 0.5;

					if (from.CanBeHarmful(_mCreature, true) && from.CanBeHarmful(creature, true))
					{
						if (!BaseInstrument.CheckMusicianship(from))
						{
							from.NextSkillTime = Core.TickCount + 5000;
							from.SendLocalizedMessage(500612); // You play poorly, and there is no effect.
							_mInstrument.PlayInstrumentBadly(from);
							_mInstrument.ConsumeUse(from);
						}
						else
						{
							//from.DoHarmful( m_Creature );
							//from.DoHarmful( creature );

							if (!from.CheckTargetSkill(SkillName.Provocation, creature, diff - 25.0, diff + 25.0))
							{
								from.NextSkillTime = Core.TickCount + 5000;
								from.SendLocalizedMessage(501599); // Your music fails to incite enough anger.
								_mInstrument.PlayInstrumentBadly(from);
								_mInstrument.ConsumeUse(from);
							}
							else
							{
								from.SendLocalizedMessage(501602); // Your music succeeds, as you start a fight.
								_mInstrument.PlayInstrumentWell(from);
								_mInstrument.ConsumeUse(from);
								_mCreature.Provoke(from, creature, true);
							}
						}
					}
				}
				else
				{
					from.SendLocalizedMessage(501593); // You can't tell someone to attack themselves!
				}
			}
			else
			{
				from.SendLocalizedMessage(501589); // You can't incite that!
			}
		}
	}
}
