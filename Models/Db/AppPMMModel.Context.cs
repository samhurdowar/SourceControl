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
    
    public partial class AppPMMEntities : DbContext
    {
        public AppPMMEntities()
            : base("name=AppPMMEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<ADGroup> ADGroups { get; set; }
        public virtual DbSet<ADPermission> ADPermissions { get; set; }
        public virtual DbSet<ApplicationFeatureRequest> ApplicationFeatureRequests { get; set; }
        public virtual DbSet<ApplicationUpdate> ApplicationUpdates { get; set; }
        public virtual DbSet<AuthToken> AuthTokens { get; set; }
        public virtual DbSet<Link> Links { get; set; }
        public virtual DbSet<MasterDeviceList> MasterDeviceLists { get; set; }
        public virtual DbSet<PoolPermission> PoolPermissions { get; set; }
        public virtual DbSet<ReportList> ReportLists { get; set; }
        public virtual DbSet<RequestIP> RequestIPs { get; set; }
        public virtual DbSet<RequestLB> RequestLBs { get; set; }
        public virtual DbSet<RequestLBMember> RequestLBMembers { get; set; }
        public virtual DbSet<RequestProxy> RequestProxies { get; set; }
        public virtual DbSet<RequestProxyMember> RequestProxyMembers { get; set; }
        public virtual DbSet<Request> Requests { get; set; }
        public virtual DbSet<RequestSSL> RequestSSLs { get; set; }
        public virtual DbSet<SiteMap> SiteMaps { get; set; }
        public virtual DbSet<SiteSetting> SiteSettings { get; set; }
        public virtual DbSet<UserActivityLog> UserActivityLogs { get; set; }
        public virtual DbSet<UserChangeLog> UserChangeLogs { get; set; }
        public virtual DbSet<UserReport> UserReports { get; set; }
        public virtual DbSet<F5Pool> F5Pool { get; set; }
        public virtual DbSet<PoolMemberInfoForWeb> PoolMemberInfoForWebs { get; set; }
    }
}
