using MySql.Data.MySqlClient;
using System;
using System.Net;

namespace Server.Mysql.Save
{
	public class MySQLRead
	{
		public static void Initialize()
		{
			Console.Write("Mysql Read Loaded.");
			Console.WriteLine();
		}

		[Usage("readmy [description]")]
		[Description("Attempts to save to mysql.")]
		public string ReadString(MySqlConnection con, Serial serial, string return_if_no_data, int Identifier)
		{
			string type = "string", sql, stringg = return_if_no_data;

			try
			{
				sql = "SELECT * FROM `" + MySQLConData.database + "`.`" + type + "` WHERE Iden='" + Identifier + "' AND serial='" + serial.Value + "'";
				MySqlCommand cmd = new(sql, con);
				MySqlDataReader dataReader = cmd.ExecuteReader();

				if (dataReader.Read())
				{
					stringg = (string)dataReader["string"];
					int id = (int)dataReader["id"];
					dataReader.Close();
				}
				if (stringg.Length < 1)
					return return_if_no_data;

				return stringg;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error: " + ex);
				return return_if_no_data;
			}
		}

		public DateTime ReadDateTime(MySqlConnection con, Serial serial, DateTime return_if_no_data, int Identifier)
		{
			string type = "dateTime", sql;
			DateTime stringg = return_if_no_data;

			try
			{
				sql = "SELECT * FROM `" + MySQLConData.database + "`.`" + type + "` WHERE Iden='" + Identifier + "' AND serial='" + serial.Value + "'";
				MySqlCommand cmd = new(sql, con);
				MySqlDataReader dataReader = cmd.ExecuteReader();

				if (dataReader.Read())
				{
					stringg = new DateTime((long)dataReader["dateTime"]);
					dataReader.Close();
				}
				return stringg;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error: " + ex);
				return return_if_no_data;
			}
		}

		public IPAddress ReadIPAddress(MySqlConnection con, Serial serial, IPAddress return_if_no_data, int Identifier)
		{
			string type = "ipaddress", sql;
			IPAddress stringg = return_if_no_data;

			try
			{
				sql = "SELECT * FROM `" + MySQLConData.database + "`.`" + type + "` WHERE Iden='" + Identifier + "' AND serial='" + serial.Value + "'";
				MySqlCommand cmd = new(sql, con);
				MySqlDataReader dataReader = cmd.ExecuteReader();

				if (dataReader.Read())
				{
					stringg = new IPAddress((long)dataReader[type]);
					dataReader.Close();
				}
				return stringg;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error: " + ex);
				return return_if_no_data;
			}
		}

		public int ReadInt(MySqlConnection con, Serial serial, int return_if_no_data, int Identifier)
		{
			string type = "int", sql;
			int stringg = return_if_no_data;

			try
			{
				sql = "SELECT * FROM `" + MySQLConData.database + "`.`" + type + "` WHERE Iden='" + Identifier + "' AND serial='" + serial.Value + "'";
				MySqlCommand cmd = new(sql, con);
				MySqlDataReader dataReader = cmd.ExecuteReader();

				if (dataReader.Read())
				{
					stringg = ((int)dataReader[type]);
					dataReader.Close();
				}
				return stringg;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error: " + ex);
				return return_if_no_data;
			}
		}

		public TimeSpan ReadTimeSpan(MySqlConnection con, Serial serial, TimeSpan return_if_no_data, int Identifier)
		{
			string type = "timespan", sql;
			TimeSpan stringg = return_if_no_data;

			try
			{
				sql = "SELECT * FROM `" + MySQLConData.database + "`.`" + type + "` WHERE Iden='" + Identifier + "' AND serial='" + serial.Value + "'";
				MySqlCommand cmd = new(sql, con);
				MySqlDataReader dataReader = cmd.ExecuteReader();

				if (dataReader.Read())
				{
					stringg = new TimeSpan((long)dataReader[type]);
					dataReader.Close();
				}
				return stringg;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error: " + ex);
				return return_if_no_data;
			}
		}

