using Server.Engines.Champions;
using Server.Mobiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Server.Accounting;

namespace Server.Misc
{
	public class Titles
	{
		public const int MinFame = 0;
		public const int MaxFame = 15000;

		public static void AwardFame(Mobile m, int offset, bool message)
		{
			switch (offset)
			{
				case > 0 when m.Fame >= MaxFame:
					return;
				case > 0:
				{
					offset -= m.Fame / 100;

					if (offset < 0)
						offset = 0;
					break;
				}
				case < 0 when m.Fame <= MinFame:
					return;
				case < 0:
				{
					offset -= m.Fame / 100;

					if (offset > 0)
						offset = 0;
					break;
				}
			}

			offset = (m.Fame + offset) switch
			{
				> MaxFame => MaxFame - m.Fame,
				< MinFame => MinFame - m.Fame,
				_ => offset
			};

			m.Fame += offset;

			if (!message) return;
			switch (offset)
			{
				case > 40:
					m.SendLocalizedMessage(1019054); // You have gained a lot of fame.
					break;
				case > 20:
					m.SendLocalizedMessage(1019053); // You have gained a good amount of fame.
					break;
				case > 10:
					m.SendLocalizedMessage(1019052); // You have gained some fame.
					break;
				case > 0:
					m.SendLocalizedMessage(1019051); // You have gained a little fame.
					break;
				case < -40:
					m.SendLocalizedMessage(1019058); // You have lost a lot of fame.
					break;
				case < -20:
					m.SendLocalizedMessage(1019057); // You have lost a good amount of fame.
					break;
				case < -10:
					m.SendLocalizedMessage(1019056); // You have lost some fame.
					break;
				case < 0:
					m.SendLocalizedMessage(1019055); // You have lost a little fame.
					break;
			}
		}

		public const int MinKarma = -15000;
		public const int MaxKarma = 15000;

		public static void AwardKarma(Mobile m, int offset, bool message)
		{
			if (offset > 0)
			{
				if (m is PlayerMobile player && player.KarmaLocked)
					return;

				if (m.Karma >= MaxKarma)
					return;

				offset -= m.Karma / 100;

				if (offset < 0)
					offset = 0;
			}
			else if (offset < 0)
			{
				if (m.Karma <= MinKarma)
					return;

				offset -= m.Karma / 100;

				if (offset > 0)
					offset = 0;
			}

			if ((m.Karma + offset) > MaxKarma)
				offset = MaxKarma - m.Karma;
			else if ((m.Karma + offset) < MinKarma)
				offset = MinKarma - m.Karma;

			bool wasPositiveKarma = (m.Karma >= 0);

			m.Karma += offset;

			if (message)
			{
				if (offset > 40)
					m.SendLocalizedMessage(1019062); // You have gained a lot of karma.
				else if (offset > 20)
					m.SendLocalizedMessage(1019061); // You have gained a good amount of karma.
				else if (offset > 10)
					m.SendLocalizedMessage(1019060); // You have gained some karma.
				else if (offset > 0)
					m.SendLocalizedMessage(1019059); // You have gained a little karma.
				else if (offset < -40)
					m.SendLocalizedMessage(1019066); // You have lost a lot of karma.
				else if (offset < -20)
					m.SendLocalizedMessage(1019065); // You have lost a good amount of karma.
				else if (offset < -10)
					m.SendLocalizedMessage(1019064); // You have lost some karma.
				else if (offset < 0)
					m.SendLocalizedMessage(1019063); // You have lost a little karma.
			}

			if (!Core.AOS && wasPositiveKarma && m.Karma < 0 && m is PlayerMobile mobile && !mobile.KarmaLocked)
			{
				mobile.KarmaLocked = true;
				m.SendLocalizedMessage(1042511, 0x22); // Karma is locked.  A mantra spoken at a shrine will unlock it again.
			}
		}

