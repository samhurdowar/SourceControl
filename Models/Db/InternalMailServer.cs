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
    
    public partial class InternalMailServer
    {
        public System.Guid guid { get; set; }
        public string name { get; set; }
        public int port { get; set; }
        public bool useAuth { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public bool active { get; set; }
    }
}