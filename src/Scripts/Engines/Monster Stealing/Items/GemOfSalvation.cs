using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using System;

namespace Server.Items;

[TypeAlias("drNO.ThieveItems.GemOfSalvation")]
public class GemOfSalvation : Item
{
	public override int LabelNumber => 1094939;  // Gem of Salvation

	[Constructable]
	public GemOfSalvation()
		: base(0x1F13)
	{
		Hue = 286;
		LootType = LootType.Blessed;
	}

	public static void Initialize()
	{
		EventSink.OnPlayerDeath += PlayerDeath;
	}

	public static void PlayerDeath(PlayerDeathEventArgs args)
	{
		PlayerMobile pm = (PlayerMobile)args.Mobile;

		if (pm is {Backpack: { }})
		{
			GemOfSalvation gem = pm.Backpack.FindItemByType<GemOfSalvation>();

			if (gem != null)
			{
				Timer.DelayCall(TimeSpan.FromSeconds(2), () =>
				{
					if (DateTime.UtcNow < pm.GemOfSalvationUse)
					{
						TimeSpan left = pm.GemOfSalvationUse - DateTime.UtcNow;

						if (left >= TimeSpan.FromMinutes(1.0))
							pm.SendLocalizedMessage(1095131, ((left.Hours * 60) + left.Minutes).ToString()); // Your spirit lacks cohesion. You must wait ~1_minutes~ minutes before invoking the power of a Gem of Salvation.
						else
							pm.SendLocalizedMessage(1095130, left.Seconds.ToString()); // Your spirit lacks cohesion. You must wait ~1_seconds~ seconds before invoking the power of a Gem of Salvation.
					}
					else
					{
						pm.CloseGump(typeof(ResurrectGump));
						pm.SendGump(new GemResurrectGump(pm, gem));
					}
				});
			}
		}
	}

	public GemOfSalvation(Serial serial)
		: base(serial)
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
}

public class GemResurrectGump : ResurrectGump
{
	private readonly GemOfSalvation _gem;
	private readonly PlayerMobile _mobile;

	public GemResurrectGump(PlayerMobile pm, GemOfSalvation gem)
		: base(pm, ResurrectMessage.GemOfSalvation)
	{
		_gem = gem;
		_mobile = pm;
	}

	public override void OnResponse(NetState state, RelayInfo info)
	{
		_mobile.CloseGump(typeof(ResurrectGump));

		if (info.ButtonID == 1 && !_gem.Deleted && _gem.IsChildOf(_mobile.Backpack))
		{
			if (_mobile.Map == null || !_mobile.Map.CanFit(_mobile.Location, 16, false, false))
			{
				_mobile.SendLocalizedMessage(502391); // Thou can not be resurrected there!
				return;
			}

			_mobile.PlaySound(0x214);
			_mobile.Resurrect();

			_mobile.SendLocalizedMessage(1095132); // The gem infuses you with its power and is destroyed in the process.

			_gem.Delete();

			_mobile.GemOfSalvationUse = DateTime.UtcNow + TimeSpan.FromHours(6);
		}
	}
}
