namespace Server.Items;

public enum DurabilityLevel
{
	Regular,
	Durable,
	Substantial,
	Massive,
	Fortified,
	Indestructible
}

public interface IImbuableEquipement
{
	int TimesImbued { get; set; }
	bool IsImbued { get; set; }

	int[] BaseResists { get; }
	void OnAfterImbued(Mobile m, int mod, int value);
}

public interface ICombatEquipment : IImbuableEquipement
{
	ItemPower ItemPower { get; set; }
	ReforgedPrefix ReforgedPrefix { get; set; }
	ReforgedSuffix ReforgedSuffix { get; set; }
	bool PlayerConstructed { get; set; }
}

public interface IOwnerRestricted
{
	Mobile Owner { get; set; }
	string OwnerName { get; set; }
}
