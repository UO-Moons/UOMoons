using System.Collections.Generic;
using System.Linq;

namespace Server.Engines.Chat;

public class Channel
{
	private string _name;
	private string _password;
	private readonly List<ChatUser> _users, _banned, _moderators, _voices;
	private bool _voiceRestricted;

	public Channel(string name)
	{
		_name = name;

		_users = new List<ChatUser>();
		_banned = new List<ChatUser>();
		_moderators = new List<ChatUser>();
		_voices = new List<ChatUser>();
	}

	public Channel(string name, string password) : this(name)
	{
		_password = password;
	}

	public string Name
	{
		get => _name;
		set
		{
			SendCommand(ChatCommand.RemoveChannel, _name);
			_name = value;
			SendCommand(ChatCommand.AddChannel, _name);
			SendCommand(ChatCommand.JoinedChannel, _name);
		}
	}

	public string Password
	{
		get => _password;
		set
		{
			string newValue = null;

			if (value != null)
			{
				newValue = value.Trim();

				if (string.IsNullOrEmpty(newValue))
					newValue = null;
			}

			_password = newValue;
		}
	}

	public bool Contains(ChatUser user)
	{
		return _users.Contains(user);
	}

	public bool IsBanned(ChatUser user)
	{
		return _banned.Contains(user);
	}

	public bool CanTalk(ChatUser user)
	{
		return (!_voiceRestricted || _voices.Contains(user) || _moderators.Contains(user));
	}

	public bool IsModerator(ChatUser user)
	{
		return _moderators.Contains(user);
	}

	public bool IsVoiced(ChatUser user)
	{
		return _voices.Contains(user);
	}

	public bool ValidatePassword(string password)
	{
		return (_password == null || Insensitive.Equals(_password, password));
	}

	public bool ValidateModerator(ChatUser user)
	{
		if (user != null && !IsModerator(user))
		{
			user.SendMessage(29); // You must have operator status to do this.
			return false;
		}

		return true;
	}

	public static bool ValidateAccess(ChatUser from, ChatUser target)
	{
		if (from != null && target != null && from.Mobile.AccessLevel < target.Mobile.AccessLevel)
		{
			from.Mobile.SendMessage("Your access level is too low to do this.");
			return false;
		}

		return true;
	}

	public bool AddUser(ChatUser user)
	{
		return AddUser(user, null);
	}

	public bool AddUser(ChatUser user, string password)
	{
		if (Contains(user))
		{
			user.SendMessage(46, _name); // You are already in the conference '%1'.
			return true;
		}

		if (IsBanned(user))
		{
			user.SendMessage(64); // You have been banned from this conference.
			return false;
		}

		if (!ValidatePassword(password))
		{
			user.SendMessage(34); // That is not the correct password.
			return false;
		}

		user.CurrentChannel?.RemoveUser(user); // Remove them from their current channel first

		ChatSystem.SendCommandTo(user.Mobile, ChatCommand.JoinedChannel, _name);

		SendCommand(ChatCommand.AddUserToChannel, user.GetColorCharacter() + user.Username);

		_users.Add(user);
		user.CurrentChannel = this;

		if (user.Mobile.AccessLevel >= AccessLevel.GameMaster || (!AlwaysAvailable && _users.Count == 1))
			AddModerator(user);

		SendUsersTo(user);

		return true;
	}

	public void RemoveUser(ChatUser user)
	{
		if (Contains(user))
		{
			_users.Remove(user);
			user.CurrentChannel = null;

			if (_moderators.Contains(user))
				_moderators.Remove(user);

			if (_voices.Contains(user))
				_voices.Remove(user);

			SendCommand(ChatCommand.RemoveUserFromChannel, user, user.Username);
			ChatSystem.SendCommandTo(user.Mobile, ChatCommand.LeaveChannel);

			if (_users.Count == 0 && !AlwaysAvailable)
				RemoveChannel(this);
		}
	}

	public void AdBan(ChatUser user)
	{
		AddBan(user, null);
	}

	public void AddBan(ChatUser user, ChatUser moderator)
	{
		if (!ValidateModerator(moderator) || !ValidateAccess(moderator, user))
			return;

		if (!_banned.Contains(user))
			_banned.Add(user);

		Kick(user, moderator, true);
	}

