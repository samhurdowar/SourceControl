using Kendo.Mvc.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using SourceControl.Common;
using SourceControl.Models.Db;

using System;
using System.Collections;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Linq.Dynamic;
using System.Text;
using SourceControl.Services;
using SourceControl.Models;
using System.Collections.Generic;
using System.Data.Entity;

namespace SourceControl.Controllers
{
	public class MenuController : Controller
	{

		[HttpPost]
		public string GetRootMenu()
		{
			using (SourceControlEntities Db = new SourceControlEntities())
			{
				StringBuilder sb = new StringBuilder();

				sb.AppendLine("<ul id=\"ul0\" style=\"width:75%; margin: 0 auto;\">");

				var menus = Db.Menus.Where(w => w.ParentId == 0).OrderBy(o => o.MenuOrder);
				foreach (var menu in menus)
				{
					sb.AppendLine("<li id='li" + menu.MenuId + "'>");
					sb.AppendLine("<table width=\"100%\">");
					sb.AppendLine("<tr class=\"app-menu-root\">");
					sb.AppendLine("<td class=\"index\" id=\"td" + menu.MenuId + "\" style='width:37%;'>");
					if ((bool)menu.HasSubMenu)
					{
						sb.AppendLine("<span id='expander" + menu.MenuId + "' onclick='ExpandRootMenu(" + menu.MenuId + ")' class='k-icon k-i-expand'>&nbsp;</span>");
					}
					else
					{
						sb.AppendLine("<span id='expander" + menu.MenuId + "' class='expander-space'>&nbsp;</span>");
					}
					sb.AppendLine("<span id='data_MenuTitle" + menu.MenuId + "'>" + menu.MenuTitle + "</span>");
					sb.AppendLine("</td>");


					sb.AppendLine("<td style='width:37%;'>");
					sb.AppendLine("<span id='data_PageFile" + menu.MenuId + "'>");
					if (menu.MenuPageTemplateId > 0)
					{
						sb.AppendLine("Template " + menu.MenuPageTemplateId);
					}
					else if (menu.PageFile != null && menu.PageFile.Length > 1)
					{
						sb.AppendLine(menu.PageFile);
					}
					sb.AppendLine("</span>");
					sb.AppendLine("</td>");

					sb.AppendLine("<td align=\"right\" style='width:26%;' nowrap>");
					sb.AppendLine("<a href='javascript:AddRootSubMenu(" + menu.MenuId + ")'>Add Sub Menu</a> | <a href='javascript:EditRootMenu(" + menu.MenuId + ")'>Edit</a> | ");
					if (menu.IsSystem)
					{
						sb.AppendLine("<a href=\"javascript:alert('System menu.  Cannot delete.')\">Delete</a>");

					}
					else
					{
						sb.AppendLine("<a href='javascript:DeleteRootMenu(" + menu.MenuId + ")'>Delete</a>");
					}
					sb.AppendLine("</td>");
					sb.AppendLine("</tr>");
					sb.AppendLine("</table>");
					sb.AppendLine("</li>");

				}

				sb.AppendLine("</ul>");

				return sb.ToString();

			}
		}

