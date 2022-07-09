using Server.Mobiles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Engines.NewMagincia;

public class StorageEntry
{
	public int Funds { get; set; }

	public DateTime Expires { get; private set; }

	public Dictionary<Type, int> CommodityTypes { get; } = new();

	public List<BaseCreature> Creatures { get; } = new();

	public StorageEntry(Mobile m, BaseBazaarBroker broker)
	{
		AddInventory(m, broker);
	}

	public void AddInventory(Mobile m, BaseBazaarBroker broker)
	{
		Funds += broker.BankBalance;
		Expires = DateTime.UtcNow + TimeSpan.FromDays(7);

		if (broker is CommodityBroker commodityBroker)
		{
			foreach (var entry in commodityBroker.CommodityEntries.Where(entry => entry.Stock > 0))
			{
				CommodityTypes[entry.CommodityType] = entry.Stock;
			}
		}
		else if (broker is PetBroker petBroker)
		{
			foreach (PetBrokerEntry entry in petBroker.BrokerEntries)
			{
				if (entry.Pet.Map != Map.Internal || !entry.Pet.IsStabled)
				{
					entry.Internalize();
				}

				Creatures.Add(entry.Pet);
			}
		}
	}

	public void RemoveCommodity(Type type, int amount)
	{
		if (CommodityTypes.ContainsKey(type))
		{
			CommodityTypes[type] -= amount;

			if (CommodityTypes[type] <= 0)
				CommodityTypes.Remove(type);
		}
	}

	public void RemovePet(BaseCreature pet)
	{
		if (Creatures.Contains(pet))
			Creatures.Remove(pet);
	}

	public StorageEntry(GenericReader reader)
	{
		int version = reader.ReadInt();

		switch (version)
		{
			case 1:
				Funds = reader.ReadInt();
				Expires = reader.ReadDateTime();

				int count = reader.ReadInt();
				for (int i = 0; i < count; i++)
				{
					Type cType = Assembler.FindTypeByName(reader.ReadString());
					int amount = reader.ReadInt();

					if (cType != null)
						CommodityTypes[cType] = amount;
				}

				count = reader.ReadInt();
				for (int i = 0; i < count; i++)
				{
					if (reader.ReadMobile() is BaseCreature bc)
						Creatures.Add(bc);
				}
				break;
			case 0:
				int type = reader.ReadInt();
				Funds = reader.ReadInt();
				Expires = reader.ReadDateTime();

				switch (type)
				{
					case 0: break;
					case 1:
					{
						int c1 = reader.ReadInt();
						for (int i = 0; i < c1; i++)
						{
							Type cType = Assembler.FindTypeByName(reader.ReadString());
							int amount = reader.ReadInt();

							if (cType != null)
								CommodityTypes[cType] = amount;
						}
						break;
					}
					case 2:
					{
						int c2 = reader.ReadInt();
						for (int i = 0; i < c2; i++)
						{
							if (reader.ReadMobile() is BaseCreature bc)
							{
								Creatures.Add(bc);
							}
						}
						break;
					}
				}
				break;
		}
	}

	public void Serialize(GenericWriter writer)
	{
		writer.Write(1);

		writer.Write(Funds);
		writer.Write(Expires);

		writer.Write(CommodityTypes.Count);
		foreach (KeyValuePair<Type, int> kvp in CommodityTypes)
		{
			writer.Write(kvp.Key.Name);
			writer.Write(kvp.Value);
		}

		writer.Write(Creatures.Count);
		foreach (BaseCreature bc in Creatures)
		{
			writer.Write(bc);
		}
	}
}
