using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;
using System;
using System.Linq;

namespace Server.Spells.Seventh;

public class GateTravelSpell : MagerySpell
{
	private static readonly SpellInfo m_Info = new(
		"Gate Travel", "Vas Rel Por",
		263,
		9032,
		Reagent.BlackPearl,
		Reagent.MandrakeRoot,
		Reagent.SulfurousAsh
	);

	public override SpellCircle Circle => SpellCircle.Seventh;

	private readonly RunebookEntry _entry;

	public GateTravelSpell(Mobile caster, Item scroll) : this(caster, scroll, null)
	{
	}

	public GateTravelSpell(Mobile caster, Item scroll, RunebookEntry entry) : base(caster, scroll, m_Info)
	{
		_entry = entry;
	}

	public override bool Cast()
	{
		bool success;
		if (Precast)
		{
			success = base.Cast();
		}
		else
		{
			if (_entry == null)
			{
				success = RequestSpellTarget();
			}
			else
			{
				SpellTargetCallback(Caster, _entry);
				success = true;
			}
		}
		return success;
	}

	public override void OnCast()
	{
		if (_entry == null)
		{
			if (Precast)
			{
				Caster.Target = new InternalTarget(this);
			}
			else
			{
				Target(SpellTarget);
			}
		}
		else
		{
			Effect(_entry.Location, _entry.Map, true);
		}
	}

	public void Target(object o)
	{
		switch (o)
		{
			case RecallRune {Marked: true} rune:
				Effect(rune.Target, rune.TargetMap, true);
				break;
			case RecallRune:
				Caster.SendLocalizedMessage(501803); // That rune is not yet marked.
				break;
			case Runebook runebook:
			{
				RunebookEntry e = runebook.Default;

				if (e != null)
					Effect(e.Location, e.Map, true);
				else
					Caster.SendLocalizedMessage(502354); // Target is not marked.
				break;
			}
			/*else if ( o is Key && ((Key)o).KeyValue != 0 && ((Key)o).Link is BaseBoat )
		{
			BaseBoat boat = ((Key)o).Link as BaseBoat;

			if ( !boat.Deleted && boat.CheckKey( ((Key)o).KeyValue ) )
				m_Owner.Effect( boat.GetMarkedLocation(), boat.Map, false );
			else
				from.Send( new MessageLocalized( from.Serial, from.Body, MessageType.Regular, 0x3B2, 3, 501030, from.Name, "" ) ); // I can not gate travel from that object.
		}*/
			case HouseRaffleDeed deed1 when deed1.ValidLocation():
				Effect(deed1.PlotLocation, deed1.PlotFacet, true);
				break;
			case Engines.NewMagincia.WritOfLease lease when lease.RecallLoc != Point3D.Zero && lease.Facet != null && lease.Facet != Map.Internal:
				Effect(lease.RecallLoc, lease.Facet, false);
				break;
			case Engines.NewMagincia.WritOfLease:
				Caster.Send(new MessageLocalized(Caster.Serial, Caster.Body, MessageType.Regular, 0x3B2, 3, 502357, Caster.Name, "")); // I can not recall from that object.
				break;
			default:
				Caster.Send(new MessageLocalized(Caster.Serial, Caster.Body, MessageType.Regular, 0x3B2, 3, 501030, Caster.Name, "")); // I can not gate travel from that object.
				break;
		}

		FinishSequence();
	}

	public override bool CheckCast()
	{
		if (Factions.Sigil.ExistsOn(Caster))
		{
			Caster.SendLocalizedMessage(1061632); // You can't do that while carrying the sigil.
			return false;
		}

		if (Caster.Criminal)
		{
			Caster.SendLocalizedMessage(1005561, 0x22); // Thou'rt a criminal and cannot escape so easily.
			return false;
		}

		if (SpellHelper.CheckCombat(Caster))
		{
			Caster.SendLocalizedMessage(1005564, 0x22); // Wouldst thou flee during the heat of battle??
			return false;
		}

		return SpellHelper.CheckTravel(Caster, TravelCheckType.GateFrom);
	}

