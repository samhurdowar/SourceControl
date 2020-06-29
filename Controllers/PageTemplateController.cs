using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using SourceControl.Common;
using Newtonsoft.Json;
using SourceControl.Models.Db;
using System.Web.Script.Serialization;
using System.Data.Entity;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using SourceControl.Services;
using SourceControl.Models;
using System.Data.SqlClient;

namespace SourceControl.Controllers
{
	public class PageTemplateController : Controller
	{
		public string GetDbEntity()
		{
			using (SourceControlEntities Db = new SourceControlEntities())
			{
				var recs = Db.DbEntities.OrderBy(o => o.EntityName).Select(s => new { ValueField = s.DbEntityId, TextField = s.EntityName} );
				return JsonConvert.SerializeObject(recs);
            }
		}

		public string LoadGridColumn(int pageTemplateId)
		{
			using (SourceControlEntities Db = new SourceControlEntities())
			{
				SortGridColumn("", 0, 0, 0, pageTemplateId);
				SortSortColumn("", 0, 0, 0, pageTemplateId);

				var gridColumns = Db.GridColumns.Where(w => w.PageTemplateId == pageTemplateId).OrderBy(o => o.SortOrder);
				int[] ids = gridColumns.Select(s => s.ColumnDefId).ToArray();
				var gridColumns_ = Db.ColumnDefs.Where(w => w.PageTemplateId == pageTemplateId && !ids.Contains(w.ColumnDefId)).OrderBy(o => o.ColumnName);

				var sortColumns = Db.SortColumns.Where(w => w.PageTemplateId == pageTemplateId).OrderBy(o => o.SortOrder);
				int[] ids2 = sortColumns.Select(s => s.ColumnDefId).ToArray();
				var sortColumns_ = Db.ColumnDefs.Where(w => w.PageTemplateId == pageTemplateId && !ids2.Contains(w.ColumnDefId)).OrderBy(o => o.ColumnName);

				var json = "{ \"GridColumns\": " + JsonConvert.SerializeObject(gridColumns) + ", \"GridColumns_\": " + JsonConvert.SerializeObject(gridColumns_) + ", \"SortColumns\": " + JsonConvert.SerializeObject(sortColumns) + ", \"SortColumns_\": " + JsonConvert.SerializeObject(sortColumns_) + "}";
				return json;
			}
		}
		
		public string SortGridColumn(string id, int fromColumnIndex, int toColumnIndex, int newOrder, int pageTemplateId = 0)
		{
            try
            {
				var gridColumnId = 0;
				var oldOrder = 0;
				using (SourceControlEntities Db = new SourceControlEntities())
				{

					// set to available.  Contains GridColumnId
					if (fromColumnIndex == 2 && toColumnIndex == 1 && id.Contains("GridColumnId"))
					{
						gridColumnId = Convert.ToInt32(id.Replace("GridColumnId", ""));
						var gridColumn = Db.GridColumns.Find(gridColumnId);
						pageTemplateId = gridColumn.PageTemplateId;

						newOrder = 10000;  // force reorder of everything
						Db.Database.ExecuteSqlCommand("DELETE FROM GridColumn WHERE GridColumnId = " + gridColumnId);
					}

					// set to be in grid.  Contains ColumnDefId
					if (fromColumnIndex == 1 && toColumnIndex == 2 && id.Contains("ColumnDefId"))
					{
						var columnDefId = Convert.ToInt32(id.Replace("ColumnDefId", ""));
						var columnDef = Db.ColumnDefs.Find(columnDefId);
						pageTemplateId = columnDef.PageTemplateId;
						oldOrder = 10000;  // reorder records >= newOrder

						// add GridColumn
						var gridColumn = new GridColumn { ColumnDefId = columnDefId, PageTemplateId = pageTemplateId, SortOrder = newOrder };
						Db.GridColumns.Add(gridColumn);
						Db.SaveChanges();
						gridColumnId = gridColumn.GridColumnId;
					}


					// Reorder within grid.  Contains GridColumnId
					if (fromColumnIndex == 2 && toColumnIndex == 2 && id.Contains("GridColumnId"))
					{
						gridColumnId = Convert.ToInt32(id.Replace("GridColumnId", ""));
						var gridColumn = Db.GridColumns.Find(gridColumnId);
						pageTemplateId = gridColumn.PageTemplateId;
						oldOrder = gridColumn.SortOrder;

						// update GridColumn.SortOrder
						gridColumn.SortOrder = newOrder;
						Db.Entry(gridColumn).State = EntityState.Modified;
						Db.SaveChanges();
					}

					// Reorder GridColumn
					if (pageTemplateId > 0)
					{
						Db.Database.ExecuteSqlCommand("dbo.SortGridColumn @pageTemplateId, @gridColumnId, @oldOrder, @newOrder", new[] { new SqlParameter("@pageTemplateId", pageTemplateId), new SqlParameter("@gridColumnId", gridColumnId), new SqlParameter("@oldOrder", oldOrder), new SqlParameter("@newOrder", newOrder) });
					}
				}
				return "";
			}
            catch (Exception ex)
            {
                return ex.Message;
            }
		}

