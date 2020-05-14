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
		public string ParentTable(ColumnDef columnDef)
		{

			StringBuilder sb = new StringBuilder();
			string columnName = columnDef.ColumnName;

			int columnWidth = (columnDef.ElementWidth < 30) ? 300 : columnDef.ElementWidth;
			int columnHeight = (columnDef.ElementHeight < 10) ? 200 : columnDef.ElementHeight;

			// get parent column and value  SELECT 
			var parentTemplate = SessionService.PageTemplate(columnDef.PageTemplateId);
			var childTemplate = SessionService.PageTemplate(columnDef.ChildTemplateId);

			sb.Append("<div id='gridChild" + columnDef.ColumnDefId + "' style='width:" + columnDef.ElementWidth + "px;'></div>");
			sb.Append("<input type='button' value='Add' onclick='Add" + columnDef.ColumnDefId + "(0)' />");
			sb.Append("<input type='button' value='Edit' onclick='Edit" + columnDef.ColumnDefId + "()' />");
			sb.Append("<input type='button' value='Delete' onclick='Delete" + columnDef.ColumnDefId + "()' />");


			DocumentReady.AppendLine("$(\"#gridChild" + columnDef.ColumnDefId + "\").kendoGrid({");
			DocumentReady.AppendLine("   dataSource: {");
			DocumentReady.AppendLine("		type: \"json\",");
			DocumentReady.AppendLine("		transport: {");
			DocumentReady.AppendLine("			read: {");
			DocumentReady.AppendLine("				url: \"/Page/GetChildData?childPageTemplateId=" + childTemplate.PageTemplateId + "&recordId=\" + $('#InternalId_" + parentTemplate.PageTemplateId + "').val() + \"&parentColumnDefId=" + columnDef.ColumnDefId + "\",");
			DocumentReady.AppendLine("				dataType: \"json\",");
			DocumentReady.AppendLine("				type: \"POST\",");
			DocumentReady.AppendLine("				contentType: \"application/json; charset=utf-8\"");
			DocumentReady.AppendLine("			},");
			DocumentReady.AppendLine("		},");
			DocumentReady.AppendLine("	},");
			DocumentReady.AppendLine("   selectable: \"row\",");
			DocumentReady.AppendLine("   height: " + columnDef.ElementHeight + ",");
			DocumentReady.AppendLine("   sortable: true,");
			DocumentReady.AppendLine("   pageable: false,");
			DocumentReady.AppendLine("   columns: [ ");

			if (columnDef.TextField.Contains(","))
			{
				string[] words = columnDef.TextField.Split(new char[] { ',' });
				foreach (string word in words)
				{
					DocumentReady.AppendLine("		{ field: \"" + word + "\", title: \"" + word + "\" },");
				}
			}
			else
			{
				DocumentReady.AppendLine("		{ field: \"" + columnDef.TextField + "\", title: \"" + columnDef.TextField + "\" }");
			}

			DocumentReady.AppendLine("   ]");
			DocumentReady.AppendLine("   ");
			DocumentReady.AppendLine("});");

			DocumentReady.AppendLine("$('#window" + columnDef.ColumnDefId + "').kendoWindow({");
			DocumentReady.AppendLine("	modal: true,");
			DocumentReady.AppendLine("	scrollable: true,");
			DocumentReady.AppendLine("	title: '" + childTemplate.TemplateName + "',");
			DocumentReady.AppendLine("	animation: {");
			DocumentReady.AppendLine("		open: {");
			DocumentReady.AppendLine("			duration: 100");
			DocumentReady.AppendLine("		}");
			DocumentReady.AppendLine("	},");
			DocumentReady.AppendLine("	visible: false,");
			DocumentReady.AppendLine("	actions: ['Maximize', 'Close'],");
			DocumentReady.AppendLine("	activate: function () {");
			DocumentReady.AppendLine("		//$('#NewTemplateName').focus();");
			DocumentReady.AppendLine("	}");
			DocumentReady.AppendLine("});");


			Functions.AppendLine("function Add" + columnDef.ColumnDefId + "() {");
			Functions.AppendLine("		BindFormData(" + childTemplate.PageTemplateId + ", 0);");
			Functions.AppendLine("		$('#window" + columnDef.ColumnDefId + "').data('kendoWindow').open().center();");
			Functions.AppendLine("	}");

			Functions.AppendLine("	function Edit" + columnDef.ColumnDefId + "() {");
			Functions.AppendLine("		var grid = $('#gridChild" + columnDef.ColumnDefId + "').data('kendoGrid');");
			Functions.AppendLine("		var dataItem = grid.dataItem(grid.select());");
			Functions.AppendLine("		BindFormData(" + childTemplate.PageTemplateId + ", dataItem." + childTemplate.PrimaryKey + ");");
			Functions.AppendLine("		$('#window" + columnDef.ColumnDefId + "').data('kendoWindow').open().center();");
			Functions.AppendLine("	}");

			Functions.AppendLine("	function Delete" + columnDef.ColumnDefId + "() {");
			Functions.AppendLine("		ConfirmMessage('Warning', 'Delete record?');");
			Functions.AppendLine("		$('#dialogYesButton').click(function () {");
			Functions.AppendLine("			var grid = $('#gridChild" + columnDef.ColumnDefId + "').data('kendoGrid');");
			Functions.AppendLine("			var dataItem = grid.dataItem(grid.select());");
			Functions.AppendLine("			$.ajax({");
			Functions.AppendLine("				url: '" + SessionService.VirtualDomain + "/Page/DeleteChildTableRecord',");
			Functions.AppendLine("				type: 'POST',");
			Functions.AppendLine("				data: { pageTemplateId: " + childTemplate.PageTemplateId + ", recordId: dataItem." + childTemplate.PrimaryKey + " },");
			Functions.AppendLine("				async: false,");
			Functions.AppendLine("				dataType: 'text',");
			Functions.AppendLine("				success: function (data) {");
			Functions.AppendLine("					$('#gridChild" + columnDef.ColumnDefId + "').data('kendoGrid').dataSource.read();");
			Functions.AppendLine("				}");
			Functions.AppendLine("			}).done(function () {");
			Functions.AppendLine("				$('#dialogYesNo').data('kendoWindow').close();");
			Functions.AppendLine("			});");
			Functions.AppendLine("		})");

			Functions.AppendLine("		$('#dialogNoButton').click(function () {");
			Functions.AppendLine("			$('#dialogYesNo').data('kendoWindow').close();");
			Functions.AppendLine("		})");
			Functions.AppendLine("	}");

			Functions.AppendLine("	function Save" + columnDef.ColumnDefId + "() {");
			Functions.AppendLine("		var parentId = $('#InternalId_" + parentTemplate.PageTemplateId + "').val();");
			Functions.AppendLine("		$('#Form_" + childTemplate.PageTemplateId + " input[id=" + parentTemplate.PrimaryKey + "]').val(parentId);");
			Functions.AppendLine("		var json = ToJsonString('Form_" + childTemplate.PageTemplateId + "');");
			Functions.AppendLine("		$.ajax({");
			Functions.AppendLine("			url: '" + SessionService.VirtualDomain + "/Page/SaveFormData',");
			Functions.AppendLine("			type: 'POST',");
			Functions.AppendLine("			data: { pageTemplateId: " + childTemplate.PageTemplateId + ", json: json, oldJson: '' },");
			Functions.AppendLine("			async: false,");
			Functions.AppendLine("			dataType: 'json',");
			Functions.AppendLine("			success: function (data) {");
			Functions.AppendLine("				$('#gridChild" + columnDef.ColumnDefId + "').data('kendoGrid').dataSource.read();");
			Functions.AppendLine("			}");
			Functions.AppendLine("		}).done(function () {");
			Functions.AppendLine("			 $('#window" + columnDef.ColumnDefId + "').data('kendoWindow').close();");
			Functions.AppendLine("		});");
			Functions.AppendLine("	}");

			AfterForm.AppendLine("<div id='window" + columnDef.ColumnDefId + "'>");
			AfterForm.AppendLine("	<form id='Form_" + childTemplate.PageTemplateId + "'>");
			AfterForm.AppendLine("		<input type='button' value='Save' onclick='Save" + columnDef.ColumnDefId + "()' />");
			AfterForm.AppendLine(SessionService.FormLayout(childTemplate.PageTemplateId, true));
			AfterForm.AppendLine("	</form>");
			AfterForm.AppendLine("</div>");

			return sb.ToString();
		}




	}
}