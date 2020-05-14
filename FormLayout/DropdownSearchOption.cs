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
		public string DropdownSearchOption(ColumnDef columnDef)
		{

			StringBuilder sb = new StringBuilder();

			sb.AppendLine("<input type='hidden' id='P" + PageTemplateId + "_" + columnDef.ColumnName + "' name='P" + PageTemplateId + "_" + columnDef.ColumnName + "' readonly style='width:25px;' />");
			sb.AppendLine("<input type='text' id='P" + PageTemplateId + "_" + columnDef.ColumnName + "_' name='P" + PageTemplateId + "_" + columnDef.ColumnName + "_' readonly style='width:" + columnDef.ElementWidth + "px;' />");

			//OpenLookupWindow
			sb.AppendLine("<a href='javascript:OpenLookupWindow(" + columnDef.PageTemplateId + "," + columnDef.ColumnDefId + ")' style='position:relative;text-decoration:none;top:7px;'><img src='/Images/ToolBar/x24/find.png' /></a>");

			sb.Append("<div id='LookupWindow_" + columnDef.ColumnDefId + "'></div>");

			DocumentReady.AppendLine("$('#LookupWindow_" + columnDef.ColumnDefId + "').kendoWindow({");
			DocumentReady.AppendLine("   height: '700px',");
			DocumentReady.AppendLine("   width: '700px',");
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