		public decimal ReadDecimal(MySqlConnection con, Serial serial, decimal return_if_no_data, int Identifier)
		{
			string type = "decimal", sql;
			decimal stringg = return_if_no_data;

			try
			{
				sql = "SELECT * FROM `" + MySQLConData.database + "`.`" + type + "` WHERE Iden='" + Identifier + "' AND serial='" + serial.Value + "'";
				MySqlCommand cmd = new(sql, con);
				MySqlDataReader dataReader = cmd.ExecuteReader();

				if (dataReader.Read())
				{
					stringg = ((decimal)dataReader[type]);
					dataReader.Close();
				}
				return stringg;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error: " + ex);
				return return_if_no_data;
			}
		}

		public long ReadLong(MySqlConnection con, Serial serial, long return_if_no_data, int Identifier)
		{
			string type = "long", sql;
			long stringg = return_if_no_data;

			try
			{
				sql = "SELECT * FROM `" + MySQLConData.database + "`.`" + type + "` WHERE Iden='" + Identifier + "' AND serial='" + serial.Value + "'";
				MySqlCommand cmd = new(sql, con);
				MySqlDataReader dataReader = cmd.ExecuteReader();

				if (dataReader.Read())
				{
					stringg = ((long)dataReader[type]);
					dataReader.Close();
				}
				return stringg;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error: " + ex);
				return return_if_no_data;
			}
		}

		public uint ReadUint(MySqlConnection con, Serial serial, uint return_if_no_data, int Identifier)
		{
			string type = "uint", sql;
			uint stringg = return_if_no_data;

			try
			{
				sql = "SELECT * FROM `" + MySQLConData.database + "`.`" + type + "` WHERE Iden='" + Identifier + "' AND serial='" + serial.Value + "'";
				MySqlCommand cmd = new(sql, con);
				MySqlDataReader dataReader = cmd.ExecuteReader();

				if (dataReader.Read())
				{

					stringg = Convert.ToUInt32((dataReader[type]));
					dataReader.Close();
				}
				return stringg;
			}
			catch (Exception ex)
			{
				Console.WriteLine();
				Console.WriteLine("Error: " + ex);
				Console.WriteLine();
				return return_if_no_data;
			}
		}

		public short ReadShort(MySqlConnection con, Serial serial, short return_if_no_data, int Identifier)
		{
			string type = "short", sql;
			short stringg = return_if_no_data;

			try
			{
				sql = "SELECT * FROM `" + MySQLConData.database + "`.`" + type + "` WHERE Iden='" + Identifier + "' AND serial='" + serial.Value + "'";
				MySqlCommand cmd = new(sql, con);
				MySqlDataReader dataReader = cmd.ExecuteReader();

				if (dataReader.Read())
				{

					stringg = Convert.ToInt16((dataReader[type]));
					dataReader.Close();
				}
				return stringg;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error: " + ex);
				return return_if_no_data;
			}
		}

		public ushort ReadUshort(MySqlConnection con, Serial serial, ushort return_if_no_data, int Identifier)
		{
			string type = "ushort", sql;
			ushort stringg = return_if_no_data;

			try
			{
				sql = "SELECT * FROM `" + MySQLConData.database + "`.`" + type + "` WHERE Iden='" + Identifier + "' AND serial='" + serial.Value + "'";
				MySqlCommand cmd = new(sql, con);
				MySqlDataReader dataReader = cmd.ExecuteReader();

				if (dataReader.Read())
				{

					stringg = Convert.ToUInt16((dataReader[type]));
					dataReader.Close();
				}
				return stringg;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error: " + ex);
				return return_if_no_data;
			}
		}

		public double ReadDouble(MySqlConnection con, Serial serial, double return_if_no_data, int Identifier)
		{
			string type = "double", sql;
			double stringg = return_if_no_data;

			try
			{
				sql = "SELECT * FROM `" + MySQLConData.database + "`.`" + type + "` WHERE Iden='" + Identifier + "' AND serial='" + serial.Value + "'";
				MySqlCommand cmd = new(sql, con);
				MySqlDataReader dataReader = cmd.ExecuteReader();

				if (dataReader.Read())
				{

					stringg = Convert.ToDouble(dataReader[type]);
					dataReader.Close();
				}
				return stringg;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error: " + ex);
				return return_if_no_data;
			}
		}
	}
}
