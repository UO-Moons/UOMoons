using Server.Engines.Craft;

namespace Server.Items;

public interface IRevealableItem
{
	bool CheckReveal(Mobile m);
	bool CheckPassiveDetect(Mobile m);
	void OnRevealed(Mobile m);

	bool CheckWhenHidden { get; }
}
