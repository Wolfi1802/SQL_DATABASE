using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQL_DATABASE.Datenbanken
{
    public class ColumnModel
    {
        /// <summary>
        /// Name des PKs.
        /// </summary>
        public KeyValuePair<string, string> PrimaryKeyValue { set; get; }

        /// <summary>
        /// KEY = SpaltenName , VALUE = Value der Zeile der jeweiligen Spalte
        /// </summary>
        public Dictionary<string, string> RowItems { private set; get; } = new Dictionary<string, string>();
    }
}
