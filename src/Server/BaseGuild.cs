using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Guilds;

public enum GuildType
{
	Regular,
	Chaos,
	Order
}

public abstract class BaseGuild : ISerializable
{
	protected BaseGuild(int id) //serialization ctor
	{
		Id = id;
		List.Add(Id, this);
		if (Id + 1 > _nextId)
		{
			_nextId = Id + 1;
		}
	}

	protected BaseGuild()
	{
		Id = _nextId++;
		List.Add(Id, this);
	}

	[CommandProperty(AccessLevel.Counselor)]
	public int Id { get; }

	int ISerializable.TypeReference => 0;

	int ISerializable.SerialIdentity => Id;

	public abstract void Deserialize(GenericReader reader);
	public abstract void Serialize(GenericWriter writer);

	public abstract string Abbreviation { get; set; }
	public abstract string Name { get; set; }

	public virtual GuildType Type { get; set; }

	public abstract bool Disbanded { get; }

	public abstract void OnDelete(Mobile mob);

	private static int _nextId = 1;

	public static Dictionary<int, BaseGuild> List { get; } = new();

	public static BaseGuild Find(int id)
	{

		List.TryGetValue(id, out var g);

		return g;
	}

	public static BaseGuild FindByName(string name)
	{
		return List.Values.FirstOrDefault(g => g.Name == name);
	}

	public static BaseGuild FindByAbbrev(string abbr)
	{
		return List.Values.FirstOrDefault(g => g.Abbreviation == abbr);
	}

	public static List<BaseGuild> Search(string find)
	{
		var words = find.ToLower().Split(' ');

		return (from g in List.Values let name = g.Name.ToLower() let match = words.All(t => name.IndexOf(t, StringComparison.Ordinal) != -1) where match select g).ToList();
	}

	public override string ToString()
	{
		return $"0x{Id:X} \"{Name} [{Abbreviation}]\"";
	}
}
