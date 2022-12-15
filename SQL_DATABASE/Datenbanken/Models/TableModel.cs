using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQL_DATABASE.Datenbanken.Models
{
    public class TableModel
    {
        public string TableName { set; get; }

        public List<ColumnModel> Rows{private set;get;} = new List<ColumnModel>();
    }
}
