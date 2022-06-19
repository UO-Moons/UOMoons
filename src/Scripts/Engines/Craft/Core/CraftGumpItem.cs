using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using System;

namespace Server.Engines.Craft;

public class CraftGumpItem : Gump
{
	private readonly Mobile _mFrom;
	private readonly CraftSystem _mCraftSystem;
	private readonly CraftItem _mCraftItem;
	private readonly ITool _mTool;

	private const int LabelHue = 0x480; // 0x384
	private const int RedLabelHue = 0x20;

	private const int LabelColor = 0x7FFF;
	private const int RedLabelColor = 0x6400;

	private const int GreyLabelColor = 0x3DEF;

	private int _mOtherCount;

	public CraftGumpItem(Mobile from, CraftSystem craftSystem, CraftItem craftItem, ITool tool)
		: base(40, 40)
	{
		_mFrom = from;
		_mCraftSystem = craftSystem;
		_mCraftItem = craftItem;
		_mTool = tool;

		from.CloseGump(typeof(CraftGump));
		from.CloseGump(typeof(CraftGumpItem));

		AddPage(0);
		AddBackground(0, 0, 530, 417, 5054);
		AddImageTiled(10, 10, 510, 22, 2624);
		AddImageTiled(10, 37, 150, 148, 2624);
		AddImageTiled(165, 37, 355, 90, 2624);
		AddImageTiled(10, 190, 155, 22, 2624);
		AddImageTiled(10, 240, 150, 57, 2624);
		AddImageTiled(165, 132, 355, 80, 2624);
		AddImageTiled(10, 325, 150, 57, 2624);
		AddImageTiled(165, 217, 355, 80, 2624);
		AddImageTiled(165, 302, 355, 80, 2624);
		AddImageTiled(10, 387, 510, 22, 2624);
		AddAlphaRegion(10, 10, 510, 399);

		AddHtmlLocalized(170, 40, 150, 20, 1044053, LabelColor, false, false); // ITEM
		AddHtmlLocalized(10, 217, 150, 22, 1044055, LabelColor, false, false); // <CENTER>MATERIALS</CENTER>
		AddHtmlLocalized(10, 302, 150, 22, 1044056, LabelColor, false, false); // <CENTER>OTHER</CENTER>

		if (craftSystem.GumpTitleNumber > 0)
		{
			AddHtmlLocalized(10, 12, 510, 20, craftSystem.GumpTitleNumber, LabelColor, false, false);
		}
		else
		{
			AddHtml(10, 12, 510, 20, craftSystem.GumpTitleString, false, false);
		}

		bool needsRecipe = craftItem.Recipe != null && from is PlayerMobile mobile && !mobile.HasRecipe(craftItem.Recipe);

		if (needsRecipe)
		{
			AddButton(405, 387, 4005, 4007, 0, GumpButtonType.Page, 0);
			AddHtmlLocalized(440, 390, 150, 18, 1044151, GreyLabelColor, false, false); // MAKE NOW
		}
		else
		{
			AddButton(405, 387, 4005, 4007, 1, GumpButtonType.Reply, 0);
			AddHtmlLocalized(445, 390, 150, 18, 1044151, LabelColor, false, false); // MAKE NOW
		}

		#region Stygian Abyss
		AddButton(265, 387, 4005, 4007, 2, GumpButtonType.Reply, 0);
		AddHtmlLocalized(300, 390, 150, 18, 1112623, LabelColor, false, false); //MAKE NUMBER

		AddButton(135, 387, 4005, 4007, 3, GumpButtonType.Reply, 0);
		AddHtmlLocalized(170, 390, 150, 18, 1112624, LabelColor, false, false); //MAKE MAX
		#endregion

		AddButton(15, 387, 4014, 4016, 0, GumpButtonType.Reply, 0);
		AddHtmlLocalized(50, 390, 150, 18, 1044150, LabelColor, false, false); // BACK

		if (craftItem.NameNumber > 0)
		{
			AddHtmlLocalized(330, 40, 180, 18, craftItem.NameNumber, LabelColor, false, false);
		}
		else
		{
			AddLabel(330, 40, LabelHue, craftItem.NameString);
		}

		if (craftItem.UseAllRes)
		{
			AddHtmlLocalized(170, 302 + (_mOtherCount++ * 20), 310, 18, 1048176, LabelColor, false, false); // Makes as many as possible at once
		}

		DrawItem();
		DrawSkill();
		DrawResource();

		/*
		if( craftItem.RequiresSE )
		AddHtmlLocalized( 170, 302 + (m_OtherCount++ * 20), 310, 18, 1063363, LabelColor, false, false ); //* Requires the "Samurai Empire" expansion
		* */

		if (craftItem.RequiredExpansion != Expansion.None)
		{
			bool supportsEx = from.NetState != null && from.NetState.SupportsExpansion(craftItem.RequiredExpansion);
			TextDefinition.AddHtmlText(this, 170, 302 + _mOtherCount++ * 20, 310, 18, RequiredExpansionMessage(craftItem.RequiredExpansion), false, false, supportsEx ? LabelColor : RedLabelColor, supportsEx ? LabelHue : RedLabelHue);
		}

		if (craftItem.RequiredThemePack != ThemePack.None)
		{
			TextDefinition.AddHtmlText(this, 170, 302 + _mOtherCount++ * 20, 310, 18, RequiredThemePackMessage(craftItem.RequiredThemePack), false, false, LabelColor, LabelHue);
		}

		if (needsRecipe)
		{
			AddHtmlLocalized(170, 302 + _mOtherCount++ * 20, 310, 18, 1073620, RedLabelColor, false, false); // You have not learned this recipe.
		}
	}

