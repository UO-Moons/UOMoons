namespace Server.Regions;

public class LenleyRegion : BaseRegion
{
	public LenleyRegion(IEntity lenley)
		: base(null, lenley.Map, Find(lenley.Location, lenley.Map), new Rectangle2D(lenley.Location.X - 2, lenley.Location.Y - 2, 5, 5))
	{
	}

	public override void OnEnter(Mobile m)
	{
		m.SendLocalizedMessage(1075014); // Psst!  Lenley isn't seen.  You help Lenley?
	}
}
