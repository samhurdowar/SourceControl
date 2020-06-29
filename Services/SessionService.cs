using SourceControl.Common;
using SourceControl.Models;
using SourceControl.Models.App;
using SourceControl.Models.Db;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace SourceControl.Services
{
    public static class SessionService
    {
        public static bool IsNinja
        {
            get
            {
                if (HttpContext.Current.Session["IsNinja"] == null)
                {
                    if (File.Exists(@"C:\Websites\NinjaLibrary\Web.config"))
                    {
                        HttpContext.Current.Session["IsNinja"] = true;
                    }
                    else
                    {
                        HttpContext.Current.Session["IsNinja"] = false;
                    }
                }

                return (bool)HttpContext.Current.Session["IsNinja"];

            }
        }

        public static bool IsLocal {
            get
            {
                if (HttpContext.Current.Session["IsLocal"] == null)
                {
                    if (File.Exists(@"D:\Perspecta\SourceControl\Web.config"))
                    {
                        HttpContext.Current.Session["IsLocal"] = true;
                    } else
                    {
                        HttpContext.Current.Session["IsLocal"] = false;
                    }
                }
                return (bool)HttpContext.Current.Session["IsLocal"];
            }
        }


		public static List<SysColumn> SysColumns(int dbEntityId, string tableName)
		{
			if (HttpContext.Current.Session["SysColumns" + dbEntityId + tableName] == null)
			{
				var sysColumns = DataService.SysColumns(dbEntityId, tableName);

				HttpContext.Current.Session["SysColumns" + dbEntityId + tableName] = sysColumns;
			}
			return (List<SysColumn>)HttpContext.Current.Session["SysColumns" + dbEntityId + tableName];
		}


		public static List<ColumnDef> ColumnDefs(int pageTemplateId) 
        {
            if (SessionService.IsLocal) HttpContext.Current.Session["sec.ColumnDefs" + pageTemplateId] = null; //xxx

            if (pageTemplateId == 0) return null;

            if (HttpContext.Current.Session["sec.ColumnDefs" + pageTemplateId] == null)
            {
                using (SourceControlEntities Db = new SourceControlEntities())
                {
                    var pageTemplate = Db.PageTemplates.Find(pageTemplateId);

                    var sysColumns = DataService.SysColumns(pageTemplate.DbEntityId, pageTemplate.TableName);
                    var columnDefs_ = Db.ColumnDefs.Where(w => w.PageTemplateId == pageTemplateId);

                    var columnDefs = from a in sysColumns
                               join b in columnDefs_ on a.ColumnName equals b.ColumnName
                               into aa
                               from bb in aa.DefaultIfEmpty()
                               select new ColumnDef {
                                   ColumnDefId = (bb == null) ? 0 : bb.ColumnDefId,
                                   ColumnName = (bb == null) ? a.ColumnName : bb.ColumnName,
                                   DisplayName = (bb == null) ? a.ColumnName : bb.DisplayName,

                                   // ColumnDef
                                   PageTemplateId = pageTemplateId,
                                   OverideValue = (bb == null) ? a.DefaultValue : bb.OverideValue,
                                   ChildTemplateId = (bb == null) ? 0 : bb.ChildTemplateId,
                                   LookupTable = (bb == null) ? "" : bb.LookupTable,
                                   LookupFilter = (bb == null) ? "" : bb.LookupFilter,
                                   ValueField = (bb == null) ? "" : bb.ValueField,
                                   TextField = (bb == null) ? "" : bb.TextField,
                                   OrderField = (bb == null) ? "" : bb.OrderField,
                                   ElementType = (bb == null) ? "Textbox" : bb.ElementType,
                                   ElementWidth = (bb == null) ? 300 : bb.ElementWidth,
                                   ElementHeight = (bb == null) ? 0 : bb.ElementHeight,
                                   ElementObject = (bb == null) ? "" : bb.ElementObject,
                                   ElementDocReady = (bb == null) ? "" : bb.ElementDocReady,
                                   ElementFunction = (bb == null) ? "" : bb.ElementFunction,
                                   ElementLabelLink = (bb == null) ? "" : bb.ElementLabelLink,
                                   AddBlankOption = (bb == null) ? false : bb.AddBlankOption,
                                   DatePickerOption = (bb == null) ? "" : bb.DatePickerOption,
                                   NumberMin = (bb == null) ? 0 : bb.NumberMin,
                                   NumberMax = (bb == null) ? 0 : bb.NumberMax,
                                   NumberOfDecimal = (bb == null) ? 0 : bb.NumberOfDecimal,
                                   ShowInGrid = (bb == null) ? true : bb.ShowInGrid,
                                   GridWidth = (bb == null) ? "" : bb.GridWidth,
                                   IsMultiSelect = (bb == null) ? false : bb.IsMultiSelect,
								   IsEncrypted = (bb == null) ? false : bb.IsEncrypted,

								   // SysColumn
								   ColumnOrder = a.ColumnOrder,
                                   DataLength = a.DataLength,
                                   IsIdentity = a.IsIdentity,
                                   IsRequired = a.IsRequired,
                                   IsComputed = a.IsComputed,
                                   IsPrimary = a.IsPrimary,
                                   DataType = a.DataType,
                                   DefaultValue = (bb == null) ? a.DefaultValue : bb.OverideValue,
                               };

                    HttpContext.Current.Session["sec.ColumnDefs" + pageTemplateId] = columnDefs.ToList();
                }       
            }
            return (List<ColumnDef>)HttpContext.Current.Session["sec.ColumnDefs" + pageTemplateId];
        }

        public static List<ColumnDef> ColumnDefs(int dbEntityId, string tableName)
        {
            if (SessionService.IsLocal) HttpContext.Current.Session["sec.ColumnDefs" + tableName] = null; //xxx

            if (HttpContext.Current.Session["sec.ColumnDefs" + tableName] == null)
            {
                using (SourceControlEntities Db = new SourceControlEntities())
                {
                    var sysColumns = DataService.SysColumns(dbEntityId, tableName);
                    var columnDefs_ = Db.ColumnDefs.Where(w => w.PageTemplateId == -1).ToList();

                    var columnDefs = from a in sysColumns
                                     join b in columnDefs_ on a.ColumnName equals b.ColumnName
                                     into aa
                                     from bb in aa.DefaultIfEmpty()
                                     select new ColumnDef
                                     {
                                         ColumnDefId = (bb == null) ? 0 : bb.ColumnDefId,
                                         ColumnName = (bb == null) ? a.ColumnName : bb.ColumnName,
                                         DisplayName = (bb == null) ? a.ColumnName : bb.DisplayName,

                                         // ColumnDef
                                         PageTemplateId = -1,
                                         OverideValue = (bb == null) ? a.DefaultValue : bb.OverideValue,
                                         ChildTemplateId = (bb == null) ? 0 : bb.ChildTemplateId,
                                         LookupTable = (bb == null) ? "" : bb.LookupTable,
                                         LookupFilter = (bb == null) ? "" : bb.LookupFilter,
                                         ValueField = (bb == null) ? "" : bb.ValueField,
                                         TextField = (bb == null) ? "" : bb.TextField,
                                         OrderField = (bb == null) ? "" : bb.OrderField,
                                         ElementType = (bb == null) ? "Textbox" : bb.ElementType,
                                         ElementWidth = (bb == null) ? 300 : bb.ElementWidth,
                                         ElementHeight = (bb == null) ? 0 : bb.ElementHeight,
                                         ElementObject = (bb == null) ? "" : bb.ElementObject,
                                         ElementDocReady = (bb == null) ? "" : bb.ElementDocReady,
                                         ElementFunction = (bb == null) ? "" : bb.ElementFunction,
                                         ElementLabelLink = (bb == null) ? "" : bb.ElementLabelLink,
                                         AddBlankOption = (bb == null) ? false : bb.AddBlankOption,
                                         DatePickerOption = (bb == null) ? "" : bb.DatePickerOption,
                                         NumberMin = (bb == null) ? 0 : bb.NumberMin,
                                         NumberMax = (bb == null) ? 0 : bb.NumberMax,
                                         NumberOfDecimal = (bb == null) ? 0 : bb.NumberOfDecimal,
                                         ShowInGrid = (bb == null) ? true : bb.ShowInGrid,
                                         GridWidth = (bb == null) ? "" : bb.GridWidth,
                                         IsMultiSelect = (bb == null) ? false : bb.IsMultiSelect,

                                         // SysColumn
                                         ColumnOrder = a.ColumnOrder,
                                         DataLength = a.DataLength,
                                         IsIdentity = a.IsIdentity,
                                         IsRequired = a.IsRequired,
                                         IsComputed = a.IsComputed,
                                         IsPrimary = a.IsPrimary,
                                         DataType = a.DataType,
                                         DefaultValue = (bb == null) ? a.DefaultValue : bb.OverideValue,
                                     };

                    HttpContext.Current.Session["sec.ColumnDefs" + tableName] = columnDefs.ToList();
                }
            }
            return (List<ColumnDef>)HttpContext.Current.Session["sec.ColumnDefs" + tableName];
        }

        public static ColumnDef ColumnDef(int columnDefId)
        {
            if (SessionService.IsLocal) HttpContext.Current.Session["sec.ColumnDefId" + columnDefId] = null; //xxx
            if (HttpContext.Current.Session["sec.ColumnDefId" + columnDefId] == null)
            {
                var pageTemplateId = DataService.GetIntValue("SELECT PageTemplateId FROM ColumnDef WHERE ColumnDefId = " + columnDefId );
                var columnDef = SessionService.ColumnDefs(pageTemplateId).Where(w => w.ColumnDefId == columnDefId).FirstOrDefault();

                HttpContext.Current.Session["sec.ColumnDefId" + columnDefId] = columnDef;

            }
            return (ColumnDef)HttpContext.Current.Session["sec.ColumnDefId" + columnDefId];
        }

        public static ColumnDef ColumnDef(int pageTemplateId, string columnName)
        {
            if (pageTemplateId == 0) return null;
            ColumnDef columnDef = SessionService.ColumnDefs(pageTemplateId).Where(w => w.ColumnName == columnName).FirstOrDefault();
            return columnDef;
        }

        public static List<PageTemplate> PageTemplates(int dbEntityId)
        {
            HttpContext.Current.Session["sec.PageTemplates" + dbEntityId] = null; //xxx

            if (HttpContext.Current.Session["sec.PageTemplates" + dbEntityId] == null)
            {
                using (SourceControlEntities Db = new SourceControlEntities())
                {
                    var sysTables = DataService.SysTables(dbEntityId);
                    var pageTemplates_ = Db.PageTemplates.Where(w => w.DbEntityId == dbEntityId).ToList();

                    var pageTemplates = from a in pageTemplates_  
                                        join b in sysTables on a.TableName equals b.TableName
                                     into aa
                                     from bb in aa.DefaultIfEmpty()
										select new PageTemplate {
											PageTemplateId = a.PageTemplateId,
											DbEntityId = a.DbEntityId,
											TemplateName = a.TemplateName,
											TableName = a.TableName,
											GridColumns = a.GridColumns,
											SortColumns = a.SortColumns,
											AddTabLabel = a.AddTabLabel,
											ViewTabLabel = a.ViewTabLabel,
											EditTabLabel = a.EditTabLabel,
											EditLayout = a.EditLayout,
											ViewLayout = a.ViewLayout,
											PageType = a.PageType,
											FilterClause = a.FilterClause,
											HelpId = a.HelpId,
											OnLoadScript = a.OnLoadScript,
											BodyScript = a.BodyScript,
											OnSaveScript = a.OnSaveScript,
											PageTemplateId2 = a.PageTemplateId2,
											RefKey2 = a.RefKey2,
											PrimaryKey2 = a.PrimaryKey2,
											GridStyle = a.GridStyle,
											GridCommand = a.GridCommand,
											GridBody = Helper.HTMLEncode(a.GridBody.Replace("[PageTemplateId]", a.PageTemplateId.ToString())),
											GridOnDataBound = Helper.HTMLEncode(a.GridOnDataBound.Replace("[PageTemplateId]", a.PageTemplateId.ToString())),
											GridScript = Helper.HTMLEncode(a.GridScript.Replace("[PageTemplateId]", a.PageTemplateId.ToString())),
											CustomEditForm = a.CustomEditForm,
											GroupByColumns = a.GroupByColumns,
											GridAutoBind = a.GridAutoBind,
											RefKey2Name = a.RefKey2Name,
											PrimaryKey2Name = a.PrimaryKey2Name,
											SearchLayout = a.SearchLayout,
											ViewFormStyle = a.ViewFormStyle,
											ViewFormCommand = a.ViewFormCommand,
											ViewFormBody = a.ViewFormBody,
											ViewFormScript = a.ViewFormScript,
											EditFormStyle = a.EditFormStyle,
											EditFormCommand = a.EditFormCommand,
											EditFormBody = a.EditFormBody,
											EditFormScript = a.EditFormScript,
											SearchFormStyle = a.SearchFormStyle,
											SearchFormCommand = a.SearchFormCommand,
											SearchFormBody = a.SearchFormBody,
											SearchFormScript = a.SearchFormScript,
											ReportGridColumns = a.ReportGridColumns,
											FormCommand = a.FormCommand,
											FormScript = a.FormScript,

											// from SysTable
											PrimaryKey = (bb == null) ? "" : bb.PrimaryKey,
											PrimaryKeyType = (bb == null) ? "" : bb.PrimaryKeyType

										};




                    HttpContext.Current.Session["sec.PageTemplates" + dbEntityId] = pageTemplates.ToList();
                }

            }
            return (List<PageTemplate>)(HttpContext.Current.Session["sec.PageTemplates" + dbEntityId]);
        }


        public static PageTemplate PageTemplate(int pageTemplateId)
        {
            if (pageTemplateId == 0) return null;

            HttpContext.Current.Session["sec.PageTemplate" + pageTemplateId] = null; //xxx
            if (HttpContext.Current.Session["sec.PageTemplate" + pageTemplateId] == null)
            {
                using (SourceControlEntities Db = new SourceControlEntities())
                {
					var pageTemplate = Db.PageTemplates.Find(pageTemplateId);
                    var pageTemplates = SessionService.PageTemplates(pageTemplate.DbEntityId);
					var pageTemplate_ = pageTemplates.Where(w => w.PageTemplateId == pageTemplateId).FirstOrDefault();

					HttpContext.Current.Session["sec.PageTemplate" + pageTemplateId] = pageTemplate_;
                }
            }
            return (PageTemplate)(HttpContext.Current.Session["sec.PageTemplate" + pageTemplateId]);
        }


        public static string VirtualDomain
		{
			get
			{
				return "";  // /cmsdb
			}
		}

		public static AppUser CurrentUser
		{
			get
			{
				if (HttpContext.Current.Session["sec.CurrentUser"] == null)
				{
					return null;
				}
				return (AppUser)(HttpContext.Current.Session["sec.CurrentUser"]);
			}
		}

        public static bool EnableDebugLog
        {
            get
            {
                try
                {
                    if (HttpContext.Current.Session["sec.EnableDebugLog"] == null)
                    {
                        HttpContext.Current.Session["sec.EnableDebugLog"] = ConfigurationManager.AppSettings["EnableDebugLog"];

                    }
                    return (HttpContext.Current.Session["sec.EnableDebugLog"].ToString() == "true" ? true : false);
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public static int UserId
		{
			get
			{
				return SessionService.CurrentUser.UserId;
			}
		}

		public static void ResetPageSession(int pageTemplateId)
		{
			HttpContext.Current.Session["sec.TableName" + pageTemplateId] = null;
			HttpContext.Current.Session["sec.ColumnDef" + pageTemplateId] = null;
			HttpContext.Current.Session["sec.ColumnDefView" + pageTemplateId] = null;
			HttpContext.Current.Session["sec.PageTemplate" + pageTemplateId] = null;
			HttpContext.Current.Session["sec.TableFilterSort" + pageTemplateId] = null;
			HttpContext.Current.Session["sec.GridColumns" + pageTemplateId] = null;
			HttpContext.Current.Session["sec.FormLayout" + pageTemplateId] = null;
		}

		public static string TableName(int pageTemplateId)
		{
			if (pageTemplateId == 0) return "";
			if (HttpContext.Current.Session["sec.TableName" + pageTemplateId] == null)
			{
                var pageTemplate = SessionService.PageTemplate(pageTemplateId);
                HttpContext.Current.Session["sec.TableName" + pageTemplateId] = pageTemplate.TableName;
            }
			return (HttpContext.Current.Session["sec.TableName" + pageTemplateId].ToString());
		}

		public static string NetworkToolboxEmailAddress()
		{
			try
			{
				if (HttpContext.Current.Session["sec.NetworkToolboxEmailAddress"] == null)
				{
					HttpContext.Current.Session["sec.NetworkToolboxEmailAddress"] = ConfigurationManager.AppSettings["NetworkToolboxEmailAddress"];
				}
				return (HttpContext.Current.Session["sec.NetworkToolboxEmailAddress"].ToString());
			}
			catch (Exception)
			{
				return "cmsnetworkeng@uspsector.com";
			}
		}

        public static string EmailFromAddress()
        {
            try
            {
                if (HttpContext.Current.Session["sec.EmailFromAddress"] == null)
                {
                    HttpContext.Current.Session["sec.EmailFromAddress"] = ConfigurationManager.AppSettings["EmailFromAddress"];
                }
                return (HttpContext.Current.Session["sec.EmailFromAddress"].ToString());
            }
            catch (Exception)
            {
                return "no-reply-cmsnetworktoolbox@entsvcscms.com";
            }
        }

        public static string ColumnName(int columnDefId)
		{
            if (SessionService.IsLocal) HttpContext.Current.Session["sec.ColumnName" + columnDefId] = null; //xxx

            if (columnDefId == 0) return "";
			if (HttpContext.Current.Session["sec.ColumnName" + columnDefId] == null)
			{
                var columnDef = SessionService.ColumnDef(columnDefId);

				HttpContext.Current.Session["sec.ColumnName" + columnDefId] = columnDef != null ? columnDef.ColumnName : "";

			}
			return (HttpContext.Current.Session["sec.ColumnName" + columnDefId].ToString());
		}

		public static int ColumnDefId(int pageTemplateId, string columnName)
		{
			if (pageTemplateId == 0 || columnName.Length == 0) return 0;
			if (HttpContext.Current.Session["sec.ColumnDefId" + pageTemplateId + columnName] == null)
			{
				var obj = DataService.GetIntValue("SELECT ColumnDefId FROM ColumnDef WHERE PageTemplateId = " + pageTemplateId + " AND ColumnName = '" + columnName + "'");
				HttpContext.Current.Session["sec.ColumnDefId" + pageTemplateId + columnName] = Helper.ToInt32(obj);

			}
			return Helper.ToInt32(HttpContext.Current.Session["sec.ColumnDefId" + pageTemplateId + columnName]);
		}

		public static string PrimaryKey(int pageTemplateId)
		{
			if (pageTemplateId == 0) return "";
			if (HttpContext.Current.Session["sec.PrimaryKey" + pageTemplateId] == null)
			{
                var pageTemplate = SessionService.PageTemplate(pageTemplateId);

				HttpContext.Current.Session["sec.PrimaryKey" + pageTemplateId] = pageTemplate.PrimaryKey;
			}
			return (HttpContext.Current.Session["sec.PrimaryKey" + pageTemplateId].ToString());
		}

		public static string PrimaryKey(int dbEntityId, string tableName)
		{

			if (HttpContext.Current.Session["sec.PrimaryKey" + tableName] == null)
			{
                var sysTable = DataService.SysTables(dbEntityId).Where(w => w.TableName == tableName).FirstOrDefault();
                
				HttpContext.Current.Session["sec.PrimaryKey" + tableName] = sysTable != null ? sysTable.PrimaryKey : "";
			}
			return (HttpContext.Current.Session["sec.PrimaryKey" + tableName].ToString());
		}

		public static string DataType(int pageTemplateId, string columnName)
		{

			if (HttpContext.Current.Session["sec.DataType" + pageTemplateId + columnName] == null)
			{
                var columnDef = SessionService.ColumnDef(pageTemplateId, columnName);
				HttpContext.Current.Session["sec.DataType" + pageTemplateId + columnName] = columnDef != null ? columnDef.DataType.ToString() : "";
			}
			return (HttpContext.Current.Session["sec.DataType" + pageTemplateId + columnName].ToString());
		}

		public static int DataLength(string tableName, string columnName)
		{
			try
			{
				if (HttpContext.Current.Session["sec.DataLength" + tableName + columnName] == null)
				{
					var obj = DataService.GetIntValue("SELECT [dbo].[GetDataLengthByName]('" + tableName + "', '" + columnName + "')");
					HttpContext.Current.Session["sec.DataLength" + tableName + columnName] = Convert.ToInt32(obj);
				}

			}
			catch (Exception)
			{
				HttpContext.Current.Session["sec.DataLength" + tableName + columnName] = 0;
			}
			return (Convert.ToInt32(HttpContext.Current.Session["sec.DataLength" + tableName + columnName]));
		}

		public static DbEntity DbEntity(int dbEntityId)
		{
			if (HttpContext.Current.Session["sec.DbEntity" + dbEntityId] == null)
			{
				using (SourceControlEntities Db = new SourceControlEntities())
				{
					var dbEntity = Db.Database.SqlQuery<DbEntity>("SELECT * FROM DbEntity WHERE DbEntityId = " + dbEntityId).FirstOrDefault();
					HttpContext.Current.Session["sec.DbEntity" + dbEntityId] = dbEntity;
				}
			}
			return (DbEntity)(HttpContext.Current.Session["sec.DbEntity" + dbEntityId]);
		}

		public static TableFilterSort TableFilterSort(int pageTemplateId)
		{
            if (SessionService.IsLocal) HttpContext.Current.Session["sec.TableFilterSort" + pageTemplateId] = null;  //xxx

            if (HttpContext.Current.Session["sec.TableFilterSort" + pageTemplateId] == null)
			{

				StringBuilder GridColumns = new StringBuilder();
				StringBuilder SortColumns = new StringBuilder();
				StringBuilder InnerSelect = new StringBuilder();
				StringBuilder OuterSelect = new StringBuilder();
				StringBuilder InnerJoin = new StringBuilder();
				StringBuilder StandardSelect = new StringBuilder();

				Dictionary<string, string> FilterMap = new Dictionary<string, string>();
				Dictionary<string, string> Sort1Map = new Dictionary<string, string>();
				Dictionary<string, string> Sort2Map = new Dictionary<string, string>();

				TableFilterSort tfs = new TableFilterSort();

				var tableName = TableName(pageTemplateId);
				if (tableName.Length == 0) return tfs;
				var primaryKey = PrimaryKey(pageTemplateId);


				GridColumns.Append(tableName + "." + primaryKey);
				InnerSelect.Append(tableName + "." + primaryKey);
				OuterSelect.Append(tableName + "." + primaryKey);

				var columnDefs = SessionService.ColumnDefs(pageTemplateId);

				var referenceIndex = 0;
				using (SourceControlEntities Db = new SourceControlEntities())
				{

					var pageTemplate = SessionService.PageTemplate(pageTemplateId);

					// get GridColumns   
					var gridColumns = Db.GridColumns.Where(w => w.PageTemplateId == pageTemplateId).OrderBy(o => o.SortOrder);
					int[] columnDefIds = gridColumns.Select(s => s.ColumnDefId).ToArray();

					var gridColumnDefs = columnDefs.Where(w => columnDefIds.Contains(w.ColumnDefId) && !(bool)w.IsPrimary);
					foreach (var columnDef in gridColumnDefs)
					{
						referenceIndex++;

						if (columnDef.ElementType == "DropdownCustomOption") 
						{

							FilterMap.Add(columnDef.ColumnName + "_lco", "CustomOption" + referenceIndex + ".OptionText");

							GridColumns.Append(string.Format(",{0}_lco", columnDef.ColumnName));

							InnerSelect.Append(string.Format(", {0} AS {1}_lco ", "CustomOption" + referenceIndex + ".OptionText", columnDef.ColumnName));
							InnerJoin.Append(string.Format(" LEFT JOIN {0} ON {1}.{2} = {3}.{4} AND {5}.ColumnDefId = {6}", "CustomOption CustomOption" + referenceIndex, tableName, columnDef.ColumnName, "CustomOption" + referenceIndex, "OptionValue", "CustomOption" + referenceIndex, columnDef.ColumnDefId));

							OuterSelect.Append(string.Format(",MAIN.{0}_lco", columnDef.ColumnName));

							// set sort
							Sort1Map.Add(columnDef.ColumnName + "_lco", "CustomOption" + referenceIndex + ".OptionText");
							Sort2Map.Add(columnDef.ColumnName + "_lco", "MAIN." + columnDef.ColumnName + "_lco");
						}
						else if(columnDef.LookupTable.Length > 0 && columnDef.TextField.Length > 0 && columnDef.ValueField.Length > 0 && columnDef.ElementType == "DropdownSimple") 
						{
							string lookUpField = "";
							string lookUpField_ = "";
							string fieldOnly = "";
							if (columnDef.TextField.Contains(","))
							{
								string[] fields = columnDef.TextField.Split(new char[] { ',' });
								lookUpField = "ISNULL(" + columnDef.LookupTable + "." + fields[0] + ",'') ";
								lookUpField_ = columnDef.LookupTable + "." + fields[0];
								fieldOnly = fields[0];
							}
							else
							{
								lookUpField = "ISNULL(" + columnDef.LookupTable + "." + columnDef.TextField + ",'') ";
								lookUpField_ = columnDef.LookupTable + "." + columnDef.TextField;
								fieldOnly = columnDef.TextField;
							}

							var filterCondition = "CAST(" + tableName + "." + columnDef.ColumnName + " AS varchar(250)) IN (SELECT " + columnDef.ValueField + " FROM " + columnDef.LookupTable + " WHERE " + lookUpField_ + " [PARAM]) ";
							FilterMap.Add(columnDef.ColumnName + "_tbl", filterCondition);

							GridColumns.Append(string.Format(",{0}_tbl", columnDef.ColumnName));


							InnerSelect.Append(string.Format(", {0} AS {1}_tbl, {2}.{3} ", lookUpField, columnDef.ColumnName, tableName, columnDef.ColumnName));
							InnerJoin.Append(string.Format(" LEFT JOIN {0} ON {1}.{2} = {3}.{4} ", columnDef.LookupTable, tableName, columnDef.ColumnName, columnDef.LookupTable, columnDef.ValueField));

							OuterSelect.Append(string.Format(",MAIN.{0}_tbl, MAIN.{1}", columnDef.ColumnName, columnDef.ColumnName));


							// set sort
							Sort1Map.Add(columnDef.ColumnName + "_tbl", columnDef.LookupTable + "." + fieldOnly);
							Sort2Map.Add(columnDef.ColumnName + "_tbl", "MAIN." + columnDef.ColumnName + "_tbl");
						}
						else
						{
							GridColumns.Append("," + columnDef.ColumnName);

							//OuterSelect.Append(string.Format(",{0}.{1}", tableName, columnDef.ColumnName));

							if (columnDef.DataType == "DATE")
							{
								OuterSelect.Append(string.Format(",{0}.{1}", tableName, columnDef.ColumnName));
								//OuterSelect.Append(",ISNULL(" + tableName + "." + columnDef.ColumnName + ",null) AS " + columnDef.ColumnName);
							}
							else if (columnDef.DataType == "DATETIME")
							{
								OuterSelect.Append(string.Format(",{0}.{1}", tableName, columnDef.ColumnName));
								//OuterSelect.Append(",ISNULL(" + tableName + "." + columnDef.ColumnName + ",null) AS " + columnDef.ColumnName);
							}
							else
							{
								OuterSelect.Append(string.Format(",{0}.{1}", tableName, columnDef.ColumnName));
							}

						}
					}

					// get SortColumns
					var sortColumns = Db.SortColumns.Where(w => w.PageTemplateId == pageTemplateId).OrderBy(o => o.SortOrder);
					
					foreach (var sortColumn in sortColumns)
					{
						var columnDef = columnDefs.Where(w => w.ColumnDefId == sortColumn.ColumnDefId).FirstOrDefault();

						string ascDesc = sortColumn.SortDir;
						if (columnDef.LookupTable.Length > 0 && columnDef.TextField.Length > 0 && columnDef.ValueField.Length > 0)
						{
							if (SortColumns.Length == 0)
							{
								SortColumns.Append(string.Format("{0}_ {1}", tableName + "." + columnDef.ColumnName, ascDesc));
							}
							else
							{
								SortColumns.Append(string.Format(", {0}_ {1}", tableName + "." + columnDef.ColumnName, ascDesc));
							}

							if (!InnerJoin.ToString().Contains("LEFT JOIN " + columnDef.LookupTable))
							{
								InnerJoin.Append(string.Format(" LEFT JOIN {0} ON {1}.{2} = {3}.{4} ", columnDef.LookupTable, tableName, columnDef.ColumnName, columnDef.LookupTable, columnDef.ValueField));
							}
						}
						else
						{
							if (SortColumns.Length == 0)
							{
								SortColumns.Append(string.Format("{0} {1}", tableName + "." + columnDef.ColumnName, ascDesc));
							}
							else
							{
								SortColumns.Append(string.Format(", {0} {1}", tableName + "." + columnDef.ColumnName, ascDesc));
							}

						}
					}
				}


				tfs.GridColumns = GridColumns.ToString();
				tfs.SortColumns = SortColumns.ToString(); ;
				tfs.InnerSelect = InnerSelect.ToString();
				tfs.OuterSelect = OuterSelect.ToString();
				tfs.InnerJoin = InnerJoin.ToString();
				tfs.FilterMap = FilterMap;
				tfs.Sort1Map = Sort1Map;
				tfs.Sort2Map = Sort2Map;

				HttpContext.Current.Session["sec.TableFilterSort" + pageTemplateId] = tfs;

			}
			return (TableFilterSort)(HttpContext.Current.Session["sec.TableFilterSort" + pageTemplateId]);

		}

		public static string GridScripts(int pageTemplateId)
		{
			if (pageTemplateId == 0) return "";
			GridSchemaColumns GridSchemaColumns = PageService.GetGridSchemaAndColumn(pageTemplateId);
			return GridSchemaColumns.GridScripts;
		}
		
		public static string GridColumns(int pageTemplateId)
		{
			if (pageTemplateId == 0) return "";
			GridSchemaColumns GridSchemaColumns = PageService.GetGridSchemaAndColumn(pageTemplateId);
			return GridSchemaColumns.GridColumns;
		}

		public static string GridSchema(int pageTemplateId)
		{
			if (pageTemplateId == 0) return "";
			GridSchemaColumns GridSchemaColumns = PageService.GetGridSchemaAndColumn(pageTemplateId);
			return GridSchemaColumns.GridSchema;
		}

        public static string FormLayout(int pageTemplateId, string layoutType)
        {
            HttpContext.Current.Session["sec.FormLayout" + pageTemplateId] = null; //xxx

            if (HttpContext.Current.Session["sec.FormLayout" + pageTemplateId] == null)
            {
                var formLayout = FormLayoutService.GetFormLayout(pageTemplateId, layoutType);
                formLayout = formLayout.Replace("&gt;", ">").Replace("&lt;", "<");
                HttpContext.Current.Session["sec.FormLayout" + pageTemplateId] = formLayout;
            }

            var resp = HttpContext.Current.Session["sec.FormLayout" + pageTemplateId].ToString();
            return resp;

        }

        public static void ClearPageTemplateSessions(int pageTemplateId)
		{
			HttpContext.Current.Session["sec.GridColumns" + pageTemplateId] = null;
			HttpContext.Current.Session["sec.GridSchema" + pageTemplateId] = null;
			HttpContext.Current.Session["sec.PageTemplate" + pageTemplateId] = null;
			HttpContext.Current.Session["sec.FormLayout" + pageTemplateId] = null;
			HttpContext.Current.Session["sec.TableFilterSort" + pageTemplateId] = null;
            HttpContext.Current.Session["sec.ColumnDefs" + pageTemplateId] = null;

			var items = SessionService.ColumnDefs(pageTemplateId);
            foreach (var item in items)
            {
				HttpContext.Current.Session["sec.ColumnDefId" + item.ColumnDefId] = null;
			}
		}

		public static SiteSettingModel SiteSettingModel 
		{
			get
			{
				SiteSettingModel siteSettingModel = new SiteSettingModel();
				if (HttpContext.Current.Session["sec.SiteSettingModel"] == null)
				{
					using (SourceControlEntities Db = new SourceControlEntities())
					{
						siteSettingModel = Db.Database.SqlQuery<SiteSettingModel>("SELECT TOP 1 * FROM WebSiteSetting").FirstOrDefault();
					}

					HttpContext.Current.Session["sec.SiteSettingModel"] = siteSettingModel;
				}
				return (SiteSettingModel)HttpContext.Current.Session["sec.SiteSettingModel"];
			}

		}

        public static string GetSiteSettingValue(string dbName, string settingName)
        {

            if (dbName.ToLower() == "networkcafe")
            {
                using (AppNetworkCafeEntities Db = new AppNetworkCafeEntities())
                {
                    var settingValue = Db.Database.SqlQuery<string>("SELECT TOP 1 SettingValue FROM SiteSettings WHERE SettingName = '" + settingName + "'").FirstOrDefault();
                    return settingValue;
                }
            } else if (dbName.ToLower() == "pmm")
            {
                using (AppPMMEntities Db = new AppPMMEntities())
                {
                    var settingValue = Db.Database.SqlQuery<string>("SELECT TOP 1 SettingValue FROM SiteSettings WHERE SettingName = '" + settingName + "'").FirstOrDefault();
                    return settingValue;
                }
            }

            return "";
        }

    }
}