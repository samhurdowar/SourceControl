//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SourceControl.Models.Db
{
    using System;
    using System.Collections.Generic;
    
    public partial class SMChangesTemp
    {
        public System.Guid guid { get; set; }
        public System.Guid jobguid { get; set; }
        public System.DateTime lastUpdated { get; set; }
        public string ChangeID { get; set; }
        public string ApprovalStatus { get; set; }
        public string Category { get; set; }
        public string ChangeCoordinator { get; set; }
        public string AssignedTo { get; set; }
        public string AssignmentGroup { get; set; }
        public string ChangeModel { get; set; }
        public Nullable<System.DateTime> DateEntered { get; set; }
        public string InitiatedBy { get; set; }
        public Nullable<bool> Open { get; set; }
        public string Phase { get; set; }
        public Nullable<System.DateTime> PlannedEnd { get; set; }
        public Nullable<System.DateTime> PlannedStart { get; set; }
        public Nullable<int> Priority { get; set; }
        public Nullable<int> RiskAssessment { get; set; }
        public string Status { get; set; }
        public string Subcategory { get; set; }
        public string Title { get; set; }
        public Nullable<System.DateTime> ImplementationEnd { get; set; }
        public Nullable<System.DateTime> ImplementationStart { get; set; }
        public Nullable<int> ClosureCode { get; set; }
        public Nullable<int> Impact { get; set; }
        public string Reason { get; set; }
    }
}
