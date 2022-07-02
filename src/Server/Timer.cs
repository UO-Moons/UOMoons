using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Server.Diagnostics;

namespace Server;

public enum TimerPriority
{
	EveryTick,
	TenMs,
	TwentyFiveMs,
	FiftyMs,
	TwoFiftyMs,
	OneSecond,
	FiveSeconds,
	OneMinute
}

public delegate void TimerCallback();
public delegate void TimerStateCallback(object state);
public delegate void TimerStateCallback<in T>(T state);
public delegate void TimerStateCallback<in T1, in T2>(T1 state1, T2 state2);
public delegate void TimerStateCallback<in T1, in T2, in T3>(T1 state1, T2 state2, T3 state3);
public delegate void TimerStateCallback<in T1, in T2, in T3, in T4>(T1 state1, T2 state2, T3 state3, T4 state4);
public delegate void TimerStateCallback<in T1, in T2, in T3, in T4, in T5>(T1 state1, T2 state2, T3 state3, T4 state4, T5 state5);

public class Timer
{
	private long _next;
	private long _delay;
	private long _interval;
	private bool _running;
	private int _index;
	private readonly int _count;
	private TimerPriority _priority;
	private List<Timer> _list;
	private bool _prioritySet;

	private static string FormatDelegate(Delegate callback)
	{
		if (callback == null)
		{
			return "null";
		}

		return callback.Method.DeclaringType == null ? callback.Method.Name : $"{callback.Method.DeclaringType.FullName}.{callback.Method.Name}";
	}

	public static void DumpInfo(TextWriter tw)
	{
		TimerThread.Dump(tw);
	}

	public TimerPriority Priority
	{
		get => _priority;
		set
		{
			if (!_prioritySet)
			{
				_prioritySet = true;
			}

			if (_priority == value)
			{
				return;
			}

			_priority = value;

			if (_running)
			{
				TimerThread.PriorityChange(this, (int)_priority);
			}
		}
	}

	public DateTime Next => DateTime.UtcNow.AddMilliseconds(_next - Core.TickCount);

	public TimeSpan Delay
	{
		get => TimeSpan.FromMilliseconds(_delay);
		set => _delay = (long)value.TotalMilliseconds;
	}

	public TimeSpan Interval
	{
		get => TimeSpan.FromMilliseconds(_interval);
		set => _interval = (long)value.TotalMilliseconds;
	}

	public bool Running
	{
		get => _running;
		set
		{
			if (value)
			{
				Start();
			}
			else
			{
				Stop();
			}
		}
	}

	public TimerProfile GetProfile()
	{
		return Core.Profiling ? TimerProfile.Acquire(ToString()) : null;
	}

	public class TimerThread
	{
		private static readonly Dictionary<Timer, TimerChangeEntry> m_Changed = new();

		private static readonly long[] m_NextPriorities = new long[8];
		private static readonly long[] m_PriorityDelays = { 0, 10, 25, 50, 250, 1000, 5000, 60000 };

		private static readonly List<Timer>[] m_Timers =
		{
			new(), new(), new(), new(),
			new(), new(), new(), new()
		};

		private static readonly Dictionary<string, int>[] m_Dump = new Dictionary<string, int>[m_Timers.Length];

		private static DateTime _dumped;

		public static void Dump(TextWriter tw)
		{
			var now = DateTime.UtcNow;

			tw.WriteLine($"Date: {now}");

			if (_dumped > DateTime.MinValue)
			{
				tw.WriteLine($"Last: {_dumped}");
				tw.WriteLine($"Span: {now - _dumped}");
			}

			tw.WriteLine();
			tw.WriteLine();

			for (var i = 0; i < m_Timers.Length; i++)
			{
				tw.WriteLine($"Priority: {(TimerPriority)i}");
				tw.WriteLine();

				var total = (double)m_Timers[i].Count;

				var timers = m_Timers[i].GroupBy(t => t.ToString()).ToDictionary(o => o.Key, o => o.Count());

				foreach (var (key, count) in timers.OrderByDescending(o => o.Value))
				{
					var percent = count / total;

					var line = $"{count:#,0} ({percent:P1})";

					if (m_Dump[i] != null && m_Dump[i].TryGetValue(key, out var lastCount))
					{
						var diff = count - lastCount;

						switch (diff)
						{
							case > 0:
								line += $" [+{diff:#,0}]";
								break;
							case < 0:
								line += $" [{diff:#,0}]";
								break;
						}
					}

					var tabs = new string('\t', 6 - line.Length / 8);

					tw.WriteLine($"{line}{tabs}{key}");
				}

				m_Dump[i] = timers;

				tw.WriteLine();
				tw.WriteLine();
			}

			_dumped = now;
		}

