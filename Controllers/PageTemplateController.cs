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

		public string GetTableOptions(int dbEntityId)
		{
			string json = DataService.GetJsonFromSQL("ValueField,TextField", "name,name", "FROM sys.objects o WHERE o.type = 'U' ORDER BY name", "", false, 0, dbEntityId);
			return json;
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
                    WHEN (system_type_id = 36)  THEN ''''''' + ISNULL(' + c.Name + ','''') + '''''''
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
            DataService.SyncDatabases(dbEntityId);
            using (SourceControlEntities Db = new SourceControlEntities())
			{
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

		public string GetColumnOptionsByName(string tableName, int dbEntityId)
		{

			string json = DataService.GetJsonFromSQL("ValueField,TextField", "c.name,c.name", "FROM sys.columns c JOIN sys.objects o ON o.object_id = c.object_id WHERE o.type = 'U' AND o.name = '" + tableName + "' ORDER BY c.name", "", false, 0, dbEntityId);
			if (json.Length < 3) json = "[{ \"ValueField\":\"\", \"TextField\":\"\"}]";

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
		public string GetPageTemplate(int pageTemplateId)
		{
            Session["sec.PageTemplate" + pageTemplateId] = null;  // force reload

            var pageTemplate = SessionService.PageTemplate(pageTemplateId);
            var json = JsonConvert.SerializeObject(pageTemplate, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

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
				string primaryKey = "";

                // get primary key for table
                var dbEntity = SessionService.DbEntity(dbEntityId);
                using (TargetEntities targetDb = new TargetEntities())
                {
                    targetDb.Database.Connection.ConnectionString = dbEntity.ConnectionString;
                    primaryKey = targetDb.Database.SqlQuery<string>("SELECT TOP 1 a.COLUMN_NAME AS PrimaryKey FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE a JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS b ON a.CONSTRAINT_NAME = b.CONSTRAINT_NAME AND b.CONSTRAINT_TYPE = 'PRIMARY KEY' AND b.TABLE_NAME = '"  + newTableName + "'").FirstOrDefault();
				}

				using (SourceControlEntities Db = new SourceControlEntities())
				{
					var sql = "INSERT INTO PageTemplate(DbEntityId,TemplateName,TableName,PrimaryKey,PageType) VALUES(" + dbEntityId + ",'" + newTemplateName + "','" + newTableName + "','" + primaryKey + "','" + newPageType + "'); SELECT CAST(@@IDENTITY AS varchar(250));";
					var pageTemplateId = Convert.ToInt32(Db.Database.SqlQuery<string>(sql).FirstOrDefault());


                    // sync database
                    Session["sec.SyncDatabases" + dbEntityId] = null;
                    DataService.SyncDatabases(dbEntityId);

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
				string obj = JsonConvert.SerializeObject(columnDef);
                return obj;
			}

		}

		public string GetDefaultSortColumn(int pageTemplateId)
		{
			using (SourceControlEntities Db = new SourceControlEntities())
			{

				var recs = Db.Database.SqlQuery<ValueText>("SELECT ColumnName AS ValueField, DisplayName AS TextField FROM ColumnDef WHERE PageTemplateId = " + pageTemplateId);
				var json = Newtonsoft.Json.JsonConvert.SerializeObject(recs);

				return json;
			}
		}


		//[AcceptVerbs(HttpVerbs.Post)]
		//public string Destroy(string json)
		//{

		//	int pageTemplateId = DataService.GetJsonIntValue(json, "PageTemplateId");
		//	StringBuilder sb = new StringBuilder();
		//	sb.Append("DELETE FROM ColumnDef WHERE PageTemplateId = " + pageTemplateId + ";");
		//	sb.Append("DELETE FROM PageGroupTemplate WHERE PageTemplateId = " + pageTemplateId + ";");
		//	sb.Append("UPDATE MenuTree SET PageTemplateId = 0 WHERE PageTemplateId = " + pageTemplateId + ";");
		//	sb.Append("UPDATE MenuSubMenu SET PageTemplateId = 0 WHERE PageTemplateId = " + pageTemplateId + ";");
		//	sb.Append("DELETE FROM PageTemplate WHERE PageTemplateId = " + pageTemplateId + ";");

		//	DataService.Execute(sb.ToString());
		//	return "T";
		//}


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


		//[HttpPost]
		//public string DeleteCustomOption(int customOptionId)
		//{
		//	try
		//	{
		//		DataService.Execute("DELETE FROM CustomOption WHERE CustomOptionId = " + customOptionId);
		//		return "";
		//	}
		//	catch (Exception ex)
		//	{
		//		return "Unable to process DeleteCustomOption() - " + ex.Message;
		//	}
		//}

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

		//[HttpPost]
		//public string DeleteColumnDef(int columnDefId)
		//{
		//	try
		//	{
		//		using (SourceControlEntities Db = new SourceControlEntities())
		//		{
		//			var columnDef = Db.ColumnDefs.Find(columnDefId);
		//			var columnName = columnDef.ColumnName;

  //                  var pageTemplate = SessionService.PageTemplate(columnDef.PageTemplateId);
		//			var tableName = pageTemplate.TableName;

		//			Db.Database.ExecuteSqlCommand("EXEC DropConstraint '" + tableName + "', '" + columnName + "'");

		//			Db.Database.ExecuteSqlCommand("ALTER TABLE " + tableName + " DROP COLUMN " + columnName);
		//		}

		//	}
		//	catch (Exception)
		//	{
		//		//return "Unable to process deleting column.()<br>" + ex.Message;
		//	}

		//	try
		//	{
		//		DataService.Execute("DELETE FROM ColumnDef WHERE ColumnDefId = " + columnDefId);
		//	}
		//	catch (Exception)
		//	{

		//	}

		//	return "";

		//}

		//[HttpPost]
		//public string AddColumnDef(int pageTemplateId, string json)
		//{
		//	try
		//	{
		//		string msg = DataService.UpdateRecordByTable(1, "ColumnDef", json);
		//		SessionService.ResetPageSession(pageTemplateId);

		//		// set PageTemplateId
		//		int columnDefId = 0;
		//		if (int.TryParse(msg, out columnDefId))
		//		{
		//			DataService.Execute("UPDATE ColumnDef SET PageTemplateId = " + pageTemplateId + " WHERE ColumnDefId = " + columnDefId);

		//			// create field in database table
		//			using (SourceControlEntities Db = new SourceControlEntities())
		//			{
		//				var pageTemplate = Db.PageTemplates.Find(pageTemplateId);
		//				var columnDef = Db.ColumnDefs.Find(columnDefId);

		//				int dataLength = DataService.GetJsonIntValue(json, "DataLength");
		//				string dataType = DataService.GetJsonStringValue(json, "DataType");
		//				bool isRequired = DataService.GetJsonBooleanValue(json, "IsRequired");
		//				var defaultValue = columnDef.DefaultValue;

		//				StringBuilder sb = new StringBuilder();
		//				sb.Append("ALTER TABLE " + pageTemplate.TableName + " ");
		//				sb.Append("ADD " + columnDef.ColumnName + " ");

		//				// set datatype & length  TEXT  MAXTEXT  NUMBER  DECIMAL  CURRENCY  BOOLEAN  DATE  DATETIME
		//				switch (dataType)
		//				{
		//					case "TEXT":
		//						if (dataLength == 0) dataLength = 50;
		//						sb.Append("varchar(" + dataLength + ") ");
		//						break;
		//					case "MAXTEXT":
		//						sb.Append("varchar(max) ");
		//						break;
		//					case "NUMBER":
		//						sb.Append("int ");
		//						break;
		//					case "DECIMAL":
		//						sb.Append("decimal ");
		//						break;
		//					case "CURRENCY":
		//						sb.Append("money ");
		//						break;
		//					case "BOOLEAN":
		//						sb.Append("bit ");
		//						break;
		//					case "DATE":
		//						sb.Append("date ");
		//						break;
		//					case "DATETIME":
		//						sb.Append("datetime ");
		//						break;
		//					default:
		//						break;
		//				}


		//				// set null
		//				if (isRequired)
		//				{
		//					sb.Append("not null ");
		//				}

		//				// set default  TEXT  MAXTEXT  NUMBER  DECIMAL  CURRENCY  BOOLEAN  DATE  DATETIME
		//				switch (dataType)
		//				{

		//					case "TEXT":
		//						sb.Append("default '" + defaultValue + "' ");
		//						break;
		//					case "MAXTEXT":
		//						sb.Append("default '" + defaultValue + "' ");
		//						break;
		//					case "NUMBER":
		//						if (defaultValue.Length > 0)
		//						{
		//							sb.Append("default " + Helper.ToInt32(defaultValue));
		//						}
		//						break;
		//					case "DECIMAL":
		//						if (defaultValue.Length > 0)
		//						{
		//							sb.Append("default " + Helper.ToDecimal(defaultValue));
		//						}
		//						break;
		//					case "CURRENCY":
		//						if (defaultValue.Length > 0)
		//						{
		//							sb.Append("default " + Helper.ToDecimal(defaultValue));
		//						}
		//						break;
		//					case "BOOLEAN":
		//						if (defaultValue.Length > 0)
		//						{
		//							if (defaultValue.ToUpper() == "TRUE")
		//							{
		//								sb.Append("default 1");
		//							}
		//							else
		//							{
		//								sb.Append("default " + Helper.ToInt32(defaultValue));
		//							}

		//						}
		//						else
		//						{
		//							sb.Append("default 0");
		//						}
		//						break;
		//					case "DATE":
		//						if (isRequired)
		//						{
		//							sb.Append("default getdate() ");
		//						}
		//						break;
		//					case "DATETIME":
		//						if (isRequired)
		//						{
		//							sb.Append("default getdate() ");
		//						}
		//						break;
		//					default:
		//						break;

		//				}

		//				Db.Database.ExecuteSqlCommand(sb.ToString());

		//			}
		//		}

		//		return msg;
		//	}
		//	catch (Exception ex)
		//	{
		//		return "Unable to process adding column.()<br>" + ex.Message;
		//	}


		//}

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


/*  DataType  TEXT  MAXTEXT  NUMBER  DECIMAL  CURRENCY  BOOLEAN  DATE  DATETIME
			{ text: "Text", value: "TEXT" },
			{ text: "Big Text (cannot search)", value: "MAXTEXT" },
			{ text: "Number", value: "NUMBER" },
			{ text: "Decimal", value: "DECIMAL" },
			{ text: "Currency", value: "CURRENCY" },
			{ text: "Boolean", value: "BOOLEAN" },
			{ text: "Date", value: "DATE" },
			{ text: "DateTime", value: "DATETIME" }
 * */
