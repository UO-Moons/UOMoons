using MySql.Data.MySqlClient;
using Server.Commands;
using Server.Misc;
using System;
using System.Collections;

namespace Server.Accounting
{
	public class WebAccounting
	{

		public enum Status
		{
			Void = 0,
			Pending = 1,
			Active = 2,
			PWChanged = 3,
			EmailChanged = 4,
			Delete = 5
		}

		private static int QueryCount = 0;
		public static readonly bool Enabled = Settings.Configuration.Get("WebAccount", "Enabled", false);
		public static readonly bool UpdateOnWorldSave = true;
		public static readonly bool UpdateOnWorldLoad = true;

		private static readonly string DatabaseDriver = "{MySQL ODBC 3.51 Driver}";
		private static readonly string DatabaseServer = Settings.Configuration.Get<string>("WebAccount", "DatabaseServer", null);//Server IP of the database
		private static readonly string DatabaseName = Settings.Configuration.Get<string>("WebAccount", "DatabaseName", null);//Name of the database
		private static readonly string DatabaseTable = Settings.Configuration.Get<string>("WebAccount", "DatabaseTable", null);//Name of the table storing accounts
		private static readonly string DatabaseUserID = Settings.Configuration.Get<string>("WebAccount", "DatabaseUserID", null);//Username for the database
		private static readonly string DatabasePassword = Settings.Configuration.Get<string>("WebAccount", "DatabasePassword", null);//Username password
		private static readonly string ConnectionString = $"DRIVER={DatabaseDriver};SERVER={DatabaseServer};DATABASE={DatabaseName};UID={DatabaseUserID};PASSWORD={DatabasePassword};";

		static bool Synchronizing = false;

		public static void Initialize()
		{
			SynchronizeDatabase();
			CommandSystem.Register("AccSync", AccessLevel.Administrator, new CommandEventHandler(Sync_OnCommand));

			if (UpdateOnWorldLoad)
			{
				EventSink.OnWorldLoad += OnLoaded;
			}

			if (UpdateOnWorldSave)
			{
				EventSink.OnWorldSave += OnSaved;
			}
			else
			{
				Timer.DelayCall(TimeSpan.FromMinutes(10.0), TimeSpan.FromMinutes(10.0), new TimerCallback(SynchronizeDatabase));
			}
		}

		public static void OnSaved()
		{
			if (Synchronizing)
				return;

			SynchronizeDatabase();
		}

		public static void OnLoaded()
		{
			if (Synchronizing)
				return;

			SynchronizeDatabase();
		}

		[Usage("AccSync")]
		[Description("Synchronizes the Accounts Database")]
		public static void Sync_OnCommand(CommandEventArgs e)
		{
			if (Synchronizing)
				return;

			Mobile from = e.Mobile;

			SynchronizeDatabase();
			from.SendMessage("Done Synchronizing Database!");
		}

		public static void CreateAccountsFromDB()
		{
			//Console.WriteLine( "Getting New Accounts..." );
			try
			{
				ArrayList ToCreateFromDB = new();
				MySqlConnection Connection = new(ConnectionString);

				Connection.Open();
				MySqlCommand Command = Connection.CreateCommand();

				Command.CommandText = $"SELECT name,password,email FROM {DatabaseTable} WHERE state='{(int)Status.Pending}'";
				MySqlDataReader reader = Command.ExecuteReader();

				QueryCount += 1;

				while (reader.Read())
				{
					string username = reader.GetString(0);
					string password = reader.GetString(1);
					string email = reader.GetString(2);

					if (Accounts.GetAccount(username) == null)
						ToCreateFromDB.Add(Accounts.AddAccount(username, password, email));
				}
				reader.Close();

				//Console.WriteLine( "Updating Database..." );
				foreach (Account a in ToCreateFromDB)
				{
					int ALevel = 0;

					if (a.AccessLevel == AccessLevel.Player)
					{
						ALevel = 1;
					}
					else if (a.AccessLevel == AccessLevel.Counselor)
					{
						ALevel = 2;
					}
					else if (a.AccessLevel == AccessLevel.GameMaster)
					{
						ALevel = 3;
					}
					else if (a.AccessLevel == AccessLevel.Seer)
					{
						ALevel = 4;
					}
					else if (a.AccessLevel == AccessLevel.Administrator)
					{
						ALevel = 6;
					}

					QueryCount += 1;

					Command.CommandText = $"UPDATE {DatabaseTable} SET email='{a.Email}',password='{a.CryptPassword}',state='{(int)Status.Active}',access='{ALevel}' WHERE name='{a.Username}'";
					Command.ExecuteNonQuery();
				}

				Connection.Close();

				Console.WriteLine("[{0} In-Game Accounts Created] ", ToCreateFromDB.Count);
			}
			catch (Exception e)
			{
				Console.WriteLine("[In-Game Account Create] Error...");
				Console.WriteLine(e);
			}
		}


