using System;

namespace Server.Spells;

public class SpellInfo
{
	public int Action { get; }
	public bool AllowTown { get; }
	public int[] Amounts { get; }
	public string Mantra { get; }
	public string Name { get; }
	public Type[] Reagents { get; }
	public int LeftHandEffect { get; set; }
	public int RightHandEffect { get; set; }

	public SpellInfo(string name, string mantra, params Type[] regs) : this(name, mantra, 16, 0, 0, true, regs)
	{
	}

	public SpellInfo(string name, string mantra, bool allowTown, params Type[] regs) : this(name, mantra, 16, 0, 0, allowTown, regs)
	{
	}

	public SpellInfo(string name, string mantra, int action, params Type[] regs) : this(name, mantra, action, 0, 0, true, regs)
	{
	}

	public SpellInfo(string name, string mantra, int action, bool allowTown, params Type[] regs) : this(name, mantra, action, 0, 0, allowTown, regs)
	{
	}

	public SpellInfo(string name, string mantra, int action, int handEffect, params Type[] regs) : this(name, mantra, action, handEffect, handEffect, true, regs)
	{
	}

	public SpellInfo(string name, string mantra, int action, int handEffect, bool allowTown, params Type[] regs) : this(name, mantra, action, handEffect, handEffect, allowTown, regs)
	{
	}

	private SpellInfo(string name, string mantra, int action, int leftHandEffect, int rightHandEffect, bool allowTown, params Type[] regs)
	{
		Name = name;
		Mantra = mantra;
		Action = action;
		Reagents = regs;
		AllowTown = allowTown;

		LeftHandEffect = leftHandEffect;
		RightHandEffect = rightHandEffect;

		Amounts = new int[regs.Length];

		for (var i = 0; i < regs.Length; ++i)
			Amounts[i] = 1;
	}
}