	public void RemoveBan(ChatUser user)
	{
		if (_banned.Contains(user))
			_banned.Remove(user);
	}

	public void Kick(ChatUser user)
	{
		Kick(user, null);
	}

	public void Kick(ChatUser user, ChatUser moderator)
	{
		Kick(user, moderator, false);
	}

	public void Kick(ChatUser user, ChatUser moderator, bool wasBanned)
	{
		if (!ValidateModerator(moderator) || !ValidateAccess(moderator, user))
			return;

		if (Contains(user))
		{
			if (moderator != null)
			{
				// %1, a conference moderator, has banned you from the conference.// %1, a conference moderator, has kicked you out of the conference.
				user.SendMessage(wasBanned ? 63 : 45, moderator.Username);
			}

			RemoveUser(user);
			ChatSystem.SendCommandTo(user.Mobile, ChatCommand.AddUserToChannel, user.GetColorCharacter() + user.Username);

			SendMessage(44, user.Username); // %1 has been kicked out of the conference.
		}

		if (wasBanned && moderator != null)
			moderator.SendMessage(62, user.Username); // You are banning %1 from this conference.
	}

	public bool VoiceRestricted
	{
		get => _voiceRestricted;
		set
		{
			_voiceRestricted = value;
			// From now on, only moderators will have speaking privileges in this conference by default.// From now on, everyone in the conference will have speaking privileges by default.
			SendMessage(value ? 56 : 55);
		}
	}

	public bool AlwaysAvailable { get; set; }

	public void AddVoiced(ChatUser user)
	{
		AddVoiced(user, null);
	}

	public void AddVoiced(ChatUser user, ChatUser moderator)
	{
		if (!ValidateModerator(moderator))
			return;

		if (!IsBanned(user) && !IsModerator(user) && !IsVoiced(user))
		{
			_voices.Add(user);

			if (moderator != null)
				user.SendMessage(54, moderator.Username); // %1, a conference moderator, has granted you speaking priviledges in this conference.

			SendMessage(52, user, user.Username); // %1 now has speaking privileges in this conference.
			SendCommand(ChatCommand.AddUserToChannel, user, user.GetColorCharacter() + user.Username);
		}
	}

	public void RemoveVoiced(ChatUser user, ChatUser moderator)
	{
		if (!ValidateModerator(moderator) || !ValidateAccess(moderator, user))
			return;

		if (!IsModerator(user) && IsVoiced(user))
		{
			_voices.Remove(user);

			if (moderator != null)
				user.SendMessage(53, moderator.Username); // %1, a conference moderator, has removed your speaking priviledges for this conference.

			SendMessage(51, user, user.Username); // %1 no longer has speaking privileges in this conference.
			SendCommand(ChatCommand.AddUserToChannel, user, user.GetColorCharacter() + user.Username);
		}
	}

	public void AddModerator(ChatUser user)
	{
		AddModerator(user, null);
	}

	public void AddModerator(ChatUser user, ChatUser moderator)
	{
		if (!ValidateModerator(moderator))
			return;

		if (IsBanned(user) || IsModerator(user))
			return;

		if (IsVoiced(user))
			_voices.Remove(user);

		_moderators.Add(user);

		if (moderator != null)
			user.SendMessage(50, moderator.Username); // %1 has made you a conference moderator.

		SendMessage(48, user, user.Username); // %1 is now a conference moderator.
		SendCommand(ChatCommand.AddUserToChannel, user.GetColorCharacter() + user.Username);
	}

	public void RemoveModerator(ChatUser user)
	{
		RemoveModerator(user, null);
	}

	public void RemoveModerator(ChatUser user, ChatUser moderator)
	{
		if (!ValidateModerator(moderator) || !ValidateAccess(moderator, user))
			return;

		if (IsModerator(user))
		{
			_moderators.Remove(user);

			if (moderator != null)
				user.SendMessage(49, moderator.Username); // %1 has removed you from the list of conference moderators.

			SendMessage(47, user, user.Username); // %1 is no longer a conference moderator.
			SendCommand(ChatCommand.AddUserToChannel, user.GetColorCharacter() + user.Username);
		}
	}

