using System.Collections.Generic;

namespace Server.Collections;

//--------------------------------------------------------------------------------
//  Implements a basic MRU/LRU cache. Least recently used elements are expired from
//  the cache after it reaches capacity. Hit() and Store() methods offer O(1)
//  performance
//--------------------------------------------------------------------------------
public class Cache<T, K>
{
	private readonly Dictionary<K, Dlist.Entry> _dict;
	private readonly Dlist _dlist;

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
		_dict = new Dictionary<K, Dlist.Entry>(size);
		_dlist = new Dlist();
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


		if (_dict.TryGetValue(key, out Dlist.Entry hit))
		{
			Hits++;
			hit.Snip();
			_dlist.PushHead(hit);
			return hit.Data;
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

			Dlist.Entry toRemove = _dlist.PopTail();
			//Console.WriteLine( "removing " + toRemove.m_key );
			_dict.Remove(toRemove.Key);
		}
		else Nentries++;

		Dlist.Entry entry = new(key, val);
		_dlist.PushHead(entry);
		_dict.Add(key, entry);
	}
	//----------------------------------------------------------------------------
	//  Minimal implementation of a basic doubly-linked list that exposes its
	//  internals in a fashion amenable to the LRU/MRU cache functionality
	//----------------------------------------------------------------------------
	internal class Dlist
	{
		private readonly Entry _sentinel;

		public Dlist()
		{
			_sentinel = new Entry();
			_sentinel.Next = _sentinel.Previous = _sentinel;
		}

		public void PushHead(Entry entry)
		{
			entry.Next = _sentinel.Next;
			entry.Previous = _sentinel;

			_sentinel.Next.Previous = entry;
			_sentinel.Next = entry;
		}

		public Entry PopTail()
		{
			Entry tail = _sentinel.Previous;

			tail.Previous.Next = _sentinel;

			_sentinel.Previous = tail.Previous;

			return tail;
		}

		internal class Entry
		{
			public K Key;
			public T Data;
			public Entry Previous;
			public Entry Next;

			public Entry()
			{
				//    m_key = -1;
			}

			public Entry(K key, T data)
			{
				Key = key;
				Data = data;
				Next = Previous = null;
			}

			public void Snip()
			{
				Previous.Next = Next;
				Next.Previous = Previous;
			}
		}
	}
}

