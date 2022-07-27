using Server.Items;
using Server.Network;
using System;
using System.Linq;

namespace Server.Gumps;

[Flags]
public enum ReforgingOption
{
	None = 0x00000000,
	Powerful = 0x00000001,
	Structural = 0x00000002,
	Fortified = 0x00000004,
	Fundamental = 0x00000008,
	Integral = 0x00000010,
	GrandArtifice = 0x00000020,
	InspiredArtifice = 0x00000040,
	ExaltedArtifice = 0x00000080,
	SublimeArtifice = 0x00000100,

	PowerfulAndStructural = Powerful | Structural,
	PowerfulAndFundamental = Powerful | Fundamental,
	StructuralAndFundamental = Structural | Fundamental,
	PowerfulStructuralAndFundamental = PowerfulAndStructural | Fundamental
}

public class RunicReforgingGump : Gump
{
	private readonly BaseRunicTool m_Tool;
	private readonly Item m_ToReforge;
	private ReforgingOption m_Options;
	private ReforgedPrefix m_Prefix;
	private ReforgedSuffix m_Suffix;

	private readonly ReforgingContext m_Context;

	private readonly ReforgingOption[] _Options =
	{
		ReforgingOption.Powerful,
		ReforgingOption.Structural,
		ReforgingOption.Fortified,
		ReforgingOption.Fundamental,
		ReforgingOption.Integral,
		ReforgingOption.GrandArtifice,
		ReforgingOption.InspiredArtifice,
		ReforgingOption.ExaltedArtifice,
		ReforgingOption.SublimeArtifice,
	};

	public RunicReforgingGump(Mobile from, Item toReforge, BaseRunicTool tool)
		: base(100, 100)
	{
		from.CloseGump(typeof(RunicReforgingGump));
		from.CloseGump(typeof(ImbueGump));

		m_Context = ReforgingContext.GetContext(from);

		if (!m_Context.Contexts.ContainsKey(tool))
		{
			m_Context.Contexts[tool] = new ReforgingInfo();
		}

		m_Tool = tool;
		m_ToReforge = toReforge;
		m_Options = m_Context.Contexts[tool].Options;
		m_Prefix = m_Context.Contexts[tool].Prefix;
		m_Suffix = m_Context.Contexts[tool].Suffix;

		AddBackground(0, 0, 370, 440, 83);
		AddHtmlLocalized(10, 10, 350, 18, 1114513, "#1151952", 0x4BB7, false, false); // Runic Crafting _Options

		var y = 40;

		for (var i = 0; i < _Options.Length; i++)
		{
			ReforgingOption option = _Options[i];

			int buttonHue;
			int buttonId;
			if ((m_Options & option) != 0)
			{
				if (CanReforge(from, option) && HasMetPrerequisite(option))
				{
					buttonHue = 0x4BB2;
					buttonId = 4006;
				}
				else
				{
					buttonHue = 0x7652;
					buttonId = 4006;
				}
			}
			else
			{
				if (CanReforge(from, option) && HasMetPrerequisite(option))
				{
					buttonHue = 0x6F7B;
					buttonId = 4005;
				}
				else
				{
					buttonHue = 0x7652;
					buttonId = 4006;
				}
			}

			if (HasMetPrerequisite(option) && CanReforge(from, option))
				AddButton(15, y, buttonId, buttonId, i + 100, GumpButtonType.Reply, 0);

			AddHtmlLocalized(55, y, 250, 20, GetCliloc(option), buttonHue, false, false);

			y += 25;
		}

		int totalCharges = GetTotalCharges();

		if ((m_Options & ReforgingOption.InspiredArtifice) != 0)
		{
			AddButton(15, 305, 4005, 4007, 1, GumpButtonType.Reply, 0);
			AddHtmlLocalized(55, 305, 250, 20, 1152087, 0x6F7B, false, false);
			AddHtmlLocalized(190, 305, 250, 20, RunicReforging.GetName((int)m_Prefix), 0x5757, false, false);
		}

		if ((m_Options & ReforgingOption.SublimeArtifice) != 0)
		{
			AddButton(15, 330, 4005, 4007, 2, GumpButtonType.Reply, 0);
			AddHtmlLocalized(55, 330, 250, 20, 1152088, 0x6F7B, false, false);
			AddHtmlLocalized(190, 330, 250, 20, RunicReforging.GetName((int)m_Suffix), 0x5757, false, false);
		}

		AddHtmlLocalized(10, 363, 140, 22, 1114514, "#1152078", 0x4BB2, false, false); // CHARGES NEEDED:
		AddLabel(160, 363, 0x113, totalCharges.ToString());

		AddHtmlLocalized(10, 385, 140, 22, 1114514, "#1152077", 0x6F7B, false, false); // TOOL CHARGES:
		AddLabel(160, 385, 0x44E, m_Tool.UsesRemaining.ToString());

		AddButton(10, 412, 4017, 4018, 0, GumpButtonType.Reply, 0);
		AddHtmlLocalized(45, 410, 200, 20, 1060675, 0x6F7B, false, false); // CLOSE

		AddButton(330, 363, 4014, 4016, 3, GumpButtonType.Reply, 0);
		AddHtmlLocalized(190, 363, 135, 22, 1114514, "#1152080", 0x6F7B, false, false); // REFORGE ITEM

		AddButton(330, 412, 4011, 4013, 4, GumpButtonType.Reply, 0);
		AddHtmlLocalized(185, 412, 140, 18, 1114514, "#1149735", 0x6F7B, false, false); // HELP
	}

