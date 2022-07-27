using MySql.Data.MySqlClient;
using Server.Commands;
using Server.Misc;
using System;
using System.Collections;

namespace Server.Accounting
{
	public class WebAccounting
	{
		private enum Status
		{
			Void = 0,
			Pending = 1,
			Active = 2,
			PwChanged = 3,
			EmailChanged = 4,
			Delete = 5
		}

		private static int _queryCount;
		private static readonly bool Enabled = Settings.Configuration.Get("WebAccount", "Enabled", false);
		private static readonly bool UpdateOnWorldSave = true;
		private static readonly bool UpdateOnWorldLoad = true;

		private static readonly string DatabaseDriver = "{MySQL ODBC 3.51 Driver}";
		private static readonly string DatabaseServer = Settings.Configuration.Get<string>("WebAccount", "DatabaseServer", null);//Server IP of the database
		private static readonly string DatabaseName = Settings.Configuration.Get<string>("WebAccount", "DatabaseName", null);//Name of the database
		private static readonly string DatabaseTable = Settings.Configuration.Get<string>("WebAccount", "DatabaseTable", null);//Name of the table storing accounts
		private static readonly string m_DatabaseUserId = Settings.Configuration.Get<string>("WebAccount", "DatabaseUserID", null);//Username for the database
		private static readonly string DatabasePassword = Settings.Configuration.Get<string>("WebAccount", "DatabasePassword", null);//Username password
		private static readonly string ConnectionString = $"DRIVER={DatabaseDriver};SERVER={DatabaseServer};DATABASE={DatabaseName};UID={m_DatabaseUserId};PASSWORD={DatabasePassword};";

		static bool _synchronizing;

		public static void Initialize()
		{
			SynchronizeDatabase();
			CommandSystem.Register("AccSync", AccessLevel.Administrator, Sync_OnCommand);

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
				Timer.DelayCall(TimeSpan.FromMinutes(10.0), TimeSpan.FromMinutes(10.0), SynchronizeDatabase);
			}
		}

		private static void OnSaved()
		{
			if (_synchronizing)
				return;

			SynchronizeDatabase();
		}

		private static void OnLoaded()
		{
			if (_synchronizing)
				return;

			SynchronizeDatabase();
		}

		[Usage("AccSync")]
		[Description("Synchronizes the Accounts Database")]
		private static void Sync_OnCommand(CommandEventArgs e)
		{
			if (_synchronizing)
				return;

			Mobile from = e.Mobile;

			SynchronizeDatabase();
			from.SendMessage("Done Synchronizing Database!");
		}

		private static void CreateAccountsFromDb()
		{
			//Console.WriteLine( "Getting New Accounts..." );
			try
			{
				ArrayList toCreateFromDb = new();
				MySqlConnection connection = new(ConnectionString);

				connection.Open();
				MySqlCommand command = connection.CreateCommand();

				command.CommandText = $"SELECT name,password,email FROM {DatabaseTable} WHERE state='{(int)Status.Pending}'";
				MySqlDataReader reader = command.ExecuteReader();

				_queryCount += 1;

				while (reader.Read())
				{
					string username = reader.GetString(0);
					string password = reader.GetString(1);
					string email = reader.GetString(2);

					if (Accounts.GetAccount(username) == null)
						toCreateFromDb.Add(Accounts.AddAccount(username, password, email));
				}
				reader.Close();

				//Console.WriteLine( "Updating Database..." );
				foreach (Account a in toCreateFromDb)
				{
					int aLevel = a.AccessLevel switch
					{
						AccessLevel.Player => 1,
						AccessLevel.Counselor => 2,
						AccessLevel.GameMaster => 3,
						AccessLevel.Seer => 4,
						AccessLevel.Administrator => 6,
						_ => 0
					};

					_queryCount += 1;

					command.CommandText = $"UPDATE {DatabaseTable} SET email='{a.Email}',password='{a.CryptPassword}',state='{(int)Status.Active}',access='{aLevel}' WHERE name='{a.Username}'";
					command.ExecuteNonQuery();
				}

				connection.Close();

				Console.WriteLine("[{0} In-Game Accounts Created] ", toCreateFromDb.Count);
			}
			catch (Exception e)
			{
				Console.WriteLine("[In-Game Account Create] Error...");
				Console.WriteLine(e);
			}
		}


