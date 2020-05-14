using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SourceControl.App.NetworkCafe
{
	public class InfoBloxDNSHostSearch
	{
		public class url
		{
			public string server = "10.10.248.40";
			public string resource = "wapi/v1.2/record:host?name~=";
		}
		public class Ipv4addrs
		{
			public string _ref { get; set; }
			public bool configure_for_dhcp { get; set; }
			public string host { get; set; }
			public string ipv4addr { get; set; }
			public string mac { get; set; }
		}

		public class Ipv6addrs
		{
			public string _ref { get; set; }
			public bool configure_for_dhcp { get; set; }
			public string host { get; set; }
			public string ipv6addr { get; set; }
		}


		public class RootIpv4Search
		{
			public string _ref { get; set; }
			public Ipv4addrs[] ipv4addrs { get; set; }
			public string name { get; set; }
			public string view { get; set; }
			public Ipv6addrs[] ipv6addrs { get; set; }
		}



        public class DnsResult
        {
            public string name { get; set; }
            public address4[] ipv4addrs { get; set; }
            public address6[] ipv6addrs { get; set; }
            public string _ref { get; set; }
            public string view { get; set; }
        }



        public class address4
        {
            public string host { get; set; }
            public string ipv4addr { get; set; }
        }

        public class address6
        {
            public string host { get; set; }
            public string ipv6addr { get; set; }
        }
    }
}