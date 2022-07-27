using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Server.Network;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Items;

[Flipable(0x1E5E, 0x1E5F)]
public class BulletinBoard : BaseBulletinBoard
{
	[Constructable]
	public BulletinBoard() : base(0x1E5E)
	{
	}

	public BulletinBoard(Serial serial) : base(serial)
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

[Flipable(0x1E5E, 0x1E5F)]
public class BountyBoard : BaseBulletinBoard
{
	[Constructable]
	public BountyBoard() : base(0x1E5E)
	{
		BoardName = "Bounty Board";
	}

	public BountyBoard(Serial serial) : base(serial)
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

	public override void Cleanup()
	{
		// no cleanup
	}

	public override DateTime GetLastPostTime(Mobile poster, bool onlyCheckRoot)
	{
		return DateTime.MinValue;
	}

	public override DateTime GetLastPostTime(BulletinMessage check)
	{
		return check.Time;
	}

	public override void PostMessage(Mobile from, BulletinMessage thread, string subject, string[] lines)
	{
		from.SendAsciiMessage("This board is for automated bounty postings only.  For communications you should use the forums at http://www.uomoons.com");
	}

	private const int BountyCount = 25;
	private static PlayerMobile[] _list;
	private static PlayerMobile[] _oldList;
	private static bool _updateMsgs;

	static BountyBoard()
	{
		_list = new PlayerMobile[BountyCount];
		_oldList = new PlayerMobile[BountyCount];
		_updateMsgs = true;
	}

	public static int LowestBounty => _list[BountyCount - 1]?.Bounty ?? 0;

	public static void Update(PlayerMobile pm)
	{
		if (pm.AccessLevel > AccessLevel.Player) return;

		PlayerMobile[] newList = _oldList;
		int ni = 0;
		int ins = -1;
		for (int i = 0; i < BountyCount; i++)
		{
			if (_list[i] == null)
			{
				if (ins == -1)
					ins = ni;
				break; // we reached the end of the list
			}

			if (pm == _list[i] || _list[i].Bounty <= 0 || _list[i].Kills <= 0)
			{
				// we are already in the array, or someone needs to be removed
				_updateMsgs = true;
			}
			else //if ( pm != m_List[i] )
			{
				if (ins == -1 && _list[i].Bounty <= pm.Bounty)
					ins = ni++;
				if (ni < BountyCount)
					newList[ni++] = _list[i];
			}

			_list[i] = null;
		}

		if (ins is >= 0 and < BountyCount)
		{
			newList[ins] = pm;
			_updateMsgs = true;
		}
		_oldList = _list;
		_list = newList;
	}

	public override void OnSingleClick(Mobile from)
	{
		GetMessages(); // check for update
		LabelTo(from,
			$"a bounty board with {BountyMessage.List.Count} posted bount{(BountyMessage.List.Count != 1 ? "ies" : "y")}");
	}

	public override ArrayList GetMessages()
	{
		if (_updateMsgs)
		{
			ArrayList del = new();
			ArrayList list = BountyMessage.List;
			for (int i = 0; i < _list.Length; i++)
			{
				BountyMessage post;
				if (_list[i] == null || _list[i].Kills <= 0 || _list[i].Bounty <= 0)
				{
					if (i < list.Count)
					{
						post = (BountyMessage)list[i];
						if (post is { Deleted: false })
							del.Add(post);
					}
					continue;
				}

				if (i < list.Count)
					post = (BountyMessage)list[i];
				else
					post = new BountyMessage(); // automatically adds itself to the list

				if (post == null)
					continue;

				post.Time = DateTime.MinValue + TimeSpan.FromTicks(_list[i].Kills); //DateTime.Now;
				post.PostedName = "";
				post.PostedBody = 0x0190;
				post.PostedHue = 0x83EA;
				if (post.PostedEquip.Length > 0)
					post.PostedEquip = Array.Empty<BulletinEquip>();
				post.Poster = null;
				post.Thread = null;
				post.Subject = $"{_list[i].Name}: {_list[i].Bounty}gp";
				post.FormatMessage(
					"A price on {0}!\n  The foul scum known as {0} hath murdered one too many! For {1} is guilty of {2} murder{3}.\n  A bounty of {4}gp is hereby offered for {5} head!\n  If you kill {0}, bring {5} head to a guard here in this city to claim your reward.",
					_list[i].Name, _list[i].Female ? "she" : "he", _list[i].Kills, _list[i].Kills != 1 ? "s" : "",
					_list[i].Bounty, _list[i].Female ? "her" : "his");
			}

			for (int i = 0; i < del.Count; i++)
				((Item)del[i])?.Delete();

			if (list.Count > _list.Length)
				BountyMessage.RemoveRange(_list.Length, list.Count - _list.Length);
			_updateMsgs = false;
			return list;
		}

		return BountyMessage.List;
	}

