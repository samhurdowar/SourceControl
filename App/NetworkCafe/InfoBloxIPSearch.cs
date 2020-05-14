using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SourceControl.App.NetworkCafe
{
	public class InfoBloxIPSearch
	{
		public class url
		{
			public string server = "10.10.248.40";
			public string resource = "wapi/v1.2/ipv4address?status=USED&ip_address=";
		}
		public class RootIPSearch
		{
			public string _ref { get; set; }
			public string ip_address { get; set; }
			public bool is_conflict { get; set; }
			public string mac_address { get; set; }
			public string[] names { get; set; }
			public string network { get; set; }
			public string network_view { get; set; }
			public string[] objects { get; set; }
			public string status { get; set; }
			public string[] types { get; set; }
			public string[] usage { get; set; }
		}

	}
}