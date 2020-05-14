using Newtonsoft.Json;
using SourceControl.Common;
using SourceControl.Models;
using SourceControl.Models.Db;
using SourceControl.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace SourceControl.Controllers
{
    public class AppController : Controller
    {
        [HttpPost]
        public string RMA(int deviceId)
        {
            try
            {
                using (TargetEntities targetDb = new TargetEntities())
                {
                    targetDb.Database.Connection.ConnectionString = "data source=localhost;initial catalog=CMDB;persist security info=True;Trusted_Connection=Yes;MultipleActiveResultSets=True;";

                    var deviceStatusId = 5; // RMA'd
                    var doD = "getdate()";  // Dod date
                    var rma = "?????";  //
                    var sql = "INSERT INTO Device(HostName, MfgId, Serial, DeviceSiteId, PoInfoId, MgtIp, DeviceModelId, DeviceTypeId, RackId, DoB, IsVirtual, Wlp, DeviceStatusId, RuNumber, DoD, FismaTag, ProvisionNnmi, ProvisionNetworkAutomation, ProvisionSolarWind, ProvisionAlgosecFiremon, ProvisionNetBrain, DeviceNote, DeviceGroups, RMA, DOMSSerial, DOMSRack, DOMSRU, SupportStart, SupportEnd, SupportContract, EOLDate) ";
                    sql += "SELECT HostName, MfgId, Serial, DeviceSiteId, PoInfoId, MgtIp, DeviceModelId, DeviceTypeId, RackId, DoB, IsVirtual, Wlp, " + deviceStatusId + ", RuNumber, " + doD + ", FismaTag, ProvisionNnmi, ProvisionNetworkAutomation, ProvisionSolarWind, ProvisionAlgosecFiremon, ProvisionNetBrain, DeviceNote, DeviceGroups,'" + rma + "', DOMSSerial, DOMSRack, DOMSRU, SupportStart, SupportEnd, SupportContract, EOLDate ";
                    sql += "FROM Device WHERE DeviceId = " + deviceId;

                    targetDb.Database.ExecuteSqlCommand(sql);

                    return "";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

        public string ClearError()
        {
            return Helper.ClearError();
        }

        public string GetF5SummaryData()
        {
            try
            {
                using (AppF5DataEntities db = new AppF5DataEntities())
                {
                    DateTime dtnow = DateTime.Now;

                    JobLog jl = db.JobLogs.OrderByDescending(a => a.endTime).FirstOrDefault();
                    //Cert Data
                    var certdata = GetCerts_(jl.guid);
                    int TotalCertCount = certdata.Count();
                    int expiredCertCount = certdata.Where(a => a.expiration < dtnow).Count();
                    int expiringOneMonthCount = certdata.Where(a => a.expiration > dtnow && a.expiration < dtnow.AddMonths(1)).Count();
                    int expiringThreeMonthCount = certdata.Where(a => a.expiration > dtnow && a.expiration < dtnow.AddMonths(3)).Count();
                    int expiringSixMonthCount = certdata.Where(a => a.expiration > dtnow && a.expiration < dtnow.AddMonths(6)).Count();

                    //Vip data
                    List<VIPSummaryClass> vipdata = VIPSummaryData(jl.guid);

                    int availableVipCount = vipdata.Where(a => a.StatusAvailabilityState == "available").Select(s => s.StatusCount).FirstOrDefault();
                    int unknownVipCount = vipdata.Where(a => a.StatusAvailabilityState == "unknown").Select(s => s.StatusCount).FirstOrDefault();
                    int offlineVipCount = vipdata.Where(a => a.StatusAvailabilityState == "offline").Select(s => s.StatusCount).FirstOrDefault();
                    int totalVIPCount = availableVipCount + unknownVipCount + offlineVipCount;

                    //Node Data  
                    var sql = string.Format(@" 
						SELECT COUNT(1) AS RecordCount 
						FROM PoolMember INNER JOIN 
						Pool ON PoolMember.PoolGuid = Pool.guid INNER JOIN 
						JobLog INNER JOIN 
						DeviceData ON JobLog.guid = DeviceData.jobguid INNER JOIN 
						VIP ON DeviceData.jobguid = VIP.jobguid AND DeviceData.name = VIP.f5HostName ON Pool.fullpath = VIP.pool AND 
						dbo.Pool.jobguid = dbo.VIP.jobguid 
						WHERE(dbo.DeviceData.selfDevice = N'true') AND (dbo.DeviceData.failoverState = N'active') AND (dbo.JobLog.guid = '{0}')
					", jl.guid);

                    var totalPoolMembers = db.Database.SqlQuery<int>(sql).FirstOrDefault();

                    sql = string.Format(@" 
						SELECT COUNT(1) AS RecordCount 
						FROM PoolMember INNER JOIN 
						Pool ON PoolMember.PoolGuid = Pool.guid INNER JOIN 
						JobLog INNER JOIN 
						DeviceData ON JobLog.guid = DeviceData.jobguid INNER JOIN 
						VIP ON DeviceData.jobguid = VIP.jobguid AND DeviceData.name = VIP.f5HostName ON Pool.fullpath = VIP.pool AND 
						dbo.Pool.jobguid = dbo.VIP.jobguid 
						WHERE(dbo.DeviceData.selfDevice = N'true') AND (dbo.DeviceData.failoverState = N'active') AND (dbo.JobLog.guid = '{0}') AND PoolMember.state = 'up'
					", jl.guid);

                    int activePoolMembers = db.Database.SqlQuery<int>(sql).FirstOrDefault();

                    //Device Data
                    int devicecount = db.MasterDeviceLists.Count();

                    //pool Data
                    var poolcount = db.Database.SqlQuery<int>(string.Format(@"
							SELECT COUNT(1) FROM (
							SELECT distinct Pool.name
							FROM Pool INNER JOIN 
							JobLog INNER JOIN 
							DeviceData ON JobLog.guid = DeviceData.jobguid INNER JOIN 
							VIP ON DeviceData.jobguid = VIP.jobguid AND DeviceData.name = VIP.f5HostName ON Pool.jobguid = VIP.jobguid AND 
							Pool.f5HostName = VIP.f5HostName AND Pool.fullpath = VIP.pool 
							WHERE(dbo.DeviceData.selfDevice = N'true') AND(dbo.DeviceData.failoverState = N'active') AND (dbo.JobLog.guid = '{0}')
							) T1
						", jl.guid)).FirstOrDefault();

                    //rule Data
                    int rulecount = db.Database.SqlQuery<int>("SELECT COUNT(1) FROM (SELECT Distinct name FROM Irule Where jobguid = '" + jl.guid + "') T1").FirstOrDefault();
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

                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                    return json;
                }

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }



        //StatusCount
        private List<VIPSummaryClass> VIPSummaryData(Guid guid)
        {
            using (AppF5DataEntities db = new AppF5DataEntities())
            {
                List<VIPSummaryClass> vipdata = db.Database.SqlQuery<VIPSummaryClass>(string.Format(@"
									SELECT VIPStats.StatusAvailabilityState, COUNT(1) AS StatusCount 
									FROM DeviceData 
									INNER JOIN  JobLog ON DeviceData.jobguid = JobLog.guid 
									INNER JOIN  VIP ON DeviceData.jobguid = VIP.jobguid AND DeviceData.hostname = VIP.f5HostName 
									INNER JOIN  VIPStats ON VIP.guid = VIPStats.VIPGuid 
									WHERE (DeviceData.failoverState = N'active') AND (JobLog.guid = '{0}') AND (DeviceData.selfDevice = N'true') GROUP BY VIPStats.StatusAvailabilityState
									", guid)).ToList();
                return vipdata;
            }
        }
        //private List<VIPSummaryClass> VIPSummaryData(Guid guid)
        //{
        //	using (AppF5DataEntities db = new AppF5DataEntities())
        //	{
        //		List<VIPSummaryClass> vipdata = db.Database.SqlQuery<VIPSummaryClass>(string.Format(@"
        //							SELECT DeviceData.Name As f5Name, VIP.name As VIPNAME,  REPLACE(VIP.destination,'/Common/','') AS VIPIP, REPLACE(VIP.pool,'/Common/','') AS pool, VIPStats.StatusAvailabilityState, VIPStats.StatusEnabledState , VIPStats.StatusStatusReason 
        //							FROM DeviceData 
        //							INNER JOIN  JobLog ON DeviceData.jobguid = JobLog.guid 
        //							INNER JOIN  VIP ON DeviceData.jobguid = VIP.jobguid AND DeviceData.hostname = VIP.f5HostName 
        //							INNER JOIN  VIPStats ON VIP.guid = VIPStats.VIPGuid 
        //							WHERE (DeviceData.failoverState = N'active') AND (JobLog.guid = '{0}') AND (DeviceData.selfDevice = N'true')
        //							", guid)).ToList();
        //		return vipdata;
        //	}
        //}

        public string EmailCertData()
        {
            try
            {
                using (AppF5DataEntities dc = new AppF5DataEntities())
                {
                    JobLog jl = dc.JobLogs.OrderByDescending(a => a.endTime).FirstOrDefault();
                    var data = GetCerts_(jl.guid);
                    return SendCertEmail(data);
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private string SendCertEmail(List<CertData> data)
        {
            try
            {
                // no-reply-cmsnetworktoolbox@entsvcscms.com  cmsnetworktools@uspsector.com
                AppUser user = SessionService.CurrentUser;

                var fromAddress = ConfigurationManager.AppSettings["EmailFromAddress"].ToString();

                string messageBody = "The following are the records: <br><br>";
                MailAddress to = new MailAddress(user.Email);
                MailAddress from = new MailAddress(fromAddress);
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
                    messageBody += htmlTrStart + string.Format("{0}{2}{1}{0}{3}{1}{0}{4}{1}{0}{5}{1}{0}{6}{1}{0}{7}{1}{0}{8}{1}", htmlTdStart, htmlTdEnd, item.f5HostName, item.VIPName, item.ProfileName, item.subject, item.subjectAlternativeName, item.chain, item.expiration) + htmlTrEnd;
                }
                messageBody += htmlTableEnd;
                mail.Body = messageBody;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "mailhost.rdcms.eds.com";
                smtp.Port = 25;
                smtp.Send(mail);
                return "Email sent to " + user.Email;
            }
            catch (Exception ex)
            {
                return "Error sending email " + ex.Message;

            }



        }



        public string GetOfflineVIPOverTime(string f5Name, string vipName)
        {

            using (AppF5DataEntities _db = new AppF5DataEntities())
            {
                var recs = _db.Database.SqlQuery<VIPStatsOverTime>(@"
					SELECT dbo.JobLog.jobname, dbo.DeviceData.name AS f5Name, dbo.VIP.name AS VIPName, dbo.VIPStats.ClientsideBitsIn, 
					dbo.VIPStats.ClientsideBitsOut, dbo.VIPStats.ClientsideTotConns
					FROM  dbo.JobLog INNER JOIN
					dbo.DeviceData ON dbo.JobLog.guid = dbo.DeviceData.jobguid INNER JOIN
					dbo.VIP ON dbo.DeviceData.jobguid = dbo.VIP.jobguid AND dbo.DeviceData.name = dbo.VIP.f5HostName INNER JOIN
					dbo.VIPStats ON dbo.VIP.guid = dbo.VIPStats.VIPGuid
					WHERE  (dbo.DeviceData.selfDevice = N'True') AND (dbo.DeviceData.failoverState = N'Active') AND dbo.DeviceData.name = '" + f5Name.Replace("'", "''") + @"' AND dbo.VIP.name = '" + vipName.Replace("'", "''") + @"' ORDER BY f5Name DESC, VIPName DESC, jobname

				").ToList();



                var json = Newtonsoft.Json.JsonConvert.SerializeObject(recs);
                return json;

            }

        }


        public string GetOfflineVIP()
        {

            using (AppF5DataEntities _db = new AppF5DataEntities())
            {
                var recs = _db.Database.SqlQuery<OfflineVip>(@"

								SELECT Activef5Name, ActiveVIPName, StatusAvailabilityState, StatusEnabledState, StatusStatusReason, VIPIP, pool, ClientsideTotConns FROM (

								SELECT * FROM (
								SELECT DeviceData.Name As f5Name, VIP.name As VIPNAME,  VIP.destination AS VIPIP, VIP.pool, VIPStats.StatusAvailabilityState, VIPStats.StatusEnabledState , VIPStats.StatusStatusReason 
								FROM DeviceData INNER JOIN 
								JobLog ON DeviceData.jobguid = JobLog.guid INNER JOIN 
								VIP ON DeviceData.jobguid = VIP.jobguid AND DeviceData.hostname = VIP.f5HostName INNER JOIN 
								VIPStats ON VIP.guid = VIPStats.VIPGuid 
								WHERE (DeviceData.failoverState = N'active') 
								AND (JobLog.guid IN (SELECT TOP 1 guid FROM JobLog ORDER BY endTime)) 
								AND (DeviceData.selfDevice = N'true')
								AND VIPStats.StatusAvailabilityState = 'offline'
								AND VIPName NOT LIKE '%ipv6%'
								) T1

								JOIN (

								SELECT dbo.JobLog.jobname, dbo.DeviceData.name AS Activef5Name, dbo.VIP.name AS ActiveVIPName, dbo.VIPStats.ClientsideBitsIn, 
																 dbo.VIPStats.ClientsideBitsOut, dbo.VIPStats.ClientsideTotConns
														  FROM  dbo.JobLog INNER JOIN
																 dbo.DeviceData ON dbo.JobLog.guid = dbo.DeviceData.jobguid INNER JOIN
																 dbo.VIP ON dbo.DeviceData.jobguid = dbo.VIP.jobguid AND dbo.DeviceData.name = dbo.VIP.f5HostName INNER JOIN
																 dbo.VIPStats ON dbo.VIP.guid = dbo.VIPStats.VIPGuid
														  WHERE  (dbo.DeviceData.selfDevice = N'True') AND (dbo.DeviceData.failoverState = N'Active')

								) T2 
								ON T1.VIPNAME = T2.ActiveVIPName AND T1.f5Name = t2.Activef5Name
								) T3 

								ORDER BY Activef5Name DESC, ActiveVIPName DESC, jobname

				").ToList();

                List<OfflineVip> data = new List<OfflineVip>();
                int changeCount = 0;
                string clientsideTotConns = "";
                string f5Name = "";
                string vipName = "";

                if (recs.Count > 0)
                {
                    clientsideTotConns = recs[0].ClientsideTotConns;
                    f5Name = recs[0].Activef5Name;
                    vipName = recs[0].ActiveVIPName;
                }

                foreach (var rec in recs)
                {

                    if (rec.ClientsideTotConns != clientsideTotConns)
                    {
                        clientsideTotConns = rec.ClientsideTotConns;
                        changeCount++;
                    }

                    if (rec.Activef5Name != f5Name && rec.ActiveVIPName != vipName)
                    {
                        if (changeCount <= 1)
                        {
                            data.Add(rec);
                        }

                        clientsideTotConns = rec.ClientsideTotConns;
                        f5Name = rec.Activef5Name;
                        vipName = rec.ActiveVIPName;
                        changeCount = 0;
                    }

                }


                var json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                return json;

            }



        }

        public string GetCerts(Guid jobGuid)
        {
            var data = GetCerts_(jobGuid);
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            return json;
        }

        public List<CertData> GetCerts_(Guid jobGuid)
        {
            using (AppF5DataEntities _db = new AppF5DataEntities())
            {

                var exe = string.Format(@"
                        SELECT VipProfilesDetails.f5HostName, VIP.name AS VIPName, VipProfilesDetails.name AS ProfileName, cast(REPLACE(CertFile.expiration,'GMT','') as datetime) AS expiration, CertFile.subject,
                        CertFile.subjectAlternativeName, ClientSSLCertKeyChain.chain 
                        FROM CertFile INNER JOIN 
                        ClientSSL ON CertFile.jobguid = ClientSSL.jobguid AND CertFile.f5HostName = ClientSSL.f5HostName AND 
                        CertFile.fullPath = ClientSSL.cert INNER JOIN 
                        ClientSSLCertKeyChain ON ClientSSL.certKeyChainGuid = ClientSSLCertKeyChain.CertGuid LEFT OUTER JOIN 
                        VIP INNER JOIN 
                        VipProfilesDetails ON VIP.guid = VipProfilesDetails.vipguid INNER JOIN 
                        JobLog INNER JOIN 
                        DeviceData ON JobLog.guid = DeviceData.jobguid ON VIP.jobguid = DeviceData.jobguid AND VIP.f5HostName = DeviceData.name ON 
                        ClientSSL.jobguid = VipProfilesDetails.jobguid AND ClientSSL.f5HostName = VipProfilesDetails.f5HostName AND 
                        ClientSSL.fullPath = VipProfilesDetails.fullpath 
                        WHERE(NOT(VIP.name LIKE N'%ipv6%')) AND(DeviceData.selfDevice = N'true') AND (DeviceData.failoverState = N'active') AND (JobLog.guid = '{0}')
                ", jobGuid);

                List <CertData> certlist = _db.Database.SqlQuery<CertData>(exe).ToList();
                return certlist;
            }
        }

        public string GetSelfIps()
        {
            using (AppF5DataEntities _db = new AppF5DataEntities())
            {
                List<SelfIp> recs = _db.Database.SqlQuery<SelfIp>(@"SELECT  TOP (100) PERCENT dbo.DeviceData.name, dbo.SelfIP.address, dbo.SelfIP.name AS interfaceName, dbo.SelfIP.floating, dbo.SelfIP.vlan, dbo.VLAN.mtu, dbo.VLAN.tag
								FROM            dbo.DeviceData INNER JOIN
								dbo.SelfIP ON dbo.DeviceData.jobguid = dbo.SelfIP.jobguid AND dbo.DeviceData.name = dbo.SelfIP.f5HostName INNER JOIN
								dbo.VLAN ON dbo.SelfIP.vlan = dbo.VLAN.name AND dbo.SelfIP.f5HostName = dbo.VLAN.f5Hostname AND dbo.SelfIP.jobguid = dbo.VLAN.jobguid
								WHERE        (dbo.DeviceData.selfDevice = N'true') AND (NOT (dbo.SelfIP.name LIKE N'%peer%') AND NOT (dbo.SelfIP.name LIKE N'%ipv6%')) AND (dbo.DeviceData.jobguid =
								(SELECT        TOP (1) guid
								FROM            dbo.JobLog
								WHERE        (Successful = N'Finished')
								ORDER BY endTime DESC))
								ORDER BY dbo.DeviceData.name, dbo.SelfIP.address, interfaceName").ToList();

                var json = Newtonsoft.Json.JsonConvert.SerializeObject(recs);
                return json;
            }
        }

        public string GetDebugLog()
        {
            var recs = Helper.GetDebugLog();
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(recs);
            return json;
        }

        public string GetProcessStatus()
        {
            try
            {
                using (AppNetworkCafeEntities db = new AppNetworkCafeEntities())
                {
                    var f5ServerPath = @"D:\Services\F5ReportService\bin\Debug\Logs\AreYouAlive.txt";

                    if (!System.IO.File.Exists(f5ServerPath))
                    {
                        using (StreamWriter outfile = new StreamWriter(f5ServerPath))
                        {
                            outfile.Write("check");
                        }

                        Thread.Sleep(5000);

                    }

                    var log = db.DebugLogs.Where(w => w.Source == "F5DownloadService" && w.LogContent == "F5DownloadService is alive").FirstOrDefault();
                    if (log != null)
                    {
                        db.Database.ExecuteSqlCommand("DELETE FROM DebugLog WHERE LogContent = 'F5DownloadService is alive'");
                        db.SaveChanges();
                        return "Running";
                    }

                    return "Not Running";

                }
            }
            catch (Exception)
            {
                return "Cannot Connect";
            }

        }


        public string GetCompareSnapshots(string firstSnapshot, string secondSnapshot)
        {
            List<compareResults> firstJobList = new List<compareResults>();

            using (AppF5DataEntities dc = new AppF5DataEntities())
            {
                string firstsqlQuery = string.Format(@"SELECT TOP (100) PERCENT dbo.DeviceData.name AS LoadBalancer, dbo.VIP.name AS VIPNAME, dbo.VIPStats.StatusAvailabilityState, dbo.VIPStats.StatusEnabledState, 
											dbo.VIPStats.StatusStatusReason, dbo.VIP.pool AS PoolName
											FROM dbo.VIP INNER JOIN
											dbo.DeviceData ON dbo.VIP.jobguid = dbo.DeviceData.jobguid AND dbo.VIP.f5HostName = dbo.DeviceData.name INNER JOIN
											dbo.VIPStats ON dbo.VIP.jobguid = dbo.VIPStats.jobguid AND dbo.VIP.guid = dbo.VIPStats.VIPGuid INNER JOIN
											dbo.JobLog ON dbo.DeviceData.jobguid = dbo.JobLog.guid
											WHERE(dbo.DeviceData.selfDevice = N'true') AND(dbo.DeviceData.failoverState = N'active') AND(dbo.JobLog.guid = '{0}') AND
											(NOT(dbo.VIP.name LIKE N'%ipv6%'))
											ORDER BY LoadBalancer, VIPNAME", firstSnapshot);


                string secondSqlQuery = string.Format(@"SELECT TOP (100) PERCENT dbo.DeviceData.name AS LoadBalancer, dbo.VIP.name AS VIPNAME, dbo.VIPStats.StatusAvailabilityState, dbo.VIPStats.StatusEnabledState, 
											dbo.VIPStats.StatusStatusReason, dbo.VIP.pool AS PoolName
											FROM  dbo.VIP INNER JOIN
											dbo.DeviceData ON dbo.VIP.jobguid = dbo.DeviceData.jobguid AND dbo.VIP.f5HostName = dbo.DeviceData.name INNER JOIN
											dbo.VIPStats ON dbo.VIP.jobguid = dbo.VIPStats.jobguid AND dbo.VIP.guid = dbo.VIPStats.VIPGuid INNER JOIN
											dbo.JobLog ON dbo.DeviceData.jobguid = dbo.JobLog.guid
											WHERE(dbo.DeviceData.selfDevice = N'true') AND(dbo.DeviceData.failoverState = N'active') AND(dbo.JobLog.guid = '{0}') AND
											(NOT(dbo.VIP.name LIKE N'%ipv6%'))
											ORDER BY LoadBalancer, VIPNAME", secondSnapshot);

                var job1List = dc.Database.SqlQuery<compareResults>(firstsqlQuery).ToList();

                var job2List = dc.Database.SqlQuery<compareResults>(secondSqlQuery).ToList();

                foreach (var item in job1List)
                {
                    int count = job2List.Where(a => a.LoadBalancer == item.LoadBalancer && a.StatusAvailabilityState == item.StatusAvailabilityState && a.StatusEnabledState == item.StatusEnabledState && a.StatusStatusReason == item.StatusStatusReason && a.VIPName == item.VIPName).Count();
                    if (count == 0)
                    {
                        firstJobList.Add(item);
                    }
                }

            }

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(firstJobList);
            return json;
        }


        public string GetComparePoolMembers(string loadBalancer, string poolName, string firstSnapshot)
        {

            using (AppF5DataEntities dc = new AppF5DataEntities())
            {
                var poolMembers = dc.Database.SqlQuery<WeeklyPoolMembersDataFormat>(@"SELECT dbo.PoolMember.Name AS ServerName, dbo.PoolMember.address AS ServerAddress, dbo.PoolMember.monitor AS ServerMonitor, dbo.PoolMember.state AS ServerState, 
						dbo.PoolMember.session AS ServerSession
						FROM  dbo.JobLog INNER JOIN
						dbo.DeviceData ON dbo.JobLog.guid = dbo.DeviceData.jobguid INNER JOIN
						dbo.Pool ON dbo.DeviceData.name = dbo.Pool.f5HostName AND dbo.DeviceData.jobguid = dbo.Pool.jobguid INNER JOIN
						dbo.PoolMember ON dbo.Pool.guid = dbo.PoolMember.PoolGuid
						WHERE  (dbo.DeviceData.name = N'" + loadBalancer + "') AND (dbo.DeviceData.selfDevice = N'true') AND (dbo.JobLog.guid = '" + firstSnapshot + "') AND (dbo.DeviceData.failoverState = N'active') AND (dbo.Pool.fullpath = N'" + poolName + " ') ORDER BY ServerName").ToList();
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(poolMembers);
                return json;
            }

        }



        public string GetVirtualPoolMembers(string poolGuid)
        {
            if (poolGuid.Length < 3) return "";
            using (AppF5DataEntities targetDb = new AppF5DataEntities())
            {
                var guid = new Guid(poolGuid);
                var recs = targetDb.PoolMembers.Where(a => a.PoolGuid == guid).ToList();

                if (recs != null)
                {
                    foreach (var item in recs)
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
                }

                var json = Newtonsoft.Json.JsonConvert.SerializeObject(recs);
                return json;
            }
        }
        public string GetJobs()
        {
            using (AppF5DataEntities targetDb = new AppF5DataEntities())
            {
                var recs = targetDb.JobLogs.OrderByDescending(a => a.endTime).Select(s => new { TextField = s.jobname, ValueField = s.guid }).ToList();
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(recs);
                return json;
            }
        }

        public string GetEnvironments()
        {
            using (AppF5DataEntities targetDb = new AppF5DataEntities())
            {
                var recs = targetDb.Database.SqlQuery<ValueText>("SELECT DISTINCT Application AS ValueField, Application AS TextField FROM Apps ORDER BY Application").ToList(); //targetDb.DeviceDatas.Where(a => a.selfDevice == "true" && a.jobguid == guid && a.failoverState == "active").OrderBy(a => a.hostname).Select(s => new { TextField = s.name, ValueField = s.name }).ToList();
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(recs);
                return json;
            }
        }

        //?jobGuid=" + jobGuid + "&environment=" + environment;
        public string GetDevices(Guid jobGuid, string environment)
        {
            using (AppF5DataEntities targetDb = new AppF5DataEntities())
            {

                if (environment == "All")
                {
                    var selfDeviceList = targetDb.DeviceDatas.Where(a => a.selfDevice == "true" && a.jobguid == jobGuid && a.failoverState == "active").OrderBy(a => a.hostname).Select(s => new { TextField = s.name, ValueField = s.name }).ToList();
                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(selfDeviceList);
                    return json;
                }
                else
                {

                    var selfDeviceList = new List<ValueText>();
                    var envDevices = targetDb.Apps.Where(a => a.Application == environment).ToList();
                    foreach (var item in envDevices)
                    {

                        var d = targetDb.DeviceDatas.Where(a => a.selfDevice == "true" && a.jobguid == jobGuid && a.failoverState == "active" && a.name.Contains(item.HostName)).Select(s => new { TextField = s.name, ValueField = s.name }).FirstOrDefault();
                        if (d != null)
                        {
                            selfDeviceList.Add(new ValueText { TextField = d.TextField, ValueField = d.ValueField });
                        }
                    }
                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(selfDeviceList);
                    return json;
                }

            }
        }


        public string GetVipsForDevice(Guid jobGuid, string f5HostName) //{86a0f707-94ec-491f-8035-026836fe5533}  CMSTGTMM01.TEST.TKO.com
        {

            using (AppF5DataEntities dc = new AppF5DataEntities())
            {
                var recs = dc.VIPs.Where(a => a.jobguid == jobGuid && a.f5HostName == f5HostName).OrderBy(a => a.name).ToList();
                if (recs != null)
                {
                    foreach (var Vip in recs)
                    {
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
                    }
                }



                var json = Newtonsoft.Json.JsonConvert.SerializeObject(recs);
                return json;
            }
        }



        //
        public string GetContract(string guid)
        {
            string json = "";

            using (TargetEntities targetDb = new TargetEntities("NetworkCafeEntities"))
            {
                StringBuilder sb = new StringBuilder();
                var sql = @"SELECT top 1 * FROM ContractV2 WHERE guid = @guid FOR JSON AUTO, INCLUDE_NULL_VALUES";
                var recs = targetDb.Database.SqlQuery<string>(sql, new SqlParameter("guid", guid)).ToList();
                foreach (var rec in recs)
                {
                    sb.Append(rec);
                }
                var result = sb.ToString();
                if (result.Length > 1)
                {
                    result = result.Substring(1, result.Length - 1);
                    result = result.Substring(0, result.Length - 1);
                }
                json = result;

            }

            return json;
        }

        public string GetNetworkInformationContracts(string deviceVendor)
        {
            string json = "";

            using (TargetEntities targetDb = new TargetEntities("NetworkCafeEntities"))
            {

                var sql = @"SELECT MasterDevice.HostName, ContractV2.guid, ContractV2.licenses, ContractV2.startDate, ContractV2.notes,ContractV2.endDate, ContractV2.contractNumber, ContractV2.poNumber,
									    MasterDevice.Site, MasterDevice.DeviceModel, MasterDevice.DeviceVendor, MasterDevice.DeviceLocation, MasterDevice.SerialNumber, MasterDevice.DeviceGUID
								FROM MasterDevice LEFT OUTER JOIN ContractV2 ON MasterDevice.DeviceGUID = ContractV2.deviceGuid
								WHERE ContractV2.guid is not null AND DeviceVendor = '" + deviceVendor + "'";

                var recs = targetDb.Database.SqlQuery<ContractData>(sql).ToList();

                json = Newtonsoft.Json.JsonConvert.SerializeObject(recs);

            }

            return json;
        }

        public string GetDeviceVendors()
        {
            string json = "";

            using (TargetEntities targetDb = new TargetEntities("NetworkCafeEntities"))
            {

                var sql = @"SELECT Distinct DeviceVendor AS TextField, DeviceVendor AS ValueField FROM MasterDevice ORDER BY DeviceVendor";

                var recs = targetDb.Database.SqlQuery<ValueText>(sql).ToList();

                json = Newtonsoft.Json.JsonConvert.SerializeObject(recs);

            }

            return json;
        }

        [HttpPost]
        public string UpdateNetworkInformationContracts(string contractData)
        {
            var rec = JsonConvert.DeserializeObject<ContractData>(contractData);

            var sql = @"UPDATE ContractV2 SET licenses = '" + Helper.ToDbString(rec.licenses) + "', notes = '" + Helper.ToDbString(rec.notes) + "',contractNumber = '" + Helper.ToDbString(rec.contractNumber) + "', startDate = " + Helper.ToDbDateTime(rec.startDate) + ", endDate = " + Helper.ToDbDateTime(rec.endDate) + ", poNumber = '" +

                 Helper.ToDbString(rec.poNumber) + "' WHERE guid = '" + rec.guid + "'";

            using (TargetEntities targetDb = new TargetEntities("NetworkCafeEntities"))
            {

                targetDb.Database.ExecuteSqlCommand(sql);
                targetDb.SaveChanges();
            }

            return contractData;

        }


    }

    public class WeeklyPoolMembersDataFormat
    {
        public string ServerName { get; set; }
        public string ServerAddress { get; set; }
        public string ServerMonitor { get; set; }
        public string ServerSession { get; set; }
        public string ServerState { get; set; }
    }

    public class compareResults
    {
        public string LoadBalancer { get; set; }
        public string VIPName { get; set; }
        public string StatusAvailabilityState { get; set; }
        public string StatusEnabledState { get; set; }
        public string StatusStatusReason { get; set; }
        public string PoolName { get; set; }

    }

    public class CertData
    {
        public string f5HostName { get; set; }
        public string VIPName { get; set; }
        public string ProfileName { get; set; }
        public DateTime expiration { get; set; }
        public string subject { get; set; }
        public string subjectAlternativeName { get; set; }
        public string chain { get; set; }
    }

    public class SelfIp
    {
        public string name { get; set; }
        public string address { get; set; }
        public string interfaceName { get; set; }
        public string floating { get; set; }
        public string vlan { get; set; }
        public int mtu { get; set; }
        public int tag { get; set; }
    }

    public class OfflineVip
    {
        public string Activef5Name { get; set; }
        public string ActiveVIPName { get; set; }
        public string StatusAvailabilityState { get; set; }
        public string StatusEnabledState { get; set; }
        public string StatusStatusReason { get; set; }
        public string VIPIP { get; set; }
        public string pool { get; set; }
        public string ClientsideTotConns { get; set; }

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

    public class VIPStatsOverTime
    {
        public string jobname { get; set; }
        public string f5Name { get; set; }
        public string VIPName { get; set; }
        public string ClientsideBitsIn { get; set; }
        public string ClientsideBitsOut { get; set; }
        public string ClientsideTotConns { get; set; }
    }

    public class VIPSummaryClass
    {
        public string StatusAvailabilityState { get; set; }
        public int StatusCount { get; set; }


    }
    public class NodeSummaryClass
    {
        public string Name { get; set; }
        public string state { get; set; }
    }
}