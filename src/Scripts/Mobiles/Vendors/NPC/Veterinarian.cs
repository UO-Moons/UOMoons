using System.Collections.Generic;

namespace Server.Mobiles;

public class Veterinarian : BaseVendor
{
	private readonly List<SbInfo> _mSbInfos = new();
	protected override List<SbInfo> SbInfos => _mSbInfos;

	[Constructable]
	public Veterinarian() : base("the vet")
	{
		Job = JobFragment.vet;
		Karma = Utility.RandomMinMax(13, -45);
		SetSkill(SkillName.AnimalLore, 85.0, 100.0);
		SetSkill(SkillName.Veterinary, 90.0, 100.0);
	}

	public override void InitSbInfo()
	{
		_mSbInfos.Add(new SbVeterinarian());
	}

	public Veterinarian(Serial serial) : base(serial)
	{
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
