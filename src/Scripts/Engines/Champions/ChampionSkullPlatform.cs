using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Champions;

public class ChampionSkullPlatform : BaseAddon
{
	private ChampionSkullBrazier _mPower, _mEnlightenment, _mVenom, _mPain, _mGreed, _mDeath;

	[Constructable]
	public ChampionSkullPlatform()
	{
		AddComponent(new AddonComponent(0x71A), -1, -1, -1);
		AddComponent(new AddonComponent(0x709), 0, -1, -1);
		AddComponent(new AddonComponent(0x709), 1, -1, -1);
		AddComponent(new AddonComponent(0x709), -1, 0, -1);
		AddComponent(new AddonComponent(0x709), 0, 0, -1);
		AddComponent(new AddonComponent(0x709), 1, 0, -1);
		AddComponent(new AddonComponent(0x709), -1, 1, -1);
		AddComponent(new AddonComponent(0x709), 0, 1, -1);
		AddComponent(new AddonComponent(0x71B), 1, 1, -1);

		AddComponent(new AddonComponent(0x50F), 0, -1, 4);
		AddComponent(_mPower = new ChampionSkullBrazier(this, ChampionSkullType.Power), 0, -1, 5);

		AddComponent(new AddonComponent(0x50F), 1, -1, 4);
		AddComponent(_mEnlightenment = new ChampionSkullBrazier(this, ChampionSkullType.Enlightenment), 1, -1, 5);

		AddComponent(new AddonComponent(0x50F), -1, 0, 4);
		AddComponent(_mVenom = new ChampionSkullBrazier(this, ChampionSkullType.Venom), -1, 0, 5);

		AddComponent(new AddonComponent(0x50F), 1, 0, 4);
		AddComponent(_mPain = new ChampionSkullBrazier(this, ChampionSkullType.Pain), 1, 0, 5);

		AddComponent(new AddonComponent(0x50F), -1, 1, 4);
		AddComponent(_mGreed = new ChampionSkullBrazier(this, ChampionSkullType.Greed), -1, 1, 5);

		AddComponent(new AddonComponent(0x50F), 0, 1, 4);
		AddComponent(_mDeath = new ChampionSkullBrazier(this, ChampionSkullType.Death), 0, 1, 5);

		AddonComponent comp = new LocalizedAddonComponent(0x20D2, 1049495)
		{
			Hue = 0x482
		};
		AddComponent(comp, 0, 0, 5);

		comp = new LocalizedAddonComponent(0x0BCF, 1049496)
		{
			Hue = 0x482
		};
		AddComponent(comp, 0, 2, -7);

		comp = new LocalizedAddonComponent(0x0BD0, 1049497)
		{
			Hue = 0x482
		};
		AddComponent(comp, 2, 0, -7);
	}

	public ChampionSkullPlatform(Serial serial)
		: base(serial)
	{
	}

	public void Validate()
	{
		if (!Validate(_mPower) || !Validate(_mEnlightenment) || !Validate(_mVenom) || !Validate(_mPain) || !Validate(_mGreed) || !Validate(_mDeath))
			return;

		Mobile harrower = Harrower.Spawn(new Point3D(X, Y, Z + 6), Map);

		if (harrower == null)
			return;

		Clear(_mPower);
		Clear(_mEnlightenment);
		Clear(_mVenom);
		Clear(_mPain);
		Clear(_mGreed);
		Clear(_mDeath);
	}

	private static void Clear(ChampionSkullBrazier brazier)
	{
		if (brazier == null)
			return;

		Effects.SendBoltEffect(brazier);

		brazier.Skull?.Delete();
	}

	private static bool Validate(ChampionSkullBrazier brazier)
	{
		return brazier?.Skull != null && !brazier.Skull.Deleted;
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
		writer.Write(_mPower);
		writer.Write(_mEnlightenment);
		writer.Write(_mVenom);
		writer.Write(_mPain);
		writer.Write(_mGreed);
		writer.Write(_mDeath);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		var version = reader.ReadInt();

		switch (version)
		{
			case 0:
			{
				_mPower = reader.ReadItem() as ChampionSkullBrazier;
				_mEnlightenment = reader.ReadItem() as ChampionSkullBrazier;
				_mVenom = reader.ReadItem() as ChampionSkullBrazier;
				_mPain = reader.ReadItem() as ChampionSkullBrazier;
				_mGreed = reader.ReadItem() as ChampionSkullBrazier;
				_mDeath = reader.ReadItem() as ChampionSkullBrazier;

				break;
			}
		}
	}
}
