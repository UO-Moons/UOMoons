using Server.Network;
using Server.Spells;
using Server.Targeting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Items;

public class Firebomb : BaseItem
{
	private Timer m_Timer;
	private int m_Ticks;
	private Mobile m_LitBy;
	private List<Mobile> m_Users;

	[Constructable]
	public Firebomb() : this(0x99B)
	{
	}

	[Constructable]
	public Firebomb(int itemId) : base(itemId)
	{
		//Name = "a firebomb";
		Weight = 2.0;
		Hue = 1260;
	}

	public Firebomb(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.WriteEncodedInt(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadEncodedInt();
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (!IsChildOf(from.Backpack))
		{
			from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
			return;
		}

		if (Core.AOS && (from.Paralyzed || from.Frozen || from.Spell is { IsCasting: true }))
		{
			// to prevent exploiting for pvp
			from.SendLocalizedMessage(1075857); // You cannot use that while paralyzed.
			return;
		}

		if (m_Timer == null)
		{
			m_Timer = Timer.DelayCall(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1), OnFirebombTimerTick);
			m_LitBy = from;
			from.SendLocalizedMessage(1060582); // You light the firebomb.  Throw it now!
		}
		else
			from.SendLocalizedMessage(1060581); // You've already lit it!  Better throw it now!

		m_Users ??= new List<Mobile>();

		if (!m_Users.Contains(from))
			m_Users.Add(from);

		from.Target = new ThrowTarget(this);
	}

	private void OnFirebombTimerTick()
	{
		if (Deleted)
		{
			m_Timer.Stop();
			return;
		}

		if (Map == Map.Internal && HeldBy == null)
			return;

		switch (m_Ticks)
		{
			case 0:
			case 1:
			case 2:
			{
				++m_Ticks;

				if (HeldBy != null)
					HeldBy.PublicOverheadMessage(MessageType.Regular, 957, false, m_Ticks.ToString());
				else switch (RootParent)
				{
					case null:
						PublicOverheadMessage(MessageType.Regular, 957, false, m_Ticks.ToString());
						break;
					case Mobile:
						((Mobile)RootParent).PublicOverheadMessage(MessageType.Regular, 957, false, m_Ticks.ToString());
						break;
				}

				break;
			}
			default:
			{
				HeldBy?.DropHolding();

				if (m_Users != null)
				{
					foreach (var m in from m in m_Users let targ = m.Target as ThrowTarget where targ != null && targ.Bomb == this select m)
					{
						m.Target.Cancel(m);
					}

					m_Users.Clear();
					m_Users = null;
				}

				switch (RootParent)
				{
					case Mobile:
					{
						Mobile parent = (Mobile)RootParent;
						parent.SendLocalizedMessage(1060583); // The firebomb explodes in your hand!
						AOS.Damage(parent, Utility.Random(3) + 4, 0, 100, 0, 0, 0);
						break;
					}
					case null:
					{
						IPooledEnumerable eable = Map.GetMobilesInRange(Location, 1);

						List<Mobile> toDamage = eable.Cast<Mobile>().ToList();
						eable.Free();

						for (int i = 0; i < toDamage.Count; ++i)
						{
							var victim = toDamage[i];

							if (m_LitBy == null || (SpellHelper.ValidIndirectTarget(m_LitBy, victim) && m_LitBy.CanBeHarmful(victim, false)))
							{
								m_LitBy?.DoHarmful(victim);

								AOS.Damage(victim, m_LitBy, Utility.Random(3) + 4, 0, 100, 0, 0, 0);
							}
						}
						(new FirebombField(m_LitBy, toDamage)).MoveToWorld(Location, Map);
						break;
					}
				}

				m_Timer.Stop();
				Delete();
				break;
			}
		}
	}

	private void OnFirebombTarget(Mobile from, object obj)
	{
		if (Deleted || Map == Map.Internal || !IsChildOf(from.Backpack))
			return;

		if (obj is not IPoint3D p)
			return;

		SpellHelper.GetSurfaceTop(ref p);

		from.RevealingAction();

		IEntity to;

		if (p is Mobile mobile)
			to = mobile;
		else
			to = new Entity(Serial.Zero, new Point3D(p), Map);

		Effects.SendMovingEffect(from, to, ItemId, 7, 0, false, false, Hue);

		Timer.DelayCall(TimeSpan.FromSeconds(1.0), new TimerStateCallback(FirebombReposition_OnTick), new object[] { p, Map });
		Internalize();
	}

	private void FirebombReposition_OnTick(object state)
	{
		if (Deleted)
			return;

		object[] states = (object[])state;
		IPoint3D p = (IPoint3D)states[0];
		Map map = (Map)states[1];

		MoveToWorld(new Point3D(p), map);
	}

	private class ThrowTarget : Target
	{
		public Firebomb Bomb { get; }

		public ThrowTarget(Firebomb bomb)
			: base(12, true, TargetFlags.None)
		{
			Bomb = bomb;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			Bomb.OnFirebombTarget(from, targeted);
		}
	}
}

public class FirebombField : BaseItem
{
	private readonly List<Mobile> m_Burning;
	private readonly Timer m_Timer;
	private readonly Mobile m_LitBy;
	private readonly DateTime m_Expire;

	public FirebombField(Mobile litBy, List<Mobile> toDamage) : base(0x376A)
	{
		Movable = false;
		m_LitBy = litBy;
		m_Expire = DateTime.UtcNow + TimeSpan.FromSeconds(10);
		m_Burning = toDamage;
		m_Timer = Timer.DelayCall(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0), OnFirebombFieldTimerTick);
	}

	public FirebombField(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		// Don't serialize these...
	}

	public override void Deserialize(GenericReader reader)
	{
	}

	public override bool OnMoveOver(Mobile m)
	{
		if (ItemId == 0x398C && m_LitBy == null || (SpellHelper.ValidIndirectTarget(m_LitBy, m) && m_LitBy.CanBeHarmful(m, false)))
		{
			if (m_LitBy != null)
				m_LitBy.DoHarmful(m);

			AOS.Damage(m, m_LitBy, 2, 0, 100, 0, 0, 0);
			m.PlaySound(0x208);

			if (!m_Burning.Contains(m))
				m_Burning.Add(m);
		}

		return true;
	}

	private void OnFirebombFieldTimerTick()
	{
		if (Deleted)
		{
			m_Timer.Stop();
			return;
		}

		if (ItemId == 0x376A)
		{
			ItemId = 0x398C;
			return;
		}

		for (int i = 0; i < m_Burning.Count;)
		{
			var victim = m_Burning[i];

			if (victim.Location == Location && victim.Map == Map && (m_LitBy == null || (SpellHelper.ValidIndirectTarget(m_LitBy, victim) && m_LitBy.CanBeHarmful(victim, false))))
			{
				m_LitBy?.DoHarmful(victim);

				AOS.Damage(victim, m_LitBy, Utility.Random(3) + 4, 0, 100, 0, 0, 0);
				++i;
			}
			else
				m_Burning.RemoveAt(i);
		}

		if (DateTime.UtcNow < m_Expire) return;
		m_Timer.Stop();
		Delete();
	}
}
