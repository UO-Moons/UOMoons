using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Server.Mobiles;

public interface IStringList
{
	string GetString(BaseConvo npc, Mobile pc);
}

public class AttitudeList : IStringList
{
	private readonly Attitude[] _atts;
	private readonly IStringList[] _strings;

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
				// ignored
			}
		}

		if (file.Eof())
			return;

		file.Seek(-1);
		_strings = KeywordCollection.MakeList(file);
		if (att.Count > 0)
			_atts = (Attitude[])att.ToArray(typeof(Attitude));
		else
			_strings = null;
	}

	public string GetString(BaseConvo npc, Mobile pc)
	{
		Attitude test = npc.Attitude;
		while (true)
		{
			if (_atts.Any(t => t == test))
			{
				string str = null;
				for (var s = 0; s < _strings.Length && str == null; s++)
					str = _strings[s].GetString(npc, pc);
				return str;
			}

			if (test < Attitude.Neutral)
				test = (Attitude)((int)test + 1);
			else if (test > Attitude.Neutral)
				test = (Attitude)((int)test - 1);
			else
				break;
		}
		return null;
	}
}

public class NotorietyList : IStringList
{
	private readonly IStringList[] _strings;
	private readonly NotoVal[] _notos;

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
				// ignored
			}
		}

		if (file.Eof())
			return;

		file.Seek(-1);
		_strings = KeywordCollection.MakeList(file);
		if (noto.Count > 0)
			_notos = (NotoVal[])noto.ToArray(typeof(NotoVal));
		else
			_strings = null;
	}

	private static NotoVal GetNotoValFor(Mobile pc)
	{
		int val = (int)((pc.Karma + 128.0) / 52.0);
		return val switch
		{
			<= 0 => NotoVal.Infamous,
			>= 4 => NotoVal.Famous,
			_ => (NotoVal) val
		};
	}

	public string GetString(BaseConvo npc, Mobile pc)
	{
		NotoVal noto = GetNotoValFor(pc);
		if (!_notos.Any(t => t == noto || t == noto + 1 || t == noto - 1)) return null;
		string str = null;
		for (var s = 0; s < _strings.Length && str == null; s++)
			str = _strings[s].GetString(npc, pc);
		return str;
	}
}

public class PhraseList : IStringList
{
	private readonly List<object> _strings;//ArrayList
	public PhraseList()
	{
		_strings = new List<object>();
	}

	public string GetString(BaseConvo npc, Mobile pc)
	{
		if (_strings is {Count: > 0})
		{
			object obj = _strings[Utility.Random(_strings.Count)];
			return obj switch
			{
				string @string => @string,
				string[] v => v[pc.Female ? 1 : 0],
				_ => null
			};
		}

		return null;
	}

	private static object Parse(string str)
	{
		StringBuilder male = new(str.Length);
		StringBuilder female = null;
		const int both = 0, maleonly = 1, femaleonly = 2;
		int gender = 0; // 0=both, 1=male,2=female

		for (var i = 0; i < str.Length; i++)
		{
			char p = str[i];
			switch (p)
			{
				case '$': // $milord/milady$
					if (gender == femaleonly)
					{
						gender = both;
					}
					else
					{
						gender = maleonly;
						female ??= new StringBuilder(male.ToString());
					}
					break;

				case '/':// $milord/milady$
					if (gender == maleonly)
					{
						gender = femaleonly;
					}
					else
					{
						male.Append(p);
						female?.Append(p);
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
							female?.Append(@"{0}");
							break;
						case 'M':
						case 'm':
							male.Append(@"{1}");
							female?.Append(@"{1}");
							break;
						case 'J':
						case 'j':
							male.Append(@"{2}");
							female?.Append(@"{2}");
							break;
						case 'T':
						case 't':
							male.Append(@"{3}");
							female?.Append(@"{3}");
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
					female?.Append(@"{4}");
					break;

				default:
					if (gender != femaleonly)
						male.Append(p);
					if (female != null && gender != maleonly)
						female.Append(p);
					break;
			}
		}

		if (female != null)
			return new[] { male.ToString(), female.ToString() };
		return male.ToString();
	}

