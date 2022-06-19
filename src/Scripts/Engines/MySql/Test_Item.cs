using MySql.Data.MySqlClient;
using Server.Mysql.Save;
using System;
using System.Net;

namespace Server.Items
{
	public class TestItem : Item
	{
		private string _test;
		private DateTime _dateTime;
		private IPAddress _ipaddress;
		private int Int;
		private TimeSpan _timespan;
		private decimal _deCimal;
		private long _loNg;
		private uint _uiNt;
		private short _shoRt;
		private ushort _ushoRt;
		private double _doubLe;

		[Constructable]
		public TestItem()
			: base(0x108F)
		{
			LootType = LootType.Blessed;
			Movable = true;
			Name = "A Test Item";
			Hue = 5;
			_test = "default";
			_dateTime = new DateTime(1);
			_ipaddress = new IPAddress(1);
			Int = 5;
			_timespan = new TimeSpan(1);
			_deCimal = (decimal)1.5;
			_loNg = 123456789;
			_uiNt = 123456;
			_shoRt = 123;
			_ushoRt = 7777;
			_doubLe = 100000;
		}

		public TestItem(Serial serial)
			: base(serial)
		{
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);
			//list.Add(Test + " | " + dateTime.ToLongTimeString() + " | " + ipaddress.ToString() + " | " + Int + " | " + timespan.TotalSeconds + " | " + deCimal.ToString()
			//    + " | " + loNg.ToString() + " | " + uiNt.ToString());
			list.Add(_shoRt + " | " + _ushoRt + " | " + _doubLe);
		}

		public override void OnDoubleClick(Mobile m)
		{
			Random ran = new();
			int random = ran.Next(999999999);
			_test = random.ToString();
			_dateTime = new DateTime(random);
			_ipaddress = m.NetState.Address;
			Int = random;
			_timespan = new TimeSpan(random);
			_deCimal = random / (decimal)77.3;
			_loNg = random;
			_uiNt = (uint)random;
			try
			{
				_shoRt = Convert.ToInt16(ran.Next(5555));
				_ushoRt = Convert.ToUInt16(ran.Next(5555));
				_doubLe = Convert.ToDouble(random);
			}
			catch
			{
				// ignored
			}

			InvalidateProperties();
		}


		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			MySqlConData conData = new(); //Methods used for connect/disconnect/storing database info
			MySqlWrite mySqlWrite = new(); //Methods for writing data

			MySqlConnection con = conData.OpenConnect(MySqlConData.db); //Open a new MySQL Connection

			mySqlWrite.Write(con, Serial, _test, 1); //Attempt to write string Test identified by 1
			mySqlWrite.Write(con, Serial, _dateTime, 1); //Attempt to write a DateTime
			mySqlWrite.Write(con, Serial, _ipaddress, 1); //Attempt to write an IPAddress
			mySqlWrite.Write(con, Serial, Int, 1); //Attempt to write an int
			mySqlWrite.Write(con, Serial, _timespan, 1); //Attempt to write a TimeSpan
			mySqlWrite.Write(con, Serial, _deCimal, 1); //Attempt to write a decimal
			mySqlWrite.Write(con, Serial, _loNg, 1); //Attempt to write a decimal
			mySqlWrite.Write(con, Serial, _uiNt, 1); //Attempt to write a uint
			mySqlWrite.Write(con, Serial, _shoRt, 1); //Attempt to write a short
			mySqlWrite.Write(con, Serial, _ushoRt, 1); //Attempt to write a ushort
			mySqlWrite.Write(con, Serial, _doubLe, 1); //Attempt to write a double
														  //Do other writes here before closing the connect. Also, all write functions return a boolean for success or failure

			conData.CloseConnection(con); //Close connection after writing everything


		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			_ = reader.ReadInt();

			MySqlConData conData = new(); //Methods used for connect/disconnect/storing database info
			MySqlRead mySqlRead = new(); //Methods for reading data

			MySqlConnection con = conData.OpenConnect(MySqlConData.db); //Open a new MySQL Connection

			_test = mySqlRead.ReadString(con, Serial, "No data found", 1); //Attempt to read a string, if no string is found in the DB, return No data found
			_dateTime = mySqlRead.ReadDateTime(con, Serial, new DateTime(1), 1); //Attempt to read DateTime
			_ipaddress = mySqlRead.ReadIPAddress(con, Serial, new IPAddress(1), 1); //Attempt to read IPAddress
			Int = mySqlRead.ReadInt(con, Serial, 1, 1); //Attempt to read int
			_timespan = mySqlRead.ReadTimeSpan(con, Serial, new TimeSpan(1), 1); //Attempt to read timespan
			_deCimal = mySqlRead.ReadDecimal(con, Serial, (decimal)1.5, 1); //Read decimal
			_loNg = mySqlRead.ReadLong(con, Serial, (long)1.5, 1); //Read long
			_uiNt = mySqlRead.ReadUint(con, Serial, 1577, 1); //Read uint
			_shoRt = mySqlRead.ReadShort(con, Serial, 1577, 1); //Read short
			_ushoRt = mySqlRead.ReadUshort(con, Serial, 17, 1); //Read ushort
			_doubLe = mySqlRead.ReadDouble(con, Serial, 4098, 1); //Read double

			conData.CloseConnection(con); //Close connection after reading everything

		}

	}


}
