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
    
    public partial class RequestLB
    {
        public int ID { get; set; }
        public int RequestNumber { get; set; }
        public string ExternalURL { get; set; }
        public string Notes { get; set; }
        public string VIPPort { get; set; }
        public string method { get; set; }
        public string monitor { get; set; }
        public string persist { get; set; }
        public string profile { get; set; }
        public string ssl { get; set; }
        public string sslName { get; set; }
        public string sslDetails { get; set; }
    }
}