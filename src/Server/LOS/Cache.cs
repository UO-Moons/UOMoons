using System.Collections.Generic;

namespace Server.Collections
{
	//--------------------------------------------------------------------------------
	//  Implements a basic MRU/LRU cache. Least recently used elements are expired from
	//  the cache after it reaches capacity. Hit() and Store() methods offer O(1)
	//  performance
	//--------------------------------------------------------------------------------
	public class Cache<T, K>
	{
		private readonly Dictionary<K, Dlist.Entry> m_dict;
		private readonly Dlist m_dlist;

		public int Nentries { get; private set; }
		public int Size { get; }

		public long Hits { get; private set; }
		public long Misses { get; private set; }
		public long Ejections { get; private set; }
		public long Stores { get; private set; }

		public Cache(int size)
		{
			Nentries = 0;
			Size = size;
			m_dict = new Dictionary<K, Dlist.Entry>(size);
			m_dlist = new Dlist();
		}
		//----------------------------------------------------------------------------
		//  Hit() -- search for a 'cache hit'
		//----------------------------------------------------------------------------
		public T Hit(K key)
		{
			//        if( !m_dict.ContainsKey( key ) )
			//        {
			//            m_misses++;
			//            return default(T);
			//        }
			//
			//        m_hits++;
			//
			//        Dlist.Entry hit = m_dict[key];
			//
			//        hit.Snip();
			//
			//        m_dlist.PushHead( hit );
			//
			//        return hit.m_data;


			if (m_dict.TryGetValue(key, out Dlist.Entry hit))
			{
				Hits++;
				hit.Snip();
				m_dlist.PushHead(hit);
				return hit.m_data;
			}

			Misses++;
			return default;
		}
		//----------------------------------------------------------------------------
		//  Store() -- store an item in the cache; expires old items
		//----------------------------------------------------------------------------
		public void Store(K key, T val)
		{
			Stores++;

			if (Nentries + 1 > Size)
			{
				Ejections++;

				Dlist.Entry toRemove = m_dlist.PopTail();
				//Console.WriteLine( "removing " + toRemove.m_key );
				m_dict.Remove(toRemove.m_key);
			}
			else Nentries++;

			Dlist.Entry entry = new(key, val);
			m_dlist.PushHead(entry);
			m_dict.Add(key, entry);
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

			internal class Entry
			{
				public K m_key;
				public T m_data;
				public Entry m_previous;
				public Entry m_next;

				public Entry()
				{
					//    m_key = -1;
				}

				public Entry(K key, T data)
				{
					m_key = key;
					m_data = data;
					m_next = m_previous = null;
				}

				public void Snip()
				{
					m_previous.m_next = m_next;
					m_next.m_previous = m_previous;
				}
			}
		}
	}
	//--------------------------------------------------------------------------------
} // namespace Custom.Collections 
  //--------------------------------------------------------------------------------

