using Server.Gumps;
using Server.Mobiles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Items;

public class MythicCharacterToken : BaseItem, IPromotionalToken
{
	public override int LabelNumber => 1070997;  // a promotional token
	public TextDefinition ItemName => 1152353;  // Mythic Character Token

	public Type GumpType => typeof(InternalGump);

	[Constructable]
	public MythicCharacterToken()
		: base(0x2AAA)
	{
		LootType = LootType.Blessed;
	}

	public override void OnDoubleClick(Mobile m)
	{
		if (m is PlayerMobile mobile && IsChildOf(mobile.Backpack))
		{
			if (mobile.Skills.Total > 2000)
			{
				mobile.SendLocalizedMessage(1152368); // You cannot use this token on this character because you have over 200 skill points. If you 
				// don’t have a way to lower your skill point total, you will have to use this Mythic Character
				// Token on another character.
			}
			else
			{
				BaseGump.SendGump(new InternalGump(mobile, this));
			}
		}
	}

	public override void GetProperties(ObjectPropertyList list)
	{
		base.GetProperties(list);

		list.Add(1070998, ItemName.ToString()); // Use this to redeem<br>Your ~1_PROMO~
	}

	public MythicCharacterToken(Serial serial)
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

	private class InternalGump : BaseGump
	{
		private MythicCharacterToken Token { get; }
		private Skill[] Selected { get; }

		public bool Editing { get; set; }

		private int Str { get; set; }
		private int Dex { get; set; }
		private int Int { get; set; }

		private const int Width = 500;
		private const int Height = 510;

		private static int Green => C32216(0x32CD32);
		private static int LightGreen => C32216(0x90EE90);
		private static int Yellow => C32216(0xFFE4C4);
		private static int Beige => C32216(0xF5F5DC);
		private static int Gray => C32216(0x696969);
		private static int White => 0x7FFF;

		private bool HasAllFive
		{
			get
			{
				return Selected != null && Selected.All(sk => sk != null);
			}
		}

		public InternalGump(PlayerMobile pm, MythicCharacterToken token)
			: base(pm, 100, 100)
		{
			Token = token;
			Selected = new Skill[5];
		}

