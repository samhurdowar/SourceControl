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
    
    public partial class ContractV2
    {
        public System.Guid guid { get; set; }
        public System.Guid deviceGuid { get; set; }
        public Nullable<System.DateTime> startDate { get; set; }
        public Nullable<System.DateTime> endDate { get; set; }
        public string contractNumber { get; set; }
        public string poNumber { get; set; }
        public string hostname { get; set; }
        public string serialNumber { get; set; }
        public string licenses { get; set; }
        public string notes { get; set; }
    }
}