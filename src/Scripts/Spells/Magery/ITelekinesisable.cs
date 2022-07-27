namespace Server;

public interface ITelekinesisable : IPoint3D
{
	void OnTelekinesis(Mobile from);
}
