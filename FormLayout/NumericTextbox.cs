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
		public string NumericTextbox(ColumnDef columnDef)
		{
			StringBuilder sb = new StringBuilder();

			int columnWidth = (columnDef.ElementWidth < 30) ? 300 : columnDef.ElementWidth;
			sb.AppendLine("<input id='" + TableName + "_" + columnDef.ColumnName + "' name='" + TableName + "_" + columnDef.ColumnName + "'" + ((columnDef.NumberMin > 0) ? " min='" + columnDef.NumberMin + "'" : "") + " " + ((columnDef.NumberMax > 0) ? " max='" + columnDef.NumberMax + "'" : "") + "   style='width:" + columnWidth + "px;' />");

			DocumentReady.AppendLine("$('#" + columnDef.ColumnName + "').kendoNumericTextBox({");
			DocumentReady.AppendLine("	format: '#',");
			DocumentReady.AppendLine("	decimals: " + columnDef.NumberOfDecimal);
			DocumentReady.AppendLine("});");

			return sb.ToString();
		}

	}
}