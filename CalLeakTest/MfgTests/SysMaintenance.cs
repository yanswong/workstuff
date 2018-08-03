using Agilent.TMFramework.InstrumentIO;
using CalLeakTest.Models;
using SerialPortIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CalLeakTest.MfgTests
{
    public class SysMaintenance
    {
        public DirectIO mySw { get; set; }
        public DirectIO myScope { get; set; }
        public NistData myNistData { get; set; }
        public VSLeakDetector myLD { get; set; }

        internal void Execute(ref PluginSequence.TestInfo myTestInfo, ref PluginSequence.UUTData myUUTData, ref PluginSequence.CommonData myCommonData)
        {
            try
            {
                switch (myTestInfo.TestLabel)
                {
                    case "InitNistCalLeakData":
                        {
                            DoCalLeakDataEntry(ref myTestInfo, ref myUUTData, ref myCommonData);
                        }
                        break;
                    case "CalibrateSystem":
                        {
                            DoInternalCalibration(ref myTestInfo, ref myUUTData, ref myCommonData);
                        }
                        break;
                    case "DailySelfTest":
                        {

                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void DoInternalCalibration(ref PluginSequence.TestInfo myTestInfo, ref PluginSequence.UUTData myUUTData, ref PluginSequence.CommonData myCommonData)
        {
            try
            {

            }
            catch (Exception)
            {

                throw;
            }
        }

        private void DoCalLeakDataEntry(ref PluginSequence.TestInfo myTestInfo, ref PluginSequence.UUTData myUUTData, ref PluginSequence.CommonData myCommonData)
        {
            try
            {
                // call form for data entry for NIST Calibrated leak.
                FormNistLeak myNistDataForm = new FormNistLeak();
                myNistDataForm.ShowDialog();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
