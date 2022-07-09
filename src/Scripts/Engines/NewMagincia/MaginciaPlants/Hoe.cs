using Server.Engines.Plants;
using Server.Network;
using Server.Targeting;
using System;

namespace Server.Items;

public class Hoe : BaseAxe, IUsesRemaining
{
	public override int LabelNumber => 1150482;  // hoe

	[Constructable]
	public Hoe()
		: base(0xE86)
	{
		Hue = 2524;
		Weight = 11.0;
		UsesRemaining = 50;
		ShowUsesRemaining = true;
	}

	public override WeaponAbility PrimaryAbility => WeaponAbility.DoubleStrike;
	public override WeaponAbility SecondaryAbility => WeaponAbility.Disarm;
	public override int StrReq => 50;
	public override int MinDamageBase => 12;
	public override int MaxDamageBase => 16;
	public override float SpeedBase => 3.00f;
	public override int InitMinHits => 31;
	public override int InitMaxHits => 60;

	public override WeaponAnimation DefAnimation => WeaponAnimation.Slash1H;

	public override void OnDoubleClick(Mobile from)
	{
		if (IsChildOf(from.Backpack))
		{
			from.Target = new InternalTarget(this);
		}
	}

	private class InternalTarget : Target
	{
		private readonly Hoe _hoe;

		public InternalTarget(Hoe hoe)
			: base(2, true, TargetFlags.None)
		{
			_hoe = hoe;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (!MaginciaPlantSystem.Enabled)
			{
				from.SendMessage("Magincia plant placement is currently disabled.");
				return;
			}

			Map map = from.Map;

			if (targeted is LandTarget landTarget && map != null)
			{
				Region r = Region.Find(landTarget.Location, map);

				if (r != null && r.IsPartOf("Magincia") && (landTarget.Name == "dirt" || landTarget.Name == "grass"))
				{
					if (MaginciaPlantSystem.CanAddPlant(from, landTarget.Location))
					{
						if (!MaginciaPlantSystem.CheckDelay(from))
						{
							return;
						}

						if (from.Mounted || from.Flying)
						{
							from.SendLocalizedMessage(501864); // You can't mine while riding.
						}
						else if (from.IsBodyMod && !from.Body.IsHuman)
						{
							from.SendLocalizedMessage(501865); // You can't mine while polymorphed.
						}
						else
						{
							_hoe.UsesRemaining--;

							from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1150492); // You till a small area to plant.                                
							from.Animate(AnimationType.Attack, 3);

							MaginciaPlantItem dirt = new()
							{
								Owner = from
							};
							dirt.StartTimer();

							MaginciaPlantSystem.OnPlantPlanted(from, from.Map);

							Timer.DelayCall(TimeSpan.FromSeconds(.7), new TimerStateCallback(MoveItem_Callback), new object[] { dirt, landTarget.Location, map });
						}
					}
				}
				else
				{
					from.SendLocalizedMessage(1150457); // The ground here is not good for gardening.
				}
			}
		}

		private static void MoveItem_Callback(object o)
		{
			if (o is object[] objs)
			{
				Item dirt = objs[0] as Item;
				Point3D p = (Point3D)objs[1];
				Map map = objs[2] as Map;

				dirt?.MoveToWorld(p, map);
			}
		}
	}

	public Hoe(Serial serial)
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
		reader.ReadInt();
	}
}
