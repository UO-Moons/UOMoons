using Server.Network;
using Server.Targeting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Items;

public class CrystalRechargeInfo
{
	public static readonly CrystalRechargeInfo[] Table = {
		new( typeof( Citrine ), 500 ),
		new( typeof( Amber ), 500 ),
		new( typeof( Tourmaline ), 750 ),
		new( typeof( Emerald ), 1000 ),
		new( typeof( Sapphire ), 1000 ),
		new( typeof( Amethyst ), 1000 ),
		new( typeof( StarSapphire ), 1250 ),
		new( typeof( Diamond ), 2000 )
	};

	public static CrystalRechargeInfo Get(Type type)
	{
		return Table.FirstOrDefault(info => info.Type == type);
	}

	private Type Type { get; }
	public int Amount { get; }

	private CrystalRechargeInfo(Type type, int amount)
	{
		Type = type;
		Amount = amount;
	}
}

public class BroadcastCrystal : BaseItem
{
	private const int MaxCharges = 2000;

	public override int LabelNumber => 1060740;  // communication crystal

	private int m_Charges;

	[CommandProperty(AccessLevel.GameMaster)]
	private bool Active
	{
		get => ItemId == 0x1ECD;
		set
		{
			ItemId = value ? 0x1ECD : 0x1ED0;
			InvalidateProperties();
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	private int Charges
	{
		get => m_Charges;
		set
		{
			m_Charges = value;
			InvalidateProperties();
		}
	}

	public List<ReceiverCrystal> Receivers { get; private set; }

	[Constructable]
	public BroadcastCrystal() : this(2000)
	{
	}

	[Constructable]
	private BroadcastCrystal(int charges) : base(0x1ED0)
	{
		Light = LightType.Circle150;

		m_Charges = charges;

		Receivers = new List<ReceiverCrystal>();
	}

	public BroadcastCrystal(Serial serial) : base(serial)
	{
	}

	public override void GetProperties(ObjectPropertyList list)
	{
		base.GetProperties(list);

		list.Add(Active ? 1060742 : 1060743); // active / inactive
		list.Add(1060745); // broadcast
		list.Add(1060741, Charges.ToString()); // charges: ~1_val~

		if (Receivers.Count > 0)
			list.Add(1060746, Receivers.Count.ToString()); // links: ~1_val~
	}

	public override void OnSingleClick(Mobile from)
	{
		base.OnSingleClick(from);

		LabelTo(from, Active ? 1060742 : 1060743); // active / inactive
		LabelTo(from, 1060745); // broadcast
		LabelTo(from, 1060741, Charges.ToString()); // charges: ~1_val~

		if (Receivers.Count > 0)
			LabelTo(from, 1060746, Receivers.Count.ToString()); // links: ~1_val~
	}

	public override bool HandlesOnSpeech => true;

	public override void OnSpeech(SpeechEventArgs e)
	{
		if (!Active || Receivers.Count == 0 || (RootParent != null && RootParent is not Mobile))
			return;

		if (e.Type == MessageType.Emote)
			return;

		Mobile from = e.Mobile;
		string speech = e.Speech;

		foreach (ReceiverCrystal receiver in new List<ReceiverCrystal>(Receivers))
		{
			if (receiver.Deleted)
			{
				Receivers.Remove(receiver);
			}
			else if (Charges > 0)
			{
				receiver.TransmitMessage(from, speech);
				Charges--;
			}
			else
			{
				Active = false;
				break;
			}
		}
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (!from.InRange(GetWorldLocation(), 2))
		{
			from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
			return;
		}

		from.Target = new InternalTarget(this);
	}

	private class InternalTarget : Target
	{
		private readonly BroadcastCrystal m_Crystal;

		public InternalTarget(BroadcastCrystal crystal) : base(2, false, TargetFlags.None)
		{
			m_Crystal = crystal;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (!m_Crystal.IsAccessibleTo(from))
				return;

			if (from.Map != m_Crystal.Map || !from.InRange(m_Crystal.GetWorldLocation(), 2))
			{
				from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
				return;
			}

			if (targeted == m_Crystal)
			{
				if (m_Crystal.Active)
				{
					m_Crystal.Active = false;
					from.SendLocalizedMessage(500672); // You turn the crystal off.
				}
				else
				{
					if (m_Crystal.Charges > 0)
					{
						m_Crystal.Active = true;
						from.SendLocalizedMessage(500673); // You turn the crystal on.
					}
					else
					{
						from.SendLocalizedMessage(500676); // This crystal is out of charges.
					}
				}
			}
			else if (targeted is ReceiverCrystal crystal)
			{
				if (m_Crystal.Receivers.Count >= 10)
				{
					from.SendLocalizedMessage(1010042); // This broadcast crystal is already linked to 10 receivers.
				}
				else if (crystal.Sender == m_Crystal)
				{
					from.SendLocalizedMessage(500674); // This crystal is already linked with that crystal.
				}
				else if (crystal.Sender != null)
				{
					from.SendLocalizedMessage(1010043); // That receiver crystal is already linked to another broadcast crystal.
				}
				else
				{
					crystal.Sender = m_Crystal;
					from.SendLocalizedMessage(500675); // That crystal has been linked to this crystal.
				}
			}
			else if (targeted == from)
			{
				foreach (ReceiverCrystal receiver in new List<ReceiverCrystal>(m_Crystal.Receivers))
				{
					receiver.Sender = null;
				}

				from.SendLocalizedMessage(1010046); // You unlink the broadcast crystal from all of its receivers.
			}
			else
			{
				if (targeted is Item targItem && targItem.VerifyMove(from))
				{
					CrystalRechargeInfo info = CrystalRechargeInfo.Get(targItem.GetType());

					if (info != null)
					{
						if (m_Crystal.Charges >= MaxCharges)
						{
							from.SendLocalizedMessage(500678); // This crystal is already fully charged.
						}
						else
						{
							targItem.Consume();

							if (m_Crystal.Charges + info.Amount >= MaxCharges)
							{
								m_Crystal.Charges = MaxCharges;
								from.SendLocalizedMessage(500679); // You completely recharge the crystal.
							}
							else
							{
								m_Crystal.Charges += info.Amount;
								from.SendLocalizedMessage(500680); // You recharge the crystal.
							}
						}

						return;
					}
				}

				from.SendLocalizedMessage(500681); // You cannot use this crystal on that.
			}
		}
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.WriteEncodedInt(0);

		writer.WriteEncodedInt(m_Charges);
		writer.WriteItemList(Receivers);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadEncodedInt();

		m_Charges = reader.ReadEncodedInt();
		Receivers = reader.ReadStrongItemList<ReceiverCrystal>();
	}
}

public class ReceiverCrystal : BaseItem
{
	public override int LabelNumber => 1060740;  // communication crystal

	private BroadcastCrystal m_Sender;

	[CommandProperty(AccessLevel.GameMaster)]
	private bool Active
	{
		get => ItemId == 0x1ED1;
		set
		{
			ItemId = value ? 0x1ED1 : 0x1ED0;
			InvalidateProperties();
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public BroadcastCrystal Sender
	{
		get => m_Sender;
		set
		{
			if (m_Sender != null)
			{
				m_Sender.Receivers.Remove(this);
				m_Sender.InvalidateProperties();
			}

			m_Sender = value;

			if (value == null)
				return;
			value.Receivers.Add(this);
			value.InvalidateProperties();
		}
	}

	[Constructable]
	public ReceiverCrystal() : base(0x1ED0)
	{
		Light = LightType.Circle150;
	}

	public ReceiverCrystal(Serial serial) : base(serial)
	{
	}

	public override void GetProperties(ObjectPropertyList list)
	{
		base.GetProperties(list);

		list.Add(Active ? 1060742 : 1060743); // active / inactive
		list.Add(1060744); // receiver
	}

	public override void OnSingleClick(Mobile from)
	{
		base.OnSingleClick(from);

		LabelTo(from, Active ? 1060742 : 1060743); // active / inactive
		LabelTo(from, 1060744); // receiver
	}

	public void TransmitMessage(Mobile from, string message)
	{
		if (!Active)
			return;

		string text = $"{from.Name} says {message}";

		switch (RootParent)
		{
			case Mobile:
				((Mobile)RootParent).SendMessage(0x2B2, "Crystal: " + text);
				break;
			case Item:
				((Item)RootParent).PublicOverheadMessage(MessageType.Regular, 0x2B2, false, "Crystal: " + text);
				break;
			default:
				PublicOverheadMessage(MessageType.Regular, 0x2B2, false, text);
				break;
		}
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (!from.InRange(GetWorldLocation(), 2))
		{
			from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
			return;
		}

		from.Target = new InternalTarget(this);
	}

	private class InternalTarget : Target
	{
		private readonly ReceiverCrystal m_Crystal;

		public InternalTarget(ReceiverCrystal crystal) : base(-1, false, TargetFlags.None)
		{
			m_Crystal = crystal;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (!m_Crystal.IsAccessibleTo(from))
				return;

			if (from.Map != m_Crystal.Map || !from.InRange(m_Crystal.GetWorldLocation(), 2))
			{
				from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
				return;
			}

			if (targeted == m_Crystal)
			{
				if (m_Crystal.Active)
				{
					m_Crystal.Active = false;
					from.SendLocalizedMessage(500672); // You turn the crystal off.
				}
				else
				{
					m_Crystal.Active = true;
					from.SendLocalizedMessage(500673); // You turn the crystal on.
				}
			}
			else if (targeted == from)
			{
				if (m_Crystal.Sender != null)
				{
					m_Crystal.Sender = null;
					from.SendLocalizedMessage(1010044); // You unlink the receiver crystal.
				}
				else
				{
					from.SendLocalizedMessage(1010045); // That receiver crystal is not linked.
				}
			}
			else
			{
				if (targeted is Item targItem && targItem.VerifyMove(from))
				{
					CrystalRechargeInfo info = CrystalRechargeInfo.Get(targItem.GetType());

					if (info != null)
					{
						from.SendLocalizedMessage(500677); // This crystal cannot be recharged.
						return;
					}
				}

				from.SendLocalizedMessage(1010045); // That receiver crystal is not linked.
			}
		}
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.WriteEncodedInt(0);

		writer.Write(m_Sender);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadEncodedInt();

		m_Sender = reader.ReadItem<BroadcastCrystal>();
	}
}