	private static TextDefinition RequiredExpansionMessage(Expansion expansion)
	{
		return expansion switch
		{
			Expansion.SE => 1063363 // * Requires the "Samurai Empire" expansion
			,
			Expansion.ML => 1072651 // * Requires the "Mondain's Legacy" expansion
			,
			Expansion.SA => 1094732 // * Requires the "Stygian Abyss" expansion
			,
			Expansion.HS => 1116296 // * Requires the "High Seas" booster
			,
			Expansion.TOL => 1155876 // * Requires the "Time of Legends" expansion.
			,
			_ => $"* Requires the \"{ExpansionInfo.GetInfo(expansion).Name}\" expansion"
		};
	}

	private static TextDefinition RequiredThemePackMessage(ThemePack pack)
	{
		return pack switch
		{
			ThemePack.Kings => 1154195 // *Requires the "King's Collection" theme pack
			,
			ThemePack.Rustic => 1150651 // * Requires the "Rustic" theme pack
			,
			ThemePack.Gothic => 1150650 // * Requires the "Gothic" theme pack
			,
			_ => string.Format("Requires the \"{0}\" theme pack.", null!)
		};
	}

	private bool _mShowExceptionalChance;

	public void DrawItem()
	{
		Type type = _mCraftItem.ItemType;
		int id = _mCraftItem.DisplayId;
		if (id == 0)
		{
			id = CraftItem.ItemIdOf(type);
		}

		Rectangle2D b = ItemBounds.Table[id];
		AddItem(90 - b.Width / 2 - b.X, 110 - b.Height / 2 - b.Y, id, _mCraftItem.ItemHue);

		if (_mCraftItem.IsMarkable(type))
		{
			AddHtmlLocalized(170, 302 + _mOtherCount++ * 20, 310, 18, 1044059, LabelColor, false, false); // This item may hold its maker's mark
			_mShowExceptionalChance = true;
		}
		else if (typeof(IQuality).IsAssignableFrom(_mCraftItem.ItemType))
		{
			_mShowExceptionalChance = true;
		}
	}

	public void DrawSkill()
	{
		for (int i = 0; i < _mCraftItem.Skills.Count; i++)
		{
			CraftSkill skill = _mCraftItem.Skills.GetAt(i);
			double minSkill = skill.MinSkill;
			_ = skill.MaxSkill;

			if (minSkill < 0)
			{
				minSkill = 0;
			}

			AddHtmlLocalized(170, 132 + i * 20, 200, 18, AosSkillBonuses.GetLabel(skill.SkillToMake), LabelColor, false, false);
			AddLabel(430, 132 + i * 20, LabelHue, $"{minSkill:F1}");
		}

		CraftSubResCol res = _mCraftItem.UseSubRes2 ? _mCraftSystem.CraftSubRes2 : _mCraftSystem.CraftSubRes;
		int resIndex = -1;

		CraftContext context = _mCraftSystem.GetContext(_mFrom);

		if (context != null)
		{
			resIndex = _mCraftItem.UseSubRes2 ? context.LastResourceIndex2 : context.LastResourceIndex;
		}

		bool allRequiredSkills = true;
		double chance = _mCraftItem.GetSuccessChance(_mFrom, resIndex > -1 ? res.GetAt(resIndex).ItemType : null, _mCraftSystem, false, ref allRequiredSkills);
		double excepChance = _mCraftItem.GetExceptionalChance(_mCraftSystem, chance, _mFrom);

		chance = chance switch
		{
			< 0.0 => 0.0,
			> 1.0 => 1.0,
			_ => chance
		};

		AddHtmlLocalized(170, 80, 250, 18, 1044057, LabelColor, false, false); // Success Chance:
		AddLabel(430, 80, LabelHue, $"{chance * 100:F1}%");

		if (!_mShowExceptionalChance) return;
		excepChance = excepChance switch
		{
			< 0.0 => 0.0,
			> 1.0 => 1.0,
			_ => excepChance
		};

		AddHtmlLocalized(170, 100, 250, 18, 1044058, 32767, false, false); // Exceptional Chance:
		AddLabel(430, 100, LabelHue, $"{excepChance * 100:F1}%");
	}

	private static readonly Type TypeofBlankScroll = typeof(BlankScroll);
	private static readonly Type TypeofSpellScroll = typeof(SpellScroll);

