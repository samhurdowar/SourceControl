﻿using SourceControl.Models.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace SourceControl.FormLayoutClasses
{
	public partial class FormLayout
	{

		public string CheckboxYesNo(ColumnDef columnDef)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("<div class='pretty p-default'><input type='checkbox' id='" + TableName + "_" + columnDef.ColumnName + "' name='" + TableName + "_" + columnDef.ColumnName + "' value='true'><div class='state p-primary'><label> </label></div></div>");
			return sb.ToString();
		}

	}
}