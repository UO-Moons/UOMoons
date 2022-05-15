namespace Server.Items
{
	public class BookOfChivalry : Spellbook
	{
		public override SpellbookType SpellbookType => SpellbookType.Paladin;
		public override int BookOffset => 200;
		public override int BookCount => 10;

		[Constructable]
		public BookOfChivalry() : this((ulong)0x3FF)
		{
		}

		[Constructable]
		public BookOfChivalry(ulong content) : base(content, 0x2252)
		{
			Layer = (Core.ML ? Layer.OneHanded : Layer.Invalid);
		}

		public BookOfChivalry(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.ReadInt();
		}
	}
}
