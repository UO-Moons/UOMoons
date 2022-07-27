using Server.Accounting;
using System;
using System.Collections;
using System.IO;

namespace Server.Commands
{
	public class ExportEmail
	{
		private const string FileName = "Email Addresses";
		private static bool _exporting;
		public static void Initialize()
		{
			CommandSystem.Register("ExportEmails", AccessLevel.Administrator, ExportEmails_OnCommand);
			//Timer.DelayCall(TimeSpan.FromMinutes(10.0), TimeSpan.FromMinutes(10.0), new TimerCallback(ProcessOne));
		}

		private static StreamWriter _emailData;

		private const string EntrySep = "; ";

		private static void ExportEmails_OnCommand(CommandEventArgs e)
		{
			if (_exporting)
			{
				e.Mobile.SendMessage("Emails Are Already Being Exported to Logs, Please Wait...");
				return;
			}

			e.Mobile.SendMessage("Exporting All Valid Email Addresses");
			ProcessOne();
			e.Mobile.SendMessage("Done Exporting Valid Email Addresses to Logs Folder");
		}

		private static void ProcessOne()
		{
			if (_exporting)
			{
				Console.WriteLine("Could Not Export Email Addresses... Exporting Already In Progress...");
				return;
			}

			//Console.Write("Exporting All Valid Email Addresses...");
			_exporting = true;

			ProcessTwo();

			ArrayList toExport = new();

			foreach (var account in Accounts.GetAccounts())
			{
				var acc = (Account)account;
				toExport.Add(acc);
			}

			foreach (Account a in toExport)
			{
				if (a?.GetTag("Email") == null)
					continue;
				string emailaddress = a.GetTag("Email");
				_emailData.Write(emailaddress);
				_emailData.Write(EntrySep);
			}

			_emailData.Close();
			_exporting = false;
			//Console.Write("Done!");
		}

		private static void ProcessTwo()
		{
			_emailData = UseUniqueWriter();
		}

		private static StreamWriter UseUniqueWriter()
		{
			string filePath = Path.Combine(Core.BaseDirectory, $"Logs/{FileName}.txt").Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			try
			{
				return new StreamWriter(filePath);
			}
			catch
			{
				for (int i = 0; i < 100; ++i)
				{
					try
					{
						filePath = Path.Combine(Core.BaseDirectory, $"Logs/{FileName}_{i}.txt").Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
						return new StreamWriter(filePath);
					}
					catch
					{
						// ignored
					}
				}
			}

			return null;
		}

	}
}
