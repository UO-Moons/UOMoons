namespace Server.Items;

public interface IArcaneEquip
{
	bool IsArcane { get; }
	int CurArcaneCharges { get; set; }
	int MaxArcaneCharges { get; set; }
	int TempHue { get; set; }
}
