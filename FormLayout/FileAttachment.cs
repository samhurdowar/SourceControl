using SourceControl.Models.Db;
using SourceControl.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace SourceControl.FormLayoutClasses
{
	public partial class FormLayout
	{
		public string FileAttachment(ColumnDef columnDef)
		{
			

			StringBuilder sb = new StringBuilder();
			sb.AppendLine("   <span id='spanDownload" + columnDef.ColumnDefId + "'></span>");

			DocumentReady.AppendLine("			$.ajax({");
			DocumentReady.AppendLine("				url: '" + SessionService.VirtualDomain + "/FileAttachment/GetAttachedFiles',");
			DocumentReady.AppendLine("				type: 'POST',");
			DocumentReady.AppendLine("				data: { pageTemplateId: " + columnDef.PageTemplateId + ",columnDefId: " + columnDef.ColumnDefId + ", recordId: $('#InternalId_" + columnDef.PageTemplateId + "').val() },");
			DocumentReady.AppendLine("				async: false,");
			DocumentReady.AppendLine("				dataType: 'text',");
			DocumentReady.AppendLine("				success: function (data) {");
			DocumentReady.AppendLine("					$('#spanDownload" + columnDef.ColumnDefId + "').html(data);");
			DocumentReady.AppendLine("				}");
			DocumentReady.AppendLine("			}).done(function () {");
			DocumentReady.AppendLine("			});");

			return sb.ToString();
		}

	}
}