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
    
    public partial class PoolPermission
    {
        public System.Guid guid { get; set; }
        public string username { get; set; }
        public string deviceName { get; set; }
        public string poolName { get; set; }
        public bool enabled { get; set; }
        public System.DateTime dateGranted { get; set; }
        public string grantedBy { get; set; }
    }
}
