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
    
    public partial class RequestSSL
    {
        public int ID { get; set; }
        public int RequestNumber { get; set; }
        public string Type { get; set; }
        public string CN { get; set; }
        public string OU { get; set; }
        public string O { get; set; }
        public string L { get; set; }
        public string ST { get; set; }
        public string C { get; set; }
        public string SA { get; set; }
        public string Notes { get; set; }
        public string sslName { get; set; }
    }
}
