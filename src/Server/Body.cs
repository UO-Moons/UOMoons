using System;
using System.IO;

namespace Server;

public enum BodyType : byte
{
	Empty,
	Monster,
	Sea,
	Animal,
	Human,
	Equipment
}

public struct Body
{
	private static readonly BodyType[] MTypes;

	static Body()
	{
		if (File.Exists("Data/bodyTable.cfg"))
		{
			using StreamReader ip = new("Data/bodyTable.cfg");
			MTypes = new BodyType[0x1000];

			while (ip.ReadLine() is { } line)
			{
				if (line.Length == 0 || line.StartsWith("#"))
					continue;

				string[] split = line.Split('\t');


				if (int.TryParse(split[0], out int bodyId) && Enum.TryParse(split[1], true, out BodyType type) && bodyId >= 0 && bodyId < MTypes.Length)
				{
					MTypes[bodyId] = type;
				}
				else
				{
					Console.WriteLine("Warning: Invalid bodyTable entry:");
					Console.WriteLine(line);
				}
			}
		}
		else
		{
			Console.WriteLine("Warning: Data/bodyTable.cfg does not exist");

			MTypes = Array.Empty<BodyType>();
		}
	}

	public Body(int bodyId)
	{
		BodyId = bodyId;
	}

	public BodyType Type
	{
		get
		{
			if (BodyId >= 0 && BodyId < MTypes.Length)
				return MTypes[BodyId];
			return BodyType.Empty;
		}
	}

	public bool IsHuman => BodyId >= 0
	                       && BodyId < MTypes.Length
	                       && MTypes[BodyId] == BodyType.Human
	                       && BodyId != 402
	                       && BodyId != 403
	                       && BodyId != 607
	                       && BodyId != 608
	                       && BodyId != 694
	                       && BodyId != 695
	                       && BodyId != 970;

	public bool IsGargoyle => BodyId is 666 or 667 or 694 or 695;

	public bool IsMale => BodyId is 183 or 185 or 400 or 402 or 605 or 607 or 666 or 694 or 750;

	public bool IsFemale => BodyId is 184 or 186 or 401 or 403 or 606 or 608 or 667 or 695 or 751;

	public bool IsGhost => BodyId is 402 or 403 or 607 or 608 or 694 or 695 or 970;

	public bool IsMonster => BodyId >= 0
	                         && BodyId < MTypes.Length
	                         && MTypes[BodyId] == BodyType.Monster;

	public bool IsAnimal => BodyId >= 0
	                        && BodyId < MTypes.Length
	                        && MTypes[BodyId] == BodyType.Animal;

	public bool IsEmpty => BodyId >= 0
	                       && BodyId < MTypes.Length
	                       && MTypes[BodyId] == BodyType.Empty;

	public bool IsSea => BodyId >= 0
	                     && BodyId < MTypes.Length
	                     && MTypes[BodyId] == BodyType.Sea;

	public bool IsEquipment => BodyId >= 0
	                           && BodyId < MTypes.Length
	                           && MTypes[BodyId] == BodyType.Equipment;

	public int BodyId { get; }

	public static implicit operator int(Body a)
	{
		return a.BodyId;
	}

	public static implicit operator Body(int a)
	{
		return new Body(a);
	}

	public override string ToString()
	{
		return $"0x{BodyId:X}";
	}

	public override int GetHashCode()
	{
		return BodyId;
	}

	public override bool Equals(object o)
	{
		if (o is not Body body) return false;

		return body.BodyId == BodyId;
	}

	public static bool operator ==(Body l, Body r)
	{
		return l.BodyId == r.BodyId;
	}

	public static bool operator !=(Body l, Body r)
	{
		return l.BodyId != r.BodyId;
	}

	public static bool operator >(Body l, Body r)
	{
		return l.BodyId > r.BodyId;
	}

	public static bool operator >=(Body l, Body r)
	{
		return l.BodyId >= r.BodyId;
	}

	public static bool operator <(Body l, Body r)
	{
		return l.BodyId < r.BodyId;
	}

	public static bool operator <=(Body l, Body r)
	{
		return l.BodyId <= r.BodyId;
	}
}
