using Server.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server;

public delegate void Slice();

public static class Core
{
	private static bool _crashed;
	private static Thread _timerThread;
	private static string _baseDirectory;
	private static string _exePath;
	private static bool _cache = true;
	private static bool _profiling;
	private static DateTime _profileStart;
	private static TimeSpan _profileTime;

	public static MessagePump MessagePump { get; set; }

	public static Slice Slice;

	public static bool Profiling
	{
		get => _profiling;
		set
		{
			if (_profiling == value)
				return;

			_profiling = value;

			if (_profileStart > DateTime.MinValue)
				_profileTime += DateTime.UtcNow - _profileStart;

			_profileStart = _profiling ? DateTime.UtcNow : DateTime.MinValue;
		}
	}

	public static TimeSpan ProfileTime
	{
		get
		{
			if (_profileStart > DateTime.MinValue)
				return _profileTime + (DateTime.UtcNow - _profileStart);

			return _profileTime;
		}
	}

	public static bool Service { get; private set; }
	public static bool Debug { get; private set; }
	public static bool FileLoad = false;
	public static bool DebugLoad = false;
	internal static bool HaltOnWarning { get; private set; }
	internal static bool VBdotNet { get; private set; }
	//public static List<string> DataDirectories { get; } = new List<string>();
	public static HashSet<string> DataDirectories { get; } = new();
	public static Assembly Assembly { get; set; }
	public static Version Version => Assembly.GetName().Version;
	public static Process Process { get; private set; }
	public static Thread Thread { get; private set; }
	public static MultiTextWriter MultiConsoleOut { get; private set; }

	/*
	 * DateTime.Now and DateTime.UtcNow are based on actual system clock time.
	 * The resolution is acceptable but large clock jumps are possible and cause issues.
	 * GetTickCount and GetTickCount64 have poor resolution.
	 * GetTickCount64 is unavailable on Windows XP and Windows Server 2003.
	 * Stopwatch.GetTimestamp() (QueryPerformanceCounter) is high resolution, but
	 * somewhat expensive to call because of its defference to DateTime.Now,
	 * which is why Stopwatch has been used to verify HRT before calling GetTimestamp(),
	 * enabling the usage of DateTime.UtcNow instead.
	 */

	private static readonly bool m_HighRes = Stopwatch.IsHighResolution;

	private static readonly double m_HighFrequency = 1000.0 / Stopwatch.Frequency;
	private const double LowFrequency = 1000.0 / TimeSpan.TicksPerSecond;

	private static bool _useHrt;

	public static bool UsingHighResolutionTiming => _useHrt && m_HighRes && !Unix;

	public static long TickCount => (long)Ticks;

	public static double Ticks
	{
		get
		{
			if (_useHrt && m_HighRes && !Unix)
			{
				return Stopwatch.GetTimestamp() * m_HighFrequency;
			}

			return DateTime.UtcNow.Ticks * LowFrequency;
		}
	}

	public static readonly bool Is64Bit = Environment.Is64BitProcess;

	public static bool MultiProcessor { get; private set; }
	public static int ProcessorCount { get; private set; }

	public static bool Unix { get; private set; }

	public static string FindDataFile(string path)
	{
		if (DataDirectories.Count == 0)
			throw new InvalidOperationException("Attempted to FindDataFile before DataDirectories list has been filled.");

		string fullPath = null;

		foreach (string p in DataDirectories)
		{
			fullPath = Path.Combine(p, path);

			if (File.Exists(fullPath))
				break;

			fullPath = null;
		}

		return fullPath;
	}

	public static string FindDataFile(string format, params object[] args)
	{
		return FindDataFile(string.Format(format, args));
	}

	#region Expansions and Publishes

	public static Expansion Expansion { get; set; }
	public static Publishes Publishes { get; set; }

	public static bool T2A => Expansion >= Expansion.T2A;

	public static bool UOR => Expansion >= Expansion.UOR;

	public static bool UOTD => Expansion >= Expansion.UOTD;

	public static bool LBR => Expansion >= Expansion.LBR;

	public static bool AOS => Expansion >= Expansion.AOS;

	public static bool SE => Expansion >= Expansion.SE;

