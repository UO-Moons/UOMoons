using Server.Items;
using Server.Mobiles;
using Server.Targeting;
using System;
using System.Collections;

namespace Server.SkillHandlers;

public class Discordance
{
	public static void Initialize()
	{
		SkillInfo.Table[(int)SkillName.Discordance].Callback = OnUse;
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
		from.SendLocalizedMessage(1049541); // Choose the target for your song of discordance.
		from.Target = new DiscordanceTarget(from, instrument);
		from.NextSkillTime = Core.TickCount + 6000;
	}

	private class DiscordanceInfo
	{
		public readonly Mobile MFrom;
		public readonly Mobile MCreature;
		public DateTime MEndTime;
		public bool MEnding;
		public Timer MTimer;
		public readonly int MEffect;
		public readonly ArrayList MMods;

		public DiscordanceInfo(Mobile from, Mobile creature, int effect, ArrayList mods)
		{
			MFrom = from;
			MCreature = creature;
			MEndTime = DateTime.UtcNow;
			MEnding = false;
			MEffect = effect;
			MMods = mods;

			Apply();
		}

		public void Apply()
		{
			for (int i = 0; i < MMods.Count; ++i)
			{
				object mod = MMods[i];

				switch (mod)
				{
					case ResistanceMod resistanceMod:
						MCreature.AddResistanceMod(resistanceMod);
						break;
					case StatMod statMod:
						MCreature.AddStatMod(statMod);
						break;
					case SkillMod skillMod:
						MCreature.AddSkillMod(skillMod);
						break;
				}
			}
		}

		public void Clear()
		{
			for (int i = 0; i < MMods.Count; ++i)
			{
				object mod = MMods[i];

				switch (mod)
				{
					case ResistanceMod resistanceMod:
						MCreature.RemoveResistanceMod(resistanceMod);
						break;
					case StatMod statMod:
						MCreature.RemoveStatMod(statMod.Name);
						break;
					case SkillMod skillMod:
						MCreature.RemoveSkillMod(skillMod);
						break;
				}
			}
		}
	}

	private static readonly Hashtable MTable = new();

	public static bool GetEffect(Mobile targ, ref int effect)
	{
		if (MTable[targ] is not DiscordanceInfo info)
			return false;

		effect = info.MEffect;
		return true;
	}

	private static void ProcessDiscordance(DiscordanceInfo info)
	{
		Mobile from = info.MFrom;
		Mobile targ = info.MCreature;
		bool ends = false;

		// According to uoherald bard must remain alive, visible, and
		// within range of the target or the effect ends in 15 seconds. needs more research.
		if (!targ.Alive || targ.Deleted || !from.Alive || from.Hidden)
		{
			ends = true;
		}
		else
		{
			int range = (int)targ.GetDistanceToSqrt(from);
			int maxRange = BaseInstrument.GetBardRange(from, SkillName.Discordance);

			if (from.Map != targ.Map || range > maxRange)
				ends = true;
		}

		if (ends && info.MEnding && info.MEndTime < DateTime.UtcNow)
		{
			info.MTimer?.Stop();

			info.Clear();
			MTable.Remove(targ);
		}
		else
		{
			switch (ends)
			{
				case true when !info.MEnding:
					info.MEnding = true;
					info.MEndTime = DateTime.UtcNow + TimeSpan.FromSeconds(15);
					break;
				case false:
					info.MEnding = false;
					info.MEndTime = DateTime.UtcNow;
					break;
			}

			targ.FixedEffect(0x376A, 1, 32);
		}
	}

	public class DiscordanceTarget : Target
	{
		private readonly BaseInstrument _mInstrument;

		public DiscordanceTarget(Mobile from, BaseInstrument inst) : base(BaseInstrument.GetBardRange(from, SkillName.Discordance), false, TargetFlags.None)
		{
			_mInstrument = inst;
		}

