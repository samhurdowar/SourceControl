using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SourceControl.Models.Db
{
    using System;
    using System.Collections.Generic;

    public partial class ColumnDef
    {
        public bool IsRequired { get; set; }
        public string DataType { get; set; }
        public int DataLength { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsIdentity { get; set; }
        public bool IsComputed { get; set; }
        public string DefaultValue { get; set; }
        public int ColumnOrder { get; set; }

    }
}


/*



     
*/