	public override bool MessageOk(BulletinMessage msg)
	{
		return BountyMessage.List.Contains(msg);
	}
}

public class BountyMessage : BulletinMessage
{
	private static ArrayList _list;
	public static ArrayList List => _list ??= new ArrayList();

	public static void RemoveRange(int index, int count)
	{
		if (index < 0 || index >= List.Count || count <= 0)
			return;

		ArrayList oldList = new(List);
		int top = index + count;
		if (top > oldList.Count)
			top = oldList.Count;
		for (int i = index; i < top; i++)
			((BountyMessage)oldList[i])?.Delete();
	}

	public BountyMessage()
	{
		List.Add(this);
	}

	public BountyMessage(Serial serial) : base(serial)
	{
	}

	public override void OnAfterDelete()
	{
		List.Remove(this);
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		List.Add(this);
		_ = reader.ReadInt();
	}
}

public abstract class BaseBulletinBoard : BaseItem
{
	[CommandProperty(AccessLevel.GameMaster)]
	public string BoardName { get; set; }

	public BaseBulletinBoard(int itemId) : base(itemId)
	{
		BoardName = "bulletin board";
		Movable = false;
	}

	// Threads will be removed six hours after the last post was made
	private static readonly TimeSpan ThreadDeletionTime = TimeSpan.FromHours(6.0);

	// A player may only create a thread once every two minutes
	private static readonly TimeSpan ThreadCreateTime = TimeSpan.FromMinutes(2.0);

	// A player may only reply once every thirty seconds
	private static readonly TimeSpan ThreadReplyTime = TimeSpan.FromSeconds(30.0);

	private static bool CheckTime(DateTime time, TimeSpan range)
	{
		return time + range < DateTime.UtcNow;
	}

	private static string FormatTs(TimeSpan ts)
	{
		int totalSeconds = (int)ts.TotalSeconds;
		int seconds = totalSeconds % 60;
		int minutes = totalSeconds / 60;

		if (minutes != 0 && seconds != 0)
			return $"{minutes} minute{(minutes == 1 ? "" : "s")} and {seconds} second{(seconds == 1 ? "" : "s")}";
		return minutes != 0 ? $"{minutes} minute{(minutes == 1 ? "" : "s")}" : $"{seconds} second{(seconds == 1 ? "" : "s")}";
	}

	public virtual void Cleanup()
	{
		List<Item> items = Items;

		for (int i = items.Count - 1; i >= 0; --i)
		{
			if (i >= items.Count)
				continue;

			if (items[i] is not BulletinMessage msg)
				continue;

			if (msg.Thread != null || !CheckTime(msg.LastPostTime, ThreadDeletionTime)) continue;
			msg.Delete();
			RecurseDelete(msg); // A root-level thread has expired
		}
	}

	private void RecurseDelete(BulletinMessage msg)
	{
		List<Item> found = new();
		List<Item> items = Items;

		for (int i = items.Count - 1; i >= 0; --i)
		{
			if (i >= items.Count)
				continue;

			if (items[i] is not BulletinMessage check)
				continue;

			if (check.Thread == msg)
			{
				check.Delete();
				found.Add(check);
			}
		}

		for (int i = 0; i < found.Count; ++i)
			RecurseDelete((BulletinMessage)found[i]);
	}

	public virtual bool GetLastPostTime(Mobile poster, bool onlyCheckRoot, ref DateTime lastPostTime)
	{
		List<Item> items = Items;
		bool wasSet = false;

		for (int i = 0; i < items.Count; ++i)
		{
			if (items[i] is not BulletinMessage msg || msg.Poster != poster)
				continue;

			if (onlyCheckRoot && msg.Thread != null)
				continue;

			if (msg.Time <= lastPostTime) continue;
			wasSet = true;
			lastPostTime = msg.Time;
		}

		return wasSet;
	}

	public virtual DateTime GetLastPostTime(Mobile poster, bool onlyCheckRoot)
	{
		DateTime lastPostTime = DateTime.MinValue;
		for (int i = 0; i < Items.Count; ++i)
		{
			if (Items[i] is not BulletinMessage msg || msg.Poster != poster)
				continue;

			if (onlyCheckRoot && msg.Thread != null)
				continue;

			if (msg.Time > lastPostTime)
				lastPostTime = msg.Time;
		}

		return lastPostTime;
	}

