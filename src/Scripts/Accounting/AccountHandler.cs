using Server.Accounting;
using Server.Commands;
using Server.Engines.Help;
using Server.Network;
using Server.Regions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Server.Misc;

public enum PasswordProtection
{
	None,
	Crypt,
	NewCrypt
}

public class AccountHandler
{
	private static readonly int MaxAccountsPerIp = Settings.Configuration.Get<int>("Accounts", "AccountsPerIp");
	private static readonly bool AutoAccountCreation = Settings.Configuration.Get<bool>("Accounts", "AutoCreateAccounts");
	//private static readonly bool RestrictDeletion = Settings.Configuration.Get<bool>("Accounts", "RestrictDeletion");
	private static readonly TimeSpan DeleteDelay = TimeSpan.FromDays(Settings.Configuration.Get<int>("Accounts", "DeleteDaysDelay"));
	private static readonly bool PasswordCommandEnabled = Settings.Configuration.Get<bool>("Accounts", "PasswordCommandEnabled");

	public static readonly PasswordProtection ProtectPasswords = (PasswordProtection)Settings.Configuration.Get<int>("Accounts", "ProtectPasswords");

	public static bool RestrictCharacterDeletion { get; set; } = Settings.Configuration.Get<bool>("Accounts", "RestrictDeletion");

	public static AccessLevel LockdownLevel { get; set; }

	private static readonly CityInfo[] StartingCities = {
		new( "New Haven",  "New Haven Bank",   1150168, 3667,  2625,   0  ),
		new( "Yew",        "The Empath Abbey", 1075072, 633,   858,    0  ),
		new( "Minoc",      "The Barnacle",     1075073, 2476,  413,    15 ),
		new( "Britain",    "The Wayfarer's Inn",   1075074, 1602,  1591,   20 ),
		new( "Moonglow",   "The Scholars Inn", 1075075, 4408,  1168,   0  ),
		new( "Trinsic",    "The Traveler's Inn",   1075076, 1845,  2745,   0  ),
		new( "Jhelom",     "The Mercenary Inn",    1075078, 1374,  3826,   0  ),
		new( "Skara Brae", "The Falconer's Inn",   1075079, 618,   2234,   0  ),
		new( "Vesper",     "The Ironwood Inn", 1075080, 2771,  976,    0  )
	};

	/* Old Haven/Magincia Locations
		new CityInfo( "Britain", "Sweet Dreams Inn", 1496, 1628, 10 );
		// ..
		// Trinsic
		new CityInfo( "Magincia", "The Great Horns Tavern", 3734, 2222, 20 ),
		// Jhelom
		// ..
		new CityInfo( "Haven", "Buckler's Hideaway", 3667, 2625, 0 )

		if ( Core.AOS )
		{
			//CityInfo haven = new CityInfo( "Haven", "Uzeraan's Mansion", 3618, 2591, 0 );
			CityInfo haven = new CityInfo( "Haven", "Uzeraan's Mansion", 3503, 2574, 14 );
			StartingCities[StartingCities.Length - 1] = haven;
		}
	*/

	public static void Initialize()
	{
		EventSink.OnDeleteRequest += EventSink_DeleteRequest;
		EventSink.AccountLogin += EventSink_AccountLogin;
		EventSink.GameLogin += EventSink_GameLogin;

		if (PasswordCommandEnabled)
			CommandSystem.Register("Password", AccessLevel.Player, new CommandEventHandler(Password_OnCommand));
	}

