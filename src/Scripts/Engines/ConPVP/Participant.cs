using Server.Mobiles;
using System;
using System.Linq;
using System.Text;

namespace Server.Engines.ConPVP;

public class Participant
{
	public int Count => Players.Length;
	public DuelPlayer[] Players { get; private set; }
	public DuelContext Context { get; }
	public TournyParticipant TournyPart { get; set; }

	public DuelPlayer Find(Mobile mob)
	{
		if (mob is PlayerMobile pm)
		{
			if (pm.DuelContext == Context && pm.DuelPlayer.Participant == this)
				return pm.DuelPlayer;

			return null;
		}

		return Players.FirstOrDefault(t => t != null && t.Mobile == mob);
	}

	public bool Contains(Mobile mob)
	{
		return Find(mob) != null;
	}

	public void Broadcast(int hue, string message, string nonLocalOverhead, string localOverhead)
	{
		for (int i = 0; i < Players.Length; ++i)
		{
			if (Players[i] != null)
			{
				if (message != null)
					Players[i].Mobile.SendMessage(hue, message);

				if (nonLocalOverhead != null)
					Players[i].Mobile.NonlocalOverheadMessage(Network.MessageType.Regular, hue, false, string.Format(nonLocalOverhead, Players[i].Mobile.Name, Players[i].Mobile.Female ? "her" : "his"));

				if (localOverhead != null)
					Players[i].Mobile.LocalOverheadMessage(Network.MessageType.Regular, hue, false, localOverhead);
			}
		}
	}

	public int FilledSlots
	{
		get
		{
			return Players.Count(t => t != null);
		}
	}

	public bool HasOpenSlot
	{
		get
		{
			return Players.Any(t => t == null);
		}
	}

	public bool Eliminated
	{
		get
		{
			return Players.All(t => t == null || t.Eliminated);
		}
	}

	public string NameList
	{
		get
		{
			StringBuilder sb = new();

			for (int i = 0; i < Players.Length; ++i)
			{
				if (Players[i] == null)
					continue;

				Mobile mob = Players[i].Mobile;

				if (sb.Length > 0)
					sb.Append(", ");

				sb.Append(mob.Name);
			}

			return sb.Length == 0 ? "Empty" : sb.ToString();
		}
	}

	public void Nullify(DuelPlayer player)
	{
		if (player == null)
			return;

		int index = Array.IndexOf(Players, player);

		if (index == -1)
			return;

		Players[index] = null;
	}

	public void Remove(DuelPlayer player)
	{
		if (player == null)
			return;

		int index = Array.IndexOf(Players, player);

		if (index == -1)
			return;

		DuelPlayer[] old = Players;
		Players = new DuelPlayer[old.Length - 1];

		for (int i = 0; i < index; ++i)
			Players[i] = old[i];

		for (int i = index + 1; i < old.Length; ++i)
			Players[i - 1] = old[i];
	}

	public void Remove(Mobile player)
	{
		Remove(Find(player));
	}

	public void Add(Mobile player)
	{
		if (Contains(player))
			return;

		for (int i = 0; i < Players.Length; ++i)
		{
			if (Players[i] == null)
			{
				Players[i] = new DuelPlayer(player, this);
				return;
			}
		}

		Resize(Players.Length + 1);
		Players[^1] = new DuelPlayer(player, this);
	}

	public void Resize(int count)
	{
		DuelPlayer[] old = Players;
		Players = new DuelPlayer[count];

		if (old != null)
		{
			int ct = 0;

			for (int i = 0; i < old.Length; ++i)
			{
				if (old[i] != null && ct < count)
					Players[ct++] = old[i];
			}
		}
	}

	public Participant(DuelContext context, int count)
	{
		Context = context;
		Resize(count);
	}
}

public class DuelPlayer
{
	private bool _eliminated;

	public Mobile Mobile { get; }
	public bool Ready { get; set; }
	public bool Eliminated { get => _eliminated; set { _eliminated = value; if (Participant.Context._Tournament != null && _eliminated) { Participant.Context._Tournament.OnEliminated(this); Mobile.SendEverything(); } } }
	public Participant Participant { get; set; }

	public DuelPlayer(Mobile mob, Participant p)
	{
		Mobile = mob;
		Participant = p;

		if (mob is PlayerMobile mobile)
			mobile.DuelPlayer = this;
	}
}
