using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Server.Mobiles
{
	public interface IStringList
	{
		string GetString(BaseConvo npc, Mobile pc);
	}

	public class AttitudeList : IStringList
	{
		private readonly Attitude[] m_Atts;
		private readonly IStringList[] m_Strings;

		public AttitudeList(FileStrBuff file)
		{
			string tok;
			ArrayList att = new(5);
			while (!file.Eof() && (tok = file.GetNextToken()) != "{")
			{
				try
				{
					att.Add(Enum.Parse(typeof(Attitude), tok, true));
				}
				catch
				{
				}
			}

			if (file.Eof())
				return;

			file.Seek(-1);
			m_Strings = KeywordCollection.MakeList(file);
			if (att.Count > 0)
				m_Atts = (Attitude[])att.ToArray(typeof(Attitude));
			else
				m_Strings = null;
		}

		public string GetString(BaseConvo npc, Mobile pc)
		{
			Attitude test = npc.Attitude;
			while (true)
			{
				for (int i = 0; i < m_Atts.Length; i++)
				{
					if (m_Atts[i] == test)
					{
						string str = null;
						for (int s = 0; s < m_Strings.Length && str == null; s++)
							str = m_Strings[s].GetString(npc, pc);
						return str;
					}
				}

				if (test < Attitude.Neutral)
					test = (Attitude)(((int)test) + 1);
				else if (test > Attitude.Neutral)
					test = (Attitude)(((int)test) - 1);
				else
					break;
			}
			return null;
		}
	}

	public class NotorietyList : IStringList
	{
		private readonly IStringList[] m_Strings;
		private readonly NotoVal[] m_Notos;

		public NotorietyList(FileStrBuff file)
		{
			string tok;
			ArrayList noto = new(5);
			while (!file.Eof() && (tok = file.GetNextToken()) != "{")
			{
				try
				{
					noto.Add(Enum.Parse(typeof(NotoVal), tok, true));
				}
				catch
				{
				}
			}

			if (file.Eof())
				return;

			file.Seek(-1);
			m_Strings = KeywordCollection.MakeList(file);
			if (noto.Count > 0)
				m_Notos = (NotoVal[])noto.ToArray(typeof(NotoVal));
			else
				m_Strings = null;
		}

		private static NotoVal GetNotoValFor(Mobile pc)
		{
			int val = (int)((pc.Karma + 128.0) / 52.0);
			if (val <= 0)
				return NotoVal.Infamous;
			else if (val >= 4)
				return NotoVal.Famous;
			else
				return (NotoVal)val;
		}

		public string GetString(BaseConvo npc, Mobile pc)
		{
			NotoVal noto = GetNotoValFor(pc);
			for (int i = 0; i < m_Notos.Length; i++)
			{
				if (m_Notos[i] == noto || m_Notos[i] == noto + 1 || m_Notos[i] == noto - 1)
				{
					string str = null;
					for (int s = 0; s < m_Strings.Length && str == null; s++)
						str = m_Strings[s].GetString(npc, pc);
					return str;
				}
			}
			return null;
		}
	}

	public class PhraseList : IStringList
	{
		private readonly ArrayList m_Strings;
		public PhraseList()
		{
			m_Strings = new ArrayList();
		}

		public string GetString(BaseConvo npc, Mobile pc)
		{
			if (m_Strings != null && m_Strings.Count > 0)
			{
				object obj = m_Strings[Utility.Random(m_Strings.Count)];
				if (obj is string @string)
					return @string;
				else if (obj is string[] v)
					return v[pc.Female ? 1 : 0];
				else
					return null;
			}
			else
			{
				return null;
			}
		}

		private static object Parse(string str)
		{
			StringBuilder male = new(str.Length);
			StringBuilder female = null;
			const int BOTH = 0, MALEONLY = 1, FEMALEONLY = 2;
			int gender = 0; // 0=both, 1=male,2=female

			for (int i = 0; i < str.Length; i++)
			{
				char p = str[i];
				switch (p)
				{
					case '$': // $milord/milady$
						if (gender == FEMALEONLY)
						{
							gender = BOTH;
						}
						else
						{
							gender = MALEONLY;
							if (female == null)
								female = new StringBuilder(male.ToString());
						}
						break;

					case '/':// $milord/milady$
						if (gender == MALEONLY)
						{
							gender = FEMALEONLY;
						}
						else
						{
							male.Append(p);
							if (female != null)
								female.Append(p);
						}
						break;

					case '_': // _Name_, _MyName_, _Town_, _Job_
						i++;
						if (i >= str.Length)
							break;
						p = str[i];

						switch (p)
						{
							case 'N':
							case 'n':
								male.Append(@"{0}");
								if (female != null)
									female.Append(@"{0}");
								break;
							case 'M':
							case 'm':
								male.Append(@"{1}");
								if (female != null)
									female.Append(@"{1}");
								break;
							case 'J':
							case 'j':
								male.Append(@"{2}");
								if (female != null)
									female.Append(@"{2}");
								break;
							case 'T':
							case 't':
								male.Append(@"{3}");
								if (female != null)
									female.Append(@"{3}");
								break;
						}

						while (i < str.Length && p != '_')
						{
							i++;
							p = str[i];
						}
						break;

					case '[': // [Attack] [Leave] etc
						while (i < str.Length && p != ']')
						{
							i++;
							p = str[i];
						}
						break;

					case '%':// %0
						i++;

						male.Append(@"{4}");
						if (female != null)
							female.Append(@"{4}");
						break;

					default:
						if (gender != FEMALEONLY)
							male.Append(p);
						if (female != null && gender != MALEONLY)
							female.Append(p);
						break;
				}
			}

			if (female != null)
				return new string[] { male.ToString(), female.ToString() };
			else
				return male.ToString();
		}

		public void Add(string str)
		{
			m_Strings.Add(Parse(str));
		}
	}

	public class Keyword
	{
		private readonly Regex m_Keywords;
		public readonly IStringList[] Lists;

		public Keyword(string keyexp, IStringList[] lists)
		{
			Lists = lists;
			m_Keywords = new Regex(keyexp, RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);
		}

		public bool Match(string said)
		{
			return m_Keywords != null && m_Keywords.IsMatch(said);
		}
	}

	public class KeywordCollection
	{
		private readonly ArrayList m_Keys;

		public KeywordCollection(FileStrBuff file)
		{
			m_Keys = new ArrayList();

			file.SkipUntil('{');
			file.NextChar();

			int brace = 1;
			while (!file.Eof() && brace > 0)
			{
				string tok = file.GetNextToken();
				switch (tok[0])
				{
					case '#':
						{
							if (tok.ToLower() != "#key")
							{
								//Console.WriteLine( "KeywordCollection : Unknown command '{0}'", tok );
								continue;
							}

							StringBuilder keyExp = new();
							int count = 0;
							while (!file.Eof() && (tok = file.GetNextToken()) != "{")
							{
								if (tok[0] == '@')
								{
									if (tok == "@InternalGreeting")
									{
										if (count > 0)
											keyExp.Append('|');
										count += 7;
										keyExp.Append("(*hi*)|(*hello*)|(*hail*)|(*greeting*)|(*how*((*you*)|(*thou*)))|(*good*see*thee*)");
									}

									continue;
								}
								else
								{
									count++;
									if (count > 1)
										keyExp.Append('|');
									keyExp.AppendFormat("({0})", tok);
								}
							}

							if (!file.Eof())
							{
								file.Seek(-1); // leave the { in the input

								IStringList[] lists = MakeList(file);
								//Console.WriteLine( "KC '{0}' loaded {1} sub-lists...", exp, lists.Length );
								if (lists != null && keyExp.Length > 0)
								{
									keyExp.Replace("*", ".*");
									string exp = keyExp.ToString();
									if (exp != null && exp.Length > 0)
										m_Keys.Add(new Keyword(exp, lists));
								}
							}
							break;
						}
					case '{':
						brace++;
						break;
					case '}':
						brace--;
						break;
					default:
						//Console.WriteLine( "KeywordCollection : Unknown token '{0}'", tok );
						break;
				}
			}
		}

		public string GetString(BaseConvo npc, Mobile pc, string said)
		{
			for (int i = 0; i < m_Keys.Count; i++)
			{
				Keyword k = (Keyword)m_Keys[i];
				if (k.Match(said))
				{
					string str = null;
					for (int s = 0; s < k.Lists.Length && str == null; s++)
						str = k.Lists[s].GetString(npc, pc);
					return str;
				}
			}

			return null;
		}

		public static IStringList[] MakeList(FileStrBuff file)
		{
			file.SkipUntil('{');
			file.NextChar();
			int brace = 1;

			ArrayList list = new();
			while (!file.Eof() && brace > 0)
			{
				string tok = file.GetNextToken();
				switch (tok[0])
				{
					case '{':
						brace++;
						break;
					case '}':
						brace--;
						break;
					case '#':
						{
							string lwr = tok.ToLower();
							if (lwr == "#attitude")
								list.Add(new AttitudeList(file));
							else if (lwr == "#notoriety")
								list.Add(new NotorietyList(file));
							//else
							//	Console.WriteLine( "MakeList : Unknown token '{0}'", lwr );
							break;
						}
					default:
						{
							PhraseList pl = new();
							pl.Add(tok);
							//if ( brace <= 0 )
							//	Console.WriteLine( "MakeList : Warning, no opening brace for PhraseList." );
							while (!file.Eof() && brace > 0)
							{
								tok = file.GetNextToken();
								if (tok == "{")
									brace++;
								else if (tok == "}")
									brace--;
								else
									pl.Add(tok);
							}
							list.Add(pl);
							break;
						}
				}
			}//while

			if (list.Count > 0)
				return (IStringList[])list.ToArray(typeof(IStringList));
			else
				return null;
		}
	}

	public class Fragment
	{
		private readonly KeywordCollection[] m_Collections;

		public Fragment(FileStrBuff file)
		{
			m_Collections = new KeywordCollection[3];
			while (!file.Eof())
			{
				string tok = file.GetNextToken().ToLower();
				if (tok == null || tok.Length <= 0)
					continue;

				if (tok == "#sophistication")
				{
					Sophistication s;
					string level = file.GetNextToken();
					try
					{
						s = (Sophistication)Enum.Parse(typeof(Sophistication), level, true);
					}
					catch
					{
						//Console.WriteLine( "Fragment : Error, invalid Sophistication {0}", level );
						continue;
					}

					m_Collections[(int)s] = new KeywordCollection(file);
				}
				else if (tok == "#fragment")
				{
					while (!file.Eof())
					{
						if (file.GetNextToken() == "{")
							break;
					}
				}
				else if (tok != "{" && tok != "}")
				{
					//Console.WriteLine( "Fragment : Unknown token '{0}'", tok );
				}
			}
		}

		public string GetString(BaseConvo npc, Mobile pc, string said)
		{
			int soph = (int)npc.Sophistication;
			string str = m_Collections[soph].GetString(npc, pc, said);
			if (str == null)
			{
				if (soph == (int)Sophistication.High)
					soph--;
				else if (soph == (int)Sophistication.Low)
					soph++;
				else
					return null;
				str = m_Collections[soph].GetString(npc, pc, said);
			}
			return str;
		}
	}

	public class FileStrBuff
	{
		private readonly string m_Data;
		private int m_Pos;

		public FileStrBuff(string fileName)
		{
			m_Pos = 0;
			using StreamReader reader = new(fileName);
			m_Data = reader.ReadToEnd();
		}

		public int SkipUntil(char stop)
		{
			int start = m_Pos;
			while (m_Pos < m_Data.Length && m_Data[m_Pos] != stop)
				m_Pos++;
			return m_Pos - start;
		}

		private static bool IsSkipChar(char c)
		{
			// include commas (used to seperate lists) as "whipespace"
			return c == ',' || c == '\t' || c == ' ' || c == '\n' || c == '\r'; //Char.IsWhiteSpace( c );
		}

		public char Peek()
		{
			if (!Eof())
				return m_Data[m_Pos];
			else
				return '\x0';
		}

		public void NextChar()
		{
			m_Pos++;
		}

		public void Seek(int amount)
		{
			m_Pos += amount;
		}

		public bool Eof()
		{
			return m_Pos >= m_Data.Length;
		}

		public string GetNextToken()
		{
			SkipWhitespace();
			if (m_Pos >= m_Data.Length)
				return string.Empty;
			StringBuilder token = new();
			if (m_Data[m_Pos] == '\"')
			{
				m_Pos++; // skip the opening "
				while (m_Pos < m_Data.Length && m_Data[m_Pos] != '\"')
				{
					if (m_Data[m_Pos] != '\n')
						token.Append(m_Data[m_Pos]);
					m_Pos++;
				}
				m_Pos++; // skip the closing "
			}
			else
			{
				bool firstChar = true;
				while (m_Pos < m_Data.Length && !IsSkipChar(m_Data[m_Pos]))
				{
					if (m_Data[m_Pos] == '{' || m_Data[m_Pos] == '}')
					{
						if (firstChar)
						{
							token.Append(m_Data[m_Pos]);
							m_Pos++;
						}
						break;
					}
					else
					{
						firstChar = false;
						token.Append(m_Data[m_Pos]);
						m_Pos++;
					}
				}
			}
			return token.ToString();
		}

		private void SkipWhitespace()
		{
			if (m_Pos >= m_Data.Length)
				return;

			bool newLine = m_Pos == 0 || m_Data[m_Pos] == '\n' || m_Data[m_Pos - 1] == '\n';

			while (m_Pos < m_Data.Length)
			{
				if (IsSkipChar(m_Data[m_Pos]))
				{
					if (m_Data[m_Pos] == '\n')
						newLine = true;
				}
				else if (newLine && m_Pos + 1 < m_Data.Length && m_Data[m_Pos] == '/' && m_Data[m_Pos + 1] == '/')
				{
					// its a comment, skip the whole line
					m_Pos += 2;
					while (m_Pos < m_Data.Length && m_Data[m_Pos] != '\n')
						m_Pos++;
					newLine = true;
				}
				else if (m_Pos + 1 < m_Data.Length && m_Data[m_Pos] == '/' && m_Data[m_Pos + 1] == '*')
				{
					// its a block comment, skip until its closed
					m_Pos += 2; // skip opener
					while (m_Pos + 1 < m_Data.Length && !(m_Data[m_Pos] == '*' && m_Data[m_Pos + 1] == '/'))
						m_Pos++;
				}
				else
				{
					// found a non-whitespace, non comment. stop.
					break;
				}

				m_Pos++;
			}
		}
	}

	public class BaseConvo : BaseCreature
	{
		private const int EndFragment = 3;
		private const int GreetingFragment = 2;
		private const int BritanniaFragment = 1;
		private const int DefaultFragment = 0;
		public static bool Enabled => Settings.Configuration.Get<bool>("Mobiles", "NpcSpeech");
		private static readonly Hashtable m_Frg = new();

		private static Fragment LoadFrg(string name)
		{
			name = Path.Combine(Path.Combine(Core.BaseDirectory, "Data/convo"), name);
			if (!File.Exists(name))
				return null;

			//Console.WriteLine( "Loading convo fragment: {0}", name );
			return new Fragment(new FileStrBuff(name));
		}

		public static void Configure()
		{
			Console.Write("Loading convo fragments... ");

			m_Frg[DefaultFragment] = LoadFrg("bdefault.frg");
			m_Frg[BritanniaFragment] = LoadFrg("britanni.frg");
			m_Frg[GreetingFragment] = LoadFrg("greetings.frg");
			m_Frg[EndFragment] = LoadFrg("convbye.frg");

			for (int i = ((int)RegionFragment._Offset) + 1; i < (int)RegionFragment._End; i++)
				m_Frg[i] = LoadFrg($"{(RegionFragment)i}.frg");

			for (int i = ((int)JobFragment._Offset) + 1; i < (int)JobFragment._End; i++)
				m_Frg[i] = LoadFrg($"{(JobFragment)i}.frg");

			Console.WriteLine("Done.");
		}

		private ArrayList m_ConvoList;

		[CommandProperty(AccessLevel.GameMaster)]
		public Sophistication Sophistication { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public Attitude Attitude { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public JobFragment Job { get; set; }

		private class ConvoTimer : Timer
		{
			private readonly BaseConvo m_Owner;
			public Mobile Mobile { get; }

			public ConvoTimer(BaseConvo owner, Mobile m) : base(TimeSpan.FromSeconds(30.0))
			{
				m_Owner = owner;
				Mobile = m;
				Priority = TimerPriority.OneSecond;
			}

			protected override void OnTick()
			{
				if (m_Owner.m_ConvoList != null)
					m_Owner.m_ConvoList.Remove(this);
				if (m_Owner.FocusMob == Mobile)
					m_Owner.FocusMob = null;
			}

			public void Refresh()
			{
				Stop();
				Start();
			}
		}

		private ConvoTimer StartConvo(Mobile m)
		{
			/* --from convinit.frg, seems unneeded w/proper use of greetings.frg
			Fragment f = (Fragment)m_Frg[InitFragment];
			if ( f != null )
			{
				string str = f.GetString( this, pc, "@InternalConvinit" );
				if ( str != null )
					Say( String.Format( str, m.Name, this.Name, "", "", "" ) );
			}
			*/

			if (m_ConvoList == null)
				m_ConvoList = new ArrayList(1);
			ConvoTimer ct = new(this, m);
			m_ConvoList.Add(ct);
			ct.Start();

			return ct;
		}

		private ConvoTimer GetConvo(Mobile m)
		{
			if (m_ConvoList != null)
			{
				for (int i = 0; i < m_ConvoList.Count; i++)
				{
					ConvoTimer ct = (ConvoTimer)m_ConvoList[i];
					if (ct.Mobile == m)
						return ct;
				}
			}
			return null;
		}

		public override void OnAfterDelete()
		{
			base.OnAfterDelete();

			if (m_ConvoList != null)
			{
				for (int i = 0; i < m_ConvoList.Count; i++)
					((Timer)m_ConvoList[i]).Stop();
			}
		}

		public BaseConvo(AIType ai, FightMode mode, int iRangePerception, int iRangeFight, double dActiveSpeed, double dPassiveSpeed)
			: base(ai, mode, iRangePerception, iRangeFight, dActiveSpeed, dPassiveSpeed)
		{
			Job = JobFragment.None;
			if (AlwaysMurderer)
				Attitude = (Attitude)Utility.Random(3); // make them never good tempered
			else
				Attitude = (Attitude)Utility.Random(5);
			Sophistication = (Sophistication)Utility.Random(3);
		}

		public BaseConvo(Serial serial) : base(serial)
		{
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
			switch (version)
			{
				case 0:
					{
						Job = (JobFragment)reader.ReadShort();
						Attitude = (Attitude)reader.ReadByte();
						Sophistication = (Sophistication)reader.ReadByte();
						break;
					}
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
			writer.Write((short)Job);
			writer.Write((byte)Attitude);
			writer.Write((byte)Sophistication);
		}

		protected virtual void GetConvoFragments(ArrayList list)
		{
			if (Region is Regions.GuardedRegion gr)
			{
				if (gr.Fragment != RegionFragment.Wilderness)
					list.Add((int)gr.Fragment);
			}
			list.Add(BritanniaFragment);
		}

		public override bool HandlesOnSpeech(Mobile from)
		{
			return base.HandlesOnSpeech(from) || (Enabled && from != null && from.Player && from.Alive && (int)GetDistanceToSqrt(from) < 4 && from != ControlMaster);
		}

		public virtual bool OnConvoStart(Mobile m)
		{
			return true;
		}

		private static readonly ArrayList m_List = new(10);
		public override void OnSpeech(SpeechEventArgs e)
		{
			Mobile pc = e.Mobile;
			if (base.HandlesOnSpeech(pc))
				base.OnSpeech(e);

			if (Enabled && !e.Handled && !e.Blocked && pc != null && pc.Player && pc.Alive && (int)GetDistanceToSqrt(pc) < 4 && InLOS(pc) && pc != ControlMaster)
			{
				string said = e.Speech;
				string str = null;

				if (said == null)
					return;

				ConvoTimer ct = GetConvo(pc);
				if (ct == null)
				{
					bool convo = false;

					Fragment f = (Fragment)m_Frg[GreetingFragment];
					if (f != null)
					{
						str = f.GetString(this, pc, said);
						convo = str != null && str.Length > 0;
					}

					if (!convo && Name != null)
						convo = said.ToLower().IndexOf(Name.ToLower()) != -1;

					if (convo)
					{
						DebugSay("They are talking to me!");

						_ = StartConvo(pc);
						if (str == null)
							str = "Hail, traveler.";
						if (!OnConvoStart(pc))
							str = null;
					}
					else
					{
						DebugSay("I dont think they are talking to me.");
					}
				}
				else
				{
					DebugSay("I'm conversing with them!");

					m_List.Clear();
					if (Job != JobFragment.None)
						m_List.Add((int)Job);

					GetConvoFragments(m_List);

					if (m_List.Count > 0)
					{
						for (int i = 0; i < m_List.Count && str == null; i++)
						{
							Fragment f = (Fragment)m_Frg[(int)m_List[i]];
							if (f != null)
								str = f.GetString(this, pc, said);
						}
					}

					if (str == null)
					{
						Fragment f = (Fragment)m_Frg[EndFragment];
						if (f != null)
							str = f.GetString(this, pc, said);

						if (str == null)
						{
							f = (Fragment)m_Frg[DefaultFragment];
							if (f != null && Utility.Random(4) == 0)
								str = f.GetString(this, pc, said);
						}
						else
						{
							DebugSay("They ended the conversation");
							if (FocusMob == pc)
								FocusMob = null;

							ct.Stop();

							if (m_ConvoList != null)
								m_ConvoList.Remove(ct);
						}
					}
					else if (AIObject != null)
					{
						ct.Refresh();

						if (AIObject.Action == ActionType.Wander || AIObject.Action == ActionType.Interact)
						{
							AIObject.Action = ActionType.Interact;
							FocusMob = pc;
						}
					}
				}

				if (str != null && str.Length > 0)
				{
					string town = Region.Name;// : "wilderness";
					if (town == null || town.Length <= 1)
						town = "great wide open";

					string job = Title != null && Title.StartsWith("the") ? Title[5..] : Job.ToString();

					Say(string.Format(str, pc != null ? pc.Name : "someone", Name, job, town, ""));
					e.Handled = true;
				}
			}
		}
	}

	public enum RegionFragment
	{
		Wilderness = 0,
		_Offset = 100,

		britain,
		magincia,
		bucden,
		cove,
		jhelom,
		moonglow,
		nujelm,
		serphold,
		skara,
		trinsic,
		vesper,
		minoc,
		wind,
		yew,

		_End,
	}

	public enum JobFragment
	{
		None = 0,

		_Offset = 200,

		// real jobs:
		horse,
		shopkeep,
		beggar,
		scholar,
		monk,
		sculptor,
		servant,
		shipwright,
		tailor,
		tinker,
		weaver,
		artist,
		architect,
		realtor,
		alchemist,
		baker,
		beekeeper,
		brigand,
		carpenter,
		cashual,
		cobbler,
		furtrader,
		gambler,
		glassblower,
		gypsy,
		herbalist,
		jailor,
		jeweler,
		master,
		mayor,
		judge,
		miner,
		rancher,
		animal,
		blacksmith,
		fighter,
		fisher,
		noble,
		bowyer,
		paladin,
		prisoner,
		sailor,
		pirate,
		waiter,
		weaponsmith,
		actor,
		armourer,
		healer,
		mage,
		bard,
		farmer,
		cook,
		guard,
		laborer,
		thief,
		innkeeper,
		tavkeep,
		minter,
		miller,
		vet,
		weaponstrainer,
		runner,
		priest,
		shepherd,
		ranger,
		tanner,
		mapmaker,
		banker,

		_End,
	}

	public enum Attitude { Wicked = 0, Belligerent = 1, Neutral = 2, Kindly = 3, Goodhearted = 4 }
	public enum NotoVal { Infamous = 0, Outlaw = 1, Anonymous = 2, Known = 3, Famous = 4 }
	public enum Sophistication { Low = 0, Medium = 1, High = 2 }
}

