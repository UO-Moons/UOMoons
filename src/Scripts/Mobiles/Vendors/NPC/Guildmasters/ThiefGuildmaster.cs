using Server.Items;
using System;

namespace Server.Mobiles;

public class ThiefGuildmaster : BaseGuildmaster
{
	public override NpcGuild NpcGuild => NpcGuild.ThievesGuild;

	public override TimeSpan JoinAge => TimeSpan.FromDays(7.0);

	[Constructable]
	public ThiefGuildmaster() : base("thief")
	{
		SetSkill(SkillName.DetectHidden, 75.0, 98.0);
		SetSkill(SkillName.Hiding, 65.0, 88.0);
		SetSkill(SkillName.Lockpicking, 85.0, 100.0);
		SetSkill(SkillName.Snooping, 90.0, 100.0);
		SetSkill(SkillName.Poisoning, 60.0, 83.0);
		SetSkill(SkillName.Stealing, 90.0, 100.0);
		SetSkill(SkillName.Fencing, 75.0, 98.0);
		SetSkill(SkillName.Stealth, 85.0, 100.0);
		SetSkill(SkillName.RemoveTrap, 85.0, 100.0);
	}

	public override void InitOutfit()
	{
		base.InitOutfit();

		if (Utility.RandomBool())
			AddItem(new Kryss());
		else
			AddItem(new Dagger());
	}

	public override bool CheckCustomReqs(PlayerMobile pm)
	{
		if (pm.Young)
		{
			SayTo(pm, 502089); // You cannot be a member of the Thieves' Guild while you are Young.
			return false;
		}
		else if (pm.Kills > 0)
		{
			SayTo(pm, 501050); // This guild is for cunning thieves, not oafish cutthroats.
			return false;
		}
		else if (pm.Skills[SkillName.Stealing].Base < 60.0)
		{
			SayTo(pm, 501051); // You must be at least a journeyman pickpocket to join this elite organization.
			return false;
		}

		return true;
	}

	public override void SayWelcomeTo(Mobile m)
	{
		SayTo(m, 1008053); // Welcome to the guild! Stay to the shadows, friend.
	}

	public override bool HandlesOnSpeech(Mobile from)
	{
		return from.InRange(Location, 2) || base.HandlesOnSpeech(from);
	}

	public override void OnSpeech(SpeechEventArgs e)
	{
		Mobile from = e.Mobile;

		if (!e.Handled && from is PlayerMobile mobile && mobile.InRange(Location, 2) && e.HasKeyword(0x1F)) // *disguise*
		{
			SayTo(mobile, mobile.NpcGuild == NpcGuild.ThievesGuild ? 501839 : 501838);

			e.Handled = true;
		}

		base.OnSpeech(e);
	}

	public override bool OnGoldGiven(Mobile from, Gold dropped)
	{
		if (from is not PlayerMobile mobile || dropped.Amount != 700) return base.OnGoldGiven(from, dropped);

		if (mobile.NpcGuild != NpcGuild.ThievesGuild) return base.OnGoldGiven(mobile, dropped);
		mobile.AddToBackpack(new DisguiseKit());

		dropped.Delete();
		return true;

	}

	public ThiefGuildmaster(Serial serial) : base(serial)
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
