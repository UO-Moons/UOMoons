using Server.Network;

namespace Server.Items;

public class Dices : BaseItem, ITelekinesisable
{
	[Constructable]
	public Dices() : base(0xFA7)
	{
		Weight = 1.0;
	}

	public Dices(Serial serial) : base(serial)
	{
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (!from.InRange(GetWorldLocation(), 2))
			return;

		Roll(from);
	}

	public void OnTelekinesis(Mobile from)
	{
		Effects.SendLocationParticles(EffectItem.Create(Location, Map, EffectItem.DefaultDuration), 0x376A, 9, 32, 5022);
		Effects.PlaySound(Location, Map, 0x1F5);

		Roll(from);
	}

	private void Roll(IEntity from)
	{
		PublicOverheadMessage(MessageType.Regular, 0, false,
			$"*{from.Name} rolls {Utility.Random(1, 6)}, {Utility.Random(1, 6)}*");
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadInt();
	}
}
