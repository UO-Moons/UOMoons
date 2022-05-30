using Server.Engines.Craft;

namespace Server.Items
{
	public interface IQuality : ICraftable
	{
		ItemQuality Quality { get; set; }
		bool PlayerConstructed { get; }
	}
}
