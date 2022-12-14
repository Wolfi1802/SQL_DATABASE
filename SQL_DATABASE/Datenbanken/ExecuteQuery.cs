﻿using Dapper;
using MySql.Data.MySqlClient;
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

namespace SQL_DATABASE.Datenbanken
{
    public class ExecuteQuery
    {
        private const string SELECT_ALL_ = "SELECT * FROM ";
        private ExecuteQuery _instance;
        private MySqlConnection DbConnection = null;

        public ExecuteQuery(MySqlConnection connection)
        {
            try
            {
                Debug.WriteLine("Erstelle Db Instanz!");
                this.DbConnection = connection;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unerwarte Exception beim DB Stuff", ex);
            }

        }

        public List<List<(string, string)>> ReadDataTable(DataTable queryResult)
        {
            List<List<(string, string)>> results = new List<List<(string, string)>>();

            foreach (DataRow row in queryResult.Rows)
            {
                List<(string, string)> rowList = new List<(string, string)>();

                foreach (var column in queryResult.Columns)
                {
                    string value = row[column.ToString()].ToString();
                    rowList.Add((column.ToString(), value));
                }
                results.Add(rowList);
            }

            return results;
        }

        public List<DataTable> GetAllContentFromDatabase(string databaseName)
        {
            if (!string.IsNullOrEmpty(databaseName))
            {
                List<DataTable> listOftables = new List<DataTable>();

                string queryString = $"SELECT table_name " +
                    $"FROM information_schema.tables " +
                    $"WHERE table_type='BASE TABLE' " +
                    $"AND table_schema = '{databaseName}'";

                var result = this.Execute(queryString);


                foreach (DataRow row in result.Rows)
                {
                    string tableName = row.ItemArray[0].ToString();

                    var tableResult = this.GetSelectTable(tableName, databaseName);

                    if (tableResult != null)
                        listOftables.Add(tableResult);
                }

                return listOftables;
            }
            else
                return null;
        }

        public DataTable GetSelectTable(string tableName, string databaseName)
        {
            string queryString = $"{SELECT_ALL_} {databaseName}.{tableName};";

            return this.Execute(queryString);
        }

        public void UpdateOnePrimaryKey(DataTable table, string databaseName)
        {//pro spalte immer ein einzelnes update
            if (table.PrimaryKey.Length == 1)
            {
                string updateQuery = $"UPDATE {databaseName}.{table.TableName}";

                var readableTable = this.ReadDataTable(table);
                string primaryKey = table.PrimaryKey[0].ToString();

                foreach (var line in readableTable)
                {
                    string Set = "SET ";
                    string Where = "WHERE ";

                    foreach (var lineItem in line)
                    {
                        if (lineItem.Item1.Equals(primaryKey))
                            Where += $" {lineItem.Item1}={lineItem.Item2};";
                        else
                            Set += $" {lineItem.Item1}='{lineItem.Item2}',";
                    }

                    this.Execute($"{updateQuery}{Set.TrimEnd(',')}{Where}");
                }
            }
            else
                Debug.WriteLine($"Es wird aktuell nur eine Tabelle mit einem Pk unterstützt.\n {table.TableName} kann nicht geändert werden!");
        }


        private DataTable Execute(string queryString)
        {
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
                    Debug.WriteLine($"RIP execute query ist bruch \n{ex}");

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
