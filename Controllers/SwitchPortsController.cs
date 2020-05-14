using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using MvcSiteMapProvider.Web.Mvc.Filters;
using System.Net.Mail;
using MvcSiteMapProvider;
using System.Data.SqlClient;
using System.Configuration;
using Dapper;
using System.IO;
using OfficeOpenXml;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using SourceControl.Models.Db;
using SourceControl.Models;
using System.ComponentModel.DataAnnotations;
using SourceControl.App.NetworkCafe;
using NetworkToolbox.Controllers;
using SourceControl.Services;
using System.Text;

namespace SourceControl.Controllers
{

    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
    public partial class SwitchPortsController : Controller
    {
        private AppNetworkCafeEntities db = new AppNetworkCafeEntities();

        public ActionResult ACI_eVPN()
        {
            return View();
        }

        public JsonNetResult eVPNDataJSON()
        {
            AppUser appUser = SessionService.CurrentUser;
            bool admin = false;
            if (appUser.RoleNames.Contains("PAAdmin"))
            {
                admin = true;
            }
            List<eVPN> eVPNData = db.eVPNs.OrderBy(a => a.ID).ToList();
            List<ACI> aciData = db.ACIs.OrderBy(a => a.ID).ToList();
            var data = new
            {
                admin = admin,
                eVPNData = eVPNData,
                aciData = aciData
            };
            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.Data = data;

            return jsonNetResult;
        }
        [SiteMapTitle("DeviceName", Target = AttributeTarget.ParentNode)]
        public ActionResult SwitchIndex(String id)
        {
            var node = SiteMaps.Current.FindSiteMapNodeFromKey("6");
            if (node != null)
            {
                node.RouteValues["id"] = id;
                var parent = node.ParentNode;
                if (parent != null)
                {
                    parent.RouteValues["id"] = id;
                }
            }
            //SwitchPort switchPort = db.SwitchPorts.Find(id);
            var portResultsQuery = db.SwitchPorts.SqlQuery("Select * From SwitchPorts Where DeviceName ='" + id + "' order by PortNum  ASC").ToList();
            //NaturalStringComparer nsc = new NaturalStringComparer();
            NaturalComparer nc = new NaturalComparer();
            portResultsQuery.Sort((x, y) => nc.Compare(x.PortNum, y.PortNum));
            ViewBag.SwitchName = id;
            return View(portResultsQuery);
        }
        public ActionResult FirewallRoutes()
        {
            return View();
        }