	public void Add(string str)
	{
		_strings.Add(Parse(str));
	}
}

public class Keyword
{
	private readonly Regex _mKeywords;
	public readonly IStringList[] Lists;

	public Keyword(string keyexp, IStringList[] lists)
	{
		Lists = lists;
		_mKeywords = new Regex(keyexp, RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);
	}

	public bool Match(string said)
	{
		return _mKeywords != null && _mKeywords.IsMatch(said);
	}
}

public class KeywordCollection
{
	private readonly List<object> _keys;//Arraylist

	public KeywordCollection(FileStrBuff file)
	{
		_keys = new List<object>();

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
							if (tok != "@InternalGreeting") continue;
							if (count > 0)
								keyExp.Append('|');
							count += 7;
							keyExp.Append("(*hi*)|(*hello*)|(*hail*)|(*greeting*)|(*how*((*you*)|(*thou*)))|(*good*see*thee*)");
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
							if (!string.IsNullOrEmpty(exp))
								_keys.Add(new Keyword(exp, lists));
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
			}
		}
	}

	public string GetString(BaseConvo npc, Mobile pc, string said)
	{
		for (int i = 0; i < _keys.Count; i++)
		{
			Keyword k = (Keyword)_keys[i];
			if (!k.Match(said)) continue;
			string str = null;
			for (int s = 0; s < k.Lists.Length && str == null; s++)
				str = k.Lists[s].GetString(npc, pc);
			return str;
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
					switch (lwr)
					{
						case "#attitude":
							list.Add(new AttitudeList(file));
							break;
						case "#notoriety":
							list.Add(new NotorietyList(file));
							break;
					}
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
						switch (tok)
						{
							case "{":
								brace++;
								break;
							case "}":
								brace--;
								break;
							default:
								pl.Add(tok);
								break;
						}
					}
					list.Add(pl);
					break;
				}
			}
		}//while

		if (list.Count > 0)
			return (IStringList[])list.ToArray(typeof(IStringList));
		return null;
	}
}

public class Fragment
{
	private readonly KeywordCollection[] _collections;

	public Fragment(FileStrBuff file)
	{
		_collections = new KeywordCollection[3];
		while (!file.Eof())
		{
			string tok = file.GetNextToken().ToLower();
			if (tok is not {Length: > 0})
				continue;

			switch (tok)
			{
				case "#sophistication":
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

					_collections[(int)s] = new KeywordCollection(file);
					break;
				}
				case "#fragment":
				{
					while (!file.Eof())
					{
						if (file.GetNextToken() == "{")
							break;
					}

					break;
				}
				default:
				{
					if (tok != "{" && tok != "}")
					{
						//Console.WriteLine( "Fragment : Unknown token '{0}'", tok );
					}

					break;
				}
			}
		}
	}

	public string GetString(BaseConvo npc, Mobile pc, string said)
	{
		int soph = (int)npc.Sophistication;
		string str = _collections[soph].GetString(npc, pc, said);
		if (str != null) return str;
		switch (soph)
		{
			case (int)Sophistication.High:
				soph--;
				break;
			case (int)Sophistication.Low:
				soph++;
				break;
			default:
				return null;
		}
		str = _collections[soph].GetString(npc, pc, said);
		return str;
	}
}

public class FileStrBuff
{
	private readonly string _mData;
	private int _mPos;

	public FileStrBuff(string fileName)
	{
		_mPos = 0;
		using StreamReader reader = new(fileName);
		_mData = reader.ReadToEnd();
	}

	public int SkipUntil(char stop)
	{
		int start = _mPos;
		while (_mPos < _mData.Length && _mData[_mPos] != stop)
			_mPos++;
		return _mPos - start;
	}

