using SourceControl.Models;
using SourceControl.Models.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace NetworkToolbox.Controllers
{
	public class F5Controller : Controller
	{
		public string GetJobs()
		{
			using (TargetEntities Db = new TargetEntities("F5DataEntities"))
			{
				var recs = Db.Database.SqlQuery<JobLog>("SELECT * FROM JobLog ORDER BY endTime DESC").ToList();
				var json = new JavaScriptSerializer().Serialize(recs);
				return json;
			}

		}

		public string GetVIPName(string jobGuid, string searchString)
		{
			using (TargetEntities Db = new TargetEntities("F5DataEntities"))
			{
				var results = Db.Database.SqlQuery<f5SearchResults>("SELECT DeviceData.name AS F5Name, VIP.name AS VIPNAME, REPLACE(VIP.destination,'/Common/','') AS VIPIP, Pool.name AS PoolName, PoolMember.Name AS NodeName, PoolMember.address AS NodeIP " +
					"FROM DeviceData INNER JOIN " +
					"JobLog ON DeviceData.jobguid = JobLog.guid INNER JOIN " +
					"VIP ON DeviceData.jobguid = VIP.jobguid AND DeviceData.hostname = VIP.f5HostName INNER JOIN " +
					"Pool ON VIP.jobguid = Pool.jobguid AND VIP.f5HostName = Pool.f5HostName AND VIP.pool = Pool.fullpath INNER JOIN " +
					"PoolMember ON Pool.guid = PoolMember.PoolGuid " +
					"WHERE(DeviceData.failoverState = N'active') " +
					"AND(DeviceData.selfDevice = N'true') " +
					"AND(JobLog.guid = '" + jobGuid + "' " +
					"And(VIP.name LIKE '%" + searchString + "%') )").ToList();

				var json = new JavaScriptSerializer().Serialize(results);
				return json;
			}
		}



		public string GetVIPIP(string jobGuid, string searchString)
		{

			List<f5SearchResults> results = new List<f5SearchResults>();
			using (TargetEntities Db = new TargetEntities("F5DataEntities"))
			{

				results = Db.Database.SqlQuery<f5SearchResults>("SELECT DeviceData.name AS F5Name, VIP.name AS VIPNAME, REPLACE(VIP.destination,'/Common/','') AS VIPIP, Pool.name AS PoolName, PoolMember.Name AS NodeName, " +
					"PoolMember.address AS NodeIP " +
					"FROM DeviceData INNER JOIN " +
					"JobLog ON DeviceData.jobguid = JobLog.guid INNER JOIN " +
					"VIP ON DeviceData.jobguid = VIP.jobguid AND DeviceData.hostname = VIP.f5HostName INNER JOIN " +
					"Pool ON VIP.jobguid = Pool.jobguid AND VIP.f5HostName = Pool.f5HostName AND VIP.pool = Pool.fullpath INNER JOIN " +
					"PoolMember ON Pool.guid = PoolMember.PoolGuid " +
					"WHERE(DeviceData.failoverState = N'active') " +
					"AND(DeviceData.selfDevice = N'true') " +
					"AND(JobLog.guid = '" + jobGuid + "' " +
					"And(VIP.destination LIKE '%" + searchString + "%') )").ToList();

				var json = new JavaScriptSerializer().Serialize(results);
				return json;
			}
		}



		public string GetPoolName(string jobGuid, string searchString)
		{

			List<f5SearchResults> results = new List<f5SearchResults>();
			using (TargetEntities Db = new TargetEntities("F5DataEntities"))
			{

				results = Db.Database.SqlQuery<f5SearchResults>("SELECT DeviceData.name AS F5Name, VIP.name AS VIPNAME, VIP.destination AS VIPIP, Pool.name AS PoolName, PoolMember.Name AS NodeName, " +
					"PoolMember.address AS NodeIP " +
					"FROM DeviceData INNER JOIN " +
					"JobLog ON DeviceData.jobguid = JobLog.guid INNER JOIN " +
					"VIP ON DeviceData.jobguid = VIP.jobguid AND DeviceData.hostname = VIP.f5HostName INNER JOIN " +
					"Pool ON VIP.jobguid = Pool.jobguid AND VIP.f5HostName = Pool.f5HostName AND VIP.pool = Pool.fullpath INNER JOIN " +
					"PoolMember ON Pool.guid = PoolMember.PoolGuid " +
					"WHERE(DeviceData.failoverState = N'active') " +
					"AND(DeviceData.selfDevice = N'true') " +
					"AND(JobLog.guid = '" + jobGuid + "' " +
					"And(Pool.name LIKE '%" + searchString + "%') )").ToList();

				var json = new JavaScriptSerializer().Serialize(results);
				return json;
			}
		}


		public string GetNodeName(string jobGuid, string searchString)
		{

			List<f5SearchResults> results = new List<f5SearchResults>();
			using (TargetEntities Db = new TargetEntities("F5DataEntities"))
			{

				results = Db.Database.SqlQuery<f5SearchResults>("SELECT DeviceData.name AS F5Name, VIP.name AS VIPNAME, VIP.destination AS VIPIP, Pool.name AS PoolName, PoolMember.Name AS NodeName, " +
					"PoolMember.address AS NodeIP " +
					"FROM DeviceData INNER JOIN " +
					"JobLog ON DeviceData.jobguid = JobLog.guid INNER JOIN " +
					"VIP ON DeviceData.jobguid = VIP.jobguid AND DeviceData.hostname = VIP.f5HostName INNER JOIN " +
					"Pool ON VIP.jobguid = Pool.jobguid AND VIP.f5HostName = Pool.f5HostName AND VIP.pool = Pool.fullpath INNER JOIN " +
					"PoolMember ON Pool.guid = PoolMember.PoolGuid " +
					"WHERE(DeviceData.failoverState = N'active') " +
					"AND(DeviceData.selfDevice = N'true') " +
					"AND(JobLog.guid = '" + jobGuid + "' " +
					"And(PoolMember.Name LIKE '%" + searchString + "%') )").ToList();

				var json = new JavaScriptSerializer().Serialize(results);
				return json;
			}
		}



		public string GetNodeIp(string jobGuid, string searchString)
		{

			List<f5SearchResults> results = new List<f5SearchResults>();
			using (TargetEntities Db = new TargetEntities("F5DataEntities"))
			{

				results = Db.Database.SqlQuery<f5SearchResults>("SELECT DeviceData.name AS F5Name, VIP.name AS VIPNAME, VIP.destination AS VIPIP, Pool.name AS PoolName, PoolMember.Name AS NodeName, " +
					"PoolMember.address AS NodeIP " +
					"FROM DeviceData INNER JOIN " +
					"JobLog ON DeviceData.jobguid = JobLog.guid INNER JOIN " +
					"VIP ON DeviceData.jobguid = VIP.jobguid AND DeviceData.hostname = VIP.f5HostName INNER JOIN " +
					"Pool ON VIP.jobguid = Pool.jobguid AND VIP.f5HostName = Pool.f5HostName AND VIP.pool = Pool.fullpath INNER JOIN " +
					"PoolMember ON Pool.guid = PoolMember.PoolGuid " +
					"WHERE(DeviceData.failoverState = N'active') " +
					"AND(DeviceData.selfDevice = N'true') " +
					"AND(JobLog.guid = '" + jobGuid + "' " +
					"And(PoolMember.address LIKE '%" + searchString + "%') )").ToList();

				var json = new JavaScriptSerializer().Serialize(results);
				return json;
			}
		}


		public string GetDetail(Guid jobGuid, string f5HostName, string vip)
		{

			List<VIP> vIPList = new List<VIP>();
			using (TargetEntities Db = new TargetEntities("F5DataEntities"))
			{
				if (vip != "undefined")
				{
					vIPList = Db.Database.SqlQuery<VIP>("SELECT * FROM VIP WHERE jobguid = '" + jobGuid + "' AND f5HostName = '" + f5HostName + "' AND name = '" + vip + "' ORDER BY name").ToList();
				}
				else
				{
					vIPList = Db.Database.SqlQuery<VIP>("SELECT * FROM VIP WHERE jobguid = '" + jobGuid + "' AND f5HostName = '" + f5HostName + "' ORDER BY name").ToList();
				}


				foreach (var Vip in vIPList)
				{
					Vip.vipStatus = Db.Database.SqlQuery<VIPStat>("SELECT TOP 1 * FROM VIPStats WHERE VIPGuid = '" + Vip.guid + "'").FirstOrDefault();
					if (Vip.pool != "" && Vip.pool != null)
					{
						string poolname = Vip.pool.Replace("/Common/", "");

						Pool vipPool = Db.Database.SqlQuery<Pool>("SELECT TOP 1 * FROM Pool WHERE jobguid = '" + jobGuid + "' AND f5HostName = '" + f5HostName + "' AND name = '" + poolname + "' ORDER BY name").FirstOrDefault();

						vipPool.poolMemberList = Db.Database.SqlQuery<PoolMember>("SELECT * FROM PoolMember WHERE PoolGuid = '" + vipPool.guid + "'").ToList();
						if (vipPool.monitor != null)
						{
							vipPool.monitor = vipPool.monitor.Replace("/Common/", "");
						}

						foreach (var item in vipPool.poolMemberList)
						{
							if (item.address.Contains('%'))
							{
								char[] chars = { '%' };
								string[] ipVar = item.address.Split(chars);
								item.IP = ipVar[0];
								item.routeDomain = ipVar[1];
							}
							else
							{
								item.IP = item.address;
							}
						}

						Vip.poolInfo = vipPool;
					}

					if (Vip.rules != null && Vip.rules != "")
					{
						string rule = Vip.rules.Replace("/Common/", ";");
						Vip.rules = rule;
						string[] vipRules = rule.Split(';');
						List<Irule> ruleList = new List<Irule>();
						foreach (var item in vipRules)
						{
							if (item != "" && item != null)
							{
								Irule viprule = Db.Database.SqlQuery<Irule>("SELECT TOP 1 * FROM Irule WHERE jobguid = '" + jobGuid + "' AND name = '" + item + "' AND f5HostName = '" + f5HostName + "'").FirstOrDefault();
								ruleList.Add(viprule);
							}
						}
						Vip.iRuleList = ruleList;
					}

					if (Vip.destination != null && Vip.destination != "")
					{
						Vip.destination = Vip.destination.Replace("/Common/", "");
						int ipv6count = Vip.destination.Count(a => a == ':');
						if (ipv6count > 1)
						{
							if (Vip.destination.Contains("%"))
							{
								char[] chars = { '%', '.' };
								string[] ipVar = Vip.destination.Split(chars);
								Vip.IP = ipVar[0];
								Vip.routeDomain = ipVar[1];
								Vip.port = ipVar[2];
							}
							else
							{
								char[] chars = { '.' };
								string[] ipVar = Vip.destination.Split(chars);
								Vip.IP = ipVar[0];
								Vip.port = ipVar[1];
							}
						}
						else
						{
							if (Vip.destination.Contains("%"))
							{
								char[] chars = { '%', ':' };
								string[] ipVar = Vip.destination.Split(chars);
								Vip.IP = ipVar[0];
								Vip.routeDomain = ipVar[1];
								Vip.port = ipVar[2];
							}
							else
							{
								char[] chars = { ':' };
								string[] ipVar = Vip.destination.Split(chars);
								Vip.IP = ipVar[0];
								Vip.port = ipVar[1];
							}
						}
					}



					//VipProfilesDetail vpd = new VipProfilesDetail();
					//int clientsideCount = dc.VipProfilesDetails.Where(a => a.vipguid == Vip.guid && a.context == "clientside").Count();

					//if (clientsideCount > 0)
					//{
					//	vpd = dc.VipProfilesDetails.Where(a => a.vipguid == Vip.guid && a.context == "clientside").FirstOrDefault();
					//	List<ClientSSL> vipssl = dc.ClientSSLs.Where(a => a.name == vpd.name && a.jobguid == jobguid && a.f5HostName == f5HostName).ToList();
					//	Vip.sslList = vipssl;
					//}
					//int persisttest = dc.VipPersists.Where(a => a.VIPguid == Vip.guid).Count();
					//if (persisttest > 0)
					//{
					//	VIPPersistData vipPersistData = new VIPPersistData();
					//	if (dc.Database.SqlQuery<VIPPersistData>("SELECT dbo.VipPersist.name AS PersistName, dbo.VIPPersistDetails.persistType, dbo.VIPPersistDetails.timeout " +
					//	"FROM dbo.JobLog INNER JOIN " +
					//	  "dbo.DeviceData ON dbo.JobLog.guid = dbo.DeviceData.jobguid INNER JOIN " +
					//	  "dbo.VIP ON dbo.DeviceData.jobguid = dbo.VIP.jobguid AND dbo.DeviceData.hostname = dbo.VIP.f5HostName INNER JOIN " +
					//	  "dbo.VipPersist ON dbo.VIP.guid = dbo.VipPersist.VIPguid INNER JOIN " +
					//	  "dbo.VIPPersistDetails ON dbo.VipPersist.f5HostName = dbo.VIPPersistDetails.f5hostname AND dbo.VipPersist.name = dbo.VIPPersistDetails.name " +
					//	"WHERE(dbo.DeviceData.failoverState = N'active') AND(dbo.DeviceData.selfDevice = N'true') AND(dbo.JobLog.guid = '" + jobguid + "') AND " +
					//	  "(dbo.VIP.name = N'" + Vip.name + "')").FirstOrDefault() != null)
					//	{
					//		vipPersistData = dc.Database.SqlQuery<VIPPersistData>("SELECT dbo.VipPersist.name AS PersistName, dbo.VIPPersistDetails.persistType, dbo.VIPPersistDetails.timeout " +
					//	"FROM dbo.JobLog INNER JOIN " +
					//	"dbo.DeviceData ON dbo.JobLog.guid = dbo.DeviceData.jobguid INNER JOIN " +
					//	"dbo.VIP ON dbo.DeviceData.jobguid = dbo.VIP.jobguid AND dbo.DeviceData.hostname = dbo.VIP.f5HostName INNER JOIN " +
					//	"dbo.VipPersist ON dbo.VIP.guid = dbo.VipPersist.VIPguid INNER JOIN " +
					//	"dbo.VIPPersistDetails ON dbo.VipPersist.f5HostName = dbo.VIPPersistDetails.f5hostname AND dbo.VipPersist.name = dbo.VIPPersistDetails.name " +
					//	"WHERE(dbo.DeviceData.failoverState = N'active') AND(dbo.DeviceData.selfDevice = N'true') AND(dbo.JobLog.guid = '" + jobguid + "') AND " +
					//	"(dbo.VIP.name = N'" + Vip.name + "')").FirstOrDefault();

					//		Vip.PersistName = vipPersistData.PersistName;
					//		Vip.persistType = vipPersistData.persistType;
					//		Vip.PersistTimeOut = vipPersistData.timeout;
					//	}

					//}

				}
			}


			var json = new JavaScriptSerializer().Serialize(vIPList);
			return json;


		}





	}


}