	private bool HasMetPrerequisite(ReforgingOption option)
	{
		return option switch
		{
			ReforgingOption.None => true,
			ReforgingOption.Powerful => true,
			ReforgingOption.Structural => true,
			ReforgingOption.Fortified => (m_Options & ReforgingOption.Structural) != 0,
			ReforgingOption.Fundamental => true,
			ReforgingOption.Integral => (m_Options & ReforgingOption.Fundamental) != 0,
			ReforgingOption.GrandArtifice => true,
			ReforgingOption.InspiredArtifice => (m_Options & ReforgingOption.GrandArtifice) != 0,
			ReforgingOption.ExaltedArtifice => (m_Options & ReforgingOption.GrandArtifice) != 0,
			ReforgingOption.SublimeArtifice => (m_Options & ReforgingOption.ExaltedArtifice) != 0 &&
			                                   (m_Options & ReforgingOption.GrandArtifice) != 0,
			_ => true
		};
	}

	private void InvalidatePrerequisite(ReforgingOption option)
	{
		switch (option)
		{
			case ReforgingOption.None:
			case ReforgingOption.Powerful: break;
			case ReforgingOption.Structural:
				if ((m_Options & ReforgingOption.Fortified) != 0)
					m_Options ^= ReforgingOption.Fortified;
				break;
			case ReforgingOption.Fortified: break;
			case ReforgingOption.Fundamental:
				if ((m_Options & ReforgingOption.Integral) != 0)
					m_Options ^= ReforgingOption.Integral;
				break;
			case ReforgingOption.Integral: break;
			case ReforgingOption.GrandArtifice:
				if ((m_Options & ReforgingOption.InspiredArtifice) != 0)
					m_Options ^= ReforgingOption.InspiredArtifice;
				if ((m_Options & ReforgingOption.ExaltedArtifice) != 0)
					m_Options ^= ReforgingOption.ExaltedArtifice;
				if ((m_Options & ReforgingOption.SublimeArtifice) != 0)
					m_Options ^= ReforgingOption.SublimeArtifice;
				break;
			case ReforgingOption.InspiredArtifice: break;
			case ReforgingOption.ExaltedArtifice:
				if ((m_Options & ReforgingOption.SublimeArtifice) != 0)
					m_Options ^= ReforgingOption.SublimeArtifice;
				break;
			case ReforgingOption.SublimeArtifice: break;
		}
	}

	private static bool CanReforge(Mobile from, ReforgingOption option)
	{
		double skill = from.Skills[SkillName.Imbuing].Value;

		return option switch
		{
			ReforgingOption.None => skill >= 65.0,
			ReforgingOption.Powerful => skill >= 65.0,
			ReforgingOption.Structural => skill >= 85.0,
			ReforgingOption.Fortified => skill >= 85.0,
			ReforgingOption.Fundamental => skill >= 97.0,
			ReforgingOption.Integral => skill >= 97.0,
			ReforgingOption.GrandArtifice => skill >= 100.1,
			ReforgingOption.InspiredArtifice => skill >= 100.1,
			ReforgingOption.ExaltedArtifice => skill >= 110.1,
			ReforgingOption.SublimeArtifice => skill >= 110.1,
			_ => skill >= 65.0
		};
	}

