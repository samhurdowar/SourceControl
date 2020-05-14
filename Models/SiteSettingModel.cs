using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SourceControl.Models
{
	public class SiteSettingModel
	{
		public string WelcomeMessage { get; set; }
		public string BannerLogo { get; set; }
		public string HomePageLogo { get; set; }
		public string CompanyName { get; set; }
	}
}