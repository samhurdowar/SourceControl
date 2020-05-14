using SourceControl.Models.Db;
using SourceControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using SourceControl.FormLayoutClasses;
using System.Reflection;

namespace SourceControl.Services
{
    public class FormLayoutService
    {

        public static string GetFormLayout(int pageTemplateId, string layoutType)
        {
            FormLayout formLayout = new FormLayout(pageTemplateId);

            formLayout.DocumentReady.AppendLine("");
            formLayout.DocumentReady.AppendLine("<script>");
            formLayout.DocumentReady.AppendLine("[functs]");
            formLayout.DocumentReady.AppendLine("$(document).ready(function () {");

            var pageTemplate = SessionService.PageTemplate(pageTemplateId);

            var layOut = "";
            if (layoutType == "View")
            {
                layOut = pageTemplate.ViewLayout;
            } 
            else if (layoutType == "Search") 
            {
                layOut = pageTemplate.SearchLayout;
            } 
            else
            {
                layOut = pageTemplate.EditLayout;
            }

            // inject primary key
            if (!layOut.Contains("[" + pageTemplate.TableName + "_" + pageTemplate.PrimaryKey + "]"))
            {
                layOut += "[" + pageTemplate.TableName + "_" + pageTemplate.PrimaryKey + "]";
            }


            var pageTemplateId2 = pageTemplate.PageTemplateId2;

            GetLayoutReplacements(pageTemplateId, layoutType, ref layOut, ref formLayout);

            if (pageTemplateId2 > 0)
            {
                formLayout.PageTemplateId = pageTemplateId2;
                GetLayoutReplacements(pageTemplateId2, layoutType, ref layOut, ref formLayout);
            }

            formLayout.DocumentReady.AppendLine("});");
            formLayout.DocumentReady.AppendLine("</script>");
            formLayout.DocumentReady.AppendLine("");

            var dReady = formLayout.DocumentReady.ToString().Replace("[functs]", formLayout.Functions.ToString());
            var finalLayout = dReady + "<form id='Form" + layoutType + "_" + pageTemplateId + "'>" + layOut + "</form>" + formLayout.AfterForm.ToString();

            return finalLayout;
        }

