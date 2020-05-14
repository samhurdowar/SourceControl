using SourceControl.Models.Db;
using SourceControl.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace SourceControl.Controllers
{
	public class AccessController : Controller
	{



		public string GetAvailRoleUsers(int id)
		{
			using (SourceControlEntities Db = new SourceControlEntities())
			{
				int[] haveIds = Db.UserRoles.Where(w => w.RoleId == id).Select(s => s.UserId).ToArray();

				var recs = Db.AppUsers.Where(w => !haveIds.Contains(w.UserId)).OrderBy(o => o.LastName).ThenBy(t => t.FirstName).ToList();
				var json = new JavaScriptSerializer().Serialize(recs);
				return json;
			}
		}


		public string GetAssignedRoleUsers(int id)
		{
			using (SourceControlEntities Db = new SourceControlEntities())
			{
				int[] haveIds = Db.UserRoles.Where(w => w.RoleId == id).Select(s => s.UserId).ToArray();

				var recs = Db.AppUsers.Where(w => haveIds.Contains(w.UserId)).OrderBy(o => o.LastName).ThenBy(t => t.FirstName).ToList();
				var json = new JavaScriptSerializer().Serialize(recs);
				return json;
			}
		}



		public string GetAvailRoles(int id)
		{
			using (SourceControlEntities Db = new SourceControlEntities())
			{
				int[] haveIds = Db.UserRoles.Where(w => w.UserId == id).Select(s => s.RoleId).ToArray();

				var recs = Db.Roles.Where(w => !haveIds.Contains(w.RoleId)).OrderBy(o => o.RoleName).ToList();
				var json = new JavaScriptSerializer().Serialize(recs);
				return json;
			}
		}

		public string GetAssignedRoles(int id)
		{
			using (SourceControlEntities Db = new SourceControlEntities())
			{
				int[] haveIds = Db.UserRoles.Where(w => w.UserId == id).Select(s => s.RoleId).ToArray();

				var recs = Db.Roles.Where(w => haveIds.Contains(w.RoleId)).OrderBy(o => o.RoleName).ToList();
				var json = new JavaScriptSerializer().Serialize(recs);
				return json;
			}
		}


		[HttpPost]
		public string SubmitRequestAccess(string menuIds)
		{
			using (SourceControlEntities Db = new SourceControlEntities())
			{
				Db.Database.ExecuteSqlCommand("DELETE FROM UserMenuAccess WHERE IsRequest = 1 AND UserId = " + SessionService.UserId);
				Db.SaveChanges();

				string[] ids = menuIds.Split(new char[] { ',' });
				foreach (string id in ids)
				{
					Db.UserMenuAccesses.Add(new UserMenuAccess { MenuId = Convert.ToInt32(id), UserId = SessionService.UserId });
				}
				Db.SaveChanges();
				return "SUCCESS";
			}
		}

		[HttpPost]
		public string UpdateAccess(int userId, string ids)
		{
			using (SourceControlEntities Db = new SourceControlEntities())
			{
				Db.Database.ExecuteSqlCommand("DELETE FROM UserRole WHERE UserId = " + userId);
				Db.SaveChanges();

				string[] roleIds = ids.Split(new char[] { ',' });
				foreach (string roleId in roleIds)
				{
					Db.UserRoles.Add(new UserRole { RoleId = Convert.ToInt32(roleId), UserId = userId });
				}
				Db.SaveChanges();
				return "SUCCESS";
			}
		}

		//
	}
}