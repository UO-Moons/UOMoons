using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Server;

public static class Assembler
{
	public static Assembly[] Assemblies { get; set; }

	private static readonly Type[] m_SerialTypeArray = { typeof(Serial) };

	private static void VerifyType(Type t)
	{
		if (t.IsSubclassOf(typeof(Item)) || t.IsSubclassOf(typeof(Mobile)))
		{
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
					if (t.GetMethod("Serialize", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly) == null)
					{
						warningSb ??= new StringBuilder();

						warningSb.AppendLine("       - No Serialize() method");
					}

					if (t.GetMethod("Deserialize", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly) == null)
					{
						warningSb ??= new StringBuilder();

						warningSb.AppendLine("       - No Deserialize() method");
					}
				}

				if (warningSb is {Length: > 0})
				{
					Console.WriteLine("Warning: {0}\n{1}", t, warningSb.ToString());
				}
			}
			catch
			{
				Console.WriteLine("Warning: Exception in serialization verification of type {0}", t);
			}
		}
	}

	public static bool Load()
	{
		List<Assembly> assemblies = new();

		Console.Write("Loading scripts...");

		assemblies.Add(Assembly.LoadFrom("Scripts.dll"));

		Utility.WriteConsole(ConsoleColor.Green, "done (cached)");

		//Load modules
		if (Directory.Exists("Modules"))
		{
			string[] scripts = Directory.EnumerateFiles($"{Directory.GetCurrentDirectory()}/Modules").Where(file => file.Contains(".dll")).ToArray();
			foreach (string script in scripts)
			{
				Console.Write($"Loading module: {Path.GetFileName(script)} ");

				assemblies.Add(Assembly.LoadFrom(script));

				Utility.WriteConsole(ConsoleColor.Green, "done (cached)");
			}
		}

		assemblies.Add(typeof(Assembler).Assembly);

		Assemblies = assemblies.ToArray();

		Console.Write("Verifying... ");

		Stopwatch watch = Stopwatch.StartNew();

		foreach (Assembly assembly in assemblies)
		{
			foreach (Type t in assembly.GetTypes())
			{
				VerifyType(t);
			}
		}

		watch.Stop();

		Utility.WriteConsole(ConsoleColor.Green, "done. ({0:F2} seconds)", watch.Elapsed.TotalSeconds);

		return true;
	}

	public static void Invoke(string method)
	{
		IEnumerable<Type> types = Assemblies.SelectMany(a => a.GetTypes());

		IEnumerable<MethodInfo> methods = types.Select(t => t.GetMethod(method, BindingFlags.Static | BindingFlags.Public));

		foreach (MethodInfo m in methods.Where(m => m != null).AsParallel().OrderBy(CallPriorityAttribute.GetValue))
		{
			m.Invoke(null, null);
		}
	}

	private static readonly Dictionary<Assembly, TypeCache> m_TypeCaches = new();
	private static TypeCache _nullCache;

	public static TypeCache GetTypeCache(Assembly asm)
	{
		if (asm == null)
		{
			return _nullCache ??= new TypeCache(null);
		}

		m_TypeCaches.TryGetValue(asm, out TypeCache c);

		if (c == null)
			m_TypeCaches[asm] = c = new TypeCache(asm);

		return c;
	}

	public static int FindHashByName(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			return 0;
		}

		var hash = 0;

		for (var i = 0; hash == 0 && i < Assemblies.Length; ++i)
		{
			hash = GetTypeCache(Assemblies[i]).GetTypeHashByName(name);
		}

		return hash != 0 ? hash : GetTypeCache(Core.Assembly).GetTypeHashByName(name);
	}

	public static int FindHashByFullName(string fullName)
	{
		if (string.IsNullOrWhiteSpace(fullName))
		{
			return 0;
		}

		var hash = 0;

		for (var i = 0; hash == 0 && i < Assemblies.Length; ++i)
		{
			hash = GetTypeCache(Assemblies[i]).GetTypeHashByFullName(fullName);
		}

		return hash != 0 ? hash : GetTypeCache(Core.Assembly).GetTypeHashByFullName(fullName);
	}

	public static Type FindTypeByFullName(string fullName)
	{
		return FindTypeByFullName(fullName, true);
	}

	public static Type FindTypeByFullName(string fullName, bool ignoreCase)
	{
		Type type = null;

		if (string.IsNullOrWhiteSpace(fullName))
			return null;

		for (int i = 0; type == null && i < Assemblies.Length; ++i)
			type = GetTypeCache(Assemblies[i]).GetTypeByFullName(fullName, ignoreCase);

		return type ?? GetTypeCache(Core.Assembly).GetTypeByFullName(fullName, ignoreCase);
	}

	public static IEnumerable<Type> FindTypesByFullName(string name)
	{
		return FindTypesByFullName(name, true);
	}

	public static IEnumerable<Type> FindTypesByFullName(string name, bool ignoreCase)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			yield break;
		}

		for (var i = 0; i < Assemblies.Length; ++i)
		{
			foreach (var t in GetTypeCache(Assemblies[i]).GetTypesByFullName(name, ignoreCase))
			{
				yield return t;
			}
		}

		foreach (var t in GetTypeCache(Core.Assembly).GetTypesByFullName(name, ignoreCase))
		{
			yield return t;
		}
	}

	public static Type FindTypeByName(string name)
	{
		return FindTypeByName(name, true);
	}

	public static Type FindTypeByName(string name, bool ignoreCase)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			return null;
		}

		Type type = null;

		for (var i = 0; type == null && i < Assemblies.Length; ++i)
		{
			type = GetTypeCache(Assemblies[i]).GetTypeByName(name, ignoreCase);
		}

		return type ?? GetTypeCache(Core.Assembly).GetTypeByName(name, ignoreCase);
	}

	public static IEnumerable<Type> FindTypesByName(string name)
	{
		return FindTypesByName(name, true);
	}

	public static IEnumerable<Type> FindTypesByName(string name, bool ignoreCase)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			yield break;
		}

		for (var i = 0; i < Assemblies.Length; ++i)
		{
			foreach (var t in GetTypeCache(Assemblies[i]).GetTypesByName(name, ignoreCase))
			{
				yield return t;
			}
		}

		foreach (var t in GetTypeCache(Core.Assembly).GetTypesByName(name, ignoreCase))
		{
			yield return t;
		}
	}

	public static Type FindTypeByNameHash(int hash)
	{
		Type type = null;

		for (var i = 0; type == null && i < Assemblies.Length; ++i)
		{
			type = GetTypeCache(Assemblies[i]).GetTypeByNameHash(hash);
		}

		return type ?? GetTypeCache(Core.Assembly).GetTypeByNameHash(hash);
	}

	public static IEnumerable<Type> FindTypesByNameHash(int hash)
	{
		for (var i = 0; i < Assemblies.Length; ++i)
		{
			foreach (var t in GetTypeCache(Assemblies[i]).GetTypesByNameHash(hash))
			{
				yield return t;
			}
		}

		foreach (var t in GetTypeCache(Core.Assembly).GetTypesByNameHash(hash))
		{
			yield return t;
		}
	}

	public static Type FindTypeByFullNameHash(int hash)
	{
		Type type = null;

		for (var i = 0; type == null && i < Assemblies.Length; ++i)
		{
			type = GetTypeCache(Assemblies[i]).GetTypeByFullNameHash(hash);
		}

		return type ?? GetTypeCache(Core.Assembly).GetTypeByFullNameHash(hash);
	}

	public static IEnumerable<Type> FindTypesByFullNameHash(int hash)
	{
		for (var i = 0; i < Assemblies.Length; ++i)
		{
			foreach (var t in GetTypeCache(Assemblies[i]).GetTypesByFullNameHash(hash))
			{
				yield return t;
			}
		}

		foreach (var t in GetTypeCache(Core.Assembly).GetTypesByFullNameHash(hash))
		{
			yield return t;
		}
	}
}

