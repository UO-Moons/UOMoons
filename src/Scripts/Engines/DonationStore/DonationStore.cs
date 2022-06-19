using MySql.Data.MySqlClient;
using Server.Commands;
using System;
using System.Collections;
using System.Data;
using System.Reflection;

namespace Server.Engines.PlayerDonation;
//public delegate Item ConstructCallback();

public class DonationStore
{
	private const string DatabaseDriver = "{MySQL ODBC 3.51 Driver}";
	private static readonly string DatabaseServer = Settings.Configuration.Get<string>("DonationStore", "DatabaseServer", null);//"dbexample.myaddress.com"; // your MySQL database hostname
	private static readonly string DatabaseName = Settings.Configuration.Get<string>("DonationStore", "DatabaseName", null); //myshard_example_db";          // the database name of your donation store db
	private static readonly string DatabaseUserId = Settings.Configuration.Get<string>("DonationStore", "DatabaseUserID", null);//"example";             // username for your MySQL database access
	private static readonly string DatabasePassword = Settings.Configuration.Get<string>("DonationStore", "DatabasePassword", null);//"randomPassword";    // password

	private static readonly string ConnectionString = $"driver={DatabaseDriver};server={DatabaseServer};database={DatabaseName};uid={DatabaseUserId};pwd={DatabasePassword}";

	//public static void Initialize()
	//{
	//    CommandSystem.Register("claimalldonationitems", AccessLevel.Player, new CommandEventHandler(ClaimAllDonationItems_OnCommand));
	//}

	public static ArrayList GetDonationGiftList(string username)
	{
		//get a list of item from redeemable_gift table
		ArrayList redeemableGifts = new();

		IDbConnection connection = null;
		IDbCommand command = null;
		IDataReader reader = null;
		try
		{
			connection = new MySqlConnection(ConnectionString);

			connection.Open();


			command = connection.CreateCommand();

			command.CommandText = $"SELECT redeemable_gift.id AS id, redeemable_gift.type_id AS type, gift_type.type_name AS name FROM redeemable_gift INNER JOIN gift_type ON redeemable_gift.type_id=gift_type.type_id WHERE redeemable_gift.account_name='{username}' ORDER BY redeemable_gift.id ASC";
			reader = command.ExecuteReader();

			while (reader.Read())
			{
				int giftId = Convert.ToInt32(reader["id"]);
				int giftTypeId = Convert.ToInt32(reader["type"]);
				string giftName = (string)reader["name"];
				DonationGift gift = new(giftId, giftTypeId, giftName);
				redeemableGifts.Add(gift);
			}
			reader.Close();
		}
		catch (Exception e)
		{
			Console.WriteLine("[Retrieve Donation Gift List] Error...");
			Console.WriteLine(e);
		}
		finally
		{
			if (reader is {IsClosed: false})
				reader.Close();
			if (command != null)
			{
				command.Dispose();
				connection.Close();
			}
		}

		return redeemableGifts;
	}

