using Server.Mobiles;
using Server.Multis;
using Server.Targeting;

namespace Server.Items
{
	public sealed class ParrotItem : Item
	{
		private static readonly int[] MHues = {
			0x3, 0xD, 0x13, 0x1C, 0x21, 0x30, 0x3F, 0x44, 0x59, 0x62, 0x71
		};
		[Constructable]
		public ParrotItem()
			: base(0x20EE)
		{
			Weight = 1;
			Hue = Utility.RandomList(MHues);
		}

		public ParrotItem(Serial serial)
			: base(serial)
		{
		}

		public override int LabelNumber => 1074281;// pet parrot
		public override void OnDoubleClick(Mobile from)
		{
			if (IsChildOf(from.Backpack))
			{
				from.Target = new InternalTarget(this);
				from.SendLocalizedMessage(1072612); // Target the Parrot Perch you wish to place this Parrot upon.
			}
			else
				from.SendLocalizedMessage(1042004); // That must be in your pack for you to use it.
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

		public class InternalTarget : Target
		{
			private readonly ParrotItem _mParrot;
			public InternalTarget(ParrotItem parrot)
				: base(2, false, TargetFlags.None)
			{
				_mParrot = parrot;
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (targeted is AddonComponent component)
				{
					if (component.Addon is ParrotPerchAddon perch)
					{
						BaseHouse house = BaseHouse.FindHouseAt(perch);

						if (house != null && house.IsCoOwner(from))
						{
							if (perch.Parrot == null || perch.Parrot.Deleted)
							{
								PetParrot parrot = new()
								{
									Hue = _mParrot.Hue
								};
								parrot.MoveToWorld(perch.Location, perch.Map);
								parrot.Z += 12;

								perch.Parrot = parrot;
								_mParrot.Delete();
							}
							else
								from.SendLocalizedMessage(1072616); //That Parrot Perch already has a Parrot.
						}
						else
							from.SendLocalizedMessage(1072618); //Parrots can only be placed on Parrot Perches in houses where you are an owner or co-owner.	
					}
					else
						from.SendLocalizedMessage(1072614); //You must place the Parrot on a Parrot Perch.
				}
				else
					from.SendLocalizedMessage(1072614); //You must place the Parrot on a Parrot Perch.
			}

			protected override void OnTargetOutOfRange(Mobile from, object targeted)
			{
				base.OnTargetOutOfRange(from, targeted);

				from.SendLocalizedMessage(1072613); //You must be closer to the Parrot Perch to place the Parrot upon it.
			}
		}
	}
}
