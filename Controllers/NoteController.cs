using SourceControl.Models.Db;
using SourceControl.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace SourceControl.Controllers
{
	public class NoteController : Controller
	{
		public string GetPartialNote(int columnDefId, int recordId)
		{

			using (SourceControlEntities Db = new SourceControlEntities())
			{

				var rec = Db.Notes.Where(w => w.ColumnDefId == columnDefId && w.RecordId == recordId).OrderByDescending(o => o.InsertDate).FirstOrDefault();
				if (rec != null)
				{
					var txt = rec.NoteText;
					if (txt.Length > 150)
					{
						txt = txt.Substring(0, 150) + "... <a href='javascript:GetFullNote(" + columnDefId + "," + recordId + ",0)'>[more]</a>";
					} else
					{
						txt = txt + "... <a href='javascript:GetFullNote(" + columnDefId + "," + recordId + ",0)'>[more]</a>";
					}
					return txt;
				}

				return "";
			}

		}

		public string SaveNote(int pageTemplateId, int columnDefId, int recordId, string noteText)
		{

			using (SourceControlEntities Db = new SourceControlEntities())
			{
				var note = new Note
				{
					ColumnDefId = columnDefId,
					InsertBy = SessionService.UserId,
					RecordId = recordId,
					NoteText = noteText,
					InsertDate = DateTime.Now
				};

				Db.Notes.Add(note);
				Db.SaveChanges();


				return GetPartialNote(columnDefId, recordId);
			}

		}

		public ActionResult NoteWindow(int columnDefId, int recordId, int getAll)
		{
			using (SourceControlEntities Db = new SourceControlEntities())
			{
				// Get data for the record column
				StringBuilder sb = new StringBuilder();
				sb.AppendLine("<table class='note-table' style='width:98%;'>");
				sb.AppendLine("<tr>");
				sb.AppendLine("<td>Date</td>");
				sb.AppendLine("<td>Added By</td>");
				sb.AppendLine("<td>Note</td>");
				sb.AppendLine("</tr>");
				var recs = Db.Notes.Where(w => w.ColumnDefId == columnDefId && w.RecordId == recordId).OrderByDescending(o => o.InsertDate);

				foreach (var rec in recs)
				{
					sb.AppendLine("<tr class='note-table-tr'>");
					sb.AppendLine("<td>" + rec.InsertDate.ToString("MM/dd/yyyy hh:mm tt") + "</td>");
					sb.AppendLine("<td>" + rec.InsertByName + "</td>");
					sb.AppendLine("<td>" + rec.NoteText + "</td>");
					sb.AppendLine("</tr>");
				}
				sb.AppendLine("</table>");
				ViewBag.DataValue = sb.ToString();
			}


			return View();
		}


	}
}