	private static bool IsSkipChar(char c)
	{
		// include commas (used to seperate lists) as "white space"
		return c is ',' or '\t' or ' ' or '\n' or '\r'; //Char.IsWhiteSpace( c );
	}

	public char Peek()
	{
		return !Eof() ? _mData[_mPos] : '\x0';
	}

	public void NextChar()
	{
		_mPos++;
	}

	public void Seek(int amount)
	{
		_mPos += amount;
	}

	public bool Eof()
	{
		return _mPos >= _mData.Length;
	}

	public string GetNextToken()
	{
		SkipWhitespace();
		if (_mPos >= _mData.Length)
			return string.Empty;
		StringBuilder token = new();
		if (_mData[_mPos] == '\"')
		{
			_mPos++; // skip the opening "
			while (_mPos < _mData.Length && _mData[_mPos] != '\"')
			{
				if (_mData[_mPos] != '\n')
					token.Append(_mData[_mPos]);
				_mPos++;
			}
			_mPos++; // skip the closing "
		}
		else
		{
			bool firstChar = true;
			while (_mPos < _mData.Length && !IsSkipChar(_mData[_mPos]))
			{
				if (_mData[_mPos] == '{' || _mData[_mPos] == '}')
				{
					if (firstChar)
					{
						token.Append(_mData[_mPos]);
						_mPos++;
					}
					break;
				}
				else
				{
					firstChar = false;
					token.Append(_mData[_mPos]);
					_mPos++;
				}
			}
		}
		return token.ToString();
	}

