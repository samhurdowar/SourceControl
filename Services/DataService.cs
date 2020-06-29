using SourceControl.Common;
using SourceControl.Models.Db;
using SourceControl.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using SourceControl.Models.App;

namespace SourceControl.Services
{
    public static class DataService
    {

        public static string GetJsonFromSQL(int dbEntityId, string sql, bool firstOrDefault = false)
        {
            var dbEntity = DataService.DbEntity(dbEntityId);
            using (TargetEntities targetDb = new TargetEntities())
            {
                targetDb.Database.Connection.ConnectionString = dbEntity.ConnectionString;

                sql += " FOR JSON PATH, INCLUDE_NULL_VALUES";
                var items = targetDb.Database.SqlQuery<string>(sql).ToList();
                var json = "";
                foreach (var item in items)
                {
                    json += item;
                }
                if (json.Length > 2 && firstOrDefault && json.Substring(0,1) == "[")
                {
                    json = json.Substring(1, json.Length - 2);
                }

                return json;
            }
        }

        public static List<SysTable> SysTables(int dbEntityId)
        {
            var sql = @"
                    SELECT [DbEntityId] AS DbEntityId, o.name AS TableName, c.name AS PrimaryKey, 
                    CASE
	                    WHEN (system_type_id = 35)  THEN 'TEXT'
	                    WHEN (system_type_id = 36)  THEN 'TEXT'
	                    WHEN (system_type_id = 40)  THEN 'DATE'
	                    WHEN (system_type_id = 41)  THEN 'DATE'
	                    WHEN (system_type_id = 42)  THEN 'DATE'
	                    WHEN (system_type_id = 48)  THEN 'NUMBER'
	                    WHEN (system_type_id = 52)  THEN 'NUMBER'
	                    WHEN (system_type_id = 56)  THEN 'NUMBER'
	                    WHEN (system_type_id = 58)  THEN 'DATE'
	                    WHEN (system_type_id = 59)  THEN 'NUMBER'
	                    WHEN (system_type_id = 60)  THEN 'CURRENCY'
	                    WHEN (system_type_id = 61)  THEN 'DATETIME'
	                    WHEN (system_type_id = 62)  THEN 'NUMBER'
	                    WHEN (system_type_id = 98)  THEN 'TEXT'
	                    WHEN (system_type_id = 99)  THEN 'TEXT'
	                    WHEN (system_type_id = 104) THEN 'BOOLEAN'
	                    WHEN (system_type_id = 106) THEN 'DECIMAL'
	                    WHEN (system_type_id = 108) THEN 'NUMBER'
	                    WHEN (system_type_id = 122) THEN 'CURRENCY'
	                    WHEN (system_type_id = 127) THEN 'NUMBER'
	                    WHEN (system_type_id = 165) THEN 'TEXT'
	                    WHEN (system_type_id = 167) THEN 'TEXT'
	                    WHEN (system_type_id = 173) THEN ''
	                    WHEN (system_type_id = 175) THEN 'TEXT'
	                    WHEN (system_type_id = 189) THEN 'DATE'
	                    WHEN (system_type_id = 231) THEN 'TEXT'
	                    WHEN (system_type_id = 239) THEN 'TEXT'
	                    WHEN (system_type_id = 241) THEN 'TEXT'
                    END
                    AS PrimaryKeyType
                    FROM sys.columns c JOIN sys.objects o ON o.object_id = c.object_id AND o.type = 'U' 
                    LEFT JOIN sys.default_constraints d ON d.object_id = c.default_object_id
                    LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE i ON i.TABLE_NAME = o.name 
                    WHERE i.COLUMN_NAME = c.name
                    ";

            var dbEntity = DataService.DbEntity(dbEntityId);
            using (TargetEntities targetDb = new TargetEntities())
            {
                targetDb.Database.Connection.ConnectionString = dbEntity.ConnectionString;

                var sysTables = targetDb.Database.SqlQuery<SysTable>(sql.Replace("[DbEntityId]", dbEntity.DbEntityId.ToString())).ToList();
                return sysTables;
            }
        }

