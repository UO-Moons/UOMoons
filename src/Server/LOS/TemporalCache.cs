using System;
using System.Collections.Generic;

namespace Server.Collections
{
	//--------------------------------------------------------------------------------
	//  Implements a basic MRU/LRU cache, with the added twist of the idea of temporal
	//  validation. The cache has both a maximum absolute size, as well as an age
	//  limit (in seconds) for each entry. Entries > limit are discared and don't
	//  count as cache "hits".
	//--------------------------------------------------------------------------------
	public class TemporalCache<T, K>
	{
		int m_nentries;
		private readonly int m_size;
		private readonly double m_limit_secs;
		private readonly Dictionary<K, Dlist.Entry> m_dict;
		private readonly Dlist m_dlist;

		public int Nentries { get { return m_nentries; } }
		public int Size { get { return m_size; } }

		public TemporalCache(int size, double limit_secs)
		{
			m_nentries = 0;
			m_size = size;
			m_limit_secs = limit_secs;
			m_dict = new Dictionary<K, Dlist.Entry>(size);
			m_dlist = new Dlist();
		}
		//----------------------------------------------------------------------------
		//  ContainsKey() -- search for a 'cache hit'
		//----------------------------------------------------------------------------
		public bool ContainsKey(K key)
		{
			Cleaner(2);

			if (!m_dict.ContainsKey(key))
			{
				//Console.WriteLine( "Entry not present" );
				return false;
			}

			Dlist.Entry hit = m_dict[key];

			hit.Snip();

			double diff = (DateTime.Now.Ticks - hit.m_time) / 10000000.0;

			if (diff > m_limit_secs)
			{
				//Console.WriteLine( "Entry too old = {0}", diff );
				m_dict.Remove(key);
				return false;
			}

			m_dlist.PushHead(hit);

			//hit.m_time = DateTime.Now.Ticks; XXX oopsie, no don't refresh the time

			return true;
		}
		//----------------------------------------------------------------------------
		//  Hit() -- search for a 'cache hit'
		//----------------------------------------------------------------------------
		public T Hit(K key)
		{
			Cleaner(2);

			if (!m_dict.ContainsKey(key))
				return default;

			Dlist.Entry hit = m_dict[key];

			hit.Snip();

			double diff = DateTime.Now.Ticks - hit.m_time;

			if (diff / 10000000.0 > m_limit_secs)
				return default;

			m_dlist.PushHead(hit);

			//hit.m_time = DateTime.Now.Ticks; XXX oopsie, no don't refresh the time

			return hit.m_val;
		}
		//----------------------------------------------------------------------------
		//  Update() -- updates only; does not update time stamp
		//----------------------------------------------------------------------------
		public void Update(K key, T val)
		{
			if (m_dict.ContainsKey(key))
			{
				Dlist.Entry hit = m_dict[key];

				hit.m_val = val;

				hit.Snip();
				m_dlist.PushHead(hit);
			}
			else
			{
				Dlist.Entry entry = new(key, val);
				m_dlist.PushHead(entry);
				m_dict.Add(key, entry);
			}
		}
		//----------------------------------------------------------------------------
		//  Store() -- store an item in the cache; expires old items (blows over them)
		//----------------------------------------------------------------------------
		public void Store(K key, T val)
		{
			if (m_dict.ContainsKey(key))
			{
				Dlist.Entry hit = m_dict[key];

				hit.m_time = DateTime.Now.Ticks;

				hit.Snip();
				m_dlist.PushHead(hit);
			}
			else
			{
				Dlist.Entry entry = new(key, val);
				m_dlist.PushHead(entry);
				m_dict.Add(key, entry);
			}
		}
		//----------------------------------------------------------------------------
		//   Cleaner() -- this looks at the last few entries in the cache and 
		//     removes them if they are too old. This is mostly to cull unneeded
		//     memory. WARNING: NOT THREAD SAFE. If you're getting the idea of
		//     running the cleaner in a subthread, thread control primitives must
		//     be introduced to hit, store, and cleaner all three.
		//----------------------------------------------------------------------------
		public void Cleaner(int max)
		{
			if (m_nentries > 1)
			{
				for (int i = 0; i <= max; i++)
				{
					Dlist.Entry peek = m_dlist.PeekTail();

					if (peek.m_time == -1) return;

					double diff = DateTime.Now.Ticks - peek.m_time;

					if (diff / 10000000.0 > m_limit_secs) m_dlist.PopTail();

					m_nentries--;
				}
			}
		}
		//----------------------------------------------------------------------------
		//  Minimal implementation of a basic doubly-linked list that exposes its
		//  internals in a fashion amenable to the LRU/MRU cache functionality
		//----------------------------------------------------------------------------
		internal class Dlist
		{
			private readonly Entry m_sentinel;

			public Dlist()
			{
				m_sentinel = new Entry();
				m_sentinel.m_next = m_sentinel.m_previous = m_sentinel;
			}

			public void PushHead(Entry entry)
			{
				entry.m_next = m_sentinel.m_next;
				entry.m_previous = m_sentinel;

				m_sentinel.m_next.m_previous = entry;
				m_sentinel.m_next = entry;
			}

			public Entry PopTail()
			{
				Entry tail = m_sentinel.m_previous;

				tail.m_previous.m_next = m_sentinel;

				m_sentinel.m_previous = tail.m_previous;

				return tail;
			}

			public Entry PeekTail()
			{
				Entry tail = m_sentinel.m_previous;

				return tail;
			}

			internal class Entry
			{
				public K m_key;
				public T m_val;
				public long m_time;
				public Entry m_previous;
				public Entry m_next;

				public Entry()
				{
					m_time = -1;
				}

				public Entry(K key, T val)
				{
					m_key = key;
					m_val = val;
					m_next = m_previous = null;
					m_time = DateTime.Now.Ticks;
				}

				public void Snip()
				{
					m_previous.m_next = m_next;
					m_next.m_previous = m_previous;
				}
			}
		}
	}
}