		private class TimerChangeEntry
		{
			public Timer Timer;
			public int NewIndex;
			public bool IsAdd;

			private TimerChangeEntry(Timer t, int newIndex, bool isAdd)
			{
				Timer = t;
				NewIndex = newIndex;
				IsAdd = isAdd;
			}

			public void Free()
			{
				Timer = null;

				lock (m_InstancePool)
				{
					if (m_InstancePool.Count < 512) // Arbitrary
					{
						m_InstancePool.Enqueue(this);
					}
				}
			}

			private static readonly Queue<TimerChangeEntry> m_InstancePool = new();

			public static TimerChangeEntry GetInstance(Timer t, int newIndex, bool isAdd)
			{
				TimerChangeEntry e = null;

				lock (m_InstancePool)
				{
					if (m_InstancePool.Count > 0)
					{
						e = m_InstancePool.Dequeue();
					}
				}

				if (e != null)
				{
					e.Timer = t;
					e.NewIndex = newIndex;
					e.IsAdd = isAdd;
				}
				else
				{
					e = new TimerChangeEntry(t, newIndex, isAdd);
				}

				return e;
			}
		}

		public static void Change(Timer t, int newIndex, bool isAdd)
		{
			lock (m_Changed)
			{
				m_Changed[t] = TimerChangeEntry.GetInstance(t, newIndex, isAdd);
			}

			m_Signal.Set();
		}

		public static void AddTimer(Timer t)
		{
			Change(t, (int)t.Priority, true);
		}

		public static void PriorityChange(Timer t, int newPrio)
		{
			Change(t, newPrio, false);
		}

		public static void RemoveTimer(Timer t)
		{
			Change(t, -1, false);
		}

		private static void ProcessChanged()
		{
			lock (m_Changed)
			{
				var curTicks = Core.TickCount;

				foreach (var tce in m_Changed.Values)
				{
					var timer = tce.Timer;
					var newIndex = tce.NewIndex;

					if (timer._list != null)
					{
						timer._list.Remove(timer);
					}

					if (tce.IsAdd)
					{
						timer._next = curTicks + timer._delay;
						timer._index = 0;
					}

					if (newIndex >= 0)
					{
						timer._list = m_Timers[newIndex];
						timer._list.Add(timer);
					}
					else
					{
						timer._list = null;
					}

					tce.Free();
				}

				m_Changed.Clear();
			}
		}

		private static readonly AutoResetEvent m_Signal = new(false);

		public static void Set()
		{
			m_Signal.Set();
		}

		public static void TimerMain()
		{
			while (!Core.Closing)
			{
				if (World.Loading || World.Saving)
				{
					m_Signal.WaitOne(1, false);
					continue;
				}

				ProcessChanged();

				var loaded = false;

				int i;
				for (i = 0; i < m_Timers.Length; i++)
				{
					var now = Core.TickCount;

					if (now < m_NextPriorities[i])
					{
						break;
					}

					m_NextPriorities[i] = now + m_PriorityDelays[i];

					int j;
					for (j = 0; j < m_Timers[i].Count; j++)
					{
						var t = m_Timers[i][j];

						if (t._queued || now <= t._next)
						{
							continue;
						}

						t._queued = true;

						lock (m_Queue)
						{
							m_Queue.Enqueue(t);
						}

						loaded = true;

						if (t._count != 0 && (++t._index >= t._count))
						{
							t.Stop();
						}
						else
						{
							t._next = now + t._interval;
						}
					}
				}

				if (loaded)
				{
					Core.Set();
				}

				m_Signal.WaitOne(1, false);
			}
		}
	}

	private static readonly Queue<Timer> m_Queue = new();

