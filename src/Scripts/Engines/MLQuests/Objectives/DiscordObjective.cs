using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Engines.Quests;

public class DiscordObjective : SimpleObjective
{
	private static readonly Type MType = typeof(Goat);

	private readonly List<string> _mDescr = new();
	public override List<string> Descriptions => _mDescr;

	public DiscordObjective()
		: base(5, -1)
	{
		_mDescr.Add("Discord five goats.");
	}

	public override bool Update(object obj)
	{
		if (obj is not Mobile mobile || mobile.GetType() != MType) return false;
		CurProgress++;

		if (Completed)
			Quest.OnCompleted();
		else
		{
			Quest.Owner.SendLocalizedMessage(1115749, true, (MaxProgress - CurProgress).ToString()); // Creatures remaining to be discorded: 
			Quest.Owner.PlaySound(Quest.UpdateSound);
		}

		return true;

	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadInt();
	}
}
