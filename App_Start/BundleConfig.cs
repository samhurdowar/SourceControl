using System.Web;
using System.Web.Optimization;

namespace SourceControl
{
	public class BundleConfig
	{

		// For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
		public static void RegisterBundles(BundleCollection bundles)
		{
			BundleTable.EnableOptimizations = false;
			bundles.Add(new ScriptBundle("~/bundles/angular").Include(
			 "~/Scripts/angular.min.js",
			 "~/Scripts/Angular/angular-ui-bootstrap-tpls.js",
			  //"~/Scripts/Angular/angular-route.js",
			  //"~/Scripts/Angular/angular-ui-tree.js",
			  "~/Scripts/Angular/angular-animate.js"
			  ));
		}
	}
}

