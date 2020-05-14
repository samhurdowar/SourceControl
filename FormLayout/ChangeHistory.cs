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
		public string ChangeHistory(ColumnDef columnDef)
		{

			StringBuilder sb = new StringBuilder();
			string columnName = columnDef.ColumnName;

			sb.AppendLine("<a href='javascript:OpenChangeHistoryWindow( " + columnDef.PageTemplateId + ")'>View " + columnDef.DisplayName + "</a>");
			sb.Append("<div id='ChangeHistoryWindow" + columnDef.PageTemplateId + "'></div>");

			DocumentReady.AppendLine("$('#ChangeHistoryWindow" + columnDef.PageTemplateId + "').kendoWindow({");
			DocumentReady.AppendLine("   height: '" + columnDef.ElementHeight + "px',");
			DocumentReady.AppendLine("   width: '" + columnDef.ElementWidth + "px',");
			DocumentReady.AppendLine("   modal: true,");
			DocumentReady.AppendLine("   scrollable: true,");
			DocumentReady.AppendLine("   title: '" + columnDef.DisplayName + "',");
			DocumentReady.AppendLine("   animation: {");
			DocumentReady.AppendLine("		open: {");
			DocumentReady.AppendLine("			duration: 100");
			DocumentReady.AppendLine("		}");
			DocumentReady.AppendLine("   },");
			DocumentReady.AppendLine("   visible: false,");
			DocumentReady.AppendLine("   actions: ['Maximize', 'Close']");
			DocumentReady.AppendLine("   ");
			DocumentReady.AppendLine("});");

			return sb.ToString();

		}

	}
}