	public void SendMessage(int number)
	{
		SendMessage(number, null, null, null);
	}

	public void SendMessage(int number, string param1)
	{
		SendMessage(number, null, param1, null);
	}

	public void SendMessage(int number, string param1, string param2)
	{
		SendMessage(number, null, param1, param2);
	}

	public void SendMessage(int number, ChatUser initiator)
	{
		SendMessage(number, initiator, null, null);
	}

	public void SendMessage(int number, ChatUser initiator, string param1)
	{
		SendMessage(number, initiator, param1, null);
	}

	public void SendMessage(int number, ChatUser initiator, string param1, string param2)
	{
		for (int i = 0; i < _users.Count; ++i)
		{
			ChatUser user = _users[i];

			if (user == initiator)
				continue;

			if (user.CheckOnline())
				user.SendMessage(number, param1, param2);
			else if (!Contains(user))
				--i;
		}
	}

	public void SendIgnorableMessage(int number, ChatUser from, string param1, string param2)
	{
		for (int i = 0; i < _users.Count; ++i)
		{
			ChatUser user = _users[i];

			if (user.IsIgnored(from))
				continue;

			if (user.CheckOnline())
				user.SendMessage(number, from.Mobile, param1, param2);
			else if (!Contains(user))
				--i;
		}
	}

	public void SendCommand(ChatCommand command)
	{
		SendCommand(command, null, null, null);
	}

	public void SendCommand(ChatCommand command, string param1)
	{
		SendCommand(command, null, param1, null);
	}

	public void SendCommand(ChatCommand command, string param1, string param2)
	{
		SendCommand(command, null, param1, param2);
	}

	public void SendCommand(ChatCommand command, ChatUser initiator)
	{
		SendCommand(command, initiator, null, null);
	}

	public void SendCommand(ChatCommand command, ChatUser initiator, string param1)
	{
		SendCommand(command, initiator, param1, null);
	}

	public void SendCommand(ChatCommand command, ChatUser initiator, string param1, string param2)
	{
		for (int i = 0; i < _users.Count; ++i)
		{
			ChatUser user = _users[i];

			if (user == initiator)
				continue;

			if (user.CheckOnline())
				ChatSystem.SendCommandTo(user.Mobile, command, param1, param2);
			else if (!Contains(user))
				--i;
		}
	}

	public void SendUsersTo(ChatUser to)
	{
		for (int i = 0; i < _users.Count; ++i)
		{
			ChatUser user = _users[i];

			ChatSystem.SendCommandTo(to.Mobile, ChatCommand.AddUserToChannel, user.GetColorCharacter() + user.Username);
		}
	}

	public static List<Channel> Channels { get; } = new();

	public static void SendChannelsTo(ChatUser user)
	{
		for (int i = 0; i < Channels.Count; ++i)
		{
			Channel channel = Channels[i];

			if (!channel.IsBanned(user))
				ChatSystem.SendCommandTo(user.Mobile, ChatCommand.AddChannel, channel.Name, "0");
		}
	}

	public static Channel AddChannel(string name)
	{
		return AddChannel(name, null);
	}

	public static Channel AddChannel(string name, string password)
	{
		Channel channel = FindChannelByName(name);

		if (channel == null)
		{
			channel = new Channel(name, password);
			Channels.Add(channel);
		}

		ChatUser.GlobalSendCommand(ChatCommand.AddChannel, name, "0");

		return channel;
	}

	public static void RemoveChannel(string name)
	{
		RemoveChannel(FindChannelByName(name));
	}

	public static void RemoveChannel(Channel channel)
	{
		if (channel == null)
			return;

		if (Channels.Contains(channel) && channel._users.Count == 0)
		{
			ChatUser.GlobalSendCommand(ChatCommand.RemoveChannel, channel.Name);

			channel._moderators.Clear();
			channel._voices.Clear();

			Channels.Remove(channel);
		}
	}

	public static Channel FindChannelByName(string name)
	{
		return Channels.FirstOrDefault(channel => channel._name == name);
	}

	public static void Initialize()
	{
		AddStaticChannel("Newbie Help");
	}

	public static void AddStaticChannel(string name)
	{
		AddChannel(name).AlwaysAvailable = true;
	}
}
