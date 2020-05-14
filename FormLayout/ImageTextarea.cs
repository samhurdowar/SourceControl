using SourceControl.Models.Db;
using SourceControl.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace SourceControl.FormLayoutClasses
{
	public partial class FormLayout
	{
		public string ImageTextarea(ColumnDef columnDef)
		{

			StringBuilder sb = new StringBuilder();

            int columnWidth = (columnDef.ElementWidth < 30) ? 500 : columnDef.ElementWidth;
            int columnHeight = (columnDef.ElementHeight < 10) ? 200 : columnDef.ElementHeight;

            sb.AppendLine("<textarea id='" + TableName + "_" + columnDef.ColumnName + "' name='" + TableName + "_" + columnDef.ColumnName + "' style='width:" + columnWidth + "px;height:" + columnHeight + "px;'></textarea>");

            DocumentReady.AppendLine("$('#" + TableName + "_" + columnDef.ColumnName + "').kendoEditor({");
            DocumentReady.AppendLine("      resizable: true,");
            DocumentReady.AppendLine("      tools: [ ]");
            DocumentReady.AppendLine("});");
            DocumentReady.AppendLine("$('.editorToolbarWindow').html('Paste image inside the textarea.'); ");


            return sb.ToString();
		}

	}
}