        public JsonNetResult FWRouteSummaryJSON()
        {
            List<FwEnvironment> fwEnvironments = new List<FwEnvironment>();

            using (TargetEntities targetDb = new TargetEntities("NetworkCafeEntities"))
            {
                fwEnvironments = targetDb.Database.SqlQuery<FwEnvironment>(@"SELECT Distinct([Environment]) FROM [NetworkCafe].[dbo].[FWRoutes]").ToList();
                foreach (var item in fwEnvironments)
                {
                    item.networks = targetDb.Database.SqlQuery<FWRoutes>
                        (@"SELECT TOP 1000 [guid]
					,[Network]
					,[CMSFirewallIP]
					,[CMSFirewall]
					,[InternetFirewallIP]
					,[InternetFirewall]
					,[Notes]
					FROM [NetworkCafe].[dbo].[FWRoutes] WHERE [Environment] = '" + item.Environment + @"' ORDER BY CAST(PARSENAME([Network], 5) AS INT),
					CAST(PARSENAME([Network], 4) AS INT),
					CAST(PARSENAME([Network], 3) AS INT),
					CAST(PARSENAME([Network], 2) AS INT),
					CAST(PARSENAME([Network], 1) AS nvarchar)").ToList();
                }
            }

            var data = new { fwList = fwEnvironments };
            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.Data = data;

            return jsonNetResult;
        }

        public ActionResult PortStats()
        {
            return View();
        }

        //dynamic obj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
        public string ReclaimPorts(string ids)
        {
            using (TargetEntities targetDb = new TargetEntities("NetworkCafeEntities"))
            {
                targetDb.Database.ExecuteSqlCommand("UPDATE SwitchPorts SET HostName = 'OPEN', VLAN = '999', Comments = null, Interface = null WHERE PortGuid IN (" + ids + ") ");
            }
            return "T";
        }


        [HttpPost]
        public string AddSwitches(int pageTemplateId, string json)
        {
            dynamic obj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            var switchCount = 0;
            using (AppNetworkCafeEntities targetDb = new AppNetworkCafeEntities())
            {

                var pageTemplate = SessionService.PageTemplate(pageTemplateId);
                int startBlade = SourceControl.Common.Helper.ToInt32(obj["StartBlade"]);
                int endBlade = SourceControl.Common.Helper.ToInt32(obj["EndBlade"]);
                int startPort = SourceControl.Common.Helper.ToInt32(obj["StartPort"]);
                int endPort = SourceControl.Common.Helper.ToInt32(obj["EndPort"]);

                string prefix = (obj["PortPrefix"] != null) ? obj["PortPrefix"].ToString() : "";
                string deviceName = (obj[pageTemplate.TableName + "_DeviceName"] != null) ? obj[pageTemplate.TableName + "_DeviceName"].ToString() : "";
                string hostName = (obj[pageTemplate.TableName + "_HostName"] != null) ? obj[pageTemplate.TableName + "_HostName"].ToString() : "";
                for (int i = startBlade; i < (endBlade + 1); i++)
                {
                    for (int i2 = startPort; i2 < (endPort + 1); i2++)
                    {
                        string portnum = prefix + i + "/" + i2.ToString();
                        if (targetDb.SwitchPorts.Where(a => a.DeviceName == deviceName && a.PortNum == portnum).Count() == 0)
                        {

                            SwitchPort switchPort = new SwitchPort();
                            switchPort.DeviceName = deviceName;
                            switchPort.PortNum = portnum;
                            switchPort.HostName = hostName;
                            switchPort.VLAN = (obj[pageTemplate.TableName + "_VLAN"] != null) ? obj[pageTemplate.TableName + "_VLAN"].ToString() : "";
                            switchPort.PortType = (obj[pageTemplate.TableName + "_PortType"] != null) ? obj[pageTemplate.TableName + "_PortType"].ToString() : "";
                            switchPort.PortGUID = Guid.NewGuid();
                            targetDb.SwitchPorts.Add(switchPort);
                            switchCount++;
                        }
                    }
                }

                targetDb.SaveChanges();
            }
            return switchCount + "switches added.";
        }


        public ActionResult SwitchEditIndex(String id)
        {
            var node = SiteMaps.Current.FindSiteMapNodeFromKey("23");
            if (node != null)
            {
                node.RouteValues["id"] = id;
                var parent = node.ParentNode;
                if (parent != null)
                {
                    parent.RouteValues["id"] = id;
                }
            }
            //SwitchPort switchPort = db.SwitchPorts.Find(id);
            var portResultsQuery = db.SwitchPorts.SqlQuery("Select * From SwitchPorts Where DeviceName ='" + id + "' order by PortNum  ASC").ToList();
            //NaturalStringComparer nsc = new NaturalStringComparer();
            NaturalComparer nc = new NaturalComparer();
            portResultsQuery.Sort((x, y) => nc.Compare(x.PortNum, y.PortNum));
            ViewBag.SwitchName = id;
            return View(portResultsQuery);
        }

        public string GetDeviceGroupTree()
        {
            StringBuilder sb = new StringBuilder();
            using (AppNetworkCafeEntities db = new AppNetworkCafeEntities())
            {
                List<DeviceGroup> groups = db.DeviceGroups.Where(a => a.ParentGroupID == 0).OrderBy(a => a.GroupName).ToList();

                sb.AppendLine("<ul id='treeDeviceGroup'>");
                foreach (var group in groups)
                {
                    sb.AppendLine("<li>");
                    sb.AppendLine("<span onclick=\"GetDeviceGroupInfo('" + group.guid + "')\">" + group.GroupName + "</span>");
                    GetSubGroups(group.GroupID, ref sb);
                    sb.AppendLine("</li>");

                }
                sb.AppendLine("</ul>");
            }

            return sb.ToString();

        }
        public void GetSubGroups(int groupId, ref StringBuilder sb)
        {

            using (AppNetworkCafeEntities db = new AppNetworkCafeEntities())
            {
                List<DeviceGroup> groups = db.DeviceGroups.Where(a => a.ParentGroupID == groupId).OrderBy(a => a.GroupName).ToList();

                if (groups.Count > 0)
                {
                    sb.AppendLine("<ul id='groupId" + groupId + "'>");
                    foreach (var group in groups)
                    {
                        sb.AppendLine("<li id='groupId_" + group.GroupID + "'>");
                        sb.AppendLine("<span onclick=\"GetDeviceGroupInfo('" + group.guid + "')\">" + group.GroupName + "</span>");
                        GetSubGroups(group.GroupID, ref sb);
                        sb.AppendLine("</li>");
                    }
                    sb.AppendLine("</ul>");
                }
            }

        }




        [HttpPost]
        public ActionResult SwitchEditIndex(List<SwitchPort> list)
        {
            if (ModelState.IsValid)
            {
                using (AppNetworkCafeEntities dc = new AppNetworkCafeEntities())
                {
                    foreach (var i in list)
                    {
                        var p = dc.SwitchPorts.Where(a => a.PortGUID.Equals(i.PortGUID)).FirstOrDefault();
                        if (p != null)
                        {
                            p.HostName = i.HostName;
                            p.PortType = i.PortType;
                            p.Comments = i.Comments;
                            p.Interface = i.Interface;
                        }


                    }
                    ViewBag.Message = "Successfully Updated.";
                    dc.SaveChanges();

                }

                return View(list);
            }
            ViewBag.Message = "Failed! Please try again or contact site Admin.";
            return View(list);
        }

        public ActionResult EndDeviceHome()
        {
            return View();
        }
        public JsonResult EndDeviceSearch(string searchString)
        {

            var hostPorts = retrieveSwitchData(searchString);

            var data = new
            {
                resultsPASearch = hostPorts,
            };


            return new JsonResult { Data = data, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        private List<SwitchPort> retrieveSwitchData(string searchString)
        {

            List<SwitchPort> hostPorts = new List<SwitchPort>();
            List<SwitchPort> tempPorts = new List<SwitchPort>();

            if (!String.IsNullOrEmpty(searchString))
            {
                List<string> searchList = new List<string>();
                if (searchString.Contains(","))
                {
                    searchList = searchString.Split(',').ToList();
                }
                else
                {
                    searchList.Add(searchString);
                }
                foreach (var item in searchList)
                {
                    using (IDbConnection _db = new SqlConnection(ConfigurationManager.ConnectionStrings["NetworkCafeConnectionString"].ConnectionString), commandTimeout = null)
                    {
                        tempPorts = _db.Query<SwitchPort>("select * from SwitchPorts where hostname like '%" + item + "%'").ToList();
                    }
                    hostPorts.AddRange(tempPorts);
                }

            }
            return hostPorts;
        }
        public JsonResult EndDeviceDNSSearch(string searchString)
        {
            searchString = searchString.ToLower();
            RestConnector r = new RestConnector();
            List<RestConnector.HostData> temp = new List<RestConnector.HostData>();
            List<RestConnector.HostData> results = new List<RestConnector.HostData>();
            List<string> searchList = new List<string>();
            if (searchString.Contains(","))
            {
                searchList = searchString.Split(',').ToList();
            }
            else
            {
                searchList.Add(searchString);
            }
            foreach (var item in searchList)
            {

                temp = r.GetHostDNS(item);
                results.AddRange(temp);
            }

            var data = new
            {
                DNSHostResults = results,
            };


            return new JsonResult { Data = data, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        public JsonNetResult EndDeviceIPSearch(string searchString)
        {
            RestConnector r = new RestConnector();
            List<string> searchList = new List<string>();
            if (searchString.Contains(","))
            {
                searchList = searchString.Split(',').ToList();
            }
            else
            {
                searchList.Add(searchString);
            }
            List<RestConnector.IPData> temp = new List<RestConnector.IPData>();
            List<RestConnector.IPData> results = new List<RestConnector.IPData>();
            foreach (var item in searchList)
            {

                temp = r.getIPDNS(item);
                results.AddRange(temp);
            }


            var data = new
            {
                DNSIPResults = results,
            };

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.Data = data;

            return jsonNetResult;

        }
        public JsonResult EndDeviceESXSearch(string searchString)
        {
            var vmList = DeviceESXSearch(searchString);
            var data = new
            {
                vmList = vmList,
            };


            return new JsonResult { Data = data, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        public List<VMPhoneBook> DeviceESXSearch(string searchString)
        {
            List<VMPhoneBook> vmList = new List<VMPhoneBook>();
            List<VMPhoneBook> temp = new List<VMPhoneBook>();
            List<string> searchList = new List<string>();
            if (searchString.Contains(","))
            {
                searchList = searchString.Split(',').ToList();
            }
            else
            {
                searchList.Add(searchString);
            }
            foreach (var item in searchList)
            {

                if (!String.IsNullOrEmpty(item))
                {
                    using (IDbConnection _db = new SqlConnection(ConfigurationManager.ConnectionStrings["NetworkCafeConnectionString"].ConnectionString), commandTimeout = null)
                    {
                        temp = _db.Query<VMPhoneBook>("select * from vmPhoneBook where ServerName like '%" + searchString + "%' OR ESXi_Host like '%" + item + "%'").ToList();
                    }
                    vmList.AddRange(temp);
                }
            }
            return vmList;
        }
        public ActionResult exportSwitchports(string searchString)
        {


            List<SwitchPort> hostPorts = new List<SwitchPort>();
            List<SwitchPort> tempPorts = new List<SwitchPort>();

            if (!String.IsNullOrEmpty(searchString))
            {
                List<string> searchList = new List<string>();
                if (searchString.Contains(","))
                {
                    searchList = searchString.Split(',').ToList();
                }
                else
                {
                    searchList.Add(searchString);
                }
                foreach (var item in searchList)
                {
                    using (IDbConnection _db = new SqlConnection(ConfigurationManager.ConnectionStrings["NetworkCafeConnectionString"].ConnectionString), commandTimeout = null)
                    {
                        tempPorts = _db.Query<SwitchPort>("select * from switchports where DeviceName like '%" + item + "%'").ToList();
                    }
                    hostPorts.AddRange(tempPorts);
                }
                IEnumerable<char> devices = tempPorts.SelectMany(a => a.DeviceName).Distinct();
                Stream stream = null;
                using (ExcelPackage excelPackage = new ExcelPackage(stream))
                {

                    excelPackage.Workbook.Properties.Title = "Switch Export Results";
                    int page = 1;

                    foreach (var item in devices)
                    {
                        List<SwitchPort> data = hostPorts.Where(a => a.DeviceName == item.ToString()).ToList();
                        string name = item.ToString();
                        excelPackage.Workbook.Worksheets.Add(name);
                        var worksheet = excelPackage.Workbook.Worksheets[page];
                        worksheet.Cells[1, 1].LoadFromCollection(data, true);
                    }
                    var memoryStream = stream as MemoryStream;
                    Response.Clear();
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("Content-disposition", "attachment; filename=ports.xlsx");

                    Response.BinaryWrite(memoryStream.ToArray());
                    Response.End();
                    return View();


                }
            }

            return RedirectToAction("EndDeviceHome", "SwitchPorts");
        }
        public ActionResult DownloadHostSearch(string searchString)
        {
            if (searchString != null && searchString != "")
            {
                var stream = ExportDataToExcel(searchString);
                var memoryStream = stream as MemoryStream;

                Response.Clear();
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("Content-disposition", "attachment; filename=HostSearchData.xlsx");

                Response.BinaryWrite(memoryStream.ToArray());
                Response.End();
                return View();
            }
            return RedirectToAction("EndDeviceHome", "SwitchPorts");

        }
        public ActionResult EmailHostSearch(string searchString)
        {
            if (searchString != null && searchString != "")
            {
                var stream = ExportDataToExcel(searchString);
                var memoryStream = stream as MemoryStream;
                SendMailAttachment(memoryStream);

                return RedirectToAction("EndDeviceHome", "SwitchPorts");
            }
            return RedirectToAction("EndDeviceHome", "SwitchPorts");
        }
        private void SendMailAttachment(MemoryStream stream)
        {
            string filename = "results.xlsx";

            AppNetworkCafeEntities db = new AppNetworkCafeEntities();
            AppUser user = SessionService.CurrentUser;
            string dstAddress = user.Email;
            string messageBody = "<font>The records you requested are in the attachment. </font><br><br>";
            MailAddress to = new MailAddress(dstAddress);
            MailAddress from = new MailAddress("cmsnetworkeng@hpe.com");
            MailMessage mail = new MailMessage(from, to);

            mail.Subject = "End Device Connectivity Report";
            mail.IsBodyHtml = true;
            mail.Body = messageBody;
            stream.Position = 0;
            mail.Attachments.Add(new Attachment(stream, filename, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));
            SmtpClient smtp = new SmtpClient();
            smtp.Host = "mailhost.rdcms.eds.com";
            smtp.Port = 25;
            smtp.Send(mail);
        }

        public Stream ExportDataToExcel(string searchString, Stream stream = null)
        {
            AppUser user = SessionService.CurrentUser;

            //get switch data
            var hostPorts = retrieveSwitchData(searchString);
            //get dns data
            RestConnector r = new RestConnector();
            List<RestConnector.HostData> dnsData = r.GetHostDNS(searchString);
            //get f5 data
            var f5Data = retrieveF5Data(searchString);
            //esx data
            var vmList = DeviceESXSearch(searchString);

            //build spreadsheet
            using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
            {
                excelPackage.Workbook.Properties.Author = user.FirstName + " " + user.LastName;
                excelPackage.Workbook.Properties.Title = "Host Search Results";
                excelPackage.Workbook.Properties.Comments = "Data found for the specific end device search.";

                excelPackage.Workbook.Worksheets.Add("Switch Data");
                var worksheet = excelPackage.Workbook.Worksheets[1];
                excelPackage.Workbook.Worksheets.Add("DNS Data");
                var worksheet2 = excelPackage.Workbook.Worksheets[2];
                excelPackage.Workbook.Worksheets.Add("f5 Data");
                var worksheet3 = excelPackage.Workbook.Worksheets[3];
                excelPackage.Workbook.Worksheets.Add("ESX Data");
                var worksheet4 = excelPackage.Workbook.Worksheets[4];

                worksheet.Cells[1, 1].LoadFromCollection(hostPorts, true);
                worksheet2.Cells[1, 1].LoadFromCollection(dnsData, true);
                worksheet3.Cells[1, 1].LoadFromCollection(f5Data, true);
                worksheet4.Cells[1, 1].LoadFromCollection(vmList, true);
                excelPackage.Save();
                return excelPackage.Stream;

            }

        }

        public JsonResult EndDevicef5Search(string searchString)
        {
            var results = retrieveF5Data(searchString);
            var data = new
            {
                f5SearchData = results,
            };
            return new JsonResult { Data = data, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        private List<f5SearchResults> retrieveF5Data(string searchString)
        {
            AppF5DataEntities db2 = new AppF5DataEntities();
            JobLog jl = db2.JobLogs.OrderByDescending(a => a.endTime).FirstOrDefault();
            string jlstring = jl.guid.ToString();



            List<f5SearchResults> results = new List<f5SearchResults>();
            List<f5SearchResults> temp = new List<f5SearchResults>();
            List<string> searchList = new List<string>();
            if (searchString.Contains(","))
            {
                searchList = searchString.Split(',').ToList();
            }
            else
            {
                searchList.Add(searchString);
            }
            foreach (var item in searchList)
            {


                using (IDbConnection _db = new SqlConnection(ConfigurationManager.ConnectionStrings["f5dataConnectionString"].ConnectionString), commandTimeout = null)
                {
                    temp = _db.Query<f5SearchResults>(@"SELECT DeviceData.name AS F5Name, VIP.name AS VIPNAME, VIP.destination AS VIPIP, Pool.name AS PoolName, PoolMember.Name AS NodeName, " +
                                "PoolMember.address AS NodeIP " +
                             "FROM DeviceData INNER JOIN " +
                                 "JobLog ON DeviceData.jobguid = JobLog.guid INNER JOIN " +
                                 "VIP ON DeviceData.jobguid = VIP.jobguid AND DeviceData.hostname = VIP.f5HostName INNER JOIN " +
                                 "Pool ON VIP.jobguid = Pool.jobguid AND VIP.f5HostName = Pool.f5HostName AND VIP.pool = Pool.fullpath INNER JOIN " +
                                 "PoolMember ON Pool.guid = PoolMember.PoolGuid " +
                              "WHERE(DeviceData.failoverState = N'active') " +
                                 "AND(DeviceData.selfDevice = N'true') " +
                                 "AND(JobLog.guid = '" + jlstring + "' " +
                                 "And(PoolMember.Name LIKE '%" + item + "%') )").ToList();
                    foreach (var subitem in results)
                    {
                        subitem.VIPIP = subitem.VIPIP.Replace("/Common/", "");
                    }
                    results.AddRange(temp);
                }
            }

            return results;

        }
        private void sendemail(List<SwitchPort> hostPorts, string dstAddress)
        {

            string messageBody = "<font>The following are the records: </font><br><br>";
            MailAddress to = new MailAddress(dstAddress);
            MailAddress from = new MailAddress("cmsnetworkeng@hpe.com");
            MailMessage mail = new MailMessage(from, to);
            mail.Subject = "End Device Connectivity Report";
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
            messageBody += string.Format("<th>Device Name</th><th>Device Location</th><th>PortNum</th><th>Host Name</th><th>Interface</th><th>VLAN</th><th>Comments</th>");
            messageBody += htmlHeaderRowEnd;


            foreach (var item in hostPorts)
            {
                messageBody += htmlTrStart + string.Format("{0}{2}{1}{0}{3}{1}{0}{4}{1}{0}{5}{1}{0}{6}{1}{0}{7}{1}{0}{8}{1}{0}{9}{1}", htmlTdStart, htmlTdEnd, item.DeviceName, item.DeviceType, item.DeviceLocation, item.PortNum, item.HostName, item.Interface, item.VLAN, item.Comments) + htmlTrEnd;
            }
            messageBody += htmlTableEnd;
            mail.Body = messageBody;
            SmtpClient smtp = new SmtpClient();
            smtp.Host = "mailhost.rdcms.eds.com";
            smtp.Port = 25;
            smtp.Send(mail);
        }


        // GET: SwitchPorts/Details/5

        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SwitchPort switchPort = db.SwitchPorts.Find(id);
            if (switchPort == null)
            {
                return HttpNotFound();
            }
            return View(switchPort);
        }

        // GET: SwitchPorts/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: SwitchPorts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "DeviceName,PortGUID,PortNum,PortType,Connected,HostName,Interface,VLAN,Comments")] SwitchPort switchPort)
        {
            if (ModelState.IsValid)
            {
                switchPort.PortGUID = Guid.NewGuid();
                db.SwitchPorts.Add(switchPort);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(switchPort);
        }

        // GET: SwitchPorts/Edit/5
        [SiteMapTitle("DeviceName", Target = AttributeTarget.ParentNode)]
        public ActionResult Edit(Guid? guid, string id)
        {
            var node = SiteMaps.Current.FindSiteMapNodeFromKey("7");
            if (node != null)
            {
                node.RouteValues["id"] = id;
                var parent = node.ParentNode;
                if (parent != null)
                {
                    parent.RouteValues["id"] = id;
                }
            }
            if (guid == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SwitchPort switchPort = db.SwitchPorts.Find(guid);
            if (switchPort == null)
            {
                return HttpNotFound();
            }
            //            var node = SiteMaps.Current.CurrentNode;
            //if (node != null && node.ParentNode != null)
            //{
            //    node.Route = switchPort.DeviceName;
            //}
            return View(switchPort);
        }

        // POST: SwitchPorts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "DeviceName,PortGUID,PortNum,PortType,Connected,HostName,Interface,VLAN,Comments")] SwitchPort switchPort)
        {
            if (ModelState.IsValid)
            {

                db.Entry(switchPort).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("SwitchIndex", new { id = switchPort.DeviceName });
                //return RedirectToAction("SwitchName", "SwitchPort", switchPort.DeviceName);
            }
            return View(switchPort);
        }

        // GET: SwitchPorts/Delete/5
        public ActionResult Delete(Guid? id)
        {
            var node = SiteMaps.Current.FindSiteMapNodeFromKey("21");
            if (node != null)
            {
                node.RouteValues["id"] = id;
                var parent = node.ParentNode;
                if (parent != null)
                {
                    parent.RouteValues["id"] = id;
                }
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SwitchPort switchPort = db.SwitchPorts.Find(id);
            if (switchPort == null)
            {
                return HttpNotFound();
            }
            return View(switchPort);
        }

        // POST: SwitchPorts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            SwitchPort switchPort = db.SwitchPorts.Find(id);
            db.SwitchPorts.Remove(switchPort);
            db.SaveChanges();
            return RedirectToAction("Index", "MasterDevices");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult IPNetworks()
        {

            return View();
        }

        public ActionResult IPNetworksAdmin()
        {

            return View();
        }

        public JsonResult IPNetworksData()
        {

            List<IPNetwork> ipNet = new List<IPNetwork>();
            using (IDbConnection _db = new SqlConnection(ConfigurationManager.ConnectionStrings["NetworkCafeConnectionString"].ConnectionString))
            {

                ipNet = _db.Query<IPNetwork>("Select * FROM IPNetworks").ToList();
            }
            return new JsonResult { Data = ipNet, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public JsonResult AddNetwork(string network)
        {

            IPNetwork network1 = new JavaScriptSerializer().Deserialize<IPNetwork>(network);

            if (network1.guid.ToString() == "00000000-0000-0000-0000-000000000000")
            {
                network1.guid = Guid.NewGuid();
                db.IPNetworks.Add(network1);
                db.SaveChanges();
            }
            else
            {
                db.IPNetworks.Attach(network1);
                db.Entry(network1).State = EntityState.Modified;
                db.SaveChanges();
            }

            List<IPNetwork> ipNet = new List<IPNetwork>();
            using (IDbConnection _db = new SqlConnection(ConfigurationManager.ConnectionStrings["NetworkCafeConnectionString"].ConnectionString))
            {

                ipNet = _db.Query<IPNetwork>("Select * FROM IPNetworks").ToList();
            }
            return new JsonResult { Data = ipNet, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

        }

        public JsonResult DeleteNetwork(Guid id)
        {
            IPNetwork network = db.IPNetworks.Find(id);
            db.IPNetworks.Remove(network);
            db.SaveChanges();


            List<IPNetwork> ipNet = new List<IPNetwork>();
            using (IDbConnection _db = new SqlConnection(ConfigurationManager.ConnectionStrings["NetworkCafeConnectionString"].ConnectionString))
            {

                ipNet = _db.Query<IPNetwork>("Select * FROM IPNetworks").ToList();
            }
            return new JsonResult { Data = ipNet, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

        }

        public ActionResult DownloadIPNetworkData()
        {

            var stream = ExportIPNetworkDataToExcel();
            var memoryStream = stream as MemoryStream;

            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("Content-disposition", "attachment; filename=MasterVLANExport.xlsx");

            Response.BinaryWrite(memoryStream.ToArray());
            Response.End();
            return View();

        }
        public Stream ExportIPNetworkDataToExcel(Stream stream = null)
        {
            AppUser user = SessionService.CurrentUser;
            List<IPNetwork> ipNet = new List<IPNetwork>();
            using (IDbConnection _db = new SqlConnection(ConfigurationManager.ConnectionStrings["NetworkCafeConnectionString"].ConnectionString))
            {

                ipNet = _db.Query<IPNetwork>("Select * FROM IPNetworks").ToList();
            }
            //build spreadsheet
            using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
            {
                excelPackage.Workbook.Properties.Author = user.FirstName + " " + user.LastName;
                excelPackage.Workbook.Properties.Title = "Host Search Results";
                excelPackage.Workbook.Properties.Comments = "Data found for the specific end device search.";

                excelPackage.Workbook.Worksheets.Add("Master VLAN Data");
                var worksheet = excelPackage.Workbook.Worksheets[1];


                worksheet.Cells[1, 1].LoadFromCollection(ipNet, true);


                excelPackage.Save();
                return excelPackage.Stream;

            }

        }

        public JsonResult submitRecord(string update)
        {
            string updateText = "";
            List<string> updates = new List<string>();
            AppNetworkCafeEntities db = new AppNetworkCafeEntities();
            PortUpdate p = new PortUpdate();
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            };
            p = JsonConvert.DeserializeObject<PortUpdate>(update, settings);


            if (p.updateType == "new")
            {
                for (int i = p.startBlade; i < (p.endBlade + 1); i++)
                {

                    for (int i2 = p.startPort; i2 < (p.endPort + 1); i2++)
                    {

                        string portnum = p.prefix + i + "/" + i2.ToString();
                        if (db.SwitchPorts.Where(a => a.DeviceName == p.switchName && a.PortNum == portnum).Count() == 0)
                        {

                            SwitchPort nP = new SwitchPort();
                            nP.DeviceName = p.switchName;
                            nP.PortNum = portnum;
                            nP.HostName = p.description;
                            nP.VLAN = p.vlan;
                            nP.PortType = p.portType;
                            nP.PortGUID = Guid.NewGuid();
                            db.SwitchPorts.Add(nP);

                            updateText = "Added switch port " + portnum + " to switch " + p.switchName;
                            updates.Add(updateText);
                        }
                        else
                        {
                            updateText = "Not added to switch port " + portnum + " to switch " + p.switchName + " because was already found.";
                            updates.Add(updateText);
                        }
                    }
                }

            }
            else
            {
                for (int i = p.startBlade; i < (p.endBlade + 1); i++)
                {
                    for (int i2 = p.startPort; i2 < (p.endPort + 1); i2++)
                    {

                        string portnum = p.prefix + i + "/" + i2.ToString();
                        if (db.SwitchPorts.Where(a => a.DeviceName == p.switchName && a.PortNum == portnum).Count() >= 1)
                        {

                            SwitchPort s = db.SwitchPorts.Where(a => a.DeviceName == p.switchName && a.PortNum == portnum).FirstOrDefault();
                            db.SwitchPorts.Remove(s);
                            updateText = "Deleted switch port " + portnum + " to switch " + p.switchName;
                            updates.Add(updateText);
                        }
                        else
                        {
                            updateText = "Switch port " + portnum + " to switch " + p.switchName + " not found to delete";
                            updates.Add(updateText);
                        }
                    }
                }
            }

            db.SaveChanges();




            var data = new
            {
                updateText = updates
            };

            return new JsonResult { Data = data, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        public ActionResult DeviceGroups()
        {

            return View(db.DeviceGroups.ToList());
        }
        public ActionResult ManageVLan()
        {

            return View();
        }
        public class VLANData
        {
            public string Site { get; set; }
            public int VLAN { get; set; }
            public string ASSIGNMENT { get; set; }

        }
        public JsonNetResult GetSiteListJSON()
        {
            AppNetworkCafeEntities db = new AppNetworkCafeEntities();

            List<string> sitesList = db.Database.SqlQuery<string>("Select distinct Site from IPNetworks").ToList();
            var data = new
            {
                sitesList = sitesList
            };
            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.Data = data;

            return jsonNetResult;
        }
        public JsonNetResult GetVlansJSON(string site)
        {
            AppNetworkCafeEntities db = new AppNetworkCafeEntities();
            //List<IPNetwork> AllNetworks = new List<IPNetwork>();
            List<VLANData> vd = new List<VLANData>();
            //List<string> sitesList = db.Database.SqlQuery<string>("Select distinct Site from IPNetworks").ToList();
            //foreach (var site in sitesList)
            //{
            //int startOpenVLAN = 0;
            //int lastVLANChecked = 0;
            List<IPNetwork> networks = db.IPNetworks.Where(a => a.Site == site).OrderBy(a => a.VLAN).ToList();


            for (int i = 2; i < 4094; i++)
            {

                if (networks.Where(a => a.VLAN == i.ToString()).Count() == 0)
                {
                    VLANData d = new VLANData();
                    d.ASSIGNMENT = "Open";
                    d.Site = site;
                    d.VLAN = i;
                    vd.Add(d);
                }
                else
                {
                    IPNetwork s = networks.Where(a => a.VLAN == i.ToString()).FirstOrDefault();
                    VLANData d = new VLANData();
                    d.ASSIGNMENT = s.ASSIGNMENT;
                    d.Site = s.Site;
                    d.VLAN = int.Parse(s.VLAN);
                    vd.Add(d);
                }

                //if (networks.Where(a=> a.VLAN == i.ToString()).Count() == 0)
                //{
                //    if (startOpenVLAN == 0)
                //    {
                //        startOpenVLAN = i;
                //        lastVLANChecked = i;
                //    }
                //    else
                //    {
                //        if (lastVLANChecked +1 == i)
                //        {
                //            lastVLANChecked = lastVLANChecked + 1;
                //        }
                //        else
                //        {
                //            string VlanID = startOpenVLAN.ToString() + " - " + lastVLANChecked.ToString();
                //            IPNetwork network = new IPNetwork();
                //            network.VLAN = VlanID;
                //            network.Site = site;
                //            network.ASSIGNMENT = "OPEN";
                //            network.Notes = i.ToString();
                //            networks.Add(network);
                //            startOpenVLAN = 0;
                //            lastVLANChecked = i;
                //        }   
                //    }

                //}

            }

            //int netgroup = 0;
            //int lastVLANid = 0;
            //foreach (var network in networks)
            //{
            //    if (network.ASSIGNMENT == "OPEN" && network.Notes == "OPEN")
            //    {
            //        if (netgroup == 0)
            //        {
            //            netgroup = 1;
            //            network.Notes = "1";
            //            lastVLANid = int.Parse(network.VLAN);
            //        }
            //        else
            //        {
            //            if ((int.Parse(network.VLAN) -1) == lastVLANid)
            //            {
            //                network.Notes = netgroup.ToString();
            //                lastVLANid = int.Parse(network.VLAN);
            //            }
            //            else
            //            {
            //                netgroup += 1;
            //                network.Notes = netgroup.ToString();
            //                lastVLANid = int.Parse(network.VLAN);

            //            }
            //        }
            //    }
            //    AllNetworks.Add(network);
            //}
            //}





            var data = new
            {
                AllNetworks = vd
            };
            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.Data = data;

            return jsonNetResult;
        }

        [HttpPost]
        public ActionResult AddGroup(string ParentGroupID)
        {


            return View();
        }
        public JsonResult getgrouptree()
        {
            List<DeviceGroup> rootgroup = db.DeviceGroups.Where(a => a.ParentGroupID == 0).OrderBy(a => a.GroupName).ToList();

            foreach (var item in rootgroup)
            {
                item.SubGroup = devicesubgroup(item);
                item.DeviceList = db.DeviceGroupMembers.Where(a => a.groupguid == item.guid).OrderBy(a => a.DeviceName).ToList();
            }

            return new JsonResult { Data = rootgroup, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

        }
        public List<DeviceGroup> devicesubgroup(DeviceGroup subgroup1)
        {
            List<DeviceGroup> subgroup = new List<DeviceGroup>();
            int count = db.DeviceGroups.Where(a => a.ParentGroupID == subgroup1.GroupID).Count();
            if (count >= 1)
            {
                subgroup = subgroup1.SubGroup = db.DeviceGroups.Where(a => a.ParentGroupID == subgroup1.GroupID).OrderBy(a => a.GroupName).ToList();
                foreach (var item in subgroup)
                {
                    devicesubgroup(item);
                    item.DeviceList = db.DeviceGroupMembers.Where(a => a.groupguid == item.guid).OrderBy(a => a.DeviceName).ToList();
                }
            }
            subgroup.OrderBy(a => a.GroupName);
            return subgroup;
        }
        public ActionResult DownloadExcelPortCapacityReport()
        {
            var stream = getDeviceGroupDataToStream();
            var memoryStream = stream as MemoryStream;
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("Content-disposition", "attachment; filename=PortCapacityReport.xlsx");
            Response.BinaryWrite(memoryStream.ToArray());
            Response.End();
            return View();


        }
        public Stream getDeviceGroupDataToStream(Stream stream = null)
        {
            List<DeviceGroup> allmastergroups = db.DeviceGroups.Where(a => a.AddMembers == true).ToList();
            List<DeviceGroup> newGroup = new List<DeviceGroup>();
            foreach (var item in allmastergroups)
            {
                DeviceGroup newItem = getGroupInfo(item.guid);
                newItem.GroupName = getGroupNameWithParents(item.GroupID, item.GroupName);
                if (newItem.PercentUsed != 0)
                {
                    newItem.PercentUsed = 100 - newItem.PercentUsed;
                }

                newGroup.Add(newItem);

            }

            using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
            {

                excelPackage.Workbook.Properties.Title = "Switch Port Data";
                excelPackage.Workbook.Properties.Comments = "Switch Port Data";

                excelPackage.Workbook.Worksheets.Add("Switch Port Data");
                var worksheet = excelPackage.Workbook.Worksheets[1];


                worksheet.Cells[1, 1].LoadFromCollection(newGroup, true);

                excelPackage.Save();
                return excelPackage.Stream;
            }

        }
        public string getGroupNameWithParents(int groupID, string name)
        {

            DeviceGroup item = db.DeviceGroups.Where(a => a.GroupID == groupID).SingleOrDefault();
            string name2 = "";
            if (item.ParentGroupID == 0)
            {
                return name;
            }
            else
            {
                DeviceGroup parentItem = db.DeviceGroups.Where(a => a.GroupID == item.ParentGroupID).SingleOrDefault();
                name2 = getGroupNameWithParents(parentItem.GroupID, parentItem.GroupName);
                name = name2 + "-" + name;
            }

            return name;

        }

        public string GetGroupDeviceInfo(Guid groupguid)
        {
            DeviceGroup item = getGroupInfo(groupguid);
            List<MasterDevice> devicesNotInGroup = new List<MasterDevice>();
            devicesNotInGroup = db.MasterDevices.Where(a => a.DeviceType == "switch").OrderBy(a => a.HostName).ToList();
            foreach (var device in item.DeviceList)
            {
                MasterDevice mdevice = db.MasterDevices.Where(a => a.HostName == device.DeviceName).FirstOrDefault();
                devicesNotInGroup.Remove(mdevice);
            }
            var data = new { devicegroupitem = item, devicesnotingroup = devicesNotInGroup };



            var json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            return json;


        }

        public DeviceGroup getGroupInfo(Guid groupguid)
        {
            DeviceGroup item = db.DeviceGroups.Where(a => a.guid == groupguid).FirstOrDefault();
            item.DeviceList = db.DeviceGroupMembers.Where(a => a.groupguid == item.guid).OrderBy(a => a.DeviceName).ToList();

            if (item.AddMembers)
            {
                item.DeviceCount = db.DeviceGroupMembers.Where(a => a.groupguid == item.guid).Count();
                item.OpenPortCount = GetDeviceGroupOpenPortCount(item.DeviceList);
                item.ReserverPortCount = GetDeviceGroupReservedPortCount(item.DeviceList);
                item.TotalPortCount = GetDeviceGroupTotalPortCount(item.DeviceList);
                if (item.TotalPortCount != 0 && item.OpenPortCount != 0)
                {
                    item.PercentUsed = (int)Math.Round(100 - ((((double)item.TotalPortCount - ((double)item.OpenPortCount + (double)item.ReserverPortCount)) / item.TotalPortCount) * 100), 0);

                }
                else
                {
                    item.PercentUsed = 0;
                }
                foreach (var device in item.DeviceList)
                {
                    device.TotalPortCount = db.SwitchPorts.Where(a => a.DeviceName == device.DeviceName).Count();
                    device.OpenPortCount = db.SwitchPorts.Where(a => a.HostName.Contains("Open") && a.DeviceName == device.DeviceName).Count();
                    device.ReservedPortCount = db.SwitchPorts.Where(a => a.HostName.Contains("Reserved") && a.DeviceName == device.DeviceName).Count();
                    if (device.TotalPortCount != 0 && device.OpenPortCount != 0)
                    {
                        device.PercentUsed = (int)Math.Round(100 - ((((double)device.TotalPortCount - ((double)device.OpenPortCount + (double)device.ReservedPortCount)) / device.TotalPortCount) * 100), 0);
                    }
                    else
                    {
                        device.PercentUsed = 0;
                    }
                }
            }
            else
            {
                item.SubGroup = devicesubgroup(item);

                DeviceGroup totalsubgroupports = GetTotalPortsForSubGroups(item);
                item.DeviceCount = totalsubgroupports.DeviceCount;
                item.OpenPortCount = totalsubgroupports.OpenPortCount;
                item.ReserverPortCount = totalsubgroupports.ReserverPortCount;
                item.TotalPortCount = totalsubgroupports.TotalPortCount;
                if (totalsubgroupports.TotalPortCount != 0 && totalsubgroupports.OpenPortCount != 0)
                {
                    item.PercentUsed = (int)Math.Round(100 - ((((double)totalsubgroupports.TotalPortCount - ((double)totalsubgroupports.OpenPortCount + (double)totalsubgroupports.ReserverPortCount)) / totalsubgroupports.TotalPortCount) * 100), 0);
                }
                else
                {
                    item.PercentUsed = 0;
                }

            }
            return item;
        }

        private DeviceGroup GetTotalPortsForSubGroups(DeviceGroup dg)
        {
            DeviceGroup tempGroup = new DeviceGroup();
            DeviceGroup subdg = new DeviceGroup();
            if (dg.SubGroup != null)
            {
                foreach (var subgroup in dg.SubGroup)
                {
                    subdg = GetTotalPortsForSubGroups(subgroup);
                    tempGroup.DeviceCount += subdg.DeviceCount;
                    tempGroup.OpenPortCount += subdg.OpenPortCount;
                    tempGroup.ReserverPortCount += subdg.ReserverPortCount;
                    tempGroup.TotalPortCount += subdg.TotalPortCount;
                }
            }
            else
            {
                tempGroup.DeviceCount += db.DeviceGroupMembers.Where(a => a.groupguid == dg.guid).Count();
                tempGroup.OpenPortCount += GetDeviceGroupOpenPortCount(dg.DeviceList);
                tempGroup.ReserverPortCount += GetDeviceGroupReservedPortCount(dg.DeviceList);
                tempGroup.TotalPortCount += GetDeviceGroupTotalPortCount(dg.DeviceList);

            }
            return tempGroup;

        }

        public string DeleteDeviceFromGroup(Guid deviceguid, Guid groupguid)
        {
            try
            {
                DeviceGroupMember device = db.DeviceGroupMembers.Find(deviceguid);
                db.DeviceGroupMembers.Remove(device);
                db.SaveChanges();
            }
            catch (Exception)
            {
            }

            return "T";
        }
        public JsonResult AddDeviceToGroup(string hostname, Guid groupguid)
        {
            DeviceGroupMember device = new DeviceGroupMember();
            Guid g = Guid.NewGuid();
            device.DeviceName = hostname;
            device.guid = g;
            device.groupguid = groupguid;
            db.DeviceGroupMembers.Add(device);
            db.SaveChanges();
            var item = getGroupInfo(groupguid);


            DeviceGroup group = getGroupInfo(groupguid);
            List<MasterDevice> devicesNotInGroup = new List<MasterDevice>();
            devicesNotInGroup = db.MasterDevices.Where(a => a.DeviceType == "switch").OrderBy(a => a.HostName).ToList();
            foreach (var device1 in group.DeviceList)
            {
                MasterDevice mdevice = db.MasterDevices.Where(a => a.HostName == device1.DeviceName).FirstOrDefault();
                devicesNotInGroup.Remove(mdevice);
            }
            var data = new { devicegroupitem = item, devicesnotingroup = devicesNotInGroup };

            return new JsonResult { Data = data, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

        }
        private int GetDeviceGroupTotalPortCount(List<DeviceGroupMember> deviceList)
        {
            int count = 0;
            foreach (var item in deviceList)
            {
                int opencount = db.SwitchPorts.Where(a => a.DeviceName == item.DeviceName).Count();
                count = count + opencount;
            }
            return count;
        }

        private int GetDeviceGroupReservedPortCount(List<DeviceGroupMember> deviceList)
        {
            int count = 0;
            foreach (var item in deviceList)
            {
                int opencount = db.SwitchPorts.Where(a => a.HostName.Contains("Reserved") && a.DeviceName == item.DeviceName).Count();
                count = count + opencount;
            }
            return count;
        }

        private int GetDeviceGroupOpenPortCount(List<DeviceGroupMember> deviceList)
        {
            int count = 0;
            foreach (var item in deviceList)
            {
                int opencount = db.SwitchPorts.Where(a => a.HostName.Contains("Open") && a.DeviceName == item.DeviceName).Count();
                count = count + opencount;
            }
            return count;
        }

        public JsonResult GetListOfSwitches()
        {
            var switches = db.MasterDevices.Where(a => a.DeviceType == "switch").OrderBy(a => a.HostName).ToList();

            return new JsonResult { Data = switches, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public string CreateGroup(int groupID, string groupName, string addMember)
        {

            var maxid = db.DeviceGroups.OrderByDescending(i => i.GroupID).FirstOrDefault();
            int newid = maxid.GroupID + 1;

            Guid g = Guid.NewGuid();
            DeviceGroup dg = new DeviceGroup();
            dg.guid = g;
            dg.ParentGroupID = groupID;
            dg.GroupName = groupName;
            dg.GroupID = newid;
            dg.AddMembers = (addMember == "true") ? true : false;
            db.DeviceGroups.Add(dg);
            db.SaveChanges();

            return "T";

        }

        public string DeleteGroup(int groupID)
        {
            //process group data delete
            DeviceGroup dg = db.DeviceGroups.Where(a => a.GroupID == groupID).FirstOrDefault();
            dg.SubGroup = devicesubgroup(dg);
            DeleteAllGroupData(dg);

            return "";
        }
        private void DeleteAllGroupData(DeviceGroup dg)
        {
            if (dg.SubGroup != null)
            {
                foreach (var subgroup in dg.SubGroup)
                {
                    DeleteAllGroupData(subgroup);
                }

            }
            if (dg.DeviceList != null)
            {
                foreach (var device in dg.DeviceList)
                {
                    db.DeviceGroupMembers.Remove(device);
                    db.SaveChanges();
                }
            }
            db.DeviceGroups.Remove(dg);
            db.SaveChanges();
            return;
        }

        [HttpPost]
        public string AddMultipleDevicesToGroup(returndata[] hostnames)
        {

            Guid tempGuid = Guid.NewGuid();
            if (hostnames != null)
            {
                foreach (var hostname in hostnames)
                {
                    DeviceGroupMember device = new DeviceGroupMember();
                    Guid g = Guid.NewGuid();
                    device.DeviceName = hostname.dname;
                    device.guid = g;
                    device.groupguid = hostname.dguid;
                    db.DeviceGroupMembers.Add(device);
                    db.SaveChanges();
                    tempGuid = hostname.dguid;
                }
            }
            return "T";

        }

        public class returndata
        {
            public string dname { get; set; }
            public Guid dguid { get; set; }
        }

        public ActionResult DeviceHome()
        {
            return View();
        }

    }

    public class NaturalComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            int lx = x.Length, ly = y.Length;

            for (int mx = 0, my = 0; mx < lx && my < ly; mx++, my++)
            {
                if (char.IsDigit(x[mx]) && char.IsDigit(y[my]))
                {
                    long vx = 0, vy = 0;

                    for (; mx < lx && char.IsDigit(x[mx]); mx++)
                        vx = vx * 10 + x[mx] - '0';

                    for (; my < ly && char.IsDigit(y[my]); my++)
                        vy = vy * 10 + y[my] - '0';

                    if (vx != vy)
                        return vx > vy ? 1 : -1;
                }

                if (mx < lx && my < ly && x[mx] != y[my])
                    return x[mx] > y[my] ? 1 : -1;
            }

            return lx - ly;
        }
    }




}
