using System;
using System.Collections;
using System.Linq;

namespace Server.Commands.Generic;

public class GlobalCommandImplementor : BaseCommandImplementor
{
	public GlobalCommandImplementor()
	{
		Accessors = new string[] { "Global" };
		SupportRequirement = CommandSupport.Global;
		SupportsConditionals = true;
		AccessLevel = AccessLevel.Administrator;
		Usage = "Global <command> [condition]";
		Description = "Invokes the command on all appropriate objects in the world. Optional condition arguments can further restrict the set of objects.";
	}

	public override void Compile(Mobile from, BaseCommand command, ref string[] args, ref object obj)
	{
		try
		{
			Extensions ext = Extensions.Parse(from, ref args);


			if (!CheckObjectTypes(from, command, ext, out bool items, out bool mobiles))
				return;

			ArrayList list = new();

			if (items)
			{
				foreach (var item in World.Items.Values.Where(item => ext.IsValid(item)))
				{
					list.Add(item);
				}
			}

			if (mobiles)
			{
				foreach (var mob in World.Mobiles.Values.Where(mob => ext.IsValid(mob)))
				{
					list.Add(mob);
				}
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
