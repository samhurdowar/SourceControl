﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class AppNetworkCafeEntities : DbContext
    {
        public AppNetworkCafeEntities()
            : base("name=AppNetworkCafeEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<ACI> ACIs { get; set; }
        public virtual DbSet<API_Commands> API_Commands { get; set; }
        public virtual DbSet<ApplicationFeatureRequest> ApplicationFeatureRequests { get; set; }
        public virtual DbSet<ApplicationUpdate> ApplicationUpdates { get; set; }
        public virtual DbSet<ContactDB> ContactDBs { get; set; }
        public virtual DbSet<ContractV2> ContractV2 { get; set; }
        public virtual DbSet<CustomOption> CustomOptions { get; set; }
        public virtual DbSet<DeviceGroup> DeviceGroups { get; set; }
        public virtual DbSet<DeviceGroupMember> DeviceGroupMembers { get; set; }
        public virtual DbSet<DeviceGroupTask> DeviceGroupTasks { get; set; }
        public virtual DbSet<Device> Devices { get; set; }
        public virtual DbSet<eVPN> eVPNs { get; set; }
        public virtual DbSet<FWNat> FWNats { get; set; }
        public virtual DbSet<FWRoute> FWRoutes { get; set; }
        public virtual DbSet<HPNA_DEVICE> HPNA_DEVICE { get; set; }
        public virtual DbSet<HPNA_DEVICE_MODULE> HPNA_DEVICE_MODULE { get; set; }
        public virtual DbSet<HPNA_DEVICE_PORT> HPNA_DEVICE_PORT { get; set; }
        public virtual DbSet<HPNA_DEVICE_VLAN_INFO> HPNA_DEVICE_VLAN_INFO { get; set; }
        public virtual DbSet<InternalMailServer> InternalMailServers { get; set; }
        public virtual DbSet<IPNetwork> IPNetworks { get; set; }
        public virtual DbSet<LBPoolMember> LBPoolMembers { get; set; }
        public virtual DbSet<LBVIPRequest> LBVIPRequests { get; set; }
        public virtual DbSet<Link> Links { get; set; }
        public virtual DbSet<MasterDevice> MasterDevices { get; set; }
        public virtual DbSet<ReportList> ReportLists { get; set; }
        public virtual DbSet<Request> Requests { get; set; }
        public virtual DbSet<SiteMap> SiteMaps { get; set; }
        public virtual DbSet<SiteMapRole> SiteMapRoles { get; set; }
        public virtual DbSet<SiteSetting> SiteSettings { get; set; }
        public virtual DbSet<SMChanx> SMChanges { get; set; }
        public virtual DbSet<SMChangesTemp> SMChangesTemps { get; set; }
        public virtual DbSet<SMGroup> SMGroups { get; set; }
        public virtual DbSet<SMJobLog> SMJobLogs { get; set; }
        public virtual DbSet<SMTask> SMTasks { get; set; }
        public virtual DbSet<SMTasksTemp> SMTasksTemps { get; set; }
        public virtual DbSet<SMUserAssignment> SMUserAssignments { get; set; }
        public virtual DbSet<SwitchModule> SwitchModules { get; set; }
        public virtual DbSet<SwitchPort> SwitchPorts { get; set; }
        public virtual DbSet<UserActivityLog> UserActivityLogs { get; set; }
        public virtual DbSet<UserReport> UserReports { get; set; }
        public virtual DbSet<VMPhoneBook> VMPhoneBooks { get; set; }
        public virtual DbSet<DebugLog> DebugLogs { get; set; }
    }
}