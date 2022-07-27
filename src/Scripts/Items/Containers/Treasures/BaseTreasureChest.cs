using System;

namespace Server.Items;

public class BaseTreasureChest : LockableContainer
{
	private TreasureResetTimer m_ResetTimer;

	[CommandProperty(AccessLevel.GameMaster)]
	private TreasureLevel Level { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	private short MaxSpawnTime { get; set; } = 60;

	[CommandProperty(AccessLevel.GameMaster)]
	private short MinSpawnTime { get; set; } = 10;

	[CommandProperty(AccessLevel.GameMaster)]
	public override bool Locked
	{
		get => base.Locked;
		set
		{
			if (base.Locked == value)
				return;

			base.Locked = value;

			if (!value)
				StartResetTimer();
		}
	}

	public override bool IsDecoContainer => false;

	public BaseTreasureChest(int itemId, TreasureLevel level = TreasureLevel.Level2) : base(itemId)
	{
		Level = level;
		Locked = true;
		Movable = false;

		SetLockLevel();
		GenerateTreasure();
	}

	public BaseTreasureChest(Serial serial) : base(serial)
	{
	}

	public override string DefaultName => Locked ? "a locked treasure chest" : "a treasure chest";

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0);
		writer.Write((byte)Level);
		writer.Write(MinSpawnTime);
		writer.Write(MaxSpawnTime);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadInt();

		Level = (TreasureLevel)reader.ReadByte();
		MinSpawnTime = reader.ReadShort();
		MaxSpawnTime = reader.ReadShort();

		if (!Locked)
			StartResetTimer();
	}

	protected virtual void SetLockLevel()
	{
		RequiredSkill = Level switch
		{
			TreasureLevel.Level1 => LockLevel = 5,
			TreasureLevel.Level2 => LockLevel = 20,
			TreasureLevel.Level3 => LockLevel = 50,
			TreasureLevel.Level4 => LockLevel = 70,
			TreasureLevel.Level5 => LockLevel = 90,
			TreasureLevel.Level6 => LockLevel = 100,
			_ => RequiredSkill
		};
	}

	private void StartResetTimer()
	{
		if (m_ResetTimer == null)
			m_ResetTimer = new TreasureResetTimer(this);
		else
			m_ResetTimer.Delay = TimeSpan.FromMinutes(Utility.Random(MinSpawnTime, MaxSpawnTime));

		m_ResetTimer.Start();
	}

	protected virtual void GenerateTreasure()
	{
		int minGold = 1;
		int maxGold = 2;

		switch (Level)
		{
			case TreasureLevel.Level1:
				minGold = 100;
				maxGold = 300;
				break;

			case TreasureLevel.Level2:
				minGold = 300;
				maxGold = 600;
				break;

			case TreasureLevel.Level3:
				minGold = 600;
				maxGold = 900;
				break;

			case TreasureLevel.Level4:
				minGold = 900;
				maxGold = 1200;
				break;

			case TreasureLevel.Level5:
				minGold = 1200;
				maxGold = 5000;
				break;

			case TreasureLevel.Level6:
				minGold = 5000;
				maxGold = 9000;
				break;
		}

		DropItem(new Gold(minGold, maxGold));
	}

	private void ClearContents()
	{
		for (int i = Items.Count - 1; i >= 0; --i)
		{
			if (i < Items.Count)
				Items[i].Delete();
		}
	}

	private void Reset()
	{
		if (m_ResetTimer is { Running: true }) m_ResetTimer.Stop();

		Locked = true;
		ClearContents();
		GenerateTreasure();
	}

	public enum TreasureLevel
	{
		Level1,
		Level2,
		Level3,
		Level4,
		Level5,
		Level6,
	};

	private class TreasureResetTimer : Timer
	{
		private readonly BaseTreasureChest m_Chest;

		public TreasureResetTimer(BaseTreasureChest chest) : base(TimeSpan.FromMinutes(Utility.Random(chest.MinSpawnTime, chest.MaxSpawnTime)))
		{
			m_Chest = chest;
			Priority = TimerPriority.OneMinute;
		}

		protected override void OnTick()
		{
			m_Chest.Reset();
		}
	}
}
