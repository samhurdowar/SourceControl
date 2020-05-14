using F5PoolData.Models;
using SourceControl.Common;
using SourceControl.Models.Db;
//using PdfSharp;
//using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace F5PoolData
{
    public class PoolReport
    {
        public string CreateReport(AppPMMEntities Db)
        {
            var responseContent = "";
            try
            {

                // generate html
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("<center>");
                sb.AppendLine("<table border='1'>");

                
                var deviceName = "";
                var shortPoolName = "";

                var poolName = "";

                var f5Pools_ = Db.F5Pool.Where(w => w.Name.ToUpper().Contains("EIDM")).OrderBy(o => o.DeviceName).ToList();

                var f5Pools = (from a in f5Pools_
                                join b in Db.MasterDeviceLists on a.DeviceName equals b.pairName 
                                into aa
                                from bb in aa.DefaultIfEmpty()
                                select new { a, DeviceIp = (bb == null) ? "" : bb.mgmtIP }).ToList();


                foreach (var f5Pool in f5Pools)
                {
                    if (deviceName != f5Pool.a.DeviceName)
                    {
                        // device header
                        sb.AppendLine("<tr>");
                        sb.AppendLine("<td style='background-color:#80bfff;color:#00004d;text-align:center;font-weight:bold;font-family:arial;padding:4px;font-size:1.1em;' colspan='8'>");
                        sb.AppendLine(f5Pool.a.DeviceName + ".rdcms.eds.com");
                        sb.AppendLine("</td>");
                        sb.AppendLine("</tr>");
                        deviceName = f5Pool.a.DeviceName;
                    }

                    // pool header

                    poolName = f5Pool.a.Name.ToLower();
                    shortPoolName = f5Pool.a.Name;


                    if (poolName.Contains("test") || poolName.Contains("dev")) {
                        shortPoolName = "EIDM Dev/Test";
                    } 
                    else if (poolName.Contains("prod"))
                    {
                        shortPoolName = "EIDM Production";
                    }
                    else if (poolName.Contains("imp"))
                    {
                        shortPoolName = "EIDM Implementation";
                    }

                    sb.AppendLine("<tr>");
                    sb.AppendLine("<td style='background-color:#235A7D;color:#ffffff;font-weight:bold;font-family:arial;padding:3px;font-size:.9em;white-space:nowrap;' nowrap>" + f5Pool.a.DeviceName + " - " + shortPoolName + "</td>");
                    sb.AppendLine("<td style='background-color:#235A7D;color:#ffffff;font-weight:bold;font-family:arial;padding:3px;font-size:.9em;'>Status</td>");
                    sb.AppendLine("<td style='background-color:#235A7D;color:#ffffff;font-weight:bold;font-family:arial;padding:3px;font-size:.9em;'>VIP IP</td>");
                    sb.AppendLine("<td style='background-color:#235A7D;color:#ffffff;font-weight:bold;font-family:arial;padding:3px;font-size:.9em;'>RD</td>");
                    sb.AppendLine("<td style='background-color:#235A7D;color:#ffffff;font-weight:bold;font-family:arial;padding:3px;font-size:.9em;'>Port</td>");
                    sb.AppendLine("<td style='background-color:#235A7D;color:#ffffff;font-weight:bold;font-family:arial;padding:3px;font-size:.9em;'>SNAT</td>");
                    sb.AppendLine("<td style='background-color:#235A7D;color:#ffffff;font-weight:bold;font-family:arial;padding:3px;font-size:.9em;'>SNAT Pool</td>");
                    sb.AppendLine("<td style='background-color:#235A7D;color:#ffffff;font-weight:bold;font-family:arial;padding:3px;font-size:.9em;'>rules</td>");
                    sb.AppendLine("</tr>");

                    // pool header II
                    sb.AppendLine("<tr>");
                    sb.AppendLine("<td style='background-color:#CFD5F9;color:#0E2838;font-weight:bold;font-family:arial;padding:3px;font-size:.9em;'>" + poolName.Replace("pool_", "vs_") + "</td>");
                    sb.AppendLine("<td style='background-color:#CFD5F9;color:#0E2838;font-weight:bold;font-family:arial;padding:3px;font-size:.9em;'>" + f5Pool.a.StatusAvailabilityState + "</td>");
                    sb.AppendLine("<td style='background-color:#CFD5F9;color:#0E2838;font-weight:bold;font-family:arial;padding:3px;font-size:.9em;'>" + f5Pool.DeviceIp + "</td>");
                    sb.AppendLine("<td style='background-color:#CFD5F9;color:#0E2838;font-weight:bold;font-family:arial;padding:3px;font-size:.9em;'>?RD</td>");
                    sb.AppendLine("<td style='background-color:#CFD5F9;color:#0E2838;font-weight:bold;font-family:arial;padding:3px;font-size:.9em;'>?Port</td>");
                    sb.AppendLine("<td style='background-color:#CFD5F9;color:#0E2838;font-weight:bold;font-family:arial;padding:3px;font-size:.9em;'>?SNAT</td>");
                    sb.AppendLine("<td style='background-color:#CFD5F9;color:#0E2838;font-weight:bold;font-family:arial;padding:3px;font-size:.9em;'>?SNAT Pool</td>");
                    sb.AppendLine("<td style='background-color:#CFD5F9;color:#0E2838;font-weight:bold;font-family:arial;padding:3px;font-size:.9em;'>?rules</td>");
                    sb.AppendLine("</tr>");



                    // members
                    if (f5Pool.a.PoolMemberInfoForWebs.Count > 0)
                    {
                        sb.AppendLine("<tr valign='top'>");
                        sb.AppendLine("<td style='background-color:#cceeff;color:#0E2838;font-weight:bold;font-family:arial;padding:3px;font-size:.9em;white-space:nowrap;' rowspan='" + (f5Pool.a.PoolMemberInfoForWebs.Count + 2) + "' valign='top' nowrap>Pool Name: " + poolName + "</td>");
                        sb.AppendLine("</tr>");


                        sb.AppendLine("<tr>");
                        sb.AppendLine("<td style='background-color:#878CAB;color:#0E2838;font-weight:bold;font-family:arial;padding:3px;font-size:.9em;'>Server Name</td>");
                        sb.AppendLine("<td style='background-color:#878CAB;color:#0E2838;font-weight:bold;font-family:arial;padding:3px;font-size:.9em;'>IP Address</td>");
                        sb.AppendLine("<td style='background-color:#878CAB;color:#0E2838;font-weight:bold;font-family:arial;padding:3px;font-size:.9em;'>RD</td>");
                        sb.AppendLine("<td style='background-color:#878CAB;color:#0E2838;font-weight:bold;font-family:arial;padding:3px;font-size:.9em;'>Port</td>");
                        sb.AppendLine("<td style='background-color:#878CAB;color:#0E2838;font-weight:bold;font-family:arial;padding:3px;font-size:.9em;'>Monitor</td>");
                        sb.AppendLine("<td style='background-color:#878CAB;color:#0E2838;font-weight:bold;font-family:arial;padding:3px;font-size:.9em;'>State</td>");
                        sb.AppendLine("<td style='background-color:#878CAB;color:#0E2838;font-weight:bold;font-family:arial;padding:3px;font-size:.9em;'>Server Session</td>");
                        sb.AppendLine("</tr>");

                        var backgroundColor = "";
                        foreach (var member in f5Pool.a.PoolMemberInfoForWebs)
                        {
                            backgroundColor = "#cceeff";
                            if (member.monitorStatus == "down")
                            {
                                backgroundColor = "#FA6359";
                            }
                            sb.AppendLine("<tr>");
                            sb.AppendLine("<td style='background-color:" + backgroundColor + ";color:#0E2838;font-family:arial;padding:3px;font-size:.9em;'>" + member.nodeName + "</td>");
                            sb.AppendLine("<td style='background-color:" + backgroundColor + ";color:#0E2838;font-family:arial;padding:3px;font-size:.9em;'>" + member.addr + "</td>");
                            sb.AppendLine("<td style='background-color:" + backgroundColor + ";color:#0E2838;font-family:arial;padding:3px;font-size:.9em;'>RD</td>");
                            sb.AppendLine("<td style='background-color:" + backgroundColor + ";color:#0E2838;font-family:arial;padding:3px;font-size:.9em;'>" + member.port + "</td>");
                            sb.AppendLine("<td style='background-color:" + backgroundColor + ";color:#0E2838;font-family:arial;padding:3px;font-size:.9em;'>?Monitor</td>");
                            sb.AppendLine("<td style='background-color:" + backgroundColor + ";color:#0E2838;font-family:arial;padding:3px;font-size:.9em;'>" + member.monitorStatus + "</td>");
                            sb.AppendLine("<td style='background-color:" + backgroundColor + ";color:#0E2838;font-family:arial;padding:3px;font-size:.9em;'>monitor-" + member.sessionStatus + "</td>");
                            sb.AppendLine("</tr>");
                        }


                    }


                }

                sb.AppendLine("</table>");

                sb.AppendLine("</center>");

                var html = sb.ToString();


                return html;
                // generate html file
                //using (StreamWriter outfile = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\document.html"))
                //{
                //    outfile.Write(html);
                //}

            }
            catch (Exception ex)
            {
                Helper.LogError(ex.Message + "\r\n" + responseContent + "\r\n");
            }

            return "";
        }



    }
}