	private static bool GateExistsAt(Map map, Point3D loc)
	{
		IPooledEnumerable eable = map.GetItemsInRange(loc, 0);
		bool gateFound = eable.Cast<Item>().Any(item => item is Moongate or PublicMoongate);
		eable.Free();

		return gateFound;
	}

	public void Effect(Point3D loc, Map map, bool checkMulti)
	{
		if (Factions.Sigil.ExistsOn(Caster))
		{
			Caster.SendLocalizedMessage(1061632); // You can't do that while carrying the sigil.
		}
		else if (map == null || (!Core.AOS && Caster.Map != map))
		{
			Caster.SendLocalizedMessage(1005570); // You can not gate to another facet.
		}
		else if (!SpellHelper.CheckTravel(Caster, TravelCheckType.GateFrom))
		{
		}
		else if (!SpellHelper.CheckTravel(Caster, map, loc, TravelCheckType.GateTo))
		{
		}
		else if (map == Map.Felucca && Caster is PlayerMobile {Young: true})
		{
			Caster.SendLocalizedMessage(1049543); // You decide against traveling to Felucca while you are still young.
		}
		else if (Caster.Murderer && map != Map.Felucca)
		{
			Caster.SendLocalizedMessage(1019004); // You are not allowed to travel there.
		}
		else if (Caster.Criminal)
		{
			Caster.SendLocalizedMessage(1005561, 0x22); // Thou'rt a criminal and cannot escape so easily.
		}
		else if (SpellHelper.CheckCombat(Caster))
		{
			Caster.SendLocalizedMessage(1005564, 0x22); // Wouldst thou flee during the heat of battle??
		}
		else if (!map.CanSpawnMobile(loc.X, loc.Y, loc.Z))
		{
			Caster.SendLocalizedMessage(501942); // That location is blocked.
		}
		else if (checkMulti && SpellHelper.CheckMulti(loc, map))
		{
			Caster.SendLocalizedMessage(501942); // That location is blocked.
		}
		else if (Core.SE && (GateExistsAt(map, loc) || GateExistsAt(Caster.Map, Caster.Location))) // SE restricted stacking gates
		{
			Caster.SendLocalizedMessage(1071242); // There is already a gate there.
		}
		else if (CheckSequence())
		{
			Caster.SendLocalizedMessage(501024); // You open a magical gate to another location

			Effects.PlaySound(Caster.Location, Caster.Map, 0x20E);

			InternalItem firstGate = new(loc, map);
			firstGate.MoveToWorld(Caster.Location, Caster.Map);

			Effects.PlaySound(loc, map, 0x20E);

			InternalItem secondGate = new(Caster.Location, Caster.Map);
			secondGate.MoveToWorld(loc, map);
		}

		FinishSequence();
	}

	[DispellableAttributes]
	private class InternalItem : Moongate
	{
		public override bool ShowFeluccaWarning => Core.AOS;

		public InternalItem(Point3D target, Map map) : base(target, map)
		{
			Map = map;

			if (ShowFeluccaWarning && map == Map.Felucca)
				ItemId = 0xDDA;

			Dispellable = true;

			InternalTimer t = new(this);
			t.Start();
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			Delete();
		}

		private class InternalTimer : Timer
		{
			private readonly Item _item;

			public InternalTimer(Item item) : base(TimeSpan.FromSeconds(30.0))
			{
				Priority = TimerPriority.OneSecond;
				_item = item;
			}

			protected override void OnTick()
			{
				_item.Delete();
			}
		}
	}

	private class InternalTarget : Target
	{
		private readonly GateTravelSpell _owner;

		public InternalTarget(GateTravelSpell owner) : base(12, false, TargetFlags.None)
		{
			_owner = owner;

			owner.Caster.LocalOverheadMessage(MessageType.Regular, 0x3B2, 501029); // Select Marked item.
		}

		protected override void OnTarget(Mobile from, object o)
		{
			_owner.Target(o);
		}

		protected override void OnNonlocalTarget(Mobile from, object o)
		{
		}

		protected override void OnTargetFinish(Mobile from)
		{
			_owner.FinishSequence();
		}
	}
}
