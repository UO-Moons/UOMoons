using Server.Accounting;
using System;
using System.Collections;
using System.IO;

namespace Server.Commands
{
	public class ExportEmail
	{
		private const string FileName = "Email Addresses";
		static bool Exporting = false;
		public static void Initialize()
		{
			CommandSystem.Register("ExportEmails", AccessLevel.Administrator, new CommandEventHandler(ExportEmails_OnCommand));
			//Timer.DelayCall(TimeSpan.FromMinutes(10.0), TimeSpan.FromMinutes(10.0), new TimerCallback(ProcessOne));
		}

		private static StreamWriter m_EmailData;

		public const string EntrySep = "; ";

		public static void ExportEmails_OnCommand(CommandEventArgs e)
		{
			if (Exporting)
			{
				e.Mobile.SendMessage("Emails Are Already Being Exported to Logs, Please Wait...");
				return;
			}

			e.Mobile.SendMessage("Exporting All Valid Email Addresses");
			ProcessOne();
			e.Mobile.SendMessage("Done Exporting Valid Email Addresses to Logs Folder");
		}

		public static void ProcessOne()
		{
			if (Exporting)
			{
				Console.WriteLine("Could Not Export Email Addresses... Exporting Already In Progress...");
				return;
			}

			//Console.Write("Exporting All Valid Email Addresses...");
			Exporting = true;

			ProcessTwo();

			ArrayList toExport = new();

			foreach (Account acc in Accounts.GetAccounts())
				toExport.Add(acc);

			foreach (Account a in toExport)
			{
				if (a != null && (a.GetTag("Email") != null))
				{
					string emailaddress = a.GetTag("Email");
					m_EmailData.Write(emailaddress);
					m_EmailData.Write(EntrySep);
				}
			}

			m_EmailData.Close();
			Exporting = false;
			//Console.Write("Done!");
		}

		public static void ProcessTwo()
		{
			m_EmailData = UseUniqueWriter();
		}

		public static StreamWriter UseUniqueWriter()
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
					}
				}
			}

			return null;
		}

	}
}
