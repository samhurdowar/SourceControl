using SourceControl.Models.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace SourceControl.Services
{
    public static class AppService
    {
        public static string F5LoadCharts
        {
            get
            {
                using (TargetEntities Db = new TargetEntities("F5DataEntities"))
                {
                    var i = 0;
                    var jobName = "";
                    var series = "";
                    var categories = "";
                    var color = "a0b0c0";
                    StringBuilder sb = new StringBuilder();
                    var loads = Db.Database.SqlQuery<JobLoad>("SELECT 'Job Name: ' + SUBSTRING(B.jobname,1,10) AS JobName, A.DeviceTask, SUM(A.LoadCount) AS LoadCount FROM JobLogTask A JOIN JobLog B ON A.ParentJobGuid = B.guid WHERE B.guid IN (SELECT TOP 2 guid FROM JobLog ORDER BY startTime DESC) GROUP BY B.jobname, A.DeviceTask ORDER BY B.jobname DESC, A.DeviceTask").ToList();
                    if (loads.Count > 0)
                    {
                        jobName = loads[0].JobName;


                        foreach (var load in loads)
                        {
                            if (jobName != load.JobName)
                            {
                                i++;
                                if (i == 2)
                                {
                                    color = "90ca78";
                                }
                                series = series.Substring(0, series.Length - 2);
                                categories = categories.Substring(0, categories.Length - 2);
                                sb.AppendLine("$(\"#chart" + i + "\").kendoChart({");
                                sb.AppendLine("title: {text: \"" + jobName + "\" },");
                                sb.AppendLine("series: [{ data: [ " + series + " ], color: \"#" + color + "\" }],");
                                sb.AppendLine("categoryAxis: {categories: [ " + categories + " ] }");
                                sb.AppendLine("});");

                                jobName = load.JobName;
                                series = "";
                                categories = "";
                            }
                            series += load.LoadCount + ", ";
                            categories += "\"" + GetShortName(load.DeviceTask) + "\", ";
                        }

                        i++;
                        sb.AppendLine("$(\"#chart" + i + "\").kendoChart({");
                        sb.AppendLine("title: {text: \"" + jobName + "\" },");
                        sb.AppendLine("series: [{ data: [ " + series + " ], color: \"#efe9a2\"}],");
                        sb.AppendLine("categoryAxis: {categories: [ " + categories + " ] }");
                        sb.AppendLine("});");
                    }

                    return sb.ToString();
                }

            }
        }



        public static string F5PoolMemberChartData
        {
            get
            {
                using (TargetEntities Db = new TargetEntities("F5DataEntities"))
                {
                    var i = 0;
                    var stringReturn = "";
                    var color = "a0b0c0";
                    StringBuilder sb = new StringBuilder();

                    //var totalMemberCount = Db.Database.SqlQuery<double>("SELECT count(1) FROM PoolMember").FirstOrDefault();

                    var members = Db.Database.SqlQuery<PoolMemberState>("SELECT state AS State, count(1) StateCount FROM PoolMember WHERE state <> 'fqdn-checking' GROUP BY state").ToList();
                    if (members.Count > 0)
                    {

                        foreach (var member in members)
                        {
                            i++;
                            switch (i)
                            {
                                case 1:
                                    color = "9de219"; break;
                                case 2:
                                    color = "90cc38"; break;
                                case 3:
                                    color = "068c35"; break;
                                case 4:
                                    color = "006634"; break;
                                case 5:
                                    color = "004d38"; break;
                                case 6:
                                    color = "033939"; break;
                                case 7:
                                    color = "03cc39"; break;
                                case 8:
                                    color = "0339cc"; break;
                                case 9:
                                    color = "03ee39"; break;
                                case 10:
                                    color = "cc3939"; break;
                                default:
                                    break;
                            }

                            //double perc = (Convert.ToDouble(member.StateCount) / totalMemberCount) * 100;

                            sb.Append("{ category: \"" + member.State + "\", value: " + member.StateCount + ", color: \"#" + color + "\" },");
                        }
                        stringReturn = sb.ToString();
                        stringReturn = stringReturn.Substring(0, stringReturn.Length - 1);
                    }

                    return stringReturn;
                }

            }
        }


        public static string GetShortName(string longName)
        {
            var shortName = longName;
            switch (shortName)
            {

                case "Cert Data":
                    shortName = "Cert"; break;
                case "Client SLL Data":
                    shortName = "ClientSLL"; break;
                case "Device Data":
                    shortName = "Device"; break;
                case "Http Monitor Data":
                    shortName = "Http"; break;
                case "iRule Data":
                    shortName = "iRule"; break;
                case "Network Route Congiuration Task":
                    shortName = "Route"; break;
                case "Network Self IP Configuration task":
                    shortName = "SelfIP"; break;
                case "Network VLAN Configuration Task":
                    shortName = "VLAN"; break;
                case "Node Data":
                    shortName = "Node"; break;
                case "Persist Cookie Data":
                    shortName = "Cookie"; break;
                case "Persist Dest-Addr Data":
                    shortName = "Dest"; break;
                case "Persist Source-Addr Data":
                    shortName = "Source"; break;
                case "Persist Universal Data":
                    shortName = "Universal"; break;
                case "Pool Data":
                    shortName = "Pool"; break;
                case "SNAT Pool Data Task":
                    shortName = "SNAT"; break;
                case "SSL Client Cert Files Data":
                    shortName = "CertFiles"; break;
                case "VIP Data":
                    shortName = "VIP"; break;
                default:
                    break;
            }

            return shortName;
        }

    }

    public class JobLoad
    {
        public string JobName { get; set; }
        public string DeviceTask { get; set; }
        public int LoadCount { get; set; }
    }


    public class PoolMemberState
    {
        public string State { get; set; }
        public int StateCount { get; set; }
    }




}