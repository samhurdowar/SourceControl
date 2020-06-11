using F5PoolData;
using SourceControl.Common;
using SourceControl.Models.Db;
using SourceControl.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SourceControl.Controllers
{
    public class F5PoolController : Controller
    {
        [HttpPost]
        public string GetEIDMReport()
        {
            try
            {
                //var result = Helper.SendEmail("no-reply-cmsnetworktoolbox@entsvcscms.com", "sam.hurdowar@perspecta.com", "Test Email", "Testing email");

                PoolData poolData = new PoolData();
                PoolReport poolReport = new PoolReport();

                using (AppPMMEntities Db = new AppPMMEntities())
                {
                    // get data from servers
                    Db.Database.ExecuteSqlCommand("DELETE FROM PoolMemberInfoForWeb;DELETE FROM F5Pool;");
                    List<ActiveDevice> activeDevices = Db.Database.SqlQuery<ActiveDevice>("SELECT DISTINCT REPLACE(REPLACE(REPLACE(DeviceName,'.RDCMS.EDS.COM',''),'.RDCMS.EDS.com',''),'02','01') AS DeviceName FROM MasterDeviceList WHERE active = 1 ORDER BY DeviceName").ToList();

                    foreach (var device in activeDevices)
                    {
                        poolData.GetPool(Db, device.DeviceName);
                    }

                    // create pdf report
                    var html = poolReport.CreateReport(Db);
                    return html;
                }
            }
            catch (Exception ex)
            {
                Helper.LogError(ex.Message);
                return ex.Message;
            }
        }

        // C:\Users\xz07pm\Downloads\EIDM-Inventory.pdf
        [HttpPost]
        public string SendEIDMReport()
        {
            try
            {
                // check file 
                //var filePath = @"C:\Users\xz07pm\Downloads\EIDM-Inventory.pdf";
                var filePath = @"D:\WebSites\NetworkToolbox\Temp\EIDM-Inventory.pdf";
                if (System.IO.File.Exists(filePath))
                {
                    // rename file
                    var timeStamp = DateTime.Now.ToString("MMM d yyyy");
                    timeStamp = timeStamp.Replace(" ","_");
                    //var newFilePath = @"C:\Users\xz07pm\Downloads\EIDM-Inventory-" + timeStamp + ".pdf";
                    var newFilePath = @"D:\WebSites\NetworkToolbox\Temp\EIDM-Inventory-" + timeStamp + ".pdf";

                    System.IO.File.Move(filePath, newFilePath);


                    var emailBody = "EIDM Inventory Report - " + DateTime.Now.ToString("MMM d yyyy");
                    var subject = "EIDM Inventory Report - " + DateTime.Now.ToString("MMM d yyyy");
                    // send email  drichmond@uspsector.com
                    EmailService.SendEmail(SessionService.EmailFromAddress(), "drichmond@uspsector.com", SessionService.NetworkToolboxEmailAddress(), subject, emailBody, newFilePath);
                } 
                else
                {
                    return filePath + " does not exist.";
                }


                return "EIDM Inventory sent.";
            }
            catch (Exception ex)
            {
                Helper.LogError(ex.Message);
                return ex.Message;
            }
        }

    }
}