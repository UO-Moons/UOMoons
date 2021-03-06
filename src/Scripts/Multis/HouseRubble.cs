using Server.Mobiles;
using System;

namespace Server.Items
{
	public class HouseRubble : Item
	{
		public override bool DisplayWeight => false;

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime Expire { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile Owner { get; set; }


		[Constructable]
		public HouseRubble() : this(null)
		{
		}

		[Constructable]
		public HouseRubble(Mobile powner) : base(2321)
		{
			Name = "Rubble";
			Owner = powner;
			Movable = false;

			switch (Utility.Random(12))
			{
				case 0: ItemId = 2321; break;
				case 1: ItemId = 2323; break;
				case 2: ItemId = 2324; break;
				case 3: ItemId = 7860; break;
				case 4: ItemId = 3118; break;
				case 5: ItemId = 3119; break;
				case 6: ItemId = 3120; break;
				case 7: ItemId = 3553; break;
				case 8: ItemId = 3892; break;
				case 9: ItemId = 4338; break;
				case 10: ItemId = 4152; break;
				case 11: ItemId = 7859; break;
			}


			Expire = DateTime.UtcNow + TimeSpan.FromMinutes(Utility.Random(5));

			new ExpireTimer(this, Expire).Start();
		}

		//only decay if nobody is around
		public bool CheckDecay()
		{
			if (Expire > DateTime.Now) return false;

			foreach (Mobile m in GetMobilesInRange(30))
			{
				if (m is PlayerMobile)
				{
					Expire = DateTime.Now + TimeSpan.FromMinutes(Utility.Random(10));
					return false;
				}
			}

			Delete();
			return true;
		}

		private class ExpireTimer : Timer
		{
			DateTime expire;
			HouseRubble owner;

			public ExpireTimer(HouseRubble powner, DateTime expiry) : base(TimeSpan.FromMinutes(1.0), TimeSpan.FromMinutes(1))
			{
				expire = expiry;
				owner = powner;
				Priority = TimerPriority.OneMinute;
			}

			protected override void OnTick()
			{
				if (owner == null || owner.Deleted)
				{
					Stop();
				}

				if (owner.CheckDecay())
				{
					Stop();
				}
			}
		}


		public HouseRubble(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
			writer.Write(Expire);
			writer.Write(Owner);

		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			_ = reader.ReadInt();

			Expire = reader.ReadDateTime();
			Owner = reader.ReadMobile();

			Delete();
		}
	}
}
