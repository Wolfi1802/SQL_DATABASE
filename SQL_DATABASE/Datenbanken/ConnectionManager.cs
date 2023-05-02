using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

namespace SQL_DATABASE.Datenbanken
{
    public class ConnectionManager
    {
        private Dictionary<string, MySqlConnection> connections = new Dictionary<string, MySqlConnection>();

        private static ConnectionManager _instance;
        public static ConnectionManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ConnectionManager();

                return _instance;
            }
        }

        private ConnectionManager()
        {
            MySqlConnection TimosTest = new MySqlConnection("server=localhost;user id=root;password=;database=TimosTest;");
            MySqlConnection Customer = new MySqlConnection("server=localhost;user id=root;password=;database=Customer;");
            MySqlConnection referencetooldb = new MySqlConnection("server=localhost;user id=root;password=;database=referencetooldb;");
            MySqlConnection CreateTest = new MySqlConnection("server=localhost;user id=root;password=;database=CreateTest;");
            MySqlConnection test = new MySqlConnection("server=localhost;user id=root;password=;database=test;");
            MySqlConnection weatherStation = new MySqlConnection("server=localhost;user id=root;password=;database=wetterstation;");

            this.connections.Add("wetterstation", weatherStation);
            this.connections.Add("TimosTest", TimosTest);
            this.connections.Add("Customer", Customer);
            this.connections.Add("referencetooldb", referencetooldb);
            this.connections.Add("CreateTest", CreateTest);
            this.connections.Add("test", test);

        }

        public MySqlConnection GetConnection(string dataBaseName)
        {
            foreach (var item in this.connections)
            {
                if (item.Key.Equals(dataBaseName))
                    return item.Value;
            }

            return null;
        }

        public Dictionary<string, MySqlConnection> GetAllConnections()
        {
            return this.connections;
        }

        public void AddConnection(string server, string userId, string database, string password = "")
        {
            if (string.IsNullOrEmpty(server))
                throw new NullReferenceException($" Hinzufügen nicht möglich weil [{server}] == null");
            if (string.IsNullOrEmpty(userId))
                throw new NullReferenceException($" Hinzufügen nicht möglich weil [{userId}] == null");
            if (string.IsNullOrEmpty(database))
                throw new NullReferenceException($" Hinzufügen nicht möglich weil [{database}] == null");

            if (!this.connections.ContainsKey(database))
                this.connections.Add(database, new MySqlConnection($"server={server};user id={userId};password={password};database={database};"));
            else
                Debug.WriteLine($"Es wurde versucht eine Verbindung für Datenbank [{database}] einzugehen, diese ist jedoch schon vorhanden.");
        }
    }
}
