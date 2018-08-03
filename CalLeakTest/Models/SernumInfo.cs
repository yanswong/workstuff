using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CalLeakTest.Models
{
    public class SernumInfo
    {
        public int Id { get; set; }
        public DateTime LogDate { get; set; }
        public string Model { get; set; }
        public string Sernum { get; set; }
        public string DummySernum { get; set; }
        public bool IsPassTest { get; set; }
        public int RunningNumber { get; set; }
    }
}
