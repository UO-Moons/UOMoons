namespace Server.Items;

public class NightSightPotion : BasePotion
{
	[Constructable]
	public NightSightPotion() : base(0xF06, PotionEffect.Nightsight)
	{
	}

	public NightSightPotion(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadInt();
	}

	public override void Drink(Mobile from)
	{
		if (from.BeginAction(typeof(LightCycle)))
		{
			new LightCycle.NightSightTimer(from).Start();
			from.LightLevel = LightCycle.DungeonLevel / 2;

			from.FixedParticles(0x376A, 9, 32, 5007, EffectLayer.Waist);
			from.PlaySound(0x1E3);

			PlayDrinkEffect(from);

			if (!Engines.ConPVP.DuelContext.IsFreeConsume(from))
				Consume();
		}
		else
		{
			from.SendMessage("You already have nightsight.");
		}
	}
}