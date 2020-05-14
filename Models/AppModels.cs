using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SourceControl.Models.Db
{


	public partial class PoolMember
	{
		public string IP { get; set; }
		public string routeDomain { get; set; }
	}

    public partial class Pool
    {
        public List<PoolMember> poolMemberList { get; set; }
    }

    public partial class VIP
	{
		public VIPStat vipStatus { get; set; }
		public Pool poolInfo { get; set; }
		public List<Irule> iRuleList { get; set; }
		public List<ClientSSL> sslList { get; set; }
		public string IP { get; set; }
		public string port { get; set; }
		public string routeDomain { get; set; }

	}


	public partial class SwitchPort
	{
		public string DeviceType { get; set; }
		public string DeviceLocation { get; set; }
	}


	public class ContractData
	{
		public Guid guid { get; set; }
		public Guid deviceGuid { get; set; }
		public DateTime? startDate { get; set; }
		public DateTime? endDate { get; set; }
		public string contractNumber { get; set; }
		public string poNumber { get; set; }
		public string HostName { get; set; }
		public string Site { get; set; }
		public string DeviceModel { get; set; }
		public string DeviceVendor { get; set; }
		public string DeviceLocation { get; set; }
		public string SerialNumber { get; set; }
		public string licenses { get; set; }
		public string notes { get; set; }

	}


	public partial class DeviceGroupMember
	{
		public int ReservedPortCount { get; set; }
		public int OpenPortCount { get; set; }
		public int TotalPortCount { get; set; }
		public int PercentUsed { get; set; }

		public string Site { get; set; }
		public string DeviceLocation { get; set; }
		public MasterDevice MasterDeviceInfo { get; set; }

	}

	public class FwEnvironment
	{
		public string Environment { get; set; }
		public List<FWRoutes> networks { get; set; }
	}

	public class FWRoutes
	{
		public Guid guid { get; set; }
		public string Network { get; set; }
		public string CMSFirewallIP { get; set; }
		public string CMSFirewall { get; set; }
		public string InternetFirewallIP { get; set; }
		public string InternetFirewall { get; set; }
		public string Notes { get; set; }
	}


	public class PortUpdate
	{
		public string updateType { get; set; }
		public string switchName { get; set; }
		public int startBlade { get; set; }
		public int startPort { get; set; }
		public int endBlade { get; set; }
		public int endPort { get; set; }
		public string portType { get; set; }
		public string description { get; set; }
		public string vlan { get; set; }
		public string prefix { get; set; }

	}

	public class f5SearchResults
	{
		public string f5Name { get; set; }
		public string VIPNAME { get; set; }
		public string VIPIP { get; set; }
		public string PoolName { get; set; }
		public string NodeName { get; set; }
		public string NodeIP { get; set; }
	}


	public partial class DeviceGroup
	{
		public int OpenPortCount { get; set; }
		public int ReserverPortCount { get; set; }
		public int TotalPortCount { get; set; }
		public int PercentUsed { get; set; }
		public List<DeviceGroupMember> DeviceList { get; set; }
		public int DeviceCount { get; set; }
		public List<DeviceGroup> SubGroup { get; set; }
	}

	[MetadataType(typeof(MasterDevice.Metadata))]
	public partial class MasterDevice
	{
		public int ReservedPortCount { get; set; }
		public int OpenPortCount { get; set; }
		public int TotalPortCount { get; set; }
		public int PercentUsed { get; set; }

		class Metadata
		{
			[StringLength(3, MinimumLength = 1)]
			public string Site { get; set; }
			public string DeviceLocation { get; set; }

			public string HostName { get; set; }

			public string DeviceIP { get; set; }
		}

	}


    public class RootPoolObject
    {
        public string kind { get; set; }
        public string selfLink { get; set; }
        public Pool[] items { get; set; }
    }

    public class ActiveDevice
    {
        public string DeviceName { get; set; }
    }
}