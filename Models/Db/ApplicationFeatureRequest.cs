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
    
    public partial class ApplicationFeatureRequest
    {
        public System.Guid guid { get; set; }
        public string requester { get; set; }
        public string application { get; set; }
        public string description { get; set; }
        public System.DateTime dateSubmitted { get; set; }
        public Nullable<System.DateTime> dueDate { get; set; }
        public string priority { get; set; }
        public Nullable<bool> completed { get; set; }
        public Nullable<System.DateTime> dateCompleted { get; set; }
        public string completionComments { get; set; }
    }
}
