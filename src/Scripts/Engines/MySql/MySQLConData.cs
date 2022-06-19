using MySql.Data.MySqlClient;

namespace Server.Mysql.Save
{
	public class MySqlConData
	{
		//Edit settings here
		public static readonly string server = Settings.Configuration.Get<string>("MySql", "Server", null);//"localhost";
		public static readonly string Database = Settings.Configuration.Get<string>("MySql", "Database", null);//"uomoons";
		public static readonly string Username = Settings.Configuration.Get<string>("MySql", "Username", null);//"root";
		public static readonly string Password = Settings.Configuration.Get<string>("MySql", "Password", null);//"password";
		//Do not edit anything below here unless you know what you're doing.


		public static readonly string db = "server=" + server + ";database=" + Database + ";uid=" + Username + ";password=" + Password + ";"; //Don't touch this line.

		public MySqlConnection OpenConnect(string dbConnectString)
		{
			MySqlConnection con = new(dbConnectString);
			con.Open();
			return con;
		}

		public MySqlConnection OpenConnect(string server, string database, string username, string password)
		{
			string connectionString = "server=" + server + ";database=" + database + ";uid=" + username + ";password=" + password + ";";
			MySqlConnection con = new(connectionString);
			con.Open();
			return con;
		}

		public void CloseConnection(MySqlConnection db)
		{
			db.Close();
		}
	}
}
