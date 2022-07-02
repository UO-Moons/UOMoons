using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Server;

public enum Profession
{
	Advanced = 0,
	Warrior = 1,
	Mage = 2,
	Blacksmith = 3,
	Necromancer = 4,
	Paladin = 5,
	Samurai = 6,
	Ninja = 7
}

public class ProfessionInfo
{
	public static ProfessionInfo[] Professions { get; private set; }

	static ProfessionInfo()
	{
		Load();
	}

	public static void Configure()
	{
		Core.OnExpansionChanged += Load;
	}

	public static void Load()
	{
		var profs = new List<ProfessionInfo>
		{
			new()
			{
				Id = Profession.Advanced,
				Name = "Advanced",
				TopLevel = false,
				GumpId = 5571,
				Skills = new SkillNameValue[4],
				Stats = new StatNameValue[3]
			}
		};

		var skillsBuffer = new List<SkillNameValue>();
		var statsBuffer = new List<StatNameValue>();

		var file = Core.FindDataFile("Prof.txt");

		if (string.IsNullOrWhiteSpace(file) || !File.Exists(file))
		{
			file = Path.Combine(Core.BaseDirectory, "Data", "Prof.txt");
		}

		if (!string.IsNullOrWhiteSpace(file) && File.Exists(file))
		{
			using var s = File.OpenText(file);

			string[] cols;

			while (!s.EndOfStream)
			{
				var line = s.ReadLine();

				if (string.IsNullOrWhiteSpace(line))
				{
					continue;
				}

				line = line.Trim();

				if (!Insensitive.StartsWith(line, "Begin"))
				{
					continue;
				}

				var prof = new ProfessionInfo();

				int stats;
				int valid;
				var skills = stats = valid = 0;

				while (!s.EndOfStream)
				{
					line = s.ReadLine();

					if (string.IsNullOrWhiteSpace(line))
					{
						continue;
					}

					line = line.Trim();

					if (Insensitive.StartsWith(line, "End"))
					{
						if (valid >= 4)
						{
							profs.Add(prof);
						}

						break;
					}

					cols = line.Split(new[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);

					for (var i = 0; i < cols.Length; i++)
					{
						cols[i] = cols[i].Trim();
					}

					switch (cols[0].ToLower())
					{
						case "truename":
						{
							prof.Name = cols[1].Trim('"');

							++valid;
						}
							break;
						case "nameid":
							prof.NameId = Utility.ToInt32(cols[1]);
							break;
						case "descid":
							prof.DescId = Utility.ToInt32(cols[1]);
							break;
						case "desc":
						{
							if (Enum.TryParse(cols[1], out Profession id))
							{
								prof.Id = id;

								++valid;
							}
						}
							break;
						case "toplevel":
							prof.TopLevel = Utility.ToBoolean(cols[1]);
							break;
						case "gump":
							prof.GumpId = Utility.ToInt32(cols[1]);
							break;
						case "skill":
						{
							if (!Enum.TryParse(cols[1].Replace(" ", string.Empty), out SkillName skill))
							{
								var info = Insensitive.Equals(cols[1], "Evaluate Intelligence") ? SkillInfo.Table[(int)SkillName.EvalInt] : SkillInfo.Table.FirstOrDefault(o => Insensitive.Contains(o.Name, cols[1]) || Insensitive.Contains(cols[1], o.Name));

								if (info == null)
								{
									break;
								}

								skill = (SkillName)info.SkillID;
							}

							skillsBuffer.Add(new SkillNameValue(skill, Utility.ToInt32(cols[2])));

							if (++skills == 1)
							{
								++valid;
							}
						}
							break;
						case "stat":
						{
							if (!Enum.TryParse(cols[1], out StatType stat))
							{
								break;
							}

							statsBuffer.Add(new StatNameValue(stat, Utility.ToInt32(cols[2])));

							if (++stats == 1)
							{
								++valid;
							}
						}
							break;
					}
				}

				prof.Skills = skillsBuffer.ToArray();
				prof.Stats = statsBuffer.ToArray();

				skillsBuffer.Clear();
				statsBuffer.Clear();
			}
		}

		Professions = new ProfessionInfo[1 + profs.Max(p => (int)p.Id)];

		foreach (var p in profs)
		{
			Professions[(int)p.Id] = p;
		}

		for (var i = 0; i < Professions.Length; i++)
		{
			Professions[i] ??= new ProfessionInfo
			{
				Id = (Profession) i,
				Name = $"Undefined-{i}",
				TopLevel = true
			};
		}

		profs.Clear();
		profs.TrimExcess();
	}

	private ProfessionInfo()
	{
		Name = string.Empty;

		Skills = Array.Empty<SkillNameValue>();
		Stats = Array.Empty<StatNameValue>();
	}

	public Profession Id { get; private set; }

	public string Name { get; private set; }

	public int NameId { get; private set; }
	public int DescId { get; private set; }

	public bool TopLevel { get; private set; }

	public int GumpId { get; private set; }

	public SkillNameValue[] Skills { get; private set; }
	public StatNameValue[] Stats { get; private set; }
}
