using Server.Targeting;
using System;

namespace Server.Items;

public class UtilityItem
{
	public static int RandomChoice(int itemId1, int itemId2)
	{
		int iRet = Utility.Random(2) switch
		{
			1 => itemId2,
			_ => itemId1
		};
		return iRet;
	}
}

public class Dough : BaseItem
{
	[Constructable]
	public Dough() : base(0x103d)
	{
		Stackable = Core.ML;
		Weight = 1.0;
	}

	public Dough(Serial serial) : base(serial)
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

#if false
		public override void OnDoubleClick( Mobile from )
		{
			if ( !Movable )
				return;

			from.Target = new InternalTarget( this );
		}
#endif

	private class InternalTarget : Target
	{
		private readonly Dough m_Item;

		public InternalTarget(Dough item) : base(1, false, TargetFlags.None)
		{
			m_Item = item;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (m_Item.Deleted) return;

			if (targeted is Eggs)
			{
				m_Item.Delete();

				((Eggs)targeted).Consume();

				from.AddToBackpack(new UnbakedQuiche());
				from.AddToBackpack(new Eggshells());
			}
			else if (targeted is CheeseWheel)
			{
				m_Item.Delete();

				((CheeseWheel)targeted).Consume();

				from.AddToBackpack(new CheesePizza());
			}
			else if (targeted is Sausage)
			{
				m_Item.Delete();

				((Sausage)targeted).Consume();

				from.AddToBackpack(new SausagePizza());
			}
			else if (targeted is Apple)
			{
				m_Item.Delete();

				((Apple)targeted).Consume();

				from.AddToBackpack(new UnbakedApplePie());
			}

			else if (targeted is Peach)
			{
				m_Item.Delete();

				((Peach)targeted).Consume();

				from.AddToBackpack(new UnbakedPeachCobbler());
			}
		}
	}
}

public class SweetDough : BaseItem
{
	public override int LabelNumber => 1041340;  // sweet dough

	[Constructable]
	public SweetDough() : base(0x103d)
	{
		Stackable = Core.ML;
		Weight = 1.0;
		Hue = 150;
	}

	public SweetDough(Serial serial) : base(serial)
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

#if false
		public override void OnDoubleClick( Mobile from )
		{
			if ( !Movable )
				return;

			from.Target = new InternalTarget( this );
		}
#endif

	private class InternalTarget : Target
	{
		private readonly SweetDough m_Item;

		public InternalTarget(SweetDough item) : base(1, false, TargetFlags.None)
		{
			m_Item = item;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (m_Item.Deleted) return;

			if (targeted is BowlFlour)
			{
				m_Item.Delete();
				((BowlFlour)targeted).Delete();

				from.AddToBackpack(new CakeMix());
			}
			else if (targeted is Campfire)
			{
				from.PlaySound(0x225);
				m_Item.Delete();
				InternalTimer t = new(from, (Campfire)targeted);
				t.Start();
			}
		}

		private class InternalTimer : Timer
		{
			private readonly Mobile m_From;
			private readonly Campfire m_Campfire;

			public InternalTimer(Mobile from, Campfire campfire) : base(TimeSpan.FromSeconds(5.0))
			{
				m_From = from;
				m_Campfire = campfire;
			}

			protected override void OnTick()
			{
				if (m_From.GetDistanceToSqrt(m_Campfire) > 3)
				{
					m_From.SendLocalizedMessage(500686); // You burn the food to a crisp! It's ruined.
					return;
				}

				if (m_From.CheckSkill(SkillName.Cooking, 0, 10))
				{
					if (m_From.AddToBackpack(new Muffins()))
						m_From.PlaySound(0x57);
				}
				else
				{
					m_From.SendLocalizedMessage(500686); // You burn the food to a crisp! It's ruined.
				}
			}
		}
	}
}

public class JarHoney : BaseItem
{
	[Constructable]
	public JarHoney() : base(0x9ec)
	{
		Weight = 1.0;
		Stackable = true;
	}

	public JarHoney(Serial serial) : base(serial)
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
		Stackable = true;
	}

	/*public override void OnDoubleClick( Mobile from )
	{
		if ( !Movable )
			return;

		from.Target = new InternalTarget( this );
	}*/

	private class InternalTarget : Target
	{
		private readonly JarHoney m_Item;

		public InternalTarget(JarHoney item) : base(1, false, TargetFlags.None)
		{
			m_Item = item;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (m_Item.Deleted) return;

			if (targeted is Dough)
			{
				m_Item.Delete();
				((Dough)targeted).Consume();

				from.AddToBackpack(new SweetDough());
			}

			if (targeted is BowlFlour)
			{
				m_Item.Consume();
				((BowlFlour)targeted).Delete();

				from.AddToBackpack(new CookieMix());
			}
		}
	}
}

public class BowlFlour : BaseItem
{
	[Constructable]
	public BowlFlour() : base(0xa1e)
	{
		Weight = 1.0;
	}

	public BowlFlour(Serial serial) : base(serial)
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

public class WoodenBowl : BaseItem
{
	[Constructable]
	public WoodenBowl() : base(0x15f8)
	{
		Weight = 1.0;
	}

