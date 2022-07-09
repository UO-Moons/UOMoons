using System;
using System.Collections.Generic;

namespace Server.Spells;

public class SpellRegistry
{
	private static readonly Type[] m_Types = new Type[700];
	private static int _count;

	public static Type[] Types
	{
		get
		{
			_count = -1;
			return m_Types;
		}
	}

	//What IS this used for anyways.
	public static int Count
	{
		get
		{
			if (_count == -1)
			{
				_count = 0;

				for (int i = 0; i < m_Types.Length; ++i)
					if (m_Types[i] != null)
						++_count;
			}

			return _count;
		}
	}

	private static readonly Dictionary<Type, int> m_IDsFromTypes = new(m_Types.Length);

	public static Dictionary<int, SpecialMove> SpecialMoves { get; } = new();

	public static int GetRegistryNumber(ISpell s)
	{
		return GetRegistryNumber(s.GetType());
	}

	public static int GetRegistryNumber(SpecialMove s)
	{
		return GetRegistryNumber(s.GetType());
	}

	public static int GetRegistryNumber(Type type)
	{
		if (m_IDsFromTypes.ContainsKey(type))
			return m_IDsFromTypes[type];

		return -1;
	}

	public static void Register(int spellId, Type type)
	{
		if (spellId < 0 || spellId >= m_Types.Length)
			return;

		if (m_Types[spellId] == null)
			++_count;

		m_Types[spellId] = type;

		if (!m_IDsFromTypes.ContainsKey(type))
			m_IDsFromTypes.Add(type, spellId);

		if (type.IsSubclassOf(typeof(SpecialMove)))
		{
			SpecialMove spm = null;

			try
			{
				spm = Activator.CreateInstance(type) as SpecialMove;
			}
			catch
			{
				// ignored
			}

			if (spm != null)
				SpecialMoves.Add(spellId, spm);
		}
	}

	public static SpecialMove GetSpecialMove(int spellId)
	{
		if (spellId < 0 || spellId >= m_Types.Length)
			return null;

		Type t = m_Types[spellId];

		if (t == null || !t.IsSubclassOf(typeof(SpecialMove)) || !SpecialMoves.ContainsKey(spellId))
			return null;

		return SpecialMoves[spellId];
	}

	private static readonly object[] m_Params = new object[2];

	public static Spell NewSpell(int spellId, Mobile caster, Item scroll)
	{
		if (spellId < 0 || spellId >= m_Types.Length)
			return null;

		Type t = m_Types[spellId];

		if (t != null && !t.IsSubclassOf(typeof(SpecialMove)))
		{
			m_Params[0] = caster;
			m_Params[1] = scroll;

			try
			{
				return (Spell)Activator.CreateInstance(t, m_Params);
			}
			catch
			{
				// ignored
			}
		}

		return null;
	}

	private static readonly string[] m_CircleNames = {
		"First",
		"Second",
		"Third",
		"Fourth",
		"Fifth",
		"Sixth",
		"Seventh",
		"Eighth",
		"Necromancy",
		"Chivalry",
		"Bushido",
		"Ninjitsu",
		"Spellweaving"
	};

	public static Spell NewSpell(string name, Mobile caster, Item scroll)
	{
		for (int i = 0; i < m_CircleNames.Length; ++i)
		{
			Type t = Assembler.FindTypeByFullName($"Server.Spells.{m_CircleNames[i]}.{name}");

			if (t != null && !t.IsSubclassOf(typeof(SpecialMove)))
			{
				m_Params[0] = caster;
				m_Params[1] = scroll;

				try
				{
					return (Spell)Activator.CreateInstance(t, m_Params);
				}
				catch
				{
					// ignored
				}
			}
		}

		return null;
	}
}