        public static List<SysColumn> SysColumns(int dbEntityId, string tableName)
        {
            var sql = @"
                    SELECT c.name AS ColumnName, ISNULL(c.column_id,0) AS ColumnOrder, CAST(ISNULL(c.max_length,0) AS int) AS DataLength,
                    CAST(ISNULL(CASE c.is_identity WHEN 1 THEN 1 ELSE 0 END, 0) AS Bit) AS IsIdentity,
                    CAST(ISNULL(CASE c.is_nullable WHEN 1 THEN 0 ELSE 1 END, 0) AS Bit) AS IsRequired,
                    CAST(ISNULL(CASE c.is_computed WHEN 1 THEN 1 ELSE 0 END, 0) AS Bit) AS IsComputed,
                    ISNULL(d.definition,'') AS DefaultValue,
                    CASE
	                    WHEN (system_type_id = 35)  THEN 'TEXT'
	                    WHEN (system_type_id = 36)  THEN 'TEXT'
	                    WHEN (system_type_id = 40)  THEN 'DATE'
	                    WHEN (system_type_id = 41)  THEN 'DATE'
	                    WHEN (system_type_id = 42)  THEN 'DATE'
	                    WHEN (system_type_id = 48)  THEN 'NUMBER'
	                    WHEN (system_type_id = 52)  THEN 'NUMBER'
	                    WHEN (system_type_id = 56)  THEN 'NUMBER'
	                    WHEN (system_type_id = 58)  THEN 'DATE'
	                    WHEN (system_type_id = 59)  THEN 'NUMBER'
	                    WHEN (system_type_id = 60)  THEN 'CURRENCY'
	                    WHEN (system_type_id = 61)  THEN 'DATETIME'
	                    WHEN (system_type_id = 62)  THEN 'NUMBER'
	                    WHEN (system_type_id = 98)  THEN 'TEXT'
	                    WHEN (system_type_id = 99)  THEN 'TEXT'
	                    WHEN (system_type_id = 104) THEN 'BOOLEAN'
	                    WHEN (system_type_id = 106) THEN 'DECIMAL'
	                    WHEN (system_type_id = 108) THEN 'NUMBER'
	                    WHEN (system_type_id = 122) THEN 'CURRENCY'
	                    WHEN (system_type_id = 127) THEN 'NUMBER'
	                    WHEN (system_type_id = 165) THEN 'TEXT'
	                    WHEN (system_type_id = 167) THEN 'TEXT'
	                    WHEN (system_type_id = 173) THEN ''
	                    WHEN (system_type_id = 175) THEN 'TEXT'
	                    WHEN (system_type_id = 189) THEN 'DATE'
	                    WHEN (system_type_id = 231) THEN 'TEXT'
	                    WHEN (system_type_id = 239) THEN 'TEXT'
	                    WHEN (system_type_id = 241) THEN 'TEXT'
                    END
                    AS DataType,
                    CAST(
                        CASE
	                        WHEN (i.COLUMN_NAME = c.name) THEN 1
	                        ELSE 0
                        END
                    AS Bit) 
                    AS IsPrimary,
                    CAST(system_type_id AS int) AS SystemTypeId 
                    FROM sys.columns c JOIN sys.objects o ON o.object_id = c.object_id AND o.type = 'U' AND o.name = @TableName
                    LEFT JOIN sys.default_constraints d ON d.object_id = c.default_object_id
                    LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE i ON i.TABLE_NAME = o.name 
                    ";

            var dbEntity = DataService.DbEntity(dbEntityId);
            using (TargetEntities targetDb = new TargetEntities())
            {
                targetDb.Database.Connection.ConnectionString = dbEntity.ConnectionString;

                var sysColumns = targetDb.Database.SqlQuery<SysColumn>(sql, new SqlParameter("@TableName", tableName)).ToList();
                return sysColumns;
            }
        }