	private static int GetCliloc(ReforgingOption option)
	{
		return option switch
		{
			ReforgingOption.None => 0,
			ReforgingOption.Powerful => 1151954,
			ReforgingOption.Structural => 1151955,
			ReforgingOption.Fortified => 1151956,
			ReforgingOption.Fundamental => 1151957,
			ReforgingOption.Integral => 1151958,
			ReforgingOption.GrandArtifice => 1151961,
			ReforgingOption.InspiredArtifice => 1151962,
			ReforgingOption.ExaltedArtifice => 1151963,
			ReforgingOption.SublimeArtifice => 1151964,
			_ => 0
		};
	}

	private int GetTotalCharges()
	{
		var count = 1 + _Options.Count(option => (m_Options & option) != 0);

		return Math.Min(10, count);
	}

	public override void OnResponse(NetState state, RelayInfo info)
	{
		Mobile from = state.Mobile;

		if (!BaseTool.CheckAccessible(m_Tool, from, true))
		{
			return;
		}

		switch (info.ButtonID)
		{
			case 0: break;
			case 1: // prefix
				from.SendGump(new ItemNameGump(m_ToReforge, m_Tool, m_Options, m_Prefix, m_Suffix, true));
				break;
			case 2: // suffix
				from.SendGump(new ItemNameGump(m_ToReforge, m_Tool, m_Options, m_Prefix, m_Suffix, false));
				break;
			case 3: // Reforge Item
			{
				if (!RunicReforging.CanReforge(from, m_ToReforge, m_Tool.CraftSystem))
				{
					return;
				}

				var totalCharges = GetTotalCharges();

				if (m_Tool.UsesRemaining >= totalCharges)
				{
					CraftResourceInfo resInfo = CraftResources.GetInfo(m_Tool.Resource);

					if (resInfo == null)
						return;

					//CraftAttributeInfo attrs = resInfo.AttributeInfo;

					const int min = 10;
					const int max = 80;

					var budget = GetBudget();

					ReforgedPrefix prefix = ReforgedPrefix.None;
					ReforgedSuffix suffix = ReforgedSuffix.None;

					if ((m_Options & ReforgingOption.GrandArtifice) != 0)
					{
						// choosing name 1
						if ((m_Options & ReforgingOption.InspiredArtifice) != 0)
						{
							prefix = m_Prefix;

							if (prefix == ReforgedPrefix.None)
							{
								from.SendLocalizedMessage(1152287); // Re-forging failed. You did not choose a name! Please try again.
								return;
							}
						}
						else
						{
							// Not choosing name 1 or 2
							if ((m_Options & ReforgingOption.SublimeArtifice) == 0)
							{
								// random prefix AND suffix
								if ((m_Options & ReforgingOption.ExaltedArtifice) != 0)
								{
									prefix = RunicReforging.ChooseRandomPrefix(m_ToReforge, budget);
									suffix = RunicReforging.ChooseRandomSuffix(m_ToReforge, budget, m_Prefix);
								}
								else // random prefix OR suffix
								{
									if (0.5 > Utility.RandomDouble())
									{
										prefix = RunicReforging.ChooseRandomPrefix(m_ToReforge, budget);
									}
									else
									{
										suffix = RunicReforging.ChooseRandomSuffix(m_ToReforge, budget, m_Prefix);
									}
								}
							}
						}
					}

					if ((m_Options & ReforgingOption.ExaltedArtifice) != 0)
					{
						if ((m_Options & ReforgingOption.SublimeArtifice) != 0)
						{
							suffix = m_Suffix;

							if (suffix == ReforgedSuffix.None)
							{
								from.SendLocalizedMessage(1152287); // Re-forging failed. You did not choose a name! Please try again.
								return;
							}
						}
						else
						{
							suffix = RunicReforging.ChooseRandomSuffix(m_ToReforge, budget, m_Prefix);
							budget = Math.Min(800, budget + 50);
						}
					}

					// 50% chance to switch prefix/suffix around
					if ((prefix != ReforgedPrefix.None || suffix != ReforgedSuffix.None) && 0.5 > Utility.RandomDouble())
					{
						int pre = (int)prefix;
						int suf = (int)suffix;

						prefix = (ReforgedPrefix)suf;
						suffix = (ReforgedSuffix)pre;
					}

					RunicReforging.ApplyReforgedProperties(m_ToReforge, prefix, suffix, budget, min, max, RunicReforging.GetPropertyCount(m_Tool), 0, m_Tool, m_Options);

					OnAfterReforged(m_ToReforge);
					from.SendLocalizedMessage(1152286); // You re-forge the item!

					from.PlaySound(0x665);

					m_Tool.UsesRemaining -= totalCharges;

					if (m_Tool.UsesRemaining <= 0)
					{
						m_Tool.Delete();
						from.SendLocalizedMessage(1044038); // You have worn out your tool!
					}
				}

				break;
			}
			case 4:
				from.SendGump(new ReforgingHelpGump());
				break;
			default: // Option
			{
				ReforgingOption option = _Options[info.ButtonID - 100];

				if (HasMetPrerequisite(option))
				{
					if ((m_Options & option) == 0)
					{
						m_Options |= option;

						if (m_Prefix != ReforgedPrefix.None && !RunicReforging.HasSelection((int)m_Prefix, m_ToReforge, m_Tool, m_Options, -1, -1))
						{
							m_Prefix = ReforgedPrefix.None;
							m_Context.Contexts[m_Tool].Prefix = ReforgedPrefix.None;
						}

						if (m_Suffix != ReforgedSuffix.None && !RunicReforging.HasSelection((int)m_Suffix, m_ToReforge, m_Tool, m_Options, -1, -1))
						{
							m_Suffix = ReforgedSuffix.None;
							m_Context.Contexts[m_Tool].Suffix = ReforgedSuffix.None;
						}
					}
					else
					{
						m_Options ^= option;
						InvalidatePrerequisite(option);
					}

					m_Context.Contexts[m_Tool].Options = m_Options;
				}

				from.SendGump(new RunicReforgingGump(from, m_ToReforge, m_Tool));
				break;
			}
		}
	}

