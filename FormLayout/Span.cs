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

		public string Span(string tableName, ColumnDef columnDef)
		{
			var response = "<span id='" + tableName + "_" + columnDef.ColumnName + "' name='" + tableName + "_" + columnDef.ColumnName + "'></span>";
			return response;
		}

	}
}