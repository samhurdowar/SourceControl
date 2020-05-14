using SourceControl.Models.Db;
using SourceControl.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using SourceControl.Services;

namespace SourceControl.Common
{

    public static class Helper
    {
        public static string ClearError()
        {
            try
            {
                using (SourceControlEntities Db = new SourceControlEntities())
                {
                    Db.Database.ExecuteSqlCommand("TRUNCATE TABLE DebugLog");
                    Db.SaveChanges();
                    return "";
                }
            }
            catch (Exception ex)
            {
                return ex.StackTrace + ex.Message;
            }
        }



        public static List<DebugLog> GetDebugLog()
        {
            using (SourceControlEntities db = new SourceControlEntities())
            {
                var recs = db.DebugLogs.OrderByDescending(o => o.LogDate).Take(10).ToList();
                return recs;

            }
        }


        public static void LogError(Exception ex)
        {
            try
            {
                Helper.LogError(ex.TargetSite.ReflectedType.FullName + "\r\n" + ex.Message + "\r\n" + ex.StackTrace);
            }
            catch (Exception)
            {
            }
        }


        public static void LogError(string logContent)
        {
            //if (!SessionService.EnableDebugLog) return;

            try
            {
                using (SourceControlEntities Db = new SourceControlEntities())
                {
                    string hostName = System.Net.Dns.GetHostName();
                    DebugLog log = new DebugLog();
                    log.Source = hostName;
                    log.LogContent = logContent;
                    log.LogDate = DateTime.Now;
                    Db.DebugLogs.Add(log);
                    Db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace + ex.Message;
            }
        }

        public static string GetPrimaryKey(string tableName)
        {

            using (SourceControlEntities Db = new SourceControlEntities())
            {
                var columnName = Db.Database.SqlQuery<string>("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_SCHEMA + '.' + CONSTRAINT_NAME), 'IsPrimaryKey') = 1 AND TABLE_NAME = '" + tableName + "'").FirstOrDefault();
                if (columnName != null)
                {
                    return columnName;
                }

            }
            return "";
        }


        public static string GetPrimaryKey(int pageTemplateId, string tableName)
        {
            var pageTemplate = SessionService.PageTemplate(pageTemplateId);
            var dbEntity = SessionService.DbEntity(pageTemplate.DbEntityId);
            using (TargetEntities Db = new TargetEntities())
            {
                Db.Database.Connection.ConnectionString = dbEntity.ConnectionString;
                var columnName = Db.Database.SqlQuery<string>("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_SCHEMA + '.' + CONSTRAINT_NAME), 'IsPrimaryKey') = 1 AND TABLE_NAME = '" + tableName + "'").FirstOrDefault();
                if (columnName != null)
                {
                    return columnName;
                }

            }
            return "";
        }


        public static string GetDBDataType(string dataType)
        {
            string dbType = "varchar";
            switch (dataType)
            {
                case "TEXT":
                    dbType = "varchar";
                    break;
                case "MAXTEXT":
                    dbType = "varchar";
                    break;
                case "NUMBER":
                    dbType = "int";
                    break;
                case "DECIMAL":
                    dbType = "decimal";
                    break;
                case "CURRENCY":
                    dbType = "money";
                    break;
                case "BOOLEAN":
                    dbType = "bit";
                    break;
                case "DATE":
                    dbType = "date";
                    break;
                case "DATETIME":
                    dbType = "datetime";
                    break;
                default:
                    break;

            }
            return dbType;
        }


        public static string GetCurrentPath(string path)
        {
            if (path.Contains("\\"))
            {
                string[] words = path.Split(new char[] { '\\' });
                return words[words.Length - 1];
            }
            return path;
        }


        public static object GetvalueParameter(string dataType, string val)
        {
            if (dataType == "text")
            {
                return val.ToString();
                //return "'" + val.Replace("'", "''") + "'";
            }
            else if (dataType == "bool")
            {
                if (val == "1" || val == "true")
                {
                    return true;
                }
                return false;
            }
            else if (dataType == "date")
            {
                DateTime dt = new DateTime();
                if (DateTime.TryParse(val, out dt))
                {
                    return dt;
                }
                return null;
            }
            else
            {

                return ToInt32(val);
            }

        }


        public static int ToInt32(object val)
        {
            try
            {
                int num;
                if (Int32.TryParse(val.ToString(), out num))
                {
                    return num;
                }
            }
            catch (Exception)
            {
                return 0;
            }

            return 0;
        }

        public static decimal ToDecimal(object val)
        {
            try
            {
                decimal num;
                if (decimal.TryParse(val.ToString(), out num))
                {
                    return num;
                }
            }
            catch (Exception)
            {
                return 0;
            }
            return 0;
        }

        public static string ToDbDateTime(object val)
        {
            try
            {
                DateTime dt = new DateTime();

                string dateTime = val.ToString();
                if (dateTime.Contains("GMT"))
                {
                    var ray = dateTime.Split(new string[] { "GMT" }, StringSplitOptions.None);
                    dateTime = ray[0];
                }

                if (DateTime.TryParse(dateTime, out dt))
                {
                    return "'" + String.Format("{0:G}", dt) + "'";
                } 
                else
                {
                    return "getdate()";
                }
            }
            catch (Exception)
            {
                return "null";
            }
        }

        public static DateTime ToDateTime(object val)
        {
            try
            {
                DateTime dt = new DateTime();
                if (DateTime.TryParse(val.ToString(), out dt))
                {
                    return dt;
                }
                return Convert.ToDateTime("1/1/9999");
            }
            catch (Exception)
            {
                return Convert.ToDateTime("1/1/9999");
            }
        }

        public static int ToDbBoolean(object val)
        {
            try
            {
                bool b = new bool();
                if (Boolean.TryParse(val.ToString(), out b))
                {
                    return b ? 1 : 0;
                }

                if (val.ToString().ToLower() == "true" || val.ToString().ToLower() == "1")
                {
                    return 1;
                }

                return 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public static string ToSafeString(object val)
        {
            try
            {
                return val.ToString();
            }
            catch (Exception)
            {
                return "";
            }
        }



        public static string ToDbString(object val)
        {
            try
            {
                return "'" + val.ToString().Replace("'", "''") + "'";
            }
            catch (Exception)
            {
                return "''";
            }
        }


        public static object FirstOrDefault(IQueryable source)
        {
            if (source == null) return null;
            return source.Provider.Execute(
                 Expression.Call(
                      typeof(Queryable), "FirstOrDefault ",
                      new Type[] { source.ElementType },
                      source.Expression));
        }

        public static int IsSystem(string columnName)
        {
            string systemColumns = ":ADDBY:ADDEDBY:ADDDATE:ADDEDDATE:CHANGEBY:CHANGEDATE:COMPANYID:";
            if (systemColumns.Contains(":" + columnName.ToUpper() + ":"))
            {
                return 1;
            }
            return 0;
        }


        public static string HTMLEncode(string str)
        {
            try
            {
                return str.Replace("[GT]", ">").Replace("[LT]", "<");
            }
            catch (Exception)
            {
                return "";
            }

        }
    }


    //function LoadAccount() {  
    //	$.ajax({
    //		url: "/Common/LoadAccount",
    //		type: "POST",
    //		dataType: "text",
    //		success: function (data) {
    //			alert("Loaded!!!");
    //		},
    //		error: function (x, y, z) {
    //			alert("Error deleting record.");
    //		}
    //	});
    //}
    //<a href="javascript:LoadAccount()">load account</a>

    public static class Loader
    {
        public static void LoadAccount()
        {


            using (SourceControlEntities Db = new SourceControlEntities())
            {

                StringBuilder sb = new StringBuilder();

                Random randomNum = new Random();
                int i = 0;
                int recordId = 0;
                string address1 = "", city = "", state = "", zip = "";
                string email = "";
                string companyName = "";
                string firstName = "", firstName_ = "";
                string lastName = "";
                string[] lines = System.IO.File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + @"App_Data\Prospects.txt");
                foreach (string line in lines)
                {
                    i++;
                    if (!line.Contains("|")) continue;

                    string[] ray = line.Split(new char[] { '|' });


                    if ((i % 2) == 0)
                    {
                        firstName = ray[0];
                    }

                    if ((i % 3) == 0)
                    {
                        lastName = ray[1];
                    }

                    if (ray[1].Length > 0)
                    {
                        companyName = ray[1] + " and " + ray[1] + " Inc.";
                    }

                    if ((i % 4) == 0)
                    {
                        address1 = ray[3];
                    }
                    if ((i % 5) == 0)
                    {
                        city = ray[5];
                        zip = ray[6];
                        state = ray[7];
                    }

                    if ((i % 6) == 0)
                    {
                        if (address1.Length == 0 || city.Length == 0 || state.Length == 0 || zip.Length == 0) continue;
                        if (city == "Boys Town") continue;


                        recordId++;
                        email = firstName + "." + lastName + "@" + lastName + ".com";

                        address1 = address1.Replace("1", "36").Replace("4", "30").Replace("7", "46").Replace("9", "8").Replace("12", "2").Replace("0", "4").Replace("Street", "Blvd.").Replace("Ave", "Circle Dr.").Replace("Road", "Dr");

                        sb.Append("INSERT INTO Account(AccountName, AccountNumber,PrimaryContactId,Street1,City,StateProvince,ZipPostal,Email,CountryId) VALUES(");
                        sb.Append("'" + companyName.Replace("'", "''") + "',");
                        sb.Append("'" + recordId + "-" + randomNum.Next(3000, 10000) + "',");
                        sb.Append("" + recordId + ",");
                        sb.Append("'" + address1 + "',");
                        sb.Append("'" + city + "',");
                        sb.Append("'" + state + "',");
                        sb.Append("'" + zip + "',");
                        sb.Append("'" + email + "',");
                        sb.Append("722);");




                        sb.Append("INSERT INTO AccountUDF(AccountId, Number0, SmallText0,MediumText0,BigText0,Date0,UnlimitedText0) VALUES(");
                        sb.Append("" + recordId + ",");
                        sb.Append("1,");
                        sb.Append("'small text example',");
                        sb.Append("'jack and jill went up the hill',");
                        sb.Append("'each employee must validate their own badge number',");
                        sb.Append("getdate(),");
                        sb.Append("'programmers are given way too much power and money');");



                        sb.Append("INSERT INTO Contact(AccountId, IsPrimary,FirstName,LastName,Street1,City,StateProvince,ZipPostal,CountryId,Email,IsActive) VALUES(");
                        sb.Append("" + recordId + ",");
                        sb.Append("1,");
                        sb.Append("'" + firstName + "',");
                        sb.Append("'" + lastName + "',");
                        sb.Append("'" + address1 + "',");
                        sb.Append("'" + city + "',");
                        sb.Append("'" + state + "',");
                        sb.Append("'" + zip + "',");
                        sb.Append("722,");
                        sb.Append("'" + email + "',");
                        sb.Append("1);");

                        Db.Database.ExecuteSqlCommand(sb.ToString());
                        sb.Clear();

                        firstName_ = firstName;
                    }



                    if ((i % 7) == 0)
                    {
                        sb.Append("INSERT INTO Contact(AccountId, IsPrimary,FirstName,LastName,Street1,City,StateProvince,ZipPostal,CountryId,Email,IsActive) VALUES(");
                        sb.Append("" + recordId + ",");
                        sb.Append("0,");
                        sb.Append("'" + firstName + "',");
                        sb.Append("'" + lastName + "',");
                        sb.Append("'" + address1 + "',");
                        sb.Append("'" + city + "',");
                        sb.Append("'" + state + "',");
                        sb.Append("'" + zip + "',");
                        sb.Append("722,");
                        sb.Append("'" + email + "',");
                        sb.Append("1);");

                        Db.Database.ExecuteSqlCommand(sb.ToString());
                        sb.Clear();
                    }







                    if (recordId > 100) break;
                }




            }






        }
    }


    public class JsonpResult : JsonResult
    {
        public JsonpResult(string callbackName)
        {
            CallbackName = callbackName;
        }

        public JsonpResult() : this("jsoncallback")
        {
        }

        public string CallbackName { get; set; }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            var request = context.HttpContext.Request;
            var response = context.HttpContext.Response;

            string jsoncallback = ((context.RouteData.Values[CallbackName] as string) ?? request[CallbackName]) ?? CallbackName;

            if (!string.IsNullOrEmpty(jsoncallback))
            {
                if (string.IsNullOrEmpty(base.ContentType))
                {
                    base.ContentType = "application/x-javascript";
                }
                response.Write(string.Format("{0}(", jsoncallback));
            }

            base.ExecuteResult(context);

            if (!string.IsNullOrEmpty(jsoncallback))
            {
                response.Write(")");
            }
        }
    }

    public static class ControllerExtensions
    {
        public static JsonpResult Jsonp(this Controller controller, object data, string callbackName = "callback")
        {
            return new JsonpResult(callbackName)
            {
                Data = data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public static T DeserializeObject<T>(this Controller controller, string key) where T : class
        {
            var value = controller.HttpContext.Request.QueryString.Get(key);
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            return javaScriptSerializer.Deserialize<T>(value);
        }
    }
}