		public string SortSortColumn(string id, int fromColumnIndex, int toColumnIndex, int newOrder, int pageTemplateId = 0)
		{
			try
			{
				var sortColumnId = 0;
				var oldOrder = 0;
				using (SourceControlEntities Db = new SourceControlEntities())
				{

					// set to available.  Contains SortColumnId
					if (fromColumnIndex == 2 && toColumnIndex == 1 && id.Contains("SortColumnId"))
					{
						sortColumnId = Convert.ToInt32(id.Replace("SortColumnId", ""));
						var sortColumn = Db.SortColumns.Find(sortColumnId);
						pageTemplateId = sortColumn.PageTemplateId;

						newOrder = 10000;  // force reorder of everything
						Db.Database.ExecuteSqlCommand("DELETE FROM SortColumn WHERE SortColumnId = " + sortColumnId);
					}

					// set to be in grid.  Contains ColumnDefId
					if (fromColumnIndex == 1 && toColumnIndex == 2 && id.Contains("ColumnDefId"))
					{
						var columnDefId = Convert.ToInt32(id.Replace("ColumnDefId", ""));
						var columnDef = Db.ColumnDefs.Find(columnDefId);
						pageTemplateId = columnDef.PageTemplateId;
						oldOrder = 10000;  // reorder records >= newOrder

						// add SortColumn
						var sortColumn = new SortColumn { ColumnDefId = columnDefId, PageTemplateId = pageTemplateId, SortOrder = newOrder };
						Db.SortColumns.Add(sortColumn);
						Db.SaveChanges();
						sortColumnId = sortColumn.SortColumnId;
					}


					// Reorder within grid.  Contains SortColumnId
					if (fromColumnIndex == 2 && toColumnIndex == 2 && id.Contains("SortColumnId"))
					{
						sortColumnId = Convert.ToInt32(id.Replace("SortColumnId", ""));
						var sortColumn = Db.SortColumns.Find(sortColumnId);
						pageTemplateId = sortColumn.PageTemplateId;
						oldOrder = sortColumn.SortOrder;

						// update SortColumn.SortOrder
						sortColumn.SortOrder = newOrder;
						Db.Entry(sortColumn).State = EntityState.Modified;
						Db.SaveChanges();
					}

					// Reorder SortColumn
					if (pageTemplateId > 0)
					{
						// Reorder exclude the record
						Db.Database.ExecuteSqlCommand("dbo.SortSortColumn @pageTemplateId, @sortColumnId, @oldOrder, @newOrder", new[] { new SqlParameter("@pageTemplateId", pageTemplateId), new SqlParameter("@sortColumnId", sortColumnId), new SqlParameter("@oldOrder", oldOrder), new SqlParameter("@newOrder", newOrder) });
					}
				}
				return "";
			}
			catch (Exception ex)
			{
				return ex.Message;
			}
		}


		public string GetTableOptions(int dbEntityId)
		{
			var sysTables = DataService.SysTables(dbEntityId).Select(s => new { ValueField = s.TableName, TextField = s.TableName });
			var json = JsonConvert.SerializeObject(sysTables);
			return json;
		}