		public static void CreateAccountsFromUO()
		{
			//Console.WriteLine( "Exporting New Accounts..." );
			try
			{
				ArrayList ToCreateFromUO = new();
				MySqlConnection Connection = new(ConnectionString);

				Connection.Open();
				MySqlCommand Command = Connection.CreateCommand();

				Command.CommandText = $"SELECT name FROM {DatabaseTable}";
				MySqlDataReader reader = Command.ExecuteReader();

				QueryCount += 1;

				while (reader.Read())
				{
					string username = reader.GetString(0);

					Account toCheck = Accounts.GetAccount(username) as Account;

					if (toCheck == null)
						ToCreateFromUO.Add(toCheck);
				}
				reader.Close();

				//Console.WriteLine( "Updating Database..." );
				foreach (Account a in ToCreateFromUO)
				{
					int ALevel = 0;

					if (a.AccessLevel == AccessLevel.Player)
					{
						ALevel = 1;
					}
					else if (a.AccessLevel == AccessLevel.Counselor)
					{
						ALevel = 2;
					}
					else if (a.AccessLevel == AccessLevel.GameMaster)
					{
						ALevel = 3;
					}
					else if (a.AccessLevel == AccessLevel.Seer)
					{
						ALevel = 4;
					}
					else if (a.AccessLevel == AccessLevel.Administrator)
					{
						ALevel = 6;
					}

					PasswordProtection PWMode = AccountHandler.ProtectPasswords;
					string Password = "";

					switch (PWMode)
					{
						case PasswordProtection.None: { Password = a.PlainPassword; } break;
						case PasswordProtection.Crypt: { Password = a.CryptPassword; } break;
						default: { Password = a.NewCryptPassword; } break;
					}

					QueryCount += 1;

					MySqlCommand InsertCommand = Connection.CreateCommand();

					InsertCommand.CommandText = $"INSERT INTO {DatabaseTable} (name,password,email,access,timestamp,state) VALUES( '{a.Username}', '{Password}', '{a.Email}', '{ALevel}', '{ToUnixTimestamp(a.Created)}', '{(int)Status.Active}')";
					InsertCommand.ExecuteNonQuery();
				}

				Connection.Close();

				Console.WriteLine("[{0} Database Accounts Added] ", ToCreateFromUO.Count);
			}
			catch (Exception e)
			{
				Console.WriteLine("[Database Account Create] Error...");
				Console.WriteLine(e);
			}
		}

