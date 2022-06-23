using Server.Commands;
using Server.Commands.Generic;
using Server.Targeting;

namespace Server.Targets;

public class MoveTarget : Target
{
	private readonly object _mObject;

	public MoveTarget(object o) : base(-1, true, TargetFlags.None)
	{
		_mObject = o;
	}

	protected override void OnTarget(Mobile from, object o)
	{
		if (o is IPoint3D p)
		{
			if (!BaseCommand.IsAccessible(from, _mObject))
			{
				from.SendMessage("That is not accessible.");
				return;
			}

			if (p is Item item)
				p = item.GetWorldTop();

			CommandLogging.WriteLine(from, "{0} {1} moving {2} to {3}", from.AccessLevel, CommandLogging.Format(from), CommandLogging.Format(_mObject), new Point3D(p));

			switch (_mObject)
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
}
