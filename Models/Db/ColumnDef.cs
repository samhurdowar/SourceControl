//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SourceControl.Models.Db
{
    using System;
    using System.Collections.Generic;
    
    public partial class ColumnDef
    {
        public int ColumnDefId { get; set; }
        public int PageTemplateId { get; set; }
        public string ColumnName { get; set; }
        public string OverideValue { get; set; }
        public string DisplayName { get; set; }
        public int ChildTemplateId { get; set; }
        public string LookupTable { get; set; }
        public string LookupFilter { get; set; }
        public string ValueField { get; set; }
        public string TextField { get; set; }
        public string OrderField { get; set; }
        public string ElementType { get; set; }
        public int ElementWidth { get; set; }
        public int ElementHeight { get; set; }
        public string ElementObject { get; set; }
        public string ElementDocReady { get; set; }
        public string ElementFunction { get; set; }
        public string ElementLabelLink { get; set; }
        public bool AddBlankOption { get; set; }
        public string DatePickerOption { get; set; }
        public int NumberMin { get; set; }
        public int NumberMax { get; set; }
        public int NumberOfDecimal { get; set; }
        public bool ShowInGrid { get; set; }
        public string GridWidth { get; set; }
        public bool IsMultiSelect { get; set; }
        public bool IsEncrypted { get; set; }
    }
}
