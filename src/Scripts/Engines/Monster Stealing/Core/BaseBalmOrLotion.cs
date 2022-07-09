using Server.Mobiles;
using System;

namespace Server.Items;

public class BaseBalmOrLotion : BaseThieveConsumable, ICommodity
{
	public BaseBalmOrLotion(int itemId) : base(itemId)
	{
		EffectDuration = TimeSpan.FromMinutes(30);
		Weight = 1.0;
	}

	protected override void OnUse(PlayerMobile by)
	{
		if (EffectType == ThieveConsumableEffect.None)
		{
			by.SendMessage("This balm or lotion is corrupted. Please contact a game master");
			return;
		}

		if (CanUse(by, this))
		{
			ApplyEffect(by);
		}
		else
		{
			by.SendLocalizedMessage(1095133);//You are already under the effect of a balm or lotion.
		}
	}


	public BaseBalmOrLotion(Serial serial)
		: base(serial)
	{

	}

	TextDefinition ICommodity.Description => LabelNumber;
	bool ICommodity.IsDeedable => true;


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
