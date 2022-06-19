using Server.Items;
using Server.Network;
using System;
using System.Collections.Generic;

namespace Server.Mobiles;

public abstract class BaseGuildmaster : BaseVendor
{
	protected override List<SbInfo> SbInfos { get; } = new();

	public override bool IsActiveVendor => false;

	public override bool ClickTitle => false;

	public virtual int JoinCost => 500;

	public virtual TimeSpan JoinAge => TimeSpan.FromDays(0.0);
	public virtual TimeSpan JoinGameAge => TimeSpan.FromDays(2.0);
	public virtual TimeSpan QuitAge => TimeSpan.FromDays(7.0);
	public virtual TimeSpan QuitGameAge => TimeSpan.FromDays(4.0);

	public override void InitSbInfo()
	{
	}

	public virtual bool CheckCustomReqs(PlayerMobile pm)
	{
		return true;
	}

	public virtual void SayGuildTo(Mobile m)
	{
		SayTo(m, 1008055 + (int)NpcGuild);
	}

	public virtual void SayWelcomeTo(Mobile m)
	{
		SayTo(m, 1008054); // Welcome to the guild! Thou shalt find that fellow members shall grant thee lower prices in shops.
	}

	public virtual void SayPriceTo(Mobile m)
	{
		m.Send(new MessageLocalizedAffix(Serial, Body, MessageType.Regular, SpeechHue, 3, 1008052, Name, AffixType.Append, JoinCost.ToString(), ""));
	}

	public virtual bool WasNamed(string speech)
	{
		string name = Name;

		return (name != null && Insensitive.StartsWith(speech, name));
	}

	public override bool HandlesOnSpeech(Mobile from)
	{
		return from.InRange(Location, 2) || base.HandlesOnSpeech(from);
	}

	public override void OnSpeech(SpeechEventArgs e)
	{
		if (!e.Handled && e.Mobile is var from and PlayerMobile && from.InRange(Location, 2) && WasNamed(e.Speech))
		{
			PlayerMobile pm = (PlayerMobile)from;

			if (e.HasKeyword(0x0004)) // *join* | *member*
			{
				if (pm.NpcGuild == NpcGuild)
					SayTo(from, 501047); // Thou art already a member of our guild.
				else if (pm.NpcGuild != NpcGuild.None)
					SayTo(from, 501046); // Thou must resign from thy other guild first.
				else if (pm.GameTime < JoinGameAge || (pm.CreationTime + JoinAge) > DateTime.UtcNow)
					SayTo(from, 501048); // You are too young to join my guild...
				else if (CheckCustomReqs(pm))
					SayPriceTo(from);

				e.Handled = true;
			}
			else if (e.HasKeyword(0x0005)) // *resign* | *quit*
			{
				if (pm.NpcGuild != NpcGuild)
				{
					SayTo(from, 501052); // Thou dost not belong to my guild!
				}
				else if (pm.NpcGuildJoinTime + QuitAge > DateTime.UtcNow || pm.NpcGuildGameTime + QuitGameAge > pm.GameTime)
				{
					SayTo(from, 501053); // You just joined my guild! You must wait a week to resign.
				}
				else
				{
					SayTo(from, 501054); // I accept thy resignation.
					pm.NpcGuild = NpcGuild.None;
				}

				e.Handled = true;
			}
		}

		base.OnSpeech(e);
	}

	public override bool OnGoldGiven(Mobile from, Gold dropped)
	{
		if (from is not PlayerMobile mobile || dropped.Amount != JoinCost) return base.OnGoldGiven(from, dropped);

		if (mobile.NpcGuild == NpcGuild)
		{
			SayTo(mobile, 501047); // Thou art already a member of our guild.
		}
		else if (mobile.NpcGuild != NpcGuild.None)
		{
			SayTo(mobile, 501046); // Thou must resign from thy other guild first.
		}
		else if (mobile.GameTime < JoinGameAge || (mobile.CreationTime + JoinAge) > DateTime.UtcNow)
		{
			SayTo(mobile, 501048); // You are too young to join my guild...
		}
		else if (CheckCustomReqs(mobile))
		{
			SayWelcomeTo(mobile);

			mobile.NpcGuild = NpcGuild;
			mobile.NpcGuildJoinTime = DateTime.UtcNow;
			mobile.NpcGuildGameTime = mobile.GameTime;

			dropped.Delete();
			return true;
		}

		return false;

	}

	public BaseGuildmaster(string title) : base(title)
	{
		Title = $"the {title} {(Female ? "guildmistress" : "guildmaster")}";
	}

	public BaseGuildmaster(Serial serial) : base(serial)
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
