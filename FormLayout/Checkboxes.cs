using SourceControl.Models;
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
		public string Checkboxes(ColumnDef columnDef)
		{

			string orderBy = "";
			if (columnDef.OrderField.Length > 3)
			{
				orderBy = " ORDER BY " + columnDef.OrderField;
			}

			int pageTemplateId = columnDef.PageTemplateId;
			string textField = "";
            var pageTemplate = SessionService.PageTemplate(pageTemplateId);
            var tableName = pageTemplate.TableName;
			if (columnDef.TextField.Contains(","))
			{

				var columns = columnDef.TextField.Split(new char[] { ',' });

				foreach (var columnName in columns)
				{
					var dataType = SessionService.DataType(pageTemplateId, columnName);

					if (dataType == "TEXT")
					{
						if (textField.Length == 0)
						{
							textField = columnName;
						}
						else
						{
							textField = textField + " + ' - ' + " + columnName;
						}
					}
					else
					{
						if (textField.Length == 0)
						{
							textField = "CAST(" + columnName + " AS varchar)";
						}
						else
						{
							textField = textField + " + ' - ' + " + "CAST(" + columnName + " AS varchar)";
						}
					}
				}
			}
			else
			{
				textField = columnDef.TextField;
			}

			string sql = "SELECT CAST(" + columnDef.ValueField + " AS varchar) AS ValueField, (" + textField + ") AS TextField FROM " + columnDef.LookupTable + " " + orderBy;


			StringBuilder sb = new StringBuilder();
			sb.AppendLine("<ul class='fieldlist'>");
			using (SourceControlEntities Db = new SourceControlEntities())
			{
				var recs = Db.Database.SqlQuery<ValueText>(sql).ToList();
				if (recs != null)
				{
					foreach (var rec in recs)
					{

						sb.AppendLine("<li style='white-space:nowrap;'>");
						sb.AppendLine("<input type='checkbox' id='" + TableName + "_" + columnDef.ColumnName + "' name='" + TableName + "_" + columnDef.ColumnName + "' class='site-checkbox' value='" + rec.ValueField + "'> " + rec.TextField);
						sb.AppendLine("</li>");
					}
				}

			}

			sb.AppendLine("</ul>");

			return sb.ToString();
		}

	}
}