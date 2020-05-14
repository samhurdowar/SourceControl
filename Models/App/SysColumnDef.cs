using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SourceControl.Models.App
{
    public class SysColumnDef
    {
        public string ColumnName { get; set; }
        public int ColumnOrder { get; set; }
        public int DataLength { get; set; }
        public bool IsIdentity { get; set; }
        public bool IsRequired { get; set; }
        public bool IsComputed { get; set; }

        public string DefaultValue { get; set; }
        public string DataType { get; set; }
        public bool IsPrimaryKey { get; set; }
    }

    public class SysTable
    {
        public int DbEntityId { get; set; }
        public string TableName { get; set; }
        public string PrimaryKey { get; set; }
        public string PrimaryKeyType { get; set; }
    }
}