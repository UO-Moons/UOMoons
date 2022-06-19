using MySql.Data.MySqlClient;
using System;
using System.Net;

namespace Server.Mysql.Save
{
	public class MySqlWrite
	{
		public static void Initialize()
		{
			Console.Write("Mysql Write loaded.");
			Console.WriteLine();
		}

		[Usage("testmy [description]")]
		[Description("Attempts to save to mysql.")]
		public bool Write(MySqlConnection con, Serial serial, string String, int Identifier)
		{
			string type = "string", sql;
			try
			{
				sql = "SELECT * FROM `" + MySqlConData.Database + "`.`" + type + "` WHERE Iden='" + Identifier + "' AND serial='" + serial.Value + "'";
				MySqlCommand cmd = new(sql, con);
				MySqlDataReader dataReader = cmd.ExecuteReader();

				if (dataReader.Read())
				{
					sql = "DELETE FROM `" + MySqlConData.Database + "`.`" + type + "` WHERE Iden='" + Identifier + "' AND serial='" + serial.Value + "'";
					dataReader.Close();
					cmd = new MySqlCommand(sql, con);
					cmd.ExecuteNonQuery();
				}

				if (!dataReader.IsClosed)
					dataReader.Close();

				sql = "INSERT INTO `" + MySqlConData.Database + "`.`" + type + "` (`id`, `serial`, `string`, `Iden`) VALUES (NULL, '" + serial.Value + "', '" + String + "', '" + Identifier + "');";

				cmd = new MySqlCommand(sql, con);
				cmd.ExecuteNonQuery();
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error: " + ex);
				return false;
			}
		}

		public bool Write(MySqlConnection con, Serial serial, DateTime dateTime, int Identifier)
		{
			string type = "dateTime", sql;
			try
			{
				sql = "SELECT * FROM `" + MySqlConData.Database + "`.`" + type + "` WHERE Iden='" + Identifier + "' AND serial='" + serial.Value + "'";
				MySqlCommand cmd = new(sql, con);
				MySqlDataReader dataReader = cmd.ExecuteReader();

				if (dataReader.Read())
				{
					sql = "DELETE FROM `" + MySqlConData.Database + "`.`" + type + "` WHERE Iden='" + Identifier + "' AND serial='" + serial.Value + "'";
					dataReader.Close();
					cmd = new MySqlCommand(sql, con);
					cmd.ExecuteNonQuery();
				}

				if (!dataReader.IsClosed)
					dataReader.Close();

				sql = "INSERT INTO `" + MySqlConData.Database + "`.`" + type + "` (`id`, `serial`, `" + type + "`, `Iden`) VALUES (NULL, '" + serial.Value + "', '" + dateTime.Ticks + "', '" + Identifier + "');";

				cmd = new MySqlCommand(sql, con);
				cmd.ExecuteNonQuery();
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error: " + ex);
				return false;
			}
		}
		public bool Write(MySqlConnection con, Serial serial, IPAddress value, int Identifier)
		{
			long data = Utility.GetLongAddressValue(value);

			string type = "ipaddress", sql;
			try
			{
				sql = "SELECT * FROM `" + MySqlConData.Database + "`.`" + type + "` WHERE Iden='" + Identifier + "' AND serial='" + serial.Value + "'";
				MySqlCommand cmd = new(sql, con);
				MySqlDataReader dataReader = cmd.ExecuteReader();

				if (dataReader.Read())
				{
					sql = "DELETE FROM `" + MySqlConData.Database + "`.`" + type + "` WHERE Iden='" + Identifier + "' AND serial='" + serial.Value + "'";
					dataReader.Close();
					cmd = new MySqlCommand(sql, con);
					cmd.ExecuteNonQuery();
				}

				if (!dataReader.IsClosed)
					dataReader.Close();

				sql = "INSERT INTO `" + MySqlConData.Database + "`.`" + type + "` (`id`, `serial`, `" + type + "`, `Iden`) VALUES (NULL, '" + serial.Value + "', '" + data + "', '" + Identifier + "');";

				cmd = new MySqlCommand(sql, con);
				cmd.ExecuteNonQuery();
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error: " + ex);
				return false;
			}
		}