		[HttpPost]
		public string GetLevelOneMenu(int menuId)
		{
			string goesTo = "";
			using (SourceControlEntities Db = new SourceControlEntities())
			{
				StringBuilder sb = new StringBuilder();
				var subMenus = Db.Menus.Where(w => w.ParentId == menuId).OrderBy(o => o.MenuOrder);
				sb.AppendLine("<ul id=\"ul" + menuId + "\">");

				// SubMenu I
				foreach (var subMenu in subMenus)
				{
					goesTo = (subMenu.TemplateName != null && subMenu.TemplateName.Length > 0) ? subMenu.TemplateName : (subMenu.PageFile.Length > 0) ? subMenu.PageFile : "";

					sb.AppendLine("<li id='li" + subMenu.MenuId + "'>");

					sb.AppendLine("<table width=\"100%\">");
					sb.AppendLine("<tr class='app-menu-submenu'>");


					sb.AppendLine("<td class=\"index\" id=\"td" + subMenu.MenuId + "\" style='width:40%;'>");
					if ((bool)subMenu.HasSubMenu)
					{
						sb.AppendLine("<span id='expander" + subMenu.MenuId + "' onclick='ExpandLevelOneMenu(" + subMenu.MenuId + ")' class='k-icon k-i-expand'>&nbsp;</span>");
					}
					else
					{
						sb.AppendLine("<span id='expander" + subMenu.MenuId + "' class='expander-space'>&nbsp;</span>");
					}
					sb.AppendLine("<span id='data_MenuTitle" + subMenu.MenuId + "'>" + subMenu.MenuTitle + "</span>");
					sb.AppendLine("</td>");


					sb.AppendLine("<td nowrap width=\"40%\">" + goesTo + "</td>");
					sb.AppendLine("<td nowrap align='right' width=\"20%\" nowrap>");

					if (goesTo.Length < 1)
					{
						sb.AppendLine("<a href='javascript:AddSubMenu(" + subMenu.MenuId + ")'>Add Sub Menu</a> | ");
					}


					sb.AppendLine("<a href='javascript:EditSubMenu(" + subMenu.MenuId + ")'>Edit</a> | ");
					if (subMenu.IsSystem)
					{
						sb.AppendLine("<a href=\"javascript:alert('System menu.  Cannot delete.')\">Delete</a>");
					}
					else
					{
						sb.AppendLine("<a href='javascript:DeleteSubMenu(" + subMenu.ParentId + "," + subMenu.MenuId + ")'>Delete</a>");
					}

					sb.AppendLine("</td>");
					sb.AppendLine("</tr>");
					sb.AppendLine("</table>");

					sb.AppendLine("</li>");

				}

				sb.AppendLine("</ul>");

				return sb.ToString();

			}
		}

		[HttpPost]
		public string GetLevelOneItems(int menuId)
		{
			string goesTo = "";
			using (SourceControlEntities Db = new SourceControlEntities())
			{
				StringBuilder sb = new StringBuilder();

				var menu = Db.Menus.Find(menuId);

				sb.AppendLine("<ul id=\"ul" + menuId + "\">");

				if ((bool)menu.HasSubMenu)
				{
					var subMenus = Db.Menus.Where(w => w.ParentId == menuId).OrderBy(o => o.MenuOrder);

					foreach (var subMenu in subMenus)
					{
						goesTo = (subMenu.TemplateName != null && subMenu.TemplateName.Length > 0) ? subMenu.TemplateName : (subMenu.PageFile.Length > 0) ? subMenu.PageFile : "";

						sb.AppendLine("<li id='li" + subMenu.MenuId + "'>");

						sb.AppendLine("<table width=\"100%\">");
						sb.AppendLine("<tr class='app-menu-submenu'>");

						sb.AppendLine("<td class=\"index\" id=\"td" + menuId + "_" + subMenu.MenuId + "_M\" width=\"40%\">");

						sb.AppendLine(subMenu.MenuTitle);
						sb.AppendLine("</td>");
						sb.AppendLine("<td nowrap width=\"40%\">" + goesTo + "</td>");
						sb.AppendLine("<td nowrap align='right' width=\"20%\" nowrap>");
						sb.AppendLine("<a href='javascript:EditSubMenu(" + subMenu.MenuId + ")'>Edit</a> | ");
						if (subMenu.IsSystem)
						{
							sb.AppendLine("<a href=\"javascript:alert('System menu.  Cannot delete.')\">Delete</a>");
						}
						else
						{
							sb.AppendLine("<a href='javascript:DeleteSubMenu(" + subMenu.ParentId + "," + subMenu.MenuId + ")'>Delete</a>");
						}

						sb.AppendLine("</td>");
						sb.AppendLine("</tr>");
						sb.AppendLine("</table>");

						sb.AppendLine("</li>");

					}
				}

				sb.AppendLine("</ul>");

				return sb.ToString();

			}
		}

