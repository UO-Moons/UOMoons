using Server.Items;
using Server.Mobiles;

namespace Server.Regions;

public class ExodusChestRegion : BaseRegion
{
	private ExodusChest ExodusChest { get; }

	public ExodusChestRegion(ExodusChest chest)
		: base(null, chest.Map, Find(chest.Location, chest.Map), new Rectangle2D(chest.Location.X - 2, chest.Location.Y - 2, 5, 5))
	{
		ExodusChest = chest;
	}

	public override void OnEnter(Mobile m)
	{
		if (!ExodusChest.Visible && m is PlayerMobile && m.Skills[SkillName.DetectHidden].Value >= 98.0)
		{
			m.SendLocalizedMessage(1153493); // Your keen senses detect something hidden in the area...
		}
	}
}
