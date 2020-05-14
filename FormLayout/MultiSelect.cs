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
		public string MultiSelect(ColumnDef columnDef)
		{
			if (columnDef.LookupTable.Length < 2)
			{
				return "Column definition not set properly";
			}

			var textFields = columnDef.TextField;
			if (columnDef.TextField.Contains(","))
			{
				textFields = "";
				string[] words = columnDef.TextField.Split(new char[] { ',' });
				foreach (string word in words)
				{
					textFields += "CAST(" + word + " AS varchar(250)) + ' ' + ";
				}

				textFields = textFields.Substring(0, textFields.Length - 9);
			}

			string json = DataService.GetJsonFromSQL("ValueField,TextField", columnDef.ValueField + "," + textFields, "FROM " + columnDef.LookupTable, "", false, columnDef.PageTemplateId);
			if (json.Length < 2)
			{
				json = "[];";
			}

			DocumentReady.AppendLine("var ds" + columnDef.ColumnDefId + " = " + json + ";");
			DocumentReady.AppendLine("");
			DocumentReady.AppendLine("$('#" + TableName + "_" + columnDef.ColumnName + "').kendoMultiSelect({");
			DocumentReady.AppendLine("	placeholder: 'Select " + columnDef.DisplayName + "',");
			DocumentReady.AppendLine("	dataValueField: 'ValueField',");
			DocumentReady.AppendLine("	dataTextField: 'TextField',");
			DocumentReady.AppendLine("	autoBind: true,");
			DocumentReady.AppendLine("	dataSource: ds" + columnDef.ColumnDefId + "");
			//DocumentReady.AppendLine("	dataBound: function (e) {");
			//DocumentReady.AppendLine("		var ids = $('#FormName input[id=UserIds_]').val();");
			//DocumentReady.AppendLine("		if (ids.length > 0) {");
			//DocumentReady.AppendLine("			var ray = ids.split(',');");
			//DocumentReady.AppendLine("			$('#UserIds').data('kendoMultiSelect').value(ray);");
			//DocumentReady.AppendLine("		}");
			//DocumentReady.AppendLine("	}");
			DocumentReady.AppendLine("});");


			StringBuilder sb = new StringBuilder();
			sb.AppendLine("<select id='" + TableName + "_" + columnDef.ColumnName + "' name='" + TableName + "_" + columnDef.ColumnName + "' multiple='multiple' style='width:" + columnDef.ElementWidth + "px'></select>");
			sb.AppendLine("");


			return sb.ToString();
		}



	}
}