	public static int BreakCount { get; set; } = 20000;

	private bool _queued;

	public static void Slice()
	{
		lock (m_Queue)
		{
			var index = 0;

			while (index < BreakCount && m_Queue.Count != 0)
			{
				var t = m_Queue.Dequeue();
				var prof = t.GetProfile();

				prof?.Start();

				t.OnTick();
				t._queued = false;
				++index;

				prof?.Finish();
			}
		}
	}

	public Timer(TimeSpan delay)
		: this(delay, TimeSpan.Zero, 1)
	{ }

	public Timer(TimeSpan delay, TimeSpan interval)
		: this(delay, interval, 0)
	{ }

	public virtual bool DefRegCreation => true;

	public void RegCreation()
	{
		var prof = GetProfile();

		if (prof != null)
		{
			prof.Created++;
		}
	}

	public Timer(TimeSpan delay, TimeSpan interval, int count)
	{
		_toString = GetType().FullName;

		_delay = (long)delay.TotalMilliseconds;
		_interval = (long)interval.TotalMilliseconds;
		_count = count;

		if (!_prioritySet)
		{
			_priority = ComputePriority(count == 1 ? delay : interval);
			_prioritySet = true;
		}

		if (DefRegCreation)
		{
			RegCreation();
		}
	}

	private readonly string _toString;

	public override string ToString()
	{
		return _toString;
	}

	public static TimerPriority ComputePriority(TimeSpan ts)
	{
		if (ts.TotalMinutes >= 10.0)
		{
			return TimerPriority.OneMinute;
		}

		return ts.TotalSeconds switch
		{
			>= 30.0 => TimerPriority.FiveSeconds,
			>= 10.0 => TimerPriority.OneSecond,
			>= 5.0 => TimerPriority.TwoFiftyMs,
			>= 2.5 => TimerPriority.FiftyMs,
			>= 1.0 => TimerPriority.TwentyFiveMs,
			>= 0.5 => TimerPriority.TenMs,
			_ => TimerPriority.EveryTick
		};
	}

	#region DelayCall(..)
	public static Timer DelayCall(TimerCallback callback)
	{
		return DelayCall(TimeSpan.Zero, TimeSpan.Zero, 1, callback);
	}

	public static Timer DelayCall(TimeSpan delay, TimerCallback callback)
	{
		return DelayCall(delay, TimeSpan.Zero, 1, callback);
	}

	public static Timer DelayCall(TimeSpan delay, TimeSpan interval, TimerCallback callback)
	{
		return DelayCall(delay, interval, 0, callback);
	}

	public static Timer DelayCall(TimeSpan delay, TimeSpan interval, int count, TimerCallback callback)
	{
		Timer t = new DelayCallTimer(delay, interval, count, callback)
		{
			Priority = ComputePriority(count == 1 ? delay : interval)
		};

		t.Start();

		return t;
	}

	public static Timer DelayCall(TimerStateCallback callback, object state)
	{
		return DelayCall(TimeSpan.Zero, TimeSpan.Zero, 1, callback, state);
	}

	public static Timer DelayCall(TimeSpan delay, TimerStateCallback callback, object state)
	{
		return DelayCall(delay, TimeSpan.Zero, 1, callback, state);
	}

	public static Timer DelayCall(TimeSpan delay, TimeSpan interval, TimerStateCallback callback, object state)
	{
		return DelayCall(delay, interval, 0, callback, state);
	}

	public static Timer DelayCall(TimeSpan delay, TimeSpan interval, int count, TimerStateCallback callback, object state)
	{
		Timer t = new DelayStateCallTimer(delay, interval, count, callback, state)
		{
			Priority = ComputePriority(count == 1 ? delay : interval)
		};

		t.Start();

		return t;
	}
	#endregion

	#region DelayCall<T>(..)
	public static Timer DelayCall<T>(TimerStateCallback<T> callback, T state)
	{
		return DelayCall(TimeSpan.Zero, TimeSpan.Zero, 1, callback, state);
	}

	public static Timer DelayCall<T>(TimeSpan delay, TimerStateCallback<T> callback, T state)
	{
		return DelayCall(delay, TimeSpan.Zero, 1, callback, state);
	}

