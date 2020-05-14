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
		public string HyperLink(ColumnDef columnDef)
		{

			StringBuilder sb = new StringBuilder();


			sb.Append("<input type='text' id='" + TableName + "_" + columnDef.ColumnName + "' name='" + TableName + "_" + columnDef.ColumnName + "' style='width:");

			sb.Append((columnDef.ElementWidth > 0) ? columnDef.ElementWidth + "px;'" : "300px;'");

			sb.Append(", maxlength='" + columnDef.DataLength + "'");

			string validationMsg = "";
			if (Convert.ToInt32(columnDef.IsRequired) == 1)
			{
				sb.Append(", required ");
				validationMsg = "Required";
			}


			if (validationMsg.Length > 0)
			{
				sb.Append(", validationMessage='" + validationMsg + "'");
			}

			sb.AppendLine(" />");
			return sb.ToString();
		}


	}
}