using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceControl.Models.Db
{

	public class wrapperData
	{
		public CreateTicket CreateTicket { get; set; }
		public List<int> recordIds { get; set; }
		public string NessusReportID { get; set; }
	}
	public class CreateTicket
	{
		public Incident Incident { get; set; }

	}
	public class Incident
	{
		public string[] AffectedCI { get; set; }
		//public string Area { get; set; }
		public string AssignmentGroup { get; set; }
		public string Category { get; set; }
		public string Company { get; set; }
		public string Contact { get; set; }
		public string[] Description { get; set; }
		public string Impact { get; set; }
		public string Phase { get; set; }
		public string Service { get; set; }
		public string Title { get; set; }
		public string Urgency { get; set; }
		public string relatedrecordIds { get; set; }
	}

	public class serverinfo
	{
		public string ID { get; set; }
		public string name { get; set; }

	}
	public class SMUsers
	{
		public string UserID { get; set; }
		public string FullName { get; set; }
		public string Email { get; set; }
		public List<string> Groups { get; set; }
	}
	public class ServerStats
	{
		public string server { get; set; }
		public bool remediated { get; set; }
		public int remediatedCount { get; set; }
		public int pendingCount { get; set; }
	}
	public partial class ManagementRecord
	{
		public string assetGuid { get; set; }
		public Guid nessusDataGuid { get; set; }
	}


	public class Notifications
	{
		public Guid guid { get; set; }
		public DateTime date { get; set; }
		public string title { get; set; }
		public string description { get; set; }
		public string link { get; set; }
	}
	public class SummaryData
	{
		public int TotalRecordCount { get; set; }
		public int DistinctPluginIDCount { get; set; }
		public int DistinctIPCount { get; set; }
		public int TotalWithExploit { get; set; }
		public int TotalMitigated { get; set; }
		public int Sev4RecordCount { get; set; }
		public int Sev3RecordCount { get; set; }
		public int Sev2RecordCount { get; set; }
		public int Sev1RecordCount { get; set; }
		public List<PluginCount> Top20PluginHitters { get; set; }
		public int LastSeen30DayCount { get; set; }
		public int FirstSeen30DayCount { get; set; }
		public int LastSeen15DayCount { get; set; }
		public int FirstSeen15DayCount { get; set; }
		public int LastSeen7DayCount { get; set; }
		public int FirstSeen7DayCount { get; set; }
	}
	public class PluginCount
	{
		public string PluginID { get; set; }
		public int CurrentCount { get; set; }
		public string Description { get; set; }
		public string PluginName { get; set; }
	}
	public class findingsList
	{
		public Guid guid { get; set; }
		public string pluginID { get; set; }
		public string ip { get; set; }
		public string port { get; set; }
		public string pluginName { get; set; }
		public string name { get; set; }
		public string engGroup { get; set; }
		public string severity { get; set; }
		public string hasBeenMitigated { get; set; }
		public string firstSeen { get; set; }
		public string exploitAvailable { get; set; }
		public string stigSeverity { get; set; }
		public string cve { get; set; }
		public string xref { get; set; }
		public Guid assignmentGroupGuid { get; set; }
		public Guid assetGuid { get; set; }
		public Guid nessusDataGuid { get; set; }
	}
	public class managementRecordList
	{
		public Guid guid { get; set; }
		public string title { get; set; }
		public string owner { get; set; }
		public string assignmentGroup { get; set; }
		public bool active { get; set; }
		public string status { get; set; }
		public string engGroup { get; set; }
		public string creator { get; set; }
		public bool falsePositive { get; set; }

	}

	public class engineeringGroupList
	{
		public string name { get; set; }
		public Guid guid { get; set; }
		public string owner { get; set; }
		public string email { get; set; }
		public string assignmentName { get; set; }
	}
	public class selectedGroups
	{
		public Guid asset { get; set; }
		public Guid engineeringGroup { get; set; }
	}

	public partial class Asset
	{
		public string engineeringGroup { get; set; }
	}


}


