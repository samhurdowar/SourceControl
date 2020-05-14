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

		public string Textarea(ColumnDef columnDef)
		{

			int columnWidth = (columnDef.ElementWidth < 30) ? 300 : columnDef.ElementWidth;
			int columnHeight = (columnDef.ElementHeight < 10) ? 200 : columnDef.ElementHeight;

			var response = "<textarea id='" + TableName + "_" + columnDef.ColumnName + "' name='" + TableName + "_" + columnDef.ColumnName + "' style='width:" + columnWidth + "px;height:" + columnHeight + "px;'></textarea>";

			return response;
		}

	}
}