	public virtual DateTime GetLastPostTime(BulletinMessage check)
	{
		DateTime lastPostTime = check.Time;
		for (int i = 0; i < Items.Count; ++i)
		{
			if (Items[i] is not BulletinMessage msg || msg.Thread != check)
				continue;

			if (msg.Time > lastPostTime)
				lastPostTime = msg.Time;
		}
		return lastPostTime;
	}

	public virtual ArrayList GetMessages()
	{
		return new ArrayList(Items);
	}

	public virtual bool MessageOk(BulletinMessage msg)
	{
		return msg.Parent == this;
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (CheckRange(from))
		{
			Cleanup();

			NetState state = from.NetState;

			state.Send(new BbDisplayBoard(this));
			ContainerContent.Send(state, this);
			//if (state.ContainerGridLines)
			//	state.Send(new ContainerContent6017(from, this));
			//else
			//	state.Send(new ContainerContent(from, this));
		}
		else
		{
			from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
		}
	}

	public virtual bool CheckRange(Mobile from)
	{
		if (from.AccessLevel >= AccessLevel.GameMaster)
			return true;

		return from.Map == Map && from.InRange(GetWorldLocation(), 2);
	}

	public virtual void PostMessage(Mobile from, BulletinMessage thread, string subject, string[] lines)
	{
		if (thread != null)
			thread.LastPostTime = DateTime.UtcNow;

		AddItem(new BulletinMessage(from, thread, subject, lines));
	}

	public BaseBulletinBoard(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version

		writer.Write(BoardName);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		int version = reader.ReadInt();

		switch (version)
		{
			case 0:
			{
				BoardName = reader.ReadString();
				break;
			}
		}
	}

	public static void Initialize()
	{
		PacketHandlers.Register(0x71, 0, true, BbClientRequest);
	}

	private static void BbClientRequest(NetState state, PacketReader pvSrc)
	{
		Mobile from = state.Mobile;

		int packetId = pvSrc.ReadByte();

		if (World.FindItem(pvSrc.ReadSerial()) is not BaseBulletinBoard board || !board.CheckRange(from))
			return;

		switch (packetId)
		{
			case 3: BbRequestContent(from, board, pvSrc); break;
			case 4: BbRequestHeader(from, board, pvSrc); break;
			case 5: BbPostMessage(from, board, pvSrc); break;
			case 6: BbRemoveMessage(from, board, pvSrc); break;
		}
	}

	private static void BbRequestContent(Mobile from, BaseBulletinBoard board, PacketReader pvSrc)
	{
		if (World.FindItem(pvSrc.ReadSerial()) is not BulletinMessage msg || msg.Parent != board)
			return;

		from.Send(new BbMessageContent(board, msg));
	}

	private static void BbRequestHeader(Mobile from, BaseBulletinBoard board, PacketReader pvSrc)
	{
		if (World.FindItem(pvSrc.ReadSerial()) is not BulletinMessage msg || msg.Parent != board)
			return;

		from.Send(new BbMessageHeader(board, msg));
	}

	private static void BbPostMessage(Mobile from, BaseBulletinBoard board, PacketReader pvSrc)
	{
		BulletinMessage thread = World.FindItem(pvSrc.ReadSerial()) as BulletinMessage;

		if (thread != null && thread.Parent != board)
			thread = null;

		int breakout = 0;

		while (thread is { Thread: { } } && breakout++ < 10)
			thread = thread.Thread;

		DateTime lastPostTime = DateTime.MinValue;

		if (board.GetLastPostTime(from, thread == null, ref lastPostTime))
		{
			if (!CheckTime(lastPostTime, thread == null ? ThreadCreateTime : ThreadReplyTime))
			{
				if (thread == null)
					from.SendMessage("You must wait {0} before creating a new thread.", FormatTs(ThreadCreateTime));
				else
					from.SendMessage("You must wait {0} before replying to another thread.", FormatTs(ThreadReplyTime));

				return;
			}
		}

		string subject = pvSrc.ReadUTF8StringSafe(pvSrc.ReadByte());

		if (subject.Length == 0)
			return;

		string[] lines = new string[pvSrc.ReadByte()];

		if (lines.Length == 0)
			return;

		for (int i = 0; i < lines.Length; ++i)
			lines[i] = pvSrc.ReadUTF8StringSafe(pvSrc.ReadByte());

		board.PostMessage(from, thread, subject, lines);
	}