        public static DbEntity DbEntity(int dbEntityId)
        {
            using (SourceControlEntities Db = new SourceControlEntities())
            {
                var dbEntity = Db.DbEntities.Find(dbEntityId);
                return dbEntity;
            }
        }

        public static string GetPasswordModifiedDate()
        {
            try
            {
                using (TargetEntities targetDb = new TargetEntities("NetworkCafeEntities"))
                {

                    var sql = @"SELECT SettingValue FROM SiteSettings WHERE SettingName = 'PasswordModifiedDate'";

                    var rec = targetDb.Database.SqlQuery<string>(sql).FirstOrDefault();

                    return rec;
                }
            }
            catch (Exception ex)
            {
                Helper.LogError(ex);
                return "Unable to process. Error - " + ex.Message;
            }
        }


        public static string GetJsonStringValue(string json, string propertyName)
        {
            try
            {
                string strReturn = JObject.Parse(json)[propertyName].ToString();
                return strReturn;
            }
            catch (Exception ex)
            {
                Helper.LogError(ex);
                return "";
            }

        }

        public static bool GetJsonBooleanValue(string json, string propertyName)
        {
            try
            {
                var str = JObject.Parse(json)[propertyName].ToString();
                bool ret = (str == "true" || str == "1");
                return ret;
            }
            catch (Exception ex)
            {
                Helper.LogError(ex);
                return false;
            }

        }

        public static int GetJsonIntValue(string json, string propertyName)
        {
            try
            {
                int intReturn = Helper.ToInt32(JObject.Parse(json)[propertyName]);
                return intReturn;
            }
            catch (Exception ex)
            {
                Helper.LogError(ex);
                return 0;
            }
        }

        public static string GetStringValue(string sql)
        {
            try
            {
                using (SourceControlEntities Db = new SourceControlEntities())
                {
                    var ret = Db.Database.SqlQuery<string>(sql).FirstOrDefault();
                    return ret;
                }
            }
            catch (Exception ex)
            {
                Helper.LogError(ex);
                return "";
            }
        }

        public static int GetIntValue(string sql)
        {
            try
            {
                using (SourceControlEntities Db = new SourceControlEntities())
                {
                    var ret = Db.Database.SqlQuery<int>(sql).FirstOrDefault();
                    return ret;
                }
            }
            catch (Exception ex)
            {
                Helper.LogError(ex);
                return 0;
            }
        }