	public static Timer DelayCall<T>(TimeSpan delay, TimeSpan interval, TimerStateCallback<T> callback, T state)
	{
		return DelayCall(delay, interval, 0, callback, state);
	}

	public static Timer DelayCall<T>(TimeSpan delay, TimeSpan interval, int count, TimerStateCallback<T> callback, T state)
	{
		Timer t = new DelayStateCallTimer<T>(delay, interval, count, callback, state)
		{
			Priority = ComputePriority(count == 1 ? delay : interval)
		};

		t.Start();

		return t;
	}
	#endregion

	#region DelayCall<T1, T2>(..)
	public static Timer DelayCall<T1, T2>(TimerStateCallback<T1, T2> callback, T1 state1, T2 state2)
	{
		return DelayCall(TimeSpan.Zero, TimeSpan.Zero, 1, callback, state1, state2);
	}

	public static Timer DelayCall<T1, T2>(TimeSpan delay, TimerStateCallback<T1, T2> callback, T1 state1, T2 state2)
	{
		return DelayCall(delay, TimeSpan.Zero, 1, callback, state1, state2);
	}

	public static Timer DelayCall<T1, T2>(TimeSpan delay, TimeSpan interval, TimerStateCallback<T1, T2> callback, T1 state1, T2 state2)
	{
		return DelayCall(delay, interval, 0, callback, state1, state2);
	}

	public static Timer DelayCall<T1, T2>(TimeSpan delay, TimeSpan interval, int count, TimerStateCallback<T1, T2> callback, T1 state1, T2 state2)
	{
		Timer t = new DelayStateCallTimer<T1, T2>(delay, interval, count, callback, state1, state2)
		{
			Priority = ComputePriority(count == 1 ? delay : interval)
		};

		t.Start();

		return t;
	}
	#endregion

	#region DelayCall<T1, T2, T3>(..)
	public static Timer DelayCall<T1, T2, T3>(TimerStateCallback<T1, T2, T3> callback, T1 state1, T2 state2, T3 state3)
	{
		return DelayCall(TimeSpan.Zero, TimeSpan.Zero, 1, callback, state1, state2, state3);
	}

	public static Timer DelayCall<T1, T2, T3>(TimeSpan delay, TimerStateCallback<T1, T2, T3> callback, T1 state1, T2 state2, T3 state3)
	{
		return DelayCall(delay, TimeSpan.Zero, 1, callback, state1, state2, state3);
	}

	public static Timer DelayCall<T1, T2, T3>(TimeSpan delay, TimeSpan interval, TimerStateCallback<T1, T2, T3> callback, T1 state1, T2 state2, T3 state3)
	{
		return DelayCall(delay, interval, 0, callback, state1, state2, state3);
	}

	public static Timer DelayCall<T1, T2, T3>(TimeSpan delay, TimeSpan interval, int count, TimerStateCallback<T1, T2, T3> callback, T1 state1, T2 state2, T3 state3)
	{
		Timer t = new DelayStateCallTimer<T1, T2, T3>(delay, interval, count, callback, state1, state2, state3)
		{
			Priority = ComputePriority(count == 1 ? delay : interval)
		};

		t.Start();

		return t;
	}
	#endregion

	#region DelayCall<T1, T2, T3, T4>(..)
	public static Timer DelayCall<T1, T2, T3, T4>(TimerStateCallback<T1, T2, T3, T4> callback, T1 state1, T2 state2, T3 state3, T4 state4)
	{
		return DelayCall(TimeSpan.Zero, TimeSpan.Zero, 1, callback, state1, state2, state3, state4);
	}

	public static Timer DelayCall<T1, T2, T3, T4>(TimeSpan delay, TimerStateCallback<T1, T2, T3, T4> callback, T1 state1, T2 state2, T3 state3, T4 state4)
	{
		return DelayCall(delay, TimeSpan.Zero, 1, callback, state1, state2, state3, state4);
	}

	public static Timer DelayCall<T1, T2, T3, T4>(TimeSpan delay, TimeSpan interval, TimerStateCallback<T1, T2, T3, T4> callback, T1 state1, T2 state2, T3 state3, T4 state4)
	{
		return DelayCall(delay, interval, 0, callback, state1, state2, state3, state4);
	}