	public static bool ML => Expansion >= Expansion.ML;

	public static bool SA => Expansion >= Expansion.SA;

	public static bool HS => Expansion >= Expansion.HS;

	public static bool TOL => Expansion >= Expansion.TOL;

	public static bool EJ => Expansion >= Expansion.EJ;

	public static bool I => Publishes >= Publishes.I;
	public static bool II => Publishes >= Publishes.II;
	public static bool III => Publishes >= Publishes.III;
	public static bool IV => Publishes >= Publishes.IV;
	public static bool V => Publishes >= Publishes.V;
	public static bool VI => Publishes >= Publishes.VI;
	public static bool VII => Publishes >= Publishes.VII;
	public static bool VIII => Publishes >= Publishes.VIII;
	public static bool IX => Publishes >= Publishes.IX;
	public static bool X => Publishes >= Publishes.X;
	public static bool XI => Publishes >= Publishes.XI;
	public static bool XII => Publishes >= Publishes.XII;
	public static bool XIII => Publishes >= Publishes.XIII;
	public static bool XIV => Publishes >= Publishes.XIV;
	public static bool XV => Publishes >= Publishes.XV;
	public static bool XVI => Publishes >= Publishes.XVI;
	public static bool XVII => Publishes >= Publishes.XVII;
	public static bool XVIII => Publishes >= Publishes.XVIII;
	public static bool XIX => Publishes >= Publishes.XIX;
	public static bool XX => Publishes >= Publishes.XX;
	public static bool XXI => Publishes >= Publishes.XXI;
	public static bool XXII => Publishes >= Publishes.XXII;
	public static bool XXIII => Publishes >= Publishes.XXIII;
	public static bool XXIV => Publishes >= Publishes.XXIV;
	public static bool XXV => Publishes >= Publishes.XXV;
	public static bool XXVI => Publishes >= Publishes.XXVI;
	public static bool XXVII => Publishes >= Publishes.XXVII;
	public static bool XXVIII => Publishes >= Publishes.XXVIII;
	public static bool XXIX => Publishes >= Publishes.XXIX;
	public static bool XXX => Publishes >= Publishes.XXX;
	public static bool XXXI => Publishes >= Publishes.XXXI;
	public static bool XXXII => Publishes >= Publishes.XXXII;
	public static bool XXXIII => Publishes >= Publishes.XXXIII;
	public static bool XXXIV => Publishes >= Publishes.XXXIV;
	public static bool XXXV => Publishes >= Publishes.XXXV;
	public static bool XXXVI => Publishes >= Publishes.XXXVI;
	public static bool XXXVII => Publishes >= Publishes.XXXVII;
	public static bool XXXVIII => Publishes >= Publishes.XXXVIII;
	public static bool XXXIX => Publishes >= Publishes.XXXIX;
	public static bool XL => Publishes >= Publishes.XL;
	public static event Action OnExpansionChanged;
	#endregion

	public static string ExePath => _exePath ??= Assembly.Location;

	public static string BaseDirectory
	{
		get
		{
			if (_baseDirectory == null)
			{
				try
				{
					_baseDirectory = ExePath;

					if (_baseDirectory.Length > 0)
						_baseDirectory = Path.GetDirectoryName(_baseDirectory);
				}
				catch
				{
					_baseDirectory = "";
				}
			}

			return _baseDirectory;
		}
	}

	private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
	{
		Console.WriteLine(e.IsTerminating ? "Error:" : "Warning:");
		Console.WriteLine(e.ExceptionObject);

		if (e.IsTerminating)
		{
			_crashed = true;

			bool close = false;

			try
			{
				CrashedEventArgs args = new(e.ExceptionObject as Exception);

				EventSink.InvokeCrashed(args);

				close = args.Close;
			}
			catch
			{
				// ignored
			}

			if (!close && !Service)
			{
				if (MessagePump != null)
				{
					try
					{
						foreach (Listener l in MessagePump.Listeners)
						{
							l.Dispose();
						}
					}
					catch
					{
						// ignored
					}
				}

				Console.WriteLine("This exception is fatal, press return to exit");
				Console.ReadLine();
			}

			Kill();
		}
	}

