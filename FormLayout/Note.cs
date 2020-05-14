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
		public string Note(ColumnDef columnDef)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("   <span id='spanNote" + columnDef.ColumnDefId + "' style='width:" + columnDef.ElementWidth + "px;'></span>");

			DocumentReady.AppendLine("			$.ajax({");
			DocumentReady.AppendLine("				url: '" + SessionService.VirtualDomain + "/Note/GetPartialNote',");
			DocumentReady.AppendLine("				type: 'POST',");
			DocumentReady.AppendLine("				data: { columnDefId: " + columnDef.ColumnDefId + ", recordId: $('#InternalId_" + columnDef.PageTemplateId + "').val() },");
			DocumentReady.AppendLine("				async: false,");
			DocumentReady.AppendLine("				dataType: 'text',");
			DocumentReady.AppendLine("				success: function (data) {");
			DocumentReady.AppendLine("					$('#spanNote" + columnDef.ColumnDefId + "').html(data);");
			DocumentReady.AppendLine("				}");
			DocumentReady.AppendLine("			}).done(function () {");
			DocumentReady.AppendLine("			});");

			return sb.ToString();
		}

	}
}