		public bool Write(MySqlConnection con, Serial serial, int value, int Identifier)
		{
			string type = "int", sql;
			try
			{
				sql = "SELECT * FROM `" + MySqlConData.Database + "`.`" + type + "` WHERE Iden='" + Identifier + "' AND serial='" + serial.Value + "'";
				MySqlCommand cmd = new(sql, con);
				MySqlDataReader dataReader = cmd.ExecuteReader();

				if (dataReader.Read())
				{
					sql = "DELETE FROM `" + MySqlConData.Database + "`.`" + type + "` WHERE Iden='" + Identifier + "' AND serial='" + serial.Value + "'";
					dataReader.Close();
					cmd = new MySqlCommand(sql, con);
					cmd.ExecuteNonQuery();
				}

				if (!dataReader.IsClosed)
					dataReader.Close();

				sql = "INSERT INTO `" + MySqlConData.Database + "`.`" + type + "` (`id`, `serial`, `" + type + "`, `Iden`) VALUES (NULL, '" + serial.Value + "', '" + value + "', '" + Identifier + "');";

				cmd = new MySqlCommand(sql, con);
				cmd.ExecuteNonQuery();
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error: " + ex);
				return false;
			}
		}

		public bool Write(MySqlConnection con, Serial serial, TimeSpan value, int Identifier)
		{
			string type = "timespan", sql;
			try
			{
				sql = "SELECT * FROM `" + MySqlConData.Database + "`.`" + type + "` WHERE Iden='" + Identifier + "' AND serial='" + serial.Value + "'";
				MySqlCommand cmd = new(sql, con);
				MySqlDataReader dataReader = cmd.ExecuteReader();

				if (dataReader.Read())
				{
					sql = "DELETE FROM `" + MySqlConData.Database + "`.`" + type + "` WHERE Iden='" + Identifier + "' AND serial='" + serial.Value + "'";
					dataReader.Close();
					cmd = new MySqlCommand(sql, con);
					cmd.ExecuteNonQuery();
				}

				if (!dataReader.IsClosed)
					dataReader.Close();

				sql = "INSERT INTO `" + MySqlConData.Database + "`.`" + type + "` (`id`, `serial`, `" + type + "`, `Iden`) VALUES (NULL, '" + serial.Value + "', '" + value.Ticks + "', '" + Identifier + "');";

				cmd = new MySqlCommand(sql, con);
				cmd.ExecuteNonQuery();
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error: " + ex);
				return false;
			}
		}

		public bool Write(MySqlConnection con, Serial serial, decimal value, int Identifier)
		{
			string type = "decimal", sql;
			try
			{
				sql = "SELECT * FROM `" + MySqlConData.Database + "`.`" + type + "` WHERE Iden='" + Identifier + "' AND serial='" + serial.Value + "'";
				MySqlCommand cmd = new(sql, con);
				MySqlDataReader dataReader = cmd.ExecuteReader();

				if (dataReader.Read())
				{
					sql = "DELETE FROM `" + MySqlConData.Database + "`.`" + type + "` WHERE Iden='" + Identifier + "' AND serial='" + serial.Value + "'";
					dataReader.Close();
					cmd = new MySqlCommand(sql, con);
					cmd.ExecuteNonQuery();
				}

				if (!dataReader.IsClosed)
					dataReader.Close();

				sql = "INSERT INTO `" + MySqlConData.Database + "`.`" + type + "` (`id`, `serial`, `" + type + "`, `Iden`) VALUES (NULL, '" + serial.Value + "', '" + value + "', '" + Identifier + "');";

				cmd = new MySqlCommand(sql, con);
				cmd.ExecuteNonQuery();
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error: " + ex);
				return false;
			}
		}

		public bool Write(MySqlConnection con, Serial serial, long value, int Identifier)
		{
			string type = "long", sql;
			try
			{
				sql = "SELECT * FROM `" + MySqlConData.Database + "`.`" + type + "` WHERE Iden='" + Identifier + "' AND serial='" + serial.Value + "'";
				MySqlCommand cmd = new(sql, con);
				MySqlDataReader dataReader = cmd.ExecuteReader();

				if (dataReader.Read())
				{
					sql = "DELETE FROM `" + MySqlConData.Database + "`.`" + type + "` WHERE Iden='" + Identifier + "' AND serial='" + serial.Value + "'";
					dataReader.Close();
					cmd = new MySqlCommand(sql, con);
					cmd.ExecuteNonQuery();
				}

				if (!dataReader.IsClosed)
					dataReader.Close();

				sql = "INSERT INTO `" + MySqlConData.Database + "`.`" + type + "` (`id`, `serial`, `" + type + "`, `Iden`) VALUES (NULL, '" + serial.Value + "', '" + value + "', '" + Identifier + "');";

				cmd = new MySqlCommand(sql, con);
				cmd.ExecuteNonQuery();
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error: " + ex);
				return false;
			}
		}

