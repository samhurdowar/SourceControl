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
    
    public partial class SiteSetting
    {
        public System.Guid guid { get; set; }
        public string SettingName { get; set; }
        public string SettingValue { get; set; }
        public string SettingType { get; set; }
        public bool Password { get; set; }
        public string Notes { get; set; }
    }
}
