namespace Server.Items;

[Flipable(0x19BC, 0x19BD)]
public partial class BaseCostume : BaseShield
{
	public virtual string CreatureName { get; }

	[CommandProperty(AccessLevel.GameMaster)]
	private bool Transformed { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int CostumeBody { get; protected set; }

	[CommandProperty(AccessLevel.GameMaster)]
	private int CostumeHue { get; set; } = -1;

	public BaseCostume(string creatureName, int costumeHue, int costumeBody)
		: base(0x19BC)
	{
		CreatureName = creatureName;
		CostumeHue = costumeHue;
		CostumeBody = costumeBody;
		Resource = CraftResource.None;
		Attributes.SpellChanneling = 1;
		Layer = Layer.FirstValid;
		Weight = 4.0;
		StrRequirement = 10;
	}

	public BaseCostume(Serial serial)
		: base(serial)
	{
	}

	private bool EnMask(Mobile from)
	{
		if (from.Mounted || from.Flying) // You cannot use this while mounted or flying. 
		{
			from.SendLocalizedMessage(1010097);
		}
		else if (from.IsBodyMod || from.HueMod > -1)
		{
			from.SendLocalizedMessage(1158010); // You cannot use that item in this form.
		}
		else
		{
			from.BodyMod = CostumeBody;
			from.HueMod = CostumeHue;
			Transformed = true;

			return true;
		}

		return false;
	}

	private void DeMask(Mobile from)
	{
		from.BodyMod = 0;
		from.HueMod = -1;
		Transformed = false;
	}

	public virtual bool Dye(Mobile from, DyeTub sender)
	{
		if (Deleted)
			return false;

		if (RootParent is Mobile && from != RootParent)
			return false;

		Hue = sender.DyedHue;
		return true;
	}

	public override bool OnEquip(Mobile from)
	{
		return !Transformed ? EnMask(from) : base.OnEquip(from);
	}

	public override void OnRemoved(IEntity parent)
	{
		if (parent is Mobile mobile && Transformed)
		{
			DeMask(mobile);
		}

		base.OnRemoved(parent);
	}

	public static void OnDamaged(Mobile m)
	{
		if (m.FindItemOnLayer(Layer.FirstValid) is BaseCostume costume)
		{
			m.AddToBackpack(costume);
		}
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0);
		writer.Write(CostumeBody);
		writer.Write(CostumeHue);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		int version = reader.ReadInt();

		switch (version)
		{
			case 0:
				CostumeBody = reader.ReadInt();
				CostumeHue = reader.ReadInt();
				break;
		}

		if (RootParent is Mobile mobile && mobile.Items.Contains(this))
		{
			EnMask(mobile);
		}
	}
}