		public string GetMenuOptions()
		{
			using (SourceControlEntities Db = new SourceControlEntities())
			{
				List<ValueText> valueTexts = new List<ValueText>();

				ValueText valueText0 = new ValueText();
				valueText0.ValueField = "0:0";
				valueText0.TextField = "";
				valueTexts.Add(valueText0);

				var rootMenus = Db.Menus.Where(w => w.ParentId == 0).OrderBy(o => o.MenuOrder);

				foreach (var rootMenu in rootMenus)
				{
					var subMenus = Db.Menus.Where(w => w.ParentId == rootMenu.MenuId).OrderBy(o => o.MenuOrder);
					foreach (var subMenu in subMenus)
					{

						if ((bool)subMenu.HasSubMenu)
						{
							var lastSubMenus = Db.Menus.Where(w => w.ParentId == subMenu.MenuId).OrderBy(o => o.MenuOrder);
							foreach (var lastSubMenu in lastSubMenus)
							{
								ValueText valueText = new ValueText();
								valueText.ValueField = lastSubMenu.MenuId + ":0";
								valueText.TextField = rootMenu.MenuTitle + " > " + subMenu.MenuTitle + " > " + lastSubMenu.MenuTitle;
								valueTexts.Add(valueText);
							}
						}
						else if (!(bool)subMenu.HasSubMenu)
						{
							ValueText valueText = new ValueText();
							valueText.ValueField = subMenu.MenuId + ":0";
							valueText.TextField = rootMenu.MenuTitle + " > " + subMenu.MenuTitle;

							valueTexts.Add(valueText);
						}
					}
				}

				var json = new JavaScriptSerializer().Serialize(valueTexts);
				return json;

			}
		}

		public string GetParentMenu()
		{
			using (SourceControlEntities Db = new SourceControlEntities())
			{
				List<ValueText> valueTexts = new List<ValueText>();

				ValueText valueText0 = new ValueText();
				valueText0.ValueField = "0";
				valueText0.TextField = "Is Root Menu";
				valueTexts.Add(valueText0);

				var rootMenus = Db.Menus.Where(w => w.ParentId == 0 && w.MenuPageTemplateId == 0 && w.PageFile.Length < 4).OrderBy(o => o.MenuOrder);
				foreach (var rootMenu in rootMenus)
				{
					ValueText valueText1 = new ValueText();
					valueText1.ValueField = rootMenu.MenuId.ToString();
					valueText1.TextField = rootMenu.MenuTitle;
					valueTexts.Add(valueText1);
					var subMenus = Db.Menus.Where(w => w.ParentId == rootMenu.MenuId && w.MenuPageTemplateId == 0 && w.PageFile.Length < 4).OrderBy(o => o.MenuOrder);
					foreach (var subMenu in subMenus)
					{
						ValueText valueText2 = new ValueText();
						valueText2.ValueField = subMenu.MenuId.ToString();
						valueText2.TextField = rootMenu.MenuTitle + " > " + subMenu.MenuTitle;
						valueTexts.Add(valueText2);


						if ((bool)subMenu.HasSubMenu)
						{
							var lastSubMenus = Db.Menus.Where(w => w.ParentId == subMenu.MenuId && w.MenuPageTemplateId == 0 && w.PageFile.Length < 4).OrderBy(o => o.MenuOrder);

							foreach (var lastSubMenu in lastSubMenus)
							{
								ValueText valueText3 = new ValueText();
								valueText3.ValueField = lastSubMenu.MenuId.ToString();
								valueText3.TextField = rootMenu.MenuTitle + " > " + subMenu.MenuTitle + " > " + lastSubMenu.MenuTitle;
								valueTexts.Add(valueText3);
							}
						}

					}
				}

				var json = new JavaScriptSerializer().Serialize(valueTexts);
				return json;

			}
		}