        public static string UpdateRecord(int pageTemplateId, string json, string oldJson)
        {
            try
            {
                using (SourceControlEntities Db = new SourceControlEntities())
                {
                    // check for history columns, 
                    var historyDef = Db.ColumnDefs.Where(w => w.ElementType == "ChangeHistory" && w.PageTemplateId == pageTemplateId).FirstOrDefault();
                    if (historyDef != null && oldJson.Length > 3)
                    {

                        // loop through columns and compare newJson and oldJson
                        var pageTemplate = SessionService.PageTemplate(pageTemplateId);
                        StringBuilder sb = new StringBuilder();
                        dynamic newJson = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                        var primaryKey = pageTemplate.PrimaryKey;
                        var recordId = Helper.ToInt32(newJson[pageTemplate.TableName + "_" + primaryKey]);

                        if (recordId > 0)
                        {

                            dynamic origJson = Newtonsoft.Json.JsonConvert.DeserializeObject(oldJson);

                            var columnDefs = SessionService.ColumnDefs(pageTemplateId);

                            foreach (var columnDef in columnDefs.Where(w => !(bool)w.IsComputed))
                            {
                                var formColumnName = pageTemplate.TableName + "_" + columnDef.ColumnName;


                                // set value for column
                                if (columnDef.DataType == "BOOLEAN" && newJson[formColumnName] == null)
                                {
                                    int origVal = Helper.ToDbBoolean(origJson[formColumnName]);
                                    int newVal = Helper.ToDbBoolean(newJson[formColumnName]);

                                    var origVal_ = (origVal == 1) ? "true" : "false";
                                    var newVal_ = (newVal == 1) ? "true" : "false";


                                    if (origVal != newVal)
                                    {
                                        sb.Append("[Changed " + columnDef.ColumnName + " from " + origVal_ + " to " + newVal_ + "] ");
                                    }
                                }

                                if (newJson[formColumnName] != null)
                                {
                                    if ((columnDef.DataType == "TEXT" || columnDef.DataType == "MAXTEXT") && columnDef.ElementType != "MultiSelect")
                                    {
                                        var origVal = Helper.ToSafeString(origJson[formColumnName]);
                                        var newVal = Helper.ToSafeString(newJson[formColumnName]);
                                        if (origVal != newVal)
                                        {
                                            sb.Append("[Changed " + columnDef.ColumnName + " from " + origVal + " to " + newVal + "] ");
                                        }

                                    }
                                    else if (columnDef.DataType == "DATE")
                                    {
                                        string origVal = Helper.ToDbDateTime(origJson[formColumnName]);
                                        string newVal = Helper.ToDbDateTime(newJson[formColumnName]);

                                        if (origVal != newVal)
                                        {
                                            sb.Append("[Changed " + columnDef.ColumnName + " from " + origVal + " to " + newVal + "] ");
                                        }
                                    }
                                    else if (columnDef.DataType == "BOOLEAN")
                                    {
                                        int origVal = Helper.ToDbBoolean(origJson[formColumnName]);
                                        int newVal = Helper.ToDbBoolean(newJson[formColumnName]);
                                        var origVal_ = (origVal == 1) ? "true" : "false";
                                        var newVal_ = (newVal == 1) ? "true" : "false";

                                        if (origVal != newVal)
                                        {
                                            sb.Append("[Changed " + columnDef.ColumnName + " from " + origVal_ + " to " + newVal_ + "] ");
                                        }
                                    }
                                    else if (columnDef.ElementType == "MultiSelect")
                                    {
                                        string origVal = Helper.ToSafeString(origJson[formColumnName]);
                                        string newVal = Helper.ToSafeString(newJson[formColumnName]);
                                        if (origVal != newVal)
                                        {
                                            // lookup values for all ids
                                            var origVal_ = "";
                                            if (origVal.Length > 0)
                                            {
                                                origVal_ = DataService.GetStringValue("SELECT STUFF((SELECT ', ' + " + columnDef.TextField + " FROM (SELECT " + columnDef.TextField + " FROM " + columnDef.LookupTable + " WHERE " + columnDef.ValueField + " IN (" + origVal + ")) AS T FoR XML PATH('')), 1, 1, '')");
                                                origVal_ = origVal_.Replace("<" + columnDef.ColumnName + ">", "").Replace("</" + columnDef.ColumnName + ">", "");
                                            }

                                            var newVal_ = "";
                                            if (newVal.Length > 0)
                                            {
                                                newVal_ = DataService.GetStringValue("SELECT STUFF((SELECT ', ' + " + columnDef.TextField + " FROM (SELECT " + columnDef.TextField + " FROM " + columnDef.LookupTable + " WHERE " + columnDef.ValueField + " IN (" + newVal + ")) AS T FoR XML PATH('')), 1, 1, '')");
                                                newVal_ = newVal_.Replace("<" + columnDef.ColumnName + ">", "").Replace("</" + columnDef.ColumnName + ">", "");

                                            }

                                            // add field_Text to json for update  Manager":null,"AnsibleGroup":"3","ActiveI
                                            var replaceThis = "\"" + columnDef.ColumnName + "\":";
                                            var withThis = "\"" + columnDef.ColumnName + "_Text\":\"" + newVal_ + "\",\"" + columnDef.ColumnName + "\":";
                                            json = json.Replace(replaceThis, withThis);


                                            sb.Append("[Changed " + columnDef.ColumnName + " from " + origVal_ + " to " + newVal_ + "] ");
                                        }
                                    }
                                    else if (columnDef.ElementType == "DropdownCustomOption")
                                    {
                                        var origVal = Helper.ToInt32(origJson[formColumnName]);
                                        var newVal = Helper.ToInt32(newJson[formColumnName]);
                                        if (origVal != newVal)
                                        {

                                            string origVal_ = Helper.ToSafeString(DataService.GetStringValue("SELECT OptionText FROM CustomOption WHERE OptionValue = " + origVal));
                                            string newVal_ = Helper.ToSafeString(DataService.GetStringValue("SELECT OptionText FROM CustomOption WHERE OptionValue = " + newVal));
                                            if (origVal_ != newVal_)
                                            {
                                                if (origVal_.Length == 0) origVal_ = "''";
                                                if (newVal_.Length == 0) newVal_ = "''";
                                                sb.Append("[Changed " + columnDef.ColumnName + " from " + origVal_ + " to " + newVal_ + "] ");
                                            }

                                        }
                                    }
                                    else
                                    {
                                        var origVal = Helper.ToInt32(origJson[formColumnName]);
                                        var newVal = Helper.ToInt32(newJson[formColumnName]);
                                        if (origVal != newVal)
                                        {
                                            if (columnDef.LookupTable.Length > 0 && columnDef.ValueField.Length > 0)
                                            {
                                                var dbFld = columnDef.TextField;
                                                if (columnDef.TextField.Contains(","))
                                                {
                                                    var columns = columnDef.TextField.Split(new char[] { ',' });
                                                    dbFld = columns[0];
                                                }
                                                string origVal_ = Helper.ToSafeString(DataService.GetStringValue("SELECT " + dbFld + " FROM " + columnDef.LookupTable + " WHERE " + columnDef.ValueField + "=" + origVal));
                                                string newVal_ = Helper.ToSafeString(DataService.GetStringValue("SELECT " + dbFld + " FROM " + columnDef.LookupTable + " WHERE " + columnDef.ValueField + "=" + newVal));
                                                if (origVal_ != newVal_)
                                                {
                                                    if (origVal_.Length == 0) origVal_ = "''";
                                                    if (newVal_.Length == 0) newVal_ = "''";
                                                    sb.Append("[Changed " + columnDef.ColumnName + " from " + origVal_ + " to " + newVal_ + "] ");
                                                }
                                            }
                                            else
                                            {
                                                sb.Append("[Changed " + columnDef.ColumnName + " from " + origVal + " to " + newVal + "] ");
                                            }
                                        }
                                    }
                                }
                            }

                            //Changes were made   
                            if (sb.Length > 0)
                            {
                                Db.Database.ExecuteSqlCommand("INSERT INTO ChangeHistory(PageTemplateId,RecordId,UserId,ChangeHistoryText) VALUES(" + pageTemplateId + "," + recordId + "," + SessionService.UserId + ", '" + sb.ToString().Replace("'", "''") + "');");
                            }
                        }
                    }

                    return UpdateRecord(pageTemplateId, json);
                }
            }
            catch (Exception ex)
            {
                Helper.LogError(ex);
                return "Unable to process UpdateRecord() - " + ex.Message;
            }
        }

