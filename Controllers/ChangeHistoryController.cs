using SourceControl.Models.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace SourceControl.Controllers
{
    public class ChangeHistoryController : Controller
    {
        // GET: ChangeHistory
        public ActionResult Index()
        {
            return View();
        }


			[AllowAnonymous]
			public ActionResult ChangeHistoryWindow(int pageTemplateId, int recordId)
			{
				using (SourceControlEntities Db = new SourceControlEntities())
				{
					// Get data for the record column
					StringBuilder sb = new StringBuilder();
					sb.AppendLine("<table class='change-history' style='width:98%;'>");
					sb.AppendLine("<tr>");
					sb.AppendLine("<td>Modified Date</td>");
					sb.AppendLine("<td>Updated By</td>");
					sb.AppendLine("<td>Changes</td>");
					sb.AppendLine("</tr>");
					//var recs = Db.ChangeHistories.Where(w => w.PageTemplateId == pageTemplateId && w.RecordId == recordId).OrderByDescending(o => o.InsertDate);
					//foreach (var rec in recs)
					//{
					//	sb.AppendLine("<tr class='change-history-tr'>");
					//	sb.AppendLine("<td>" + rec.InsertDate.ToString("MM/dd/yyyy hh:mm tt") + "</td>");
					//	sb.AppendLine("<td>" + rec.Username + "</td>");
					//	sb.AppendLine("<td>" + rec.ChangeHistoryText.Replace("] [", "]<br>[") + "</td>");
					//	sb.AppendLine("</tr>");
					//}
					sb.AppendLine("</table>");
					ViewBag.DataValue = sb.ToString();
				}


				return View();
			}

	}
}