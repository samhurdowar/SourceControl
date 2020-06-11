
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Web.Mvc;
using SourceControl.Models.Db;
using SourceControl.Services;
using SourceControl.Models;

namespace SourceControl.Controllers
{
	public class DataController : Controller
	{

		public string GetMultiSelect(int pageTemplateId, int columnDefId)
		{
			try
			{
				var pageTemplate = SessionService.PageTemplate(pageTemplateId);
				var columnDef = SessionService.ColumnDef(columnDefId);
                var dbEntity = SessionService.DbEntity(pageTemplate.DbEntityId);
                using (TargetEntities Db = new TargetEntities())
				{
                    Db.Database.Connection.ConnectionString = dbEntity.ConnectionString;

                    var columnValue = columnDef.ColumnName;
					var columnText = columnDef.ColumnName;
					var tableName = pageTemplate.TableName;
					var sql = "SELECT DISTINCT ISNULL(CAST(" + columnValue + " AS varchar(500)),'') AS ValueField, ISNULL(CAST(" + columnText + " AS varchar(500)),'') AS TextField FROM  " + tableName + " ORDER BY TextField";

					if (columnDef.ElementType == "DropdownCustomOption")
					{
						sql = "SELECT OptionValue AS ValueField, OptionText AS TextField FROM CustomOption WHERE ColumnDefId = " + columnDefId + " ORDER BY TextField";
					}
					else if (columnDef.ElementType == "DropdownSimple")
					{
						sql = "SELECT ISNULL(CAST(" + columnDef.ValueField + " AS varchar(250)),'') AS ValueField, ISNULL(CAST(" + columnDef.TextField.Replace(",", " + ' - ' + ") + " AS varchar(500)),'') AS TextField FROM " + columnDef.LookupTable + " ORDER BY TextField";
					}

					var recs = Db.Database.SqlQuery<ValueText>(sql).ToList();
					var json = JsonConvert.SerializeObject(recs);
                    return json;
				}
				
			}
			catch (Exception ex)
			{
				return ex.Message;
			}

		}

		[HttpPost]
		public string DeleteRecord(string ids_, int pageTemplateId)
		{
			try
			{
				var pageTemplate = SessionService.PageTemplate(pageTemplateId);
				var dbEntity = SessionService.DbEntity(pageTemplate.DbEntityId);
				using (TargetEntities targetDb = new TargetEntities())
				{
                    targetDb.Database.Connection.ConnectionString = dbEntity.ConnectionString;

                    var exe = "DELETE FROM " + pageTemplate.TableName + " WHERE " + pageTemplate.PrimaryKey + " IN (" + ids_ + ")";
                    targetDb.Database.ExecuteSqlCommand(exe);
                    targetDb.SaveChanges();
				}
				return "";
			}
			catch (Exception ex)
			{
				return ex.Message;
			}

		}

	}
}