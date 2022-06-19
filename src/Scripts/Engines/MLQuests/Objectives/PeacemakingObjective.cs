using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Engines.Quests;

public class PeacemakingObjective : SimpleObjective
{
	private static readonly Type MType = typeof(Mongbat);

	private readonly List<string> _mDescr = new();
	public override List<string> Descriptions => _mDescr;

	public PeacemakingObjective()
		: base(5, -1)
	{
		_mDescr.Add("Calm five mongbats.");
	}

	public override bool Update(object obj)
	{
		if (obj is Mobile mobile && mobile.GetType() == MType)
		{
			CurProgress++;

			if (Completed)
				Quest.OnCompleted();
			else
			{
				Quest.Owner.SendLocalizedMessage(1115747, true, (MaxProgress - CurProgress).ToString()); // Creatures remaining to be calmed:   ~1_val~.
				Quest.Owner.PlaySound(Quest.UpdateSound);
			}

			return true;
		}

		return false;
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
