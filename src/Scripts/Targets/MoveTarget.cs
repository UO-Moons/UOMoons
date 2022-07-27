using Server.Commands;
using Server.Commands.Generic;
using Server.Targeting;

namespace Server.Targets;

public class MoveTarget : Target
{
	private readonly object m_Object;

	public MoveTarget(object o) : base(-1, true, TargetFlags.None)
	{
		m_Object = o;
	}

	protected override void OnTarget(Mobile from, object o)
	{
		if (o is not IPoint3D p)
			return;
		if (!BaseCommand.IsAccessible(from, m_Object))
		{
			from.SendMessage("That is not accessible.");
			return;
		}

		if (p is Item item)
			p = item.GetWorldTop();

		CommandLogging.WriteLine(from, "{0} {1} moving {2} to {3}", from.AccessLevel, CommandLogging.Format(from), CommandLogging.Format(m_Object), new Point3D(p));

		switch (m_Object)
		{
			case Item objectItem:
			{
				if (!objectItem.Deleted)
					objectItem.MoveToWorld(new Point3D(p), from.Map);
				break;
			}
			case Mobile {Deleted: false} m:
				m.MoveToWorld(new Point3D(p), from.Map);
				break;
		}
	}
}
