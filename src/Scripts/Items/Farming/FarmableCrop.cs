using Server.Network;
using System;

namespace Server.Items;

public abstract class FarmableCrop : BaseItem
{
	private bool m_Picked;

	public abstract Item GetCropObject();
	public abstract int GetPickedId();

	public FarmableCrop(int itemId) : base(itemId)
	{
		Movable = false;
	}

	public override void OnDoubleClick(Mobile from)
	{
		Map map = Map;
		Point3D loc = Location;

		if (Parent != null || Movable || IsLockedDown || IsSecure || map == null || map == Map.Internal)
			return;

		if (!from.InRange(loc, 2) || !from.InLOS(this))
			from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
		else if (!m_Picked)
			OnPicked(loc, map);
	}

	public virtual void OnPicked(Point3D loc, Map map)
	{
		ItemId = GetPickedId();

		Item spawn = GetCropObject();

		spawn?.MoveToWorld(loc, map);

		m_Picked = true;

		Unlink();

		Timer.DelayCall(TimeSpan.FromMinutes(5.0), Delete);
	}

	private void Unlink()
	{
		ISpawner se = Spawner;

		if (se == null)
			return;

		Spawner.Remove(this);
		Spawner = null;

	}

	public FarmableCrop(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.WriteEncodedInt(0); // version

		writer.Write(m_Picked);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		int version = reader.ReadEncodedInt();

		m_Picked = version switch
		{
			0 => reader.ReadBool(),
			_ => m_Picked
		};

		if (!m_Picked)
			return;
		Unlink();
		Delete();
	}
}
