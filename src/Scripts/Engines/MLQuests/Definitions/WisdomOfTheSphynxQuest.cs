using Server.Items;

namespace Server.Engines.Quests;

public class WisdomOfTheSphynxQuest : BaseQuest
{
	public WisdomOfTheSphynxQuest()
	{
		AddObjective(new InternalObjective());

		AddReward(new BaseReward(1072805)); // The boon of Enigma.
	}

	public override bool ForceRemember => true;
	/* Wisdom of the Sphynx */
	public override object Title => 1072784;
	/* I greet thee human and divine my boon thou seek.  Convey hence the object of my riddle and I shall reward thee 
    with thy desire.<br><br>Three lives have I.<br>Gentle enough to soothe the skin,<br>Light enough to caress the sky,
    <br>Hard enough to crack rocks<br>What am I? */
	public override object Description => 1072822;
	/* As thou wish, human. */
	public override object Refuse => 1072823;
	/* I give thee a hint then human.  The answer to my riddle must be held carefully or it cannot be contained at all.  
    Bring this elusive item to me in a suitable container. */
	public override object Uncomplete => 1072824;
	/* Ah, thus it ends. */
	public override object Complete => 1074176;
	public override void GiveRewards()
	{
		base.GiveRewards();

		Owner.SendLocalizedMessage(1074945, null, 0x23); // You have gained the boon of Enigma!  You are wise enough to know how little you know.  You are one step closer to claiming your elven heritage.
	}

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

	private class InternalObjective : ObtainObjective
	{
		public InternalObjective()
			: base(typeof(Pitcher), "Answer To the Riddle", 1)
		{
		}

		public override bool IsObjective(Item item)
		{
			if (base.IsObjective(item))
			{
				Pitcher pitcher = (Pitcher)item;

				if (pitcher.Content == BeverageType.Water && !pitcher.IsEmpty)
					return true;
			}

			return false;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			_ = reader.ReadEncodedInt();
		}
	}
}