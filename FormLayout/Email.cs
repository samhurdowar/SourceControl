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
		public string Email(ColumnDef columnDef)
		{

			StringBuilder sb = new StringBuilder();
			string columnName = columnDef.ColumnName;

			sb.Append("<input type='text' id='" + columnName + "' name='" + columnName + "' data-email-msg='Email is not valid' style = 'width:");

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