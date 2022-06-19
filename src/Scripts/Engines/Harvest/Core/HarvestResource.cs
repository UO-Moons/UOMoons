using System;

namespace Server.Engines.Harvest;

public class HarvestResource
{
	public HarvestResource(double reqSkill, double minSkill, double maxSkill, object message, params Type[] types)
	{
		ReqSkill = reqSkill;
		MinSkill = minSkill;
		MaxSkill = maxSkill;
		Types = types;
		SuccessMessage = message;
	}

	public Type[] Types { get; set; }
	public double ReqSkill { get; set; }
	public double MinSkill { get; set; }
	public double MaxSkill { get; set; }
	public object SuccessMessage { get; }

	public void SendSuccessTo(Mobile m)
	{
		switch (SuccessMessage)
		{
			case int message:
				m.SendLocalizedMessage(message);
				break;
			case string s:
				m.SendMessage(s);
				break;
		}
	}
}