        private static void GetLayoutReplacements(int pageTemplateId, string layoutType, ref string layOut, ref FormLayout formLayout)
        {
            var recordId = "$('#InternalId_" + pageTemplateId + "').val()";
            var pageTemplate = SessionService.PageTemplate(pageTemplateId);

            var columnDefs = SessionService.ColumnDefs(pageTemplateId);
            var columnDefId = columnDefs[0].ColumnDefId.ToString();
            foreach (var columnDef in columnDefs)
            {
                if ((bool)columnDef.IsPrimary)  // display only for Primary key   
                {
                    Type type = typeof(FormLayout);
                    var elementType = "Hidden";

                    var replaceWith = (string)type.InvokeMember(elementType, BindingFlags.InvokeMethod, null, formLayout, new object[] { columnDef });
                    layOut = layOut.Replace("[" + pageTemplate.TableName + "_" + columnDef.ColumnName + "]", replaceWith);

                    if (columnDef.ElementType == "DisplayOnly")
                    {
                        layOut = layOut.Replace("[" + pageTemplate.TableName + "_" + columnDef.ColumnName + "]", formLayout.DisplayOnly(columnDef));
                    }
                }
                else if (columnDef.ElementType == "Note")
                {
                    var linkUpload = "&nbsp;&nbsp;<img src='" + SessionService.VirtualDomain + "\\Images\\plus.png'><a href=\"javascript:AddNote(" + columnDef.PageTemplateId + ", " + columnDef.ColumnDefId + ")\">Add Note</a>";

                    layOut = layOut.Replace("[" + pageTemplate.TableName + "_" + columnDef.ColumnName + "]", formLayout.Note(columnDef));
                    layOut = layOut.Replace("[" + pageTemplate.TableName + "_" + columnDef.ColumnName + "LINK]", linkUpload);
                }
                else if (columnDef.ElementType == "Custom")
                {
                    var elementObject = columnDef.ElementObject.Replace("[PageTemplateId]", pageTemplateId.ToString()).Replace("[ColumnDefId]", columnDefId).Replace("[RecordId]", recordId).Replace("[GT]", ">").Replace("[LT]", "<").Replace("[CL]", ";");
                    formLayout.DocumentReady.AppendLine(columnDef.ElementDocReady.Replace("[PageTemplateId]", pageTemplateId.ToString()).Replace("[ColumnDefId]", columnDefId).Replace("[RecordId]", recordId)).Replace("[GT]", ">").Replace("[LT]", "<").Replace("[CL]", ";");
                    var elementLink = columnDef.ElementLabelLink.Replace("[PageTemplateId]", pageTemplateId.ToString()).Replace("[ColumnDefId]", columnDefId).Replace("[RecordId]", recordId).Replace("[GT]", ">").Replace("[LT]", "<").Replace("[CL]", ";");
                    formLayout.Functions.AppendLine(columnDef.ElementFunction);


                    layOut = layOut.Replace("[" + pageTemplate.TableName + "_" + columnDef.ColumnName + "LINK]", elementLink);
                    layOut = layOut.Replace("[" + pageTemplate.TableName + "_" + columnDef.ColumnName + "]", elementObject);

                }
                else if (columnDef.ElementType == "FileAttachment")
                {
                    string linkUpload = "&nbsp;&nbsp;<img src='" + SessionService.VirtualDomain + "\\Images\\paperclip.png'><a href=\"javascript:UploadFile1(" + columnDef.PageTemplateId + ", " + columnDef.ColumnDefId + ")\">Upload</a><span id='spanUpload" + columnDef.ColumnDefId + "'></span>";

                    layOut = layOut.Replace("[" + pageTemplate.TableName + "_" + columnDef.ColumnName + "]", formLayout.FileAttachment(columnDef));
                    layOut = layOut.Replace("[" + pageTemplate.TableName + "_" + columnDef.ColumnName + "LINK]", linkUpload);

                }
                else
                {
                    if (columnDef.ElementType.Length > 2 || layoutType == "View" || layoutType == "Search")
                    {
                        Type type = typeof(FormLayout);
                        var elementType = columnDef.ElementType;

                        if ((layoutType == "View" || layoutType == "Search") && !"Textarea:CheckboxTrueFalse:CheckboxYesNo:".Contains(elementType)) elementType = "Textbox";

                        var replaceWith = (string)type.InvokeMember(elementType, BindingFlags.InvokeMethod, null, formLayout, new object[] { columnDef });
                        if (layoutType == "Search")
                        {
                            replaceWith = replaceWith.Replace("id='", "id='Search_").Replace("name='", "name='Search_");
                        }
                        layOut = layOut.Replace("[" + pageTemplate.TableName + "_" + columnDef.ColumnName + "]", replaceWith);


                        // set element to span for lookup table fields
                        if (columnDef.LookupTable.Length > 0 && columnDef.ValueField.Length > 0 && columnDef.TextField.Length > 0)
                        {
                            var lookupColumnDefs = SessionService.ColumnDefs(pageTemplate.DbEntityId, columnDef.LookupTable);
                            foreach (var lookupColumnDef in lookupColumnDefs)
                            {
                                elementType = (layoutType == "Search") ? "Textbox" : "Span";

                                replaceWith = (string)type.InvokeMember(elementType, BindingFlags.InvokeMethod, null, formLayout, new object[] { columnDef.LookupTable, lookupColumnDef });

                                if (layoutType == "Search")
                                {
                                    replaceWith = replaceWith.Replace("id='","id='Search_").Replace("name='", "name='Search_");
                                }

                                layOut = layOut.Replace("[" + columnDef.LookupTable + "_" + lookupColumnDef.ColumnName + "]", replaceWith);
                            }
                        }
                    }

                }
            }

        }
    }
}

/*  

	 
*/