		private static void CreateAccountsFromUo()
		{
			//Console.WriteLine( "Exporting New Accounts..." );
			try
			{
				ArrayList toCreateFromUo = new();
				MySqlConnection connection = new(ConnectionString);

				connection.Open();
				MySqlCommand command = connection.CreateCommand();

				command.CommandText = $"SELECT name FROM {DatabaseTable}";
				MySqlDataReader reader = command.ExecuteReader();

				_queryCount += 1;

				while (reader.Read())
				{
					string username = reader.GetString(0);

					if (Accounts.GetAccount(username) is not Account)
						toCreateFromUo.Add(null);
				}
				reader.Close();

				//Console.WriteLine( "Updating Database..." );
				foreach (Account a in toCreateFromUo)
				{
					int aLevel = a.AccessLevel switch
					{
						AccessLevel.Player => 1,
						AccessLevel.Counselor => 2,
						AccessLevel.GameMaster => 3,
						AccessLevel.Seer => 4,
						AccessLevel.Administrator => 6,
						_ => 0
					};

					PasswordProtection pwMode = AccountHandler.ProtectPasswords;

					string password = pwMode switch
					{
						PasswordProtection.None => a.PlainPassword,
						PasswordProtection.Crypt => a.CryptPassword,
						_ => a.NewCryptPassword
					};

					_queryCount += 1;

					MySqlCommand insertCommand = connection.CreateCommand();

					insertCommand.CommandText = $"INSERT INTO {DatabaseTable} (name,password,email,access,timestamp,state) VALUES( '{a.Username}', '{password}', '{a.Email}', '{aLevel}', '{ToUnixTimestamp(a.Created)}', '{(int)Status.Active}')";
					insertCommand.ExecuteNonQuery();
				}

				connection.Close();

				Console.WriteLine("[{0} Database Accounts Added] ", toCreateFromUo.Count);
			}
			catch (Exception e)
			{
				Console.WriteLine("[Database Account Create] Error...");
				Console.WriteLine(e);
			}
		}

		private static void UpdateUoPasswords()
		{
			//Console.WriteLine( "Getting New Passwords..." );
			try
			{
				ArrayList toUpdatePwFromDb = new();
				MySqlConnection connection = new(ConnectionString);

				connection.Open();
				MySqlCommand command = connection.CreateCommand();

				command.CommandText = $"SELECT name,password FROM {DatabaseTable} WHERE state='{(int)Status.PwChanged}'";
				MySqlDataReader reader = command.ExecuteReader();

				_queryCount += 1;

				while (reader.Read())
				{
					string username = reader.GetString(0);
					string password = reader.GetString(1);

					if (Accounts.GetAccount(username) is not Account atoUpdate) continue;
					PasswordProtection pwMode = AccountHandler.ProtectPasswords;
					string passwords;

					switch (pwMode)
					{
						case PasswordProtection.None: { passwords = atoUpdate.PlainPassword; } break;
						case PasswordProtection.Crypt: { passwords = atoUpdate.CryptPassword; } break;
						default: { passwords = atoUpdate.NewCryptPassword; } break;
					}

					if (!string.IsNullOrEmpty(passwords) && passwords == password) continue;
					atoUpdate.SetPassword(password);
					toUpdatePwFromDb.Add(atoUpdate);
				}
				reader.Close();

				//Console.WriteLine( "Updating Database..." );
				foreach (Account a in toUpdatePwFromDb)
				{
					PasswordProtection pwModeU = AccountHandler.ProtectPasswords;
					string passwordU;

					switch (pwModeU)
					{
						case PasswordProtection.None: { passwordU = a.PlainPassword; } break;
						case PasswordProtection.Crypt: { passwordU = a.CryptPassword; } break;
						default: { passwordU = a.NewCryptPassword; } break;
					}

					_queryCount += 1;

					command.CommandText = $"UPDATE {DatabaseTable} SET state='{(int)Status.Active}',password='{passwordU}' WHERE name='{a.Username}'";
					command.ExecuteNonQuery();
				}

				connection.Close();

				Console.WriteLine("[{0} In-game Passwords Changed] ", toUpdatePwFromDb.Count);
			}
			catch (Exception e)
			{
				Console.WriteLine("[In-Game Password Change] Error...");
				Console.WriteLine(e);
			}
		}

		private static void UpdateDbPasswords()
		{
			//Console.WriteLine( "Exporting New Passwords..." );
			try
			{
				ArrayList toUpdatePwFromUo = new();
				MySqlConnection connection = new(ConnectionString);

				connection.Open();
				MySqlCommand command = connection.CreateCommand();

				command.CommandText = $"SELECT name,password FROM {DatabaseTable} WHERE state='{(int)Status.Active}'";
				MySqlDataReader reader = command.ExecuteReader();

				_queryCount += 1;

				while (reader.Read())
				{
					string username = reader.GetString(0);
					string password = reader.GetString(1);

					if (Accounts.GetAccount(username) is Account atoUpdate)
					{
						PasswordProtection pwMode = AccountHandler.ProtectPasswords;
						string passwords;

						switch (pwMode)
						{
							case PasswordProtection.None: { passwords = atoUpdate.PlainPassword; } break;
							case PasswordProtection.Crypt: { passwords = atoUpdate.CryptPassword; } break;
							default: { passwords = atoUpdate.NewCryptPassword; } break;
						}

						if (string.IsNullOrEmpty(passwords) || passwords != password)
						{
							toUpdatePwFromUo.Add(atoUpdate);
						}
					}
				}
				reader.Close();

				//Console.WriteLine( "Updating Database..." );
				foreach (Account a in toUpdatePwFromUo)
				{
					PasswordProtection pwModeU = AccountHandler.ProtectPasswords;
					string passwordU;

					switch (pwModeU)
					{
						case PasswordProtection.None: { passwordU = a.PlainPassword; } break;
						case PasswordProtection.Crypt: { passwordU = a.CryptPassword; } break;
						default: { passwordU = a.NewCryptPassword; } break;
					}

					_queryCount += 1;

					command.CommandText = $"UPDATE {DatabaseTable} SET state='{(int)Status.Active}',password='{passwordU}' WHERE name='{a.Username}'";
					command.ExecuteNonQuery();
				}

				connection.Close();

				Console.WriteLine("[{0} Database Passwords Changed] ", toUpdatePwFromUo.Count);
			}
			catch (Exception e)
			{
				Console.WriteLine("[Database Password Change] Error...");
				Console.WriteLine(e);
			}
		}