	internal enum ConsoleEventType
	{
		CtrlCEvent,
		CtrlBreakEvent,
		CtrlCloseEvent,
		CtrlLogoffEvent = 5,
		CtrlShutdownEvent
	}

	internal delegate bool ConsoleEventHandler(ConsoleEventType type);
	internal static ConsoleEventHandler m_ConsoleEventHandler;

	internal class UnsafeNativeMethods
	{
		[DllImport("Kernel32")]
		internal static extern bool SetConsoleCtrlHandler(ConsoleEventHandler callback, bool add);
	}

	private static bool OnConsoleEvent(ConsoleEventType type)
	{
		if (World.Saving || (Service && type == ConsoleEventType.CtrlLogoffEvent))
			return true;

		Kill(); //Kill -> HandleClosed will handle waiting for the completion of flushing to disk

		return true;
	}

	private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
	{
		HandleClosed();
	}

	public static bool Closing { get; private set; }

	private static int _cycleIndex = 1;
	private static readonly float[] m_CyclesPerSecond = new float[100];

	public static float CyclesPerSecond => m_CyclesPerSecond[(_cycleIndex - 1) % m_CyclesPerSecond.Length];

	public static float AverageCps => m_CyclesPerSecond.Take(_cycleIndex).Average();

	public static void Kill()
	{
		Kill(false);
	}

	public static void Kill(bool restart)
	{
		HandleClosed();

		if (restart)
			Process.Start(ExePath, Arguments);

		Process.Kill();
	}

	private static void HandleClosed()
	{
		if (Closing)
			return;

		Closing = true;

		Console.Write("Exiting...");

		World.WaitForWriteCompletion();

		if (!_crashed)
			EventSink.InvokeShutdown();

		Timer.TimerThread.Set();

		Console.WriteLine("done");
	}

	private static readonly AutoResetEvent m_Signal = new(true);

	public static void Set() { m_Signal.Set(); }

