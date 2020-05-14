using SourceControl.Models.Db;
using SourceControl.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace SourceControl.FormLayoutClasses
{
	public partial class FormLayout
	{
		public StringBuilder DocumentReady = new StringBuilder();
		public StringBuilder Functions = new StringBuilder();
		public StringBuilder AfterForm = new StringBuilder();
		public string TableName;
		public int PageTemplateId;

		public FormLayout(int pageTemplateId)
		{
			PageTemplateId = pageTemplateId;
			TableName = SessionService.TableName(pageTemplateId);
		}


	}
}