	private void SkipWhitespace()
	{
		if (_mPos >= _mData.Length)
			return;

		bool newLine = _mPos == 0 || _mData[_mPos] == '\n' || _mData[_mPos - 1] == '\n';

		while (_mPos < _mData.Length)
		{
			if (IsSkipChar(_mData[_mPos]))
			{
				if (_mData[_mPos] == '\n')
					newLine = true;
			}
			else if (newLine && _mPos + 1 < _mData.Length && _mData[_mPos] == '/' && _mData[_mPos + 1] == '/')
			{
				// its a comment, skip the whole line
				_mPos += 2;
				while (_mPos < _mData.Length && _mData[_mPos] != '\n')
					_mPos++;
				//newLine = true;
			}
			else if (_mPos + 1 < _mData.Length && _mData[_mPos] == '/' && _mData[_mPos + 1] == '*')
			{
				// its a block comment, skip until its closed
				_mPos += 2; // skip opener
				while (_mPos + 1 < _mData.Length && !(_mData[_mPos] == '*' && _mData[_mPos + 1] == '/'))
					_mPos++;
			}
			else
			{
				// found a non-whitespace, non comment. stop.
				break;
			}

			_mPos++;
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
	private static readonly Hashtable MFrg = new();

	private static Fragment LoadFrg(string name)
	{
		name = Path.Combine(Path.Combine(Core.BaseDirectory, "Data/convo"), name);
		return !File.Exists(name) ? null : new Fragment(new FileStrBuff(name));

		//Console.WriteLine( "Loading convo fragment: {0}", name );
	}

	public static void Configure()
	{
		Console.Write("Loading convo fragments... ");

		MFrg[DefaultFragment] = LoadFrg("bdefault.frg");
		MFrg[BritanniaFragment] = LoadFrg("britanni.frg");
		MFrg[GreetingFragment] = LoadFrg("greetings.frg");
		MFrg[EndFragment] = LoadFrg("convbye.frg");

		for (int i = ((int)RegionFragment._Offset) + 1; i < (int)RegionFragment._End; i++)
			MFrg[i] = LoadFrg($"{(RegionFragment)i}.frg");

		for (int i = ((int)JobFragment._Offset) + 1; i < (int)JobFragment._End; i++)
			MFrg[i] = LoadFrg($"{(JobFragment)i}.frg");

		Console.WriteLine("Done.");
	}

	private List<object> _mConvoList;//ArrayList

	[CommandProperty(AccessLevel.GameMaster)]
	public Sophistication Sophistication { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public Attitude Attitude { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public JobFragment Job { get; set; }

	private class ConvoTimer : Timer
	{
		private readonly BaseConvo _mOwner;
		public Mobile Mobile { get; }

		public ConvoTimer(BaseConvo owner, Mobile m) : base(TimeSpan.FromSeconds(30.0))
		{
			_mOwner = owner;
			Mobile = m;
			Priority = TimerPriority.OneSecond;
		}

		protected override void OnTick()
		{
			_mOwner._mConvoList?.Remove(this);
			if (_mOwner.FocusMob == Mobile)
				_mOwner.FocusMob = null;
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

		_mConvoList ??= new List<object>(1);
		ConvoTimer ct = new(this, m);
		_mConvoList.Add(ct);
		ct.Start();

		return ct;
	}

	private ConvoTimer GetConvo(IEntity m)
	{
		return _mConvoList?.Cast<ConvoTimer>().FirstOrDefault(ct => ct.Mobile == m);
	}

	public override void OnAfterDelete()
	{
		base.OnAfterDelete();

		if (_mConvoList == null) return;
		for (int i = 0; i < _mConvoList.Count; i++)
			((Timer)_mConvoList[i])?.Stop();
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

	private static readonly ArrayList MList = new(10);
	public override void OnSpeech(SpeechEventArgs e)
	{
		Mobile pc = e.Mobile;
		if (base.HandlesOnSpeech(pc))
			base.OnSpeech(e);

		if (!Enabled || e.Handled || e.Blocked || pc is not {Player: true, Alive: true} || (int) GetDistanceToSqrt(pc) >= 4 || !InLOS(pc) || pc == ControlMaster) return;
		string said = e.Speech;
		string str = null;

		if (said == null)
			return;

		ConvoTimer ct = GetConvo(pc);
		if (ct == null)
		{
			bool convo = false;

			Fragment f = (Fragment)MFrg[GreetingFragment];
			if (f != null)
			{
				str = f.GetString(this, pc, said);
				convo = str != null && str.Length > 0;
			}

			if (!convo && Name != null)
				convo = said.ToLower().IndexOf(Name.ToLower(), StringComparison.Ordinal) != -1;

			if (convo)
			{
				DebugSay("They are talking to me!");

				_ = StartConvo(pc);
				str ??= "Hail, traveler.";
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

			MList.Clear();
			if (Job != JobFragment.None)
				MList.Add((int)Job);

			GetConvoFragments(MList);

			if (MList.Count > 0)
			{
				for (int i = 0; i < MList.Count && str == null; i++)
				{
					Fragment f = (Fragment)MFrg[(int)MList[i]!];
					if (f != null)
						str = f.GetString(this, pc, said);
				}
			}

			if (str == null)
			{
				Fragment f = (Fragment)MFrg[EndFragment];
				if (f != null)
					str = f.GetString(this, pc, said);

				if (str == null)
				{
					f = (Fragment)MFrg[DefaultFragment];
					if (f != null && Utility.Random(4) == 0)
						str = f.GetString(this, pc, said);
				}
				else
				{
					DebugSay("They ended the conversation");
					if (FocusMob == pc)
						FocusMob = null;

					ct.Stop();

					_mConvoList?.Remove(ct);
				}
			}
			else if (AIObject != null)
			{
				ct.Refresh();

				if (AIObject.Action is ActionType.Wander or ActionType.Interact)
				{
					AIObject.Action = ActionType.Interact;
					FocusMob = pc;
				}
			}
		}

		if (!string.IsNullOrEmpty(str))
		{
			string town = Region.Name;// : "wilderness";
			if (town is not {Length: > 1})
				town = "great wide open";

			string job = Title != null && Title.StartsWith("the") ? Title[5..] : Job.ToString();

			Say(string.Format(str, pc.Name, Name, job, town, ""));
			e.Handled = true;
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
