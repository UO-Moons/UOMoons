using Server.Items;

namespace Server.Engines.Quests;

public sealed class NecessitysMotherQuest : BaseQuest
{
	public NecessitysMotherQuest()
	{
		AddObjective(new ObtainObjective(typeof(TinkerTools), "Tinker's Tools", 10, 0x1EB8));

		AddReward(new BaseReward(typeof(TinkersCraftsmanSatchel), 1074282));
	}

	/* Necessity's Mother */
	public override object Title => 1073906;
	/* What a thing, this human need to tinker. It seems there is no end to what might be produced with a set of 
    Tinker's Tools. Who knows what an elf might build with some? Could you obtain some tinker's tools and bring 
    them to me? In exchange, I offer you elven lore and knowledge.  */
	public override object Description => 1074096;
	/* I will patiently await your reconsideration. */
	public override object Refuse => 1073921;
	/* I will be in your debt if you bring me tinker's tools. */
	public override object Uncomplete => 1073952;
	/* Now, I shall see what an elf can invent! */
	public override object Complete => 1073977;
}
