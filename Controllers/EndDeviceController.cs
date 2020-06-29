using OfficeOpenXml;
using SourceControl.App.NetworkCafe;
using SourceControl.Models.Db;
using SourceControl.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace SourceControl.Controllers
{
	public class EndDeviceController : Controller
	{
		[HttpPost]
		public string GetPortResults(string searchString, string searchBy)
		{
			var results = GetPortResultsObject(searchString, searchBy);

			var json = new JavaScriptSerializer().Serialize(results);
			return json;
		}

		public List<SwitchPort> GetPortResultsObject(string searchString, string searchBy)
		{
			var likeClause = "";
			searchString = searchString.ToLower();
			if (searchString.Contains(","))
			{
				var words = searchString.Split(',').ToList();
				foreach (var word in words)
				{
					likeClause += " HostName LIKE '%" + searchString.Replace("'", "''") + "%' OR ";
				}
				likeClause = likeClause.Substring(0, likeClause.Length - 3);
			}
			else
			{
				likeClause = " HostName LIKE '%" + searchString.Replace("'", "''") + "%' ";
			}

			using (TargetEntities targetDb = new TargetEntities("NetworkCafeEntities"))
			{
				var sql = "SELECT * FROM SwitchPorts WHERE " + likeClause;
				var results = targetDb.Database.SqlQuery<SwitchPort>(sql).ToList();
				return results;
			}
		}

		[HttpPost]
		public string GetDNSResults(string searchString, string searchBy)
		{
			var results = GetDNSResultsObject(searchString, searchBy);
			var json = new JavaScriptSerializer().Serialize(results);
			return json;
		}

		public List<RestConnector.HostData> GetDNSResultsObject(string searchString, string searchBy)
		{
			searchString = searchString.ToLower();

			RestConnector restConnector = new RestConnector();

			List<RestConnector.HostData> temp = new List<RestConnector.HostData>();
			List<RestConnector.HostData> results = new List<RestConnector.HostData>();
			List<string> searchLists = new List<string>();

			if (searchString.Contains(","))
			{
				searchLists = searchString.Split(',').ToList();
			}
			else
			{
				searchLists.Add(searchString);
			}

			foreach (var searchList in searchLists)
			{
				temp = restConnector.GetHostDNS(searchList, searchBy);
				results.AddRange(temp);
			}

			return results;
		}

		[HttpPost]
		public string GetVIPResults(string searchString, string searchBy)
		{
			var results = GetVIPResultsObject(searchString, searchBy);
			var json = new JavaScriptSerializer().Serialize(results);
			return json;
		}

		public List<f5SearchResults> GetVIPResultsObject(string searchString, string searchBy)
		{
			var likeClause = "";
			searchString = searchString.ToLower();
			if (searchString.Contains(","))
			{
				var words = searchString.Split(',').ToList();
				foreach (var word in words)
				{
					likeClause += " PoolMember.Name LIKE '%" + searchString.Replace("'", "''") + "%' OR ";
				}
				likeClause = likeClause.Substring(0, likeClause.Length - 3);
			}
			else
			{
				likeClause = " PoolMember.Name LIKE '%" + searchString.Replace("'", "''") + "%' ";
			}

			// get lastest job guid in f5data database 
			var jobGuid = "";
			using (TargetEntities targetDb = new TargetEntities("F5DataEntities"))
			{
				jobGuid = targetDb.Database.SqlQuery<string>("SELECT CAST(guid AS varchar(250)) AS guid FROM JobLog ORDER BY endTime DESC").FirstOrDefault();


				var sql = "SELECT DeviceData.name AS F5Name, VIP.name AS VIPNAME, REPLACE(VIP.destination,'/Common/','') AS VIPIP, Pool.name AS PoolName, PoolMember.Name AS NodeName,PoolMember.address AS NodeIP " +
						"FROM DeviceData INNER JOIN " +
						"JobLog ON DeviceData.jobguid = JobLog.guid INNER JOIN " +
						"VIP ON DeviceData.jobguid = VIP.jobguid AND DeviceData.hostname = VIP.f5HostName INNER JOIN " +
						"Pool ON VIP.jobguid = Pool.jobguid AND VIP.f5HostName = Pool.f5HostName AND VIP.pool = Pool.fullpath INNER JOIN " +
						"PoolMember ON Pool.guid = PoolMember.PoolGuid " +
						"WHERE (DeviceData.failoverState = N'active') " +
						"AND (DeviceData.selfDevice = N'true') " +
						"AND (JobLog.guid = '" + jobGuid + "' AND (" + likeClause + ") )";

				var results = targetDb.Database.SqlQuery<f5SearchResults>(sql).ToList();
				return results;
			}

		}

		[HttpPost]
		public string GetVMResults(string searchString, string searchBy)
		{
			var results = GetVMResultsObject(searchString, searchBy);
			var json = new JavaScriptSerializer().Serialize(results);
			return json;
		}

		public List<VMPhoneBook> GetVMResultsObject(string searchString, string searchBy)
		{

			var likeClause = "";
			searchString = searchString.ToLower();
			if (searchString.Contains(","))
			{
				var words = searchString.Split(',').ToList();

				if (searchBy == "ipv4addr")
                {
					foreach (var word in words)
					{
						likeClause += " OR (Mgt_IP LIKE '%" + word.Replace("'", "''") + "%' OR Prod_IP LIKE '%" + word.Replace("'", "''") + "%') ";
					}
				} else
                {
					foreach (var word in words)
					{
						likeClause += " OR ESXi_Host LIKE '%" + word.Replace("'", "''") + "%' ";
					}
				}
			}
			else
			{
				if (searchBy == "ipv4addr")
                {
					likeClause = " OR (Mgt_IP LIKE '%" + searchString.Replace("'", "''") + "%' OR Prod_IP LIKE '%" + searchString.Replace("'", "''") + "%') ";
				} else
                {
					likeClause = " OR ESXi_Host LIKE '%" + searchString.Replace("'", "''") + "%' ";
				}	
			}

			using (TargetEntities targetDb = new TargetEntities("NetworkCafeEntities"))
			{
				var sql = "SELECT * FROM vmPhoneBook WHERE ServerName like '%" + searchString + "%' " + likeClause;
				var results = targetDb.Database.SqlQuery<VMPhoneBook>(sql).ToList();
				return results;
			}
		}


		public ActionResult DownloadHostSearch(string searchString, string searchBy)
		{
			if (searchString != null && searchString != "")
			{
				var stream = ExportDataToExcel(searchString, searchBy);
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

		[HttpPost]
		public string EmailHostSearch(string searchString, string searchBy)
		{
			try
			{
				if (searchString != null && searchString != "")
				{
					var msg = "T";
					var stream = ExportDataToExcel(searchString, searchBy);
					var memoryStream = stream as MemoryStream;
					msg = SendMailAttachment(memoryStream);

					return msg;
				}
				return "F";
			}
			catch (Exception ex)
			{
				return ex.Message;
			}
		}

		private string SendMailAttachment(MemoryStream stream)
		{
			try
			{
				AppUser user = (AppUser)Session["sec.CurrentUser"];
				string filename = "results.xlsx";


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
				return "T";
			}
			catch (Exception ex)
			{
				return ex.Message;
			}
		}


		public Stream ExportDataToExcel(string searchString, string searchBy, Stream stream = null)
		{
			AppUser user = (AppUser)Session["sec.CurrentUser"];

			//get switch data
			List<SwitchPort> hostPorts = GetPortResultsObject(searchString, searchBy);

			//get dns data
			List<RestConnector.HostData> dnsData = GetDNSResultsObject(searchString, searchBy);

			//get f5 data
			var f5Data = GetVIPResultsObject(searchString, searchBy);

			//esx data
			var vmList = GetVMResultsObject(searchString, searchBy);

			//build spreadsheet
			using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
			{
				excelPackage.Workbook.Properties.Author = user.FirstName + " " + user.LastName;
				excelPackage.Workbook.Properties.Title = "Host Search Results";
				excelPackage.Workbook.Properties.Comments = "Data found for the specific end device search.";

				if (searchBy == "ipv4addr")
                {
					excelPackage.Workbook.Worksheets.Add("DNS Data");
					var worksheet2 = excelPackage.Workbook.Worksheets[1];
					excelPackage.Workbook.Worksheets.Add("ESX Data");
					var worksheet4 = excelPackage.Workbook.Worksheets[2];

					worksheet2.Cells[1, 1].LoadFromCollection(dnsData, true);
					worksheet4.Cells[1, 1].LoadFromCollection(vmList, true);
				} else
                {
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
				}


				excelPackage.Save();
				return excelPackage.Stream;

			}
		}
	}

}

