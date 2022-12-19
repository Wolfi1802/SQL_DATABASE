using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQL_DATABASE.Datenbanken.Helper;
using System;
using System.Data;
using System.Diagnostics;

namespace Tests
{
    [TestClass]
    public class DataBaseConverterTests
    {
        #region Convert 

        [TestMethod]
        public void Convert_Null()
        {
            var instance = new DatabaseConverter();
            var result = instance.Convert(null);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void Convert_EmptyObject()
        {
            var instance = new DatabaseConverter();
            var table = new DataTable();
            var result = instance.Convert(table);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Convert_EmptyObject_RowCount()
        {
            var instance = new DatabaseConverter();
            var table = new DataTable();
            var result = instance.Convert(table);

            Assert.AreEqual(table.Rows.Count, result.Rows.Count);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Convert_EmptyObject_TableName()
        {
            var instance = new DatabaseConverter();
            var table = new DataTable();
            var result = instance.Convert(table);

            Assert.AreEqual(table.TableName, result.TableName);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Convert_TableName()
        {
            var instance = new DatabaseConverter();
            var table = new DataTable();
            table.TableName= "Test";
            var result = instance.Convert(table);

            Assert.AreEqual(table.TableName, result.TableName);
        }

        #endregion

        #region ConvertIntoReadable

        [TestMethod]
        public void ConvertIntoReadable_Null()
        {
            var instance = new DatabaseConverter();
            var result = instance.ConvertIntoReadable(null);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void ConvertIntoReadable_EmptyObject()
        {
            var instance = new DatabaseConverter();
            var table = new DataTable();
            var result = instance.ConvertIntoReadable(table);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ConvertIntoReadable_EmptyObject_RowCount()
        {
            var instance = new DatabaseConverter();
            var table = new DataTable();
            var result = instance.ConvertIntoReadable(table);

            Assert.AreEqual(table.Rows.Count, result.Count);
            Assert.IsNotNull(result);
        }

        #endregion
    }
}
