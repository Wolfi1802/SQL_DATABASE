using MySqlX.XDevAPI.Common;
using SQL_DATABASE.Datenbanken.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQL_DATABASE.Datenbanken.Helper
{
    public class DatabaseConverter
    {
        public TableModel Convert(DataTable table)
        {
            TableModel tableModel = new TableModel();
            List<ColumnModel> tableList = new List<ColumnModel>();

            List<List<(string, string)>> readableTable = this.ConvertIntoReadable(table);

            foreach (List<(string, string)> row in readableTable)
            {
                string primaryKey = table.PrimaryKey[0].ToString();

                ColumnModel columnModel = new ColumnModel();

                foreach ((string,string) lineItem in row)
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

        public List<List<(string, string)>> ConvertIntoReadable(DataTable queryResult)
        {
            List<List<(string, string)>> results = new List<List<(string, string)>>();

            foreach (DataRow row in queryResult.Rows)
            {
                List<(string, string)> rowList = new List<(string, string)>();

                foreach (DataColumnCollection column in queryResult.Columns)
                {
                    string value = row[column.ToString()].ToString();
                    rowList.Add((column.ToString(), value));
                }

                results.Add(rowList);
            }

            return results;
        }
    }
}
