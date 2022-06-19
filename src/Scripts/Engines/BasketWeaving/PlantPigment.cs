using Server.Engines.Plants;
using Server.Targeting;

namespace Server.Items;

public class PlantPigment : Item, IPigmentHue
{
	private PlantPigmentHue _mHue;
	[Constructable]
	public PlantPigment()
		: this(PlantPigmentHue.None)
	{
	}

	[Constructable]
	public PlantPigment(PlantPigmentHue hue)
		: base(0x0F02)
	{
		Weight = 1.0;
		PigmentHue = hue;
	}

	[Constructable]
	public PlantPigment(PlantHue hue)
		: base(0x0F02)
	{
		Weight = 1.0;
		PigmentHue = PlantPigmentHueInfo.HueFromPlantHue(hue);
	}

	public PlantPigment(Serial serial)
		: base(serial)
	{
	}

	public static bool RetainsColorFrom => true;
	[CommandProperty(AccessLevel.GameMaster)]
	public PlantPigmentHue PigmentHue
	{
		get => _mHue;
		set
		{
			_mHue = value;
			//set any invalid pigment hue to Plain
			if (_mHue != PlantPigmentHueInfo.GetInfo(_mHue).PlantPigmentHue)
				_mHue = PlantPigmentHue.Plain;
			Hue = PlantPigmentHueInfo.GetInfo(_mHue).Hue;
			InvalidateProperties();
		}
	}
	public override int LabelNumber => 1112132;// plant pigment
	public override void AddNameProperty(ObjectPropertyList list)
	{
		PlantPigmentHueInfo info = PlantPigmentHueInfo.GetInfo(_mHue);
		int cliloc;

		if (Amount > 1)
		{
			cliloc = info.IsBright() ? 1113271 : 1113270;
			list.Add(cliloc, $"{Amount}\t#{info.Name}");
		}
		else
		{
			cliloc = info.IsBright() ? 1112134 : 1112133;
			list.Add(cliloc, $"#{info.Name}");
		}
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
		writer.Write((int)_mHue);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		var version = reader.ReadInt();

		_mHue = version switch
		{
			0 => (PlantPigmentHue) reader.ReadInt(),
			_ => _mHue
		};
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (!PlantPigmentHueInfo.IsMixable(PigmentHue))
			from.SendLocalizedMessage(1112125); // This pigment is saturated and cannot be mixed further.
		else
		{
			from.SendLocalizedMessage(1112123); // Which plant pigment do you wish to mix this with?

			from.Target = new InternalTarget(this);
		}
	}

	private class InternalTarget : Target
	{
		private readonly PlantPigment _mItem;
		public InternalTarget(PlantPigment item)
			: base(2, false, TargetFlags.None)
		{
			_mItem = item;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (_mItem.Deleted)
				return;

			if (targeted is not PlantPigment pigment)
			{
				from.SendLocalizedMessage(1112124); // You may only mix this with another non-saturated plant pigment.
				return;
			}

			if (from.Skills[SkillName.Alchemy].Base < 75.0 && from.Skills[SkillName.Cooking].Base < 75.0)
			{
				from.SendLocalizedMessage(1112214); // You lack the alchemy or cooking skills to mix plant pigments.
			}
			else if ((pigment.PigmentHue is PlantPigmentHue.White or PlantPigmentHue.Black || _mItem.PigmentHue is PlantPigmentHue.White or PlantPigmentHue.Black) &&
			         from.Skills[SkillName.Alchemy].Base < 100.0 &&
			         from.Skills[SkillName.Cooking].Base < 100.0)
			{
				from.SendLocalizedMessage(1112213); // You lack the alchemy or cooking skills to mix so unstable a pigment.
			}
			else if (_mItem.PigmentHue == pigment.PigmentHue)
			{
				from.SendLocalizedMessage(1112242); // You decide not to waste pigments by mixing two identical colors.
			}
			else if ((_mItem.PigmentHue & ~(PlantPigmentHue.Bright | PlantPigmentHue.Dark | PlantPigmentHue.Ice)) ==
			         (pigment.PigmentHue & ~(PlantPigmentHue.Bright | PlantPigmentHue.Dark | PlantPigmentHue.Ice)))
			{
				from.SendLocalizedMessage(1112243); // You decide not to waste pigments by mixing variations of the same hue.
			}
			else if ((PlantPigmentHue.White == _mItem.PigmentHue && PlantPigmentHueInfo.IsBright(pigment.PigmentHue)) ||
			         (PlantPigmentHue.White == pigment.PigmentHue && PlantPigmentHueInfo.IsBright(_mItem.PigmentHue)))
			{
				from.SendLocalizedMessage(1112241); // This pigment is too diluted to be faded further.
			}
			else if (!PlantPigmentHueInfo.IsMixable(pigment.PigmentHue))
				from.SendLocalizedMessage(1112125); // This pigment is saturated and cannot be mixed further.
			else
			{
				PlantPigmentHue newHue = PlantPigmentHueInfo.Mix(_mItem.PigmentHue, pigment.PigmentHue);
				if (PlantPigmentHue.None == newHue)
					from.SendLocalizedMessage(1112125); // This pigment is saturated and cannot be mixed further.
				else
				{
					pigment.PigmentHue = newHue;
					EmptyBottle bottle = new();
					bottle.MoveToWorld(_mItem.Location, _mItem.Map);
					_mItem.Delete();
					from.PlaySound(0x240);
				}
			}
		}
	}
}
