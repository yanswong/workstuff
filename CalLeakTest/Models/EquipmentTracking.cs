using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CalLeakTest.Models
{
    public class EquipmentTracking
    {
        public long Id { get; set; }
        public string TrackingId { get; set; }
        public string EquipmentPartnumber { get; set; }
        public string SerialNumber { get; set; }
        public string Description { get; set; }
        public DateTime CalibrationDate { get; set; }
        public DateTime CalibrationDueDate { get; set; }
    }
}