public class TypeCache
{
	public Type[] Types { get; }
	public TypeTable Names { get; }
	public TypeTable FullNames { get; }

	public Type GetTypeByNameHash(int hash)
	{
		return GetTypesByNameHash(hash).FirstOrDefault(t => t != null);
	}

	public IEnumerable<Type> GetTypesByNameHash(int hash)
	{
		return Names.Get(hash);
	}

	public Type GetTypeByFullNameHash(int hash)
	{
		return GetTypesByFullNameHash(hash).FirstOrDefault(t => t != null);
	}

	public IEnumerable<Type> GetTypesByFullNameHash(int hash)
	{
		return FullNames.Get(hash);
	}

	public Type GetTypeByName(string name, bool ignoreCase)
	{
		return GetTypesByName(name, ignoreCase).FirstOrDefault(t => t != null);
	}

	public IEnumerable<Type> GetTypesByName(string name, bool ignoreCase)
	{
		return Names.Get(name, ignoreCase);
	}

	public Type GetTypeByFullName(string fullName, bool ignoreCase)
	{
		return GetTypesByFullName(fullName, ignoreCase).FirstOrDefault(t => t != null);
	}

	public IEnumerable<Type> GetTypesByFullName(string fullName, bool ignoreCase)
	{
		return FullNames.Get(fullName, ignoreCase);
	}

	public int GetTypeHashByName(string name)
	{
		return Names.GetHash(name);
	}

	public int GetTypeHashByFullName(string fullName)
	{
		return FullNames.GetHash(fullName);
	}

	public TypeCache(Assembly asm)
	{
		if (asm == null)
			Types = Type.EmptyTypes;
		else
			Types = asm.GetTypes();

		Names = new TypeTable(Types.Length);
		FullNames = new TypeTable(Types.Length);

		foreach (var g in Types.ToLookup(t => t.Name))
		{
			Names.Add(g.Key, g);

			foreach (var type in g)
			{
				FullNames.Add(type.FullName, type);

				var attr = type.GetCustomAttribute<TypeAliasAttribute>(false);

				if (attr != null)
				{
					foreach (var a in attr.Aliases)
					{
						FullNames.Add(a, type);
					}
				}
			}
		}

		Names.Sort();
		FullNames.Sort();
	}
}

