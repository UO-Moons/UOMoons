using Server.Mobiles;
using System;
using System.Collections.Generic;
using System.Linq;
using Server.Engines.Plants;

namespace Server.Items;

public class ScouringToxin : Item, IUsesRemaining, ICommodity
{
	public override int LabelNumber => 1112292;  // scouring toxin

	private int m_UsesRemaining;

	[CommandProperty(AccessLevel.GameMaster)]
	public int UsesRemaining { get => m_UsesRemaining;
		set { m_UsesRemaining = value; if (m_UsesRemaining <= 0) Delete(); else InvalidateProperties(); } }

	public bool ShowUsesRemaining { get => false;
		set { { } } }

	[Constructable]
	public ScouringToxin()
		: this(10)
	{
	}

	[Constructable]
	public ScouringToxin(int amount)
		: base(0x1848)
	{
		m_UsesRemaining = amount;
	}

	public override void AddNameProperty(ObjectPropertyList list)
	{
		if (m_UsesRemaining <= 1)
			list.Add(LabelNumber);
		else
			list.Add(1050039, "{0}\t#{1}", m_UsesRemaining, LabelNumber); // ~1_NUMBER~ ~2_ITEMNAME~
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (!IsChildOf(from.Backpack))
			return;
		from.SendLocalizedMessage(1112348); // Which item do you wish to scour?
		from.BeginTarget(-1, false, Targeting.TargetFlags.None, OnTarget);
	}

	private void OnTarget(Mobile from, object targeted)
	{
		if (targeted is Item item)
		{
			if (item.Parent is Mobile)
			{
				from.SendLocalizedMessage(1112350); // You cannot scour items that are being worn!
			}
			else if (item.IsLockedDown || item.IsSecure)
			{
				from.SendLocalizedMessage(1112351); // You may not scour items which are locked down.
			}
			else if (item.QuestItem)
			{
				from.SendLocalizedMessage(1151837); // You may not scour toggled quest items.
			}
			else if (item is DryReeds reed1)
			{
				if (from is not PlayerMobile { BasketWeaving: true } mobile)
				{
					from.SendLocalizedMessage(1112253); //You haven't learned basket weaving. Perhaps studying a book would help!
				}
				else
				{
					Container cont = mobile.Backpack;

					PlantHue hue = reed1.PlantHue;

					if (!reed1.IsChildOf(mobile.Backpack))
						mobile.SendLocalizedMessage(1116249); //That must be in your backpack for you to use it.
					else if (cont != null)
					{
						Item[] items = cont.FindItemsByType(typeof(DryReeds));
						List<Item> list = new();
						int total = 0;

						foreach (Item it in items)
						{
							if (it is not DryReeds check)
								continue;
							if (reed1.PlantHue != check.PlantHue)
								continue;
							total += check.Amount;
							list.Add(check);
						}

						int toConsume = 2;

						if (list.Count > 0 && total > 1)
						{
							foreach (Item it in list)
							{
								if (it.Amount >= toConsume)
								{
									it.Consume(toConsume);
									toConsume = 0;
								}
								else if (it.Amount < toConsume)
								{
									it.Delete();
									toConsume -= it.Amount;
								}

								if (toConsume <= 0)
									break;
							}

							SoftenedReeds sReed = new(hue);

							if (!mobile.Backpack.TryDropItem(mobile, sReed, false))
								sReed.MoveToWorld(mobile.Location, mobile.Map);

							m_UsesRemaining--;

							if (m_UsesRemaining <= 0)
								Delete();
							else
								InvalidateProperties();

							mobile.PlaySound(0x23E);
						}
						else
							mobile.SendLocalizedMessage(1112250); //You don't have enough of this type of dry reeds to make that.
					}
				}
			}
			else if (BasePigmentsOfTokuno.IsValidItem(item))
			{
				from.PlaySound(0x23E);

				item.Hue = 0;

				m_UsesRemaining--;

				if (m_UsesRemaining <= 0)
					Delete();
				else
					InvalidateProperties();
			}
			else
			{
				from.SendLocalizedMessage(1112349); // You cannot scour that!
			}
		}
		else
		{
			from.SendLocalizedMessage(1112349); // You cannot scour that!
		}
	}

	private static bool IsInTypeList(Type t, IEnumerable<Type> list) => list.Any(t1 => t1 == t);

	public ScouringToxin(Serial serial)
		: base(serial)
	{
	}

	TextDefinition ICommodity.Description => LabelNumber;
	bool ICommodity.IsDeedable => true;

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
		writer.Write(m_UsesRemaining);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadInt();
		m_UsesRemaining = reader.ReadInt();
	}
}