	public WoodenBowl(Serial serial) : base(serial)
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

// ********** PitcherWater **********
/*public class PitcherWater : BaseItem
{
	[Constructable]
	public PitcherWater() : base(Utility.Random( 0x1f9d, 2 ))
	{
		Weight = 1.0;
	}

	public PitcherWater( Serial serial ) : base( serial )
	{
	}

	public override void Serialize( GenericWriter writer )
	{
		base.Serialize( writer );

		writer.Write( (int) 0 );
	}

	public override void Deserialize( GenericReader reader )
	{
		base.Deserialize( reader );

		int version = reader.ReadInt();
	}

	public override void OnDoubleClick( Mobile from )
	{
		if ( !Movable )
			return;

		from.Target = new InternalTarget( this );
	}

	private class InternalTarget : Target
	{
		private PitcherWater m_Item;

		public InternalTarget( PitcherWater item ) : base( 1, false, TargetFlags.None )
		{
			m_Item = item;
		}

		protected override void OnTarget( Mobile from, object targeted )
		{
			if ( m_Item.Deleted ) return;

			if ( targeted is BowlFlour )
			{
				m_Item.Delete();
				((BowlFlour)targeted).Delete();

				from.AddToBackpack( new Dough() );
				from.AddToBackpack( new WoodenBowl() );
			}
		}
	}
}*/

[TypeAlias("Server.Items.SackFlourOpen")]
public class SackFlour : BaseItem, IHasQuantity
{
	private int m_Quantity;

	[CommandProperty(AccessLevel.GameMaster)]
	public int Quantity
	{
		get => m_Quantity;
		set
		{
			if (value < 0)
				value = 0;
			else if (value > 20)
				value = 20;

			m_Quantity = value;

			if (m_Quantity == 0)
				Delete();
			else if (m_Quantity < 20 && (ItemId == 0x1039 || ItemId == 0x1045))
				++ItemId;
		}
	}

	[Constructable]
	public SackFlour() : base(0x1039)
	{
		Weight = 5.0;
		m_Quantity = 20;
	}

	public SackFlour(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0);

		writer.Write(m_Quantity);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		int version = reader.ReadInt();

		switch (version)
		{
			case 0:
			{
				m_Quantity = reader.ReadInt();
				break;
			}
		}
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (!Movable)
			return;

		if ((ItemId == 0x1039 || ItemId == 0x1045))
			++ItemId;

#if false
			this.Delete();

			from.AddToBackpack( new SackFlourOpen() );
#endif
	}

}

#if false
	// ********** SackFlourOpen **********
	public class SackFlourOpen : BaseItem
	{
		public override int LabelNumber{ get{ return 1024166; } } // open sack of flour

		[Constructable]
		public SackFlourOpen() : base(UtilityItem.RandomChoice( 0x1046, 0x103a ))
		{
			Weight = 1.0;
		}

		public SackFlourOpen( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( !Movable )
				return;

			from.Target = new InternalTarget( this );
		}

		private class InternalTarget : Target
		{
			private SackFlourOpen m_Item;

			public InternalTarget( SackFlourOpen item ) : base( 1, false, TargetFlags.None )
			{
				m_Item = item;
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				if ( m_Item.Deleted ) return;

				if ( targeted is WoodenBowl )
				{
					m_Item.Delete();
					((WoodenBowl)targeted).Delete();

					from.AddToBackpack( new BowlFlour() );
				}
				else if ( targeted is TribalBerry )
				{
					if ( from.Skills[SkillName.Cooking].Base >= 80.0 )
					{
						m_Item.Delete();
						((TribalBerry)targeted).Delete();

						from.AddToBackpack( new TribalPaint() );

						from.SendLocalizedMessage( 1042002 ); // You combine the berry and the flour into the tribal paint worn by the savages.
					}
					else
					{
						from.SendLocalizedMessage( 1042003 ); // You don't have the cooking skill to create the body paint.
					}
				}
			}
		}
	}
#endif

public class Eggshells : BaseItem
{
	[Constructable]
	public Eggshells() : base(0x9b4)
	{
		Weight = 0.5;
	}

	public Eggshells(Serial serial) : base(serial)
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

public sealed class WheatSheaf : BaseItem
{
	[Constructable]
	public WheatSheaf() : this(1)
	{
	}

	[Constructable]
	private WheatSheaf(int amount) : base(7869)
	{
		Weight = 1.0;
		Stackable = true;
		Amount = amount;
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (!Movable)
			return;

		from.BeginTarget(4, false, TargetFlags.None, OnTarget);
	}

	private void OnTarget(Mobile from, object obj)
	{
		if (obj is AddonComponent component)
			obj = component.Addon;

		if (obj is not IFlourMill mill)
			return;
		int needs = mill.MaxFlour - mill.CurFlour;

		if (needs > Amount)
			needs = Amount;

		mill.CurFlour += needs;
		Consume(needs);
	}

	public WheatSheaf(Serial serial) : base(serial)
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