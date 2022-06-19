using Server.Engines.Quests;
using Server.Mobiles;
using System.Collections;
using System.Xml;

namespace Server.Regions;

public class ApprenticeRegion : BaseRegion
{
	public ApprenticeRegion(XmlElement xml, Map map, Region parent)
		: base(xml, map, parent)
	{
	}

	public Hashtable Table { get; } = new();
	public override void OnEnter(Mobile m)
	{
		base.OnEnter(m);

		if (m is not PlayerMobile player) return;
		for (var i = 0; i < player.Quests.Count; i++)
		{
			BaseQuest quest = player.Quests[i];

			for (var j = 0; j < quest.Objectives.Count; j++)
			{
				BaseObjective objective = quest.Objectives[j];

				if (objective is not ApprenticeObjective objective1 || objective.Completed) continue;

				if (!IsPartOf(objective1.Region)) continue;
				switch (objective1.Enter)
				{
					case int @int:
						player.SendLocalizedMessage(@int);
						break;
					case string @string:
						player.SendMessage(@string);
						break;
				}

				BuffInfo info = new(BuffIcon.ArcaneEmpowerment, 1078511, 1078512, objective1.Skill.ToString()); // Accelerated Skillgain Skill: ~1_val~
				BuffInfo.AddBuff(m, info);
				Table[m] = info;
			}
		}
	}

	public override void OnExit(Mobile m)
	{
		base.OnExit(m);

		if (m is not PlayerMobile player) return;
		for (var i = 0; i < player.Quests.Count; i++)
		{
			BaseQuest quest = player.Quests[i];

			for (var j = 0; j < quest.Objectives.Count; j++)
			{
				BaseObjective objective = quest.Objectives[j];

				if (objective is not ApprenticeObjective objective1 || objective.Completed) continue;

				if (!IsPartOf(objective1.Region)) continue;
				switch (objective1.Leave)
				{
					case int @int:
						player.SendLocalizedMessage(@int);
						break;
					case string @string:
						player.SendMessage(@string);
						break;
				}

				if (Table[m] is BuffInfo info)
					BuffInfo.RemoveBuff(m, info);
			}
		}
	}
}
