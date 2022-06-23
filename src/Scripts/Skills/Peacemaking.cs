using Server.Items;
using Server.Mobiles;
using Server.Targeting;
using System;

namespace Server.SkillHandlers;

public class Peacemaking
{
	public static void Initialize()
	{
		SkillInfo.Table[(int)SkillName.Peacemaking].Callback = OnUse;
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
		from.SendLocalizedMessage(1049525); // Whom do you wish to calm?
		from.Target = new InternalTarget(from, instrument);
		from.NextSkillTime = Core.TickCount + 21600000;
	}

	private class InternalTarget : Target
	{
		private readonly BaseInstrument _mInstrument;
		private bool _mSetSkillTime = true;

		public InternalTarget(Mobile from, BaseInstrument instrument) : base(BaseInstrument.GetBardRange(from, SkillName.Peacemaking), false, TargetFlags.None)
		{
			_mInstrument = instrument;
		}

		protected override void OnTargetFinish(Mobile from)
		{
			if (_mSetSkillTime)
				from.NextSkillTime = Core.TickCount;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			from.RevealingAction();

			if (targeted is not Mobile mobile)
			{
				from.SendLocalizedMessage(1049528); // You cannot calm that!
			}
			else if (from.Region.IsPartOf(typeof(Engines.ConPVP.SafeZone)))
			{
				from.SendMessage("You may not peace make in this area.");
			}
			else if (mobile.Region.IsPartOf(typeof(Engines.ConPVP.SafeZone)))
			{
				from.SendMessage("You may not peace make there.");
			}
			else if (!_mInstrument.IsChildOf(from.Backpack))
			{
				from.SendLocalizedMessage(1062488); // The instrument you are trying to play is no longer in your backpack!
			}
			else
			{
				_mSetSkillTime = false;
				from.NextSkillTime = Core.TickCount + 10000;

				if (targeted == from)
				{
					// Standard mode : reset combatants for everyone in the area

					if (!BaseInstrument.CheckMusicianship(from))
					{
						from.SendLocalizedMessage(500612); // You play poorly, and there is no effect.
						_mInstrument.PlayInstrumentBadly(from);
						_mInstrument.ConsumeUse(from);
					}
					else if (!from.CheckSkill(SkillName.Peacemaking, 0.0, 120.0))
					{
						from.SendLocalizedMessage(500613); // You attempt to calm everyone, but fail.
						_mInstrument.PlayInstrumentBadly(from);
						_mInstrument.ConsumeUse(from);
					}
					else
					{
						from.NextSkillTime = Core.TickCount + 5000;
						_mInstrument.PlayInstrumentWell(from);
						_mInstrument.ConsumeUse(from);

						Map map = from.Map;

						if (map != null)
						{
							int range = BaseInstrument.GetBardRange(from, SkillName.Peacemaking);

							bool calmed = false;

							foreach (Mobile m in from.GetMobilesInRange(range))
							{
								if (m is BaseCreature {Uncalmable: true} || (m is BaseCreature creature && creature.AreaPeaceImmune) || m == from || !from.CanBeHarmful(m, false))
									continue;

								calmed = true;

								m.SendLocalizedMessage(500616); // You hear lovely music, and forget to continue battling!
								m.Combatant = null;
								m.Warmode = false;

								if (m is BaseCreature {BardPacified: false} bc)
									bc.Pacify(from, DateTime.UtcNow + TimeSpan.FromSeconds(1.0));
							}
							// You play hypnotic music, but there is nothing in range for you to calm.// You play your hypnotic music, stopping the battle.
							from.SendLocalizedMessage(!calmed ? 1049648 : 500615);
						}
					}
				}
				else
				{
					// Target mode : pacify a single target for a longer duration

					Mobile targ = mobile;

					if (!from.CanBeHarmful(targ, false))
					{
						from.SendLocalizedMessage(1049528);
						_mSetSkillTime = true;
					}
					else if (targ is BaseCreature {Uncalmable: true})
					{
						from.SendLocalizedMessage(1049526); // You have no chance of calming that creature.
						_mSetSkillTime = true;
					}
					else if (targ is BaseCreature {BardPacified: true})
					{
						from.SendLocalizedMessage(1049527); // That creature is already being calmed.
						_mSetSkillTime = true;
					}
					else if (!BaseInstrument.CheckMusicianship(from))
					{
						from.SendLocalizedMessage(500612); // You play poorly, and there is no effect.
						from.NextSkillTime = Core.TickCount + 5000;
						_mInstrument.PlayInstrumentBadly(from);
						_mInstrument.ConsumeUse(from);
					}
					else
					{
						double diff = _mInstrument.GetDifficultyFor(targ) - 10.0;
						double music = from.Skills[SkillName.Musicianship].Value;

						if (music > 100.0)
							diff -= (music - 100.0) * 0.5;

						if (!from.CheckTargetSkill(SkillName.Peacemaking, targ, diff - 25.0, diff + 25.0))
						{
							from.SendLocalizedMessage(1049531); // You attempt to calm your target, but fail.
							_mInstrument.PlayInstrumentBadly(from);
							_mInstrument.ConsumeUse(from);
						}
						else
						{
							_mInstrument.PlayInstrumentWell(from);
							_mInstrument.ConsumeUse(from);

							from.NextSkillTime = Core.TickCount + 5000;
							if (targ is BaseCreature targbc)
							{
								from.SendLocalizedMessage(1049532); // You play hypnotic music, calming your target.

								targ.Combatant = null;
								targ.Warmode = false;

								double seconds = 100 - (diff / 1.5);

								seconds = seconds switch
								{
									> 120 => 120,
									< 10 => 10,
									_ => seconds
								};

								targbc.Pacify(from, DateTime.UtcNow + TimeSpan.FromSeconds(seconds));
							}
							else
							{
								from.SendLocalizedMessage(1049532); // You play hypnotic music, calming your target.

								targ.SendLocalizedMessage(500616); // You hear lovely music, and forget to continue battling!
								targ.Combatant = null;
								targ.Warmode = false;
							}
						}
					}
				}
			}
		}
	}
}