		[HttpPost]
		public string SortRootMenu(int menuId, int newOrder)
		{
			using (SourceControlEntities Db = new SourceControlEntities())
			{
				Db.Database.ExecuteSqlCommand(string.Format("SortRootMenu {0}, {1}", menuId, newOrder));
				return "T";
			}
		}

		[HttpPost]
		public string SortSubMenu(int parentId, int menuId, int newOrder)
		{
			using (SourceControlEntities Db = new SourceControlEntities())
			{
				Db.Database.ExecuteSqlCommand(string.Format("SortSubMenu {0}, {1}, {2}", menuId, parentId, newOrder));
				return "T";
			}
		}

		[HttpPost]
		public string EditRootMenu(string json, int menuId)
		{

			string menuTitle = DataService.GetJsonStringValue(json, "P1_MenuTitle");
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("");

			if (menuId == 0)
			{

				int menuOrder = 0;
				var menuOrder_ = DataService.GetStringValue("SELECT CAST(MAX(MenuOrder) AS varchar(50)) FROM Menu WHERE ParentId = 0");
				if (int.TryParse(menuOrder_, out menuOrder))
				{
					menuOrder++;
				}
				var parsedJson = JObject.Parse(json);
				parsedJson["MenuOrder"] = menuOrder;
				string stringJson = parsedJson.ToString();

				menuId = Helper.ToInt32(DataService.UpdateRecord(1, stringJson, ""));


				sb.AppendLine("<li id='li" + menuId + "'  class='ui-sortable-handle'>");
				sb.AppendLine("<table width=\"100%\">");
				sb.AppendLine("<tr class=\"app-menu-root\">");
				sb.AppendLine("<td class=\"index\" id=\"td" + menuId + "\">");
				sb.AppendLine("<span id='data_MenuTitle" + menuId + "'>" + menuTitle + "</span>");
				sb.AppendLine("</td>");
				sb.AppendLine("<td align=\"right\">");
				sb.AppendLine("<a href='javascript:AddRootSubMenu(" + menuId + ")'>Add Sub Menu</a> | <a href='javascript:EditRootMenu(" + menuId + ")'>Edit</a> | <a href='javascript:DeleteRootMenu(" + menuId + ")'>Delete</a>");
				sb.AppendLine("</td>");
				sb.AppendLine("</tr>");
				sb.AppendLine("</table>");
				sb.AppendLine("</li>");
			}
			else
			{
				Helper.ToInt32(DataService.UpdateRecord(1, json, ""));
			}

			return sb.ToString();
		}

		[HttpPost]
		public string DeleteRootMenu(int menuId)
		{
			string ids = DataService.GetStringValue("SELECT dbo.GetMenuChildrenIds(" + menuId + ")");
			using (SourceControlEntities Db = new SourceControlEntities())
			{
				Db.Database.ExecuteSqlCommand("DELETE FROM Menu WHERE MenuId IN (" + ids + ");");
				SortRootMenu(0, 0);
			}
			return "T";
		}

		[HttpPost]
		public string DeleteSubMenu(int parentId, int menuId)
		{
			string ids = DataService.GetStringValue("SELECT dbo.GetMenuChildrenIds(" + menuId + ")");
			using (SourceControlEntities Db = new SourceControlEntities())
			{
				if (ids != null)
				{
					Db.Database.ExecuteSqlCommand("DELETE FROM Menu WHERE MenuId IN (" + ids + ");DELETE FROM Menu WHERE MenuId IN (" + menuId + ");");
				}
				else
				{
					Db.Database.ExecuteSqlCommand("DELETE FROM Menu WHERE MenuId IN (" + menuId + ");");
				}
				SortSubMenu(parentId, 0, 0);
			}
			return "T";
		}