public class TypeTable
{
	private readonly Dictionary<string, int> _hashes;
	private readonly Dictionary<int, HashSet<Type>> _hashed;
	private readonly Dictionary<string, HashSet<Type>> _sensitive;
	private readonly Dictionary<string, HashSet<Type>> _insensitive;

	public void Sort()
	{
		Sort(_hashed);
		Sort(_sensitive);
		Sort(_insensitive);
	}

	private static void Sort<T>(Dictionary<T, HashSet<Type>> types)
	{
		var sorter = new List<Type>();

		foreach (var list in types.Values)
		{
			sorter.AddRange(list);
			sorter.Sort(InternalSort);

			list.Clear();
			list.UnionWith(sorter);

			sorter.Clear();
		}

		sorter.TrimExcess();
	}

	private static int InternalSort(Type l, Type r)
	{
		if (l == r)
		{
			return 0;
		}

		if (l != null && r == null)
		{
			return -1;
		}

		if (l == null)
		{
			return 1;
		}

		var a = IsEntity(l);
		var b = IsEntity(r);

		if (a && b)
		{
			a = IsConstructable(l, out var x);
			b = IsConstructable(r, out var y);

			return a switch
			{
				true when !b => -1,
				false when b => 1,
				_ => x > y ? -1 : x < y ? 1 : 0
			};
		}

		return a ? -1 : b ? 1 : 0;
	}

	private static bool IsEntity(Type type)
	{
		return type.GetInterface("IEntity") != null;
	}

	private static bool IsConstructable(Type type, out AccessLevel access)
	{
		foreach (var ctor in type.GetConstructors().OrderBy(o => o.GetParameters().Length))
		{
			var attr = ctor.GetCustomAttribute<ConstructableAttribute>(false);

			if (attr != null)
			{
				access = attr.AccessLevel;
				return true;
			}
		}

		access = 0;
		return false;
	}

	public void Add(string key, IEnumerable<Type> types)
	{
		if (!string.IsNullOrWhiteSpace(key) && types != null)
		{
			Add(key, types.ToArray());
		}
	}

	public void Add(string key, params Type[] types)
	{
		if (string.IsNullOrWhiteSpace(key) || types == null || types.Length == 0)
		{
			return;
		}

		if (!_sensitive.TryGetValue(key, out var sensitive) || sensitive == null)
		{
			_sensitive[key] = new HashSet<Type>(types);
		}
		else if (types.Length == 1)
		{
			sensitive.Add(types[0]);
		}
		else
		{
			sensitive.UnionWith(types);
		}

		if (!_insensitive.TryGetValue(key, out var insensitive) || insensitive == null)
		{
			_insensitive[key] = new HashSet<Type>(types);
		}
		else if (types.Length == 1)
		{
			insensitive.Add(types[0]);
		}
		else
		{
			insensitive.UnionWith(types);
		}

		var hash = GenerateHash(key);

		if (!_hashed.TryGetValue(hash, out var hashed) || hashed == null)
		{
			_hashed[hash] = new HashSet<Type>(types);
		}
		else if (types.Length == 1)
		{
			hashed.Add(types[0]);
		}
		else
		{
			hashed.UnionWith(types);
		}
	}

	public IEnumerable<Type> Get(string key, bool ignoreCase)
	{
		if (string.IsNullOrWhiteSpace(key))
		{
			return Type.EmptyTypes;
		}

		HashSet<Type> t;

		if (ignoreCase)
		{
			_insensitive.TryGetValue(key, out t);
		}
		else
		{
			_sensitive.TryGetValue(key, out t);
		}

		if (t == null)
		{
			return Type.EmptyTypes;
		}

		return t.AsEnumerable();
	}

	public IEnumerable<Type> Get(int hash)
	{
		_hashed.TryGetValue(hash, out var t);

		if (t == null)
		{
			return Type.EmptyTypes;
		}

		return t.AsEnumerable();
	}

	public int GetHash(string key)
	{
		if (string.IsNullOrWhiteSpace(key))
		{
			return 0;
		}

		_hashes.TryGetValue(key, out var hash);

		return hash;
	}

	private int GenerateHash(string key)
	{
		if (string.IsNullOrWhiteSpace(key))
		{
			return 0;
		}

		var hash = GetHash(key);

		if (hash != 0)
		{
			return hash;
		}

		hash = key.Length;

		unchecked
		{
			hash = key.Aggregate(hash, (current, t) => (current * 397) ^ Convert.ToInt32(t));
		}

		_hashes[key] = hash;

		return hash;
	}

	public TypeTable(int capacity)
	{
		_hashes = new Dictionary<string, int>();
		_hashed = new Dictionary<int, HashSet<Type>>(capacity);
		_sensitive = new Dictionary<string, HashSet<Type>>(capacity);
		_insensitive = new Dictionary<string, HashSet<Type>>(capacity, StringComparer.OrdinalIgnoreCase);
	}
}
