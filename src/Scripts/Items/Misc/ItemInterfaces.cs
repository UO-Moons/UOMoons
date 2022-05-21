using Server.Mobiles;

namespace Server.Items
{
	public interface IConditionalVisibility
	{
		bool CanBeSeenBy(PlayerMobile m);
	}
}