        public static string UpdateRecordByTable(int dbEntityId, string tableName, string json)
        {
            try
            {
                var recordId = "";
                string updateClause = "";
                StringBuilder sbInsert = new StringBuilder();
                StringBuilder sbValue = new StringBuilder();
                StringBuilder sbUpdate = new StringBuilder();
                sbUpdate.Append("UPDATE " + tableName + " SET ");
                sbInsert.Append("INSERT INTO " + tableName + "(");
                sbValue.Append(" VALUES(");

                dynamic obj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                var sysColumns = DataService.SysColumns(dbEntityId, tableName);
                foreach (var sysColumn in sysColumns.Where(w => !w.IsComputed))
                {
                    if (sysColumn.IsPrimary)
                    {
                        if (obj[sysColumn.ColumnName] != null && obj[sysColumn.ColumnName].ToString() != "" && obj[sysColumn.ColumnName].ToString() != "0") // is update
                        {
                            recordId = obj[sysColumn.ColumnName].ToString();
                            updateClause = (sysColumn.DataType == "TEXT") ? " WHERE " + sysColumn.ColumnName + " = '" + recordId + "'" : " WHERE " + sysColumn.ColumnName + " = " + recordId;
                        } 
                        else
                        {
                            if (sysColumn.DataType == "TEXT")
                            {
                                Guid newGuid = Guid.NewGuid();
                                sbInsert.Append(sysColumn.ColumnName + ",");
                                sbValue.Append("'" + newGuid + "',");
                            }
                        }
                    } 
                    else
                    {
                        if (obj[sysColumn.ColumnName] != null || sysColumn.IsRequired)
                        {
                            sbInsert.Append(sysColumn.ColumnName + ",");
                            
                            if (sysColumn.DataType == "TEXT" || sysColumn.DataType == "MAXTEXT")
                            {
                                sbValue.Append(Helper.ToDbString(obj[sysColumn.ColumnName]) + ",");

                                if (obj[sysColumn.ColumnName] != null) sbUpdate.Append(sysColumn.ColumnName + " = " + Helper.ToDbString(obj[sysColumn.ColumnName]) + ",");
                            }
                            else if (sysColumn.DataType == "DATE")
                            {

                                sbValue.Append(Helper.ToDbDateTime(obj[sysColumn.ColumnName]) + ",");

                                if (obj[sysColumn.ColumnName] != null) sbUpdate.Append(sysColumn.ColumnName + " = " + Helper.ToDbDateTime(obj[sysColumn.ColumnName]) + ", ");
                            }
                            else if (sysColumn.DataType == "BOOLEAN")
                            {
 
                                sbValue.Append(Helper.ToDbBoolean(obj[sysColumn.ColumnName]) + ",");

                                if (obj[sysColumn.ColumnName] != null) sbUpdate.Append(sysColumn.ColumnName + " = " + Helper.ToDbBoolean(obj[sysColumn.ColumnName]) + ",");
                            }
                            else
                            {
                                sbValue.Append(Helper.ToInt32(obj[sysColumn.ColumnName]) + ",");

                                if (obj[sysColumn.ColumnName] != null) sbUpdate.Append(sysColumn.ColumnName + " = " + Helper.ToInt32(obj[sysColumn.ColumnName]) + ",");
                            }
                        }
                    }
                }

                var dbEntity = SessionService.DbEntity(dbEntityId);
                using (TargetEntities targetDb = new TargetEntities())
                {
                    targetDb.Database.Connection.ConnectionString = dbEntity.ConnectionString;

                    if (updateClause.Length > 1)
                    {
                        var sql = sbUpdate.ToString().Substring(0, sbUpdate.ToString().Length - 1) + updateClause;
                        targetDb.Database.ExecuteSqlCommand(sql);
                    }
                    else
                    {
                        var sql = sbInsert.ToString().Substring(0, sbInsert.ToString().Length - 1) + ") " + sbValue.ToString().Substring(0, sbValue.ToString().Length - 1) + "); SELECT CAST(@@IDENTITY AS varchar(250));";

                        recordId = targetDb.Database.SqlQuery<string>(sql).FirstOrDefault();
                        targetDb.SaveChanges();
                    }
                }
                return recordId;

            }
            catch (Exception ex)
            {
                Helper.LogError(ex);
                return "Unable to process UpdateRecord() - " + ex.Message;
            }
        }

