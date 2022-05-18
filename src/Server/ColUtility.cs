using System;
using System.Collections.Generic;
using System.Linq;

namespace Server
{
	public static class ColUtility
	{
		public static void Free<T>(List<T> l)
		{
			if (l == null)
				return;

			l.Clear();
			l.TrimExcess();
		}

		public static void ForEach<T>(IEnumerable<T> list, Action<T> action)
		{
			if (list == null || action == null)
				return;

			List<T> l = list.ToList();

			foreach (T o in l)
				action(o);

			Free(l);
		}

		public static void ForEach<TKey, TValue>(
			IDictionary<TKey, TValue> dictionary, Action<KeyValuePair<TKey, TValue>> action)
		{
			if (dictionary == null || dictionary.Count == 0 || action == null)
				return;

			List<KeyValuePair<TKey, TValue>> l = dictionary.ToList();

			foreach (KeyValuePair<TKey, TValue> kvp in l)
				action(kvp);

			Free(l);
		}

		public static void ForEach<TKey, TValue>(IDictionary<TKey, TValue> dictionary, Action<TKey, TValue> action)
		{
			if (dictionary == null || dictionary.Count == 0 || action == null)
				return;

			List<KeyValuePair<TKey, TValue>> l = dictionary.ToList();

			foreach (KeyValuePair<TKey, TValue> kvp in l)
				action(kvp.Key, kvp.Value);

			Free(l);
		}

		public static void For<T>(IEnumerable<T> list, Action<int, T> action)
		{
			if (list == null || action == null)
				return;

			List<T> l = list.ToList();

			for (int i = 0; i < l.Count; i++)
				action(i, l[i]);

			Free(l);
		}

		public static void For<TKey, TValue>(IDictionary<TKey, TValue> list, Action<int, TKey, TValue> action)
		{
			if (list == null || action == null)
				return;

			List<KeyValuePair<TKey, TValue>> l = list.ToList();

			for (int i = 0; i < l.Count; i++)
				action(i, l[i].Key, l[i].Value);

			Free(l);
		}

		public static void IterateReverse<T>(this T[] list, Action<T> action)
		{
			if (list == null || action == null)
			{
				return;
			}

			int i = list.Length;

			while (--i >= 0)
			{
				if (i < list.Length)
				{
					action(list[i]);
				}
			}
		}

		public static void IterateReverse<T>(this List<T> list, Action<T> action)
		{
			if (list == null || action == null)
			{
				return;
			}

			int i = list.Count;

			while (--i >= 0)
			{
				if (i < list.Count)
				{
					action(list[i]);
				}
			}
		}

		public static void IterateReverse<T>(this IEnumerable<T> list, Action<T> action)
		{
			if (list == null || action == null)
			{
				return;
			}

			if (list is T[])
			{
				IterateReverse((T[])list, action);
				return;
			}

			if (list is List<T>)
			{
				IterateReverse((List<T>)list, action);
				return;
			}

			var toList = list.ToList();

			foreach (var o in toList)
			{
				action(o);
			}

			Free(toList);
		}

		public static void SafeDelete<T>(List<T> list)
		{
			SafeDelete(list, null);
		}

		/// <summary>
		/// Safely deletes any entities based on predicate from a list that by deleting such entity would cause the collection to be modified.
		/// ie item.Items or mobile.Items. Omitting the predicate will delete all items in the collection.
		/// </summary>
		/// <param name="list"></param>
		/// <param name="predicate"></param>
		public static void SafeDelete<T>(List<T> list, Func<T, bool> predicate)
		{
			if (list == null)
			{
				return;
			}

			int i = list.Count;

			while (--i >= 0)
			{
				if (i < list.Count)
				{
					var entity = list[i] as IEntity;

					if (entity != null && !entity.Deleted && (predicate == null || predicate((T)entity)))
					{
						entity.Delete();
					}
				}
			}
		}
	}
}
