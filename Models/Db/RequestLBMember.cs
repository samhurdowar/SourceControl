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
    
    public partial class RequestLBMember
    {
        public int ID { get; set; }
        public int RequestNumber { get; set; }
        public string name { get; set; }
        public string ip { get; set; }
        public string port { get; set; }
    }
}