		public static List<string> GetFameKarmaEntries(Mobile m)
		{
			List<string> list = new();
			int fame = m.Fame;
			int karma = m.Karma;

			for (var i = 0; i < MFameEntries.Length; ++i)
			{
				FameEntry fe = MFameEntries[i];

				if (fame < fe.Fame) continue;
				KarmaEntry[] karmaEntries = fe.Karma;

				for (var j = 0; j < karmaEntries.Length; ++j)
				{
					KarmaEntry ke = karmaEntries[j];
					StringBuilder title = new StringBuilder();

					if ((karma >= 0 && ke.Karma >= 0 && karma >= ke.Karma) || (karma < 0 && ke.Karma < 0 && karma < ke.Karma))
					{
						list.Add(title.AppendFormat(ke.Title, m.Name, m.Female ? "Lady" : "Lord").ToString());
					}
				}
			}

			return list;
		}
		public static readonly string[] HarrowerTitles = { "Spite", "Opponent", "Hunter", "Venom", "Executioner", "Annihilator", "Champion", "Assailant", "Purifier", "Nullifier" };

		public static string ComputeFameTitle(Mobile beheld)
		{
			int fame = beheld.Fame;
			int karma = beheld.Karma;

			for (int i = 0; i < MFameEntries.Length; ++i)
			{
				FameEntry fe = MFameEntries[i];

				if (fame <= fe.Fame || i == (MFameEntries.Length - 1))
				{
					KarmaEntry[] karmaEntries = fe.Karma;

					for (int j = 0; j < karmaEntries.Length; ++j)
					{
						KarmaEntry ke = karmaEntries[j];

						if (karma <= ke.Karma || j == (karmaEntries.Length - 1))
						{
							return string.Format(ke.Title, beheld.Name, beheld.Female ? "Lady" : "Lord");
						}
					}

					return string.Empty;
				}
			}
			return string.Empty;
		}

		public static string ComputeTitle(Mobile beholder, Mobile beheld)
		{
			StringBuilder title = new();

			int fame = beheld.Fame;
			int karma = beheld.Karma;

			bool showSkillTitle = beheld.ShowFameTitle && (beholder == beheld || fame >= 5000);

			/*if ( beheld.Murderer )
			{
				title.AppendFormat( beheld.Fame >= 10000 ? "The Murderer {1} {0}" : "The Murderer {0}", beheld.Name, beheld.Female ? "Lady" : "Lord" );
			}
			else*/
			var pm = beheld as PlayerMobile;
			if (Core.SA && beheld.ShowFameTitle && pm is {FameKarmaTitle: { }})
			{
				title.AppendFormat(pm.FameKarmaTitle, beheld.Name, beheld.Female ? "Lady" : "Lord");
			}
			else if(beheld.ShowFameTitle || beholder == beheld)
			{
				title.Append(ComputeFameTitle(beheld));
			}
			else
			{
				title.Append(beheld.Name);
			}


			if (pm is {DisplayChampionTitle: true})
			{
				ChampionTitleInfo info = pm.ChampionTitles;

				if (Core.SA)
				{
					if (pm.CurrentChampTitle != null)
						title.AppendFormat(pm.CurrentChampTitle);
				}
				else if (info.Harrower > 0)
					title.Append($": {HarrowerTitles[Math.Min(HarrowerTitles.Length, info.Harrower) - 1]} of Evil");
				else
				{
					int highestValue = 0, highestType = 0;
					for (var i = 0; i < ChampionSpawnInfo.Table.Length; i++)
					{
						int v = info.GetValue(i);

						if (v <= highestValue) continue;
						highestValue = v;
						highestType = i;
					}

					int offset = highestValue switch
					{
						> 800 => 3,
						> 300 => highestValue / 300,
						_ => 0
					};

					if (offset > 0)
					{
						var champInfo = ChampionSpawnInfo.GetInfo((ChampionSpawnType)highestType);
						title.AppendFormat(": {0} of the {1}", champInfo.LevelNames[Math.Min(offset, champInfo.LevelNames.Length) - 1], champInfo.Name);
					}
				}
			}

			string customTitle = beheld.Title;

			if (Core.SA)
			{
				if (pm is {PaperdollSkillTitle: { }})
					title.Append(", ").Append(pm.PaperdollSkillTitle);
				else if (beheld is BaseVendor)
					title.Append($" {customTitle}");
			}
			else if (customTitle != null && (customTitle = customTitle.Trim()).Length > 0)
			{
				title.Append($" {customTitle}");
			}
			else if (showSkillTitle && beheld.Player)
			{
				string skillTitle = GetSkillTitle(beheld);

				if (skillTitle != null)
				{
					title.Append(", ").Append(skillTitle);
				}
			}

			return title.ToString();
		}