		[HttpPost]
		public string EncryptRecords(int columnDefId)
		{
			var numOfRecord = 0;
			var columnName = "";
			try
            {
				StringBuilder sb = new StringBuilder();
				var i = 0;
				var columnDef = SessionService.ColumnDef(columnDefId);
				var pageTemplate = SessionService.PageTemplate(columnDef.PageTemplateId);
				var dbEntity = SessionService.DbEntity(pageTemplate.DbEntityId);


				using (TargetEntities targetDb = new TargetEntities())
				{
					targetDb.Database.Connection.ConnectionString = dbEntity.ConnectionString;

					var encryptedValue = "";
					columnName = columnDef.ColumnName;
					var tableName = pageTemplate.TableName;
					var primaryKey = pageTemplate.PrimaryKey;
					var primaryKeyType = pageTemplate.PrimaryKeyType;
					var sql = "SELECT CAST(" + primaryKey + " AS nvarchar(36)) AS RecordId, CAST(ISNULL(" + columnName + ", '') AS varchar(max)) AS RecordValue FROM " + tableName;
					var exe = "UPDATE " + tableName + " SET " + columnName + " = '[RecordValue]' WHERE " + primaryKey + " = '[RecordId]';";
					var items = targetDb.Database.SqlQuery<EncrypedRecord>(sql).ToList();
                    foreach (var item in items.Where(w => w.RecordValue.Length > 0))
                    {
						encryptedValue = Crypto.Encrypt(item.RecordValue).Replace("'","''");
						sb.Append(exe.Replace("[RecordId]", item.RecordId).Replace("[RecordValue]", encryptedValue));
						numOfRecord++;
						i++;
						if (i > 50)
                        {
							i = 0;
							targetDb.Database.ExecuteSqlCommand(sb.ToString());
							sb.Clear();
                        }
					}
					if (i > 0 && sb.Length > 0)
					{
						targetDb.Database.ExecuteSqlCommand(sb.ToString());
					}
				}
				return "Successful.  " + numOfRecord + " records were encrypted for column " + columnName + ".";
			}
            catch (Exception ex)
            {
                return ex.Message;
            }

		}


