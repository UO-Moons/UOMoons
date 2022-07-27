using Server.Engines.PartySystem;
using Server.Mobiles;
using Server.Targeting;
using System.Linq;

namespace Server.Items;

public class ExodusSummoningRite : BaseDecayingItem
{
	public override int LabelNumber => 1153498;  // exodus summoning rite 

	[Constructable]
	public ExodusSummoningRite() : base(0x2258)
	{
		Weight = 1;
		Hue = 1910;
		LootType = LootType.Regular;
	}

	public override int Lifespan => 604800;
	public override bool UseSeconds => false;

	public ExodusSummoningRite(Serial serial) : base(serial)
	{
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (!IsChildOf(from.Backpack))
		{
			from.SendLocalizedMessage(1054107); // This item must be in your backpack.
		}
		else if (Party.Get(from) == null)
		{
			from.SendLocalizedMessage(1153596); // You must join a party with the players you wish to perform the ritual with. 
		}
		else if (from.FindItemOnLayer(Layer.OuterTorso) is not RobeofRite || from.FindItemOnLayer(Layer.OneHanded) is not ExodusSacrificalDagger)
		{
			from.SendLocalizedMessage(1153591); // Thou art not properly attired to perform such a ritual.
		}
		else
		{
			from.SendLocalizedMessage(1153600); // Which Summoning Tome do you wish to use this on? 
			from.Target = new RiteTarget(this);
		}
	}

	private class RiteTarget : Target
	{
		private readonly Item m_Deed;

		public RiteTarget(Item deed) : base(2, true, TargetFlags.None)
		{
			m_Deed = deed;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (targeted is ExodusTomeAltar altar)
			{
				if (ExodusTomeAltar.CheckParty(altar.Owner, from))
				{
					if (altar.Rituals.All(s => s.RitualMobile != from))
					{
						altar.Rituals.Add(new RitualArray { RitualMobile = from, Ritual1 = false, Ritual2 = false });
					}

					bool riteRitual = altar.Rituals.Find(s => s.RitualMobile == from)!.Ritual1;

					if (!riteRitual)
					{
						((PlayerMobile)from).UseSummoningRite = true;
						from.Say(1153597); // You place the rite within the tome and begin to meditate...
						altar.Rituals.Find(s => s.RitualMobile == from)!.Ritual1 = true;
						m_Deed.Delete();
						from.SendLocalizedMessage(1153598, from.Name); // ~1_PLAYER~ has read the Summoning Rite! 
					}
					else
					{
						from.SendLocalizedMessage(1153599); // You've already used this item in another ritual. 
					}
				}
				else
				{
					from.SendLocalizedMessage(1153595); // You must first join the party of the person who built this altar.
				}
			}
			else
			{
				from.SendLocalizedMessage(1153601); // That is not a Summoning Tome. 
			}
		}
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadInt();
	}
}
