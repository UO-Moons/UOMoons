using MySql.Data.MySqlClient;
using Server.Mysql.Save;
using System;
using System.Net;

namespace Server.Items
{
	public class Test_Item : Item
	{
		private string Test;
		private DateTime dateTime;
		private IPAddress ipaddress;
		private int Int;
		private TimeSpan timespan;
		private decimal deCimal;
		private long loNg;
		private uint uiNt;
		private short shoRt;
		private ushort ushoRt;
		private double doubLe;

		[Constructable]
		public Test_Item()
			: base(0x108F)
		{
			LootType = LootType.Blessed;
			Movable = true;
			Name = "A Test Item";
			Hue = 5;
			Test = "default";
			dateTime = new DateTime(1);
			ipaddress = new IPAddress(1);
			Int = 5;
			timespan = new TimeSpan(1);
			deCimal = (decimal)1.5;
			loNg = 123456789;
			uiNt = 123456;
			shoRt = 123;
			ushoRt = 7777;
			doubLe = 100000;
		}

		public Test_Item(Serial serial)
			: base(serial)
		{
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);
			//list.Add(Test + " | " + dateTime.ToLongTimeString() + " | " + ipaddress.ToString() + " | " + Int + " | " + timespan.TotalSeconds + " | " + deCimal.ToString()
			//    + " | " + loNg.ToString() + " | " + uiNt.ToString());
			list.Add(shoRt + " | " + ushoRt + " | " + doubLe);
		}

		public override void OnDoubleClick(Mobile m)
		{
			Random ran = new();
			int random = ran.Next(999999999);
			Test = random.ToString();
			dateTime = new DateTime(random);
			ipaddress = m.NetState.Address;
			Int = random;
			timespan = new TimeSpan(random);
			deCimal = random / (decimal)77.3;
			loNg = random;
			uiNt = (uint)random;
			try
			{
				shoRt = Convert.ToInt16(ran.Next(5555));
				ushoRt = Convert.ToUInt16(ran.Next(5555));
				doubLe = Convert.ToDouble(random);
			}
			catch
			{ }
			InvalidateProperties();
		}


		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			MySQLConData ConData = new(); //Methods used for connect/disconnect/storing database info
			MySQLWrite MySqlWrite = new(); //Methods for writing data

			MySqlConnection con = ConData.OpenConnect(MySQLConData.db); //Open a new MySQL Connection

			MySqlWrite.Write(con, Serial, Test, 1); //Attempt to write string Test identified by 1
			MySqlWrite.Write(con, Serial, dateTime, 1); //Attempt to write a DateTime
			MySqlWrite.Write(con, Serial, ipaddress, 1); //Attempt to write an IPAddress
			MySqlWrite.Write(con, Serial, Int, 1); //Attempt to write an int
			MySqlWrite.Write(con, Serial, timespan, 1); //Attempt to write a TimeSpan
			MySqlWrite.Write(con, Serial, deCimal, 1); //Attempt to write a decimal
			MySqlWrite.Write(con, Serial, loNg, 1); //Attempt to write a decimal
			MySqlWrite.Write(con, Serial, uiNt, 1); //Attempt to write a uint
			MySqlWrite.Write(con, Serial, shoRt, 1); //Attempt to write a short
			MySqlWrite.Write(con, Serial, ushoRt, 1); //Attempt to write a ushort
			MySqlWrite.Write(con, Serial, doubLe, 1); //Attempt to write a double
														  //Do other writes here before closing the connect. Also, all write functions return a boolean for success or failure

			ConData.CloseConnection(con); //Close connection after writing everything


		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			_ = reader.ReadInt();

			MySQLConData ConData = new(); //Methods used for connect/disconnect/storing database info
			MySQLRead MySqlRead = new(); //Methods for reading data

			MySqlConnection con = ConData.OpenConnect(MySQLConData.db); //Open a new MySQL Connection

			Test = MySqlRead.ReadString(con, Serial, "No data found", 1); //Attempt to read a string, if no string is found in the DB, return No data found
			dateTime = MySqlRead.ReadDateTime(con, Serial, new DateTime(1), 1); //Attempt to read DateTime
			ipaddress = MySqlRead.ReadIPAddress(con, Serial, new IPAddress(1), 1); //Attempt to read IPAddress
			Int = MySqlRead.ReadInt(con, Serial, 1, 1); //Attempt to read int
			timespan = MySqlRead.ReadTimeSpan(con, Serial, new TimeSpan(1), 1); //Attempt to read timespan
			deCimal = MySqlRead.ReadDecimal(con, Serial, (decimal)1.5, 1); //Read decimal
			loNg = MySqlRead.ReadLong(con, Serial, (long)1.5, 1); //Read long
			uiNt = MySqlRead.ReadUint(con, Serial, 1577, 1); //Read uint
			shoRt = MySqlRead.ReadShort(con, Serial, 1577, 1); //Read short
			ushoRt = MySqlRead.ReadUshort(con, Serial, 17, 1); //Read ushort
			doubLe = MySqlRead.ReadDouble(con, Serial, 4098, 1); //Read double

			ConData.CloseConnection(con); //Close connection after reading everything

		}

	}


}
