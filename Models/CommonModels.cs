using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SourceControl.Models
{
	public class CommonModels
	{
	}

	public class EncrypedRecord
	{
		public string RecordId { get; set; }
		public string RecordValue { get; set; }
	}

	public class MenuModel
	{
		public int MenuId { get; set; }
		public string MenuTitle { get; set; }
		public int MenuOrder { get; set; }
		public int ParentId { get; set; }
		public string PageFile { get; set; }
		public int MenuPageTemplateId { get; set; }
		public int HelpId { get; set; }
		public Nullable<bool> HasSubMenu { get; set; }
		public bool IsSystem { get; set; }
		public Nullable<bool> IsLastSubmenu { get; set; }
		public string TemplateName { get; set; }
		public int? UserId { get; set; }
	}

	public class LookupSchema
	{
		public string SelectColumn { get; set; }
		public string SelectColumns { get; set; }
		public string GridColumns { get; set; }
	}



	public class GridFilter
	{
		public string Operator { get; set; }
		public string Field { get; set; }
		public string Value { get; set; }
		public string Logic { get; set; }
		public List<GridFilter> Filters { get; set; }
	}

	public class GridFilters
	{
		public List<GridFilter> Filters { get; set; }
		public string Logic { get; set; }
	}

	public class GridSort
	{
		public string Field { get; set; }
		public string Dir { get; set; }
	}

	public class GridSchemaColumns
	{
		public string GridSchema { get; set; }
		public string GridColumns { get; set; }
		public string GridScripts { get; set; }



	}


	public class SysColumn
	{
		
		public string ColumnName { get; set; }
		public int ColumnOrder { get; set; }
		public int DataLength { get; set; }
		public bool IsIdentity { get; set; }
		public bool IsRequired { get; set; }
		public bool IsComputed { get; set; }
        public bool IsPrimary { get; set; }
        public string DataType { get; set; }
		public string DefaultValue { get; set; }
	}


	public class ColumnDisplay
	{
		public string ColumnName { get; set; }
		public string DisplayName { get; set; }
	}

	public class ValueText
	{
		public string ValueField { get; set; }
		public string TextField { get; set; }
	}

	public class DropDownModel
	{
		public string Text { get; set; }
		public string Value { get; set; }
		public string FilterKey { get; set; }
	}

	public class PageRoute
	{
		public int PageTemplateId { get; set; }
		public string PageType { get; set; }
		public string PrimaryKey { get; set; }
		public string TableName { get; set; }
		public string FilterClause { get; set; }
	}

}