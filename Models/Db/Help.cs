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
    
    public partial class Help
    {
        public int HelpId { get; set; }
        public string HelpTitle { get; set; }
        public string HelpContent { get; set; }
        public int HelpOrder { get; set; }
        public bool IsDoc { get; set; }
        public int ParentId { get; set; }
        public Nullable<bool> HasChildren { get; set; }
    }
}