	private static void BbRemoveMessage(Mobile from, BaseBulletinBoard board, PacketReader pvSrc)
	{
		if (World.FindItem(pvSrc.ReadSerial()) is not BulletinMessage msg || msg.Parent != board)
			return;

		if (from.AccessLevel < AccessLevel.GameMaster && msg.Poster != from)
			return;

		msg.Delete();
	}
}

public struct BulletinEquip
{
	public int ItemId;
	public int Hue;

	public BulletinEquip(int itemId, int hue)
	{
		ItemId = itemId;
		Hue = hue;
	}
}

public class BulletinMessage : BaseItem
{
	public string GetTimeAsString()
	{
		return Time.ToString("MMM dd, yyyy");
	}

	public string GetHeaderTime()
	{
		long kills = (Time - DateTime.MinValue).Ticks;
		return $"{kills} kill{(kills != 1 ? "s" : "")}";
	}

	public override bool CheckTarget(Mobile from, Target targ, object targeted)
	{
		return false;
	}

	public override bool IsAccessibleTo(Mobile check)
	{
		return false;
	}

	public BulletinMessage() : this(null, null, "", Array.Empty<string>())
	{
	}

	public BulletinMessage(Mobile poster, BulletinMessage thread, string subject, string[] lines) : base(0xEB0)
	{
		Movable = false;

		Poster = poster;
		Subject = subject;
		Time = DateTime.UtcNow;
		LastPostTime = Time;
		Thread = thread;
		if (Poster == null)
		{
			PostedName = "";
			PostedBody = 0x0190;
			PostedHue = 0x83EA;
			PostedEquip = Array.Empty<BulletinEquip>();
		}
		else
		{
			PostedName = Poster.Name;
			PostedBody = Poster.Body;
			PostedHue = Poster.Hue;
			Lines = lines;

			PostedEquip = (from item in poster.Items where item.Layer is >= Layer.OneHanded and <= Layer.Mount select new BulletinEquip(item.ItemId, item.Hue)).ToArray();
		}
	}

	public Mobile Poster { get; set; }
	public BulletinMessage Thread { get; set; }
	public string Subject { get; set; }
	public DateTime Time { get; set; }
	public DateTime LastPostTime { get; set; }
	public string PostedName { get; set; }
	public int PostedBody { get; set; }
	public int PostedHue { get; set; }
	public BulletinEquip[] PostedEquip { get; set; }
	public string[] Lines { get; private set; }

	public void FormatMessage(string fmt, params object[] args)
	{
		FormatMessage(string.Format(fmt, args));
	}

	public virtual void FormatMessage(string msg)
	{
		StringBuilder sb = new(msg.Length + 32);
		int len = 0;
		int space = -1;
		int i = 0;

		while (i < msg.Length)
		{
			char ch = msg[i];
			sb.Append(ch);
			len++; i++;

			if (ch == ' ' || ch == '-')
			{
				space = sb.Length;
			}
			else if (ch == '\n')
			{
				len = 0;
				space = -1;
				sb.Append('\r');
			}
			else if (len >= 30)
			{
				if (space != -1)
				{
					len = 2 + sb.Length - space;
					sb.Insert(space, "\n\r");
				}
				else
				{
					len = 0;
					sb.Append("\n\r");
				}
				space = -1;
			}
		}

		if (len != 0)
			sb.Append("\n\r");

		Lines = sb.ToString().Split('\r');
	}

	public BulletinMessage(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version

		writer.Write(Poster);
		writer.Write(Subject);
		writer.Write(Time);
		writer.Write(LastPostTime);
		writer.Write(Thread != null);
		writer.Write(Thread);
		writer.Write(PostedName);
		writer.Write(PostedBody);
		writer.Write(PostedHue);

		writer.Write(PostedEquip.Length);

		for (int i = 0; i < PostedEquip.Length; ++i)
		{
			writer.Write(PostedEquip[i].ItemId);
			writer.Write(PostedEquip[i].Hue);
		}

		writer.Write(Lines.Length);

		for (int i = 0; i < Lines.Length; ++i)
			writer.Write(Lines[i]);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		int version = reader.ReadInt();

		switch (version)
		{
			case 0:
			{
				Poster = reader.ReadMobile();
				Subject = reader.ReadString();
				Time = reader.ReadDateTime();
				LastPostTime = reader.ReadDateTime();
				bool hasThread = reader.ReadBool();
				Thread = reader.ReadItem() as BulletinMessage;
				PostedName = reader.ReadString();
				PostedBody = reader.ReadInt();
				PostedHue = reader.ReadInt();

				PostedEquip = new BulletinEquip[reader.ReadInt()];

				for (int i = 0; i < PostedEquip.Length; ++i)
				{
					PostedEquip[i].ItemId = reader.ReadInt();
					PostedEquip[i].Hue = reader.ReadInt();
				}

				Lines = new string[reader.ReadInt()];

				for (int i = 0; i < Lines.Length; ++i)
					Lines[i] = reader.ReadString();

				if (hasThread && Thread == null)
					Delete();

				break;
			}
		}
	}

