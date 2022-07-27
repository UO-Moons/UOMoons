using Server.Network;
using System;

namespace Server.Items;

public class Guillotine : BaseItem
{
	[Constructable]
	public Guillotine()
		: base(4656)
	{
		Movable = false;
	}

	private DateTime _mNextUse;

	public override void OnDoubleClick(Mobile from)
	{
		if (!from.InRange(GetWorldLocation(), 2) || !from.InLOS(this))
		{
			from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that
		}
		else if (Visible && (ItemId == 4656 || ItemId == 4702) && DateTime.UtcNow >= _mNextUse)
		{
			Point3D p = GetWorldLocation();

			if (1 > Utility.Random(Math.Max(Math.Abs(from.X - p.X), Math.Abs(from.Y - p.Y))))
			{
				Effects.PlaySound(from.Location, from.Map, from.GetHurtSound());
				from.PublicOverheadMessage(MessageType.Regular, from.SpeechHue, true, "Ouch!");
				Spells.SpellHelper.Damage(TimeSpan.FromSeconds(0.5), from, Utility.Dice(2, 10, 5));
			}

			Effects.PlaySound(GetWorldLocation(), Map, 0x387);

			Timer.DelayCall(TimeSpan.FromSeconds(0.25), Down1);
			Timer.DelayCall(TimeSpan.FromSeconds(0.50), Down2);

			Timer.DelayCall(TimeSpan.FromSeconds(5.00), BackUp);

			_mNextUse = DateTime.UtcNow + TimeSpan.FromSeconds(10.0);

		}
	}

	public void Down1()
	{
		ItemId = ItemId == 4656 ? 4678 : 4712;
	}

	private void Down2()
	{
		ItemId = ItemId == 4678 ? 4679 : 4713;

		Point3D p = GetWorldLocation();
		Map f = Map;

		if (f == null)
			return;

		new Blood(4650).MoveToWorld(p, f);

		for (int i = 0; i < 4; ++i)
		{
			int x = p.X - 2 + Utility.Random(5);
			int y = p.Y - 2 + Utility.Random(5);
			int z = p.Z;

			if (!f.CanFit(x, y, z, 1, false, false, true))
			{
				z = f.GetAverageZ(x, y);

				if (!f.CanFit(x, y, z, 1, false, false, true))
					continue;
			}

			new Blood().MoveToWorld(new Point3D(x, y, z), f);
		}
	}

	private void BackUp()
	{
		ItemId = ItemId switch
		{
			4678 or 4679 => 4656,
			4712 or 4713 => 4702,
			_ => ItemId
		};
	}

	public Guillotine(Serial serial)
		: base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write((byte)0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadByte();

		ItemId = ItemId switch
		{
			4678 or 4679 => 4656,
			4712 or 4713 => 4702,
			_ => ItemId
		};
	}
}