		[HttpPost]
		public string AddRootSubMenu(int parentId, string json)
		{
			StringBuilder sb = new StringBuilder();

			// set MenuOrder
			int menuOrder = 0;
			var menuOrder_ = DataService.GetStringValue("SELECT CAST(MAX(MenuOrder) AS varchar(50)) FROM Menu WHERE ParentId = " + parentId);
			if (int.TryParse(menuOrder_, out menuOrder))
			{
				menuOrder++;
			}

			var parsedJson = JObject.Parse(json);
			parsedJson["MenuOrder"] = menuOrder;
			string stringJson = parsedJson.ToString();

			var menuId = 0;
			menuId = Helper.ToInt32(DataService.UpdateRecord(1, stringJson, ""));

			sb.AppendLine("<li id='li" + menuId + "' class='ui-sortable-handle'>");
			sb.AppendLine(GetMenuRow(menuId));
			sb.AppendLine("</li>");

			return sb.ToString();
		}

		[HttpPost]
		public string EditSubMenu(int menuId, string json)
		{
			if (menuId == 0)
			{
				string parentId = DataService.GetJsonStringValue(json, "P1_ParentId");
				int menuOrder = 0;
				var menuOrder_ = DataService.GetStringValue("SELECT CAST(MAX(MenuOrder) AS varchar(50)) FROM Menu WHERE ParentId = " + parentId);
				if (int.TryParse(menuOrder_, out menuOrder))
				{
					menuOrder++;
				}
				var parsedJson = JObject.Parse(json);
				parsedJson["MenuOrder"] = menuOrder;
				string stringJson = parsedJson.ToString();


				menuId = Helper.ToInt32(DataService.UpdateRecord(1, stringJson, ""));

				return "<li id='li" + menuId + "' class='ui-sortable-handle'>" + GetMenuRow(menuId) + "</li>";

			}
			else
			{
				DataService.UpdateRecord(1, json, "");
				return GetMenuRow(menuId);
			}

		}

		[HttpPost]
		public string RefreshSubMenu(int menuId)
		{
			return GetMenuRow(menuId);
		}

		private string GetMenuRow(int menuId)
		{

			StringBuilder sb = new StringBuilder();
			using (SourceControlEntities Db = new SourceControlEntities())
			{
				var menu = Db.Menus.Find(menuId);

				if (menu != null)
				{
					var goesTo = (menu.TemplateName != null && menu.TemplateName.Length > 0) ? menu.TemplateName : (menu.PageFile.Length > 0) ? menu.PageFile : "";


					sb.AppendLine("<table width=\"100%\">");
					sb.AppendLine("<tr class='app-menu-submenu'>");
					sb.AppendLine("<td class=\"index\" id=\"td" + menu.ParentId + "_" + menu.MenuId + "_M\" width=\"40%\">");

					sb.AppendLine("<span id='expander" + menu.MenuId + "'>&nbsp;</span>");
					sb.AppendLine(menu.MenuTitle);
					sb.AppendLine("</td>");


					sb.AppendLine("<td nowrap width=\"40%\">" + goesTo + "</td>");
					sb.AppendLine("<td nowrap align='right' width=\"20%\">");

					if (!(bool)menu.HasSubMenu && goesTo.Length < 1 && !(bool)menu.IsLastSubmenu)
					{
						sb.AppendLine("<a href='javascript:AddSubMenu(" + menu.MenuId + ")'>Add Sub Menu</a> | ");
					}

					sb.AppendLine("<a href='javascript:EditSubMenu(" + menu.MenuId + ")'>Edit</a> | ");
					sb.AppendLine("<a href='javascript:DeleteSubMenu(" + menu.ParentId + "," + menu.MenuId + ")'>Delete</a>");

					sb.AppendLine("</td>");
					sb.AppendLine("</tr>");
					sb.AppendLine("</table>");

				}

			}

			return sb.ToString();

		}

    }
}