using Server.Items;

namespace Server.Engines.Quests;

public class FreedomQuest : BaseQuest
{
	public override bool DoneOnce => true;

	public FreedomQuest()
	{
		AddObjective(new EscortObjective("Sanctuary Entrance"));

		AddReward(new BaseReward(typeof(StolenRing), "Lenley's Favorite Sparkly"));
	}

	/* Freedom! */
	public override object Title => 1072367;

	/*
     * Lenley isn't seen.  Why you see me? Lenley is sneaking.  Lenley runs away.
     * You help Lenley to not get dead?  We go out past pig-men orcs?  Yes? Yes? You say yes?
    */
	public override object Description => 1072552;

	/* You no like Lenley? No hurt Lenley!  No see Lenley.  Go 'way. */
	public override object Refuse => 1072553;

	/* Lenley not run away yet.  Go, go, Lenley not past pig-men orcs.  You go, Lenley go after you.  Go! */
	public override object Uncomplete => 1072554;

	/* Lenley so happy!  Lenley not get dead.  You have best Lenley shiny! */
	public override object Complete => 1072556;

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		_ = reader.ReadInt();
	}
}