	public static IEntity RedeemGift(long giftId, string username)
	{
		// move the record from redeenable_gift table to redeemed_gift table
		IDbConnection connection = null;
		IDbCommand command = null;
		IDataReader reader = null;

		IEntity gift = null;

		try
		{
			connection = new MySqlConnection(ConnectionString);

			connection.Open();
			command = connection.CreateCommand();

			//get the gift type by selecting redeemable_gift table using id
			command.CommandText = $"SELECT type_id,donate_time,paypal_txn_id FROM redeemable_gift WHERE id='{giftId}' AND account_name='{username}'";
			reader = command.ExecuteReader();

			int typeId;
			int donateTime;
			string paypalTxnId;

			if (reader.Read())
			{
				typeId = Convert.ToInt32(reader["type_id"]);
				donateTime = Convert.ToInt32(reader["donate_time"]);
				paypalTxnId = (string)reader["paypal_txn_id"];
			}
			else
			{
				Console.WriteLine($"[Redeem Donation Gift] No such Gift(ID:{giftId}) for Account Name: {username}");
				return null;
			}
			reader.Close();
			command.Dispose();

			// insert record to redeemed_gift first
			command = connection.CreateCommand();
			IDbTransaction transaction = connection.BeginTransaction();
			command.Connection = connection;
			command.Transaction = transaction;
			DateTime currTime = DateTime.Now;

			string classConstructString = GetClassNameByType(typeId);
			gift = GetGiftInstance(classConstructString);
			if (gift == null)
			{
				Console.WriteLine($"[Redeem Donation Gift] Unable to finished the process. Gift(ID:{giftId}) for Account Name: {username}");
			}

			//get the Serial from its instance
			if (gift != null)
			{
				Serial serial = gift.Serial.Value;

				//update the serial to database for your later tracking
				command.CommandText = $"INSERT INTO redeemed_gift (id,type_id,account_name,donate_time,redeem_time,serial,paypal_txn_id) VALUES ('{giftId}','{typeId}','{username}','{donateTime}','{Convert.ToInt32(ToUnixTimestamp(currTime))}','{serial}','{paypalTxnId}')";
			}

			if (command.ExecuteNonQuery() != 1)
			{
				Console.WriteLine($"[Redeem Donation Gift] (insert record to redeemed_gift) SQL Error. Unable to finished the process. Gift(ID:{giftId}) for Account Name: {username}");
				transaction.Rollback();
				return null;
			}

			//remove record from redeemable_gift
			command.CommandText = $"DELETE FROM redeemable_gift WHERE id='{giftId}' AND account_name='{username}'";

			if (command.ExecuteNonQuery() != 1)
			{
				Console.WriteLine($"[Redeem Donation Gift] (remove record from redeemable_gift) SQL Error. Unable to finished the process. Gift(ID:{giftId}) for Account Name: {username}");
				transaction.Rollback();
				return null;
			}
			transaction.Commit();
		}
		catch (Exception e)
		{
			Console.WriteLine("[Redeem Donation Gift] Error...");
			Console.WriteLine(e);
		}
		finally
		{
			if (reader is {IsClosed: false})
				reader.Close();
			if (command != null)
			{
				command.Dispose();
				connection.Close();
			}
		}

		return gift;
	}

	public static string GetClassNameByType(int typeId)
	{
		IDbConnection connection = null;
		IDbCommand command = null;
		IDataReader reader = null;

		string className = string.Empty;

		try
		{
			connection = new MySqlConnection(ConnectionString);

			connection.Open();
			command = connection.CreateCommand();

			command.CommandText = $"SELECT class_name FROM gift_type WHERE type_id='{typeId}'";
			reader = command.ExecuteReader();


			if (reader.Read())
			{
				className = (string)reader["class_name"];
			}
			else
			{
				Console.WriteLine($"[Retrieve Donation Gift Class Name] No such gift type: {typeId}");
				return null;
			}


			reader.Close();
			command.Dispose();
			connection.Close();
		}
		catch (Exception e)
		{
			Console.WriteLine("[Retrieve Donation Gift Class Name] Error...");
			Console.WriteLine(e);
		}
		finally
		{
			if (reader is {IsClosed: false})
				reader.Close();
			if (command != null)
			{
				command.Dispose();
				connection.Close();
			}
		}

		return className.Trim();
	}

	public static IEntity GetGiftInstance(string classConstructString)
	{
		IEntity gift = null;
		//create the object of the gift by its name
		string[] classContructParams = classConstructString.Split(' '); // use space as sperator
		string className = classContructParams[0];
		Type giftType = Assembler.FindTypeByName(className);
		ConstructorInfo[] ctors = giftType.GetConstructors();

		for (int i = 0; i < ctors.Length; ++i)
		{
			ConstructorInfo ctor = ctors[i];

			if (!Add.IsConstructable(ctor, AccessLevel.GameMaster))
				continue;

			ParameterInfo[] paramList = ctor.GetParameters();
			if (paramList.Length == (classContructParams.Length - 1))   // we don't use complex constructors to create the item
			{
				string[] args = new string[classContructParams.Length - 1];
				Array.Copy(classContructParams, 1, args, 0, args.Length);
				object[] param = Add.ParseValues(paramList, args);
				if (param == null)
					continue;
				object giftInstance = ctor.Invoke(param);
				gift = (IEntity)giftInstance;
				break;
			}
		}

		// get the accessor of this item and check whether it has IsDonation attribute
		PropertyInfo propInfo = giftType.GetProperty("IsDonationItem");
		if (propInfo != null)
		{
			MethodInfo setterMethod = propInfo.GetSetMethod();
			bool isDonationItem = true;
			object[] parameters = {true};
			if (setterMethod != null) setterMethod.Invoke(gift, parameters);
		}

		/*
		ConstructCallback cstr = new ConstructCallback( className );
		gift = cstr();
		*/

		return gift;
	}

	private static double ToUnixTimestamp(DateTime date)
	{
		DateTime origin = new(1970, 1, 1, 0, 0, 0, 0);
		TimeSpan diff = date - origin;

		return Math.Floor(diff.TotalSeconds);
	}


}
