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
    
    public partial class Menu
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
        public string PageType { get; set; }
        public string PageUrl { get; set; }
        public string PageTarget { get; set; }
    }
}