	public static Timer DelayCall<T1, T2, T3, T4>(TimeSpan delay, TimeSpan interval, int count, TimerStateCallback<T1, T2, T3, T4> callback, T1 state1, T2 state2, T3 state3, T4 state4)
	{
		Timer t = new DelayStateCallTimer<T1, T2, T3, T4>(delay, interval, count, callback, state1, state2, state3, state4)
		{
			Priority = ComputePriority(count == 1 ? delay : interval)
		};

		t.Start();

		return t;
	}
	#endregion

	#region DelayCall<T1, T2, T3, T4, T5>(..)
	public static Timer DelayCall<T1, T2, T3, T4, T5>(TimerStateCallback<T1, T2, T3, T4, T5> callback, T1 state1, T2 state2, T3 state3, T4 state4, T5 state5)
	{
		return DelayCall(TimeSpan.Zero, TimeSpan.Zero, 1, callback, state1, state2, state3, state4, state5);
	}

	public static Timer DelayCall<T1, T2, T3, T4, T5>(TimeSpan delay, TimerStateCallback<T1, T2, T3, T4, T5> callback, T1 state1, T2 state2, T3 state3, T4 state4, T5 state5)
	{
		return DelayCall(delay, TimeSpan.Zero, 1, callback, state1, state2, state3, state4, state5);
	}

	public static Timer DelayCall<T1, T2, T3, T4, T5>(TimeSpan delay, TimeSpan interval, TimerStateCallback<T1, T2, T3, T4, T5> callback, T1 state1, T2 state2, T3 state3, T4 state4, T5 state5)
	{
		return DelayCall(delay, interval, 0, callback, state1, state2, state3, state4, state5);
	}

	public static Timer DelayCall<T1, T2, T3, T4, T5>(TimeSpan delay, TimeSpan interval, int count, TimerStateCallback<T1, T2, T3, T4, T5> callback, T1 state1, T2 state2, T3 state3, T4 state4, T5 state5)
	{
		Timer t = new DelayStateCallTimer<T1, T2, T3, T4, T5>(delay, interval, count, callback, state1, state2, state3, state4, state5)
		{
			Priority = ComputePriority(count == 1 ? delay : interval)
		};

		t.Start();

		return t;
	}
	#endregion

	#region DelayCall Timers
	private class DelayCallTimer : Timer
	{
		private TimerCallback Callback { get; }

		public override bool DefRegCreation => false;

		public DelayCallTimer(TimeSpan delay, TimeSpan interval, int count, TimerCallback callback)
			: base(delay, interval, count)
		{
			Callback = callback;
			RegCreation();
		}

		protected override void OnTick()
		{
			Callback?.Invoke();
		}

		public override string ToString()
		{
			return $"DelayCallTimer[{FormatDelegate(Callback)}]";
		}
	}

	private class DelayStateCallTimer : Timer
	{
		private readonly object _state;

		private TimerStateCallback Callback { get; }

		public override bool DefRegCreation => false;

		public DelayStateCallTimer(TimeSpan delay, TimeSpan interval, int count, TimerStateCallback callback, object state)
			: base(delay, interval, count)
		{
			Callback = callback;
			_state = state;

			RegCreation();
		}

		protected override void OnTick()
		{
			Callback?.Invoke(_state);
		}

		public override string ToString()
		{
			return $"DelayStateCall[{FormatDelegate(Callback)}]";
		}
	}

	private class DelayStateCallTimer<T> : Timer
	{
		private readonly T _state;

		private TimerStateCallback<T> Callback { get; }

		public override bool DefRegCreation => false;

		public DelayStateCallTimer(TimeSpan delay, TimeSpan interval, int count, TimerStateCallback<T> callback, T state)
			: base(delay, interval, count)
		{
			Callback = callback;
			_state = state;

			RegCreation();
		}

		protected override void OnTick()
		{
			Callback?.Invoke(_state);
		}

		public override string ToString()
		{
			return $"DelayStateCall[{FormatDelegate(Callback)}]";
		}
	}

	private class DelayStateCallTimer<T1, T2> : Timer
	{
		private readonly T1 _state1;
		private readonly T2 _state2;

