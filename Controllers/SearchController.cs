
using SourceControl.Common;
using SourceControl.Models.Db;
using SourceControl.Services;
using System;
using System.Web.Mvc;


namespace SourceControl.Controllers
{
	public class SearchController : Controller
	{


        public ActionResult Index(int pageTemplateId, string json)
        {
            try
            {
                PageTemplate pageTemplate = SessionService.PageTemplate(pageTemplateId);
                PageTemplate pageTemplate2 = new PageTemplate { PageTemplateId = 0, PrimaryKey = "dummy", PrimaryKey2 = 0 };
                ViewData["PageTemplate"] = pageTemplate;
                ViewData["PageTemplate2"] = pageTemplate2;

                string whereClause = "";
                var formColumnName = "";
                dynamic obj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

                var columnDefs = SessionService.ColumnDefs(pageTemplateId);
                foreach (var columnDef in columnDefs)
                {
                    formColumnName = "Search_" + pageTemplate.TableName + "_" + columnDef.ColumnName;
                    GetWhereClause(columnDef, obj[formColumnName], ref whereClause);

                    // get lookup tables
                    if (columnDef.LookupTable.Length > 0 && columnDef.ValueField.Length > 0 && columnDef.TextField.Length > 0)
                    {
                        var lookupColumnDefs = SessionService.ColumnDefs(pageTemplate.DbEntityId, columnDef.LookupTable);
                        foreach (var lookupColumnDef in lookupColumnDefs)
                        {
                            formColumnName = "Search_" + columnDef.LookupTable + "_" + lookupColumnDef.ColumnName;
                            GetWhereClause(lookupColumnDef, obj[formColumnName], ref whereClause);
                        }
                    }
                }

                if (whereClause.Length > 3)
                {
                    whereClause = whereClause.Substring(0, whereClause.Length - 4);
                }

                Session["WhereFilter" + pageTemplateId] = whereClause;


                return PartialView("~/Views/Home/_GridTemplate.cshtml");
            }
            catch (Exception ex)
            {
                Helper.LogError(ex.Message + "\r\n" + ex.StackTrace);
            }

            return null;
        }

        protected void GetWhereClause(ColumnDef columnDef, object objValue, ref string whereClause)
        {
            try
            {
                if (objValue != null && objValue.ToString().Length > 0)
                {
                    objValue = objValue.ToString();

                    whereClause += columnDef.ColumnName;

                    if (columnDef.DataType == "TEXT" || columnDef.DataType == "MAXTEXT")
                    {
                        whereClause += " LIKE '%" + Helper.ToSafeString(objValue).Replace("'", "''") + "%' AND ";
                    }
                    else if (columnDef.DataType == "DATE")
                    {
                        string dateTime = Helper.ToDbDateTime(objValue);
                        if (dateTime != "null")
                        {
                            whereClause += " = '" + dateTime + "' AND ";
                        }

                    }
                    else if (columnDef.DataType == "BOOLEAN")
                    {
                        int val01 = Helper.ToDbBoolean(objValue);
                        whereClause += " = " + val01 + " AND ";
                    }
                    else
                    {
                        whereClause += " = " + Helper.ToInt32(objValue) + " AND ";
                    }
                }

            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }

        }

    }

}