using SourceControl.Common;
using SourceControl.Models.Db;
using SourceControl.Models;
using SourceControl.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace SourceControl.Controllers
{
	public class HelpController : Controller
	{

		[HttpPost]
		public string GetHelpLevel0(string scope, int selectHelpId)
		{
			using (SourceControlEntities Db = new SourceControlEntities())
			{
				bool expandAll = false;
				string selectHelpIds = "";
				if (scope == "ALL")
				{
					expandAll = true;
				} else if (selectHelpId > 0)
				{
					selectHelpIds = Db.Database.SqlQuery<string>("dbo.GetHelpParentIds " + selectHelpId).FirstOrDefault();
					selectHelpIds = ":" + selectHelpIds.Replace(",",":").Replace(" ", "") + ":";
				}

				StringBuilder sb = new StringBuilder();
				sb.AppendLine("<ul id='ulHelp0'  class='sortULHelp'>");

				var helps = Db.Helps.Where(w => w.ParentId == 0).OrderBy(o => o.HelpOrder).Select(s => new { s.HelpId, s.HelpTitle, s.HasChildren, s.IsDoc });
				foreach (var help in helps)
				{
					sb.AppendLine("<li id='liHelp" + help.HelpId + "'>");

					if ((bool)help.HasChildren)
					{
						sb.AppendLine("<span id='expanderHelp" + help.HelpId + "' onclick='ExpandHelp(" + help.HelpId + ")' class='help-expander k-icon " + ((expandAll) ? " k-i-collapse" : " k-i-expand") + "'>&nbsp;</span>");
					} else
					{
						sb.AppendLine("<span id='expanderHelp" + help.HelpId + "' class='help-spacer'>&nbsp;</span>");
					}

					if (help.IsDoc)
					{
						sb.AppendLine("<span class=\'k-sprite doc\'></span>");
					}
					else
					{
						sb.AppendLine("<span class=\'k-sprite folder\'></span>");
					}

					sb.AppendLine("<span class='help-item li-state-default' id='Help0_" + help.HelpId + "' IsDoc='" + help.IsDoc + "' onclick='SelectHelp(this)'>");
					sb.AppendLine(help.HelpTitle);
					sb.AppendLine("</span>");

					if ((expandAll || selectHelpIds.Contains(":" + help.HelpId + ":")) && (bool)help.HasChildren)
					{
						sb.AppendLine(GetHelpChild(help.HelpId, selectHelpIds, true));
					}

					sb.AppendLine("</li>");
				}

				sb.AppendLine("</ul>");

				return sb.ToString();
			}

		}

		public string GetHelpChild(int parentId, string selectHelpIds = "", bool expandAll = false)
		{
			using (SourceControlEntities Db = new SourceControlEntities())
			{
				StringBuilder sb = new StringBuilder();
				sb.AppendLine("<ul id='ulHelp" + parentId + "' class='sortULHelp'>");

				var helps = Db.Helps.Where(w => w.ParentId == parentId).OrderBy(o => o.HelpOrder).Select(s => new { s.HelpId, s.HelpTitle, s.HasChildren, s.IsDoc, s.ParentId });

				foreach (var help in helps)
				{
					sb.AppendLine("<li id='liHelp" + help.HelpId + "'>");

					if ((bool)help.HasChildren)
					{
						sb.AppendLine("<span id='expanderHelp" + help.HelpId + "' onclick='ExpandHelp(" + help.HelpId + ")' class='help-expander k-icon " + ((expandAll) ? " k-i-collapse" : " k-i-expand") + "'>&nbsp;</span>");
					}
					else
					{
						sb.AppendLine("<span id='expanderHelp" + help.HelpId + "' class='help-spacer'>&nbsp;</span>");
					}

					if (help.IsDoc)
					{
						sb.AppendLine("<span class=\'k-sprite doc\'></span>");
					}
					else
					{
						sb.AppendLine("<span class=\'k-sprite folder\'></span>");
					}

					sb.AppendLine("<span class='help-item li-state-default' id='Help" + help.ParentId + "_" + help.HelpId + "' IsDoc='" + help.IsDoc + "' onclick='SelectHelp(this)'>");
					sb.AppendLine(help.HelpTitle);
					sb.AppendLine("</span>");


					if ((expandAll || selectHelpIds.Contains(":" + help.HelpId + ":")) && (bool)help.HasChildren)
					{
						sb.AppendLine(GetHelpChild(help.HelpId, selectHelpIds, true));
					}

					sb.AppendLine("</li>");
				}

				sb.AppendLine("</ul>");

				return sb.ToString();
			}

		}


		[HttpPost]
		public string SortHelp(int parentId, int helpId, int newOrder)
		{
			using (SourceControlEntities Db = new SourceControlEntities())
			{
				Db.Database.ExecuteSqlCommand(string.Format("SortHelp {0}, {1}, {2}", parentId, helpId, newOrder));
				return "T";
			}
		}


		[HttpPost]
		public string GetHelp(int helpId)
		{
			using (SourceControlEntities Db = new SourceControlEntities())
			{
				var helpContent = DataService.GetStringValue("SELECT HelpContent FROM Help WHERE HelpId = " + helpId);
				if (helpContent !=null)
				{
					return helpContent;
				}
				return "";
			}
		}

		[HttpPost]
		public string AddHelp(int parentId, string helpTitle, string helpContent, int isDoc)
		{
			
			using (SourceControlEntities Db = new SourceControlEntities())
			{
				var helpOrder = Db.Database.SqlQuery<int>("SELECT ISNULL(MAX(HelpOrder), -1) + 1 FROM Help WHERE ParentId = " + parentId).FirstOrDefault();

				var exe = string.Format("INSERT INTO Help(ParentId, HelpTitle, HelpContent, HelpOrder, IsDoc) VALUES({0},'{1}','{2}',{3},{4}); SELECT CAST(@@IDENTITY AS int);", parentId, Helper.ToDbString(helpTitle), Helper.ToDbString(helpContent), helpOrder, isDoc);

				var helpId = Db.Database.SqlQuery<int>(exe).FirstOrDefault();

				return helpId.ToString();
			}
		}



		[HttpPost]
		public string DeleteHelp(int parentId, int helpId)
		{
			using (SourceControlEntities Db = new SourceControlEntities())
			{

				var ids = Db.Database.SqlQuery<string>("dbo.GetHelpChildrenIds " + helpId).FirstOrDefault();
				if (ids != null)
				{
					Db.Database.ExecuteSqlCommand("DELETE FROM Help WHERE HelpId IN (" + ids + ")");
				} else
				{
					Db.Database.ExecuteSqlCommand("DELETE FROM Help WHERE HelpId IN (" + helpId + ")");
				}

				// reorder
				Db.Database.ExecuteSqlCommand(string.Format("SortHelp {0}, {1}, {2}", parentId, 0, 0));


				return "";
			}
		}

		//[HttpPost]
		//[ValidateInput(false)]
		//public string EditHelp(int helpId, string helpTitle, string helpContent)
		//{
		//	var exe = string.Format("UPDATE Help SET HelpTitle = '{0}', HelpContent = '{1}' WHERE HelpId = {2}", Helper.ToDbString(helpTitle), Helper.ToDbString(helpContent), helpId);
		//	DataService.Execute(exe);

		//	return "T";
		//}



		public string GetHelpOptions()
		{
			using (SourceControlEntities Db = new SourceControlEntities())
			{

				List<ValueText> valueTexts = new List<ValueText>();
				ValueText valueText0 = new ValueText();
				valueText0.ValueField = "0";
				valueText0.TextField = "";
				valueTexts.Add(valueText0);

				var parentHelps = Db.Helps.Where(w => w.ParentId == 0).OrderBy(o => o.HelpOrder).Select(s => new { s.HelpId, s.HelpTitle, s.HasChildren });
				foreach (var parentHelp in parentHelps)
				{

					//ValueText valueText = new ValueText();
					//valueText.ValueField = parentHelp.HelpId.ToString();
					//valueText.TextField = parentHelp.HelpTitle;
					//valueTexts.Add(valueText);

					// get children
					if ((bool)parentHelp.HasChildren)
					{
						GetChildrenHelpOptions(parentHelp.HelpId, parentHelp.HelpTitle, ref valueTexts);
					}
				}

				var json = new JavaScriptSerializer().Serialize(valueTexts);
				return json;
			}

		}




		public void GetChildrenHelpOptions(int parentId, string parentTitle, ref List<ValueText> valueTexts)
		{
			using (SourceControlEntities Db = new SourceControlEntities())
			{
				var helps = Db.Helps.Where(w => w.ParentId == parentId).OrderBy(o => o.HelpOrder).Select(s => new { s.HelpId, s.IsDoc, s.HelpTitle, s.HasChildren });
				foreach (var help in helps)
				{
					if (help.IsDoc)
					{
						ValueText valueText = new ValueText();
						valueText.ValueField = help.HelpId.ToString();
						valueText.TextField = parentTitle + " > " + help.HelpTitle;
						valueTexts.Add(valueText);
					}


					if ((bool)help.HasChildren)
					{
						GetChildrenHelpOptions(help.HelpId, parentTitle + " > " + help.HelpTitle, ref valueTexts);
					}
				}
			}
		}

	}

}