using SourceControl.Models.Db;
using SourceControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace SourceControl.Services
{
	public static class MenuService
	{
		public static string GetUserMenu()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("");

			try
			{
				using (SourceControlEntities Db = new SourceControlEntities())
				{
					var user = SessionService.CurrentUser;

					var accessMenuIds = ":" + user.RoleMenuIds.Replace(" ","").Replace(",", ":") + ":";
					
					var isSiteAdmin = (user.RoleNames.Contains("SiteAdministrator") || user.RoleNames.Contains("Site Administrator")) ? true : false;

					var menus = Db.Menus.Where(w => w.ParentId == 0).OrderBy(o => o.MenuOrder);
					foreach (var menu in menus)
					{
						if (accessMenuIds.Contains(":" + menu.MenuId + ":") || isSiteAdmin)
						{
							sb.AppendLine("<li id='targetMenu" + menu.MenuId + "' onclick=\"GotoParentPage(" + menu.MenuId + "," + menu.MenuPageTemplateId + ",'" + menu.PageFile + "','" + menu.MenuTitle + "')\" class='li-first-class'>");
							sb.AppendLine(menu.MenuTitle);

							// SubMenu I
							if ((bool)menu.HasSubMenu)
							{
								sb.AppendLine("<ul>");

								var subMenus = Db.Database.SqlQuery<Menu>("SELECT * FROM Menu WHERE ParentId = " + menu.MenuId + " ORDER BY MenuOrder").ToList();
								foreach (var subMenu in subMenus)
								{
									if (accessMenuIds.Contains(":" + subMenu.MenuId + ":") || isSiteAdmin)
									{
										sb.AppendLine("<li onclick=\"GotoPage(" + subMenu.MenuId + "," + subMenu.MenuPageTemplateId + ",'" + subMenu.PageFile + "','" + subMenu.MenuTitle + "')\">");
										sb.AppendLine(subMenu.MenuTitle);


										// SubMenu II 
										if ((bool)subMenu.HasSubMenu)
										{
											sb.AppendLine("<ul>");

											var secondMenus = Db.Database.SqlQuery<Menu>("SELECT * FROM Menu WHERE ParentId = " + subMenu.MenuId + " ORDER BY MenuOrder").ToList();
											foreach (var secondMenu in secondMenus)
											{
												if (accessMenuIds.Contains(":" + secondMenu.MenuId + ":") || isSiteAdmin)
												{
													sb.AppendLine("<li onclick=\"GotoPage(" + secondMenu.MenuId + "," + secondMenu.MenuPageTemplateId + ",'" + secondMenu.PageFile + "','" + secondMenu.MenuTitle + "')\">");
													sb.AppendLine(secondMenu.MenuTitle);
													sb.AppendLine("</li>");

													// SubMenu III 
													if ((bool)secondMenu.HasSubMenu)
													{
														sb.AppendLine("<ul>");

														var lastMenus = Db.Database.SqlQuery<Menu>("SELECT * FROM Menu WHERE ParentId = " + secondMenu.MenuId + " ORDER BY MenuOrder").ToList();
														foreach (var lastMenu in lastMenus)
														{
															if (accessMenuIds.Contains(":" + lastMenu.MenuId + ":"))
															{
																sb.AppendLine("<li onclick=\"GotoPage(" + lastMenu.MenuId + "," + lastMenu.MenuPageTemplateId + ",'" + lastMenu.PageFile + "','" + lastMenu.MenuTitle + "')\">");
																sb.AppendLine(lastMenu.MenuTitle);
																sb.AppendLine("</li>");
															}

														}
														sb.AppendLine("</ul>");
													}


												}

											}
											sb.AppendLine("</ul>");
										}

										sb.AppendLine("</li>");
									}

								}
								sb.AppendLine("</ul>");
							}

							sb.AppendLine("</li>");
						}

					}
				}
			}
			catch (Exception)
			{
				return "";
			}

			return sb.ToString();
		}

		public static string GetAccessMenus()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("");

			try
			{
				var levelCount = 0;
				var childClasses = "";
				using (SourceControlEntities Db = new SourceControlEntities())
				{
					sb.AppendLine("<table class='table-object'>");
					sb.AppendLine("<tr class='table-header'><td>Menu</td><td>Add</td><td>Edit</td><td>Delete</td><td>Download</td><td>Print</td></tr>");
					var menus = Db.Menus.Where(w => w.ParentId == 0).OrderBy(o => o.MenuOrder);
					foreach (var menu in menus)
					{
						sb.AppendLine("<tr class='table-parent-record'>");
						if (menu.PageFile.Length > 3 || menu.MenuPageTemplateId > 0)
						{
							sb.AppendLine("<td><input type='checkbox' id='MenuId" + menu.MenuId + "' class='menu-access' value='" + menu.MenuId + "' parentId='0'> " + menu.MenuTitle + "</td>");
							sb.AppendLine("<td align='center'><input type='checkbox' id='MenuAdd" + menu.MenuId + "' class='menu-add' value='" + menu.MenuId + "' parentId='0'></td>");
							sb.AppendLine("<td align='center'><input type='checkbox' id='MenuEdit" + menu.MenuId + "' class='menu-edit' value='" + menu.MenuId + "' parentId='0'></td>");
							sb.AppendLine("<td align='center'><input type='checkbox' id='MenuDelete" + menu.MenuId + "' class='menu-delete' value='" + menu.MenuId + "' parentId='0'></td>");
							sb.AppendLine("<td align='center'><input type='checkbox' id='MenuDownload" + menu.MenuId + "' class='menu-download' value='" + menu.MenuId + "' parentId='0'></td>");
							sb.AppendLine("<td align='center'><input type='checkbox' id='MenuPrint" + menu.MenuId + "' class='menu-print' value='" + menu.MenuId + "' parentId='0'></td>");
						}
						else
						{
							sb.AppendLine("<td colspan='6'><input type='checkbox' id='MenuId" + menu.MenuId + "' class='menu-expand' value='" + menu.MenuId + "' parentId='0'> " + menu.MenuTitle + "</td>");
						}
						sb.AppendLine("</tr>");

						var subMenus = Db.Menus.Where(w => w.ParentId == menu.MenuId).OrderBy(o => o.MenuOrder).ToList();
						if (subMenus.Count > 0)
						{
							levelCount = 0;
							childClasses = " childOf" + menu.MenuId;
							GetAccessSubMenus(Db, menu.MenuId, subMenus, ref sb, ref levelCount, ref childClasses);
						}


					}
					sb.AppendLine("</table>");
				}
			}
			catch (Exception)
			{
				return "";
			}

			return sb.ToString();
		}

		public static void GetAccessSubMenus(SourceControlEntities Db, int parentId, List<Menu> subMenus, ref StringBuilder sb, ref int levelCount, ref string childClasses)
		{

			try
			{
				levelCount++;
				string spacers = "";
				for (int i = 0; i < levelCount; i++)
				{
					spacers += "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";
				}

				foreach (var menu in subMenus)
				{
					sb.AppendLine("<tr class='table-record'>");


					if (menu.PageFile.Length > 3 || menu.MenuPageTemplateId > 0)
					{
						sb.AppendLine("<td>" + spacers + "<input type='checkbox' id='MenuId" + menu.MenuId + "' class='menu-access " + childClasses + "' value='" + menu.MenuId + "' parentId='" + parentId + "'> " + menu.MenuTitle + "</td>");
						sb.AppendLine("<td align='center'><input type='checkbox' id='MenuAdd" + menu.MenuId + "' class='menu-add' value='" + menu.MenuId + "' parentId='" + parentId + "'></td>");
						sb.AppendLine("<td align='center'><input type='checkbox' id='MenuEdit" + menu.MenuId + "' class='menu-edit' value='" + menu.MenuId + "' parentId='" + parentId + "'></td>");
						sb.AppendLine("<td align='center'><input type='checkbox' id='MenuDelete" + menu.MenuId + "' class='menu-delete' value='" + menu.MenuId + "' parentId='" + parentId + "'></td>");
						sb.AppendLine("<td align='center'><input type='checkbox' id='MenuDownload" + menu.MenuId + "' class='menu-download' value='" + menu.MenuId + "' parentId='" + parentId + "'></td>");
						sb.AppendLine("<td align='center'><input type='checkbox' id='MenuPrint" + menu.MenuId + "' class='menu-print' value='" + menu.MenuId + "' parentId='" + parentId + "'></td>");
					}
					else
					{
						sb.AppendLine("<td colspan='6'>" + spacers + "<input type='checkbox' id='MenuId" + menu.MenuId + "' class='menu-expand " + childClasses + "' value='" + menu.MenuId + "' parentId='" + parentId + "'> " + menu.MenuTitle + "</td>");
					}


					sb.AppendLine("</tr>");

					var subMenus_ = Db.Menus.Where(w => w.ParentId == menu.MenuId).OrderBy(o => o.MenuOrder).ToList();
					if (subMenus_.Count > 0)
					{
						childClasses += " childOf" + menu.MenuId;
						GetAccessSubMenus(Db, menu.MenuId, subMenus_, ref sb, ref levelCount, ref childClasses);
					}
				}

				
			}
			catch (Exception)
			{

			}

		}
	}
}



