using SourceControl.Models;
using SourceControl.Models.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace SourceControl.Services
{
	public static class AccessService
	{

		public static string GetMenuTreeView(int userId)
		{
			StringBuilder sb = new StringBuilder();
			var json = "";
			using (SourceControlEntities Db = new SourceControlEntities())
			{
				json += "[ { \"id\": -1, \"text\": \"Available Pages\", \"expanded\": \"true\", \"spriteCssClass\": \"rootfolder\", \"items\": [";

				var rootMenus = Db.Menus.Where(w => w.ParentId == 0).OrderBy(o => o.MenuOrder);
				foreach (var rootMenu in rootMenus)
				{
					json += "{ \"id\": " + rootMenu.MenuId + ", \"text\": \"" + rootMenu.MenuTitle + "\", \"expanded\": \"true\", \"spriteCssClass\": \"image\", \"items\": [";


					//var isRequest = Db.Database.SqlQuery<string>("SELECT CASE IsRequest WHEN 0 THEN '0' ELSE '1' END AS IsRequest FROM UserMenuAccess WHERE UserId = " + userId + " ORDER BY IsRequest").FirstOrDefault();
					//if (isRequest == null)
					//{
					//	isRequest = "1";
					//}


					var subMenus = Db.Database.SqlQuery<MenuModel>("SELECT a.*, b.UserId FROM Menu a LEFT JOIN UserMenuAccess b ON a.MenuId = b.MenuId AND b.UserId = " + userId + " WHERE a.ParentId = " + rootMenu.MenuId + " ORDER BY a.MenuOrder");
					foreach (var subMenu in subMenus)
					{
						if (subMenu.UserId != null) 
						{
							json += "{ \"id\": " + subMenu.MenuId + ", \"text\": \"" + subMenu.MenuTitle + "\", \"spriteCssClass\": \"html\", \"checked\": \"true\" },";

						} else
						{
							json += "{ \"id\": " + subMenu.MenuId + ", \"text\": \"" + subMenu.MenuTitle + "\", \"spriteCssClass\": \"html\"  },";
						}
						
					}

					if (json.Substring(json.Length - 1, 1) == ",") json = json.Substring(0, json.Length - 1);

					json += "] },";
				}

				if (json.Substring(json.Length - 1, 1) == ",") json = json.Substring(0, json.Length - 1);

				json += "] } ]";

			}
			return json;
		}

	}
}