		private static void UpdateUoEmails()
		{
			//Console.WriteLine( "Getting New Emails..." );
			try
			{
				ArrayList toUpdateEmailFromDb = new();
				MySqlConnection connection = new(ConnectionString);

				connection.Open();
				MySqlCommand command = connection.CreateCommand();

				command.CommandText = $"SELECT name,email FROM {DatabaseTable} WHERE state='{(int)Status.EmailChanged}'";
				MySqlDataReader reader = command.ExecuteReader();

				_queryCount += 1;

				while (reader.Read())
				{
					string username = reader.GetString(0);
					string email = reader.GetString(1);


					if (Accounts.GetAccount(username) is Account atoUpdate && (string.IsNullOrEmpty(atoUpdate.Email) || atoUpdate.Email != email))
					{
						atoUpdate.Email = email;
						toUpdateEmailFromDb.Add(atoUpdate);
					}
				}
				reader.Close();

				//Console.WriteLine( "Updating Database..." );
				foreach (Account a in toUpdateEmailFromDb)
				{
					_queryCount += 1;

					command.CommandText = $"UPDATE {DatabaseTable} SET state='{(int)Status.Active}',email='{a.Email}' WHERE name='{a.Username}'";
					command.ExecuteNonQuery();
				}

				connection.Close();

				Console.WriteLine("[{0} In-Game Emails Changed] ", toUpdateEmailFromDb.Count);
			}
			catch (Exception e)
			{
				Console.WriteLine("[In-Game Email Change] Error...");
				Console.WriteLine(e);
			}
		}

		private static void UpdateDbEmails()
		{
			//Console.WriteLine( "Exporting New Emails..." );
			try
			{
				ArrayList toUpdateEmailFromUo = new();
				MySqlConnection connection = new(ConnectionString);

				connection.Open();
				MySqlCommand command = connection.CreateCommand();

				command.CommandText = $"SELECT name,email FROM {DatabaseTable} WHERE state='{(int)Status.Active}'";
				MySqlDataReader reader = command.ExecuteReader();

				_queryCount += 1;

				while (reader.Read())
				{
					string username = reader.GetString(0);
					string email = reader.GetString(1);

					if (Accounts.GetAccount(username) is Account atoUpdate && (string.IsNullOrEmpty(atoUpdate.Email) || atoUpdate.Email != email))
						toUpdateEmailFromUo.Add(atoUpdate);
				}
				reader.Close();

				//Console.WriteLine( "Updating Database..." );
				foreach (Account a in toUpdateEmailFromUo)
				{
					_queryCount += 1;

					command.CommandText = $"UPDATE {DatabaseTable} SET state='{(int)Status.Active}',email='{a.Email}' WHERE name='{a.Username}'";
					command.ExecuteNonQuery();
				}

				connection.Close();

				Console.WriteLine("[{0} Database Emails Changed] ", toUpdateEmailFromUo.Count);
			}
			catch (Exception e)
			{
				Console.WriteLine("[Database Email Change] Error...");
				Console.WriteLine(e);
			}
		}

		private static void SynchronizeDatabase()
		{
			if (_synchronizing || !Enabled)
				return;

			_synchronizing = true;

			Console.WriteLine("Accounting System...");

			CreateAccountsFromDb();
			CreateAccountsFromUo();

			UpdateUoEmails();
			UpdateDbEmails();

			UpdateUoPasswords();
			UpdateDbPasswords();

			Console.WriteLine($"[Executed {_queryCount} Database Queries]");

			_queryCount = 0;

			World.Save();

			_synchronizing = false;
		}

		private static double ToUnixTimestamp(DateTime date)
		{
			DateTime origin = new(1970, 1, 1, 0, 0, 0, 0);
			TimeSpan diff = date - origin;
			return Math.Floor(diff.TotalSeconds);
		}


	}
}