		private TimerStateCallback<T1, T2> Callback { get; }

		public override bool DefRegCreation => false;

		public DelayStateCallTimer(TimeSpan delay, TimeSpan interval, int count, TimerStateCallback<T1, T2> callback, T1 state1, T2 state2)
			: base(delay, interval, count)
		{
			Callback = callback;
			_state1 = state1;
			_state2 = state2;

			RegCreation();
		}

		protected override void OnTick()
		{
			Callback?.Invoke(_state1, _state2);
		}

		public override string ToString()
		{
			return $"DelayStateCall[{FormatDelegate(Callback)}]";
		}
	}

	private class DelayStateCallTimer<T1, T2, T3> : Timer
	{
		private readonly T1 _state1;
		private readonly T2 _state2;
		private readonly T3 _state3;

		private TimerStateCallback<T1, T2, T3> Callback { get; }

		public override bool DefRegCreation => false;

		public DelayStateCallTimer(TimeSpan delay, TimeSpan interval, int count, TimerStateCallback<T1, T2, T3> callback, T1 state1, T2 state2, T3 state3)
			: base(delay, interval, count)
		{
			Callback = callback;
			_state1 = state1;
			_state2 = state2;
			_state3 = state3;

			RegCreation();
		}

		protected override void OnTick()
		{
			Callback?.Invoke(_state1, _state2, _state3);
		}

		public override string ToString()
		{
			return $"DelayStateCall[{FormatDelegate(Callback)}]";
		}
	}

	private class DelayStateCallTimer<T1, T2, T3, T4> : Timer
	{
		private readonly T1 _state1;
		private readonly T2 _state2;
		private readonly T3 _state3;
		private readonly T4 _state4;

		private TimerStateCallback<T1, T2, T3, T4> Callback { get; }

		public override bool DefRegCreation => false;

		public DelayStateCallTimer(TimeSpan delay, TimeSpan interval, int count, TimerStateCallback<T1, T2, T3, T4> callback, T1 state1, T2 state2, T3 state3, T4 state4)
			: base(delay, interval, count)
		{
			Callback = callback;
			_state1 = state1;
			_state2 = state2;
			_state3 = state3;
			_state4 = state4;

			RegCreation();
		}

		protected override void OnTick()
		{
			Callback?.Invoke(_state1, _state2, _state3, _state4);
		}

		public override string ToString()
		{
			return $"DelayStateCall[{FormatDelegate(Callback)}]";
		}
	}

	private class DelayStateCallTimer<T1, T2, T3, T4, T5> : Timer
	{
		private readonly T1 _state1;
		private readonly T2 _state2;
		private readonly T3 _state3;
		private readonly T4 _state4;
		private readonly T5 _state5;

		private TimerStateCallback<T1, T2, T3, T4, T5> Callback { get; }

		public override bool DefRegCreation => false;

		public DelayStateCallTimer(TimeSpan delay, TimeSpan interval, int count, TimerStateCallback<T1, T2, T3, T4, T5> callback, T1 state1, T2 state2, T3 state3, T4 state4, T5 state5)
			: base(delay, interval, count)
		{
			Callback = callback;
			_state1 = state1;
			_state2 = state2;
			_state3 = state3;
			_state4 = state4;
			_state5 = state5;

			RegCreation();
		}

		protected override void OnTick()
		{
			Callback?.Invoke(_state1, _state2, _state3, _state4, _state5);
		}

		public override string ToString()
		{
			return $"DelayStateCall[{FormatDelegate(Callback)}]";
		}
	}
	#endregion

	public void Start()
	{
		if (_running)
		{
			return;
		}

		_running = true;

		TimerThread.AddTimer(this);

		OnStart();

		var prof = GetProfile();

		if (prof != null)
		{
			prof.Started++;
		}
	}

	public void Stop()
	{
		if (!_running)
		{
			return;
		}

		_running = false;

		TimerThread.RemoveTimer(this);

		OnStop();

		var prof = GetProfile();

		if (prof != null)
		{
			prof.Stopped++;
		}
	}

	protected virtual void OnStart()
	{
	}

	protected virtual void OnTick()
	{ }

	protected virtual void OnStop()
	{
	}
}