	[Usage("Password <newPassword> <repeatPassword>")]
	[Description("Changes the password of the commanding players account. Requires the same C-class IP address as the account's creator.")]
	public static void Password_OnCommand(CommandEventArgs e)
	{
		Mobile from = e.Mobile;

		if (from.Account is not Account acct)
			return;

		IPAddress[] accessList = acct.LoginIPs;

		if (accessList.Length == 0)
			return;

		NetState ns = from.NetState;

		if (ns == null)
			return;

		switch (e.Length)
		{
			case 0:
				from.SendMessage("You must specify the new password.");
				return;
			case 1:
				from.SendMessage("To prevent potential typing mistakes, you must type the password twice. Use the format:");
				from.SendMessage("Password \"(newPassword)\" \"(repeated)\"");
				return;
		}

		string pass = e.GetString(0);
		string pass2 = e.GetString(1);

		if (pass != pass2)
		{
			from.SendMessage("The passwords do not match.");
			return;
		}

		bool isSafe = true;

		for (var i = 0; isSafe && i < pass.Length; ++i)
			isSafe = (pass[i] >= 0x20 && pass[i] < 0x7F);

		if (!isSafe)
		{
			from.SendMessage("That is not a valid password.");
			return;
		}

		try
		{
			IPAddress ipAddress = ns.Address;

			if (Utility.IPMatchClassC(accessList[0], ipAddress))
			{
				acct.SetPassword(pass);
				from.SendMessage("The password to your account has changed.");
			}
			else
			{
				PageEntry entry = PageQueue.GetEntry(from);

				if (entry != null)
				{
					from.SendMessage(entry.Message.StartsWith("[Automated: Change Password]")
						? "You already have a password change request in the help system queue."
						: "Your IP address does not match that which created this account.");
				}
				else if (PageQueue.CheckAllowedToPage(from))
				{
					from.SendMessage("Your IP address does not match that which created this account.  A page has been entered into the help system on your behalf.");

					from.SendLocalizedMessage(501234, 0x35); /* The next available Counselor/Game Master will respond as soon as possible.
																		* Please check your Journal for messages every few minutes.
																		*/

					PageQueue.Enqueue(new PageEntry(from, string.Format("[Automated: Change Password]<br>Desired password: {0}<br>Current IP address: {1}<br>Account IP address: {2}", pass, ipAddress, accessList[0]), PageType.Account));
				}

			}
		}
		catch
		{
			// ignored
		}
	}

	private static void EventSink_DeleteRequest(NetState state, int index)
	{
		if (state.Account is not Account acct)
		{
			state.Dispose();
		}
		else if (index < 0 || index >= acct.Length)
		{
			state.Send(new DeleteResult(DeleteResultType.BadRequest));
			state.Send(new CharacterListUpdate(acct));
		}
		else
		{
			Mobile m = acct[index];

			if (m == null)
			{
				state.Send(new DeleteResult(DeleteResultType.CharNotExist));
				state.Send(new CharacterListUpdate(acct));
			}
			else if (m.NetState != null)
			{
				state.Send(new DeleteResult(DeleteResultType.CharBeingPlayed));
				state.Send(new CharacterListUpdate(acct));
			}
			else if (RestrictCharacterDeletion && DateTime.UtcNow < (m.CreationTime + DeleteDelay))
			{
				state.Send(new DeleteResult(DeleteResultType.CharTooYoung));
				state.Send(new CharacterListUpdate(acct));
			}
			else if (m.IsPlayer() && Region.Find(m.LogoutLocation, m.LogoutMap).BlockCharacterDeletion) //Don't need to check current location, if netstate is null, they're logged out
			{
				state.Send(new DeleteResult(DeleteResultType.BadRequest));
				state.Send(new CharacterListUpdate(acct));
			}
			else
			{
				Console.WriteLine("Client: {0}: Deleting character {1} (0x{2:X})", state, index, m.Serial.Value);

				acct.Comments.Add(new AccountComment("System", $"Character #{index + 1} {m} deleted by {state}"));

				m.Delete();
				state.Send(new CharacterListUpdate(acct));
			}
		}
	}

	public static bool CanCreate(IPAddress ip)
	{
		if (!IpTable.ContainsKey(ip))
			return true;

		return (IpTable[ip] < MaxAccountsPerIp);
	}

	private static Dictionary<IPAddress, int> _mIpTable;

	public static Dictionary<IPAddress, int> IpTable
	{
		get
		{
			if (_mIpTable == null)
			{
				_mIpTable = new Dictionary<IPAddress, int>();

				foreach (var account in Accounts.GetAccounts())
				{
					var a = (Account) account;
					if (a.LoginIPs.Length <= 0) continue;
					IPAddress ip = a.LoginIPs[0];

					if (_mIpTable.ContainsKey(ip))
						_mIpTable[ip]++;
					else
						_mIpTable[ip] = 1;
				}
			}

			return _mIpTable;
		}
	}

	private static readonly char[] MForbiddenChars = {
		'<', '>', ':', '"', '/', '\\', '|', '?', '*'
	};

	private static bool IsForbiddenChar(char c)
	{
		for (var i = 0; i < MForbiddenChars.Length; ++i)
			if (c == MForbiddenChars[i])
				return true;

		return false;
	}

