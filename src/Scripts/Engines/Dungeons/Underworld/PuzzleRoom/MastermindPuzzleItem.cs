using System;

namespace Server.Items;

public sealed class MastermindPuzzleItem : PuzzleChest
{
	private Timer m_Timer;
	private int m_Lifespan;

	[CommandProperty(AccessLevel.GameMaster)]
	private MagicKey Key { get; set; }

	public override int LabelNumber => 1113379;   // Puzzle Board

	[Constructable]
	public MastermindPuzzleItem(MagicKey key) : base(0x2AAA)
	{
		Hue = 914;
		Key = key;
		m_Lifespan = 600;
		Movable = true;

		m_Timer = Timer.DelayCall(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10), Slice);
		m_Timer.Start();

		LootType = LootType.Blessed;
	}

	public override void GetProperties(ObjectPropertyList list)
	{
		base.GetProperties(list);

		list.Add(1072517, m_Lifespan.ToString()); // Lifespan: ~1_val~ seconds
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (MazePuzzleItem.IsInPuzzleRoom(from))
			base.OnDoubleClick(from);
	}

	/*public override void OnDoubleClick(Mobile from)
	{
	    PuzzleChestSolution solution = GetLastGuess(from);
	    if (solution != null)
	        solution = new PuzzleChestSolution(solution);
	    else
	        solution = new PuzzleChestSolution(PuzzleChestCylinder.None, PuzzleChestCylinder.None, PuzzleChestCylinder.None, PuzzleChestCylinder.None, PuzzleChestCylinder.None);

	    from.CloseGump(typeof(PuzzleChest.PuzzleGump));
	    from.CloseGump(typeof(PuzzleChest.StatusGump));
	    from.SendGump(new PuzzleChest.PuzzleGump(from, this, solution, 0));
	}*/

	private void Slice()
	{
		m_Lifespan -= 10;

		InvalidateProperties();

		if (m_Lifespan <= 0)
			Decay();
	}

	private void Decay()
	{
		if (RootParent is Mobile parent)
		{
			if (Name == null)
				parent.SendLocalizedMessage(1072515, "#" + LabelNumber); // The ~1_name~ expired...
			else
				parent.SendLocalizedMessage(1072515, Name); // The ~1_name~ expired...

			Effects.SendLocationParticles(EffectItem.Create(parent.Location, parent.Map, EffectItem.DefaultDuration), 0x3728, 8, 20, 5042);
			Effects.PlaySound(parent.Location, parent.Map, 0x201);

			parent.CloseGump(typeof(PuzzleGump));
			parent.CloseGump(typeof(StatusGump));
		}
		else
		{
			Effects.SendLocationParticles(EffectItem.Create(Location, Map, EffectItem.DefaultDuration), 0x3728, 8, 20, 5042);
			Effects.PlaySound(Location, Map, 0x201);
		}

		if (m_Timer != null)
			m_Timer.Stop();

		Delete();
	}

	public override void LockPick(Mobile from)
	{
		base.LockPick(from);

		Timer.DelayCall(TimeSpan.FromSeconds(3), Delete);

		Key?.Decay();

		ExperimentalGem gem = new()
		{
			Owner = from
		};

		from.AddToBackpack(gem);

		from.SendLocalizedMessage(1113382); // You've solved the puzzle!! An item has been placed in your bag.
	}

	protected override void GenerateTreasure()
	{
		Solution = new PuzzleChestSolution();
	}

	public MastermindPuzzleItem(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
		writer.Write(Key);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadInt();
		Key = reader.ReadItem() as MagicKey;

		m_Timer = Timer.DelayCall(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10), Slice);
		m_Timer.Start();
	}
}