		public string GetImportData(int dbEntityId, string tableName)
        {
            var sql = "";
            var insertStatement = "";
            var insertSqlStatement = "";
            var buildSql = "";
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("TRUNCATE TABLE " + tableName );
            sb.AppendLine("GO");
            sb.AppendLine("SET IDENTITY_INSERT " + tableName + " ON;");
            sb.AppendLine("GO");

            
            var dbEntity = SessionService.DbEntity(dbEntityId);
            using (TargetEntities targetDb = new TargetEntities())
            {
                targetDb.Database.Connection.ConnectionString = dbEntity.ConnectionString;

                // get insert statement
                sql = "SELECT (SELECT 'INSERT INTO " + tableName + "(' + STUFF((SELECT ', ' + ColumnName FROM (SELECT c.Name AS ColumnName FROM sys.columns c JOIN sys.objects o ON o.object_id = c.object_id WHERE o.name = '" + tableName + "' AND c.is_computed = 0 )  AS T FoR XML PATH('')), 1, 1, '') + ') VALUES' AS ColumnNames)";
                insertStatement = targetDb.Database.SqlQuery<string>(sql).FirstOrDefault();
                sb.AppendLine(insertStatement);

                // get insert SQL statement
                sql = @"
                    SELECT (SELECT 'SELECT ''(''' + STUFF((SELECT ', ' + ValuesString FROM (
                    SELECT 
                    CASE 
                    WHEN (is_identity = 1) THEN ' + CAST(' + c.Name + ' AS varchar) + '''  
                    WHEN (system_type_id = 35)  THEN ''''''' + REPLACE(CAST(ISNULL(' + c.Name + ','''') AS varchar(max)), '''''''','''''''''''') + '''''''  
                    WHEN (system_type_id = 36)  THEN ''''''' + CAST(ISNULL(' + c.Name + ','''') AS nvarchar(36))  + '''''''
                    WHEN (system_type_id = 40)  THEN ''''''' + CAST(ISNULL(' + c.Name + ','''') AS varchar) + '''''''  
                    WHEN (system_type_id = 42)  THEN 'datetime2'
                    WHEN (system_type_id = 48)  THEN ''' + ISNULL(' + c.Name + ',0) + '''
                    WHEN (system_type_id = 52)  THEN ''' + CAST(ISNULL(' + c.Name + ', 0) AS varchar) + '''  
                    WHEN (system_type_id = 56)  THEN ''' + CAST(ISNULL(' + c.Name + ', 0) AS varchar) + '''  
                    WHEN (system_type_id = 58)  THEN 'smalldatetime'   
                    WHEN (system_type_id = 59)  THEN ''' + ISNULL(' + c.Name + ',0) + '''
                    WHEN (system_type_id = 60)  THEN ''' + ISNULL(' + c.Name + ',0) + '''
                    WHEN (system_type_id = 61)  THEN ''''''' + CAST(ISNULL(' + c.Name + ','''') AS varchar) + '''''''  
                    WHEN (system_type_id = 62)  THEN ''' + ISNULL(' + c.Name + ',0) + '''
                    WHEN (system_type_id = 98)  THEN 'sql_variant'
                    WHEN (system_type_id = 99)  THEN ''''''' + ISNULL(' + c.Name + ','''') + '''''''
                    WHEN (system_type_id = 104) THEN ''' + CAST(ISNULL(' + c.Name + ',0) AS varchar) + '''  
                    WHEN (system_type_id = 106) THEN ''' + ISNULL(' + c.Name + ',0) + '''
                    WHEN (system_type_id = 108) THEN ''' + ISNULL(' + c.Name + ',0) + '''  
                    WHEN (system_type_id = 122) THEN ''' + ISNULL(' + c.Name + ',0) + '''
                    WHEN (system_type_id = 127) THEN ''' + ISNULL(' + c.Name + ',0) + '''
                    WHEN (system_type_id = 165) THEN 'varbinary'
                    WHEN (system_type_id = 167) THEN ''''''' + REPLACE(ISNULL(' + c.Name + ',''''), '''''''','''''''''''') + '''''''  
                    WHEN (system_type_id = 173) THEN 'binary'
                    WHEN (system_type_id = 175) THEN ''''''' + ISNULL(' + c.Name + ','''') + '''''''  
                    WHEN (system_type_id = 189) THEN 'timestamp'
                    WHEN (system_type_id = 231) THEN ''''''' + REPLACE(CAST(ISNULL(' + c.Name + ','''') AS varchar(max)), '''''''','''''''''''') + '''''''  
                    WHEN (system_type_id = 239) THEN ''''''' + ISNULL(' + c.Name + ','''') + '''''''
                    WHEN (system_type_id = 241) THEN ''''''' + ISNULL(' + c.Name + ','''') + '''''''
                    END 
                    AS ValuesString 
                    FROM sys.columns c JOIN sys.objects o ON o.object_id = c.object_id WHERE o.name = 'PageTemplate' AND c.is_computed = 0
                    )  AS T FoR XML PATH('')), 1, 1, '') + '), '' FROM PageTemplate' AS SqlString)
                ";
                sql = sql.Replace("PageTemplate", tableName);
                insertSqlStatement = targetDb.Database.SqlQuery<string>(sql).FirstOrDefault();

				insertSqlStatement = insertSqlStatement.Replace("SELECT '(' ''' + CAST(ISNULL(guid", "SELECT '(''' + CAST(ISNULL(guid");

				var recs = targetDb.Database.SqlQuery<string>(insertSqlStatement);
                foreach (var rec in recs)
                {
                    sb.AppendLine(rec);
                }

                buildSql = sb.ToString();
                buildSql = buildSql.Substring(0, buildSql.Length - 4);

            }
            sb.Clear();
            sb.AppendLine(buildSql);

            sb.AppendLine("GO");
            sb.AppendLine("SET IDENTITY_INSERT " + tableName + " OFF;");
            sb.AppendLine("GO");


            return sb.ToString();
        }


        public string GetPageTemplates(int dbEntityId)
		{
            using (SourceControlEntities Db = new SourceControlEntities())
			{
				// initialize GridColumn table from PageTemplate.GridColumns
				var gridColumns_ = Db.GridColumns.ToList();
				if (gridColumns_.Count == 0)
                {
					var sortOrder = 0;
					List<GridColumn> gridColumns = new List<GridColumn>();
					var pageTemplates = Db.PageTemplates;
					foreach (var pageTemplate in pageTemplates.Where(w => w.GridColumns.Length > 1))
					{
						sortOrder = 0;
						int[] ids = Array.ConvertAll(pageTemplate.GridColumns.Split(new char[] { ',' }), s => int.Parse(s));
						for (int i = 0; i < ids.Length; i++)
						{
							sortOrder++;
							GridColumn gridColumn = new GridColumn { ColumnDefId = ids[i], PageTemplateId = pageTemplate.PageTemplateId, SortOrder = sortOrder };

							gridColumns.Add(gridColumn);
						}
					}

					Db.GridColumns.AddRange(gridColumns);
					Db.SaveChanges();
				}

				// initialize SortColumn table from PageTemplate.SortColumns
				var sortColumns_ = Db.SortColumns.ToList();
				if (sortColumns_.Count == 0)
				{
					var sortOrder = 0;
					List<SortColumn> sortColumns = new List<SortColumn>();
					var pageTemplates = Db.PageTemplates;
					foreach (var pageTemplate in pageTemplates.Where(w => w.SortColumns.Length > 1))
					{
						sortOrder = 0;
						string[] ids = pageTemplate.SortColumns.Split(new char[] { ',' });
						for (int i = 0; i < ids.Length; i++)
						{
							sortOrder++;

							var id_ = ids[i].Replace("ASC","").Replace("DESC", "").Replace(" ", "");
							SortColumn sortColumn = new SortColumn { ColumnDefId = Convert.ToInt32(id_), PageTemplateId = pageTemplate.PageTemplateId, SortOrder = sortOrder, SortDir = "ASC" };

							sortColumns.Add(sortColumn);
						}
					}

					Db.SortColumns.AddRange(sortColumns);
					Db.SaveChanges();
				}

				var recs = Db.PageTemplates.Where(w => w.DbEntityId == dbEntityId).OrderBy(o => o.TemplateName).Select(s => new { s.PageTemplateId, s.TemplateName });
				var json = JsonConvert.SerializeObject(recs);
                return json;
            }

		}

		public string GetColumnOptions(int pageTemplateId)
		{
            if (pageTemplateId == 0) return "";
            try
            {
                var columnDefs = SessionService.ColumnDefs(pageTemplateId).OrderBy(o => o.DisplayName).Select(s => new { ValueField = s.ColumnDefId, TextField = s.DisplayName }).ToList();

                string json = JsonConvert.SerializeObject(columnDefs);
                return json;
            }
            catch (Exception ex)
            {
                throw;
            }

		}

		public string GetSortColumnOptions(int pageTemplateId)
		{

			var columnDefs1 = SessionService.ColumnDefs(pageTemplateId).Select(s => new { ValueField = s.ColumnDefId + " ASC", TextField = s.DisplayName + " Ascending" }).ToList();
			var columnDefs2 = SessionService.ColumnDefs(pageTemplateId).Select(s => new { ValueField = s.ColumnDefId + " DESC", TextField = s.DisplayName + " Descending" }).ToList();

			columnDefs1.AddRange(columnDefs2);
			columnDefs1.Insert(0, new { ValueField = "", TextField = "" });
            string json = JsonConvert.SerializeObject(columnDefs1.OrderBy(o => o.TextField));
			return json;
		}

		public string GetPageTemplateOptions()
		{
			using (SourceControlEntities Db = new SourceControlEntities())
			{
				var recs = Db.PageTemplates.OrderBy(o => o.TemplateName).Select(s => new { ValueField = s.PageTemplateId, TextField = s.TemplateName, s.TableName });
                return JsonConvert.SerializeObject(recs);
            }
		}

		public string GetColumnOptionsByName(int dbEntityId, string tableName)
		{
			var sysColumns = SessionService.SysColumns(dbEntityId, tableName).Select(s => new { ValueField = s.ColumnName, TextField = s.ColumnName });

			string json = JsonConvert.SerializeObject(sysColumns);
			if (json.Length < 5) json = "[{ \"ValueField\":\"\", \"TextField\":\"\"}]";

			return json;
		}

		public string GetSortColumnOptionsByName(string tableName, int dbEntityId)
		{
			StringBuilder sb = new StringBuilder();
			var json = "";

			sb.Append("SELECT c.name + ' ASC' AS ValueField, c.name + ' Ascending' AS TextField FROM sys.columns c JOIN sys.objects o ON o.object_id = c.object_id WHERE o.type = 'U' AND o.name = '" + tableName + "' ");
			sb.Append("UNION ");
			sb.Append("SELECT c.name + ' DESC' AS ValueField, c.name + ' Descending' AS TextField FROM sys.columns c JOIN sys.objects o ON o.object_id = c.object_id WHERE o.type = 'U' AND o.name = '" + tableName + "' ");
			sb.Append("ORDER BY ValueField");

            var dbEntity = SessionService.DbEntity(dbEntityId);
            using (TargetEntities targetDb = new TargetEntities())
            {
                targetDb.Database.Connection.ConnectionString = dbEntity.ConnectionString;

                var recs = targetDb.Database.SqlQuery<ValueText>(sb.ToString());
				json = Newtonsoft.Json.JsonConvert.SerializeObject(recs);

				json = "[{ \"ValueField\":\"\", \"TextField\":\"\"}," + json.Substring(1, json.Length - 1);

				return json;
			}
		}

		public string GetColumnForLookup(int pageTemplateId)
		{
			var columnDefs = SessionService.ColumnDefs(pageTemplateId).OrderBy(o => o.DisplayName).Select(s => new { ValueField = s.ColumnName, TextField = s.ColumnName }).ToList();
			columnDefs.Add(new { ValueField = "", TextField = "" });

			var json = JsonConvert.SerializeObject(columnDefs);
            return json;
		}

		public string GetColumnDefs(int pageTemplateId)
		{
			var columnDefs = SessionService.ColumnDefs(pageTemplateId).OrderBy(o => o.ColumnOrder);

			var json = JsonConvert.SerializeObject(columnDefs);
            return json;
		}

		public string GetColumnDefInline(int pageTemplateId)
		{
			var columnDefs = SessionService.ColumnDefs(pageTemplateId).OrderBy(o => o.ColumnName);
			string obj = JsonConvert.SerializeObject(columnDefs);
            return obj;
		}

		[HttpPost]
		public string GetCustomOption(int customDefId, int dbEntityId)
		{
            var dbEntity = SessionService.DbEntity(dbEntityId);
            using (TargetEntities targetDb = new TargetEntities())
            {
                targetDb.Database.Connection.ConnectionString = dbEntity.ConnectionString;
                var recs = targetDb.Database.SqlQuery<CustomOption>("SELECT * FROM CustomOption WHERE ColumnDefId = " + customDefId);
				string json = JsonConvert.SerializeObject(recs);
                return json;
			}
		}

		[HttpPost]
		public string GetPageTemplateData(int pageTemplateId)
		{
			SessionService.ClearPageTemplateSessions(pageTemplateId);  // force reload

			var pageTemplate = SessionService.PageTemplate(pageTemplateId);
            var pageTemplate_ = JsonConvert.SerializeObject(pageTemplate, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

			var json = "{ \"GridColumns\": {} , \"PageTemplate\": " + pageTemplate_ + " }";

			return json;
		}


		[HttpPost]
		public string IsExistingTable(string tableName)
		{
			using (SourceControlEntities Db = new SourceControlEntities())
			{
				var rec = Db.Database.SqlQuery<ValueText>("SELECT c.name AS ValueField, c.name AS TextField FROM sys.columns c JOIN sys.objects o ON o.object_id = c.object_id WHERE o.type = 'U' AND o.name = '" + tableName + "' ORDER BY c.name").FirstOrDefault();
				if (rec != null) return "T";
			}

			return "F";
		}

		[AcceptVerbs(HttpVerbs.Post)]
		public string SavePageTemplate(int pageTemplateId, string json)
		{
			SessionService.ClearPageTemplateSessions(pageTemplateId);
			string msg = DataService.UpdateRecordByTable(1, "PageTemplate", json);
			return msg;
		}


		[HttpPost]
		public string SaveCustomOption(int dbEntityId, string tableName, string json)
		{
			var id = DataService.UpdateRecordByTable(dbEntityId, tableName, json);
			return id;
		}

		[HttpPost]
		public string SaveColumnDef(int pageTemplateId, string json)
		{
			SessionService.ClearPageTemplateSessions(pageTemplateId);
			var msg = DataService.UpdateRecordByTable(1, "ColumnDef", json);
			SessionService.ResetPageSession(pageTemplateId);
			return msg;
		}




		[HttpPost]
		[ValidateInput(false)]
		public string SaveLayout(int pageTemplateId, string layout, string columnName)
		{
			SessionService.ClearPageTemplateSessions(pageTemplateId);

			try
			{
                using (SourceControlEntities Db = new SourceControlEntities())
                {
                    Db.Database.ExecuteSqlCommand("UPDATE PageTemplate SET " + columnName + " = @Layout WHERE PageTemplateId = " + pageTemplateId, new SqlParameter("@Layout", layout));
                }
                    
				SessionService.ResetPageSession(pageTemplateId);
				return "";
			}
			catch (Exception ex)
			{
				return "Unable to process UpdateRecord() - " + ex.Message;
			}
		}

		[HttpPost]
		public string CreatePageTemplate(int dbEntityId, string newTemplateName, string newPageType, string newTableName)
		{
			try
			{
				using (SourceControlEntities Db = new SourceControlEntities())
				{
					var sql = "INSERT INTO PageTemplate(DbEntityId,TemplateName,TableName,PageType) VALUES(" + dbEntityId + ",'" + newTemplateName + "','" + newTableName + "','" + newPageType + "'); SELECT CAST(@@IDENTITY AS varchar(250));";
					var pageTemplateId = Convert.ToInt32(Db.Database.SqlQuery<string>(sql).FirstOrDefault());

                    return pageTemplateId.ToString();
				}
			}
			catch (Exception ex)
			{
				return "Unable to process - " + ex.Message;
			} 
		}

		public string GetColumnDefById(int columnDefId)
		{
			if (columnDefId == 0)
			{
				ColumnDef columnDef = new ColumnDef();
				columnDef.ColumnName = "";
				columnDef.DisplayName = "";
				columnDef.IsRequired = false;
				columnDef.ShowInGrid = true;
				columnDef.ElementType = "Textbox";
				columnDef.DataLength = 50;
				columnDef.DataType = "TEXT";
				columnDef.LookupTable = "";
				string obj = JsonConvert.SerializeObject(columnDef);
                return obj;
			}
			else
			{
				var columnDef = SessionService.ColumnDef(columnDefId);
				Session["sec.ColumnDefId" + columnDefId] = null;
				Session["sec.ColumnDefs" + columnDef.PageTemplateId] = null;
				columnDef = SessionService.ColumnDef(columnDefId);
				string obj = JsonConvert.SerializeObject(columnDef);
                return obj;
			}

		}

		[HttpPost]
		public string GetLayoutFromColumnDef(int pageTemplateId, int numOfCol)
		{
			using (SourceControlEntities Db = new SourceControlEntities())
			{

                var pageTemplate = SessionService.PageTemplate(pageTemplateId);
				StringBuilder sb = new StringBuilder();

				sb.AppendLine("<table style='width:95%;'>");
	
				var i = 0;
				var columnDefs = SessionService.ColumnDefs(pageTemplateId).Where(w => !w.IsPrimary).OrderBy(o => o.ColumnOrder);
				foreach (var columnDef in columnDefs)
				{
					i++;

					if (numOfCol==1)
					{
						sb.AppendLine("<tr>");
						sb.AppendLine("<td style='width:20%' nowrap>");
						sb.AppendLine(columnDef.DisplayName);
						sb.AppendLine("</td>");
						sb.AppendLine("<td style='width:80%'>");
						sb.AppendLine("[" + pageTemplate.TableName + "_" + columnDef.ColumnName + "]");
						sb.AppendLine("</td>");

						sb.AppendLine("</tr>");
					} else if (numOfCol > 1)
					{
						if ((i == 1))
						{
							sb.AppendLine("<tr>");
						}

						if (numOfCol == 2)
						{
							sb.AppendLine("<td style='width:17%' nowrap>");
							sb.AppendLine(columnDef.DisplayName);
							sb.AppendLine("</td>");
							sb.AppendLine("<td style='width:28%'>");
							sb.AppendLine("[" + pageTemplate.TableName + "_" + columnDef.ColumnName + "]");
							sb.AppendLine("</td>");

							if ((i % 2) != 0)
							{
								sb.AppendLine("<td style='width:10%' nowrap>&nbsp;</td>");
							}

						}
						else
						{
							sb.AppendLine("<td style='width:9%' nowrap>");
							sb.AppendLine(columnDef.DisplayName);
							sb.AppendLine("</td>");
							sb.AppendLine("<td style='width:20%'>");
							sb.AppendLine("[" + pageTemplate.TableName + "_" + columnDef.ColumnName + "]");
							sb.AppendLine("</td>");

							if ((i % 3) != 0)
							{
								sb.AppendLine("<td style='width:6%' nowrap>&nbsp;</td>");
							}
						}

						if ((i % numOfCol) == 0)
						{
							if (i == columnDefs.Count())
							{
								sb.AppendLine("</tr>");
							} else
							{
								sb.AppendLine("</tr><tr>");
							}
							
						}
					}

				}
				if ((numOfCol > 1 && i != numOfCol))
				{
					for (int x = i; x <= columnDefs.Count(); x++)
					{
						sb.AppendLine("<td nowrap>&nbsp;</td>");
					}
					sb.AppendLine("<tr>");
				}

				sb.AppendLine("</table>");

				return sb.ToString();
			}
		}

		public string GetPageTemplateOptionsWithAdd()
		{
			StringBuilder sb = new StringBuilder();
			using (SourceControlEntities Db = new SourceControlEntities())
			{

				sb.Append("[ {\"ValueField\":\"0\",\"TextField\":\" \"}");

				var pageTemplates = Db.PageTemplates.OrderBy(o => o.TemplateName);
				foreach (var pageTemplate in pageTemplates)
				{
					sb.Append(", {\"ValueField\":\"" + pageTemplate.PageTemplateId + "\",\"TextField\":\"" + pageTemplate.TemplateName + "\"}");
				}
				sb.Append(" ]");

				return sb.ToString();
			}

		}

		[HttpPost]
		public string DeletePageTemplate(int pageTemplateId)
		{
			try
			{
                using (SourceControlEntities Db = new SourceControlEntities())
                {
                    Db.Database.ExecuteSqlCommand("DELETE FROM Menu WHERE MenuPageTemplateId = " + pageTemplateId + ";DELETE FROM PageTemplate WHERE PageTemplateId = " + pageTemplateId + ";DELETE FROM ColumnDef WHERE PageTemplateId = " + pageTemplateId);
                }
				SessionService.ResetPageSession(pageTemplateId);
				return "";
			}
			catch (Exception ex)
			{
				return "Unable to process DeletePageTemplate() - " + ex.Message;
			}
		}

		public string RenameColumnName(int pageTemplateId, int columnDefId, string oldColumnName, string newColumnName)
		{
			try
			{
				using (SourceControlEntities Db = new SourceControlEntities())
				{
					var pageTemplate = Db.PageTemplates.Find(pageTemplateId);

					Db.Database.ExecuteSqlCommand("EXEC sp_RENAME '" + pageTemplate.TableName + "." + oldColumnName + "', '" + newColumnName + "', 'COLUMN'");
				}
				return "";
			}
			catch (Exception ex)
			{
				return ex.Message;
			}
		}

		public string ChangeDataLength(int pageTemplateId, int columnDefId, string currentColumnName, string currentDataType, int currentDataLength, int formDataLength)
		{
			try
			{
				using (SourceControlEntities Db = new SourceControlEntities())
				{
					var pageTemplate = Db.PageTemplates.Find(pageTemplateId);

					Db.Database.ExecuteSqlCommand("ALTER TABLE " + pageTemplate.TableName + " ALTER COLUMN " + currentColumnName + " " + Helper.GetDBDataType(currentDataType) + " (" + formDataLength + ")");
				}
				return "";
			}
			catch (Exception ex)
			{
				return ex.Message;
			}
		}

		public string GetReportClass()
		{
			using (SourceControlEntities Db = new SourceControlEntities())
			{

				List<ValueText> valueTexts = new List<ValueText>();

				string reportLibraryPath = AppDomain.CurrentDomain.BaseDirectory + @"ReportLibrary";

				if (Directory.Exists(reportLibraryPath))
				{
					foreach (string file in Directory.GetFiles(reportLibraryPath))
					{
						if (Path.GetFileName(file).Contains(".cs") && !Path.GetFileName(file).Contains("Designer.cs"))
						{
							ValueText valueText0 = new ValueText();
							valueText0.ValueField = Path.GetFileNameWithoutExtension(file);
							valueText0.TextField = Path.GetFileNameWithoutExtension(file);
							valueTexts.Add(valueText0);
						}
					}
				}

				var json = new JavaScriptSerializer().Serialize(valueTexts);
				return json;
			}

		}
	}


}
