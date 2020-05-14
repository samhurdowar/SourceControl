using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F5PoolData.Models
{
    public class ParsePoolStats
    {
        public class url
        {
            public string resource = "mgmt/tm/ltm/pool/~Common~";

        }
        public class ActiveMemberCnt
        {
            public string value { get; set; }
        }

        public class ConnqAllAgeEdm
        {
            public string value { get; set; }
        }

        public class ConnqAllAgeEma
        {
            public string value { get; set; }
        }

        public class ConnqAllAgeHead
        {
            public string value { get; set; }
        }

        public class ConnqAllAgeMax
        {
            public string value { get; set; }
        }

        public class ConnqAllDepth
        {
            public string value { get; set; }
        }

        public class ConnqAllServiced
        {
            public string value { get; set; }
        }

        public class ConnqAgeEdm
        {
            public string value { get; set; }
        }

        public class ConnqAgeEma
        {
            public string value { get; set; }
        }

        public class ConnqAgeHead
        {
            public string value { get; set; }
        }

        public class ConnqAgeMax
        {
            public string value { get; set; }
        }

        public class ConnqDepth
        {
            public string value { get; set; }
        }

        public class ConnqServiced
        {
            public string value { get; set; }
        }

        public class CurSessions
        {
            public string value { get; set; }
        }

        public class MinActiveMembers
        {
            public string value { get; set; }
        }

        public class MonitorRule
        {
            public string description { get; set; }
        }

        public class TmName
        {
            public string description { get; set; }
        }

        public class ServersideBitsIn
        {
            public string value { get; set; }
        }

        public class ServersideBitsOut
        {
            public string value { get; set; }
        }

        public class ServersideCurConns
        {
            public string value { get; set; }
        }

        public class ServersideMaxConns
        {
            public string value { get; set; }
        }

        public class ServersidePktsIn
        {
            public string value { get; set; }
        }

        public class ServersidePktsOut
        {
            public string value { get; set; }
        }

        public class ServersideTotConns
        {
            public string value { get; set; }
        }

        public class StatusAvailabilityState
        {
            public string description { get; set; }
        }

        public class StatusEnabledState
        {
            public string description { get; set; }
        }

        public class StatusStatusReason
        {
            public string description { get; set; }
        }

        public class TotRequests
        {
            public string value { get; set; }
        }

        public class Entries2
        {
            public ActiveMemberCnt activeMemberCnt { get; set; }
            public ConnqAllAgeEdm connqAllageEdm { get; set; }
            public ConnqAllAgeEma connqAllageEma { get; set; }
            public ConnqAllAgeHead connqAllageHead { get; set; }
            public ConnqAllAgeMax connqAllageMax { get; set; }
            public ConnqAllDepth connqAlldepth { get; set; }
            public ConnqAllServiced connqAllserviced { get; set; }
            public ConnqAgeEdm connqageEdm { get; set; }
            public ConnqAgeEma connqageEma { get; set; }
            public ConnqAgeHead connqageHead { get; set; }
            public ConnqAgeMax connqageMax { get; set; }
            public ConnqDepth connqdepth { get; set; }
            public ConnqServiced connqserviced { get; set; }
            public CurSessions curSessions { get; set; }
            public MinActiveMembers minActiveMembers { get; set; }
            public MonitorRule monitorRule { get; set; }
            public TmName tmName { get; set; }
            public ServersideBitsIn serversidebitsIn { get; set; }
            public ServersideBitsOut serversidebitsOut { get; set; }
            public ServersideCurConns serversidecurConns { get; set; }
            public ServersideMaxConns serversidemaxConns { get; set; }
            public ServersidePktsIn serversidepktsIn { get; set; }
            public ServersidePktsOut serversidepktsOut { get; set; }
            public ServersideTotConns serversidetotConns { get; set; }
            public StatusAvailabilityState statusavailabilityState { get; set; }
            public StatusEnabledState statusenabledState { get; set; }
            public StatusStatusReason statusstatusReason { get; set; }
            public TotRequests totRequests { get; set; }
        }

        public class NestedStats
        {
            public string kind { get; set; }
            public string selfLink { get; set; }
            public Entries2 entries { get; set; }
        }

        public class STATS
        {
            public NestedStats nestedStats { get; set; }
        }

        public class Entries
        {
            public STATS STATS { get; set; }
        }

        public class PoolStatsRootObject
        {
            public string kind { get; set; }
            public string generation { get; set; }
            public string selfLink { get; set; }
            public Entries entries { get; set; }
        }
    }
}
