namespace Server.Engines.PlayerDonation;

public class DonationGift
{
	public DonationGift(int id, int type, string name)
	{
		Id = id;
		Type = type;
		Name = name;
	}

	public int Id { get; }

	private int Type { get; }

	public string Name { get; }
}
