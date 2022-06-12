using Server.Engines.Craft;
namespace Server.Items;

//[Alterable(typeof(DefTinkering), typeof(GargishGlasses), true)]
public class BaseGlasses : BaseArmor, IRepairable
{
	public override int BasePhysicalResistance => 2;
	public override int BaseFireResistance => 4;
	public override int BaseColdResistance => 4;
	public override int BasePoisonResistance => 3;
	public override int BaseEnergyResistance => 2;
	public override int InitHits => Utility.RandomMinMax(36, 48);
	public override int StrReq => 45;
	public override ArmorMaterialType MaterialType => ArmorMaterialType.Leather;
	public override CraftResource DefaultResource => CraftResource.RegularLeather;
	public override ArmorMeditationAllowance DefMedAllowance => ArmorMeditationAllowance.All;
	public CraftSystem RepairSystem => DefTinkering.CraftSystem;

	[Constructable]
	public BaseGlasses()
		: base(0x2FB8)
	{
		Weight = 2.0;
	}

	public BaseGlasses(Serial serial)
		: base(serial)
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
		_ = reader.ReadInt();
	}
}