		public bool Write(MySqlConnection con, Serial serial, uint value, int Identifier)
		{
			string type = "uint", sql;
			try
			{
				sql = "SELECT * FROM `" + MySqlConData.Database + "`.`" + type + "` WHERE Iden='" + Identifier + "' AND serial='" + serial.Value + "'";
				MySqlCommand cmd = new(sql, con);
				MySqlDataReader dataReader = cmd.ExecuteReader();

				if (dataReader.Read())
				{
					sql = "DELETE FROM `" + MySqlConData.Database + "`.`" + type + "` WHERE Iden='" + Identifier + "' AND serial='" + serial.Value + "'";
					dataReader.Close();
					cmd = new MySqlCommand(sql, con);
					cmd.ExecuteNonQuery();
				}

				if (!dataReader.IsClosed)
					dataReader.Close();

				sql = "INSERT INTO `" + MySqlConData.Database + "`.`" + type + "` (`id`, `serial`, `" + type + "`, `Iden`) VALUES (NULL, '" + serial.Value + "', '" + value + "', '" + Identifier + "');";

				cmd = new MySqlCommand(sql, con);
				cmd.ExecuteNonQuery();
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error: " + ex);
				return false;
			}
		}

		public bool Write(MySqlConnection con, Serial serial, short value, int Identifier)
		{
			if (value > 32767)
				value = 32767;

			if (value < -32767)
				value = -32767;

			string type = "short", sql;
			try
			{
				sql = "SELECT * FROM `" + MySqlConData.Database + "`.`" + type + "` WHERE Iden='" + Identifier + "' AND serial='" + serial.Value + "'";
				MySqlCommand cmd = new(sql, con);
				MySqlDataReader dataReader = cmd.ExecuteReader();

				if (dataReader.Read())
				{
					sql = "DELETE FROM `" + MySqlConData.Database + "`.`" + type + "` WHERE Iden='" + Identifier + "' AND serial='" + serial.Value + "'";
					dataReader.Close();
					cmd = new MySqlCommand(sql, con);
					cmd.ExecuteNonQuery();
				}

				if (!dataReader.IsClosed)
					dataReader.Close();

				sql = "INSERT INTO `" + MySqlConData.Database + "`.`" + type + "` (`id`, `serial`, `" + type + "`, `Iden`) VALUES (NULL, '" + serial.Value + "', '" + value + "', '" + Identifier + "');";

				cmd = new MySqlCommand(sql, con);
				cmd.ExecuteNonQuery();
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error: " + ex);
				return false;
			}
		}

		public bool Write(MySqlConnection con, Serial serial, ushort value, int Identifier)
		{
			if (value > 65535)
				value = 65535;

			if (value < 0)
				value = 0;

			string type = "ushort", sql;
			try
			{
				sql = "SELECT * FROM `" + MySqlConData.Database + "`.`" + type + "` WHERE Iden='" + Identifier + "' AND serial='" + serial.Value + "'";
				MySqlCommand cmd = new(sql, con);
				MySqlDataReader dataReader = cmd.ExecuteReader();

				if (dataReader.Read())
				{
					sql = "DELETE FROM `" + MySqlConData.Database + "`.`" + type + "` WHERE Iden='" + Identifier + "' AND serial='" + serial.Value + "'";
					dataReader.Close();
					cmd = new MySqlCommand(sql, con);
					cmd.ExecuteNonQuery();
				}

				if (!dataReader.IsClosed)
					dataReader.Close();

				sql = "INSERT INTO `" + MySqlConData.Database + "`.`" + type + "` (`id`, `serial`, `" + type + "`, `Iden`) VALUES (NULL, '" + serial.Value + "', '" + value + "', '" + Identifier + "');";

				cmd = new MySqlCommand(sql, con);
				cmd.ExecuteNonQuery();
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error: " + ex);
				return false;
			}
		}

		public bool Write(MySqlConnection con, Serial serial, double value, int Identifier)
		{
			string type = "double", sql;
			try
			{
				sql = "SELECT * FROM `" + MySqlConData.Database + "`.`" + type + "` WHERE Iden='" + Identifier + "' AND serial='" + serial.Value + "'";
				MySqlCommand cmd = new(sql, con);
				MySqlDataReader dataReader = cmd.ExecuteReader();

				if (dataReader.Read())
				{
					sql = "DELETE FROM `" + MySqlConData.Database + "`.`" + type + "` WHERE Iden='" + Identifier + "' AND serial='" + serial.Value + "'";
					dataReader.Close();
					cmd = new MySqlCommand(sql, con);
					cmd.ExecuteNonQuery();
				}

				if (!dataReader.IsClosed)
					dataReader.Close();

				sql = "INSERT INTO `" + MySqlConData.Database + "`.`" + type + "` (`id`, `serial`, `" + type + "`, `Iden`) VALUES (NULL, '" + serial.Value + "', '" + value + "', '" + Identifier + "');";

				cmd = new MySqlCommand(sql, con);
				cmd.ExecuteNonQuery();
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error: " + ex);
				return false;
			}
		}

	}
}