	public void DrawResource()
	{
		bool retainedColor = false;

		CraftContext context = _mCraftSystem.GetContext(_mFrom);

		CraftSubResCol res = _mCraftItem.UseSubRes2 ? _mCraftSystem.CraftSubRes2 : _mCraftSystem.CraftSubRes;
		int resIndex = -1;

		if (context != null)
		{
			resIndex = _mCraftItem.UseSubRes2 ? context.LastResourceIndex2 : context.LastResourceIndex;
		}

		bool cropScroll = _mCraftItem.Resources.Count > 1 &&
		                  _mCraftItem.Resources.GetAt(_mCraftItem.Resources.Count - 1).ItemType == TypeofBlankScroll &&
		                  TypeofSpellScroll.IsAssignableFrom(_mCraftItem.ItemType);

		for (int i = 0; i < _mCraftItem.Resources.Count - (cropScroll ? 1 : 0) && i < 4; i++)
		{
			CraftRes craftResource = _mCraftItem.Resources.GetAt(i);

			var type = craftResource.ItemType;
			var nameString = craftResource.NameString;
			var nameNumber = craftResource.NameNumber;

			// Resource Mutation
			if (type == res.ResType && resIndex > -1)
			{
				CraftSubRes subResource = res.GetAt(resIndex);

				type = subResource.ItemType;

				nameString = subResource.NameString;
				nameNumber = subResource.GenericNameNumber;

				if (nameNumber <= 0)
				{
					nameNumber = subResource.NameNumber;
				}
			}
			// ******************

			if (!retainedColor && _mCraftItem.RetainsColorFrom(_mCraftSystem, type))
			{
				retainedColor = true;
				AddHtmlLocalized(170, 302 + _mOtherCount++ * 20, 310, 18, 1044152, LabelColor, false, false); // * The item retains the color of this material
				AddLabel(500, 219 + i * 20, LabelHue, "*");
			}

			if (nameNumber > 0)
			{
				AddHtmlLocalized(170, 219 + i * 20, 310, 18, nameNumber, LabelColor, false, false);
			}
			else
			{
				AddLabel(170, 219 + i * 20, LabelHue, nameString);
			}

			AddLabel(430, 219 + i * 20, LabelHue, craftResource.Amount.ToString());
		}

		if (_mCraftItem.NameNumber == 1041267) // runebook
		{
			AddHtmlLocalized(170, 219 + _mCraftItem.Resources.Count * 20, 310, 18, 1044447, LabelColor, false, false);
			AddLabel(430, 219 + _mCraftItem.Resources.Count * 20, LabelHue, "1");
		}

		if (cropScroll)
		{
			AddHtmlLocalized(170, 302 + (_mOtherCount++ * 20), 360, 18, 1044379, LabelColor, false, false); // Inscribing scrolls also requires a blank scroll and mana.
		}
	}

	public override void OnResponse(NetState sender, RelayInfo info)
	{
		switch (info.ButtonID)
		{
			case 0: // Back Button
			{
				CraftGump craftGump = new(_mFrom, _mCraftSystem, _mTool, null);
				_mFrom.SendGump(craftGump);
				break;
			}
			case 1: // Make Button
			{
				if (_mCraftItem.TryCraft != null)
				{
					_mCraftItem.TryCraft(_mFrom, _mCraftItem, _mTool);
					return;
				}

				int num = _mCraftSystem.CanCraft(_mFrom, _mTool, _mCraftItem.ItemType);

				if (num > 0)
				{
					_mFrom.SendGump(new CraftGump(_mFrom, _mCraftSystem, _mTool, num));
				}
				else
				{
					Type type = null;

					CraftContext context = _mCraftSystem.GetContext(_mFrom);

					if (context != null)
					{
						CraftSubResCol res = _mCraftItem.UseSubRes2 ? _mCraftSystem.CraftSubRes2 : _mCraftSystem.CraftSubRes;
						int resIndex = _mCraftItem.UseSubRes2 ? context.LastResourceIndex2 : context.LastResourceIndex;

						if (resIndex > -1)
						{
							type = res.GetAt(resIndex).ItemType;
						}
					}

					_mCraftSystem.CreateItem(_mFrom, _mCraftItem.ItemType, type, _mTool, _mCraftItem);
				}
				break;
			}
			case 2: //Make Number
				_mFrom.Prompt = new MakeNumberCraftPrompt(_mFrom, _mCraftSystem, _mCraftItem, _mTool);
				_mFrom.SendLocalizedMessage(1112576); //Please type the amount you wish to create(1 - 100): <Escape to cancel>
				break;
			case 3: //Make Max
				AutoCraftTimer.EndTimer(_mFrom);
				new AutoCraftTimer(_mFrom, _mCraftSystem, _mCraftItem, _mTool, 9999, TimeSpan.FromSeconds(_mCraftSystem.Delay * _mCraftSystem.MaxCraftEffect + 1.0), TimeSpan.FromSeconds(_mCraftSystem.Delay * _mCraftSystem.MaxCraftEffect + 1.0));
				break;
		}
	}
}
