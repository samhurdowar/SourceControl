using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using SourceControl.Common;
using SourceControl.Models.Db;
using SourceControl.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace SourceControl.App.NetworkCafe
{
	public class RestConnector
	{
		public List<HostData> GetHostDNS(string hostname)
		{
            try
            {
                var json = "";
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore
                };

                Helper.LogError("GetHostDNS()    SessionService.IsLocal=" + SessionService.IsLocal);

                if (SessionService.IsLocal)
                {
                    string testFilePath = @"D:\Perspecta\SourceControl\App_Data\DNSResults.txt";
                    json = System.IO.File.ReadAllText(testFilePath);
                }
                else
                {
                    InfoBloxDNSHostSearch.url url = new InfoBloxDNSHostSearch.url();
                    string serverAddress = url.server;
                    string resource = url.resource + hostname; //  Https://10.10.248.40//wapi/v1.2/record:host?name~=ustlvcmsp2167     toolboxapi/1qazXSW@3edcVFR$5tgb  ustlvcmsp2349   GetHostDNS()    resource=Https://10.10.248.40//wapi/v1.2/record:host?name~=ustlvcmsp2167

                    Helper.LogError("GetHostDNS()    resource=" + serverAddress + url.resource + hostname);

                    IRestResponse response = EstablishInfoBloxConnection(resource, serverAddress);
                    json = response.Content;
                }

                Helper.LogError("GetHostDNS()    json=" + json);

                List<HostData> results = new List<HostData>();
                var dnsResults = JsonConvert.DeserializeObject<List<InfoBloxDNSHostSearch.DnsResult>>(json, settings);

                if (dnsResults != null)
                {
                    foreach (var dnsResult in dnsResults)
                    {
                        if (dnsResult.ipv4addrs != null)
                        {
                            foreach (var ipAddress in dnsResult.ipv4addrs)
                            {
                                HostData n = new HostData();
                                n.host = ipAddress.host;
                                n.ipAddress = ipAddress.ipv4addr;
                                n.ipType = "ipv4";
                                results.Add(n);
                            }
                        }

                        if (dnsResult.ipv6addrs != null)
                        {
                            foreach (var ipAddress in dnsResult.ipv6addrs)
                            {
                                HostData n = new HostData();
                                n.host = ipAddress.host;
                                n.ipAddress = ipAddress.ipv6addr;
                                n.ipType = "ipv6";
                                results.Add(n);
                            }
                        }
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                Helper.LogError(ex.Message + "\r\n" + ex.StackTrace);
                return null;
            }


		}

		public List<IPData> getIPDNS(string hostname)
		{
			InfoBloxIPSearch.url url = new InfoBloxIPSearch.url();
			string serverAddress = url.server;
			string resource = url.resource + hostname;
			JsonSerializerSettings settings = new JsonSerializerSettings
			{
				MissingMemberHandling = MissingMemberHandling.Ignore,
				NullValueHandling = NullValueHandling.Ignore
			};
			List<InfoBloxIPSearch.RootIPSearch> data = new List<InfoBloxIPSearch.RootIPSearch>();
			IRestResponse response = EstablishInfoBloxConnection(resource, serverAddress);
			data = JsonConvert.DeserializeObject<List<InfoBloxIPSearch.RootIPSearch>>(response.Content, settings);


			List<IPData> results = new List<IPData>();
			if (data != null)
			{
				foreach (var item in data)
				{
					IPData ipdata = new IPData();
					ipdata.names = item.names.ToList();
					ipdata.ip_address = item.ip_address;
					ipdata.network = item.network;
					results.Add(ipdata);
				}

			}

			return results;
		}

		private IRestResponse EstablishInfoBloxConnection(string uri, string Hostname)
		{
            try
            {
                string InfoBloxRestAPIUser = "";
                string InfoBloxRestAPIPass = "";

                using (TargetEntities targetDb = new TargetEntities("NetworkCafeEntities"))
                {
                    InfoBloxRestAPIUser = targetDb.Database.SqlQuery<string>("Select SettingValue From SiteSettings Where SettingName = 'InfoBloxRestAPIUser'").FirstOrDefault();
                    InfoBloxRestAPIPass = targetDb.Database.SqlQuery<string>("Select SettingValue From SiteSettings Where SettingName = 'InfoBloxRestAPIPass'").FirstOrDefault();
                }

                Helper.LogError("establishInfoBloxConnection()    InfoBloxRestAPIUser=" + InfoBloxRestAPIUser + "/" + InfoBloxRestAPIPass);

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                string url = "Https://" + Hostname;
                RestClient client = new RestClient();
                client.Authenticator = new HttpBasicAuthenticator(InfoBloxRestAPIUser, InfoBloxRestAPIPass);
                client.BaseUrl = new Uri(url);
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                var request = new RestRequest();
                request.Resource = uri;
                request.RequestFormat = DataFormat.Json;
                IRestResponse response = client.Execute(request);


                if (response != null && response.ErrorMessage != null && response.StatusDescription != null && (response.ErrorMessage.Length > 5 || response.StatusDescription.Length > 5))
                {
                    Helper.LogError("EstablishConnection()  " + response.ErrorMessage + "\r\n" + response.StatusDescription);
                }

                return response;

            }
            catch (Exception ex)
            {
                Helper.LogError("EstablishConnection() " + ex);
            }

            return null;




        }

		public class HostData
		{
			public string host { get; set; }
			public string ipAddress { get; set; }
            public string ipType { get; set; }
        }

		public class IPData
		{
			public string ip_address { get; set; }
			public List<string> names { get; set; }
			public string network { get; set; }
		}
	}
}