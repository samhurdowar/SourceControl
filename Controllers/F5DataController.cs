using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using System.Globalization;
using System.Net.Mail;
using OfficeOpenXml;
using System.IO;
using System.Data.SqlClient;
using System.Configuration;
using Dapper;
using Newtonsoft.Json;
using SourceControl.Models;
using SourceControl.Models.Db;
using SourceControl.Services;

namespace SourceControl.Controllers
{

	[OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
	public class F5DataController : Controller
	{

		public ActionResult ScriptGenerator()
		{
			return View();

		}
		public ActionResult Index()
		{
			return View();
		}
		public ActionResult Virtual()
		{
			return View();
		}
		public ActionResult DownVIPs()
		{
			return View();
		}
		public ActionResult SSLCerts()
		{
			return View();
		}
		public ActionResult VIPSearch()
		{
			return View();
		}
		public class WeeklyPoolMembersDataFormat
		{
			public string ServerName { get; set; }
			public string ServerAddress { get; set; }
			public string ServerMonitor { get; set; }
			public string ServerSession { get; set; }
			public string ServerState { get; set; }
		}
		public class WeeklyVIPDataFormat
		{
			public string Application { get; set; }
			public string HostName { get; set; }
			public string VIPName { get; set; }
			public string destination { get; set; }
			public string PoolName { get; set; }
			public string LoadBalancingMode { get; set; }
			public List<WeeklyPoolMembersDataFormat> PoolMembers { get; set; }
			public bool VIPEnabled { get; set; }
			public string StatusAvailabilityState { get; set; }
			public string StatusEnabledState { get; set; }
			public string StatusStatusReason { get; set; }
		}
		public ActionResult Managef5Data()
		{
			return View();
		}
		public JsonNetResult WeeklyData()
		{
			List<WeeklyVIPDataFormat> dataList = new List<WeeklyVIPDataFormat>();
			using (AppF5DataEntities dc = new AppF5DataEntities())
			{
				JobLog jl = dc.JobLogs.OrderByDescending(a => a.endTime).FirstOrDefault();
				dataList = dc.Database.SqlQuery<WeeklyVIPDataFormat>(@"SELECT dbo.Apps.Application, dbo.Apps.HostName, dbo.VIP.name AS VIPName, dbo.VIP.destination, dbo.VIP.pool AS PoolName, dbo.VIP.enabled AS VIPEnabled, dbo.VIPStats.StatusAvailabilityState, 
                         dbo.VIPStats.StatusEnabledState, dbo.VIPStats.StatusStatusReason
                        FROM            dbo.JobLog INNER JOIN
                                                 dbo.VIP INNER JOIN
                                                 dbo.Apps INNER JOIN
                                                 dbo.DeviceData ON dbo.Apps.HostName = dbo.DeviceData.name ON dbo.VIP.f5HostName = dbo.DeviceData.name AND dbo.VIP.jobguid = dbo.DeviceData.jobguid ON 
                                                 dbo.JobLog.guid = dbo.DeviceData.jobguid INNER JOIN
                                                 dbo.VIPStats ON dbo.VIP.guid = dbo.VIPStats.VIPGuid
                        WHERE        (dbo.DeviceData.selfDevice = N'true') AND (dbo.DeviceData.failoverState = N'active') AND (dbo.Apps.Application = N'Him Prod') AND (dbo.JobLog.guid = '" + jl.guid + "') " +
						  " ORDER BY dbo.DeviceData.hostname, VIPName").ToList();

				foreach (var item in dataList)
				{
					item.PoolMembers = dc.Database.SqlQuery<WeeklyPoolMembersDataFormat>(@"SELECT        dbo.PoolMember.Name AS ServerName, dbo.PoolMember.address AS ServerAddress, dbo.PoolMember.monitor AS ServerMonitor, dbo.PoolMember.state AS ServerState, 
                         dbo.PoolMember.session AS ServerSession
FROM            dbo.JobLog INNER JOIN
                         dbo.DeviceData ON dbo.JobLog.guid = dbo.DeviceData.jobguid INNER JOIN
                         dbo.Pool ON dbo.DeviceData.name = dbo.Pool.f5HostName AND dbo.DeviceData.jobguid = dbo.Pool.jobguid INNER JOIN
                         dbo.PoolMember ON dbo.Pool.guid = dbo.PoolMember.PoolGuid
WHERE        (dbo.DeviceData.name = N'" + item.HostName + "') AND (dbo.DeviceData.selfDevice = N'true') AND (dbo.JobLog.guid = '" + jl.guid + "') AND (dbo.DeviceData.failoverState = N'active') AND " +
						  "(dbo.Pool.fullpath = N'" + item.PoolName + " ') ORDER BY ServerName").ToList();
				}
			}
			var data = new
			{
				dataList = dataList
			};
			JsonNetResult jsonNetResult = new JsonNetResult();
			jsonNetResult.Formatting = Formatting.Indented;
			jsonNetResult.Data = data;

			return jsonNetResult;
		}
		public JsonNetResult GetJobs()
		{
			List<JobLog> alljobs = new List<JobLog>();
			using (AppF5DataEntities dc = new AppF5DataEntities())
			{
				alljobs = dc.JobLogs.OrderByDescending(a => a.endTime).ToList();
			}
			var data = alljobs;

			JsonNetResult jsonNetResult = new JsonNetResult();
			jsonNetResult.Formatting = Formatting.Indented;
			jsonNetResult.Data = data;

			return jsonNetResult;
		}
		public JsonResult GetEnvironmentData()
		{
			List<EnvironmentList> apps = new List<EnvironmentList>();
			using (AppF5DataEntities dc = new AppF5DataEntities())
			{
				apps = dc.Database.SqlQuery<EnvironmentList>("SELECT DISTINCT Application FROM Apps ORDER BY Application").ToList();
			}
			return new JsonResult { Data = apps, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}
		public class EnvironmentList
		{
			public string Application { get; set; }
		}
		public JsonResult GetJobTasks(Guid jobguid)
		{
			List<JobLogTask> jltlist = new List<JobLogTask>();
			using (AppF5DataEntities dc = new AppF5DataEntities())
			{
				jltlist = dc.JobLogTasks.Where(a => a.ParentJobGuid == jobguid).OrderBy(a => a.Device).ToList();
			}
			return new JsonResult { Data = jltlist, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}
		public JsonResult GetSelfDeviceFromJob(Guid jobguid, string env)
		{
			List<DeviceData> selfDeviceList = new List<DeviceData>();
			if (env == "All")
			{
				using (AppF5DataEntities dc = new AppF5DataEntities())
				{
					selfDeviceList = dc.DeviceDatas.Where(a => a.selfDevice == "true" && a.jobguid == jobguid && a.failoverState == "active").OrderBy(a => a.hostname).ToList();
				}
			}
			else
			{
				List<SourceControl.Models.Db.App> envDevices = new List<SourceControl.Models.Db.App>();
				using (AppF5DataEntities dc = new AppF5DataEntities())
				{
					envDevices = dc.Apps.Where(a => a.Application == env).ToList();
					foreach (var item in envDevices)
					{
						DeviceData d = new DeviceData();
						d = dc.DeviceDatas.Where(a => a.selfDevice == "true" && a.jobguid == jobguid && a.failoverState == "active" && a.name.Contains(item.HostName)).FirstOrDefault();
						if (d != null)
						{
							selfDeviceList.Add(d);
						}
					}
				}
			}
			return new JsonResult { Data = selfDeviceList, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}

		public ActionResult LoadBalancerIP()
		{
			return View();
		}
		public ActionResult CompareSnapshots()
		{
			return View();
		}
		public class compareResults
		{
			public string LoadBalancer { get; set; }
			public string VIPName { get; set; }
			public string StatusAvailabilityState { get; set; }
			public string StatusEnabledState { get; set; }
			public string StatusStatusReason { get; set; }
			public string PoolName { get; set; }
			public List<WeeklyPoolMembersDataFormat> PoolMembers { get; set; }

		}
		public JsonNetResult CompareJobsJSON(string firstJob, string secondJob)
		{
			List<compareResults> firstJobList = new List<compareResults>();
			List<compareResults> secondJobList = new List<compareResults>();

			using (AppF5DataEntities dc = new AppF5DataEntities())
			{
				string firstsqlQuery = string.Format(@"SELECT     TOP (100) PERCENT dbo.DeviceData.name AS LoadBalancer, dbo.VIP.name AS VIPNAME, dbo.VIPStats.StatusAvailabilityState, dbo.VIPStats.StatusEnabledState, 
                      dbo.VIPStats.StatusStatusReason, dbo.VIP.pool AS PoolName
FROM         dbo.VIP INNER JOIN
                      dbo.DeviceData ON dbo.VIP.jobguid = dbo.DeviceData.jobguid AND dbo.VIP.f5HostName = dbo.DeviceData.name INNER JOIN
                      dbo.VIPStats ON dbo.VIP.jobguid = dbo.VIPStats.jobguid AND dbo.VIP.guid = dbo.VIPStats.VIPGuid INNER JOIN
                      dbo.JobLog ON dbo.DeviceData.jobguid = dbo.JobLog.guid
WHERE(dbo.DeviceData.selfDevice = N'true') AND(dbo.DeviceData.failoverState = N'active') AND(dbo.JobLog.guid = '{0}') AND
                      (NOT(dbo.VIP.name LIKE N'%ipv6%'))
ORDER BY LoadBalancer, VIPNAME", firstJob);

				string secondSqlQuery = string.Format(@"SELECT     TOP (100) PERCENT dbo.DeviceData.name AS LoadBalancer, dbo.VIP.name AS VIPNAME, dbo.VIPStats.StatusAvailabilityState, dbo.VIPStats.StatusEnabledState, 
                      dbo.VIPStats.StatusStatusReason, dbo.VIP.pool AS PoolName
FROM         dbo.VIP INNER JOIN
                      dbo.DeviceData ON dbo.VIP.jobguid = dbo.DeviceData.jobguid AND dbo.VIP.f5HostName = dbo.DeviceData.name INNER JOIN
                      dbo.VIPStats ON dbo.VIP.jobguid = dbo.VIPStats.jobguid AND dbo.VIP.guid = dbo.VIPStats.VIPGuid INNER JOIN
                      dbo.JobLog ON dbo.DeviceData.jobguid = dbo.JobLog.guid
WHERE(dbo.DeviceData.selfDevice = N'true') AND(dbo.DeviceData.failoverState = N'active') AND(dbo.JobLog.guid = '{0}') AND
                      (NOT(dbo.VIP.name LIKE N'%ipv6%'))
ORDER BY LoadBalancer, VIPNAME", secondJob);

				var job1List = dc.Database.SqlQuery<compareResults>(firstsqlQuery).ToList();

				var job2List = dc.Database.SqlQuery<compareResults>(secondSqlQuery).ToList();

				foreach (var item in job1List)
				{
					int count = job2List.Where(a => a.LoadBalancer == item.LoadBalancer &&
					a.StatusAvailabilityState == item.StatusAvailabilityState &&
					a.StatusEnabledState == item.StatusEnabledState &&
					a.StatusStatusReason == item.StatusStatusReason &&
					a.VIPName == item.VIPName).Count();
					if (count == 0)
					{
						firstJobList.Add(item);
					}
				}
				foreach (var item in firstJobList)
				{
					item.PoolMembers = dc.Database.SqlQuery<WeeklyPoolMembersDataFormat>(@"SELECT        dbo.PoolMember.Name AS ServerName, dbo.PoolMember.address AS ServerAddress, dbo.PoolMember.monitor AS ServerMonitor, dbo.PoolMember.state AS ServerState, 
                         dbo.PoolMember.session AS ServerSession
FROM            dbo.JobLog INNER JOIN
                         dbo.DeviceData ON dbo.JobLog.guid = dbo.DeviceData.jobguid INNER JOIN
                         dbo.Pool ON dbo.DeviceData.name = dbo.Pool.f5HostName AND dbo.DeviceData.jobguid = dbo.Pool.jobguid INNER JOIN
                         dbo.PoolMember ON dbo.Pool.guid = dbo.PoolMember.PoolGuid
WHERE        (dbo.DeviceData.name = N'" + item.LoadBalancer + "') AND (dbo.DeviceData.selfDevice = N'true') AND (dbo.JobLog.guid = '" + firstJob + "') AND (dbo.DeviceData.failoverState = N'active') AND " +
						  "(dbo.Pool.fullpath = N'" + item.PoolName + " ') ORDER BY ServerName").ToList();

				}
			}


			var data = new
			{
				firstJobList = firstJobList

			};
			JsonNetResult jsonNetResult = new JsonNetResult();
			jsonNetResult.Formatting = Formatting.Indented;
			jsonNetResult.Data = data;

			return jsonNetResult;
		}
		public ActionResult WeeklyReport()
		{
			return View();
		}
		public class f5IPList
		{
			public string f5HostName { get; set; }
			public string name { get; set; }
			public string address { get; set; }
			public string floating { get; set; }
			public string vlan { get; set; }

		}

		public JsonNetResult f5LoadBalancerIPJSON()
		{

			List<f5IPList> IPList = new List<f5IPList>();
			using (AppF5DataEntities dc = new AppF5DataEntities())
			{
				IPList = dc.Database.SqlQuery<f5IPList>(@"SELECT     TOP (100) PERCENT dbo.SelfIP.f5HostName, dbo.SelfIP.name, dbo.SelfIP.address, dbo.SelfIP.floating, dbo.SelfIP.vlan
FROM         dbo.DeviceData INNER JOIN
                      dbo.SelfIP ON dbo.DeviceData.jobguid = dbo.SelfIP.jobguid AND dbo.DeviceData.name = dbo.SelfIP.f5HostName
WHERE     (dbo.DeviceData.selfDevice = N'True') AND (dbo.DeviceData.jobguid =
                          (SELECT     TOP (1) guid
                            FROM          dbo.JobLog
                            WHERE      (Successful = N'Finished')
                            ORDER BY endTime DESC)) AND (NOT (dbo.SelfIP.name LIKE N'%peer%'))
ORDER BY dbo.SelfIP.f5HostName, dbo.SelfIP.name").ToList();
			}

			var data = new
			{
				IPList = IPList
			};
			JsonNetResult jsonNetResult = new JsonNetResult();
			jsonNetResult.Formatting = Formatting.Indented;
			jsonNetResult.Data = data;

			return jsonNetResult;
		}
		public class VIPPersistData
		{
			public string PersistName { get; set; }
			public string persistType { get; set; }
			public string timeout { get; set; }
		}

		public JsonResult GetSSLCertData(Guid jobguid)
		{
			var data = SSLCertData(jobguid);
			return new JsonResult { Data = data, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}
		public class CertData
		{
			public string f5HostName { get; set; }
			public string VIPName { get; set; }
			public string ProfileName { get; set; }
			public string expiration { get; set; }
			public string subject { get; set; }
			public string subjectAlternativeName { get; set; }
			public string chain { get; set; }
		}
		public class CertDataConvertedDate
		{
			public string f5HostName { get; set; }
			public string VIPName { get; set; }
			public string ProfileName { get; set; }
			public string expiration { get; set; }
			public DateTime experationDate { get; set; }
			public string subject { get; set; }
			public string subjectAlternativeName { get; set; }
			public string chain { get; set; }
		}
		public class basicVIPStatus
		{
			public string f5hostname { get; set; }
			public string name { get; set; }
			public string StatusAvailabilityState { get; set; }
			public string StatusEnabledState { get; set; }
			public string StatusStatusReason { get; set; }

		}
		public ActionResult selfIPs()
		{
			return View();
		}
		public JsonNetResult SelfIPsJSON()
		{
			using (IDbConnection _db = new SqlConnection(ConfigurationManager.ConnectionStrings["f5dataConnectionString"].ConnectionString), commandTimeout = null)
			{
				List<SelfIP> selfIPList = _db.Query<SelfIP>(@"SELECT        TOP (100) PERCENT dbo.DeviceData.name, dbo.SelfIP.address, dbo.SelfIP.name AS interfaceName, dbo.SelfIP.floating, dbo.SelfIP.vlan, dbo.VLAN.mtu, dbo.VLAN.tag
FROM            dbo.DeviceData INNER JOIN
                         dbo.SelfIP ON dbo.DeviceData.jobguid = dbo.SelfIP.jobguid AND dbo.DeviceData.name = dbo.SelfIP.f5HostName INNER JOIN
                         dbo.VLAN ON dbo.SelfIP.vlan = dbo.VLAN.name AND dbo.SelfIP.f5HostName = dbo.VLAN.f5Hostname AND dbo.SelfIP.jobguid = dbo.VLAN.jobguid
WHERE        (dbo.DeviceData.selfDevice = N'true') AND (NOT (dbo.SelfIP.name LIKE N'%peer%') AND NOT (dbo.SelfIP.name LIKE N'%ipv6%')) AND (dbo.DeviceData.jobguid =
                             (SELECT        TOP (1) guid
                               FROM            dbo.JobLog
                               WHERE        (Successful = N'Finished')
                               ORDER BY endTime DESC))
ORDER BY dbo.DeviceData.name, dbo.SelfIP.address, interfaceName").ToList();

				var data = new
				{
					selfIPList = selfIPList
				};

				JsonNetResult jsonNetResult = new JsonNetResult();
				jsonNetResult.Formatting = Formatting.Indented;
				jsonNetResult.Data = data;

				return jsonNetResult;
			}
		}
		public class SelfIP
		{
			public string name { get; set; }
			public string address { get; set; }
			public string interfaceName { get; set; }
			public string floating { get; set; }
			public string vlan { get; set; }
			public string mtu { get; set; }
			public string tag { get; set; }
		}

		public List<CertDataConvertedDate> SSLCertData(Guid jobguid)
		{
			CultureInfo provider = CultureInfo.InvariantCulture;
			List<CertDataConvertedDate> data = new List<CertDataConvertedDate>();
			using (IDbConnection _db = new SqlConnection(ConfigurationManager.ConnectionStrings["f5dataConnectionString"].ConnectionString), commandTimeout = null)
			{

				List<CertData> certlist = _db.Query<CertData>("SELECT VipProfilesDetails.f5HostName, VIP.name AS VIPName, VipProfilesDetails.name AS ProfileName, CertFile.expiration, CertFile.subject, " +
								 "CertFile.subjectAlternativeName, ClientSSLCertKeyChain.chain " +
						  "FROM CertFile INNER JOIN " +
								 "ClientSSL ON CertFile.jobguid = ClientSSL.jobguid AND CertFile.f5HostName = ClientSSL.f5HostName AND " +
								 "CertFile.fullPath = ClientSSL.cert INNER JOIN " +
								 "ClientSSLCertKeyChain ON ClientSSL.certKeyChainGuid = ClientSSLCertKeyChain.CertGuid LEFT OUTER JOIN " +
								 "VIP INNER JOIN " +
								 "VipProfilesDetails ON VIP.guid = VipProfilesDetails.vipguid INNER JOIN " +
								 "JobLog INNER JOIN " +
								 "DeviceData ON JobLog.guid = DeviceData.jobguid ON VIP.jobguid = DeviceData.jobguid AND VIP.f5HostName = DeviceData.name ON " +
								 "ClientSSL.jobguid = VipProfilesDetails.jobguid AND ClientSSL.f5HostName = VipProfilesDetails.f5HostName AND " +
								 "ClientSSL.fullPath = VipProfilesDetails.fullpath " +
						  "WHERE(NOT(VIP.name LIKE N'%ipv6%')) AND(DeviceData.selfDevice = N'true') AND(DeviceData.failoverState = N'active') AND (JobLog.guid = '" + jobguid + "')").ToList();
				foreach (var s in certlist)
				{
					CertDataConvertedDate d = new CertDataConvertedDate();
					DateTime convertedDate;
					//Jan 28 23:59:59 2017 GMT
					s.expiration = s.expiration.Replace(" GMT", "");
					string format = "MMM dd HH:mm:ss yyyy";
					string format2 = "MMM  d HH:mm:ss yyyy";
					try
					{
						convertedDate = DateTime.ParseExact(s.expiration, format, null);
					}
					catch
					{
						convertedDate = DateTime.ParseExact(s.expiration, format2, null);
					}

					if (s.subject != null)
					{
						string[] subject = s.subject.Split(',');
						foreach (var item in subject)
						{
							if (item.Contains("CN"))
							{
								string[] subjects = item.Split('=');
								d.subject = subjects[1];
							}
						}
					}
					if (s.subjectAlternativeName != null)
					{
						d.subjectAlternativeName = s.subjectAlternativeName.Replace("DNS:", "");
					}
					if (s.chain != null)
					{
						d.chain = s.chain.Replace("/Common/", "");
					}

					d.expiration = convertedDate.ToString("yyyy MMM dd HH:mm:ss");
					d.experationDate = convertedDate;
					d.f5HostName = s.f5HostName;
					d.ProfileName = s.ProfileName;
					d.VIPName = s.VIPName;
					data.Add(d);
				}
			}
			data.OrderBy(a => a.VIPName);
			data.Sort((x, y) => DateTime.Compare(x.experationDate, y.experationDate));
			return data;
		}
		//public JsonResult GetDownVIPs(Guid jobguid)
		//{
		//    AppF5DataEntities db = new AppF5DataEntities();



		//    return new JsonResult { Data = data, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		//}
		public JsonResult f5Summary()
		{
			AppF5DataEntities db = new AppF5DataEntities();
			DateTime dtnow = DateTime.Now;
			using (IDbConnection _db = new SqlConnection(ConfigurationManager.ConnectionStrings["f5dataConnectionString"].ConnectionString), commandTimeout = null)
			{
				JobLog jl = db.JobLogs.OrderByDescending(a => a.endTime).FirstOrDefault();
				//Cert Data
				var certdata = SSLCertData(jl.guid);
				int TotalCertCount = certdata.Count();
				int expiredCertCount = certdata.Where(a => a.experationDate < dtnow).Count();
				int expiringOneMonthCount = certdata.Where(a => a.experationDate > dtnow && a.experationDate < dtnow.AddMonths(1)).Count();
				int expiringThreeMonthCount = certdata.Where(a => a.experationDate > dtnow && a.experationDate < dtnow.AddMonths(3)).Count();
				int expiringSixMonthCount = certdata.Where(a => a.experationDate > dtnow && a.experationDate < dtnow.AddMonths(6)).Count();
				//Vip data
				List<VIPSummaryClass> vipdata = VIPSummaryData(jl.guid);
				int totalVIPCount = vipdata.Count();
				int availableVipCount = vipdata.Where(a => a.StatusAvailabilityState == "available").Count();
				int unknownVipCount = vipdata.Where(a => a.StatusAvailabilityState == "unknown").Count();
				int offlineVipCount = vipdata.Where(a => a.StatusAvailabilityState == "offline").Count();
				//Node Data
				List<NodeSummaryClass> nodedata = _db.Query<NodeSummaryClass>("SELECT PoolMember.Name, PoolMember.state " +
						  "FROM PoolMember INNER JOIN " +
								 "Pool ON PoolMember.PoolGuid = Pool.guid INNER JOIN " +
								 "JobLog INNER JOIN " +
								 "DeviceData ON JobLog.guid = DeviceData.jobguid INNER JOIN " +
								 "VIP ON DeviceData.jobguid = VIP.jobguid AND DeviceData.name = VIP.f5HostName ON Pool.fullpath = VIP.pool AND " +
								 "dbo.Pool.jobguid = dbo.VIP.jobguid " +
						  "WHERE(dbo.DeviceData.selfDevice = N'true') AND(dbo.DeviceData.failoverState = N'active') AND(dbo.JobLog.guid = '" + jl.guid + "')").ToList();
				int totalPoolMembers = nodedata.Count();
				int activePoolMembers = nodedata.Where(a => a.state == "up").Count();
				//Device Data
				int devicecount = db.MasterDeviceLists.Count();
				//pool Data
				List<string> pooldata = _db.Query<string>("SELECT Pool.name " +
						  "FROM Pool INNER JOIN " +
								 "JobLog INNER JOIN " +
								 "DeviceData ON JobLog.guid = DeviceData.jobguid INNER JOIN " +
								 "VIP ON DeviceData.jobguid = VIP.jobguid AND DeviceData.name = VIP.f5HostName ON Pool.jobguid = VIP.jobguid AND " +
								 "Pool.f5HostName = VIP.f5HostName AND Pool.fullpath = VIP.pool " +
						  "WHERE(dbo.DeviceData.selfDevice = N'true') AND(dbo.DeviceData.failoverState = N'active') AND(dbo.JobLog.guid = '" + jl.guid + "')").ToList();
				var poolDataDistinct = pooldata.Distinct();
				int poolcount = poolDataDistinct.Count();
				//rule Data
				int rulecount = _db.Query<string>("SELECT Distinct name " +
								"FROM Irule Where jobguid = '" + jl.guid + "'").Count();
				var data = new
				{
					poolcount = poolcount,
					rulecount = rulecount,
					devicecount = devicecount,
					activePoolMembers = activePoolMembers,
					totalPoolMembers = totalPoolMembers,
					offlineVipCount = offlineVipCount,
					unknownVipCount = unknownVipCount,
					availableVipCount = availableVipCount,
					totalVIPCount = totalVIPCount,
					expiringSixMonthCount = expiringSixMonthCount,
					expiringThreeMonthCount = expiringThreeMonthCount,
					expiringOneMonthCount = expiringOneMonthCount,
					expiredCertCount = expiredCertCount,
					TotalCertCount = TotalCertCount
				};

				return new JsonResult { Data = data, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
			}
		}

		private List<VIPSummaryClass> VIPSummaryData(Guid guid)
		{
			using (IDbConnection _db = new SqlConnection(ConfigurationManager.ConnectionStrings["f5dataConnectionString"].ConnectionString), commandTimeout = null)
			{
				List<VIPSummaryClass> vipdata = _db.Query<VIPSummaryClass>(string.Format(@"SELECT DeviceData.Name As f5Name, VIP.name As VIPNAME,  VIP.destination AS VIPIP, VIP.pool, VIPStats.StatusAvailabilityState, VIPStats.StatusEnabledState , VIPStats.StatusStatusReason 
									FROM DeviceData 
									INNER JOIN  JobLog ON DeviceData.jobguid = JobLog.guid 
									INNER JOIN  VIP ON DeviceData.jobguid = VIP.jobguid AND DeviceData.hostname = VIP.f5HostName 
									INNER JOIN  VIPStats ON VIP.guid = VIPStats.VIPGuid 
									WHERE (DeviceData.failoverState = N'active') AND (JobLog.guid = '{0}') AND (DeviceData.selfDevice = N'true')"
									, guid)).ToList();
				foreach (var item in vipdata)
				{
					item.VIPIP = item.VIPIP.Replace("/Common/", "");
					if (item.pool != null)
					{
						item.pool = item.pool.Replace("/Common/", "");
					}

				}
				return vipdata;
			}
		}
		public class VIPSummaryClass
		{
			public string VIPNAME { get; set; }
			public string StatusAvailabilityState { get; set; }
			public string StatusEnabledState { get; set; }
			public string StatusStatusReason { get; set; }
			public string f5Name { get; set; }
			public string VIPIP { get; set; }
			public string pool { get; set; }

			public List<VIPStatsOverTime> VIPStats { get; set; }

		}
		public class NodeSummaryClass
		{
			public string Name { get; set; }
			public string state { get; set; }
		}
		public JsonResult Searchf5s(Guid jobguid, string searchString)
		{
			List<f5SearchResults> vipNameSearchList = SearchVIPName(jobguid, searchString);
			List<f5SearchResults> vipIPSearchList = SearchVIPIP(jobguid, searchString);
			List<f5SearchResults> poolNameSearchList = SearchPoolName(jobguid, searchString);
			List<f5SearchResults> nodeNameSearchList = SearchNodeName(jobguid, searchString);
			List<f5SearchResults> nodeIPSearchList = SearchNodeIP(jobguid, searchString);
			int vipNameSearchCount = vipNameSearchList.Count();
			int vipIPSearchCount = vipIPSearchList.Count();
			int poolNameSearchCount = poolNameSearchList.Count();
			int nodeNameSearchCount = nodeNameSearchList.Count();
			int nodeIPSearchCount = nodeIPSearchList.Count();

			var data = new
			{
				vipNameSearchList = vipNameSearchList,
				vipIPSearchList = vipIPSearchList,
				poolNameSearchList = poolNameSearchList,
				nodeNameSearchList = nodeNameSearchList,
				nodeIPSearchList = nodeIPSearchList,
				vipNameSearchCount = vipNameSearchCount,
				vipIPSearchCount = vipIPSearchCount,
				poolNameSearchCount = poolNameSearchCount,
				nodeNameSearchCount = nodeNameSearchCount,
				nodeIPSearchCount = nodeIPSearchCount,
			};


			return new JsonResult { Data = data, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}

		private List<f5SearchResults> SearchNodeIP(Guid jobguid, string searchString)
		{

			List<f5SearchResults> results = new List<f5SearchResults>();
			using (IDbConnection _db = new SqlConnection(ConfigurationManager.ConnectionStrings["f5dataConnectionString"].ConnectionString), commandTimeout = null)
			{
				results = _db.Query<f5SearchResults>("SELECT DeviceData.name AS F5Name, VIP.name AS VIPNAME, VIP.destination AS VIPIP, Pool.name AS PoolName, PoolMember.Name AS NodeName, " +
							"PoolMember.address AS NodeIP " +
						 "FROM DeviceData INNER JOIN " +
							 "JobLog ON DeviceData.jobguid = JobLog.guid INNER JOIN " +
							 "VIP ON DeviceData.jobguid = VIP.jobguid AND DeviceData.hostname = VIP.f5HostName INNER JOIN " +
							 "Pool ON VIP.jobguid = Pool.jobguid AND VIP.f5HostName = Pool.f5HostName AND VIP.pool = Pool.fullpath INNER JOIN " +
							 "PoolMember ON Pool.guid = PoolMember.PoolGuid " +
						  "WHERE(DeviceData.failoverState = N'active') " +
							 "AND(DeviceData.selfDevice = N'true') " +
							 "AND(JobLog.guid = '" + jobguid + "' " +
							 "And(PoolMember.address LIKE '%" + searchString + "%') )").ToList();
				foreach (var item in results)
				{
					item.VIPIP = item.VIPIP.Replace("/Common/", "");
				}

				return results;
			}
		}

		private List<f5SearchResults> SearchNodeName(Guid jobguid, string searchString)
		{

			List<f5SearchResults> results = new List<f5SearchResults>();
			using (IDbConnection _db = new SqlConnection(ConfigurationManager.ConnectionStrings["f5dataConnectionString"].ConnectionString), commandTimeout = null)
			{
				results = _db.Query<f5SearchResults>("SELECT DeviceData.name AS F5Name, VIP.name AS VIPNAME, VIP.destination AS VIPIP, Pool.name AS PoolName, PoolMember.Name AS NodeName, " +
							"PoolMember.address AS NodeIP " +
						 "FROM DeviceData INNER JOIN " +
							 "JobLog ON DeviceData.jobguid = JobLog.guid INNER JOIN " +
							 "VIP ON DeviceData.jobguid = VIP.jobguid AND DeviceData.hostname = VIP.f5HostName INNER JOIN " +
							 "Pool ON VIP.jobguid = Pool.jobguid AND VIP.f5HostName = Pool.f5HostName AND VIP.pool = Pool.fullpath INNER JOIN " +
							 "PoolMember ON Pool.guid = PoolMember.PoolGuid " +
						  "WHERE(DeviceData.failoverState = N'active') " +
							 "AND(DeviceData.selfDevice = N'true') " +
							 "AND(JobLog.guid = '" + jobguid + "' " +
							 "And(PoolMember.Name LIKE '%" + searchString + "%') )").ToList();
				foreach (var item in results)
				{
					item.VIPIP = item.VIPIP.Replace("/Common/", "");
				}

				return results;
			}
		}

		private List<f5SearchResults> SearchPoolName(Guid jobguid, string searchString)
		{

			List<f5SearchResults> results = new List<f5SearchResults>();
			using (IDbConnection _db = new SqlConnection(ConfigurationManager.ConnectionStrings["f5dataConnectionString"].ConnectionString), commandTimeout = null)
			{
				results = _db.Query<f5SearchResults>("SELECT DeviceData.name AS F5Name, VIP.name AS VIPNAME, VIP.destination AS VIPIP, Pool.name AS PoolName, PoolMember.Name AS NodeName, " +
							"PoolMember.address AS NodeIP " +
						 "FROM DeviceData INNER JOIN " +
							 "JobLog ON DeviceData.jobguid = JobLog.guid INNER JOIN " +
							 "VIP ON DeviceData.jobguid = VIP.jobguid AND DeviceData.hostname = VIP.f5HostName INNER JOIN " +
							 "Pool ON VIP.jobguid = Pool.jobguid AND VIP.f5HostName = Pool.f5HostName AND VIP.pool = Pool.fullpath INNER JOIN " +
							 "PoolMember ON Pool.guid = PoolMember.PoolGuid " +
						  "WHERE(DeviceData.failoverState = N'active') " +
							 "AND(DeviceData.selfDevice = N'true') " +
							 "AND(JobLog.guid = '" + jobguid + "' " +
							 "And(Pool.name LIKE '%" + searchString + "%') )").ToList();
				foreach (var item in results)
				{
					item.VIPIP = item.VIPIP.Replace("/Common/", "");
				}

				return results;
			}
		}

		private List<f5SearchResults> SearchVIPIP(Guid jobguid, string searchString)
		{

			List<f5SearchResults> results = new List<f5SearchResults>();
			using (IDbConnection _db = new SqlConnection(ConfigurationManager.ConnectionStrings["f5dataConnectionString"].ConnectionString), commandTimeout = null)
			{
				results = _db.Query<f5SearchResults>("SELECT DeviceData.name AS F5Name, VIP.name AS VIPNAME, VIP.destination AS VIPIP, Pool.name AS PoolName, PoolMember.Name AS NodeName, " +
							"PoolMember.address AS NodeIP " +
						 "FROM DeviceData INNER JOIN " +
							 "JobLog ON DeviceData.jobguid = JobLog.guid INNER JOIN " +
							 "VIP ON DeviceData.jobguid = VIP.jobguid AND DeviceData.hostname = VIP.f5HostName INNER JOIN " +
							 "Pool ON VIP.jobguid = Pool.jobguid AND VIP.f5HostName = Pool.f5HostName AND VIP.pool = Pool.fullpath INNER JOIN " +
							 "PoolMember ON Pool.guid = PoolMember.PoolGuid " +
						  "WHERE(DeviceData.failoverState = N'active') " +
							 "AND(DeviceData.selfDevice = N'true') " +
							 "AND(JobLog.guid = '" + jobguid + "' " +
							 "And(VIP.destination LIKE '%" + searchString + "%') )").ToList();
				foreach (var item in results)
				{
					item.VIPIP = item.VIPIP.Replace("/Common/", "");
				}

				return results;
			}
		}

		private List<f5SearchResults> SearchVIPName(Guid jobguid, string searchString)
		{

			List<f5SearchResults> results = new List<f5SearchResults>();
			using (IDbConnection _db = new SqlConnection(ConfigurationManager.ConnectionStrings["f5dataConnectionString"].ConnectionString), commandTimeout = null)
			{
				results = _db.Query<f5SearchResults>("SELECT DeviceData.name AS F5Name, VIP.name AS VIPNAME, VIP.destination AS VIPIP, Pool.name AS PoolName, PoolMember.Name AS NodeName, " +
							"PoolMember.address AS NodeIP " +
						 "FROM DeviceData INNER JOIN " +
							 "JobLog ON DeviceData.jobguid = JobLog.guid INNER JOIN " +
							 "VIP ON DeviceData.jobguid = VIP.jobguid AND DeviceData.hostname = VIP.f5HostName INNER JOIN " +
							 "Pool ON VIP.jobguid = Pool.jobguid AND VIP.f5HostName = Pool.f5HostName AND VIP.pool = Pool.fullpath INNER JOIN " +
							 "PoolMember ON Pool.guid = PoolMember.PoolGuid " +
						  "WHERE(DeviceData.failoverState = N'active') " +
							 "AND(DeviceData.selfDevice = N'true') " +
							 "AND(JobLog.guid = '" + jobguid + "' " +
							 "And(VIP.name LIKE '%" + searchString + "%') )").ToList();
				foreach (var item in results)
				{
					item.VIPIP = item.VIPIP.Replace("/Common/", "");
				}


				return results;
			}
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

		public ActionResult ManageOfflineVips()
		{

			return View();
		}
		public JsonNetResult DeleteJobsByDate(string dateString)
		{
			DateTime date = DateTime.Parse(dateString).Date;
			AppF5DataEntities db = new AppF5DataEntities();
			db.Database.ExecuteSqlCommand("DELETE [dbo].[Cert] FROM [dbo].[Cert] INNER JOIN dbo.JobLog ON [dbo].[Cert].jobguid = dbo.JobLog.guid WHERE     (dbo.JobLog.endTime < CONVERT(DATETIME, '" + date + "', 102))");
			db.Database.ExecuteSqlCommand("DELETE [dbo].[CertFile] FROM [dbo].[CertFile] INNER JOIN dbo.JobLog ON [dbo].[CertFile].jobguid = dbo.JobLog.guid WHERE     (dbo.JobLog.endTime < CONVERT(DATETIME, '" + date + "', 102))");
			db.Database.ExecuteSqlCommand("DELETE [dbo].[ClientSSL] FROM [dbo].[ClientSSL] INNER JOIN dbo.JobLog ON [dbo].[ClientSSL].jobguid = dbo.JobLog.guid WHERE     (dbo.JobLog.endTime < CONVERT(DATETIME, '" + date + "', 102))");
			db.Database.ExecuteSqlCommand("DELETE [dbo].[ClientSSLCertKeyChain] FROM [dbo].[ClientSSLCertKeyChain] INNER JOIN dbo.JobLog ON [dbo].[ClientSSLCertKeyChain].jobguid = dbo.JobLog.guid WHERE     (dbo.JobLog.endTime < CONVERT(DATETIME, '" + date + "', 102))");
			db.Database.ExecuteSqlCommand("DELETE [dbo].[DeviceData] FROM [dbo].[DeviceData]  INNER JOIN dbo.JobLog ON dbo.DeviceData.jobguid = dbo.JobLog.guid WHERE     (dbo.JobLog.endTime < CONVERT(DATETIME, '" + date + "', 102))");
			db.Database.ExecuteSqlCommand("DELETE [dbo].[Irule] FROM [dbo].[Irule] INNER JOIN dbo.JobLog ON dbo.Irule.jobguid = dbo.JobLog.guid WHERE     (dbo.JobLog.endTime < CONVERT(DATETIME, '" + date + "', 102))");
			db.Database.ExecuteSqlCommand("DELETE [dbo].[IruleApiRawValues2] FROM [dbo].[IruleApiRawValues2] INNER JOIN dbo.JobLog ON dbo.DeviceData.jobguid = dbo.JobLog.guid WHERE     (dbo.JobLog.endTime < CONVERT(DATETIME, '" + date + "', 102))");
			db.Database.ExecuteSqlCommand("DELETE FROM [dbo].[JobLog] WHERE     (dbo.JobLog.endTime < CONVERT(DATETIME, '" + date + "', 102))");
			db.Database.ExecuteSqlCommand("DELETE [dbo].[JobLogTask] FROM [dbo].[JobLogTask] INNER JOIN dbo.JobLog ON dbo.JobLogTask.jobguid = dbo.JobLog.guid WHERE     (dbo.JobLog.endTime < CONVERT(DATETIME, '" + date + "', 102))");
			db.Database.ExecuteSqlCommand("DELETE [dbo].[MonitorHttp] FROM [dbo].[MonitorHttp] INNER JOIN dbo.JobLog ON dbo.MonitorHttp.jobguid = dbo.JobLog.guid WHERE     (dbo.JobLog.endTime < CONVERT(DATETIME, '" + date + "', 102))");
			db.Database.ExecuteSqlCommand("DELETE [dbo].[Node] FROM [dbo].[Node] INNER JOIN dbo.JobLog ON dbo.Node.jobguid = dbo.JobLog.guid WHERE     (dbo.JobLog.endTime < CONVERT(DATETIME, '" + date + "', 102))");
			db.Database.ExecuteSqlCommand("DELETE [dbo].[NodeFqdn] FROM [dbo].[NodeFqdn] INNER JOIN dbo.JobLog ON dbo.NodeFqdn.jobguid = dbo.JobLog.guid WHERE     (dbo.JobLog.endTime < CONVERT(DATETIME, '" + date + "', 102))");
			db.Database.ExecuteSqlCommand("DELETE [dbo].[Pool] FROM [dbo].[Pool] INNER JOIN dbo.JobLog ON dbo.Pool.jobguid = dbo.JobLog.guid WHERE     (dbo.JobLog.endTime < CONVERT(DATETIME, '" + date + "', 102))");
			db.Database.ExecuteSqlCommand("DELETE [dbo].[PoolMember] FROM [dbo].[PoolMember] INNER JOIN dbo.JobLog ON dbo.PoolMember.jobguid = dbo.JobLog.guid WHERE     (dbo.JobLog.endTime < CONVERT(DATETIME, '" + date + "', 102))");
			db.Database.ExecuteSqlCommand("DELETE [dbo].[PoolMembersReference] FROM [dbo].[PoolMembersReference] INNER JOIN dbo.JobLog ON dbo.PoolMembersReference.jobguid = dbo.JobLog.guid WHERE     (dbo.JobLog.endTime < CONVERT(DATETIME, '" + date + "', 102))");
			db.Database.ExecuteSqlCommand("DELETE [dbo].[Routes] FROM [dbo].[Routes] INNER JOIN dbo.JobLog ON dbo.Routes.jobguid = dbo.JobLog.guid WHERE     (dbo.JobLog.endTime < CONVERT(DATETIME, '" + date + "', 102))");
			db.Database.ExecuteSqlCommand("DELETE [dbo].[SelfIP] FROM [dbo].[SelfIP] INNER JOIN dbo.JobLog ON dbo.SelfIP.jobguid = dbo.JobLog.guid WHERE     (dbo.JobLog.endTime < CONVERT(DATETIME, '" + date + "', 102))");
			db.Database.ExecuteSqlCommand("DELETE [dbo].[SNATPool] FROM [dbo].[SNATPool] INNER JOIN dbo.JobLog ON dbo.SNATPool.jobguid = dbo.JobLog.guid WHERE     (dbo.JobLog.endTime < CONVERT(DATETIME, '" + date + "', 102))");
			db.Database.ExecuteSqlCommand("DELETE [dbo].[SNATTranslate] FROM [dbo].[SNATTranslate] INNER JOIN dbo.JobLog ON dbo.SNATTranslate.jobguid = dbo.JobLog.guid WHERE     (dbo.JobLog.endTime < CONVERT(DATETIME, '" + date + "', 102))");
			db.Database.ExecuteSqlCommand("DELETE [dbo].[VIP] FROM [dbo].[VIP] INNER JOIN dbo.JobLog ON dbo.VIP.jobguid = dbo.JobLog.guid WHERE     (dbo.JobLog.endTime < CONVERT(DATETIME, '" + date + "', 102))");
			db.Database.ExecuteSqlCommand("DELETE [dbo].[VipPersist] FROM [dbo].[VipPersist] INNER JOIN dbo.JobLog ON dbo.VipPersist.jobguid = dbo.JobLog.guid WHERE     (dbo.JobLog.endTime < CONVERT(DATETIME, '" + date + "', 102))");
			db.Database.ExecuteSqlCommand("DELETE [dbo].[VIPPersistDetails] FROM [dbo].[VIPPersistDetails] INNER JOIN dbo.JobLog ON dbo.VIPPersistDetails.jobguid = dbo.JobLog.guid WHERE     (dbo.JobLog.endTime < CONVERT(DATETIME, '" + date + "', 102))");
			db.Database.ExecuteSqlCommand("DELETE [dbo].[VIPPoliciesReference] FROM [dbo].[VIPPoliciesReference] INNER JOIN dbo.JobLog ON dbo.VIPPoliciesReference.jobguid = dbo.JobLog.guid WHERE     (dbo.JobLog.endTime < CONVERT(DATETIME, '" + date + "', 102))");
			db.Database.ExecuteSqlCommand("DELETE [dbo].[VipProfilesDetails] FROM [dbo].[VipProfilesDetails] INNER JOIN dbo.JobLog ON dbo.VipProfilesDetails.jobguid = dbo.JobLog.guid WHERE     (dbo.JobLog.endTime < CONVERT(DATETIME, '" + date + "', 102))");
			db.Database.ExecuteSqlCommand("DELETE [dbo].[VIPProfilesReference] FROM [dbo].[VIPProfilesReference] INNER JOIN dbo.JobLog ON dbo.VIPProfilesReference.jobguid = dbo.JobLog.guid WHERE     (dbo.JobLog.endTime < CONVERT(DATETIME, '" + date + "', 102))");
			db.Database.ExecuteSqlCommand("DELETE [dbo].[VIPStats] FROM [dbo].[VIPStats] INNER JOIN dbo.JobLog ON dbo.VIPStats.jobguid = dbo.JobLog.guid WHERE     (dbo.JobLog.endTime < CONVERT(DATETIME, '" + date + "', 102))");
			db.Database.ExecuteSqlCommand("DELETE [dbo].[VLAN] FROM [dbo].[VLAN] INNER JOIN dbo.JobLog ON dbo.VLAN.jobguid = dbo.JobLog.guid WHERE     (dbo.JobLog.endTime < CONVERT(DATETIME, '" + date + "', 102))");
			db.Database.ExecuteSqlCommand("DELETE [dbo].[PoolMember] FROM [dbo].[PoolMember] INNER JOIN dbo.JobLog ON dbo.PoolMember. = dbo.JobLog.guid WHERE     (dbo.JobLog.endTime < CONVERT(DATETIME, '" + date + "', 102))");
			db.Database.ExecuteSqlCommand("DELETE [dbo].[PoolMembersReference] FROM [dbo].[PoolMembersReference] INNER JOIN dbo.JobLog ON dbo.PoolMembersReference. = dbo.JobLog.guid WHERE     (dbo.JobLog.endTime < CONVERT(DATETIME, '" + date + "', 102))");
			db.Database.ExecuteSqlCommand("DELETE [dbo].[VipPersist] FROM [dbo].[VipPersist] INNER JOIN dbo.JobLog ON dbo.VipPersist.jobguid = dbo.JobLog.guid WHERE     (dbo.JobLog.endTime < CONVERT(DATETIME, '" + date + "', 102))");
			db.Database.ExecuteSqlCommand("DELETE [dbo].[VIPStats] FROM [dbo].[VIPStats] INNER JOIN dbo.JobLog ON dbo.VIPStats.jobguid = dbo.JobLog.guid WHERE     (dbo.JobLog.endTime < CONVERT(DATETIME, '" + date + "', 102))");
			db.Database.ExecuteSqlCommand("DELETE [dbo].[IruleApiRawValues2] FROM [dbo].[IruleApiRawValues2] INNER JOIN dbo.JobLog ON dbo.[IruleApiRawValues2].jobguid = dbo.JobLog.guid WHERE     (dbo.JobLog.endTime < CONVERT(DATETIME, '" + date + "', 102))");
			db.Database.ExecuteSqlCommand("DELETE [dbo].[ClientSSLCertKeyChain] FROM [dbo].[ClientSSLCertKeyChain] INNER JOIN dbo.JobLog ON [dbo].[ClientSSLCertKeyChain].jobguid = dbo.JobLog.guid WHERE     (dbo.JobLog.endTime < CONVERT(DATETIME, '" + date + "', 102))");

			List<JobLog> alljobs = new List<JobLog>();
			using (AppF5DataEntities dc = new AppF5DataEntities())
			{
				alljobs = dc.JobLogs.OrderByDescending(a => a.endTime).ToList();
			}
			var data = alljobs;

			JsonNetResult jsonNetResult = new JsonNetResult();
			jsonNetResult.Formatting = Formatting.Indented;
			jsonNetResult.Data = data;

			return jsonNetResult;
		}
		public JsonNetResult DeleteJobsIndividuallyJSON(string guids)
		{
			JsonSerializerSettings settings = new JsonSerializerSettings
			{
				MissingMemberHandling = MissingMemberHandling.Ignore,
				NullValueHandling = NullValueHandling.Ignore
			};
			List<Guid> jobGuids = new List<Guid>();
			jobGuids = JsonConvert.DeserializeObject<List<Guid>>(guids, settings);
			foreach (var guid in jobGuids)
			{
				string jobguid = guid.ToString();
				AppF5DataEntities db = new AppF5DataEntities();
				db.Database.ExecuteSqlCommand("Delete  From Cert Where jobguid = '" + jobguid + "'");
				db.Database.ExecuteSqlCommand("Delete  From CertFile Where jobguid = '" + jobguid + "'");
				db.Database.ExecuteSqlCommand("Delete  From ClientSSL Where jobguid = '" + jobguid + "'");
				db.Database.ExecuteSqlCommand("Delete  From ClientSSLCertKeyChain Where jobguid = '" + jobguid + "'");
				db.Database.ExecuteSqlCommand("Delete  From DeviceData Where jobguid = '" + jobguid + "'");
				db.Database.ExecuteSqlCommand("Delete  From Irule Where jobguid = '" + jobguid + "'");
				db.Database.ExecuteSqlCommand("Delete  From IruleApiRawValues2 Where jobguid = '" + jobguid + "'");
				db.Database.ExecuteSqlCommand("Delete  From joblog Where guid = '" + jobguid + "'");
				db.Database.ExecuteSqlCommand("Delete  From JobLogTask Where parentjobguid = '" + jobguid + "'");
				db.Database.ExecuteSqlCommand("Delete  From MonitorHttp Where jobguid = '" + jobguid + "'");
				db.Database.ExecuteSqlCommand("Delete  From Node Where jobguid = '" + jobguid + "'");
				db.Database.ExecuteSqlCommand("Delete  From NodeFqdn Where jobguid = '" + jobguid + "'");
				db.Database.ExecuteSqlCommand("Delete  From Pool Where jobguid = '" + jobguid + "'");
				db.Database.ExecuteSqlCommand("Delete  From PoolMember Where jobguid = '" + jobguid + "'");
				db.Database.ExecuteSqlCommand("Delete  From PoolMembersReference Where jobguid = '" + jobguid + "'");
				db.Database.ExecuteSqlCommand("Delete  From Routes Where jobguid = '" + jobguid + "'");
				db.Database.ExecuteSqlCommand("Delete  From SelfIP Where jobguid = '" + jobguid + "'");
				db.Database.ExecuteSqlCommand("Delete  From SNATPool Where jobguid = '" + jobguid + "'");
				db.Database.ExecuteSqlCommand("Delete  From VIP Where jobguid = '" + jobguid + "'");
				db.Database.ExecuteSqlCommand("Delete  From VipPersist Where jobguid = '" + jobguid + "'");
				db.Database.ExecuteSqlCommand("Delete  From VIPPersistDetails Where jobguid = '" + jobguid + "'");
				db.Database.ExecuteSqlCommand("Delete  From VipProfilesDetails Where jobguid = '" + jobguid + "'");
				db.Database.ExecuteSqlCommand("Delete  From VIPProfilesReference Where jobguid = '" + jobguid + "'");
				db.Database.ExecuteSqlCommand("Delete  From VIPStats Where jobguid = '" + jobguid + "'");
				db.Database.ExecuteSqlCommand("Delete  From VLAN Where jobguid = '" + jobguid + "'");
				db.Database.ExecuteSqlCommand("Delete  From PoolMember Where jobguid = '" + jobguid + "'");
				db.Database.ExecuteSqlCommand("Delete  From PoolMembersReference Where jobguid = '" + jobguid + "'");
				db.Database.ExecuteSqlCommand("Delete  From VipPersist Where jobguid = '" + jobguid + "'");
				db.Database.ExecuteSqlCommand("Delete  From VIPStats Where jobguid = '" + jobguid + "'");
				db.Database.ExecuteSqlCommand("Delete  From IruleApiRawValues2 Where jobguid = '" + jobguid + "'");
				db.Database.ExecuteSqlCommand("Delete  From ClientSSLCertKeyChain Where jobguid = '" + jobguid + "'");


			}
			List<JobLog> alljobs = new List<JobLog>();
			using (AppF5DataEntities dc = new AppF5DataEntities())
			{
				alljobs = dc.JobLogs.OrderByDescending(a => a.endTime).ToList();
			}
			var data = alljobs;

			JsonNetResult jsonNetResult = new JsonNetResult();
			jsonNetResult.Formatting = Formatting.Indented;
			jsonNetResult.Data = data;

			return jsonNetResult;

		}
		public JsonResult GetOffLineVIPData()
		{
			//needs updating to ingore case when filtering ipv6


			AppF5DataEntities dc = new AppF5DataEntities();
			DateTime dtNow = DateTime.Now;
			JobLog jl = dc.JobLogs.OrderByDescending(a => a.endTime).FirstOrDefault();
			List<VIPSummaryClass> vipdata = VIPSummaryData(jl.guid);
			List<VIPSummaryClass> offlineVipData = vipdata.Where(a => a.StatusAvailabilityState == "offline").ToList();
			var ipv6List = offlineVipData.Where(a => a.VIPNAME.Contains("ipv6"));
			var filteredList = offlineVipData.Except(ipv6List);
			List<VIPSummaryClass> returnData = new List<VIPSummaryClass>();
			using (IDbConnection _db = new SqlConnection(ConfigurationManager.ConnectionStrings["f5dataConnectionString"].ConnectionString), commandTimeout = null)
			{
				var data = _db.Query<VIPStatsOverTime>(@"SELECT TOP (100) PERCENT dbo.JobLog.jobname, dbo.DeviceData.name AS f5Name, dbo.VIP.name AS VIPName, dbo.VIPStats.ClientsideBitsIn,  dbo.VIPStats.ClientsideBitsOut, dbo.VIPStats.ClientsideTotConns
                    FROM dbo.JobLog 
							INNER JOIN dbo.DeviceData ON dbo.JobLog.guid = dbo.DeviceData.jobguid 
							INNER JOIN dbo.VIP ON dbo.DeviceData.jobguid = dbo.VIP.jobguid AND dbo.DeviceData.name = dbo.VIP.f5HostName 
							INNER JOIN dbo.VIPStats ON dbo.VIP.guid = dbo.VIPStats.VIPGuid
							WHERE (dbo.DeviceData.selfDevice = N'True') AND (dbo.DeviceData.failoverState = N'Active')
							ORDER BY f5Name DESC, VIPName DESC, jobname").ToList();
				foreach (var item in filteredList)
				{
					item.VIPStats = data.Where(a => a.VIPName == item.VIPNAME && a.f5Name == item.f5Name).ToList();
					int changecount = 0;
					string baseconnnectioncount = "0";
					for (int i = 0; i < item.VIPStats.Count(); i++)
					{
						if (item.VIPStats[i].ClientsideTotConns != baseconnnectioncount)
						{
							baseconnnectioncount = item.VIPStats[i].ClientsideTotConns;
							changecount++;
						}
					}
					if (changecount <= 1)
					{
						returnData.Add(item);
					}

				}
			}


			return new JsonResult { Data = returnData, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}

		public class VIPStatsOverTime
		{
			public string jobname { get; set; }
			public string f5Name { get; set; }
			public string VIPName { get; set; }
			public string ClientsideBitsIn { get; set; }
			public string ClientsideBitsOut { get; set; }
			public string ClientsideTotConns { get; set; }
		}
		public JsonResult EmailCertData()
		{
			AppF5DataEntities dc = new AppF5DataEntities();
			JobLog jl = dc.JobLogs.OrderByDescending(a => a.endTime).FirstOrDefault();
			var data = SSLCertData(jl.guid);
			sendcertmail(data);

			return new JsonResult { Data = "alljob", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}

		private void sendcertmail(List<CertDataConvertedDate> data)
		{
			AppNetworkCafeEntities db = new AppNetworkCafeEntities();
			var user = SessionService.CurrentUser;

			string dstAddress = user.Email;
			string messageBody = "The following are the records: <br><br>";
			MailAddress to = new MailAddress(dstAddress);
			MailAddress from = new MailAddress("cmsnetworkeng@hpe.com");
			MailMessage mail = new MailMessage(from, to);
			mail.Subject = "SSL Cert Data from f5s";
			mail.IsBodyHtml = true;
			string htmlTableStart = "<table style=\"border-collapse:collapse; text-align:center; align=left; border=2px; cellpadding=3px; cellspacing=3px\" >";
			string htmlTableEnd = "</table>";
			string htmlHeaderRowStart = "<tr style =\"background-color:#6FA1D2; color:#ffffff; padding:5px; border: 1px;border-style: solid;border-color: #666666;\">";
			string htmlHeaderRowEnd = "</tr>";
			string htmlTrStart = "<tr style =\"color:#555555; padding:5px; border:2px; border-color:Black\">";
			string htmlTrEnd = "</tr>";
			string htmlTdStart = "<td style=\" border-color:#5c87b2; border-style:solid; border-width:thin; padding: 5px;\">";
			string htmlTdEnd = "</td>";

			messageBody += htmlTableStart;
			messageBody += htmlHeaderRowStart;
			messageBody += string.Format("<th>F5 Device</th><th>VIP Name</th><th>Profile Name</th><th>CN Name</th><th>Alt Name</th><th>Chain</th><th>Expiration Date</th>");
			messageBody += htmlHeaderRowEnd;


			foreach (var item in data)
			{
				messageBody += htmlTrStart + string.Format("{0}{2}{1}{0}{3}{1}{0}{4}{1}{0}{5}{1}{0}{6}{1}{0}{7}{1}{0}{8}{1}", htmlTdStart, htmlTdEnd, item.f5HostName, item.VIPName, item.ProfileName, item.subject, item.subjectAlternativeName, item.chain, item.experationDate) + htmlTrEnd;
			}
			messageBody += htmlTableEnd;
			mail.Body = messageBody;
			SmtpClient smtp = new SmtpClient();
			smtp.Host = "mailhost.rdcms.eds.com";
			smtp.Port = 25;
			smtp.Send(mail);
		}

		private void sendemail()
		{

			//string messageBody = "<font>The following are the records: </font><br><br>";
			//MailAddress to = new MailAddress(dstAddress);
			//MailAddress from = new MailAddress("cmsnetworkeng@hpe.com");
			//MailMessage mail = new MailMessage(from, to);
			//mail.Subject = "End Device Connectivity Report";
			//mail.IsBodyHtml = true;
			//string htmlTableStart = "<table style=\"border-collapse:collapse; text-align:center; align=left; border=2px; cellpadding=3px; cellspacing=3px\" >";
			//string htmlTableEnd = "</table>";
			//string htmlHeaderRowStart = "<tr style =\"background-color:#6FA1D2; color:#ffffff; padding:5px; border: 1px;border-style: solid;border-color: #666666;\">";
			//string htmlHeaderRowEnd = "</tr>";
			//string htmlTrStart = "<tr style =\"color:#555555; padding:5px; border:2px; border-color:Black\">";
			//string htmlTrEnd = "</tr>";
			//string htmlTdStart = "<td style=\" border-color:#5c87b2; border-style:solid; border-width:thin; padding: 5px;\">";
			//string htmlTdEnd = "</td>";

			//messageBody += htmlTableStart;
			//messageBody += htmlHeaderRowStart;
			//messageBody += string.Format("<th>Device Name</th><th>Device Location</th><th>PortNum</th><th>Host Name</th><th>Interface</th><th>VLAN</th><th>Comments</th>");
			//messageBody += htmlHeaderRowEnd;


			//foreach (var item in hostPorts)
			//{
			//    messageBody += htmlTrStart + string.Format("{0}{2}{1}{0}{3}{1}{0}{4}{1}{0}{5}{1}{0}{6}{1}{0}{7}{1}{0}{8}{1}", htmlTdStart, htmlTdEnd, item.DeviceName, item.DeviceLocation, item.PortNum, item.HostName, item.Interface, item.VLAN, item.Comments) + htmlTrEnd;
			//}
			//messageBody += htmlTableEnd;
			//mail.Body = messageBody;
			//SmtpClient smtp = new SmtpClient();
			//smtp.Host = "mailhost.rdcms.eds.com";
			//smtp.Port = 25;
			//smtp.Send(mail);
		}


		public JsonResult DeleteAllDataForJob(Guid jobguid)
		{
			AppF5DataEntities db = new AppF5DataEntities();
			string[] tables = new string[] {
					 "Cert",
					 "CertFile",
					 "ClientSSL",
					 "DeviceData",
					 "Irule",
					 "MonitorHttp",
					 "Node",
					 "Pool",
					 "VIP",
					 "VipPersist",
					 "VipPoliciesReference",
					 "VipProfilesDetail",
					 "VIPProfilesReference",
					 "VIPSourceAddressTranslation",
					 "VIPStats"
				};


			//Delete all rows from Cert Table
			//foreach (var item in tables)
			//{

			var sql = @"Delete from Cert Where jobguid = '" + jobguid + "'";
			//    db.Database.ExecuteSqlCommand(sql, jobguid, item);
			//}
			db.Database.ExecuteSqlCommand(sql);


			//return existing jobs after finishing deleting all data for job.
			List<JobLog> alljobs = new List<JobLog>();
			using (AppF5DataEntities dc = new AppF5DataEntities())
			{
				alljobs = dc.JobLogs.OrderByDescending(a => a.endTime).ToList();
			}
			return new JsonResult { Data = alljobs, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}

		public ActionResult DownloadExcelOffline()
		{
			var stream = ExportDataToExcel();
			var memoryStream = stream as MemoryStream;
			Response.Clear();
			Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
			Response.AddHeader("Content-disposition", "attachment; filename=OfflineVIP.xlsx");
			Response.BinaryWrite(memoryStream.ToArray());
			Response.End();
			return View();


		}
		public Stream ExportDataToExcel(Stream stream = null)
		{
			var user = SessionService.CurrentUser;

			//using (var db = new AppF5DataEntities())
			//{
			//    jl = db.JobLogs.OrderByDescending(a => a.endTime).FirstOrDefault();
			//}
			//using (IDbConnection _db = new SqlConnection(ConfigurationManager.ConnectionStrings["f5dataConnectionString"].ConnectionString), commandTimeout = null)
			//{
			//    List<OfflineVIPsForXLSX> listOffline = _db.Query<OfflineVIPsForXLSX>("SELECT TOP (100) PERCENT dbo.Apps.Application AS Environment, dbo.Apps.HostName AS f5, dbo.VIP.name AS VIPName,  VIP.destination AS VIPIP, VIP.pool, dbo.VIPStats.StatusAvailabilityState, dbo.VIPStats.StatusEnabledState, dbo.VIPStats.StatusStatusReason " +
			//        "FROM dbo.VIP INNER JOIN " +
			//             "dbo.DeviceData ON dbo.VIP.jobguid = dbo.DeviceData.jobguid AND dbo.VIP.f5HostName = dbo.DeviceData.name INNER JOIN " +
			//             "dbo.VIPStats ON dbo.VIP.guid = dbo.VIPStats.VIPGuid AND dbo.VIP.f5HostName = dbo.VIPStats.f5HostName INNER JOIN " +
			//             "dbo.Apps ON dbo.DeviceData.name = dbo.Apps.HostName " +
			//        "WHERE(dbo.DeviceData.failoverState = N'Active') AND(dbo.DeviceData.selfDevice = N'True') AND(dbo.VIPStats.StatusAvailabilityState = N'Offline') AND " +
			//             "(dbo.DeviceData.jobguid = '" + jl.guid + "') AND(dbo.Apps.Application NOT LIKE 'ALL') " +
			//        "ORDER BY dbo.Apps.Application").ToList();

			//    foreach (var item in listOffline)
			//    {
			//        item.VIPIP = item.VIPIP.Replace("/Common/", "");
			//        if (item.pool != null)
			//        {
			//            item.pool = item.pool.Replace("/Common/", "");
			//        }

			//    }
			AppF5DataEntities dc = new AppF5DataEntities();
			DateTime dtNow = DateTime.Now;
			JobLog jl = dc.JobLogs.OrderByDescending(a => a.endTime).FirstOrDefault();
			List<VIPSummaryClass> vipdata = VIPSummaryData(jl.guid);
			List<VIPSummaryClass> offlineVipData = vipdata.Where(a => a.StatusAvailabilityState == "offline").ToList();
			var ipv6List = offlineVipData.Where(a => a.VIPNAME.Contains("ipv6"));
			var filteredList = offlineVipData.Except(ipv6List);
			List<VIPSummaryClass> returnData = new List<VIPSummaryClass>();
			using (IDbConnection _db = new SqlConnection(ConfigurationManager.ConnectionStrings["f5dataConnectionString"].ConnectionString), commandTimeout = null)
			{
				var data = _db.Query<VIPStatsOverTime>(@"SELECT        TOP (100) PERCENT dbo.JobLog.jobname, dbo.DeviceData.name AS f5Name, dbo.VIP.name AS VIPName, dbo.VIPStats.ClientsideBitsIn, 
                         dbo.VIPStats.ClientsideBitsOut, dbo.VIPStats.ClientsideTotConns
                    FROM            dbo.JobLog INNER JOIN
                         dbo.DeviceData ON dbo.JobLog.guid = dbo.DeviceData.jobguid INNER JOIN
                         dbo.VIP ON dbo.DeviceData.jobguid = dbo.VIP.jobguid AND dbo.DeviceData.name = dbo.VIP.f5HostName INNER JOIN
                         dbo.VIPStats ON dbo.VIP.guid = dbo.VIPStats.VIPGuid
                    WHERE        (dbo.DeviceData.selfDevice = N'True') AND (dbo.DeviceData.failoverState = N'Active')
                    ORDER BY f5Name DESC, VIPName DESC, jobname").ToList();
				foreach (var item in filteredList)
				{
					item.VIPStats = data.Where(a => a.VIPName == item.VIPNAME && a.f5Name == item.f5Name).ToList();
					int changecount = 0;
					string baseconnnectioncount = "0";
					for (int i = 0; i < item.VIPStats.Count(); i++)
					{
						if (item.VIPStats[i].ClientsideTotConns != baseconnnectioncount)
						{
							baseconnnectioncount = item.VIPStats[i].ClientsideTotConns;
							changecount++;
						}
					}
					if (changecount <= 1)
					{
						returnData.Add(item);
					}

				}
			}
			using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
			{
				excelPackage.Workbook.Properties.Author = user.FirstName + " " + user.LastName;
				excelPackage.Workbook.Properties.Title = "Offline VIP Data";
				excelPackage.Workbook.Properties.Comments = "Offline VIP Data";

				excelPackage.Workbook.Worksheets.Add("Offline VIPs");
				var worksheet = excelPackage.Workbook.Worksheets[1];


				worksheet.Cells[1, 1].LoadFromCollection(returnData, true);

				excelPackage.Save();
				return excelPackage.Stream;
			}
		}

		class OfflineVIPsForXLSX
		{
			public string Environment { get; set; }
			public string f5 { get; set; }
			public string VIPName { get; set; }
			public string VIPIP { get; set; }
			public string pool { get; set; }
			public string StatusAvailabilityState { get; set; }
			public string StatusEnabledState { get; set; }
			public string StatusStatusReason { get; set; }

		}

	}
}