        public static string UpdateRecord(int pageTemplateId, string json, bool includePrefix = true)
        {
            try
            {
                var pageTemplate = SessionService.PageTemplate(pageTemplateId);
                var columnDefs = SessionService.ColumnDefs(pageTemplateId);
                var tableName = pageTemplate.TableName;
                var dbEntity = SessionService.DbEntity(pageTemplate.DbEntityId);
                var recordId = "";
                using (TargetEntities Db = new TargetEntities())
                {
                    Db.Database.Connection.ConnectionString = dbEntity.ConnectionString;

                    string wherePrimaryKey = "";
                    string primaryKeyType = "";
                    StringBuilder sbInsert = new StringBuilder();
                    StringBuilder sbValue = new StringBuilder();
                    StringBuilder sbUpdate = new StringBuilder();
                    StringBuilder sbLinkNew = new StringBuilder();
                    sbUpdate.Append("UPDATE " + tableName + " SET ");
                    sbInsert.Append("INSERT INTO " + tableName + "(");
                    sbValue.Append(" VALUES(");
                    string formColumnName = "";
                    Guid newGuid = Guid.NewGuid();

                    dynamic obj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                    foreach (var columnDef in columnDefs.Where(w => !(bool)w.IsComputed))
                    {
                        formColumnName = (includePrefix) ? pageTemplate.TableName + "_" + columnDef.ColumnName : columnDef.ColumnName;


                        if (columnDef.IsPrimary)
                        {
                            primaryKeyType = SessionService.DataType(pageTemplateId, columnDef.ColumnName);

                            if (obj[formColumnName] != null)
                            {
                                recordId = obj[formColumnName].ToString();
                                wherePrimaryKey = " WHERE " + columnDef.ColumnName + " = '" + recordId + "'";
                            }

                            if (primaryKeyType == "TEXT")
                            {
                                sbInsert.Append(columnDef.ColumnName + ",");
                                sbValue.Append("'" + newGuid + "',");
                            }


                            continue;
                        }
                        else if (columnDef.IsEncrypted)
                        {
                            sbInsert.Append(columnDef.ColumnName + ",");
                            sbUpdate.Append(columnDef.ColumnName + " = ");
                            sbValue.Append("'" + Helper.ToEncrypted(obj[formColumnName]).Replace("'", "''") + "',");
                            sbUpdate.Append("'" + Helper.ToEncrypted(obj[formColumnName]).Replace("'", "''") + "',");
                        }
                        else if (columnDef.ColumnName == "ChangeDate" || columnDef.ElementType == "DateChanged")
                        {
                            sbInsert.Append(columnDef.ColumnName + ",");
                            sbValue.Append("getdate(),");
                            sbUpdate.Append(columnDef.ColumnName + " = getdate(),");
                            continue;
                        }
                        else if (columnDef.ColumnName == "ChangeBy")
                        {
                            sbInsert.Append(columnDef.ColumnName + ",");
                            sbValue.Append(SessionService.UserId + ",");
                            sbUpdate.Append(columnDef.ColumnName + " = " + SessionService.UserId + ",");
                            continue;
                        }

                        // set value for column  
                        if (columnDef.ElementType == "Note" || columnDef.ElementType == "FileAttachment")
                        {
                            sbLinkNew.Append("UPDATE " + columnDef.ElementType + " SET RecordId = [RecordId] WHERE RecordId = -" + (columnDef.ColumnDefId + SessionService.UserId) + ";");
                        }
                        else if (columnDef.DataType == "BOOLEAN" && obj[formColumnName] == null)
                        {
                            sbInsert.Append(columnDef.ColumnName + ",");
                            sbUpdate.Append(columnDef.ColumnName + " = ");
                            sbValue.Append("0,");
                            sbUpdate.Append("0,");
                        }
                        else if (obj[formColumnName] != null)
                        {
                            sbInsert.Append(columnDef.ColumnName + ",");
                            sbUpdate.Append(columnDef.ColumnName + " = ");

                            if (columnDef.DataType == "TEXT" || columnDef.DataType == "MAXTEXT")
                            {
                                sbValue.Append("'" + Helper.ToSafeString(obj[formColumnName]).Replace("'", "''") + "',");
                                sbUpdate.Append("'" + Helper.ToSafeString(obj[formColumnName]).Replace("'", "''") + "',");
                            }
                            else if (columnDef.DataType == "DATE")
                            {
                                string dateTime = Helper.ToDbDateTime(obj[formColumnName]);
                                if (dateTime == "null" && (bool)columnDef.IsRequired)
                                {
                                    dateTime = "getdate()";
                                }
                                sbValue.Append(dateTime + ",");
                                sbUpdate.Append(dateTime + ",");
                            }
                            else if (columnDef.DataType == "BOOLEAN")
                            {
                                int val01 = Helper.ToDbBoolean(obj[formColumnName]);
                                sbValue.Append(val01 + ",");
                                sbUpdate.Append(val01 + ",");
                            }
                            else
                            {
                                sbValue.Append(Helper.ToInt32(obj[formColumnName]) + ",");
                                sbUpdate.Append(Helper.ToInt32(obj[formColumnName]) + ",");
                            }
                        }
                    }

                    if (recordId != "0" && recordId.Length > 0 && !recordId.Contains("newid"))
                    {
                        var sql = sbUpdate.ToString().Substring(0, sbUpdate.ToString().Length - 1) + wherePrimaryKey;
                        Db.Database.ExecuteSqlCommand(sql);
                    }
                    else
                    {
                        var sql = sbInsert.ToString().Substring(0, sbInsert.ToString().Length - 1) + ") " + sbValue.ToString().Substring(0, sbValue.ToString().Length - 1) + "); SELECT CAST(@@IDENTITY AS varchar(250));";
                        if (primaryKeyType == "TEXT") // is guid
                        {
                            sql = sbInsert.ToString().Substring(0, sbInsert.ToString().Length - 1) + ") " + sbValue.ToString().Substring(0, sbValue.ToString().Length - 1) + ");";
                            recordId = newGuid.ToString();
                            Db.Database.ExecuteSqlCommand(sql);
                        }
                        else
                        {
                            recordId = Db.Database.SqlQuery<string>(sql).FirstOrDefault();
                        }
                        Db.SaveChanges();

                        //custom code for templatid = 2079
                        if (obj["P2079_PortNum2"] != null)
                        {
                            var portNum1 = Helper.ToInt32(obj["P2079_PortNum"].ToString());
                            var portNum2 = Helper.ToInt32(obj["P2079_PortNum2"].ToString());
                            if (portNum1 > 0 && portNum2 > 0 && portNum2 > portNum1)
                            {
                                sql = sql.Replace(recordId, "[guid]").Replace("'" + portNum1 + "'", "[portNum]");
                                for (int i = portNum1 + 1; i <= portNum2; i++)
                                {
                                    newGuid = Guid.NewGuid();
                                    var exe = sql.Replace("[guid]", newGuid.ToString()).Replace("[portNum]", "'" + i + "'");
                                    Db.Database.ExecuteSqlCommand(exe);
                                    Db.SaveChanges();
                                }
                            }
                        }

                        // update new notes and link to new record.  Need to finish...not coded for guids
                        //if (sbLinkNew.Length > 0) {
                        //	var exe = sbLinkNew.ToString().Replace("[RecordId]", recordId.ToString());
                        //	Db.Database.ExecuteSqlCommand(exe);
                        //}
                    }

                    return recordId.ToString();
                }
            }
            catch (Exception ex)
            {
                Helper.LogError(ex);
                return "Unable to process UpdateRecord() - " + ex.Message;
            }
        }
    }

}