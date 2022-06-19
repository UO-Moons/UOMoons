using Server.Engines.Quests;
using Server.Gumps;
using System;
using System.Collections.Generic;

namespace Server.Mobiles;

public class HumilityQuestMobile : BaseVendor
{
	public virtual int Greeting => 0;

	public override bool IsActiveVendor => false;
	public override bool IsInvulnerable => true;
	public override bool CanTeach => false;
	public override bool PlayerRangeSensitive => false;

	private List<SbInfo> m_SBInfos = new();
	protected override List<SbInfo> SbInfos => m_SBInfos;

	public override void InitSbInfo()
	{
	}

	public HumilityQuestMobile(string name) : base(null)
	{
		Name = name;
		m_NextGreet = DateTime.UtcNow;
	}

	public HumilityQuestMobile(string name, string title) : base(title)
	{
		Name = name;
		m_NextGreet = DateTime.UtcNow;
	}

	public static List<HumilityQuestMobile> Instance { get; } = new List<HumilityQuestMobile>();

	public HumilityQuestMobile(Serial serial) : base(serial)
	{
	}

	private DateTime m_NextGreet;

	public override void OnMovement(Mobile m, Point3D oldLocation)
	{
		if (m is not PlayerMobile pm || !pm.InRange(Location, 3))
			return;

		if (QuestHelper.GetQuest(pm, typeof(WhosMostHumbleQuest)) is WhosMostHumbleQuest)
		{
			if (m_NextGreet < DateTime.UtcNow && pm is not null)
			{
				Item item = pm.FindItemOnLayer(Layer.Cloak);

				if (item is GreyCloak && /*((GreyCloak)item).Owner == null &&*/ Greeting > 0)
				{
					SayTo(pm, Greeting);

					m_NextGreet = DateTime.UtcNow + TimeSpan.FromSeconds(5);
				}
			}
		}
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (from is not PlayerMobile pm || !InRange(from.Location, 3))
			return;

		if (QuestHelper.GetQuest(pm, typeof(WhosMostHumbleQuest)) is WhosMostHumbleQuest quest && pm.Backpack != null && !quest.HasGivenTo(this))
		{
			Item item = from.FindItemOnLayer(Layer.Cloak);

			if (item is GreyCloak /*&& ((GreyCloak)item).Owner == null*/)
			{
				int idx = HumilityQuestMobileInfo.GetNPCIndex(GetType());

				if (idx > -1 && quest.Infos.ContainsKey(idx) && idx < quest.Infos.Count)
				{
					Type needs = quest.Infos[idx].Needs;

					Item need = from.Backpack.FindItemByType(needs);

					// Found needed item
					if (need != null)
					{
						need.Delete();
						quest.RemoveQuestItem(need);

						Item nextItem = Loot.Construct(quest.Infos[idx].Gives);

						if (nextItem != null)
						{
							from.Backpack.DropItem(nextItem);
							quest.AddQuestItem(nextItem, this);

							if (this is Sean)
								SayTo(from, Greeting + 3, string.Format("#{0}", quest.Infos[idx].NeedsLoc));
							else
								SayTo(from, Greeting + 4, string.Format("#{0}\t#{1}", quest.Infos[idx].NeedsLoc, quest.Infos[idx].GivesLoc));
						}
					}
					else //Didn't find needed item
					{
						_ = from.SendGump(new HumilityItemQuestGump(this, quest, idx));
					}
				}
				else
					Console.WriteLine("Error finding index for {0}", this);
			}
			else
				base.OnDoubleClick(from);
		}
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		_ = reader.ReadInt();

		m_NextGreet = DateTime.UtcNow;
	}
}

public class HumilityQuestMobileInfo
{// Greeting(say), Greeting2(gump), Desire(say), Gift(say), OnExchange(say)
	public Type Needs { get; }
	public Type Gives { get; }
	public int NeedsLoc { get; }
	public int GivesLoc { get; }

	public HumilityQuestMobileInfo(Type needs, Type gives, int needsLoc, int givesLoc)
	{
		Needs = needs;
		Gives = gives;
		NeedsLoc = needsLoc;
		GivesLoc = givesLoc;
	}

	public HumilityQuestMobileInfo(GenericReader reader)
	{
		_ = reader.ReadInt();

		int needs = reader.ReadInt();
		int gives = reader.ReadInt();

		Needs = ItemTypes[needs];

		if (gives == -1)
			Gives = typeof(IronChain);
		else
			Gives = ItemTypes[gives];

		NeedsLoc = reader.ReadInt();
		GivesLoc = reader.ReadInt();
	}

	public void Serialize(GenericWriter writer)
	{
		writer.Write(0);

		int needs = Array.IndexOf(ItemTypes, Needs);
		writer.Write(needs);

		int gives = Array.IndexOf(ItemTypes, Gives);
		writer.Write(gives);

		writer.Write(NeedsLoc);
		writer.Write(GivesLoc);
	}

	public static int GetNPCIndex(Type type)
	{
		for (int i = 0; i < MobileTypes.Length; i++)
		{
			if (MobileTypes[i] == type)
				return i;
		}

		return -1;
	}

	public static int GetLoc(Type type)
	{
		for (int i = 0; i < ItemTypes.Length; i++)
		{
			if (type == ItemTypes[i])
				return ItemLocs[i];
		}

		return -1;
	}

	public static Type[] ItemTypes { get; } = new Type[]
	{
		typeof(BrassRing),
		typeof(SeasonedSkillet),
		typeof(VillageCauldron),
		typeof(ShortStool),
		typeof(FriendshipMug),
		typeof(WornHammer),
		typeof(PairOfWorkGloves),
		typeof(IronChain)
	};

	public static int[] ItemLocs { get; } = new int[]
	{
		1075778,
		1075774,
		1075775,
		1075776,
		1075777,
		1075779,
		1075780,
		1075788
	};

	public static Type[] MobileTypes { get; } = new Type[]
	{
		typeof(Maribel),
		typeof(Dierdre),
		typeof(Kevin),
		typeof(Jason),
		typeof(Walton),
		typeof(Nelson),
		typeof(Sean)
	};
}