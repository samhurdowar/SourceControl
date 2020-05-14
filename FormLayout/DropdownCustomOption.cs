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
		public string DropdownCustomOption(ColumnDef columnDef)
		{
			StringBuilder sb = new StringBuilder();

			int columnWidth = (columnDef.ElementWidth < 30) ? 300 : columnDef.ElementWidth;
			sb.AppendLine("<input id='" + TableName + "_" + columnDef.ColumnName + "' name='" + TableName + "_" + columnDef.ColumnName + "' style='width:" + columnWidth + "px;' />");

			DocumentReady.AppendLine("$('#Form_" + columnDef.PageTemplateId + " input[id=" + TableName + "_" + columnDef.ColumnName + "] ').kendoDropDownList({");
			DocumentReady.AppendLine("   dataValueField: 'ValueField',");
			DocumentReady.AppendLine("   dataTextField: 'TextField',");
			DocumentReady.AppendLine("   dataSource: {");
			DocumentReady.AppendLine("      transport: {");
			DocumentReady.AppendLine("			read: {");
			DocumentReady.AppendLine("				dataType: 'json',");
			DocumentReady.AppendLine("				url: '/Page/GetCustomOptions?pageTemplateId=" + columnDef.PageTemplateId + "&columnDefId=" + columnDef.ColumnDefId + "',");
			DocumentReady.AppendLine("			}");
			DocumentReady.AppendLine("      }");
			DocumentReady.AppendLine("   }");
			DocumentReady.AppendLine("});");
			return sb.ToString();
		}




	}
}