		public override void AddGumpLayout()
		{
			AddPage(0);
			AddBackground(0, 0, Width, Height, 9200);

			AddImageTiled(10, 10, 480, 25, 2624);
			AddAlphaRegion(10, 10, 480, 25);

			AddImageTiled(10, 45, 480, 425, 2624);
			AddAlphaRegion(10, 45, 480, 425);

			AddImageTiled(10, 480, 480, 22, 2624);
			AddAlphaRegion(10, 480, 480, 22);

			AddHtmlLocalized(0, 12, Width, 20, 1152352, White, false, false); // <center>Mythic Character Skill Selection</center>

			AddHtmlLocalized(0, 45, Width / 3, 20, 1152354, Yellow, false, false); // <CENTER>Set Attributes</CENTER>
			AddHtmlLocalized(0, 65, Width / 3, 20, 1152355, User.StatCap.ToString(), Beige, false, false); // <CENTER>Total Must Equal ~1_VAL~

			AddBackground(11, 85, 80, 20, 3000);
			AddBackground(11, 106, 80, 20, 3000);
			AddBackground(11, 127, 80, 20, 3000);

			AddTextEntry(13, 85, 75, 20, 0, 1, Str > 0 ? Str.ToString() : "");
			AddTextEntry(13, 106, 75, 20, 0, 2, Dex > 0 ? Dex.ToString() : "");
			AddTextEntry(13, 127, 75, 20, 0, 3, Int > 0 ? Int.ToString() : "");

			AddHtmlLocalized(98, 85, 100, 20, 3000111, White, false, false); // Strength
			AddHtmlLocalized(98, 106, 100, 20, 3000113, White, false, false); // Dexterity
			AddHtmlLocalized(98, 127, 100, 20, 3000112, White, false, false); // Intelligence

			AddHtmlLocalized(0, 170, Width / 3, 20, 1152356, Yellow, false, false); // <CENTER>Selected Skills</CENTER>

			AddButton(10, Height - 30, 4017, 4018, 0, GumpButtonType.Reply, 0);
			AddHtmlLocalized(42, Height - 30, 100, 20, 1153439, White, false, false); // CANCEL

			for (int i = 0; i < Selected.Length; i++)
			{
				Skill sk = Selected[i];

				if (sk == null)
					continue;

				AddButton(12, 190 + i * 20, 4017, 4018, 5000 + i, GumpButtonType.Reply, 0);
				AddHtmlLocalized(45, 190 + i * 20, 150, 20, sk.Info.Localization, Green, false, false);
			}

			if (HasAllFive)
			{
				AddHtmlLocalized(Width / 3, 65, Width / 3 * 2 - 15, Height - 100, 1152358, LightGreen, false, false);
				/*Please confirm that you wish to set your attributes as indicated in the upper left area of this window. 
				If you wish to change these values, edit them and click the EDIT button below.<br><br>Please confirm that 
				you wish to set the five skills selected on the left to 90.0 skill. If you wish to make changes, click the 
				[X] button next to a skill name to remove it from the list.<br><br>If are sure you wish to apply the selected
				skills and attributes, click the CONTINUE button below.<br><br>If you wish to abort the application of the 
				Mythic Character Token, click the CANCEL button below.*/

				AddButton(Width / 3, Height - 100, 4005, 4007, 2500, GumpButtonType.Reply, 0);
				AddHtmlLocalized(Width / 3 + 32, Height - 100, 100, 20, 1150647, White, false, false); // EDIT

				AddButton(Width / 3, Height - 120, 4005, 4007, 2501, GumpButtonType.Reply, 0);
				AddHtmlLocalized(Width / 3 + 32, Height - 120, 100, 20, 1011011, White, false, false); // CONTINUE
			}
			else
			{
				AddHtmlLocalized(Width / 3, 45, Width / 3 * 2, 20, 1152357, White, false, false); // <CENTER>Select Five Skills to Advance</CENTER>

				AddPage(1);

				BuildSkillCategory(BaseSpecialScrollBook.GetCategoryLocalization(SkillCat.Magic), Width / 3, 65, ScrollOfAlacrityBook.m_SkillInfo[SkillCat.Magic]);
				BuildSkillCategory(BaseSpecialScrollBook.GetCategoryLocalization(SkillCat.Bard), Width / 3, 345, ScrollOfAlacrityBook.m_SkillInfo[SkillCat.Bard]);
				BuildSkillCategory(BaseSpecialScrollBook.GetCategoryLocalization(SkillCat.Combat), Width / 3 * 2, 65, ScrollOfAlacrityBook.m_SkillInfo[SkillCat.Combat]);
				BuildSkillCategory(BaseSpecialScrollBook.GetCategoryLocalization(SkillCat.Wilderness), Width / 3 * 2, 305, ScrollOfAlacrityBook.m_SkillInfo[SkillCat.Wilderness]);

				AddButton(Width - 120, Height - 30, 4005, 4007, 0, GumpButtonType.Page, 2);
				AddHtmlLocalized(Width - 85, Height - 30, 75, 20, 3005109, White, false, false); // Next
				AddPage(2);
				AddButton(Width - 160, Height - 30, 4014, 4015, 0, GumpButtonType.Page, 1);
				AddHtmlLocalized(Width - 128, Height - 30, 75, 20, 3010002, White, false, false); // Back

				BuildSkillCategory(BaseSpecialScrollBook.GetCategoryLocalization(SkillCat.TradeSkills), Width / 3, 65, ScrollOfAlacrityBook.m_SkillInfo[SkillCat.TradeSkills]);
				BuildSkillCategory(BaseSpecialScrollBook.GetCategoryLocalization(SkillCat.Miscellaneous), Width / 3, 285, ScrollOfAlacrityBook.m_SkillInfo[SkillCat.Miscellaneous]);
				BuildSkillCategory(BaseSpecialScrollBook.GetCategoryLocalization(SkillCat.Thievery), Width / 3 * 2, 150, ScrollOfAlacrityBook.m_SkillInfo[SkillCat.Thievery]);
			}
		}

