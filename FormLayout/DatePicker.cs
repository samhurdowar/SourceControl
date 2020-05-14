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
		public string DatePicker(ColumnDef columnDef)
		{
			StringBuilder sb = new StringBuilder();

            int columnWidth = (columnDef.ElementWidth < 10) ? 150 : columnDef.ElementWidth;
            sb.AppendLine("<input id='" + TableName + "_" + columnDef.ColumnName + "' name='" + TableName + "_" + columnDef.ColumnName + "' style='width:" + columnWidth + "px;' />");


            //sb.AppendLine("<div class='form-group'>");
            //sb.AppendLine("   <div class='input-group date' id='" + TableName + "_" + columnDef.ColumnName + "'>");
            //sb.AppendLine("       <input type='text' class='form-control' />");
            //sb.AppendLine("       <span class='input-group-addon'>");
            //sb.AppendLine("           <span class='glyphicon glyphicon-calendar'></span>");
            //sb.AppendLine("       </span>");
            //sb.AppendLine("   </div>");
            //sb.AppendLine("</div>");

            //DocumentReady.AppendLine("$(function () {");
            //DocumentReady.AppendLine("$('#" + TableName + "_" + columnDef.ColumnName + "').datetimepicker();");
            //DocumentReady.AppendLine("});");

            if (columnDef.DatePickerOption == "Date")
            {
                DocumentReady.AppendLine("$('#" + TableName + "_" + columnDef.ColumnName + "').kendoDatePicker({");
            }
            else if (columnDef.DatePickerOption == "DateTime")
            {
                DocumentReady.AppendLine("$('#" + TableName + "_" + columnDef.ColumnName + "').kendoDateTimePicker({");
            }
            else if (columnDef.DatePickerOption == "MonthYear")
            {
                DocumentReady.AppendLine("$('#" + TableName + "_" + columnDef.ColumnName + "').kendoDatePicker({");
                DocumentReady.AppendLine("start: 'year',");
                DocumentReady.AppendLine("depth: 'year',");
                DocumentReady.AppendLine("format: 'MMMM yyyy'");
            }
            else if (columnDef.DatePickerOption == "Year")
            {
                DocumentReady.AppendLine("$('#" + TableName + "_" + columnDef.ColumnName + "').kendoDatePicker({");
                DocumentReady.AppendLine("start: 'year',");
                DocumentReady.AppendLine("depth: 'year',");
                DocumentReady.AppendLine("format: 'yyyy'");
            } else
            {
                DocumentReady.AppendLine("$('#" + TableName + "_" + columnDef.ColumnName + "').kendoDatePicker({");
            }

            DocumentReady.AppendLine("});");
            return sb.ToString();
		}

	}
}
