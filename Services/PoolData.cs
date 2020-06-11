using F5PoolData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using System.Configuration;
using SourceControl.Models.Db;
using SourceControl.Common;
using SourceControl.Services;

namespace F5PoolData
{
    public class PoolData
    {
        //"admin", "3nt3rpR1se2020!!"

        // rdcms\f5pmmsvc     1qazXSW@3edcVFR$    old: 9r9FG#SYxcfsrv9r9FGs


        public string ApiAccount;
        public string ApiPassword;

        public PoolData() {
            ApiAccount = SessionService.GetSiteSettingValue("pmm", "f5AdminSyncAccountUser");
            ApiPassword = SessionService.GetSiteSettingValue("pmm", "f5AdminSyncAccountPass");
        }

        public void GetPool(AppPMMEntities Db, string deviceName)
        {
            var responseContent = "";
            try
            {
                ICR icr = new ICR(ApiAccount, ApiPassword);

                IRestResponse response = icr.EstablishConnection("/mgmt/tm/ltm/pool", deviceName);

                responseContent = response.Content;

                //Helper.LogError(responseContent);

                var data = JsonConvert.DeserializeObject<RootPoolObject>(responseContent, new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Ignore, NullValueHandling = NullValueHandling.Ignore });
                if (data != null)
                {
                    var pools = data.items.ToList();
                    foreach (var pool in pools)
                    {
                        F5Pool f5Pool = new F5Pool();
                        f5Pool.DeviceName = deviceName;
                        f5Pool.Name = pool.name;
                        f5Pool.Monitor = pool.monitor;
                        f5Pool.LoadBalancingMode = pool.loadBalancingMode;
                        ParsePoolStats.PoolStatsRootObject stats = GetPoolStats(pool.name, deviceName);
                        f5Pool.StatusAvailabilityState = stats.entries.STATS.nestedStats.entries.statusavailabilityState.description;
                        f5Pool.StatusEnabledState = stats.entries.STATS.nestedStats.entries.statusenabledState.description;
                        f5Pool.StatusStatusReason = stats.entries.STATS.nestedStats.entries.statusstatusReason.description;
                        f5Pool.CurSessions = stats.entries.STATS.nestedStats.entries.curSessions.value;
                        f5Pool.TotRequests = stats.entries.STATS.nestedStats.entries.totRequests.value;
                        Db.F5Pool.Add(f5Pool);

                        Db.SaveChanges();

                        // get pool members
                        GetPoolMemberStats(Db, f5Pool.F5PoolId, pool.name, deviceName);
                    }


                }
            }
            catch (Exception ex)
            {
                Helper.LogError(ex.Message + "\r\n" + responseContent + "\r\n");
            }

        }