	private static Account CreateAccount(NetState state, string un, string pw)
	{
		if (un.Length == 0 || pw.Length == 0)
			return null;

		bool isSafe = !(un.StartsWith(" ") || un.EndsWith(" ") || un.EndsWith("."));

		for (var i = 0; isSafe && i < un.Length; ++i)
			isSafe = (un[i] >= 0x20 && un[i] < 0x7F && !IsForbiddenChar(un[i]));

		for (var i = 0; isSafe && i < pw.Length; ++i)
			isSafe = (pw[i] >= 0x20 && pw[i] < 0x7F);

		if (!isSafe)
			return null;

		if (!CanCreate(state.Address))
		{
			Console.WriteLine("Login: {0}: Account '{1}' not created, ip already has {2} account{3}.", state, un, MaxAccountsPerIp, MaxAccountsPerIp == 1 ? "" : "s");
			return null;
		}

		Console.WriteLine("Login: {0}: Creating new account '{1}'", state, un);

		Account a = new(un, pw);

		return a;
	}

	public static void EventSink_AccountLogin(AccountLoginEventArgs e)
	{
		if (!IpLimiter.SocketBlock && !IpLimiter.Verify(e.State.Address))
		{
			e.Accepted = false;
			e.RejectReason = ALRReason.InUse;

			Console.WriteLine("Login: {0}: Past IP limit threshold", e.State);

			using StreamWriter op = new("ipLimits.log", true);
			op.WriteLine("{0}\tPast IP limit threshold\t{1}", e.State, DateTime.UtcNow);

			return;
		}

		string un = e.Username;
		string pw = e.Password;

		e.Accepted = false;

		if (Accounts.GetAccount(un) is not Account acct)
		{
			if (AutoAccountCreation && un.Trim().Length > 0) // To prevent someone from making an account of just '' or a bunch of meaningless spaces
			{
				e.State.Account = acct = CreateAccount(e.State, un, pw);
				e.Accepted = acct != null && acct.CheckAccess(e.State);

				if (!e.Accepted)
					e.RejectReason = ALRReason.BadComm;
			}
			else
			{
				Console.WriteLine("Login: {0}: Invalid username '{1}'", e.State, un);
				e.RejectReason = ALRReason.Invalid;
			}
		}
		else if (!acct.HasAccess(e.State))
		{
			Console.WriteLine("Login: {0}: Access denied for '{1}'", e.State, un);
			e.RejectReason = (LockdownLevel > AccessLevel.Player ? ALRReason.BadComm : ALRReason.BadPass);
		}
		else if (!acct.CheckPassword(pw))
		{
			Console.WriteLine("Login: {0}: Invalid password for '{1}'", e.State, un);
			e.RejectReason = ALRReason.BadPass;
		}
		else if (acct.Banned)
		{
			Console.WriteLine("Login: {0}: Banned account '{1}'", e.State, un);
			e.RejectReason = ALRReason.Blocked;
		}
		else
		{
			Console.WriteLine("Login: {0}: Valid credentials for '{1}'", e.State, un);
			e.State.Account = acct;
			e.Accepted = true;

			acct.LogAccess(e.State);
		}

		if (!e.Accepted)
			AccountAttackLimiter.RegisterInvalidAccess(e.State);
	}

	public static void EventSink_GameLogin(GameLoginEventArgs e)
	{
		if (!IpLimiter.SocketBlock && !IpLimiter.Verify(e.State.Address))
		{
			e.Accepted = false;

			Console.WriteLine("Login: {0}: Past IP limit threshold", e.State);

			using StreamWriter op = new("ipLimits.log", true);
			op.WriteLine("{0}\tPast IP limit threshold\t{1}", e.State, DateTime.UtcNow);

			return;
		}

		string un = e.Username;
		string pw = e.Password;

		if (Accounts.GetAccount(un) is not Account acct)
		{
			e.Accepted = false;
		}
		else if (!acct.HasAccess(e.State))
		{
			Console.WriteLine("Login: {0}: Access denied for '{1}'", e.State, un);
			e.Accepted = false;
		}
		else if (!acct.CheckPassword(pw))
		{
			Console.WriteLine("Login: {0}: Invalid password for '{1}'", e.State, un);
			e.Accepted = false;
		}
		else if (acct.Banned)
		{
			Console.WriteLine("Login: {0}: Banned account '{1}'", e.State, un);
			e.Accepted = false;
		}
		else
		{
			acct.LogAccess(e.State);

			Console.WriteLine("Login: {0}: Account '{1}' at character list", e.State, un);
			e.State.Account = acct;
			e.Accepted = true;
			e.CityInfo = StartingCities;
		}

		if (!e.Accepted)
			AccountAttackLimiter.RegisterInvalidAccess(e.State);
	}

	public static bool CheckAccount(Mobile mobCheck, Mobile accCheck)
	{
		if (accCheck?.Account is not Account a) return false;
		for (var i = 0; i < a.Length; ++i)
		{
			if (a[i] == mobCheck)
				return true;
		}

		return false;
	}
}
