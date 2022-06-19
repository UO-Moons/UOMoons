using Server.Engines.Harvest;

namespace Server;

public interface IHarvestTool : IEntity
{
	HarvestSystem HarvestSystem { get; }
}