		protected override void OnTarget(Mobile from, object target)
		{
			from.RevealingAction();
			from.NextSkillTime = Core.TickCount + 1000;

			if (!_mInstrument.IsChildOf(from.Backpack))
			{
				from.SendLocalizedMessage(1062488); // The instrument you are trying to play is no longer in your backpack!
			}
			else if (target is Mobile targ)
			{
				if (targ == from || (targ is BaseCreature creature && (creature.BardImmune || !from.CanBeHarmful(targ, false)) && creature.ControlMaster != from))
				{
					from.SendLocalizedMessage(1049535); // A song of discord would have no effect on that.
				}
				else if (MTable.Contains(targ)) //Already discorded
				{
					from.SendLocalizedMessage(1049537);// Your target is already in discord.
				}
				else if (!targ.Player)
				{
					double diff = _mInstrument.GetDifficultyFor(targ) - 10.0;
					double music = from.Skills[SkillName.Musicianship].Value;

					if (music > 100.0)
						diff -= (music - 100.0) * 0.5;

					if (!BaseInstrument.CheckMusicianship(from))
					{
						from.SendLocalizedMessage(500612); // You play poorly, and there is no effect.
						_mInstrument.PlayInstrumentBadly(from);
						_mInstrument.ConsumeUse(from);
					}
					else if (from.CheckTargetSkill(SkillName.Discordance, target, diff - 25.0, diff + 25.0))
					{
						from.SendLocalizedMessage(1049539); // You play the song surpressing your targets strength
						_mInstrument.PlayInstrumentWell(from);
						_mInstrument.ConsumeUse(from);

						ArrayList mods = new();
						int effect;
						double scalar;

						if (Core.AOS)
						{
							double discord = from.Skills[SkillName.Discordance].Value;

							if (discord > 100.0)
								effect = -20 + (int)((discord - 100.0) / -2.5);
							else
								effect = (int)(discord / -5.0);

							if (Core.SE && BaseInstrument.GetBaseDifficulty(targ) >= 160.0)
								effect /= 2;

							scalar = effect * 0.01;

							mods.Add(new ResistanceMod(ResistanceType.Physical, effect));
							mods.Add(new ResistanceMod(ResistanceType.Fire, effect));
							mods.Add(new ResistanceMod(ResistanceType.Cold, effect));
							mods.Add(new ResistanceMod(ResistanceType.Poison, effect));
							mods.Add(new ResistanceMod(ResistanceType.Energy, effect));

							for (int i = 0; i < targ.Skills.Length; ++i)
							{
								if (targ.Skills[i].Value > 0)
									mods.Add(new DefaultSkillMod((SkillName)i, true, targ.Skills[i].Value * scalar));
							}
						}
						else
						{
							effect = (int)(from.Skills[SkillName.Discordance].Value / -5.0);
							scalar = effect * 0.01;

							mods.Add(new StatMod(StatType.Str, "DiscordanceStr", (int)(targ.RawStr * scalar), TimeSpan.Zero));
							mods.Add(new StatMod(StatType.Int, "DiscordanceInt", (int)(targ.RawInt * scalar), TimeSpan.Zero));
							mods.Add(new StatMod(StatType.Dex, "DiscordanceDex", (int)(targ.RawDex * scalar), TimeSpan.Zero));

							for (int i = 0; i < targ.Skills.Length; ++i)
							{
								if (targ.Skills[i].Value > 0)
									mods.Add(new DefaultSkillMod((SkillName)i, true, targ.Skills[i].Value * scalar));
							}
						}

						DiscordanceInfo info = new(from, targ, Math.Abs(effect), mods);
						info.MTimer = Timer.DelayCall(TimeSpan.Zero, TimeSpan.FromSeconds(1.25), ProcessDiscordance, info);

						MTable[targ] = info;
					}
					else
					{
						from.SendLocalizedMessage(1049540);// You fail to disrupt your target
						_mInstrument.PlayInstrumentBadly(from);
						_mInstrument.ConsumeUse(from);
					}

					from.NextSkillTime = Core.TickCount + 12000;
				}
				else
				{
					_mInstrument.PlayInstrumentBadly(from);
				}
			}
			else
			{
				from.SendLocalizedMessage(1049535); // A song of discord would have no effect on that.
			}
		}
	}
}