	public static void Run(string[] args)
	{
		AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
		AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

		foreach (string a in args)
		{
			if (Insensitive.Equals(a, "-debug"))
				Debug = true;
			else if (Insensitive.Equals(a, "-service"))
				Service = true;
			else if (Insensitive.Equals(a, "-profile"))
				Profiling = true;
			else if (Insensitive.Equals(a, "-nocache"))
				_cache = false;
			else if (Insensitive.Equals(a, "-haltonwarning"))
				HaltOnWarning = true;
			else if (Insensitive.Equals(a, "-vb"))
				VBdotNet = true;
			else if (Insensitive.Equals(a, "-usehrt"))
				_useHrt = true;
		}

		try
		{
			if (Service)
			{
				if (!Directory.Exists("Logs"))
					Directory.CreateDirectory("Logs");

				Console.SetOut(MultiConsoleOut = new MultiTextWriter(new FileLogger("Logs/Console.log")));
			}
			else
			{
				Console.SetOut(MultiConsoleOut = new MultiTextWriter(Console.Out));
			}
		}
		catch
		{
		}

		Thread = Thread.CurrentThread;
		Process = Process.GetCurrentProcess();
		Assembly = Assembly.GetEntryAssembly();

		if (Thread != null)
			Thread.Name = "Core Thread";

		if (BaseDirectory.Length > 0)
			Directory.SetCurrentDirectory(BaseDirectory);

		Timer.TimerThread ttObj = new();
		_timerThread = new Thread(Timer.TimerThread.TimerMain)
		{
			Name = "Timer Thread"
		};

		Version ver = Assembly.GetName().Version;

		// Added to help future code support on forums, as a 'check' people can ask for to it see if they recompiled core or not
		Utility.WriteConsole(ConsoleColor.Cyan, "UOMoons - [https://github.com/UO-Moons/UOMoons] Version {0}.{1}.{2}.{3}", ver.Major, ver.Minor, ver.Build, ver.Revision);
		Utility.WriteConsole(ConsoleColor.Cyan, "Core: Running on .NET Core {0}.{1}.{2}", Environment.Version.Major, Environment.Version.Minor, Environment.Version.Build);

		string s = Arguments;

		if (s.Length > 0)
			Console.WriteLine("Core: Running with arguments: {0}", s);

		ProcessorCount = Environment.ProcessorCount;

		if (ProcessorCount > 1)
			MultiProcessor = true;

		if (MultiProcessor || Is64Bit)
			Console.WriteLine("Core: Optimizing for {0} {2}processor{1}", ProcessorCount, ProcessorCount == 1 ? "" : "s", Is64Bit ? "64-bit " : "");

		int platform = (int)Environment.OSVersion.Platform;
		if (platform is 4 or 128)
		{ // MS 4, MONO 128
			Unix = true;
			Console.WriteLine("Core: Unix environment detected");
		}
		else
		{
			m_ConsoleEventHandler = OnConsoleEvent;
			UnsafeNativeMethods.SetConsoleCtrlHandler(m_ConsoleEventHandler, true);
		}

		if (GCSettings.IsServerGC)
			Console.WriteLine("Core: Server garbage collection mode enabled");

		if (_useHrt)
			Console.WriteLine("Core: Requested high resolution timing ({0})", UsingHighResolutionTiming ? "Supported" : "Unsupported");

		Utility.WriteConsole(ConsoleColor.Green, "RandomImpl: {0} ({1})", RandomImpl.Type.Name, RandomImpl.IsHardwareRng ? "Hardware" : "Software");

		while (!Assembler.Load())
		{
			Utility.WriteConsole(ConsoleColor.Red, "Scripts: One or more scripts failed to compile or no script files were found.");

			if (Service)
				return;

			Console.WriteLine(" - Press return to exit, or R to try again.");

			if (Console.ReadKey(true).Key != ConsoleKey.R)
				return;
		}

		Assembler.Invoke("Configure");

		Region.Load();
		World.Load();

		Assembler.Invoke("Initialize");

		MessagePump messagePump = MessagePump = new MessagePump();

		_timerThread.Start();

		foreach (Map m in Map.AllMaps)
			TileMatrix.Force();

		NetState.Initialize();

		EventSink.InvokeServerStarted();

		try
		{
			long last = TickCount;

			const int sampleInterval = 100;
			const float ticksPerSecond = 1000.0f * sampleInterval;

			long sample = 0;

			while (!Closing)
			{
				m_Signal.WaitOne();

				Mobile.ProcessDeltaQueue();
				Item.ProcessDeltaQueue();

				Timer.Slice();
				messagePump.Slice();

				NetState.FlushAll();
				NetState.ProcessDisposedQueue();

				Slice?.Invoke();

				if (sample++ % sampleInterval != 0)
				{
					continue;
				}

				var now = TickCount;
				m_CyclesPerSecond[_cycleIndex++ % m_CyclesPerSecond.Length] = ticksPerSecond / (now - last);
				last = now;
			}
		}
		catch (Exception e)
		{
			CurrentDomain_UnhandledException(null, new UnhandledExceptionEventArgs(e, true));
		}
	}

	public static string Arguments
	{
		get
		{
			StringBuilder sb = new();

			if (Debug)
				Utility.Separate(sb, "-debug", " ");

			if (Service)
				Utility.Separate(sb, "-service", " ");

			if (_profiling)
				Utility.Separate(sb, "-profile", " ");

			if (!_cache)
				Utility.Separate(sb, "-nocache", " ");

			if (HaltOnWarning)
				Utility.Separate(sb, "-haltonwarning", " ");

			if (VBdotNet)
				Utility.Separate(sb, "-vb", " ");

			if (_useHrt)
				Utility.Separate(sb, "-usehrt", " ");

			return sb.ToString();
		}
	}

	private static int _itemCount, _mobileCount;

	public static int ScriptItems => _itemCount;
	public static int ScriptMobiles => _mobileCount;

	public static void VerifySerialization()
	{
		_itemCount = 0;
		_mobileCount = 0;

		Assembly ca = Assembly.GetCallingAssembly();

		VerifySerialization(ca);

		foreach (Assembly a in Assembler.Assemblies.Where(a => a != ca))
		{
			VerifySerialization(a);
		}
	}

	private static readonly Type[] m_SerialTypeArray = { typeof(Serial) };

