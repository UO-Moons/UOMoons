using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Xml;

namespace Server.Accounting
{
	public class Accounts
	{
		private static Dictionary<string, IAccount> m_Accounts = new();
		private static Hashtable m_AccsByMail = new Hashtable();

		public static void Configure()
		{
			EventSink.OnWorldLoad += OnWorldLoad;
			EventSink.OnWorldSave += OnWorldSave;
		}

		static Accounts()
		{
		}

		public static int Count => m_Accounts.Count;

		public static ICollection<IAccount> GetAccounts()
		{
			return m_Accounts.Values;
		}

		public static IAccount GetAccount(string username)
		{

			m_Accounts.TryGetValue(username, out IAccount a);

			return a;
		}

		public static Account AddAccount(string user, string pass, string email)
		{
			Account a = new(user, pass);
			if (m_Accounts.Count == 0)
				a.AccessLevel = AccessLevel.Administrator;

			m_Accounts[a.Username] = a;

			SetEmail(a, email);

			return a;
		}

		public static void SetEmail(Account acc, string email)
		{
			if (acc.Email == "" || acc.Email != email)
				acc.Email = email;
		}

		public static bool RegisterEmail(Account acc, string newMail)
		{
			UnregisterEmail(acc.Email);
			if (newMail == "")
				return true;

			if (m_AccsByMail.Contains(newMail))
				return false;

			m_AccsByMail.Add(newMail, acc);
			return true;
		}

		public static void UnregisterEmail(string mail)
		{
			if (mail != null && mail != "")
				m_AccsByMail.Remove(mail);
		}

		public static Account GetByMail(string email)
		{
			return m_AccsByMail[email] as Account;
		}

		public static void Add(IAccount a)
		{
			m_Accounts[a.Username] = a;
		}

		public static void Remove(string username)
		{
			m_Accounts.Remove(username);
		}

		public static void OnWorldLoad()
		{
			m_Accounts = new Dictionary<string, IAccount>(32, StringComparer.OrdinalIgnoreCase);

			string filePath = Path.Combine("Saves/Accounts", "accounts.xml");

			if (!File.Exists(filePath))
				return;

			XmlDocument doc = new();
			doc.Load(filePath);

			XmlElement root = doc["accounts"];

			foreach (XmlElement account in root.GetElementsByTagName("account"))
			{
				try
				{
					Account acct = new(account);
				}
				catch
				{
					Console.WriteLine("Warning: Account instance load failed");
				}
			}
		}

		public static void OnWorldSave()
		{
			if (!Directory.Exists("Saves/Accounts"))
				Directory.CreateDirectory("Saves/Accounts");

			string filePath = Path.Combine("Saves/Accounts", "accounts.xml");

			using StreamWriter op = new(filePath);
			XmlTextWriter xml = new(op)
			{
				Formatting = Formatting.Indented,
				IndentChar = '\t',
				Indentation = 1
			};

			xml.WriteStartDocument(true);
			xml.WriteStartElement("accounts");
			xml.WriteAttributeString("count", m_Accounts.Count.ToString());

			foreach (Account a in GetAccounts())
				a.Save(xml);

			xml.WriteEndElement();
			xml.Close();
		}
	}
}
