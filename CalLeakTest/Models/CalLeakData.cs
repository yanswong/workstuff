using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CalLeakTest.Models
{
    public class CalLeakData
    {
        public int Id { get; set; }
        public string PartNumber { get; set; }
        public string SerialNumber { get; set; }
        public int TestPortId { get; set; }
        public double LeakRate { get; set; }
        public double UUTTemp { get; set; }
        public double BoardTemp { get; set; }
        public double Factor { get; set; }
        public DateTime TestDate { get; set; }
        public string TestedBy { get; set; }
        public string NISTSerialNumber { get; set; }
        public string NISTDescription { get; set; }
        public string NISTPartNumber { get; set; }
        public string NISTReportNumber { get; set; }
        public DateTime NISTCalDate { get; set; }
        public DateTime NISTCalDueDate { get; set; }
        public string StationSerialNumber { get; set; }
        public string StationDescription { get; set; }
        public string StationModelNumber { get; set; }
        public string StationReportNumber { get; set; }
        public DateTime StationCalDate { get; set; }
        public DateTime StationCalDueDate { get; set; }
        public bool IsPass { get; set; }
    }

    public class CalLeakCertificate
    {
        public Int64 Id { get; set; }
        public int CalLeakDataId { get; set; }
        public string EquipmentIds { get; set; }
    }
}