	private static void VerifyType(Type t)
	{
		bool isItem = t.IsSubclassOf(typeof(Item));

		switch (isItem)
		{
			case false when !t.IsSubclassOf(typeof(Mobile)):
				return;
			case true:
				//++_ItemCount;
				Interlocked.Increment(ref _itemCount);
				break;
			default:
				//++_MobileCount;
				Interlocked.Increment(ref _mobileCount);
				break;
		}

		StringBuilder warningSb = null;

		try
		{
			if (t.GetConstructor(m_SerialTypeArray) == null)
			{
				warningSb = new StringBuilder();

				warningSb.AppendLine("       - No serialization constructor");
			}

			UnserializableAttribute attributes = t.GetCustomAttribute<UnserializableAttribute>();
			if (attributes == null)
			{
				if (
					t.GetMethod(
						"Serialize",
						BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly) == null)
				{
					warningSb ??= new StringBuilder();

					warningSb.AppendLine("       - No Serialize() method");
				}

				if (
					t.GetMethod(
						"Deserialize",
						BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly) == null)
				{
					warningSb ??= new StringBuilder();

					warningSb.AppendLine("       - No Deserialize() method");
				}
			}

			if (warningSb is {Length: > 0})
			{
				Utility.WriteConsole(ConsoleColor.Yellow, "Warning: {0}\n{1}", t, warningSb);
			}
		}
		catch
		{
			Utility.WriteConsole(ConsoleColor.Yellow, "Warning: Exception in serialization verification of type {0}", t);
		}
	}

	private static void VerifySerialization(Assembly a)
	{
		if (a != null)
		{
			Parallel.ForEach(a.GetTypes(), VerifyType);
		}
	}
}

public class FileLogger : TextWriter
{
	public const string DateFormat = "[MMMM dd hh:mm:ss.f tt]: ";

	private bool _newLine;

	public string FileName { get; }

	public FileLogger(string file)
		: this(file, false)
	{ }

	public FileLogger(string file, bool append)
	{
		FileName = file;

		using (StreamWriter writer = new(new FileStream(FileName, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.Read)))
		{
			writer.WriteLine(">>>Logging started on {0:f}.", DateTime.UtcNow);
			//f = Tuesday, April 10, 2001 3:51 PM
		}

		_newLine = true;
	}

	public override void Write(char ch)
	{
		using StreamWriter writer = new(new FileStream(FileName, FileMode.Append, FileAccess.Write, FileShare.Read));
		if (_newLine)
		{
			writer.Write(DateTime.UtcNow.ToString(DateFormat));
			_newLine = false;
		}

		writer.Write(ch);
	}

	public override void Write(string str)
	{
		using StreamWriter writer = new(new FileStream(FileName, FileMode.Append, FileAccess.Write, FileShare.Read));
		if (_newLine)
		{
			writer.Write(DateTime.UtcNow.ToString(DateFormat));
			_newLine = false;
		}

		writer.Write(str);
	}

	public override void WriteLine(string line)
	{
		using StreamWriter writer = new(new FileStream(FileName, FileMode.Append, FileAccess.Write, FileShare.Read));
		if (_newLine)
		{
			writer.Write(DateTime.UtcNow.ToString(DateFormat));
		}

		writer.WriteLine(line);
		_newLine = true;
	}

	public override Encoding Encoding => Encoding.Default;
}

public class MultiTextWriter : TextWriter
{
	private readonly List<TextWriter> _streams;

	public MultiTextWriter(params TextWriter[] streams)
	{
		_streams = new List<TextWriter>(streams);

		if (_streams.Count < 0)
		{
			throw new ArgumentException("You must specify at least one stream.");
		}
	}

	public void Add(TextWriter tw)
	{
		_streams.Add(tw);
	}

	public void Remove(TextWriter tw)
	{
		_streams.Remove(tw);
	}

	public override void Write(char ch)
	{
		foreach (TextWriter t in _streams)
		{
			t.Write(ch);
		}
	}

	public override void WriteLine(string line)
	{
		foreach (TextWriter t in _streams)
		{
			t.WriteLine(line);
		}
	}

	public override void WriteLine(string line, params object[] args)
	{
		WriteLine(string.Format(line, args));
	}

	public override Encoding Encoding => Encoding.Default;
}
