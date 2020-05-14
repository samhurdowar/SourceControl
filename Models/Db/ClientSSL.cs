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
    
    public partial class ClientSSL
    {
        public System.Guid guid { get; set; }
        public Nullable<System.Guid> jobguid { get; set; }
        public string kind { get; set; }
        public string f5HostName { get; set; }
        public string name { get; set; }
        public string partition { get; set; }
        public string fullPath { get; set; }
        public Nullable<long> generation { get; set; }
        public string selfLink { get; set; }
        public string alertTimeout { get; set; }
        public string allowNonSsl { get; set; }
        public string authenticate { get; set; }
        public Nullable<long> authenticateDepth { get; set; }
        public Nullable<long> cacheSize { get; set; }
        public Nullable<long> cacheTimeout { get; set; }
        public string cert { get; set; }
        public string certExtensionIncludes { get; set; }
        public Nullable<long> certLifespan { get; set; }
        public string certLookupByIpaddrPort { get; set; }
        public string chain { get; set; }
        public string ciphers { get; set; }
        public string defaultsFrom { get; set; }
        public string forwardProxyBypassDefaultAction { get; set; }
        public string genericAlert { get; set; }
        public string handshakeTimeout { get; set; }
        public string inheritCertkeychain { get; set; }
        public string key { get; set; }
        public Nullable<long> maxRenegotiationsPerMinute { get; set; }
        public string modSslMethods { get; set; }
        public string mode { get; set; }
        public string tmOptions { get; set; }
        public string peerCertMode { get; set; }
        public string peerNoRenegotiateTimeout { get; set; }
        public string proxySsl { get; set; }
        public string proxySslPassthrough { get; set; }
        public string renegotiateMaxRecordDelay { get; set; }
        public string renegotiatePeriod { get; set; }
        public string renegotiateSize { get; set; }
        public string renegotiation { get; set; }
        public string retainCertificate { get; set; }
        public string secureRenegotiation { get; set; }
        public string sessionMirroring { get; set; }
        public string sessionTicket { get; set; }
        public string sniDefault { get; set; }
        public string sniRequire { get; set; }
        public string sslForwardProxy { get; set; }
        public string sslForwardProxyBypass { get; set; }
        public string sslSignHash { get; set; }
        public string strictResume { get; set; }
        public string uncleanShutdown { get; set; }
        public Nullable<System.Guid> certKeyChainGuid { get; set; }
    }
}
