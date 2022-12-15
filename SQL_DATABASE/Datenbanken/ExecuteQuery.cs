using Dapper;
using Google.Protobuf.WellKnownTypes;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Utilities.Collections;
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
using System.Xml.Linq;

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


        public void UpdateOnePrimaryKey(DataTable table, string databaseName)
        {
            //TODO[TS] für jede id nen select auf id machen, wenn result != null dann update sonst insert!

            var tableModel = this.GetModel(table);

            foreach (var tableRow in tableModel.Rows)
            {
                var selectIdResult = this.GetSelectTableWherePk(tableModel.TableName, databaseName, tableRow.PrimaryKeyValue.Key, tableRow.PrimaryKeyValue.Value);

                if (selectIdResult != null && selectIdResult.Rows.Count >= 1)
                {
                    this.DoUpdateOneKey(tableRow, databaseName, tableModel.TableName);
                }
                else
                {
                    this.DoInsert(tableRow, databaseName, tableModel.TableName);
                }
            }
        }

        //TODO[TS] in helper auslagern
        private TableModel GetModel(DataTable table)
        {
            TableModel tableModel = new TableModel();
            List<ColumnModel> tableList = new List<ColumnModel>();

            var readableTable = this.ReadDataTable(table);

            foreach (var row in readableTable)
            {
                string primaryKey = table.PrimaryKey[0].ToString();

                ColumnModel columnModel = new ColumnModel();

                foreach (var lineItem in row)
                {
                    if (lineItem.Item1.Equals(primaryKey))
                        columnModel.PrimaryKeyValue = new KeyValuePair<string, string>(lineItem.Item1, lineItem.Item2);
                    else
                        columnModel.RowItems.Add(lineItem.Item1, lineItem.Item2);
                }

                tableList.Add(columnModel);
            }

            tableModel.TableName = table.TableName;
            tableModel.Rows.AddRange(tableList);


            return tableModel;
        }

        private void DoUpdateOneKey(ColumnModel column, string databaseName, string tableName)
        {
            string updateQuery = $"UPDATE {databaseName}.{tableName}";
            string Set = "SET ";
            string Where = $"WHERE {column.PrimaryKeyValue.Key}={column.PrimaryKeyValue.Value};";

            foreach (var item in column.RowItems)
            {

                Set += $" {item.Key}='{item.Value}',";
            }

            this.Execute($"{updateQuery}\n{Set.TrimEnd(',')}\n{Where}");
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

        #region Querys

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

        public DataTable GetSelectTableWherePk(string tableName, string databaseName, string pkName, string pkValue)
        {
            string queryString = $"{SELECT_ALL_} {databaseName}.{tableName} Where {pkName}={pkValue};";

            return this.Execute(queryString);
        }

        public bool DoInsert(ColumnModel column, string databaseName, string tableName)
        {
            string insertInto = $"INSERT INTO {databaseName}.{tableName} ( {column.PrimaryKeyValue.Key},";
            string values = $"VALUES ('{column.PrimaryKeyValue.Value}', ";

            foreach (var item in column.RowItems)
            {
                insertInto += $" {item.Key},";
                    values += $" '{item.Value}',";
            }
          
            return this.Execute($"{insertInto.TrimEnd(',')})\n{values.TrimEnd(',')});") != null;
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

        #endregion
    }
}
