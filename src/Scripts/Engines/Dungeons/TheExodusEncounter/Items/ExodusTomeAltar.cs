using Server.Commands;
using Server.ContextMenus;
using Server.Engines.Exodus;
using Server.Engines.PartySystem;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Items;

public sealed class ExodusTomeAltar : BaseDecayingItem
{
	public override int LabelNumber => 1153602;  // Exodus Summoning Tome 
	public static ExodusTomeAltar Altar { get; private set; }
	public static TimeSpan DelayExit => TimeSpan.FromMinutes(10);
	private readonly Point3D m_TeleportDest = new(764, 640, 0);
	public override int Lifespan => 420;
	public override bool UseSeconds => false;
	private Item m_ExodusAlterAddon;

	public List<RitualArray> Rituals { get; }

	public Mobile Owner { get; set; }

	[Constructable]
	public ExodusTomeAltar()
		: base(0x1C11)
	{
		Hue = 1943;
		Movable = false;
		LootType = LootType.Regular;
		Weight = 0.0;

		Rituals = new List<RitualArray>();
		m_ExodusAlterAddon = new ExodusAlterAddon
		{
			Movable = false
		};
	}

	public ExodusTomeAltar(Serial serial) : base(serial)
	{
	}

	private class BeginTheRitual : ContextMenuEntry
	{
		private readonly Mobile m_Mobile;
		private readonly ExodusTomeAltar m_Altar;

		public BeginTheRitual(ExodusTomeAltar altar, Mobile from) : base(1153608, 2) // Begin the Ritual
		{
			m_Mobile = from;
			m_Altar = altar;

			if (altar.Owner != from)
				Flags |= CMEFlags.Disabled;
		}

		public override void OnClick()
		{
			if (m_Altar.Owner == m_Mobile)
			{
				m_Altar.SendConfirmationsExodus(m_Mobile);
			}
			else
			{
				m_Mobile.SendLocalizedMessage(1153610); // Only the altar owner can commence with the ritual. 
			}
		}
	}

	public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
	{
		base.GetContextMenuEntries(from, list);

		list.Add(new BeginTheRitual(this, from));
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (!from.HasGump(typeof(AltarGump)))
		{
			from.SendGump(new AltarGump());
		}
	}

	public override bool OnDragDrop(Mobile from, Item dropped)
	{
		return false;
	}

	public override void OnAfterDelete()
	{
		base.OnAfterDelete();

		m_ExodusAlterAddon?.Delete();

		if (Altar != null)
			Altar = null;
	}

	public override void OnMapChange()
	{
		if (Deleted)
			return;

		if (m_ExodusAlterAddon != null)
			m_ExodusAlterAddon.Map = Map;
	}

	public override void OnLocationChange(Point3D oldLoc)
	{
		if (Deleted)
			return;

		if (m_ExodusAlterAddon != null)
			m_ExodusAlterAddon.Location = new Point3D(X - 1, Y - 1, Z - 18);
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0); // version

		writer.Write(m_ExodusAlterAddon);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadInt();

		m_ExodusAlterAddon = reader.ReadItem();
	}

	public static bool CheckParty(Mobile from, Mobile m)
	{
		Party party = Party.Get(from);

		if (party == null)
			return false;

		return party.Members.Any(info => info.Mobile == m);
	}

	private void SendConfirmationsExodus(Mobile from)
	{
		Party party = Party.Get(from);

		if (party != null)
		{
			int memberRange = party.Members.Count(x => !from.InRange(x.Mobile, 5));

			if (memberRange != 0)
			{
				from.SendLocalizedMessage(1153611); // One or more members of your party are not close enough to you to perform the ritual.
				return;
			}

			foreach (var info in from info in party.Members let robe = info.Mobile.FindItemOnLayer(Layer.OuterTorso) as RobeofRite where !Rituals.Any(z => z.RitualMobile == info.Mobile && z.Ritual1 && z.Ritual2) || robe == null select info)
			{
				from.SendLocalizedMessage(1153609, info.Mobile.Name); // ~1_PLAYER~ has not fulfilled all the requirements of the Ritual! You cannot commence until they do.
				return;
			}

			foreach (PartyMemberInfo info in party.Members)
			{
				SendBattleground(info.Mobile);
			}
		}
		else
		{
			from.SendLocalizedMessage(1153596); // You must join a party with the players you wish to perform the ritual with. 
		}
	}

	private void SendBattleground(Mobile from)
	{
		if (VerLorRegController.Active && VerLorRegController.Mobile != null && ExodusSummoningAlter.CheckExodus())
		{
			// teleport party member
			from.FixedParticles(0x376A, 9, 32, 0x13AF, EffectLayer.Waist);
			from.PlaySound(0x1FE);
			from.MoveToWorld(m_TeleportDest, Map.Ilshenar);
			BaseCreature.TeleportPets(from, m_TeleportDest, Map.Ilshenar);

			// Robe of Rite Delete
			if (from.FindItemOnLayer(Layer.OuterTorso) is RobeofRite robe)
			{
				robe.Delete();
			}

			// Altar Delete
			Timer.DelayCall(TimeSpan.FromSeconds(2), Delete);
		}
		else
		{
			from.SendLocalizedMessage(1075213); // The master of this realm has already been summoned and is engaged in combat.  Your opportunity will come after he has squashed the current batch of intruders!
		}
	}
}

public class RitualArray
{
	public Mobile RitualMobile { get; init; }
	public bool Ritual1 { get; set; }
	public bool Ritual2 { get; set; }
}

public class AltarGump : Gump
{
	public static void Initialize()
	{
		CommandSystem.Register("TomeAltarGump", AccessLevel.Administrator, TomeAltarGump_OnCommand);
	}

	[Usage("TomeAltarGump")]
	private static void TomeAltarGump_OnCommand(CommandEventArgs e)
	{
		Mobile from = e.Mobile;

		if (!from.HasGump(typeof(AltarGump)))
		{
			from.SendGump(new AltarGump());
		}
	}

	public AltarGump() : base(100, 100)
	{
		Closable = true;
		Disposable = true;
		Dragable = true;

		AddPage(0);
		AddBackground(0, 0, 447, 195, 5120);
		AddHtmlLocalized(17, 14, 412, 161, 1153607, 0x7FFF, false, false); // Contained within this Tome is the ritual by which Lord Exodus may once again be called upon Britannia in his physical form, summoned from deep within the Void.  Only when the Summoning Rite has been rejoined with the tome and only when the Robe of Rite covers the caster can the Sacrificial Dagger be used to seal thy fate.  Stab into this book the dagger and declare thy quest for Valor as thou stand to defend Britannia from this evil, or sacrifice thy blood unto this altar to declare thy quest for greed and wealth...only thou can judge thyself...
	}

	public override void OnResponse(NetState state, RelayInfo info)
	{
		switch (info.ButtonID)
		{
			case 0:
			{
				//Cancel
				break;
			}
		}
	}
}