		public static void UpdateUOPasswords()
		{
			//Console.WriteLine( "Getting New Passwords..." );
			try
			{
				ArrayList ToUpdatePWFromDB = new();
				MySqlConnection Connection = new(ConnectionString);

				Connection.Open();
				MySqlCommand Command = Connection.CreateCommand();

				Command.CommandText = $"SELECT name,password FROM {DatabaseTable} WHERE state='{(int)Status.PWChanged}'";
				MySqlDataReader reader = Command.ExecuteReader();

				QueryCount += 1;

				while (reader.Read())
				{
					string username = reader.GetString(0);
					string password = reader.GetString(1);

					if (Accounts.GetAccount(username) is Account AtoUpdate)
					{
						PasswordProtection PWMode = AccountHandler.ProtectPasswords;
						string Password = "";

						switch (PWMode)
						{
							case PasswordProtection.None: { Password = AtoUpdate.PlainPassword; } break;
							case PasswordProtection.Crypt: { Password = AtoUpdate.CryptPassword; } break;
							default: { Password = AtoUpdate.NewCryptPassword; } break;
						}

						if (Password == null || Password == "" || Password != password)
						{
							AtoUpdate.SetPassword(password);
							ToUpdatePWFromDB.Add(AtoUpdate);
						}
					}
				}
				reader.Close();

				//Console.WriteLine( "Updating Database..." );
				foreach (Account a in ToUpdatePWFromDB)
				{
					PasswordProtection PWModeU = AccountHandler.ProtectPasswords;
					string PasswordU = "";

					switch (PWModeU)
					{
						case PasswordProtection.None: { PasswordU = a.PlainPassword; } break;
						case PasswordProtection.Crypt: { PasswordU = a.CryptPassword; } break;
						default: { PasswordU = a.NewCryptPassword; } break;
					}

					QueryCount += 1;

					Command.CommandText = $"UPDATE {DatabaseTable} SET state='{(int)Status.Active}',password='{PasswordU}' WHERE name='{a.Username}'";
					Command.ExecuteNonQuery();
				}

				Connection.Close();

				Console.WriteLine("[{0} In-game Passwords Changed] ", ToUpdatePWFromDB.Count);
			}
			catch (System.Exception e)
			{
				Console.WriteLine("[In-Game Password Change] Error...");
				Console.WriteLine(e);
			}
		}

		public static void UpdateDBPasswords()
		{
			//Console.WriteLine( "Exporting New Passwords..." );
			try
			{
				ArrayList ToUpdatePWFromUO = new();
				MySqlConnection Connection = new(ConnectionString);

				Connection.Open();
				MySqlCommand Command = Connection.CreateCommand();

				Command.CommandText = $"SELECT name,password FROM {DatabaseTable} WHERE state='{(int)Status.Active}'";
				MySqlDataReader reader = Command.ExecuteReader();

				QueryCount += 1;

				while (reader.Read())
				{
					string username = reader.GetString(0);
					string password = reader.GetString(1);

					if (Accounts.GetAccount(username) is Account AtoUpdate)
					{
						PasswordProtection PWMode = AccountHandler.ProtectPasswords;
						string Password = "";

						switch (PWMode)
						{
							case PasswordProtection.None: { Password = AtoUpdate.PlainPassword; } break;
							case PasswordProtection.Crypt: { Password = AtoUpdate.CryptPassword; } break;
							default: { Password = AtoUpdate.NewCryptPassword; } break;
						}

						if (Password == null || Password == "" || Password != password)
						{
							ToUpdatePWFromUO.Add(AtoUpdate);
						}
					}
				}
				reader.Close();

				//Console.WriteLine( "Updating Database..." );
				foreach (Account a in ToUpdatePWFromUO)
				{
					PasswordProtection PWModeU = AccountHandler.ProtectPasswords;
					string PasswordU = "";

					switch (PWModeU)
					{
						case PasswordProtection.None: { PasswordU = a.PlainPassword; } break;
						case PasswordProtection.Crypt: { PasswordU = a.CryptPassword; } break;
						default: { PasswordU = a.NewCryptPassword; } break;
					}

					QueryCount += 1;

					Command.CommandText = $"UPDATE {DatabaseTable} SET state='{(int)Status.Active}',password='{PasswordU}' WHERE name='{a.Username}'";
					Command.ExecuteNonQuery();
				}

				Connection.Close();

				Console.WriteLine("[{0} Database Passwords Changed] ", ToUpdatePWFromUO.Count);
			}
			catch (Exception e)
			{
				Console.WriteLine("[Database Password Change] Error...");
				Console.WriteLine(e);
			}
		}



