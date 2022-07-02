using System;
using System.Collections.Generic;

namespace Server;

[Parsable]
public abstract class Race
{
	public static Race DefaultRace => Races[0];

	public static Race[] Races { get; } = new Race[0x100];

	public static Race Human => Races[0];
	public static Race Elf => Races[1];
	public static Race Gargoyle => Races[2];

	public static List<Race> AllRaces { get; } = new();

	private static string[] _raceNames;
	private static Race[] _raceValues;

	public static string[] GetRaceNames()
	{
		CheckNamesAndValues();
		return _raceNames;
	}

	public static Race[] GetRaceValues()
	{
		CheckNamesAndValues();
		return _raceValues;
	}

	public static Race Parse(string value)
	{
		CheckNamesAndValues();

		for (int i = 0; i < _raceNames.Length; ++i)
		{
			if (Insensitive.Equals(_raceNames[i], value))
				return _raceValues[i];
		}

		if (int.TryParse(value, out int index))
		{
			if (index >= 0 && index < Races.Length && Races[index] != null)
				return Races[index];
		}

		throw new ArgumentException("Invalid race name");
	}

	private static void CheckNamesAndValues()
	{
		if (_raceNames != null && _raceNames.Length == AllRaces.Count)
			return;

		_raceNames = new string[AllRaces.Count];
		_raceValues = new Race[AllRaces.Count];

		for (int i = 0; i < AllRaces.Count; ++i)
		{
			Race race = AllRaces[i];

			_raceNames[i] = race.Name;
			_raceValues[i] = race;
		}
	}

	public override string ToString()
	{
		return Name;
	}

	public Expansion RequiredExpansion { get; }

	public int MaleBody { get; }
	public int MaleGhostBody { get; }

	public int FemaleBody { get; }
	public int FemaleGhostBody { get; }

	protected Race(int raceId, int raceIndex, string name, string pluralName, int maleBody, int femaleBody, int maleGhostBody, int femaleGhostBody, Expansion requiredExpansion)
	{
		RaceId = raceId;
		RaceIndex = raceIndex;

		Name = name;

		MaleBody = maleBody;
		FemaleBody = femaleBody;
		MaleGhostBody = maleGhostBody;
		FemaleGhostBody = femaleGhostBody;

		RequiredExpansion = requiredExpansion;
		PluralName = pluralName;
	}

	public virtual bool ValidateHair(Mobile m, int itemId) { return ValidateHair(m.Female, itemId); }
	public abstract bool ValidateHair(bool female, int itemId);

	public virtual int RandomHair(Mobile m) { return RandomHair(m.Female); }
	public abstract int RandomHair(bool female);

	public virtual bool ValidateFacialHair(Mobile m, int itemId) { return ValidateFacialHair(m.Female, itemId); }
	public abstract bool ValidateFacialHair(bool female, int itemId);

	public virtual int RandomFacialHair(Mobile m) { return RandomFacialHair(m.Female); }
	public abstract int RandomFacialHair(bool female);  //For the *ahem* bearded ladies

	public abstract bool ValidateFace(bool female, int itemId);

	public virtual int RandomFace(Mobile m)
	{
		return RandomFace(m.Female);
	}
	public abstract int RandomFace(bool female);

	public abstract int ClipSkinHue(int hue);
	public abstract int RandomSkinHue();

	public abstract int ClipHairHue(int hue);
	public abstract int RandomHairHue();

	public abstract int ClipFaceHue(int hue);
	public abstract int RandomFaceHue();

	public abstract bool ValidateEquipment(Item item);

	public virtual int Body(Mobile m)
	{
		if (m.Alive)
			return AliveBody(m.Female);

		return GhostBody(m.Female);
	}

	public virtual int AliveBody(Mobile m) { return AliveBody(m.Female); }
	public virtual int AliveBody(bool female)
	{
		return female ? FemaleBody : MaleBody;
	}

	public virtual int GhostBody(Mobile m) { return GhostBody(m.Female); }
	public virtual int GhostBody(bool female)
	{
		return (female ? FemaleGhostBody : MaleGhostBody);
	}

	public int RaceId { get; }

	public int RaceIndex { get; }

	public string Name { get; set; }

	public string PluralName { get; set; }
}
