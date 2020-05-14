using SourceControl.Models.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace SourceControl.FormLayoutClasses
{
	public partial class FormLayout
	{
		public string DisplayOnly(ColumnDef columnDef)
		{
			return "<span  id='" + TableName + "_" + columnDef.ColumnName + "_' name='" + TableName + "_" + columnDef.ColumnName + "_'></span>";
		}

	}
}