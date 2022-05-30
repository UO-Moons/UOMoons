using Server.Engines.Craft;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Items
{
	public class HammerOfHephaestus : AncientSmithyHammer
	{
		private static readonly List<HammerOfHephaestus> _Instances = new();

		public static void Initialize()
		{
			_ = Timer.DelayCall(TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5), new TimerCallback(Tick_Callback));
		}

		private static void Tick_Callback()
		{
			foreach (var hammer in _Instances.Where(h => h != null && !h.Deleted && h.UsesRemaining < 20))
			{
				hammer.UsesRemaining++;
				hammer.InvalidateProperties();
			}
		}

		[Constructable]
		public HammerOfHephaestus()
			: base(10, 20)
		{
			LootType = LootType.Blessed;
			Hue = 0x0;

			_Instances.Add(this);
		}

		public HammerOfHephaestus(Serial serial)
			: base(serial)
		{
		}

		public override void Delete()
		{
			base.Delete();

			_ = _Instances.Remove(this);
		}

		public override int LabelNumber => 1077740;// Hammer of Hephaestus

		public override void OnDoubleClick(Mobile from)
		{
			if (IsChildOf(from.Backpack) || Parent == from)
			{
				if (UsesRemaining > 0)
				{
					CraftSystem system = CraftSystem;

					int num = system.CanCraft(from, this, null);

					if (num > 0)
					{
						from.SendLocalizedMessage(num);
					}
					else
					{
						_ = system.GetContext(from);

						_ = from.SendGump(new CraftGump(from, system, this, null));
					}
				}
				else
					from.SendLocalizedMessage(1072306); // You must wait a moment for it to recharge.
			}
			else
			{
				from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
			}
		}

		public override bool CanEquip(Mobile from)
		{
			if (UsesRemaining > 0)
				return base.CanEquip(from);

			from.SendLocalizedMessage(1072306); // You must wait a moment for it to recharge.
			return false;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			_ = reader.ReadEncodedInt();

			_Instances.Add(this);
		}
	}
}
