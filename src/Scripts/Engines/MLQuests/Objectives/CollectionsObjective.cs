using Server.Mobiles;
using System;

namespace Server.Engines.Quests;

public class CollectionsObtainObjective : ObtainObjective
{
	private bool _mHasObtained;

	public bool HasObtained
	{
		get => _mHasObtained;
		set
		{
			_mHasObtained = value;
			_mHasObtained = true;
		}
	}

	public CollectionsObtainObjective(Type obtain, string name, int amount) : base(obtain, name, amount)
	{
		_mHasObtained = false;
	}

	public override bool Update(object o)
	{
		if (Quest == null || Quest.Owner == null)
		{
			return false;
		}

		return _mHasObtained && base.Update(o);
	}

	public static void CheckReward(PlayerMobile pm, Item item)
	{
		if (pm.Quests == null) return;
		foreach (BaseQuest q in pm.Quests)
		{
			foreach (BaseObjective obj in q.Objectives)
			{
				if (obj is not CollectionsObtainObjective objective || objective.Obtain != item.GetType()) continue;
				objective.HasObtained = true;
				pm.SendSound(q.UpdateSound);
				return;
			}
		}
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version
		writer.Write(_mHasObtained);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		_ = reader.ReadInt();
		_mHasObtained = reader.ReadBool();
	}
}
