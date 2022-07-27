using Server.Engines.NewMagincia;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Multis;
using Server.Network;
using Server.Spells.Necromancy;
using Server.Targeting;

namespace Server.Spells.Fourth;

public class RecallSpell : MagerySpell
{
	private static readonly SpellInfo m_Info = new(
		"Recall", "Kal Ort Por",
		239,
		9031,
		Reagent.BlackPearl,
		Reagent.Bloodmoss,
		Reagent.MandrakeRoot
	);

	public override SpellCircle Circle => SpellCircle.Fourth;

	private readonly RunebookEntry _entry;
	private readonly Runebook _book;

	private bool NoSkillRequirement => (Core.SE && _book != null) || TransformationSpellHelper.UnderTransformation(Caster, typeof(WraithFormSpell));

	public RecallSpell(Mobile caster, Item scroll, RunebookEntry entry = null, Runebook book = null) : base(caster, scroll, m_Info)
	{
		_entry = entry;
		_book = book;
	}

	public override void GetCastSkills(out double min, out double max)
	{
		if (NoSkillRequirement)
		{
			min = max = 0;
		}
		else
		{
			base.GetCastSkills(out min, out max);
		}
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

	private void Target(object o)
	{
		switch (o)
		{
			case RecallRune {Marked: true} rune:
				Effect(rune.Target, rune.TargetMap, true);
				break;
			case RecallRune:
				Caster.SendLocalizedMessage(501805); // That rune is not yet marked.
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
			case Key key when key.KeyValue != 0 && key.Link is BaseBoat boat:
			{
				if (!boat.Deleted && boat.CheckKey(key.KeyValue))
					Effect(boat.GetMarkedLocation(), boat.Map, false);
				else
					Caster.Send(new MessageLocalized(Caster.Serial, Caster.Body, MessageType.Regular, 0x3B2, 3, 502357, Caster.Name, "")); // I can not recall from that object.
				break;
			}
			case HouseRaffleDeed deed1 when deed1.ValidLocation():
				Effect(deed1.PlotLocation, deed1.PlotFacet, true);
				break;
			case WritOfLease lease when lease.RecallLoc != Point3D.Zero && lease.Facet != null && lease.Facet != Map.Internal:
				Effect(lease.RecallLoc, lease.Facet, false);
				break;
			case WritOfLease:
				Caster.Send(new MessageLocalized(Caster.Serial, Caster.Body, MessageType.Regular, 0x3B2, 3, 502357, Caster.Name, "")); // I can not recall from that object.
				break;
			default:
				Caster.Send(new MessageLocalized(Caster.Serial, Caster.Body, MessageType.Regular, 0x3B2, 3, 502357, Caster.Name, "")); // I can not recall from that object.
				break;
		}
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

		if (WeightOverloading.IsOverloaded(Caster))
		{
			Caster.SendLocalizedMessage(502359, 0x22); // Thou art too encumbered to move.
			return false;
		}

		return SpellHelper.CheckTravel(Caster, TravelCheckType.RecallFrom);
	}

	private void Effect(Point3D loc, Map map, bool checkMulti)
	{
		if (Factions.Sigil.ExistsOn(Caster))
		{
			Caster.SendLocalizedMessage(1061632); // You can't do that while carrying the sigil.
		}
		else if (map == null || (!Core.AOS && Caster.Map != map))
		{
			Caster.SendLocalizedMessage(1005569); // You can not recall to another facet.
		}
		else if (!SpellHelper.CheckTravel(Caster, TravelCheckType.RecallFrom))
		{
		}
		else if (!SpellHelper.CheckTravel(Caster, map, loc, TravelCheckType.RecallTo))
		{
		}
		else if (map == Map.Felucca && Caster is PlayerMobile {Young: true})
		{
			Caster.SendLocalizedMessage(1049543); // You decide against traveling to Felucca while you are still young.
		}
		else if (SpellHelper.RestrictRedTravel && Caster.Murderer && map.Rules != ZoneRules.FeluccaRules)
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
		else if (WeightOverloading.IsOverloaded(Caster))
		{
			Caster.SendLocalizedMessage(502359, 0x22); // Thou art too encumbered to move.
		}
		else if (!map.CanSpawnMobile(loc.X, loc.Y, loc.Z))
		{
			Caster.SendLocalizedMessage(501942); // That location is blocked.
		}
		else if ((checkMulti && SpellHelper.CheckMulti(loc, map)))
		{
			Caster.SendLocalizedMessage(501942); // That location is blocked.
		}
		else if (_book is {CurCharges: <= 0})
		{
			Caster.SendLocalizedMessage(502412); // There are no charges left on that item.
		}
		else if (Caster.Holding != null)
		{
			Caster.SendLocalizedMessage(1071955); // You cannot teleport while dragging an object.
		}
		else if (CheckSequence())
		{
			BaseCreature.TeleportPets(Caster, loc, map, true);

			if (_book != null)
				--_book.CurCharges;

			Caster.PlaySound(0x1FC);
			Caster.MoveToWorld(loc, map);
			Caster.PlaySound(0x1FC);
		}

		FinishSequence();
	}

	private class InternalTarget : Target
	{
		private readonly RecallSpell _owner;

		public InternalTarget(RecallSpell owner) : base(owner.SpellRange, false, TargetFlags.None)
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
