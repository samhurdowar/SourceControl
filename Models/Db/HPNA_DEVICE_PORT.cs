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
    
    public partial class HPNA_DEVICE_PORT
    {
        public double DevicePortID { get; set; }
        public Nullable<double> DeviceID { get; set; }
        public string PortCustom1 { get; set; }
        public string PortCustom2 { get; set; }
        public string PortCustom3 { get; set; }
        public string PortCustom4 { get; set; }
        public string PortCustom5 { get; set; }
        public string PortCustom6 { get; set; }
        public string Comments { get; set; }
        public string PortName { get; set; }
        public string PortAllows { get; set; }
        public string PortType { get; set; }
        public string PortStatus { get; set; }
        public string Description { get; set; }
        public Nullable<System.DateTime> LastModifiedDate { get; set; }
        public string MacAddress { get; set; }
        public string AssociatedVlanID { get; set; }
        public string ConfiguredDuplex { get; set; }
        public string ConfiguredSpeed { get; set; }
        public string NegotiatedDuplex { get; set; }
        public string NegotiatedSpeed { get; set; }
        public Nullable<double> SlotNumber { get; set; }
        public string PortState { get; set; }
        public string NativeVlan { get; set; }
        public string AssociatedChannelID { get; set; }
    }
}
