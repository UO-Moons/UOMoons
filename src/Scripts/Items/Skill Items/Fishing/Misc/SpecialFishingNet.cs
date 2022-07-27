using Server.Mobiles;
using Server.Spells;
using Server.Targeting;
using System;
using Server.Misc;

namespace Server.Items;

public class SpecialFishingNet : BaseItem
{
	public override int LabelNumber => 1041079;  // a special fishing net

	[CommandProperty(AccessLevel.GameMaster)]
	public bool InUse { get; set; }

	[Constructable]
	public SpecialFishingNet() : base(0x0DCA)
	{
		Weight = 1.0;

		Hue = 0.01 > Utility.RandomDouble() ? Utility.RandomList(m_Hues) : 0x8A0;
	}

	private static readonly int[] m_Hues = {
		0x09B,
		0x0CD,
		0x0D3,
		0x14D,
		0x1DD,
		0x1E9,
		0x1F4,
		0x373,
		0x451,
		0x47F,
		0x489,
		0x492,
		0x4B5,
		0x8AA
	};

	public SpecialFishingNet(Serial serial) : base(serial)
	{
	}

	public override void GetProperties(ObjectPropertyList list)
	{
		base.GetProperties(list);

		AddNetProperties(list);
	}

	protected virtual void AddNetProperties(ObjectPropertyList list)
	{
		// as if the name wasn't enough..
		list.Add(1017410); // Special Fishing Net
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version

		writer.Write(InUse);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		int version = reader.ReadInt();

		switch (version)
		{
			case 0:
			{
				InUse = reader.ReadBool();

				if (InUse)
					Delete();

				break;
			}
		}

		Stackable = false;
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (InUse)
		{
			from.SendLocalizedMessage(1010483); // Someone is already using that net!
		}
		else if (IsChildOf(from.Backpack))
		{
			from.SendLocalizedMessage(1010484); // Where do you wish to use the net?
			from.BeginTarget(-1, true, TargetFlags.None, OnTarget);
		}
		else
		{
			from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
		}
	}

	public virtual bool RequireDeepWater => true;

