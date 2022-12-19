using Dapper;
using Google.Protobuf.WellKnownTypes;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
using Org.BouncyCastle.Utilities.Collections;
using SQL_DATABASE.Datenbanken.Helper;
using SQL_DATABASE.Datenbanken.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media.Animation;
using System.Xml.Linq;

namespace SQL_DATABASE.Datenbanken
{
    public class ExecuteQuery
    {
        private const string SELECT_ALL_ = "SELECT * FROM ";
        private const string SELECT_ALL_TABLES = "SELECT table_name " +
                    "FROM information_schema.tables " +
                    "WHERE table_type='BASE TABLE' " +
                    "AND table_schema =";

        private const string CREATE_TABLE_PERSON = "CREATE TABLE Persons (PersonID int NOT NULL, LastName varchar(255) NOT NULL, FirstName varchar(255), Age int, CONSTRAINT PK_Person PRIMARY KEY (PersonID,LastName));";

        private const string CREATE_TABLE_ORDERS_FK_PERSON = "CREATE TABLE Orders (OrderID int NOT NULL,OrderNumber int NOT NULL,PersonID int,PRIMARY KEY (OrderID),CONSTRAINT FK_PersonOrder FOREIGN KEY (PersonID)REFERENCES Persons(PersonID));";

        private ExecuteQuery _instance;
        private MySqlConnection DbConnection = null;
        private DatabaseConverter databaseConverterHelper;

        public ExecuteQuery(MySqlConnection connection)
        {
            try
            {
                Debug.WriteLine("Erstelle Db Instanz!");
                this.DbConnection = connection;
                this.databaseConverterHelper = new DatabaseConverter();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unerwarte Exception beim DB Stuff", ex);
            }
        }

        public List<DataTable> GetAllContentFromDatabase(string databaseName)
        {
            if (string.IsNullOrEmpty(databaseName))
                return null;

            List<DataTable> listOfTables = new List<DataTable>();
            DataTable allTableQueryResult = this.DoSelectAllTableNames(databaseName);

            if (allTableQueryResult != null)
            {
                foreach (DataRow row in allTableQueryResult.Rows)
                {
                    DataTable tableResult = this.DoSelectTable(row.ItemArray[0].ToString(), databaseName);

                    if (tableResult != null)
                        listOfTables.Add(tableResult);
                }

                return listOfTables;
            }
            else
                return null;
        }

        public void CreateTestTables()
        {
            this.Execute(CREATE_TABLE_PERSON);
            this.Execute(CREATE_TABLE_ORDERS_FK_PERSON);
        }

        public bool? Update(DataTable table, string databaseName)
        {
            if (table == null || string.IsNullOrEmpty(databaseName))
                return null;

            TableModel tableModel = this.databaseConverterHelper.Convert(table);

            if (tableModel != null)
            {
                try
                {
                    foreach (ColumnModel tableRow in tableModel.Rows)
                    {
                        this.Update(tableRow, databaseName, tableModel.TableName);
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{nameof(ExecuteQuery)},{nameof(Update)},{ex.Message}");
                    return false;
                }
            }
            else
                return null;
        }

        public DataTable Update(ColumnModel tableRow, string databaseName, string tableName)
        {
            if (tableRow == null || string.IsNullOrEmpty(databaseName) || string.IsNullOrEmpty(tableName))
                return null;

            DataTable selectIdResult = this.DoSelectTableWherePK(tableName, databaseName, tableRow.PrimaryKeyValue.Key,
                tableRow.PrimaryKeyValue.Value);

            if (selectIdResult != null && selectIdResult.Rows.Count >= 1)
                return this.DoUpdate(tableRow, databaseName, tableName);
            else
                return this.DoInsert(tableRow, databaseName, tableName);
        }

        public DataTable DoSelectTable(string tableName, string databaseName)
        {
            if (string.IsNullOrEmpty(tableName) || string.IsNullOrEmpty(databaseName))
                return null;

            string queryString = $"{SELECT_ALL_} {databaseName}.{tableName};";

            return this.Execute(queryString);
        }

        public DataTable DoSelectAllTableNames(string databaseName)
        {
            if (string.IsNullOrEmpty(databaseName))
                return null;

            return this.Execute($"{SELECT_ALL_TABLES} '{databaseName}'");
        }

        public DataTable DoSelectTableWherePK(string tableName, string databaseName, string pkName, string pkValue)
        {
            if (string.IsNullOrEmpty(tableName) || string.IsNullOrEmpty(databaseName) ||
                string.IsNullOrEmpty(pkName) || string.IsNullOrEmpty(pkValue))
                return null;

            string queryString = $"{SELECT_ALL_} {databaseName}.{tableName} Where {pkName}={pkValue};";

            return this.Execute(queryString);
        }

        public DataTable DoInsert(ColumnModel column, string databaseName, string tableName)
        {
            if (column == null || string.IsNullOrEmpty(tableName) || string.IsNullOrEmpty(databaseName))
                return null;

            string insertInto = $"INSERT INTO {databaseName}.{tableName} ( {column.PrimaryKeyValue.Key},";
            string values = $"VALUES ('{column.PrimaryKeyValue.Value}', ";

            foreach (var item in column.RowItems)
            {
                insertInto += $" {item.Key},";
                values += $" '{item.Value}',";
            }

            return this.Execute($"{insertInto.TrimEnd(',')})\n{values.TrimEnd(',')});");
        }

        public DataTable DoUpdate(ColumnModel column, string databaseName, string tableName)
        {
            if (column == null || string.IsNullOrEmpty(tableName) || string.IsNullOrEmpty(databaseName))
                return null;

            string updateQuery = $"UPDATE {databaseName}.{tableName}";
            string Set = "SET ";
            string Where = $"WHERE {column.PrimaryKeyValue.Key}={column.PrimaryKeyValue.Value};";

            foreach (var item in column.RowItems)
            {
                Set += $" {item.Key}='{item.Value}',";
            }

            return this.Execute($"{updateQuery}\n{Set.TrimEnd(',')}\n{Where}");
        }

        private DataTable Execute(string queryString)
        {
            if (string.IsNullOrEmpty(queryString))
                return null;

            using (MySqlConnection connection = this.DbConnection)
            {
                try
                {
                    connection.Open();
                    Debug.WriteLine($"{DateTime.Now} connection wurde geöffnet!");
                    Debug.WriteLine($"Benutze Query [{queryString}]");

                    this.DbConnection.Query<string>(queryString).FirstOrDefault();

                    MySqlCommand command = new MySqlCommand(queryString, connection);
                    MySqlDataReader reader = command.ExecuteReader();
                    DataTable actualldata = new DataTable();

                    actualldata.Load(reader);

                    return actualldata;
                }
                catch (MySqlException ex)
                {
                    Debug.WriteLine($"RIP execute query ist bruch \n{ex.Message}\n");

                    return null;
                }
                finally
                {
                    connection.Close();
                    Debug.WriteLine($"{DateTime.Now} connection wurde geschlossen!");
                }
            }
        }

    }
}