		private void BuildSkillCategory(int titleLoc, int x, int y, IReadOnlyList<SkillName> skills)
		{
			AddHtmlLocalized(x, y, Width / 3, 20, CenterLoc, "#" + titleLoc, White, false, false);
			y += 20;

			for (int i = 0; i < skills.Count; i++)
			{
				int hue = Gray;
				if (CanSelect(skills[i]))
				{
					AddButton(x, y + i * 20, 4005, 4006, (int)skills[i] + 500, GumpButtonType.Reply, 0);
					hue = Green;
				}

				AddHtmlLocalized(x + 34, y + i * 20, Width / 3, 20, User.Skills[skills[i]].Info.Localization, hue, false, false);
			}
		}

		public override void OnResponse(RelayInfo info)
		{
			if (!Token.IsChildOf(User.Backpack) || !User.Alive || User.Skills.Total > 2000)
				return;

			int buttonId = info.ButtonID;

			if (buttonId == 0)
				return;

			switch (buttonId)
			{
				case 2500: // Edit
					SetStats(info);
					break;
				case 2501: // Continue
					SetStats(info);
					if (Str + Dex + Int != User.StatCap)
					{
						User.SendLocalizedMessage(1152359); // Your Strength, Dexterity, and Intelligence values do not add up to the total indicated in 
						// the upper left area of this window. Before continuing, you must adjust these values so 
						// their total adds up to exactly the displayed value. Please edit your desired attribute 
						// values and click the EDIT button below to continue.
					}
					else
					{
						Effects.SendLocationParticles(EffectItem.Create(User.Location, User.Map, EffectItem.DefaultDuration), 0, 0, 0, 0, 0, 5060, 0);
						Effects.PlaySound(User.Location, User.Map, 0x243);

						Effects.SendMovingParticles(new Entity(Server.Serial.Zero, new Point3D(User.X - 6, User.Y - 6, User.Z + 15), User.Map), User, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100);
						Effects.SendMovingParticles(new Entity(Server.Serial.Zero, new Point3D(User.X - 4, User.Y - 6, User.Z + 15), User.Map), User, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100);
						Effects.SendMovingParticles(new Entity(Server.Serial.Zero, new Point3D(User.X - 6, User.Y - 4, User.Z + 15), User.Map), User, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100);

						Effects.SendTargetParticles(User, 0x375A, 35, 90, 0x00, 0x00, 9502, (EffectLayer)255, 0x100);

						foreach (Skill sk in Selected)
						{
							sk.Base = 90;
						}

						User.RawStr = Str;
						User.RawDex = Dex;
						User.RawInt = Int;

						Token.Delete();
						return;
					}
					break;
				default:
				{
					if (buttonId >= 5000)
					{
						Selected[buttonId - 5000] = null;
					}
					else if (!HasAllFive)
					{
						SkillName sk = (SkillName)buttonId - 500;

						for (int i = 0; i < Selected.Length; i++)
						{
							if (Selected[i] == null)
							{
								Selected[i] = User.Skills[sk];
								break;
							}
						}
					}
				}
					break;
			}

			Refresh();
		}

		private void SetStats(RelayInfo info)
		{
			TextRelay entry1 = info.GetTextEntry(1);
			TextRelay entry2 = info.GetTextEntry(2);
			TextRelay entry3 = info.GetTextEntry(3);

			if (entry1 != null)
				Str = Math.Min(125, Math.Max(10, Utility.ToInt32(entry1.Text)));

			if (entry2 != null)
				Dex = Math.Min(125, Math.Max(10, Utility.ToInt32(entry2.Text)));

			if (entry3 != null)
				Int = Math.Min(125, Math.Max(10, Utility.ToInt32(entry3.Text)));
		}

		private bool CanSelect(SkillName skill)
		{
			if (Selected.Any(sk => User.Skills[skill] == sk))
			{
				return false;
			}

			return skill switch
			{
				SkillName.Spellweaving when !User.Spellweaving => false,
				SkillName.Throwing when User.Race != Race.Gargoyle => false,
				SkillName.Archery when User.Race == Race.Gargoyle => false,
				_ => true
			};
		}
	}
}