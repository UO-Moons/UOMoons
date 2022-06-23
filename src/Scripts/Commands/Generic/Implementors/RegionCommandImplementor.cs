using System;
using System.Collections;
using System.Linq;

namespace Server.Commands.Generic
{
	public class RegionCommandImplementor : BaseCommandImplementor
	{
		public RegionCommandImplementor()
		{
			Accessors = new[] { "Region" };
			SupportRequirement = CommandSupport.Region;
			SupportsConditionals = true;
			AccessLevel = AccessLevel.GameMaster;
			Usage = "Region <command> [condition]";
			Description = "Invokes the command on all appropriate mobiles in your current region. Optional condition arguments can further restrict the set of objects.";
		}

		public override void Compile(Mobile from, BaseCommand command, ref string[] args, ref object obj)
		{
			try
			{
				Extensions ext = Extensions.Parse(from, ref args);


				if (!CheckObjectTypes(from, command, ext, out bool items, out bool mobiles))
					return;

				Region reg = from.Region;

				ArrayList list = new();

				if (mobiles)
				{
					foreach (var mob in reg.GetMobiles().Where(mob => BaseCommand.IsAccessible(from, mob)).Where(mob => ext.IsValid(mob)))
					{
						list.Add(mob);
					}
				}
				else
				{
					command.LogFailure("This command does not support items.");
					return;
				}

				ext.Filter(list);

				obj = list;
			}
			catch (Exception ex)
			{
				from.SendMessage(ex.Message);
			}
		}
	}
}