	private void OnTarget(Mobile from, object obj)
	{
		if (Deleted || InUse)
			return;

		if (obj is not IPoint3D p3D)
			return;

		Map map = from.Map;

		if (map == null || map == Map.Internal)
			return;

		int x = p3D.X, y = p3D.Y, z = map.GetAverageZ(x, y); // OSI just takes the targeted Z

		if (!from.InRange(p3D, 6))
		{
			from.SendLocalizedMessage(500976); // You need to be closer to the water to fish!
		}
		else if (!from.InLOS(obj))
		{
			from.SendLocalizedMessage(500979); // You cannot see that location.
		}
		else if (RequireDeepWater ? FullValidation(map, x, y) : Helpers.ValidateDeepWater(map, x, y) || Helpers.ValidateUndeepWater(map, obj, ref z))
		{
			Point3D p = new(x, y, z);

			if (GetType() == typeof(SpecialFishingNet))
			{
				for (int i = 1; i < Amount; ++i) // these were stackable before, doh
					from.AddToBackpack(new SpecialFishingNet());
			}

			InUse = true;
			Movable = false;
			MoveToWorld(p, map);

			SpellHelper.Turn(from, p);
			from.Animate(12, 5, 1, true, false, 0);

			Effects.SendLocationEffect(p, map, 0x352D, 16, 4);
			Effects.PlaySound(p, map, 0x364);

			Timer.DelayCall(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.25), 14, new TimerStateCallback(DoEffect), new object[] { p, 0, from });

			from.SendLocalizedMessage(RequireDeepWater ? 1010487 : 1074492); // You plunge the net into the sea... / You plunge the net into the water...
		}
		else
		{
			from.SendLocalizedMessage(RequireDeepWater ? 1010485 : 1074491); // You can only use this net in deep water! / You can only use this net in water!
		}
	}

	private void DoEffect(object state)
	{
		if (Deleted)
			return;

		object[] states = (object[])state;

		Point3D p = (Point3D)states[0];
		int index = (int)states[1];
		Mobile from = (Mobile)states[2];

		states[1] = ++index;

		switch (index)
		{
			case 1:
				Effects.SendLocationEffect(p, Map, 0x352D, 16, 4);
				Effects.PlaySound(p, Map, 0x364);
				break;
			case <= 7:
			case 14:
			{
				if (RequireDeepWater)
				{
					for (int i = 0; i < 3; ++i)
					{
						int x, y;

						switch (Utility.Random(8))
						{
							default:
								x = -1; y = -1; break;
							case 1: x = -1; y = 0; break;
							case 2: x = -1; y = +1; break;
							case 3: x = 0; y = -1; break;
							case 4: x = 0; y = +1; break;
							case 5: x = +1; y = -1; break;
							case 6: x = +1; y = 0; break;
							case 7: x = +1; y = +1; break;
						}

						Effects.SendLocationEffect(new Point3D(p.X + x, p.Y + y, p.Z), Map, 0x352D, 16, 4);
					}
				}
				else
				{
					Effects.SendLocationEffect(p, Map, 0x352D, 16, 4);
				}

				if (Utility.RandomBool())
					Effects.PlaySound(p, Map, 0x364);

				if (index == 14)
					FinishEffect(p, Map, from);
				else
					Z -= 1;
				break;
			}
		}
	}

	protected virtual int GetSpawnCount()
	{
		int count = Utility.RandomMinMax(1, 3);

		if (Hue != 0x8A0)
			count += Utility.RandomMinMax(1, 2);

		return count;
	}

	protected static void Spawn(Point3D p, Map map, BaseCreature spawn)
	{
		if (map == null)
		{
			spawn.Delete();
			return;
		}

		int x = p.X, y = p.Y;

		for (int j = 0; j < 20; ++j)
		{
			int tx = p.X - 2 + Utility.Random(5);
			int ty = p.Y - 2 + Utility.Random(5);

			LandTile t = map.Tiles.GetLandTile(tx, ty);

			if (t.Z == p.Z && (t.Id is >= 0xA8 and <= 0xAB || t.Id is >= 0x136 and <= 0x137) && !SpellHelper.CheckMulti(new Point3D(tx, ty, p.Z), map))
			{
				x = tx;
				y = ty;
				break;
			}
		}

		spawn.MoveToWorld(new Point3D(x, y, p.Z), map);

		if (spawn is Kraken && 0.2 > Utility.RandomDouble())
			spawn.PackItem(new MessageInABottle(map == Map.Felucca ? Map.Felucca : Map.Trammel));
	}

	protected virtual void FinishEffect(Point3D p, Map map, Mobile from)
	{
		from.RevealingAction();

		int count = GetSpawnCount();

		for (int i = 0; map != null && i < count; ++i)
		{
			BaseCreature spawn = Utility.Random(4) switch
			{
				1 => new DeepSeaSerpent(),
				2 => new WaterElemental(),
				3 => new Kraken(),
				_ => new SeaSerpent()
			};

			Spawn(p, map, spawn);

			spawn.Combatant = from;
		}

		Delete();
	}

	public static bool FullValidation(Map map, int x, int y)
	{
		bool valid = Helpers.ValidateDeepWater(map, x, y);

		for (int j = 1, offset = 5; valid && j <= 5; ++j, offset += 5)
		{
			if (!Helpers.ValidateDeepWater(map, x + offset, y + offset))
				valid = false;
			else if (!Helpers.ValidateDeepWater(map, x + offset, y - offset))
				valid = false;
			else if (!Helpers.ValidateDeepWater(map, x - offset, y + offset))
				valid = false;
			else if (!Helpers.ValidateDeepWater(map, x - offset, y - offset))
				valid = false;
		}

		return valid;
	}
}

public class FabledFishingNet : SpecialFishingNet
{
	public override int LabelNumber => 1063451;  // a fabled fishing net

	[Constructable]
	public FabledFishingNet()
	{
		Hue = 0x481;
	}

	protected override void AddNetProperties(ObjectPropertyList list)
	{
	}

	protected override int GetSpawnCount()
	{
		return base.GetSpawnCount() + 4;
	}

	protected override void FinishEffect(Point3D p, Map map, Mobile from)
	{
		Spawn(p, map, new Leviathan(from));

		base.FinishEffect(p, map, from);
	}

	public FabledFishingNet(Serial serial) : base(serial)
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
