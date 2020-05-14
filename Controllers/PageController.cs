using SourceControl.Common;
using SourceControl.Models.Db;
using SourceControl.Models;
using SourceControl.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace SourceControl.Controllers
{
    public class PageController : Controller
    {
        [HttpPost]
        public string ClearPageFilter(int pageTemplateId)
        {
            Session["WhereFilter" + pageTemplateId] = null;
            return "T";
        }

        public string GetChildData(int childPageTemplateId, int parentColumnDefId, int recordId)
        {
            PageTemplate childPageTemplate = SessionService.PageTemplate(childPageTemplateId);
            
            ColumnDef parentColumnDef = SessionService.ColumnDef(parentColumnDefId);
            PageTemplate parentPageTemplate = SessionService.PageTemplate(parentColumnDef.PageTemplateId);

            var childKey = SessionService.PrimaryKey(childPageTemplate.DbEntityId, childPageTemplate.TableName) + ",";
            if (parentColumnDef.TextField.Contains(childKey))
            {
                childKey = "";
            }

            string fromClause = "FROM " + childPageTemplate.TableName + " WHERE " + SessionService.PrimaryKey(parentPageTemplate.DbEntityId, parentPageTemplate.TableName) + " = " + recordId + " ORDER BY " + parentColumnDef.OrderField;
            var json = DataService.GetJsonFromSQL(childKey + parentColumnDef.TextField, childKey + parentColumnDef.TextField, fromClause, "", false);
            return json;
        }

        [HttpPost]
        public string GetDetailGrid(int pageTemplateId, string refKeyValue)
        {
            PageTemplate pageTemplate = SessionService.PageTemplate(pageTemplateId);

            if (pageTemplate.RefKey2 == 0) return "";

            GridFilters gridFilters = new GridFilters { Logic = "and", Filters = new List<GridFilter> { new GridFilter { Field = pageTemplate.RefKey2Name, Operator = "eq", Value = refKeyValue } } };


            PageTemplate pageTemplate2 = SessionService.PageTemplate(pageTemplate.PageTemplateId2);
            List<GridSort> gridSorts = new List<GridSort>();

            if (pageTemplate2.SortColumns.Length > 2)
            {
                var sortColumns = pageTemplate2.SortColumns;
                if (sortColumns.Contains(","))
                {
                    var items = sortColumns.Split(new char[',']);
                    foreach (var item in items)
                    {
                        var columnId = item.Replace(" ", "").Replace("ASC", "").Replace("DESC", "");
                        var columnName = SessionService.ColumnName(Convert.ToInt32(columnId));
                        GridSort gridSort = new GridSort { Field = columnName, Dir = (item.Contains("ASC")) ? "ASC" : "DESC" };
                        gridSorts.Add(gridSort);
                    }
                } else
                {
                    var columnId = sortColumns.Replace(" ", "").Replace("ASC", "").Replace("DESC", "");
                    var columnName = SessionService.ColumnName(Convert.ToInt32(columnId));
                    GridSort gridSort = new GridSort { Field = columnName, Dir = (sortColumns.Contains("ASC")) ? "ASC" : "DESC" };
                    gridSorts.Add(gridSort);
                }
            }
             
            var json = PageService.GetServerSideRecords(0, 500000, 0, 0, gridSorts, gridFilters, pageTemplate.PageTemplateId2);
            return json;
        }

        [HttpPost]
        public string DeleteChildTableRecord(int pageTemplateId, int recordId)
        {
            PageTemplate pageTemplate = SessionService.PageTemplate(pageTemplateId);
            var dbEntity = SessionService.DbEntity(pageTemplate.DbEntityId);
            using (TargetEntities targetDb = new TargetEntities())
            {
                targetDb.Database.Connection.ConnectionString = dbEntity.ConnectionString;

                string sql = "DELETE FROM " + pageTemplate.TableName + " WHERE " + pageTemplate.PrimaryKey + " = " + recordId;
                targetDb.Database.ExecuteSqlCommand(sql);
                return "T";
            }
        }


        [HttpPost]
        public string GetServerSideRecords(int skip = 0, int take = 0, int page = 0, int pageSize = 0, List<GridSort> sort = null, GridFilters filter = null, int pageTemplateId = 0, string tableNameOveride = "", string selectColumns = "", string sortOveride = "")
        {
            try
            {
                var json = PageService.GetServerSideRecords(skip, take, page, pageSize, sort, filter, pageTemplateId, tableNameOveride, selectColumns, sortOveride);
                return json;
            }
            catch (Exception ex)
            {
                Helper.LogError(ex);
            }
            return "";
        }


        [HttpPost]
        public string GetFormData(int pageTemplateId = 0, string recordId = "", string layoutType = "")
        {
            try
            {
                string tableName = "";
                string primaryKey = "";
                string json = "";

                StringBuilder sbJsonFields = new StringBuilder();
                StringBuilder sbDbFields = new StringBuilder();

                StringBuilder sb = new StringBuilder();
                StringBuilder sbLinkNew = new StringBuilder();

                var pageTemplate = SessionService.PageTemplate(pageTemplateId);
                tableName = pageTemplate.TableName;
                primaryKey = pageTemplate.PrimaryKey;

                if (recordId.Length > 0 && recordId != "0")
                {
                    
                    var dbEntity = SessionService.DbEntity(pageTemplate.DbEntityId);
                    using (TargetEntities targetDb = new TargetEntities())
                    {
                        targetDb.Database.Connection.ConnectionString = dbEntity.ConnectionString;

                        if (pageTemplate.PrimaryKeyType == "TEXT") recordId = "'" + recordId + "'";
                        StringBuilder sbSelect = new StringBuilder();
                        StringBuilder sbFrom = new StringBuilder();
                        sbSelect.Append("SELECT " + tableName + "." + primaryKey + " AS " + tableName + "_" + primaryKey);
                        sbFrom.Append(" FROM " + tableName);
                        var columnDefs = SessionService.ColumnDefs(pageTemplateId);
                        foreach (var columnDef in columnDefs.Where(w => !w.IsPrimary))
                        {
                            if (layoutType == "View" && columnDef.LookupTable.Length > 0 && columnDef.ValueField.Length > 0 && columnDef.TextField.Length > 0)
                            {
                                if (columnDef.ElementType == "MultiSelect")
                                {
                                    // get items and inject into select statement
                                    var lookupValue = targetDb.Database.SqlQuery<string>("SELECT " + columnDef.ColumnName + " FROM " + tableName + " WHERE " + primaryKey + " = " + recordId).FirstOrDefault();
                                    if (lookupValue != null && lookupValue.Length > 0)
                                    {
                                        var injectSql = "SELECT STUFF((SELECT ', ' + " + columnDef.TextField + " FROM (SELECT " + columnDef.TextField + " FROM " + columnDef.LookupTable + " WHERE " + columnDef.ValueField + " IN (" + lookupValue + ")) AS T FoR XML PATH('')), 1, 1, '') AS InjectValue";
                                        var injectValue = targetDb.Database.SqlQuery<string>(injectSql).FirstOrDefault();
                                        if (injectValue != null)
                                        {
                                            sbSelect.Append(", '" + injectValue.Replace("'","''") + "' AS " + tableName + "_" + columnDef.ColumnName);
                                        }
                                    }
                                }
                                else
                                {
                                    sbSelect.Append(", " + columnDef.LookupTable + "." + columnDef.TextField + " AS " + tableName + "_" + columnDef.ColumnName);

                                    // append fields from the lookup table
                                    var lookupColumnDefs = SessionService.ColumnDefs(pageTemplate.DbEntityId, columnDef.LookupTable);
                                    foreach (var lookupColumnDef in lookupColumnDefs)
                                    {
                                        sbSelect.Append(", " + columnDef.LookupTable + "." + lookupColumnDef.ColumnName + " AS " + columnDef.LookupTable + "_" + lookupColumnDef.ColumnName);
                                    }


                                    sbFrom.Append(" LEFT JOIN " + columnDef.LookupTable + " ON " + columnDef.LookupTable + "." + columnDef.ValueField + " = " + tableName + "." + columnDef.ColumnName + " ");
                                }

                            } 
                            else
                            {
                                sbSelect.Append(", " + tableName + "." + columnDef.ColumnName + " AS " + tableName + "_" + columnDef.ColumnName);
                            }
                        }

                        
                        var recordSql = sbSelect.ToString() + sbFrom.ToString() + " WHERE " + tableName + "." + primaryKey + " = " + recordId + " FOR JSON PATH";
                        var recs = targetDb.Database.SqlQuery<string>(recordSql).ToList();
                        json = "";
                        foreach (var rec in recs)
                        {
                            json += rec;
                        }
                        if (json.Length > 2)
                        {
                            json = json.Substring(1, json.Length - 2);
                        }

                        return json;

                    }

                }
                else  // recordId = 0
                {
                    // get defaults for new record
                    sb.Clear();
                    sb.Append("{\"" + tableName + "_" + primaryKey + "\":0");

                    List<ColumnDef> columnDefs = new List<ColumnDef>();

                    columnDefs = SessionService.ColumnDefs(pageTemplateId).Where(w => w.IsRequired == true || (w.DefaultValue != null)).ToList();

                    foreach (var columnDef in columnDefs)
                    {

                        if (columnDef.ElementType == "Note" || columnDef.ElementType == "FileAttachment")
                        {
                            sbLinkNew.Append("DELETE FROM " + columnDef.ElementType + " WHERE RecordId = -" + (columnDef.ColumnDefId + SessionService.UserId) + ";");
                        }


                        var defaultValue = columnDef.DefaultValue;
                        if (columnDef.OverideValue != null && columnDef.OverideValue.Length > 0)
                        {
                            defaultValue = columnDef.OverideValue;
                        }

                        if (columnDef.IsRequired || (defaultValue != null && defaultValue.Length > 0))
                        {
                            sb.Append(",\"" + tableName + "_" + columnDef.ColumnName + "\":");
                            if (defaultValue != null && defaultValue.Length > 0)
                            {
                                if (columnDef.DataType == "NUMBER")
                                {
                                    sb.Append(Helper.ToInt32(defaultValue).ToString());

                                    // add lookup display value
                                    if (columnDef.LookupTable != null && columnDef.LookupTable.Length > 0 && Helper.ToInt32(defaultValue) > 0)
                                    {

                                        string lookUpFields = "";
                                        if (columnDef.TextField.Contains(","))
                                        {
                                            lookUpFields = "(";
                                            string[] fields = columnDef.TextField.Split(new char[] { ',' });
                                            foreach (var field in fields)
                                            {
                                                if (lookUpFields.Length < 3)
                                                {
                                                    lookUpFields += columnDef.LookupTable + "." + field;
                                                }
                                                else
                                                {
                                                    lookUpFields += " + ' - ' + " + columnDef.LookupTable + "." + field;
                                                }

                                            }
                                            lookUpFields += ")";

                                        }
                                        else
                                        {
                                            lookUpFields = "ISNULL(" + columnDef.LookupTable + "." + columnDef.TextField + ",'') ";
                                        }

                                        var displayValue = DataService.GetStringValue("SELECT " + lookUpFields + " FROM " + columnDef.LookupTable + " WHERE " + columnDef.ValueField + " = " + defaultValue);
                                        sb.Append(",\"" + tableName + "_" + columnDef.ColumnName + "_\":\"" + displayValue.Replace("\"", "\"\"") + "\"");
                                    }
                                }
                                else if (columnDef.DataType == "DECIMAL" || columnDef.DataType == "CURRENCY")
                                {
                                    sb.Append(Helper.ToDecimal(defaultValue).ToString());
                                }
                                else if (columnDef.DataType == "TEXT" || columnDef.DataType == "MAXTEXT")
                                {
                                    sb.Append("\"" + defaultValue.ToString() + "\"");
                                }
                                else if (columnDef.DataType == "BOOLEAN")
                                {
                                    if (defaultValue.ToLower() == "true" || defaultValue == "1")
                                    {
                                        sb.Append("true");
                                    }
                                    else
                                    {
                                        sb.Append("false");
                                    }
                                }
                                else if (columnDef.DataType == "DATE")
                                {
                                    if (defaultValue == "getdate()")
                                    {
                                        sb.Append("\"" + DateTime.Now.ToLongDateString() + "\"");
                                    }
                                    else
                                    {
                                        sb.Append("\"" + Helper.ToDateTime(defaultValue).ToLongDateString() + "\"");
                                    }

                                }
                                else
                                {
                                    sb.Append("\"" + defaultValue.ToString() + "\"");
                                }
                            }
                            else if (columnDef.IsRequired == true)
                            {
                                if (columnDef.DataType == "NUMBER")
                                {
                                    sb.Append("0");
                                }
                                else if (columnDef.DataType == "DECIMAL" || columnDef.DataType == "CURRENCY")
                                {
                                    sb.Append("0");
                                }
                                else if (columnDef.DataType == "TEXT" || columnDef.DataType == "MAXTEXT")
                                {
                                    sb.Append("\"\"");
                                }
                                else if (columnDef.DataType == "BOOLEAN")
                                {
                                    sb.Append("false");
                                }
                                else if (columnDef.DataType == "DATE")
                                {
                                    sb.Append("\"" + DateTime.Now.ToLongDateString() + "\"");
                                }
                                else
                                {
                                    sb.Append("\"\"");
                                }
                            }
                            else
                            {
                                if (columnDef.DataType == "NUMBER")
                                {
                                    sb.Append("0");
                                }
                                else if (columnDef.DataType == "DECIMAL" || columnDef.DataType == "CURRENCY")
                                {
                                    sb.Append("0");
                                }
                                else if (columnDef.DataType == "TEXT" || columnDef.DataType == "MAXTEXT")
                                {
                                    sb.Append("\"\"");
                                }
                                else if (columnDef.DataType == "BOOLEAN")
                                {
                                    sb.Append("false");
                                }
                                else if (columnDef.DataType == "DATE")
                                {
                                    sb.Append("\"" + DateTime.Now.ToLongDateString() + "\"");
                                }
                                else
                                {
                                    sb.Append("\"\"");
                                }
                            }
                        }
                    }
                    sb.Append("}");
                    json = sb.ToString();

                    //if (sbLinkNew.Length > 0)
                    //{
                    //    DataService.Execute(sbLinkNew.ToString());
                    //}
                }

                return json;
            }
            catch (Exception ex)
            {
                Helper.LogError(ex);
            }
            return "";


        }

        private void GetJsonDbFields(int pageTemplateId, ref StringBuilder sbJsonFields, ref StringBuilder sbDbFields)
        {
            var tableName = SessionService.TableName(pageTemplateId);
            var columnDefs = SessionService.ColumnDefs(pageTemplateId);

            foreach (var columnDef in columnDefs)
            {

                // lookup field  DropdownSearchOption
                if (columnDef.LookupTable != null && columnDef.LookupTable.Length > 0 && columnDef.ElementType == "DropdownSearchOption")
                {
                    string lookUpFields = "";
                    if (columnDef.TextField.Contains(","))
                    {
                        string[] fields = columnDef.TextField.Split(new char[] { ',' });
                        lookUpFields = columnDef.LookupTable + "." + fields[0];
                    }
                    else
                    {
                        lookUpFields = columnDef.LookupTable + "." + columnDef.TextField;
                    }
                    sbJsonFields.Append("," + tableName + "_" + columnDef.ColumnName + "," + tableName + "_" + columnDef.ColumnName + "_");
                    sbDbFields.Append("," + tableName + "." + columnDef.ColumnName + ",(SELECT " + lookUpFields + " FROM " + columnDef.LookupTable + " WHERE " + columnDef.ValueField + " = " + tableName + "." + columnDef.ColumnName + ")");

                }
                else if (columnDef.LookupTable != null && columnDef.LookupTable.Length > 0 && columnDef.ElementType == "DropdownSimple")
                {

                    string lookUpFields = "";
                    if (columnDef.TextField.Contains(","))
                    {
                        lookUpFields = "(";
                        string[] fields = columnDef.TextField.Split(new char[] { ',' });
                        foreach (var field in fields)
                        {
                            if (lookUpFields.Length < 3)
                            {
                                lookUpFields += columnDef.LookupTable + "." + field;
                            }
                            else
                            {
                                lookUpFields += " + ' - ' + " + columnDef.LookupTable + "." + field;
                            }
                        }
                        lookUpFields += ")";
                    }
                    else
                    {
                        lookUpFields = columnDef.LookupTable + "." + columnDef.TextField;
                    }
                    sbJsonFields.Append("," + tableName + "_" + columnDef.ColumnName);
                    sbDbFields.Append("," + tableName + "." + columnDef.ColumnName);
                    //dbFld += ",(SELECT " + lookUpFields + " FROM " + columnDef.LookupTable + " WHERE " + columnDef.ValueField + " = " + tableName + "." + columnDef.ColumnName + ")";

                    //sb.Append(", (SELECT " + lookUpFields + " FROM " + columnDef.LookupTable + " WHERE " + columnDef.ValueField + " = " + tableName + "." + columnDef.ColumnName + ") AS " + columnDef.ColumnName + "_ ");
                }
                else if (columnDef.ElementType == "DisplayOnly" || columnDef.ColumnName == "AddBy" || columnDef.ColumnName == "ChangeBy")  // Lookup user names
                {
                    if (columnDef.ColumnName == "AddBy" || columnDef.ColumnName == "ChangeBy")
                    {
                        sbJsonFields.Append("," + tableName + "_" + columnDef.ColumnName + "_");
                        sbDbFields.Append(",(SELECT FullName FROM AppUser WHERE UserId = " + tableName + "." + columnDef.ColumnName + ")");
                        //sb.Append(", (SELECT FullName FROM AppUser WHERE UserId = " + tableName + "." + columnDef.ColumnName + ") AS " + columnDef.ColumnName + "_ ");
                    }
                    else
                    {
                        sbJsonFields.Append("," + tableName + "_" + columnDef.ColumnName + "_");
                        sbDbFields.Append("," + tableName + "." + columnDef.ColumnName);
                        //sb.Append(", " + columnDef.ColumnName + " AS " + columnDef.ColumnName + "_ ");
                    }
                }
                else if (columnDef.ElementType == "Hidden")
                {
                    if (columnDef.OverideValue.Length > 0)
                    {
                        sbJsonFields.Append("," + tableName + "_" + columnDef.ColumnName);
                        sbDbFields.Append(",'" + columnDef.OverideValue + "'");
                    }
                    else
                    {
                        sbJsonFields.Append("," + tableName + "_" + columnDef.ColumnName + "_");
                        sbDbFields.Append("," + tableName + "." + columnDef.ColumnName);
                    }
                }
                else
                {
                    if (columnDef.IsPrimary == false)
                    {
                        sbJsonFields.Append("," + tableName + "_" + columnDef.ColumnName);
                        sbDbFields.Append("," + tableName + "." + columnDef.ColumnName);
                    }

                }
            }
        }

        [HttpPost]
        public string SaveFormData(int pageTemplateId, string json, string oldJson)
        {
            var id = DataService.UpdateRecord(pageTemplateId, json, oldJson);
            return id;
        }


        public string GetOptionsByColumnDefId(int pageTemplateId, int columnDefId)
        {
            var columnDef = SessionService.ColumnDefs(pageTemplateId).Where(w => w.ColumnDefId == columnDefId).FirstOrDefault();

            string orderBy = "";
            if (columnDef.OrderField.Length > 3)
            {
                orderBy = " ORDER BY " + columnDef.OrderField;
            }

            string textField = "";

            if (columnDef.TextField.Contains(","))
            {

                var columns = columnDef.TextField.Split(new char[] { ',' });

                foreach (var columnName in columns)
                {
                    var dataType = SessionService.DataType(pageTemplateId, columnName);

                    if (dataType == "TEXT")
                    {
                        if (textField.Length == 0)
                        {
                            textField = columnName;
                        }
                        else
                        {
                            textField = textField + " + ' - ' + " + columnName;
                        }
                    }
                    else
                    {
                        if (textField.Length == 0)
                        {
                            textField = "CAST(" + columnName + " AS varchar)";
                        }
                        else
                        {
                            textField = textField + " + ' - ' + " + "CAST(" + columnName + " AS varchar)";
                        }
                    }
                }
            }
            else
            {
                textField = columnDef.TextField;
            }

            string sql = "SELECT CAST(" + columnDef.ValueField + " AS varchar) AS ValueField, (" + textField + ") AS TextField FROM " + columnDef.LookupTable + " " + orderBy;
            string json = "";
            PageTemplate pageTemplate = SessionService.PageTemplate(pageTemplateId);
            var dbEntity = SessionService.DbEntity(pageTemplate.DbEntityId);
            using (TargetEntities targetDb = new TargetEntities())
            {
                targetDb.Database.Connection.ConnectionString = dbEntity.ConnectionString;
                var recs = targetDb.Database.SqlQuery<ValueText>(sql);
                json = Newtonsoft.Json.JsonConvert.SerializeObject(recs);
                if (json.Length < 2)
                {
                    json = "[];";
                }
            }

            if (columnDef.AddBlankOption)
            {
                json = "[{ \"ValueField\":\"\", \"TextField\":\"\"}," + json.Substring(1, json.Length - 1);
            }
            return json;
        }

        public string GetCustomOptions(int pageTemplateId, int columnDefId)
        {
            string json = "";

            PageTemplate pageTemplate = SessionService.PageTemplate(pageTemplateId);
            var dbEntity = SessionService.DbEntity(pageTemplate.DbEntityId);
            using (TargetEntities targetDb = new TargetEntities())
            {
                targetDb.Database.Connection.ConnectionString = dbEntity.ConnectionString;

                List<ValueText> recs = targetDb.Database.SqlQuery<ValueText>("SELECT OptionValue AS ValueField, OptionText AS TextField FROM CustomOption WHERE ColumnDefId = " + columnDefId + " ORDER BY OptionText").ToList();

                json = Newtonsoft.Json.JsonConvert.SerializeObject(recs);
                if (json != null && json.Length > 2) json = "[{ \"ValueField\":\"\", \"TextField\":\"\"}," + json.Substring(1, json.Length - 1);
            }

            return json;
        }

    }
}