		public static string GetSkillTitle(Mobile mob)
		{
			Skill highest = GetHighestSkill(mob);// beheld.Skills.Highest;

			if (highest == null || highest.BaseFixedPoint < 300) return null;
			string skillLevel = GetSkillLevel(highest);
			string skillTitle = highest.Info.Title;

			if (mob.Female && skillTitle.EndsWith("man"))
				skillTitle = skillTitle[..^3] + "woman";

			return string.Concat(skillLevel, " ", skillTitle);

		}

		public static string GetSkillTitle(Mobile mob, Skill skill)
		{
			if (skill == null || skill.BaseFixedPoint < 300) return null;
			string skillLevel = GetSkillLevel(skill);
			string skillTitle = skill.Info.Title;

			if (mob.Female && skillTitle.EndsWith("man"))
				skillTitle = skillTitle[..^3] + "woman";

			return string.Concat(skillLevel, " ", skillTitle);

		}

		private static Skill GetHighestSkill(Mobile m)
		{
			Skills skills = m.Skills;

			if (!Core.AOS)
				return skills.Highest;

			Skill highest = null;

			for (var i = 0; i < m.Skills.Length; ++i)
			{
				Skill check = m.Skills[i];

				if (highest == null || check.BaseFixedPoint > highest.BaseFixedPoint)
					highest = check;
				else if (highest.Lock != SkillLock.Up && check.Lock == SkillLock.Up && check.BaseFixedPoint == highest.BaseFixedPoint)
					highest = check;
			}

			return highest;
		}

		private static readonly string[,] MLevels = {
				{ "Neophyte",       "Neophyte",     "Neophyte"      },
				{ "Novice",         "Novice",       "Novice"        },
				{ "Apprentice",     "Apprentice",   "Apprentice"    },
				{ "Journeyman",     "Journeyman",   "Journeyman"    },
				{ "Expert",         "Expert",       "Expert"        },
				{ "Adept",          "Adept",        "Adept"         },
				{ "Master",         "Master",       "Master"        },
				{ "Grandmaster",    "Grandmaster",  "Grandmaster"   },
				{ "Elder",          "Tatsujin",     "Shinobi"       },
				{ "Legendary",      "Kengo",        "Ka-ge"         }
			};

		private static string GetSkillLevel(Skill skill)
		{
			return MLevels[GetTableIndex(skill), GetTableType(skill)];
		}

		private static int GetTableType(Skill skill)
		{
			return skill.SkillName switch
			{
				SkillName.Bushido => 1,
				SkillName.Ninjitsu => 2,
				_ => 0,
			};
		}

		private static int GetTableIndex(Skill skill)
		{
			int fp = Math.Min(skill.BaseFixedPoint, 1200);

			return (fp - 300) / 100;
		}

