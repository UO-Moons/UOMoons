using Server.Items;
using Server.Misc;
using System.Collections;

namespace Server.Mobiles
{
	public class XmlSpawnerSkillCheck
	{
		// alternate skillcheck hooks to replace those in SkillCheck.cs
		public static bool Mobile_SkillCheckLocation(Mobile from, SkillName skillName, double minSkill, double maxSkill)
		{
			Skill skill = from.Skills[skillName];

			if (skill == null)
				return false;

			// call the default skillcheck handler
			bool success = SkillCheck.Mobile_SkillCheckLocation(from, skillName, minSkill, maxSkill);

			// call the xmlspawner skillcheck handler
			CheckSkillUse(from, skill, success);

			return success;
		}

		public static bool Mobile_SkillCheckDirectLocation(Mobile from, SkillName skillName, double chance)
		{
			Skill skill = from.Skills[skillName];

			if (skill == null)
				return false;

			// call the default skillcheck handler
			bool success = SkillCheck.Mobile_SkillCheckDirectLocation(from, skillName, chance);

			// call the xmlspawner skillcheck handler
			CheckSkillUse(from, skill, success);

			return success;
		}

		public static bool Mobile_SkillCheckTarget(Mobile from, SkillName skillName, object target, double minSkill, double maxSkill)
		{
			Skill skill = from.Skills[skillName];

			if (skill == null)
				return false;

			// call the default skillcheck handler
			bool success = SkillCheck.Mobile_SkillCheckTarget(from, skillName, target, minSkill, maxSkill);

			// call the xmlspawner skillcheck handler
			CheckSkillUse(from, skill, success);

			return success;
		}

		public static bool Mobile_SkillCheckDirectTarget(Mobile from, SkillName skillName, object target, double chance)
		{
			Skill skill = from.Skills[skillName];

			if (skill == null)
				return false;

			// call the default skillcheck handler
			bool success = SkillCheck.Mobile_SkillCheckDirectTarget(from, skillName, target, chance);

			// call the xmlspawner skillcheck handler
			CheckSkillUse(from, skill, success);

			return success;
		}


		public class RegisteredSkill
		{
			public const int MaxSkills = 58;//52 old
			public const SkillName Invalid = (SkillName)(-1);

			public object target;
			public SkillName sid;

			// note the extra skill MaxSkills +1 is used for any unknown skill that falls outside of the known 52
			private static readonly ArrayList[] m_FeluccaSkillList = new ArrayList[MaxSkills + 1];
			private static readonly ArrayList[] m_TrammelSkillList = new ArrayList[MaxSkills + 1];
			private static readonly ArrayList[] m_MalasSkillList = new ArrayList[MaxSkills + 1];
			private static readonly ArrayList[] m_IlshenarSkillList = new ArrayList[MaxSkills + 1];
			private static readonly ArrayList[] m_TokunoSkillList = new ArrayList[MaxSkills + 1];

			// primary function that returns the list of objects (spawners) that are associated with a given skillname by map
			public static ArrayList TriggerList(SkillName index, Map map)
			{
				if (map == null || map == Map.Internal) return null;

				ArrayList[] maplist;

				// get the list for the specified map

				if (map == Map.Felucca)
					maplist = m_FeluccaSkillList;
				else
				if (map == Map.Ilshenar)
					maplist = m_IlshenarSkillList;
				else
				if (map == Map.Malas)
					maplist = m_MalasSkillList;
				else
				if (map == Map.Trammel)
					maplist = m_TrammelSkillList;
				else
				if (map == Map.Tokuno)
					maplist = m_TokunoSkillList;
				else
					return null;

				// is it one of the standard 52 skills
				if ((int)index >= 0 && (int)index < MaxSkills)
				{
					if (maplist[(int)index] == null)
						maplist[(int)index] = new ArrayList();

					return maplist[(int)index];
				}
				else
				// otherwise pull it out of the final slot for unknown skills.  I dont know of a condition that would lead to 
				// additional skills being registered but it will support them if they are
				{
					if (maplist[MaxSkills] == null)
						maplist[MaxSkills] = new ArrayList();

					return maplist[MaxSkills];
				}

			}
		}

		public static void RegisterSkillTrigger(object o, SkillName s, Map map)
		{
			if (o == null || s == RegisteredSkill.Invalid) return;

			// go through the list and if the spawner is not on it yet, then add it
			bool found = false;

			ArrayList skilllist = RegisteredSkill.TriggerList(s, map);

			if (skilllist == null) return;

			foreach (RegisteredSkill rs in skilllist)
			{
				if (rs.target == o && rs.sid == s)
				{
					found = true;
					// dont register a skill if it is already on the list for this spawner
					break;
				}
			}

			// if it hasnt already been added to the list, then add it
			if (!found)
			{
				RegisteredSkill newrs = new()
				{
					target = o,
					sid = s
				};

				skilllist.Add(newrs);

			}
		}

		public static void UnRegisterSkillTrigger(object o, SkillName s, Map map, bool all)
		{
			if (o == null || s == RegisteredSkill.Invalid) return;

			// go through the list and if the spawner is on it regardless of the skill registered, then remove it
			if (all)
			{
				for (int i = 0; i < RegisteredSkill.MaxSkills + 1; i++)
				{
					ArrayList skilllist = RegisteredSkill.TriggerList((SkillName)i, map);

					if (skilllist == null) return;

					foreach (RegisteredSkill rs in skilllist)
					{
						if (rs.target == o)
						{
							skilllist.Remove(rs);
							break;
						}

					}
				}
			}
			else
			{
				ArrayList skilllist = RegisteredSkill.TriggerList(s, map);

				if (skilllist == null) return;

				// if the all flag is not set then just remove the spawner from the list for the specified skill
				foreach (RegisteredSkill rs in skilllist)
				{
					if ((rs.target == o) && (rs.sid == s))
					{
						skilllist.Remove(rs);
						break;
					}

				}
			}
		}

		// determines whether  XmlSpawner, XmlAttachment, or XmlQuest OnSkillUse methods should be invoked.
		public static void CheckSkillUse(Mobile m, Skill skill, bool success)
		{
			if (m is not PlayerMobile || skill == null)
				return;

			// then check for registered skills
			ArrayList skilllist = RegisteredSkill.TriggerList(skill.SkillName, m.Map);

			if (skilllist == null) return;

			// determine whether there are any registered objects for this skill
			foreach (RegisteredSkill rs in skilllist)
			{
				if (rs.sid == skill.SkillName)
				{
					// if so then invoke their skill handlers
					if (rs.target is XmlSpawner spawner)
					{
						if (spawner.HandlesOnSkillUse)
						{
							// call the spawner handler
							spawner.OnSkillUse(m, skill, success);
						}
					}
					else
					if (rs.target is IXmlQuest quest)
					{
						if (quest.HandlesOnSkillUse)
						{
							// call the xmlquest handler
							quest.OnSkillUse(m, skill, success);
						}
					}
				}
			}
		}

	}
}