		public static void UpdateUOEmails()
		{
			//Console.WriteLine( "Getting New Emails..." );
			try
			{
				ArrayList ToUpdateEmailFromDB = new();
				MySqlConnection Connection = new(ConnectionString);

				Connection.Open();
				MySqlCommand Command = Connection.CreateCommand();

				Command.CommandText = $"SELECT name,email FROM {DatabaseTable} WHERE state='{(int)Status.EmailChanged}'";
				MySqlDataReader reader = Command.ExecuteReader();

				QueryCount += 1;

				while (reader.Read())
				{
					string username = reader.GetString(0);
					string email = reader.GetString(1);


					if (Accounts.GetAccount(username) is Account AtoUpdate && (AtoUpdate.Email == null || AtoUpdate.Email == "" || AtoUpdate.Email != email))
					{
						AtoUpdate.Email = email;
						ToUpdateEmailFromDB.Add(AtoUpdate);
					}
				}
				reader.Close();

				//Console.WriteLine( "Updating Database..." );
				foreach (Account a in ToUpdateEmailFromDB)
				{
					QueryCount += 1;

					Command.CommandText = $"UPDATE {DatabaseTable} SET state='{(int)Status.Active}',email='{a.Email}' WHERE name='{a.Username}'";
					Command.ExecuteNonQuery();
				}

				Connection.Close();

				Console.WriteLine("[{0} In-Game Emails Changed] ", ToUpdateEmailFromDB.Count);
			}
			catch (System.Exception e)
			{
				Console.WriteLine("[In-Game Email Change] Error...");
				Console.WriteLine(e);
			}
		}

		public static void UpdateDBEmails()
		{
			//Console.WriteLine( "Exporting New Emails..." );
			try
			{
				ArrayList ToUpdateEmailFromUO = new();
				MySqlConnection Connection = new(ConnectionString);

				Connection.Open();
				MySqlCommand Command = Connection.CreateCommand();

				Command.CommandText = $"SELECT name,email FROM {DatabaseTable} WHERE state='{(int)Status.Active}'";
				MySqlDataReader reader = Command.ExecuteReader();

				QueryCount += 1;

				while (reader.Read())
				{
					string username = reader.GetString(0);
					string email = reader.GetString(1);

					if (Accounts.GetAccount(username) is Account AtoUpdate && (AtoUpdate.Email == null || AtoUpdate.Email == "" || AtoUpdate.Email != email))
						ToUpdateEmailFromUO.Add(AtoUpdate);
				}
				reader.Close();

				//Console.WriteLine( "Updating Database..." );
				foreach (Account a in ToUpdateEmailFromUO)
				{
					QueryCount += 1;

					Command.CommandText = $"UPDATE {DatabaseTable} SET state='{(int)Status.Active}',email='{a.Email}' WHERE name='{a.Username}'";
					Command.ExecuteNonQuery();
				}

				Connection.Close();

				Console.WriteLine("[{0} Database Emails Changed] ", ToUpdateEmailFromUO.Count);
			}
			catch (Exception e)
			{
				Console.WriteLine("[Database Email Change] Error...");
				Console.WriteLine(e);
			}
		}

		public static void SynchronizeDatabase()
		{
			if (Synchronizing || !Enabled)
				return;

			Synchronizing = true;

			Console.WriteLine("Accounting System...");

			CreateAccountsFromDB();
			CreateAccountsFromUO();

			UpdateUOEmails();
			UpdateDBEmails();

			UpdateUOPasswords();
			UpdateDBPasswords();

			Console.WriteLine($"[Executed {QueryCount} Database Queries]");

			QueryCount = 0;

			World.Save();

			Synchronizing = false;
		}

		static double ToUnixTimestamp(DateTime date)
		{
			DateTime origin = new(1970, 1, 1, 0, 0, 0, 0);
			TimeSpan diff = date - origin;
			return Math.Floor(diff.TotalSeconds);
		}


	}
}