		private static readonly FameEntry[] MFameEntries = {
				new( 1249, new[]
				{
					new KarmaEntry( -10000, "The Outcast {0}" ),
					new KarmaEntry( -5000, "The Despicable {0}" ),
					new KarmaEntry( -2500, "The Scoundrel {0}" ),
					new KarmaEntry( -1250, "The Unsavory {0}" ),
					new KarmaEntry( -625, "The Rude {0}" ),
					new KarmaEntry( 624, "{0}" ),
					new KarmaEntry( 1249, "The Fair {0}" ),
					new KarmaEntry( 2499, "The Kind {0}" ),
					new KarmaEntry( 4999, "The Good {0}" ),
					new KarmaEntry( 9999, "The Honest {0}" ),
					new KarmaEntry( 10000, "The Trustworthy {0}" )
				} ),
				new( 2499, new[]
				{
					new KarmaEntry( -10000, "The Wretched {0}" ),
					new KarmaEntry( -5000, "The Dastardly {0}" ),
					new KarmaEntry( -2500, "The Malicious {0}" ),
					new KarmaEntry( -1250, "The Dishonorable {0}" ),
					new KarmaEntry( -625, "The Disreputable {0}" ),
					new KarmaEntry( 624, "The Notable {0}" ),
					new KarmaEntry( 1249, "The Upstanding {0}" ),
					new KarmaEntry( 2499, "The Respectable {0}" ),
					new KarmaEntry( 4999, "The Honorable {0}" ),
					new KarmaEntry( 9999, "The Commendable {0}" ),
					new KarmaEntry( 10000, "The Estimable {0}" )
				} ),
				new( 4999, new[]
				{
					new KarmaEntry( -10000, "The Nefarious {0}" ),
					new KarmaEntry( -5000, "The Wicked {0}" ),
					new KarmaEntry( -2500, "The Vile {0}" ),
					new KarmaEntry( -1250, "The Ignoble {0}" ),
					new KarmaEntry( -625, "The Notorious {0}" ),
					new KarmaEntry( 624, "The Prominent {0}" ),
					new KarmaEntry( 1249, "The Reputable {0}" ),
					new KarmaEntry( 2499, "The Proper {0}" ),
					new KarmaEntry( 4999, "The Admirable {0}" ),
					new KarmaEntry( 9999, "The Famed {0}" ),
					new KarmaEntry( 10000, "The Great {0}" )
				} ),
				new( 9999, new[]
				{
					new KarmaEntry( -10000, "The Dread {0}" ),
					new KarmaEntry( -5000, "The Evil {0}" ),
					new KarmaEntry( -2500, "The Villainous {0}" ),
					new KarmaEntry( -1250, "The Sinister {0}" ),
					new KarmaEntry( -625, "The Infamous {0}" ),
					new KarmaEntry( 624, "The Renowned {0}" ),
					new KarmaEntry( 1249, "The Distinguished {0}" ),
					new KarmaEntry( 2499, "The Eminent {0}" ),
					new KarmaEntry( 4999, "The Noble {0}" ),
					new KarmaEntry( 9999, "The Illustrious {0}" ),
					new KarmaEntry( 10000, "The Glorious {0}" )
				} ),
				new( 10000, new[]
				{
					new KarmaEntry( -10000, "The Dread {1} {0}" ),
					new KarmaEntry( -5000, "The Evil {1} {0}" ),
					new KarmaEntry( -2500, "The Dark {1} {0}" ),
					new KarmaEntry( -1250, "The Sinister {1} {0}" ),
					new KarmaEntry( -625, "The Dishonored {1} {0}" ),
					new KarmaEntry( 624, "{1} {0}" ),
					new KarmaEntry( 1249, "The Distinguished {1} {0}" ),
					new KarmaEntry( 2499, "The Eminent {1} {0}" ),
					new KarmaEntry( 4999, "The Noble {1} {0}" ),
					new KarmaEntry( 9999, "The Illustrious {1} {0}" ),
					new KarmaEntry( 10000, "The Glorious {1} {0}" )
				} )
			};

		public static VeteranTitle[] VeteranTitles { get; set; }

		public static void Initialize()
		{
			VeteranTitles = new VeteranTitle[9];

			for (var i = 0; i < 9; i++)
			{
				VeteranTitles[i] = new VeteranTitle(1154341 + i, 2 * (i + 1));
			}
		}

		public static List<VeteranTitle> GetVeteranTitles(Mobile m)
		{
			if (m.Account is not Account a)
				return null;

			int years = (int)(DateTime.UtcNow - a.Created).TotalDays;
			years /= 365;

			return years < 2 ? null : VeteranTitles.Where(title => years >= title.Years).ToList();
		}
	}

	public class FameEntry
	{
		public int Fame { get; set; }
		public KarmaEntry[] Karma { get; set; }

		public FameEntry(int fame, KarmaEntry[] karma)
		{
			Fame = fame;
			Karma = karma;
		}
	}

	public class KarmaEntry
	{
		public int Karma { get; set; }
		public string Title { get; set; }

		public KarmaEntry(int karma, string title)
		{
			Karma = karma;
			Title = title;
		}
	}

	public class VeteranTitle
	{
		public int Title { get; set; }
		public int Years { get; set; }

		public VeteranTitle(int title, int years)
		{
			Title = title;
			Years = years;
		}
	}
}