	private int GetBudget()
	{
		int budget = m_Tool.Resource switch
		{
			CraftResource.DullCopper => 140,
			CraftResource.ShadowIron => 140,
			CraftResource.SpinedLeather => 140,
			CraftResource.OakWood => 140,
			CraftResource.Copper => 350,
			CraftResource.AshWood => 350,
			CraftResource.Bronze => 500,
			CraftResource.YewWood => 500,
			CraftResource.HornedLeather => 500,
			CraftResource.Gold => 600,
			CraftResource.Agapite => 600,
			CraftResource.Heartwood => 600,
			CraftResource.Bloodwood => 600,
			CraftResource.Verite => 700,
			CraftResource.Frostwood => 700,
			CraftResource.BarbedLeather => 700,
			CraftResource.Valorite => 750,
			_ => 140
		};

		if ((m_Options & ReforgingOption.Powerful) != 0)
			budget += 60;

		if ((m_Options & ReforgingOption.Structural) != 0)
			budget += 60;

		if ((m_Options & ReforgingOption.Fundamental) != 0)
			budget += 100;

		return budget;
	}

	private void OnAfterReforged(Item item)
	{
		AosAttributes attr = RunicReforging.GetAosAttributes(item);
		NegativeAttributes neg = RunicReforging.GetNegativeAttributes(item);

		var durability = 0;

		attr = item switch
		{
			BaseWeapon weapon => weapon.Attributes,
			BaseArmor armor => armor.Attributes,
			_ => attr
		};

		if (attr != null && (m_Options & ReforgingOption.Structural) != 0)
		{
			if (neg != null)
				neg.Brittle = 1;

			if ((m_Options & ReforgingOption.Fortified) != 0)
				durability = 150;

			if (item is BaseArmor or BaseClothing)
				item.Hue = 2500;
		}

		if ((m_Options & ReforgingOption.Fundamental) != 0)
		{
			if (neg != null)
				neg.NoRepair = 1;

			durability = (m_Options & ReforgingOption.Integral) != 0 ? 255 : 200;

			if (item.Hue == 0 && (item is BaseArmor || item is BaseClothing))
				item.Hue = 2500;
		}

		if (durability > 0 && item is IDurability durability1)
		{
			durability1.MaxHitPoints = durability;
			durability1.HitPoints = durability;
		}

		RunicReforging.ApplyItemPower(item, true);
	}

