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
    
    public partial class VIPStat
    {
        public System.Guid guid { get; set; }
        public System.Guid jobguid { get; set; }
        public System.Guid VIPGuid { get; set; }
        public string f5HostName { get; set; }
        public string VIPName { get; set; }
        public string ActualPvaAccel { get; set; }
        public string ClientsideBitsIn { get; set; }
        public string ClientsideBitsOut { get; set; }
        public string ClientsideCurConns { get; set; }
        public string ClientsideEvictedConns { get; set; }
        public string ClientsideMaxConns { get; set; }
        public string ClientsidePktsIn { get; set; }
        public string ClientsidePktsOut { get; set; }
        public string ClientsideSlowKilled { get; set; }
        public string ClientsideTotConns { get; set; }
        public string CmpEnableMode { get; set; }
        public string CmpEnabled { get; set; }
        public string CsMaxConnDur { get; set; }
        public string CsMeanConnDur { get; set; }
        public string CsMinConnDur { get; set; }
        public string Destination { get; set; }
        public string EphemeralBitsIn { get; set; }
        public string EphemeralBitsOut { get; set; }
        public string EphemeralCurConns { get; set; }
        public string EphemeralEvictedConns { get; set; }
        public string EphemeralMaxConns { get; set; }
        public string EphemeralPktsIn { get; set; }
        public string EphemeralPktsOut { get; set; }
        public string EphemeralSlowKilled { get; set; }
        public string EphemeralTotConns { get; set; }
        public string FiveMinAvgUsageRatio { get; set; }
        public string FiveSecAvgUsageRatio { get; set; }
        public string TmName { get; set; }
        public string OneMinAvgUsageRatio { get; set; }
        public string StatusAvailabilityState { get; set; }
        public string StatusEnabledState { get; set; }
        public string StatusStatusReason { get; set; }
        public string SyncookieStatus { get; set; }
        public string SyncookieAccepts { get; set; }
        public string SyncookieHwAccepts { get; set; }
        public string SyncookieHwSyncookies { get; set; }
        public string SyncookieHwsyncookieInstance { get; set; }
        public string SyncookieRejects { get; set; }
        public string SyncookieSwsyncookieInstance { get; set; }
        public string SyncookieSyncacheCurr { get; set; }
        public string SyncookieSyncacheOver { get; set; }
        public string SyncookieSyncookies { get; set; }
        public string TotRequests { get; set; }
    }
}
