using System;
using Server.Mobiles;

namespace Server.Items
{
	public class SelfDeletingItem : Item
	{
		[Constructable]
		public SelfDeletingItem( int id, string name, int duration ) : base ( 8391 )
		{
			Weight = 1.0;
			ItemId = id;
			Name = "name";
			Movable = false;

			Timer.DelayCall( TimeSpan.FromSeconds( duration ), new TimerCallback( Expire ) );
		}

		private void Expire()
		{
			if ( Deleted )
				return;

			Delete();
		}

		public SelfDeletingItem( Serial serial ) : base( serial )
		{
			Timer.DelayCall( TimeSpan.FromSeconds( 1 ), new TimerCallback( Expire ) );
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 1 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}
