using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Xml;

namespace Server.Accounting;

public class Accounts
{
	private static Dictionary<string, IAccount> _mAccounts = new();
	private static readonly Hashtable MAccsByMail = new();

	public static void Configure()
	{
		EventSink.OnWorldLoad += OnWorldLoad;
		EventSink.OnWorldSave += OnWorldSave;
	}

	static Accounts()
	{
	}

	public static int Count => _mAccounts.Count;

	public static ICollection<IAccount> GetAccounts()
	{
		return _mAccounts.Values;
	}

	public static IAccount GetAccount(string username)
	{

		_mAccounts.TryGetValue(username, out IAccount a);

		return a;
	}

	public static Account AddAccount(string user, string pass, string email)
	{
		Account a = new(user, pass);
		if (_mAccounts.Count == 0)
			a.AccessLevel = AccessLevel.Administrator;

		_mAccounts[a.Username] = a;

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

		if (MAccsByMail.Contains(newMail))
			return false;

		MAccsByMail.Add(newMail, acc);
		return true;
	}

	public static void UnregisterEmail(string mail)
	{
		if (!string.IsNullOrEmpty(mail))
			MAccsByMail.Remove(mail);
	}

	public static Account GetByMail(string email)
	{
		return MAccsByMail[email] as Account;
	}

	public static void Add(IAccount a)
	{
		_mAccounts[a.Username] = a;
	}

	public static void Remove(string username)
	{
		_mAccounts.Remove(username);
	}

	public static void OnWorldLoad()
	{
		_mAccounts = new Dictionary<string, IAccount>(32, StringComparer.OrdinalIgnoreCase);

		string filePath = Path.Combine("Saves/Accounts", "accounts.xml");

		if (!File.Exists(filePath))
			return;

		XmlDocument doc = new();
		doc.Load(filePath);

		XmlElement root = doc["accounts"];

		if (root == null) return;
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
		xml.WriteAttributeString("count", _mAccounts.Count.ToString());

		foreach (var account in GetAccounts())
		{
			var a = (Account) account;
			a.Save(xml);
		}

		xml.WriteEndElement();
		xml.Close();
	}
}
