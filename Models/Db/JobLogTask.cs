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
    
    public partial class JobLogTask
    {
        public System.Guid guid { get; set; }
        public string JobTaskName { get; set; }
        public string Device { get; set; }
        public string DeviceTask { get; set; }
        public Nullable<System.DateTime> StartTime { get; set; }
        public Nullable<System.DateTime> EndTime { get; set; }
        public bool Successful { get; set; }
        public System.Guid ParentJobGuid { get; set; }
    }
}
