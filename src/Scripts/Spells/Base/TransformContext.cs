using System;
using System.Collections.Generic;

namespace Server.Spells;

public class TransformContext
{
	public Timer Timer { get; }
	public List<ResistanceMod> Mods { get; }
	public Type Type { get; }
	public ITransformationSpell Spell { get; }

	public TransformContext(Timer timer, List<ResistanceMod> mods, Type type, ITransformationSpell spell)
	{
		Timer = timer;
		Mods = mods;
		Type = type;
		Spell = spell;
	}
}
