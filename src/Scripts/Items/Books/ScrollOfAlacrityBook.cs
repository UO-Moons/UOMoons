using System;
using System.Collections.Generic;

namespace Server.Items;

[Flipable(0x9981, 0x9982)]
public class ScrollOfAlacrityBook : BaseSpecialScrollBook
{
	public override Type ScrollType => typeof(ScrollOfAlacrity);
	public override int LabelNumber => 1154321;  // Scrolls of Alacrity Book
	public override int BadDropMessage => 1154323;  // This book only holds Scrolls of Alacrity.
	public override int DropMessage => 1154326;     // You add the scroll to your Scrolls of Alacrity book.
	public override int RemoveMessage => 1154322;   // You remove a Scroll of Alacrity and put it in your pack.
	public override int GumpTitle => 1154324;   // Alacrity Scrolls

	[Constructable]
	public ScrollOfAlacrityBook()
		: base(0x9981)
	{
		Hue = 1195;
	}

	public ScrollOfAlacrityBook(Serial serial)
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

	public override Dictionary<SkillCat, List<SkillName>> SkillInfo => m_SkillInfo;
	public override Dictionary<int, double> ValueInfo => m_ValueInfo;

	public static Dictionary<SkillCat, List<SkillName>> m_SkillInfo;
	private static Dictionary<int, double> m_ValueInfo;

	public static void Initialize()
	{
		m_SkillInfo = new Dictionary<SkillCat, List<SkillName>>
		{
			[SkillCat.Miscellaneous] = new() { SkillName.ArmsLore, SkillName.Begging, SkillName.Camping, SkillName.Cartography, SkillName.Forensics, SkillName.ItemID, SkillName.TasteID },
			[SkillCat.Combat] = new() { SkillName.Anatomy, SkillName.Archery, SkillName.Fencing, SkillName.Focus, SkillName.Healing, SkillName.Macing, SkillName.Parry, SkillName.Swords, SkillName.Tactics, SkillName.Throwing, SkillName.Wrestling },
			[SkillCat.TradeSkills] = new() { SkillName.Alchemy, SkillName.Blacksmith, SkillName.Fletching, SkillName.Carpentry, SkillName.Cooking, SkillName.Inscribe, SkillName.Lumberjacking, SkillName.Mining, SkillName.Tailoring, SkillName.Tinkering },
			[SkillCat.Magic] = new() { SkillName.Bushido, SkillName.Chivalry, SkillName.EvalInt, SkillName.Imbuing, SkillName.Magery, SkillName.Meditation, SkillName.Mysticism, SkillName.Necromancy, SkillName.Ninjitsu, SkillName.MagicResist, SkillName.Spellweaving, SkillName.SpiritSpeak },
			[SkillCat.Wilderness] = new() { SkillName.AnimalLore, SkillName.AnimalTaming, SkillName.Fishing, SkillName.Herding, SkillName.Tracking, SkillName.Veterinary },
			[SkillCat.Thievery] = new() { SkillName.DetectHidden, SkillName.Hiding, SkillName.Lockpicking, SkillName.Poisoning, SkillName.RemoveTrap, SkillName.Snooping, SkillName.Stealing, SkillName.Stealth },
			[SkillCat.Bard] = new() { SkillName.Discordance, SkillName.Musicianship, SkillName.Peacemaking, SkillName.Provocation }
		};

		m_ValueInfo = new Dictionary<int, double>
		{
			[1154324] = 0.0
		};
	}
}
