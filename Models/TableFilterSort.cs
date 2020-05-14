using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SourceControl.Models
{
	public class TableFilterSort
	{
		public string GridColumns { get; set; }
		public string SortColumns { get; set; }
		public string StandardSelect { get; set; }
		public string InnerSelect { get; set; }
		public string OuterSelect { get; set; }
		public string InnerJoin { get; set; }
		public Dictionary<string, string> FilterMap = new Dictionary<string, string>();
		public Dictionary<string, string> Sort1Map = new Dictionary<string, string>();
		public Dictionary<string, string> Sort2Map = new Dictionary<string, string>();
	}
}