/*using Server.Network;
using System;
using Server.Misc;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Server.Accounting
{
	public class AccountSettings
	{
		public static void Configure()
		{
			Console.Write("Accounting: Loading settings...");
			if (LoadSettings())
				Console.WriteLine("done");
			else
				Console.WriteLine("failed");
		}

		public static bool LoadSettings()
		{
			string filePath = Path.Combine( "Data", "Configuration.xml" );

			if ( !File.Exists( filePath ) )
				return false;

			XmlDocument x = new XmlDocument();
			x.Load(filePath);

			try
			{
				XmlElement e = x["Configuration"];
				if (e == null) return false;
				e = e["AccountSettings"];
				if (e == null) return false;

				foreach (XmlElement s in e)
				{
					try
					{
						switch (s.Name)
						{
							//case "AutoAccountCreation": AccountHandler.AutoAccountCreation = Utility.ToBoolean(s.InnerText); break;
							//case "MaxAccountsPerIP": AccountHandler.MaxAccountsPerIP = Utility.GetXMLInt32(s.InnerText, 1); break;
							//case "MaxConnectionsPerIP": IPLimiter.MaxAddresses = Utility.GetXMLInt32(s.InnerText, 10); break;
							//case "RestrictCharacterDeletion": AccountHandler.RestrictCharacterDeletion = Utility.ToBoolean(s.InnerText); break;
							//case "CharacterDeletionDelay": AccountHandler.DeleteDelay = Utility.GetXMLTimeSpan(s.InnerText, AccountHandler.DeleteDelay); break;
							//case "PasswordProtection": Enum.TryParse(s.InnerText, true, out AccountHandler.ProtectPasswords); break;
							//case "YoungDuration": Account.YoungDuration = Utility.GetXMLTimeSpan(s.InnerText, Account.YoungDuration); break;
							//case "InactiveDuration": Account.InactiveDuration = Utility.GetXMLTimeSpan(s.InnerText, Account.InactiveDuration); break;
							//case "EmptyInactiveDuration": Account.InactiveDuration = Utility.GetXMLTimeSpan(s.InnerText, Account.InactiveDuration); break;
							case "StartingLocations":
								List<CityInfo> cities = new List<CityInfo>();
								foreach (XmlElement c in s["CityInfo"]!)
								{
									try
									{
										cities.Add(new CityInfo(c.GetAttribute("cityName"), c.GetAttribute("buildingName"), Utility.GetXMLInt32(c.GetAttribute("description"), 0), Utility.GetXMLInt32(c.GetAttribute("x"), 0), Utility.GetXMLInt32(c.GetAttribute("y"), 0), Utility.GetXMLInt32(c.GetAttribute("z"), 0)));
									}
									catch (Exception ex)
									{
										Console.WriteLine("Warning: Could not load CityInfo '{0}'", c.Value);
										Console.WriteLine(ex);
									}
								}
								AccountHandler.StartingCities = cities.ToArray();
								break;
							default: Console.WriteLine("Warning: Unknown element '{0}' in AccountSettings", s.Name); break;
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine("Warning: Could not load AccountSetting '{0}'", s.Value);
						Console.WriteLine(ex);
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return false;
			}

			return true;
		}
	}
}*/