        private ParsePoolStats.PoolStatsRootObject GetPoolStats(string name, string deviceName)
        {
            var responseContent = "";
            try
            {
                ICR icr = new ICR(ApiAccount, ApiPassword);
                ParsePoolStats.url url = new ParsePoolStats.url();
                url.resource = url.resource + name + "/stats";
                string resource = url.resource;
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore
                };
                ParsePoolStats.PoolStatsRootObject data = new ParsePoolStats.PoolStatsRootObject();
                IRestResponse response = icr.EstablishConnection(resource, deviceName);
                responseContent = response.Content;
                responseContent = responseContent.Replace("connqAll.ageEdm", "connqAllageEdm");
                responseContent = responseContent.Replace("connqAll.ageEma", "connqAllageEma");
                responseContent = responseContent.Replace("connqAll.ageHead", "connqAllageHead");
                responseContent = responseContent.Replace("connqAll.ageMax", "connqAllageMax");
                responseContent = responseContent.Replace("connqAll.depth", "connqAlldepth");
                responseContent = responseContent.Replace("connqAll.serviced", "connqAllserviced");
                responseContent = responseContent.Replace("connq.ageEdm", "connqageEdm");
                responseContent = responseContent.Replace("connq.ageEma", "connqageEma");
                responseContent = responseContent.Replace("connq.ageHead", "connqageHead");
                responseContent = responseContent.Replace("connq.ageMax", "connqageMax");
                responseContent = responseContent.Replace("connq.depth", "connqdepth");
                responseContent = responseContent.Replace("connq.serviced", "connqserviced");
                responseContent = responseContent.Replace("serverside.bitsIn", "serversidebitsIn");
                responseContent = responseContent.Replace("serverside.bitsOut", "serversidebitsOut");
                responseContent = responseContent.Replace("serverside.curConns", "serversidecurConns");
                responseContent = responseContent.Replace("serverside.maxConns", "serversidemaxConns");
                responseContent = responseContent.Replace("serverside.pktsIn", "serversidepktsIn");
                responseContent = responseContent.Replace("serverside.pktsOut", "serversidepktsOut");
                responseContent = responseContent.Replace("serverside.totConns", "serversidetotConns");
                responseContent = responseContent.Replace("status.availabilityState", "statusavailabilityState");
                responseContent = responseContent.Replace("status.enabledState", "statusenabledState");
                responseContent = responseContent.Replace("status.statusReason", "statusstatusReason");
                int start = responseContent.IndexOf("entries\":{\"h") + "entries\":{\"".Length;
                int end = responseContent.IndexOf(":{\"nestedStats\"");
                int total = end - start - 1;
                responseContent = responseContent.Remove(start, total);
                responseContent = responseContent.Insert(start, "STATS");

                data = JsonConvert.DeserializeObject<ParsePoolStats.PoolStatsRootObject>(responseContent, settings);
                return data;
            }
            catch (Exception ex)
            {
                Helper.LogError(ex.Message + "\r\n" + responseContent + "\r\n");
            }
            return null;
        }


        private void GetPoolMemberStats(AppPMMEntities Db, int f5PoolId, string poolName, string deviceName)
        {
            var responseContent = "";
            try
            {
                ICR icr = new ICR(ApiAccount, ApiPassword);
                string resource = "/mgmt/tm/ltm/pool/~Common~" + poolName + "/members/stats";
                List<PoolMemberInfoForWeb> poolMembers = new List<PoolMemberInfoForWeb>();
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore
                };

                IRestResponse response = icr.EstablishConnection(resource, deviceName);
                responseContent = response.Content;

                if (responseContent.Contains("nestedStats"))
                {
                    responseContent = responseContent.Replace("connq.ageEdm", "connqageEdm");
                    responseContent = responseContent.Replace("connq.ageEma", "connqageEma");
                    responseContent = responseContent.Replace("connq.ageHead", "connqageHead");
                    responseContent = responseContent.Replace("connq.ageMax", "connqageMax");
                    responseContent = responseContent.Replace("connq.depth", "connqdepth");
                    responseContent = responseContent.Replace("connq.serviced", "connqserviced");
                    responseContent = responseContent.Replace("serverside.bitsIn", "serversidebitsIn");
                    responseContent = responseContent.Replace("serverside.bitsOut", "serversidebitsOut");
                    responseContent = responseContent.Replace("serverside.curConns", "serversidecurConns");
                    responseContent = responseContent.Replace("serverside.maxConns", "serversidemaxConns");
                    responseContent = responseContent.Replace("serverside.pktsIn", "serversidepktsIn");
                    responseContent = responseContent.Replace("serverside.pktsOut", "serversidepktsOut");
                    responseContent = responseContent.Replace("serverside.totConns", "serversidetotConns");
                    responseContent = responseContent.Replace("status.availabilityState", "statusavailabilityState");
                    responseContent = responseContent.Replace("status.enabledState", "statusenabledState");
                    responseContent = responseContent.Replace("status.statusReason", "statusstatusReason");
                    responseContent = responseContent.Replace("/Common/", "");
                    responseContent = responseContent.Replace("~Common~", "");

                    var data = JsonConvert.DeserializeObject<dynamic>(responseContent, settings);
                    foreach (var server in data.entries)
                    {
                        PoolMemberInfoForWeb poolMember = new PoolMemberInfoForWeb();

                        poolMember.deviceName = deviceName;
                        poolMember.poolName = poolName;
                        poolMember.addr = server.Value.nestedStats.entries.addr.description.Value.ToString();
                        poolMember.connqageEdm = server.Value.nestedStats.entries.connqageEdm.value.Value.ToString();
                        poolMember.connqageEma = server.Value.nestedStats.entries.connqageEma.value.Value.ToString();
                        poolMember.connqageHead = server.Value.nestedStats.entries.connqageHead.value.Value.ToString();
                        poolMember.connqageMax = server.Value.nestedStats.entries.connqageMax.value.Value.ToString();
                        poolMember.connqdepth = server.Value.nestedStats.entries.connqdepth.value.Value.ToString();
                        poolMember.connqserviced = server.Value.nestedStats.entries.connqserviced.value.Value.ToString();
                        poolMember.curSessions = server.Value.nestedStats.entries.curSessions.value.Value.ToString();
                        poolMember.monitorRule = server.Value.nestedStats.entries.monitorRule.description.Value;
                        poolMember.monitorStatus = server.Value.nestedStats.entries.monitorStatus.description.Value.ToString();
                        poolMember.nodeName = server.Value.nestedStats.entries.nodeName.description.Value.ToString();
                        poolMember.port = server.Value.nestedStats.entries.port.value.Value.ToString();
                        poolMember.serversidebitsIn = server.Value.nestedStats.entries.serversidebitsIn.value.Value.ToString();
                        poolMember.serversidebitsOut = server.Value.nestedStats.entries.serversidebitsOut.value.Value.ToString();
                        poolMember.serversidecurConns = server.Value.nestedStats.entries.serversidecurConns.value.Value.ToString();
                        poolMember.serversidemaxConns = server.Value.nestedStats.entries.serversidemaxConns.value.Value.ToString();
                        poolMember.serversidepktsIn = server.Value.nestedStats.entries.serversidepktsIn.value.Value.ToString();
                        poolMember.serversidepktsOut = server.Value.nestedStats.entries.serversidepktsOut.value.Value.ToString();
                        poolMember.serversidetotConns = server.Value.nestedStats.entries.serversidetotConns.value.Value.ToString();
                        poolMember.sessionStatus = server.Value.nestedStats.entries.sessionStatus.description.Value.ToString();
                        poolMember.statusavailabilityState = server.Value.nestedStats.entries.statusavailabilityState.description.Value.ToString();
                        poolMember.statusenabledState = server.Value.nestedStats.entries.statusenabledState.description.Value.ToString();
                        poolMember.statusstatusReason = server.Value.nestedStats.entries.statusstatusReason.description.Value.ToString();
                        poolMember.totRequests = server.Value.nestedStats.entries.totRequests.value.Value.ToString();
                        poolMember.F5PoolId = f5PoolId;
                        poolMembers.Add(poolMember);
                    }

                    if (poolMembers.Count > 0)
                    {
                        Db.PoolMemberInfoForWebs.AddRange(poolMembers);
                        Db.SaveChanges();
                    }

                }

            }
            catch (Exception ex)
            {
                Helper.LogError(ex.Message + "\r\n" + responseContent + "\r\n");
            }
        }

    }

    public class ICR
    {

        protected string ApiAccount_;
        protected string ApiPassword_;


        public ICR (string apiAccount, string apiPassword)
        {
            ApiAccount_ = apiAccount;
            ApiPassword_ = apiPassword;
        }

        public IRestResponse EstablishConnection(string uri, string deviceName)
        {
            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

                string url = "Https://" + deviceName;
                RestClient client = new RestClient();
                client.Authenticator = new HttpBasicAuthenticator(ApiAccount_, ApiPassword_);
                client.BaseUrl = new Uri(url);
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                var request = new RestRequest();
                request.Resource = uri;
                request.RequestFormat = DataFormat.Json;
                IRestResponse response = client.Execute(request);

                return response;
            }
            catch (Exception ex)
            {
                Helper.LogError(ex.Message);
            }
            return null;
        }


    }

}
