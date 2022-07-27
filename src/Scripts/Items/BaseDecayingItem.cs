using System;

namespace Server.Items;

public class BaseDecayingItem : BaseItem
{
	public virtual int Lifespan => 0;
	public virtual bool UseSeconds => true;

	[CommandProperty(AccessLevel.GameMaster)]
	public DecayingItemSocket DecayInfo => GetSocket<DecayingItemSocket>();

	[CommandProperty(AccessLevel.GameMaster)]
	public int TimeLeft
	{
		get
		{
			var socket = GetSocket<DecayingItemSocket>();

			return socket?.Remaining ?? 0;
		}
		set
		{
			var socket = GetSocket<DecayingItemSocket>();

			if (socket != null)
			{
				socket.Expires = DateTime.UtcNow + TimeSpan.FromSeconds(value);
			}
			else if (value > 0)
			{
				AttachSocket(new DecayingItemSocket(value, UseSeconds));
			}

			InvalidateProperties();
		}
	}

	public BaseDecayingItem(int itemId) : base(itemId)
	{
		LootType = LootType.Blessed;

		if (Lifespan > 0)
		{
			AttachSocket(new DecayingItemSocket(Lifespan, UseSeconds));
		}
	}

	public BaseDecayingItem(Serial serial)
		: base(serial)
	{
	}

	public virtual void Decay()
	{
		if (RootParent is Mobile)
		{
			Mobile parent = (Mobile)RootParent;

			if (Name == null)
				parent.SendLocalizedMessage(1072515, "#" + LabelNumber); // The ~1_name~ expired...
			else
				parent.SendLocalizedMessage(1072515, Name); // The ~1_name~ expired...

			Effects.SendLocationParticles(EffectItem.Create(parent.Location, parent.Map, EffectItem.DefaultDuration), 0x3728, 8, 20, 5042);
			Effects.PlaySound(parent.Location, parent.Map, 0x201);
		}
		else
		{
			Effects.SendLocationParticles(EffectItem.Create(Location, Map, EffectItem.DefaultDuration), 0x3728, 8, 20, 5042);
			Effects.PlaySound(Location, Map, 0x201);
		}

		Delete();
	}

	public virtual void SendTimeRemainingMessage(Mobile to)
	{
		var socket = GetSocket<DecayingItemSocket>();

		if (socket != null && socket.Expires > DateTime.UtcNow)
		{
			to.SendLocalizedMessage(1072516,
				$"{Name ?? $"#{LabelNumber}"}\t{socket.Remaining}"); // ~1_name~ will expire in ~2_val~ seconds!
		}
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
