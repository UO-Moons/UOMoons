using MySql.Data.MySqlClient;

namespace Server.Mysql.Save
{
	public class MySQLConData
	{
		//Edit settings here
		public static readonly string server = Settings.Configuration.Get<string>("MySql", "Server", null);//"localhost";
		public static readonly string database = Settings.Configuration.Get<string>("MySql", "Database", null);//"uomoons";
		public static readonly string username = Settings.Configuration.Get<string>("MySql", "Username", null);//"root";
		public static readonly string password = Settings.Configuration.Get<string>("MySql", "Password", null);//"password";
		//Do not edit anything below here unless you know what you're doing.


		public static readonly string db = "server=" + server + ";database=" + database + ";uid=" + username + ";password=" + password + ";"; //Don't touch this line.

		public MySqlConnection OpenConnect(string DB_Connect_String)
		{
			MySqlConnection con = new(DB_Connect_String);
			con.Open();
			return con;
		}

		public MySqlConnection OpenConnect(string server, string database, string username, string password)
		{
			string db = "server=" + server + ";database=" + database + ";uid=" + username + ";password=" + password + ";";
			MySqlConnection con = new(db);
			con.Open();
			return con;
		}

		public void CloseConnection(MySqlConnection db)
		{
			db.Close();
		}
	}
}
