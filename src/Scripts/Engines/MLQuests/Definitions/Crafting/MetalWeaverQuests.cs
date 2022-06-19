using Server.Items;

namespace Server.Engines.Quests;

public sealed class InstrumentOfWarQuest : BaseQuest
{
	public InstrumentOfWarQuest()
	{
		AddObjective(new ObtainObjective(typeof(Broadsword), "Broadswords", 12, 0xF5E));

		AddReward(new BaseReward(typeof(SmithsCraftsmanSatchel), 1074282));
	}

	/* Instrument of War */
	public override object Title => 1074055;
	/* Pathetic, this human craftsmanship! Take their broadswords - overgrown butter knives, in reality. 
    No, I cannot do them justice - you must see for yourself. Bring me broadswords and I will demonstrate 
    their feebleness. */
	public override object Description => 1074149;
	/* Fine then, I'm shall find another to run my errands then. */
	public override object Refuse => 1074063;
	/* Hurry up! I don't have all day to wait for you to bring what I desire! */
	public override object Uncomplete => 1074064;
	/* These human made goods are laughable! It offends so -- I must show you what elven skill is capable of! */
	public override object Complete => 1074065;
}

public sealed class TheShieldQuest : BaseQuest
{
	public TheShieldQuest()
	{
		AddObjective(new ObtainObjective(typeof(HeaterShield), "Heater Shields", 10, 0x1B76));

		AddReward(new BaseReward(typeof(SmithsCraftsmanSatchel), 1074282));
	}

	/* The Shield */
	public override object Title => 1074054;
	/* I doubt very much a human shield would stop a good stout elven arrow. You doubt me? I will show you - 
    get me some of these heater shields and I will piece them with sharp elven arrows! */
	public override object Description => 1074148;
	/* Fine then, I'm shall find another to run my errands then. */
	public override object Refuse => 1074063;
	/* Hurry up! I don't have all day to wait for you to bring what I desire! */
	public override object Uncomplete => 1074064;
	/* These human made goods are laughable! It offends so -- I must show you what elven skill is capable of! */
	public override object Complete => 1074065;
}

public sealed class MusicToMyEarsQuest : BaseQuest
{
	public MusicToMyEarsQuest()
	{
		AddObjective(new ObtainObjective(typeof(LapHarp), "Lap Harp", 10, 0xEB2));

		AddReward(new BaseReward(typeof(CarpentersCraftsmanSatchel), 1074282));
	}

	/* Music to my Ears */
	public override object Title => 1074023;
	/* You think you know something of music? Laughable! Take your lap harp. Crude, indelicate instruments that 
    make a noise not unlike the wailing of a choleric child or a dying cat. I will show you - bring lap harps, 
    and I will demonstrate. */
	public override object Description => 1074117;
	/* Fine then, I'm shall find another to run my errands then. */
	public override object Refuse => 1074063;
	/* Hurry up! I don't have all day to wait for you to bring what I desire! */
	public override object Uncomplete => 1074064;
	/* These human made goods are laughable! It offends so -- I must show you what elven skill is capable of! */
	public override object Complete => 1074065;
}

public sealed class TheGlassEyeQuest : BaseQuest
{
	public TheGlassEyeQuest()
	{
		AddObjective(new ObtainObjective(typeof(Spyglass), "Spyglasses", 10, 0x14F5));

		AddReward(new BaseReward(typeof(TinkersCraftsmanSatchel), 1074282));
	}

	/* The Glass Eye */
	public override object Title => 1074050;
	/* Humans are so pathetically weak, they must be augmented by glass and metal! Imagine such a thing! 
    I must see one of these spyglasses for myself, to understand the pathetic limits of human sight! */
	public override object Description => 1074144;
	/* Fine then, I'm shall find another to run my errands then. */
	public override object Refuse => 1074063;
	/* Hurry up! I don't have all day to wait for you to bring what I desire! */
	public override object Uncomplete => 1074064;
	/* These human made goods are laughable! It offends so -- I must show you what elven skill is capable of! */
	public override object Complete => 1074065;
}

public sealed class LazyHumansQuest : BaseQuest
{
	public LazyHumansQuest()
	{
		AddObjective(new ObtainObjective(typeof(FootStool), "Foot Stools", 10, 0xB5E));

		AddReward(new BaseReward(typeof(CarpentersCraftsmanSatchel), 1074282));
	}

	/* Lazy Humans */
	public override object Title => 1074024;
	/* Human fancy knows no bounds!  It's pathetic that they are so weak that they must create a special stool 
    upon which to rest their feet when they recline!  Humans don't have any clue how to live.  Bring me some of 
    these foot stools to examine and I may teach you something worthwhile. */
	public override object Description => 1074118;
	/* Fine then, I'm shall find another to run my errands then. */
	public override object Refuse => 1074063;
	/* Hurry up! I don't have all day to wait for you to bring what I desire! */
	public override object Uncomplete => 1074064;
	/* These human made goods are laughable! It offends so -- I must show you what elven skill is capable of! */
	public override object Complete => 1074065;
}

public sealed class InventiveToolsQuest : BaseQuest
{
	public InventiveToolsQuest()
	{
		AddObjective(new ObtainObjective(typeof(TinkerTools), "Tinker's Tools", 10, 0x1EB8));

		AddReward(new BaseReward(typeof(TinkersCraftsmanSatchel), 1074282));
	}

	/* Inventive Tools */
	public override object Title => 1074048;
	/* Bring me some of these tinker's tools! I am certain, in the hands of an elf, they will fashion objects of 
    ingenuity and delight that will shame all human invention! Hurry, do this quickly and I might deign to show you 
    my skill.  */
	public override object Description => 1074142;
	/* Fine then, I'm shall find another to run my errands then. */
	public override object Refuse => 1074063;
	/* Hurry up! I don't have all day to wait for you to bring what I desire! */
	public override object Uncomplete => 1074064;
	/* These human made goods are laughable! It offends so -- I must show you what elven skill is capable of! */
	public override object Complete => 1074065;
}