	public void Validate()
	{
		if (!(Parent is BulletinBoard board && board.Items.Contains(this)))
			Delete();
	}
}

public class BbDisplayBoard : Packet
{
	public BbDisplayBoard(BaseBulletinBoard board) : base(0x71)
	{
		string name = board.BoardName ?? "";

		EnsureCapacity(38);

		byte[] buffer = Utility.UTF8.GetBytes(name);

		m_Stream.Write((byte)0x00); // PacketID
		m_Stream.Write(board.Serial); // Bulletin board serial

		// Bulletin board name
		if (buffer.Length >= 29)
		{
			m_Stream.Write(buffer, 0, 29);
			m_Stream.Write((byte)0);
		}
		else
		{
			m_Stream.Write(buffer, 0, buffer.Length);
			m_Stream.Fill(30 - buffer.Length);
		}
	}
}

public class BbMessageHeader : Packet
{
	public BbMessageHeader(BaseBulletinBoard board, BulletinMessage msg) : base(0x71)
	{
		string poster = SafeString(msg.PostedName);
		string subject = SafeString(msg.Subject);
		string time = SafeString(msg.GetTimeAsString());

		EnsureCapacity(22 + poster.Length + subject.Length + time.Length);

		m_Stream.Write((byte)0x01); // PacketID
		m_Stream.Write(board.Serial); // Bulletin board serial
		m_Stream.Write(msg.Serial); // Message serial

		BulletinMessage thread = msg.Thread;

		m_Stream.Write(thread?.Serial ?? 0);

		WriteString(poster);
		WriteString(subject);
		WriteString(time);
	}

	private void WriteString(string v)
	{
		byte[] buffer = Utility.UTF8.GetBytes(v);
		int len = buffer.Length + 1;

		if (len > 255)
			len = 255;

		m_Stream.Write((byte)len);
		m_Stream.Write(buffer, 0, len - 1);
		m_Stream.Write((byte)0);
	}

	private static string SafeString(string v)
	{
		return v ?? string.Empty;
	}
}

public class BbMessageContent : Packet
{
	public BbMessageContent(BaseBulletinBoard board, BulletinMessage msg) : base(0x71)
	{
		string poster = SafeString(msg.PostedName);
		string subject = SafeString(msg.Subject);
		string time = SafeString(msg.GetTimeAsString());

		EnsureCapacity(22 + poster.Length + subject.Length + time.Length);

		m_Stream.Write((byte)0x02); // PacketID
		m_Stream.Write(board.Serial); // Bulletin board serial
		m_Stream.Write(msg.Serial); // Message serial

		WriteString(poster);
		WriteString(subject);
		WriteString(time);

		m_Stream.Write((short)msg.PostedBody);
		m_Stream.Write((short)msg.PostedHue);

		int len = msg.PostedEquip.Length;

		if (len > 255)
			len = 255;

		m_Stream.Write((byte)len);

		for (int i = 0; i < len; ++i)
		{
			BulletinEquip eq = msg.PostedEquip[i];

			m_Stream.Write((short)eq.ItemId);
			m_Stream.Write((short)eq.Hue);
		}

		len = msg.Lines.Length;

		if (len > 255)
			len = 255;

		m_Stream.Write((byte)len);

		for (int i = 0; i < len; ++i)
			WriteString(msg.Lines[i], true);
	}

	private void WriteString(string v, bool padding = false)
	{
		byte[] buffer = Utility.UTF8.GetBytes(v);
		int tail = padding ? 2 : 1;
		int len = buffer.Length + tail;

		if (len > 255)
			len = 255;

		m_Stream.Write((byte)len);
		m_Stream.Write(buffer, 0, len - tail);

		if (padding)
			m_Stream.Write((short)0);
		else
			m_Stream.Write((byte)0);
	}

	private static string SafeString(string v)
	{
		return v ?? string.Empty;
	}
}
