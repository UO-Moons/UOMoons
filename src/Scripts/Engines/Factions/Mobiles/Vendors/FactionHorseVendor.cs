using Server.Items;
using Server.Mobiles;
using Server.Network;
using System.Collections.Generic;

namespace Server.Factions
{
	public class FactionHorseVendor : BaseFactionVendor
	{
		public FactionHorseVendor(Town town, Faction faction) : base(town, faction, "the Horse Breeder")
		{
			SetSkill(SkillName.AnimalLore, 64.0, 100.0);
			SetSkill(SkillName.AnimalTaming, 90.0, 100.0);
			SetSkill(SkillName.Veterinary, 65.0, 88.0);
		}

		public override void InitSBInfo()
		{
		}

		public override VendorShoeType ShoeType => Female ? VendorShoeType.ThighBoots : VendorShoeType.Boots;

		public override int GetShoeHue()
		{
			return 0;
		}

		public override void InitOutfit()
		{
			base.InitOutfit();

			AddItem(Utility.RandomBool() ? new QuarterStaff() : new ShepherdsCrook());
		}

		public FactionHorseVendor(Serial serial) : base(serial)
		{
		}

		public override void VendorBuy(Mobile from)
		{
			if (Faction == null || Faction.Find(from, true) != Faction)
				PrivateOverheadMessage(MessageType.Regular, 0x3B2, 1042201, from.NetState); // You are not in my faction, I cannot sell you a horse!
			else if (FactionGump.Exists(from))
				from.SendLocalizedMessage(1042160); // You already have a faction menu open.
			else if (from is PlayerMobile)
				from.SendGump(new HorseBreederGump((PlayerMobile)from, Faction));
		}

		public override void VendorSell(Mobile from)
		{
		}

		public override bool OnBuyItems(Mobile buyer, List<BuyItemResponse> list)
		{
			return false;
		}

		public override bool OnSellItems(Mobile seller, List<SellItemResponse> list)
		{
			return false;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}
}
