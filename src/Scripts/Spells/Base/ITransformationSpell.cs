namespace Server.Spells;

public interface ITransformationSpell
{
	int Body { get; }
	int Hue { get; }

	int PhysResistOffset { get; }
	int FireResistOffset { get; }
	int ColdResistOffset { get; }
	int PoisResistOffset { get; }
	int NrgyResistOffset { get; }

	double TickRate { get; }
	void OnTick(Mobile m);

	void DoEffect(Mobile m);
	void RemoveEffect(Mobile m);
}
