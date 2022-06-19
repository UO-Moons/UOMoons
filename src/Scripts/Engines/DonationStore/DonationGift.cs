namespace Server.Engines.PlayerDonation;

public class DonationGift
{
	public DonationGift(int id, int type, string name)
	{
		Id = id;
		Type = type;
		Name = name;
	}

	public int Id { get; set; }

	public int Type { get; set; }

	public string Name { get; set; }
}
