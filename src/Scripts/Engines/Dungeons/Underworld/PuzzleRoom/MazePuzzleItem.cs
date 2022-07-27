using Server.Gumps;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Items;

public class MazePuzzleItem : BaseDecayingItem, ICircuitTrap
{
	public List<int> Path { get; set; }
	public List<int> Progress { get; set; }
	public CircuitCount Count => CircuitCount.ThirtySix;
	public int GumpTitle => 1153747;  // <center>GENERATOR CONTROL PANEL</center>
	public int GumpDescription => 1153749;  // // <center>Close the Grid Circuit</center>
	public bool CanDecipher => true;

	[CommandProperty(AccessLevel.GameMaster)]
	private MagicKey Key { get; set; }

	public override int LabelNumber => 1113379;  // Puzzle Board
	public override int Lifespan => 600;

	[Constructable]
	public MazePuzzleItem(MagicKey key) : base(0x2AAA)
	{
		Hue = 914;
		Key = key;
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (!IsChildOf(from.Backpack))
			from.SendLocalizedMessage(500325); // I am too far away to do that.
		else if (from is PlayerMobile mobile && IsInPuzzleRoom(mobile))
		{
			mobile.CloseGump(typeof(PuzzleChest.PuzzleGump));
			mobile.CloseGump(typeof(PuzzleChest.StatusGump));
			BaseGump.SendGump(new CircuitTrapGump(mobile, this));
		}
	}

	private static Rectangle2D _bounds = new(1089, 1162, 16, 12);

	public static bool IsInPuzzleRoom(Mobile from)
	{
		return from.Map == Map.TerMur && _bounds.Contains(new Point2D(from.X, from.Y));
	}

	public override void OnDelete()
	{
		if (RootParent is not Mobile m)
			return;

		if (m.HasGump(typeof(CircuitTrapGump)))
		{
			m.CloseGump(typeof(CircuitTrapGump));
		}
	}

	public void OnSelfClose(Mobile m)
	{
	}

	public void OnProgress(Mobile m, int pick)
	{
		m.PlaySound(0x1F5);
	}

	public void OnFailed(Mobile m)
	{
		DoDamage(m);
	}

	public void OnComplete(Mobile m)
	{
		m.PlaySound(0x3D);
		OnPuzzleCompleted(m);
	}

	private Timer m_DamageTimer;

	private void DoDamage(Mobile m)
	{
		if (m_DamageTimer is { Running: true })
			m_DamageTimer.Stop();

		m_DamageTimer = new InternalTimer(this, m);
		m_DamageTimer.Start();
	}

	private void ApplyShock(Mobile m, int tick)
	{
		if (m == null || !m.Alive || Deleted)
		{
			if (m_DamageTimer != null)
				m_DamageTimer.Stop();
		}
		else
		{
			int damage = 75 / Math.Max(1, tick - 1) + Utility.RandomMinMax(1, 9);

			AOS.Damage(m, damage, 0, 0, 0, 0, 100);

			m.BoltEffect(0);

			m.FixedParticles(0x3818, 1, 11, 0x13A8, 0, 0, EffectLayer.CenterFeet);
			m.FixedParticles(0x3818, 1, 11, 0x13A8, 0, 0, EffectLayer.Waist);
			m.FixedParticles(0x3818, 1, 11, 0x13A8, 0, 0, EffectLayer.Head);
			m.PlaySound(0x1DC);

			m.LocalOverheadMessage(Network.MessageType.Regular, 0x21, 1114443); // * Your body convulses from electric shock *
			m.NonlocalOverheadMessage(Network.MessageType.Regular, 0x21, 1114443, m.Name); //  * ~1_NAME~ spasms from electric shock *
		}
	}

	private class InternalTimer : Timer
	{
		private readonly MazePuzzleItem m_Item;
		private readonly Mobile m_From;
		private DateTime m_NextDamage;
		private int m_Tick;

		public InternalTimer(MazePuzzleItem item, Mobile from) : base(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1))
		{
			m_Item = item;
			m_From = from;
			m_NextDamage = DateTime.UtcNow;
			m_Tick = 0;

			item?.ApplyShock(from, 0);
		}

		protected override void OnTick()
		{
			m_Tick++;

			if (m_From == null || m_Item == null || !m_From.Alive || m_Item.Deleted)
			{
				Stop();
				return;
			}

			if (DateTime.UtcNow > m_NextDamage)
			{
				m_Item.ApplyShock(m_From, m_Tick);

				int delay = m_Tick switch
				{
					< 3 => 2,
					< 5 => 4,
					_ => 6
				};

				if (m_Tick >= 10)
					Stop();
				else
					m_NextDamage = DateTime.UtcNow + TimeSpan.FromSeconds(delay);
			}
		}
	}

	private void OnPuzzleCompleted(Mobile m)
	{
		Container pack = m?.Backpack;

		if (pack != null)
		{
			Item copperKey = pack.FindItemByType(typeof(CopperPuzzleKey));
			Item goldKey = pack.FindItemByType(typeof(GoldPuzzleKey));

			if (copperKey == null)
				pack.DropItem(new CopperPuzzleKey());
			else if (goldKey == null)
				pack.DropItem(new GoldPuzzleKey());
			else
				return;

			m.SendLocalizedMessage(1113382); // You've solved the puzzle!! An item has been placed in your bag.
		}

		Timer.DelayCall(TimeSpan.FromSeconds(3), Delete);
	}

	public MazePuzzleItem(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0); // ver
		writer.Write(Key);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadInt();
		Key = reader.ReadItem() as MagicKey;
	}
}
