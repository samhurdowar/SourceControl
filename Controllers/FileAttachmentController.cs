using SourceControl.Models.Db;
using SourceControl.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace SourceControl.Controllers
{
   public class FileAttachmentController : Controller
   {


		[HttpPost]
		public void UploadFile(int columnDefId_, int recordId_, HttpPostedFileBase postedFile)
		{

			byte[] bytes;
			using (BinaryReader br = new BinaryReader(postedFile.InputStream))
			{
				bytes = br.ReadBytes(postedFile.ContentLength);
			}

			using (SourceControlEntities Db = new SourceControlEntities())
			{
				Db.FileAttachments.Add(new FileAttachment
				{
					ColumnDefId = columnDefId_,
					RecordId = recordId_,
					FileName = Path.GetFileName(postedFile.FileName),
					FileSize = postedFile.ContentLength,
					UploadDate = DateTime.Now,
					UploadBy = SessionService.UserId,
					ContentType = postedFile.ContentType,
					FileData = bytes
				});

				Db.SaveChanges();
			}

		}

		[HttpPost]
		public FileResult DownloadFile(int fileAttachmentId)
		{
			using (SourceControlEntities Db = new SourceControlEntities())
			{
				FileAttachment file = Db.FileAttachments.ToList().Find(p => p.FileAttachmentId == fileAttachmentId);
				return File(file.FileData, file.ContentType, file.FileName);
			}
		}

		[HttpPost]
		public string GetAttachedFiles(int pageTemplateId, int columnDefId, int recordId)
		{
			using (SourceControlEntities Db = new SourceControlEntities())
			{
				StringBuilder sb = new StringBuilder();

				ColumnDef columnDef = SessionService.ColumnDef(columnDefId);

				sb.AppendLine("");
				var files = Db.FileAttachments.Where(w => w.ColumnDefId == columnDefId && w.RecordId == recordId).OrderBy(o => o.UploadDate);
				if (files != null)
				{
					sb.AppendLine("<select class='site-select-element' id='download" + columnDefId + "_" + recordId + "' onchange='DownloadFile(" + columnDefId + "," + recordId + ")' style='width:" + columnDef.ElementWidth + "px;'>");
					sb.AppendLine("<option value=''>Select file to download (" + files.Count() + " files)</option>");
					foreach (var file in files)
					{
						sb.AppendLine("<option value='" + file.FileAttachmentId + "'>" + file.FileName + "</option>");
						//sb.AppendLine("<a href='javascript:;' onclick='DownloadFile(" + file.FileAttachmentId + ")'>" + file.FileName + "</a><br>");
					}
					sb.AppendLine("</select>");
				}

				
				return sb.ToString();
			}
		}


	}
}