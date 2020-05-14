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
    
    public partial class Pool
    {
        public System.Guid guid { get; set; }
        public Nullable<System.Guid> jobguid { get; set; }
        public string f5HostName { get; set; }
        public string kind { get; set; }
        public string name { get; set; }
        public string partition { get; set; }
        public string fullpath { get; set; }
        public Nullable<long> generation { get; set; }
        public string selfLink { get; set; }
        public string AllowNat { get; set; }
        public string AllowSnat { get; set; }
        public string ignorePersistedWeight { get; set; }
        public string ipTosToClient { get; set; }
        public string ipTosToServer { get; set; }
        public string linkQosToClient { get; set; }
        public string linkQosToServer { get; set; }
        public string loadBalancingMode { get; set; }
        public Nullable<long> minActiveMembers { get; set; }
        public Nullable<long> minUpMembers { get; set; }
        public string minUpMembersAction { get; set; }
        public string minUpMembersChecking { get; set; }
        public string monitor { get; set; }
        public Nullable<long> queueDepthLimit { get; set; }
        public string queueOnConnectionLimit { get; set; }
        public Nullable<long> queueTimeLimit { get; set; }
        public Nullable<long> reselectTries { get; set; }
        public string serviceDownAction { get; set; }
        public Nullable<long> slowRampTime { get; set; }
        public Nullable<System.Guid> membersReferenceGuid { get; set; }
    }
}
