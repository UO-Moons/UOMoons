using Server.Network;
using Server.Regions;
using Server.Targeting;
using System;

namespace Server.Items;

public sealed class GoblinFloorTrap : BaseTrap, IRevealableItem
{
	[CommandProperty(AccessLevel.GameMaster)]
	public Mobile Owner { get; private set; }

	public override int LabelNumber => 1113296;  // Armed Floor Trap
	public bool CheckWhenHidden => true;

	[Constructable]
	public GoblinFloorTrap() : this(null)
	{
	}

	[Constructable]
	public GoblinFloorTrap(Mobile from) : base(0x4004)
	{
		Owner = from;
		Visible = false;
	}

	protected override bool PassivelyTriggered => true;
	protected override TimeSpan PassiveTriggerDelay => TimeSpan.FromSeconds(1.0);
	protected override int PassiveTriggerRange => 1;
	protected override TimeSpan ResetDelay => TimeSpan.FromSeconds(1.0);

	protected override void OnTrigger(Mobile from)
	{
		if (from.AccessLevel > AccessLevel.Player || !from.Alive)
			return;

		if (Owner != null)
		{
			if (!Owner.CanBeHarmful(from) || Owner == from)
				return;

			if (Owner.Guild != null && Owner.Guild == from.Guild)
				return;
		}

		from.SendSound(0x22B);
		from.SendLocalizedMessage(1095157); // You stepped onto a goblin trap!

		Spells.SpellHelper.Damage(TimeSpan.FromSeconds(0.30), from, from, Utility.RandomMinMax(50, 75), 100, 0, 0, 0, 0);

		if (Owner != null)
			from.DoHarmful(Owner);

		Visible = true;
		Timer.DelayCall(TimeSpan.FromSeconds(10), Rehide_Callback);

		PublicOverheadMessage(MessageType.Regular, 0x65, 500813); // [Trapped]

		new Blood().MoveToWorld(from.Location, from.Map);
	}

	public bool CheckReveal(Mobile m)
	{
		return m.CheckTargetSkill(SkillName.DetectHidden, this, 50.0, 100.0);
	}

	public void OnRevealed(Mobile m)
	{
		Unhide();
	}

	public bool CheckPassiveDetect(Mobile m)
	{
		if (Visible && 0.05 > Utility.RandomDouble())
		{
			if (m.NetState != null)
			{
				Packet p = new MessageLocalized(Serial, ItemId, MessageType.Regular, 0x65, 3, 500813, Name, string.Empty);
				p.Acquire();
				m.NetState.Send(p);
				Packet.Release(p);

				return true;
			}
		}

		return false;
	}

	private void Unhide()
	{
		Visible = true;

		Timer.DelayCall(TimeSpan.FromSeconds(10), Rehide_Callback);
	}

	private void Rehide_Callback()
	{
		Visible = false;
	}

	public GoblinFloorTrap(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version
		writer.Write(Owner);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadInt();
		Owner = reader.ReadMobile();
	}
}

public class GoblinFloorTrapKit : Item
{
	[Constructable]
	public GoblinFloorTrapKit() : base(16704)
	{
	}

	public override void OnDoubleClick(Mobile from)
	{
		Region r = from.Region;

		if (!IsChildOf(from.Backpack))
		{
			from.SendLocalizedMessage(1054107); // This item must be in your backpack.
		}
		else if (from.Skills[SkillName.Tinkering].Value < 80)
		{
			from.SendLocalizedMessage(1113318); // You do not have enough skill to set the trap.
		}
		else if (from.Mounted || from.Flying)
		{
			from.SendLocalizedMessage(1113319); // You cannot set the trap while riding or flying.
		}
		else if (r is GuardedRegion region && !region.IsDisabled())
		{
			from.SendMessage("You cannot place a trap in a guard region.");
		}
		else
		{
			from.Target = new InternalTarget(this);
		}
	}

	private class InternalTarget : Target
	{
		private readonly GoblinFloorTrapKit m_Kit;

		public InternalTarget(GoblinFloorTrapKit kit) : base(-1, false, TargetFlags.None)
		{
			m_Kit = kit;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (targeted is not IPoint3D point3D)
				return;

			Point3D p = new(point3D);
			Region r = Region.Find(p, from.Map);

			if (from.Skills[SkillName.Tinkering].Value < 80)
			{
				from.SendLocalizedMessage(1113318); // You do not have enough skill to set the trap.
			}
			else if (from.Mounted || from.Flying)
			{
				from.SendLocalizedMessage(1113319); // You cannot set the trap while riding or flying.
			}
			else if (r is GuardedRegion region && !region.IsDisabled())
			{
				from.SendMessage("You cannot place a trap in a guard region.");
			}
			if (from.InRange(p, 2))
			{
				GoblinFloorTrap trap = new(from);

				trap.MoveToWorld(p, from.Map);
				from.SendLocalizedMessage(1113294);  // You carefully arm the goblin trap.
				from.SendLocalizedMessage(1113297);  // You hide the trap to the best of your ability.            

				m_Kit.Consume();
			}
			else
				from.SendLocalizedMessage(500446); // That is too far away.
		}
	}

	public GoblinFloorTrapKit(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadInt();
	}
}
