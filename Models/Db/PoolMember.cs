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
    
    public partial class PoolMember
    {
        public System.Guid guid { get; set; }
        public System.Guid jobguid { get; set; }
        public string f5HostName { get; set; }
        public System.Guid PoolGuid { get; set; }
        public string kind { get; set; }
        public string Name { get; set; }
        public string partition { get; set; }
        public string fullpath { get; set; }
        public Nullable<long> generation { get; set; }
        public string selfLink { get; set; }
        public string address { get; set; }
        public Nullable<long> connectionLimit { get; set; }
        public Nullable<long> dynamicRatio { get; set; }
        public string ephemeral { get; set; }
        public Nullable<System.Guid> fqdnGuid { get; set; }
        public string logging { get; set; }
        public string monitor { get; set; }
        public string rateLimit { get; set; }
        public Nullable<long> ratio { get; set; }
        public string session { get; set; }
        public string state { get; set; }
    }
}
