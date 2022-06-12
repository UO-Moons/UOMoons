using System;
using Server;
using Server.Items;
using Server.Mobiles;

public enum HeadType
{
	Regular,
	Duel,
	Tournament
}

public class Head : BaseItem, ICarvable
{
	#region ICarvable Members
	public void Carve(Mobile from, Item item)
	{
		Item brain = new Item(7408);
		Item skull = new Item(6882);
		if (Owner != null)
		{
			brain.Name = "brain of " + Owner.Name;
			skull.Name = "skull of " + Owner.Name;
		}

		if (!(Parent is Container))
		{
			brain.MoveToWorld(GetWorldLocation(), Map);
			skull.MoveToWorld(GetWorldLocation(), Map);
		}
		else
		{
			Container cont = (Container)Parent;
			cont.DropItem(brain);
			cont.DropItem(skull);
		}

		Delete();
	}
	#endregion
	[CommandProperty(AccessLevel.GameMaster)]
	public string PlayerName { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public HeadType HeadType { get; set; }

	private DateTime Created;

	[CommandProperty(AccessLevel.GameMaster)]
	public Mobile Owner { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int MaxBounty { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public TimeSpan Age
	{
		get { return DateTime.UtcNow - Created; }
	}

	public override string DefaultName
	{
		get
		{
			if (PlayerName == null)
				return base.DefaultName;

			return HeadType switch
			{
				HeadType.Duel => string.Format("the head of {0}, taken in a duel", PlayerName),
				HeadType.Tournament => string.Format("the head of {0}, taken in a tournament", PlayerName),
				_ => string.Format("the head of {0}", PlayerName),
			};
		}
	}

	[Constructable]
	public Head()
		: this(null, null)
	{
	}

	[Constructable]
	public Head(Mobile owner, string playerName)
		: this(owner, HeadType.Regular, playerName)
	{
	}

	[Constructable]
	public Head(Mobile owner, HeadType headType, string playerName)
		: base(0x1DA0)
	{
		Owner = owner;
		HeadType = headType;
		PlayerName = playerName;

		Created = DateTime.UtcNow;
		Weight = 1.0;
		LastMoved = DateTime.UtcNow - DefaultDecayTime + TimeSpan.FromMinutes(7.5);

		if (Owner != null && !Owner.Deleted && Owner is PlayerMobile mobile)
			MaxBounty = mobile.Bounty;
	}

	//public Head(Mobile owner) : this(owner == null ? "a head" : $"head of {owner.Name}")
	//{
	//	Owner = owner;
	//}

	public Head(Serial serial)
		: base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
		writer.Write(PlayerName);
		writer.WriteEncodedInt((int)HeadType);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		int version = reader.ReadInt();
		switch (version)
		{
			case 0:
				{
					PlayerName = reader.ReadString();
					HeadType = (HeadType)reader.ReadEncodedInt();
					break;
				}
		}
	}
}
