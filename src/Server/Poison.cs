using System;
using System.Collections.Generic;
using System.Linq;

namespace Server;

[Parsable]
public abstract class Poison
{
	public abstract int LabelNumber { get; }
	public abstract int RealLevel { get; }
	public abstract string Name { get; }
	public abstract int Level { get; }
	public abstract Timer ConstructTimer(Mobile m);

	public static List<Poison> Poisons { get; } = new();

	public static Poison Lesser => GetPoison("Lesser");
	public static Poison Regular => GetPoison("Regular");
	public static Poison Greater => GetPoison("Greater");
	public static Poison Deadly => GetPoison("Deadly");
	public static Poison Lethal => GetPoison("Lethal");
	public static Poison Parasitic => GetPoison("DeadlyParasitic");
	public static Poison DarkGlow => GetPoison("GreaterDarkglow");

	public override string ToString()
	{
		return Name;
	}

	public static void Register(Poison reg)
	{
		string regName = reg.Name.ToLower();

		for (int i = 0; i < Poisons.Count; i++)
		{
			if (reg.Level == Poisons[i].Level)
			{
				throw new Exception("A poison with that level already exists.");
			}

			if (regName == Poisons[i].Name.ToLower())
			{
				throw new Exception("A poison with that name already exists.");
			}
		}

		Poisons.Add(reg);
	}

	public static Poison Parse(string value)
	{
		if (int.TryParse(value, out int plevel))
			GetPoison(plevel);

		return GetPoison(value);
	}

	public static Poison GetPoison(int level)
	{
		return Poisons.FirstOrDefault(p => p.Level == level);
	}

	public static Poison GetPoison(string name)
	{
		return Poisons.FirstOrDefault(p => Utility.InsensitiveCompare(p.Name, name) == 0);
	}

	public static void Serialize(Poison p, GenericWriter writer)
	{
		if (p == null)
		{
			writer.Write((byte)0);
		}
		else
		{
			writer.Write((byte)1);
			writer.Write((byte)p.Level);
		}
	}

	public static Poison Deserialize(GenericReader reader)
	{
		return reader.ReadByte() switch
		{
			1 => GetPoison(reader.ReadByte()),
			_ => null,
		};
	}
}
