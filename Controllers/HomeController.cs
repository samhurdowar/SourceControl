using OpenHtmlToPdf;
using System.IO;
using System;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;
using System.Web.Helpers;
using System.Web.Mvc;
using SourceControl.Common;
using SourceControl.Models;
using SourceControl.Models.Db;
using SourceControl.Services;

namespace SourceControl.Controllers
{
	[OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
	public class HomeController : Controller
	{

        public ActionResult Index_()
        {
            if (Session["sec.CurrentUser"] == null)
            {
                return RedirectToAction("Login", "Home");
            }
            return View();
        }


        public ActionResult Index()
		{
            //string html = System.IO.File.ReadAllText(@"D:\Kraken\SourceControl\App_Data\SampleEIDMHtml.txt");
            //var pdfBytes = Pdf.From(html).WithGlobalSetting("orientation", "Landscape").Content();
            //System.IO.File.WriteAllBytes("C:\\Temp\\Test2.pdf", pdfBytes);

            string username = User.Identity.Name.ToString();
			//string username = "DESKTOP-5409NQJ\\Monkeke_Sam";
			//Helper.LogError("User tried to log in " + username + " at " + DateTime.Now);

			using (SourceControlEntities Db = new SourceControlEntities())
			{
				var user = Db.AppUsers.Where(a => a.AdName == username).FirstOrDefault();

				if (user == null)
				{

					//username = "DESKTOP-5409NQJ\\Monkeke_Sam";

					///New user.  Add to AppUser, set to inactive
					AppUser appUser = new AppUser { AdName = username, FirstName = "", LastName = "", Email = "", PrimaryPhone = "", IsActive = true, ProfileIsComplete = false, LogonCount = 0, Password = "", AddDate = DateTime.Now };

					try
					{
						using (var context = new PrincipalContext(ContextType.Domain))
						{
							var principal = UserPrincipal.FindByIdentity(context, User.Identity.Name);

							appUser.FirstName = Helper.ToSafeString(principal.GivenName);
							appUser.LastName = Helper.ToSafeString(principal.Surname);
							appUser.Email = Helper.ToSafeString(principal.EmailAddress);
							appUser.PrimaryPhone = Helper.ToSafeString(principal.VoiceTelephoneNumber);
						}
					}
					catch (Exception)
					{
					}

					Db.AppUsers.Add(appUser);
					Db.SaveChanges();

					EmailService.SendEmail(SessionService.NetworkToolboxEmailAddress(), SessionService.NetworkToolboxEmailAddress(), "", "New user requesting access on Network Toolbox.", appUser.FirstName + " " + appUser.LastName + "<br>" + appUser.Email + "<br>" + username);
					user = Db.AppUsers.Where(a => a.AdName == username).FirstOrDefault();
				}

				if (user == null)
				{
					return RedirectToAction("Login", "Home");
				}
				else
				{
					Session["sec.CurrentUser"] = user;
				}
			}

			var columnDefs = SessionService.ColumnDefs(2126);
			var pageTemplates = SessionService.PageTemplates(1);

			return View();
		}

        public ActionResult RoutePage(int pageTemplateId = 0, string pageFile = "", string recordId = "", string refKeyColumnName = "", string refKeyValue = "", string cancelMode = "", int parentPageTemplateId = 0, string layoutType = "")
        {
            try
            {
                // page variables
                ViewData["ParentPageTemplateId"] = parentPageTemplateId;
                ViewData["RecordId"] = recordId;
                ViewData["LayoutType"] = layoutType;
                ViewData["CancelMode"] = cancelMode;
                ViewData["RefKeyColumnName"] = refKeyColumnName;
                ViewData["RefKeyValue"] = refKeyValue;

                // manual routing  !!!
                if (pageTemplateId == 645126) // Contracts page, hardcoded 
                {
                    return PartialView("~/Views/App/NetworkInformationContractsEdit.cshtml");
                }

                // set PageTemplate
                PageTemplate pageTemplate = new PageTemplate();
                PageTemplate pageTemplate2 = new PageTemplate { PageTemplateId = 0, PrimaryKey = "dummy", PrimaryKey2 = 0 };
                if (pageTemplateId > 0)
                {
                    pageTemplate = SessionService.PageTemplate(pageTemplateId);
                    if (pageTemplate.PageTemplateId2 > 0)
                    {
                        pageTemplate2 = SessionService.PageTemplate(pageTemplate.PageTemplateId2);
                    }
                }
                ViewData["PageTemplate"] = pageTemplate;
                ViewData["PageTemplate2"] = pageTemplate2;

                // route to page
                if (pageFile.Length > 3)
                {

                    // reroute to custom edit form
                    if (pageFile == "_FormTemplate")
                    {
                        if (pageTemplate.CustomEditForm.Length > 3)
                        {
                            return PartialView(pageTemplate.CustomEditForm);
                        }
                    }

                    return PartialView(pageFile);
                }
                else if (pageTemplateId > 0)
                {
                    if (pageTemplate.PageType == "formonly")
                    {
                        ViewData["RecordId"] = SessionService.UserId;
                        return PartialView("_FormTemplate");
                    }
                    else if (pageTemplate.PageType == "inline")
                    {
                        return PartialView("_InlineTemplate");
                    }
                    else
                    {
                        return PartialView("_GridTemplate");
                    }
                }

                return PartialView("_NotExist");
            }
            catch (Exception ex)
            {
                Helper.LogError(ex.Message + "\r\n" + ex.StackTrace);

            }

            return null;

        }


        public ActionResult ReportViewer(int reportPageTemplateId)
		{
			var pageTemplate = SessionService.PageTemplate(reportPageTemplateId);
			ViewBag.UserId = SessionService.UserId;
			ViewBag.PageTemplateId = pageTemplate.PageTemplateId;

			return View();
		}

		public ActionResult HomePage()
		{
			return View();
		}

		public ActionResult GetHelpDoc(int helpDocId)
		{

			using (SourceControlEntities Db = new SourceControlEntities())
			{
				var help = Db.Helps.Find(helpDocId);
				ViewBag.HelpContent = help.HelpContent;
				return PartialView("_HelpDoc");
			}


			//if (HelpDocId > 0)
			//{
			//	StringBuilder sb = new StringBuilder();
			//	sb.AppendLine("dataBound: function (e) {");
			//	sb.AppendLine("var treeView = $(\"#treeHelpDoc\").data(\"kendoTreeView\");");
			//	sb.AppendLine("treeView.expandTo(" + HelpDocId + ");");
			//	sb.AppendLine("},");

			//	ViewBag.ExpandTo = sb.ToString();
			//}
			//else
			//{
			//	ViewBag.ExpandTo = "";
			//}
		}

		public ActionResult LookupWindow(int pageTemplateId, int columnDefId)
		{
			using (SourceControlEntities Db = new SourceControlEntities())
			{
                var pageTemplate = SessionService.PageTemplate(pageTemplateId);

				var gridColumns = "";
				var selectColumn = "";
				var columnDef = SessionService.ColumnDef(columnDefId);

				// add primary key
				string selectColumns = columnDef.ValueField;
				string displayColumns = columnDef.ValueField;


				// loop thru each column and see if there are lookups.  use display name
				if (columnDef.TextField.Contains(","))
				{
					string[] words = columnDef.TextField.Split(new char[] { ',' });
					foreach (var word in words)
					{
						var colDef = SessionService.ColumnDef(columnDef.PageTemplateId, word);
						if (colDef != null)
						{
							if (colDef.LookupTable.Length > 0 && colDef.ValueField.Length > 0 && (colDef.ElementType == "DropdownSimple"))
							{
								var lookupDef = SessionService.ColumnDef(columnDef.PageTemplateId, columnDef.LookupTable);
								selectColumns += "," + colDef.ValueField + "|(SELECT " + colDef.TextField + " FROM " + colDef.LookupTable + " WHERE " + colDef.ValueField + " = " + columnDef.LookupTable + "." + colDef.ValueField + ")[BYPASS_SELECT_FIELD]";
								displayColumns += "," + colDef.DisplayName;
							}
							else
							{
								selectColumns += "," + word;
								displayColumns += "," + colDef.DisplayName;
							}
						}
						else
						{
							selectColumns += "," + word;
							displayColumns += "," + word;
						}

					}
				}
				else
				{
					selectColumns += "," + columnDef.TextField;
					displayColumns += "," + columnDef.DisplayName;
				}


				// get the field after the key to display
				if (selectColumns.Contains(","))
				{
					string[] words = selectColumns.Split(new char[] { ',' });
					selectColumn = words[1];
					var i = 0;
					foreach (var word in words)
					{
						i++;
						if (i == 1) continue;
						if (word.Contains("[BYPASS_SELECT_FIELD]"))
						{
							string[] splits = word.Split(new char[] { '|' });
							gridColumns += "{ field: \"" + splits[0] + "\", title: \"" + splits[0] + "\" },";
							continue;
						}
						gridColumns += "{ field: \"" + word + "\", title: \"" + word + "\" },";
					}
				}
				else
				{
					selectColumn = selectColumns;
				}

				ViewBag.gridColumns = gridColumns;
				ViewBag.pageTemplateId = pageTemplateId;
				ViewBag.columnDefId = columnDefId;
				ViewBag.targetColumn = pageTemplate.TableName + "_" + columnDef.ColumnName;
				ViewBag.tableNameOveride = columnDef.LookupTable;
				ViewBag.selectColumns = selectColumns;
				ViewBag.sortOveride = columnDef.OrderField;
				ViewBag.selectColumn = selectColumn;

				return View();

			}
		}


		//[AllowAnonymous]
		//public ActionResult Logout()
		//{
		//	Session.Abandon();
		//	return RedirectToAction("Login", "Home");
		//}


		//public ActionResult Login()
		//{
		//	return View();
		//}


		//[HttpPost]
		//[AllowAnonymous]
		//[ValidateAntiForgeryToken]
		//public ActionResult Login(string usr, string pwd)
		//{
		//	if (ModelState.IsValid)
		//	{
		//		using (SourceControlEntities Db = new SourceControlEntities())
		//		{
		//			var user = Db.AppUsers.Where(a => a.AdName == usr).FirstOrDefault();

		//			if (user != null && Crypto.VerifyHashedPassword(user.Password, pwd)  )
		//			{
		//				Session["sec.CurrentUser"] = user;

		//				return RedirectToAction("Index", "Home");
		//			}
		//			else
		//			{
		//				ModelState.AddModelError("", "Invalid username or password.");
		//			}
		//		}
		//	}
		//	return View();
		//}



	}


}