	private class ItemNameGump : Gump
	{
		private readonly BaseRunicTool m_Tool;
		private readonly Item m_ToReforge;
		private readonly ReforgingOption m_Options;
		private ReforgedPrefix m_Prefix;
		private ReforgedSuffix m_Suffix;
		private readonly bool m_IsPrefix;

		private const int White = 0x6F7B;
		private const int Green = 0x4BB2;
		private const int Yellow = 0x6B55;

		public ItemNameGump(Item toreforge, BaseRunicTool tool, ReforgingOption options, ReforgedPrefix prefix, ReforgedSuffix suffix, bool isprefix)
			: base(100, 100)
		{
			m_Tool = tool;
			m_ToReforge = toreforge;
			m_Options = options;
			m_Prefix = prefix;
			m_Suffix = suffix;
			m_IsPrefix = isprefix;

			AddBackground(0, 0, 370, 440, 83);

			AddHtmlLocalized(10, 10, 350, 18, 1114513, "#1152089", 0x4BB7, false, false); // Runic Crafting - Item Name Selection

			var y = 50;

			foreach (int i in Enum.GetValues(typeof(ReforgedPrefix)))
			{
				if (i == 0)
					continue;

				int buttonId;
				int buttonHue;
				if ((isprefix && prefix == (ReforgedPrefix)i) || (!isprefix && suffix == (ReforgedSuffix)i))
				{
					buttonId = 4006;
					buttonHue = Green;
				}
				else
				{
					buttonId = 4005;
					buttonHue = White;
				}

				if (RunicReforging.HasSelection(i, toreforge, tool, m_Options, (int)m_Prefix, (int)m_Suffix))
				{
					AddButton(15, y, buttonId, buttonId, 100 + i, GumpButtonType.Reply, 0);
				}
				else
				{
					buttonHue = Yellow;
				}

				AddHtmlLocalized(55, y, 250, 20, RunicReforging.GetName(i), buttonHue, false, false);

				y += 25;
			}

			AddHtmlLocalized(45, 412, 100, 20, 1060675, White, false, false);
			AddButton(10, 412, 4017, 4019, 0, GumpButtonType.Reply, 0);
		}

		public override void OnResponse(NetState state, RelayInfo info)
		{
			Mobile from = state.Mobile;

			if (info.ButtonID == 0)
				return;

			var index = info.ButtonID - 100;

			if (index is >= 0 and <= 12 && RunicReforging.HasSelection(index, m_ToReforge, m_Tool, m_Options, (int)m_Prefix, (int)m_Suffix))
			{
				ReforgingContext context = ReforgingContext.GetContext(from);

				if (m_IsPrefix)
				{
					context.Contexts[m_Tool].Prefix = (ReforgedPrefix)index;
					m_Prefix = (ReforgedPrefix)index;
				}
				else
				{
					context.Contexts[m_Tool].Suffix = (ReforgedSuffix)index;
					m_Suffix = (ReforgedSuffix)index;
				}
			}

			from.SendGump(new RunicReforgingGump(from, m_ToReforge, m_Tool));
		}
	}

	private class ReforgingHelpGump : Gump
	{
		public ReforgingHelpGump()
			: base(100, 100)
		{
			AddBackground(0, 0, 370, 440, 83);

			AddHtmlLocalized(10, 10, 350, 18, 1114513, "#1151966", 0x4BB7, false, false); // Runic Crafting Help
			AddHtmlLocalized(10, 40, 353, 365, 1151965, 0xFFE0, false, true);

			AddHtmlLocalized(45, 412, 100, 20, 1060675, 0x6F7B, false, false);
			AddButton(10, 412, 4017, 4019, 0, GumpButtonType.Reply, 0);
		}
	}
}
