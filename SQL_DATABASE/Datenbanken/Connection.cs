using MySql.Data.MySqlClient;

namespace SQL_DATABASE.Datenbanken
{
    public class Connection
    {

        public Connection(string name, MySqlConnection myconnection)
        {
            this.Name = name;
            this.MyConnection= myconnection;
        }

        public string Name { set; get; }

        public MySqlConnection MyConnection { set; get; }
    }
}
