using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PluginSequence;
using Agilent.TMFramework.InstrumentIO;
using CalLeakTest.Models;
using SystemIO;
using System.Threading;
using CalLeakTest.Repositories;
using SerialPortIO;
using System.Diagnostics;
using System.Windows.Forms;

namespace CalLeakTest.MfgTests
{
    public class SystemCal
    {
        public DirectIO mySw { get; set; }
        public DirectIO myScope { get; set; }
        public NistData myNistData { get; set; }
        public VSLeakDetector myLD { get; set; }

        public void Execute(ref TestInfo myTestInfo, ref UUTData myUUTData, ref CommonData myCommonData)
        {
            try
            {
                switch (myTestInfo.TestLabel)
                {
                    case "StoreStdLeakRate":
                        {
                            StoreLeakRate(ref myTestInfo, ref myUUTData, ref myCommonData);
                        }
                        break;
                    case "Calibrate":
                        {
                            DoExternalCal(ref myTestInfo, ref myUUTData, ref myCommonData);
                        }
                        break;
                    case "Experiment":
                        {
                            DoExperiment1(ref myTestInfo, ref myUUTData, ref myCommonData);
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

        private void DoExperiment1(ref TestInfo myTestInfo, ref UUTData myUUTData, ref CommonData myCommonData)
        {
            try
            {
                var loop = Convert.ToInt32(myTestInfo.TestParams[1].Value);
                var delay = Convert.ToInt32(myTestInfo.TestParams[2].Value);

                // Close all Test Valves
                InstrumentIO.CloseAllTestValve(mySw);
                Thread.Sleep(5000);
                InstrumentIO.OpenTestValve(mySw, 0);    // open NIST valve
                Thread.Sleep(5000);
                // Configure Thermocouple measurement
                InstrumentIO.ConfigureThermocouple(mySw);

                myTestInfo.ResultsParams = new PIResultsArray();
                List<PIResult> listOfPIResult = new List<PIResult>();
                for (int i = 0; i < loop; i++)
                {
                    listOfPIResult.Add(new PIResult
                    {
                        Label = string.Format("NistLeakRate#{0}", i + 1),
                        SpecMin = "1E-7",
                        SpecMax = "2E-7",
                        Nominal = "1.5E-7",
                        Unit = "std cc/sec"
                    });
                }
                for (int i = 0; i < loop; i++)
                {
                    listOfPIResult.Add(new PIResult
                    {
                        Label = string.Format("NISTTemp#{0}", i + 1),
                        SpecMin = "15",
                        SpecMax = "30",
                        Nominal = "22.5",
                        Unit = "Deg C"
                    });
                }
                for (int i = 0; i < loop; i++)
                {
                    listOfPIResult.Add(new PIResult
                    {
                        Label = string.Format("UUTTemp#{0}", i + 1),
                        SpecMin = "15",
                        SpecMax = "30",
                        Nominal = "22.5",
                        Unit = "Deg C"
                    });
                }
                for (int i = 0; i < loop; i++)
                {
                    listOfPIResult.Add(new PIResult
                    {
                        Label = string.Format("UUTSensorTemp#{0}", i + 1),
                        SpecMin = "15",
                        SpecMax = "30",
                        Nominal = "22.5",
                        Unit = "Deg C"
                    });
                }

                myTestInfo.ResultsParams.PIResArray = listOfPIResult.ToArray();
                myTestInfo.ResultsParams.NumResultParams = listOfPIResult.Count;


                // Test start here
                for (int i = 1; i <= loop; i++)  // loop 5760 * 60 * 60  with 5sec delay = 8hrs
                {
                    /* Read 
                     * (1) leak rate NIST,
                     * (2) Temp NIST
                     * (3) Temp UUT
                     * (4) Temp Sensor Board
                    */
                    var leakRate = Convert.ToDouble(myLD.ReadLeakRate().Trim());
                    var nistTemp = InstrumentIO.MeasureCalLeakTemp(mySw, 0);
                    var uutTemp = InstrumentIO.MeasureCalLeakTemp(mySw, 1);
                    var uutSensTemp = InstrumentIO.MeasureBoardTemperature(mySw, myScope, 1);

                    Trace.WriteLine(string.Format("NIST Leak Rate #{0} = {1}std cc/sec   &    NIST Temp #{0} = {2} Deg C   &    DUT Temp #{0} = {3} Deg C   &   Board Temp #{0} = {4} Deg C", i, leakRate, nistTemp, uutTemp, uutSensTemp));
                    myTestInfo.ResultsParams[i].Result = Math.Round(leakRate, 8).ToString();
                    myTestInfo.ResultsParams[i + loop].Result = Math.Round(nistTemp, 3).ToString();
                    myTestInfo.ResultsParams[i + (loop * 2)].Result = Math.Round(uutTemp, 3).ToString();
                    myTestInfo.ResultsParams[i + (loop * 3)].Result = Math.Round(uutSensTemp, 3).ToString();

                    // Delay 5 seconds, maybe
                    Thread.Sleep(delay * 1000);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void DoExternalCal(ref TestInfo myTestInfo, ref UUTData myUUTData, ref CommonData myCommonData)
        {
            try
            {
                var delay = Convert.ToInt32(myTestInfo.TestParams[1].Value);

                // Make sure all valve is closed.
                InstrumentIO.CloseAllTestValve(mySw);
                string retVal = string.Empty;

                // Rough vacuum
                Trace.WriteLine("Roughing in progress...");
                myLD.Rough();
                Thread.Sleep(10000);
                //bool isContraFlow = myLD.WaitForContraFlow(ref retVal);
                bool isMidStage = myLD.WaitForFineTest(ref retVal);
                if (!isMidStage)
                {
                    myTestInfo.ResultsParams[1].Result = retVal;
                    Trace.WriteLine("Unable to set to MIDSTAGE state.");
                }

                Thread.Sleep(5000);
                // Once in Fine-Test mode, let the leak rate to stabilize
                double zeroleakrate = Convert.ToDouble(myLD.ReadLeakRate());
                int timerys = 0;
                while (zeroleakrate > 5E-10)
                {
                    Thread.Sleep(1000);
                    zeroleakrate = Convert.ToDouble(myLD.ReadLeakRate());
                    Trace.WriteLine("calibration wait time: "+timerys);
                    timerys++;
                    if (timerys == 180)
                    {
                        Trace.WriteLine("Zero-ing");
                        myLD.Zero();
                        Thread.Sleep(2000);
                        //MessageBox.Show("to reach 5E-10 in 15 minutes","Fail",MessageBoxButtons.OK);
                        //myTestInfo.ResultsParams[1].Result = "FAIL";
                        break;
                    }
                }

               
                // Set LD to Hold state
                myLD.Hold();
                Thread.Sleep(5000);

                // Open NIST Valve
                Trace.WriteLine("Open NIST Test Valve.");
                InstrumentIO.OpenTestValve(mySw, 0);
                Thread.Sleep(1000);

                // Now set the LD to TEST mode
                var stat = myLD.Rough();
                Thread.Sleep(10000);
                isMidStage = myLD.WaitForFineTest(ref retVal);
                if (!isMidStage)
                {
                    myTestInfo.ResultsParams[1].Result = retVal;
                    Trace.WriteLine("Unable to set to MIDSTAGE state.");
                }
                else
                {
                    myTestInfo.ResultsParams[1].Result = "FINETEST";
                    // Delay 2 minutes for the LD to stabilize.
                    Trace.WriteLine("Waiting for the VS Leak Detector to stabilize in 2 minutes");
                    Thread.Sleep(delay * 1000);   // wait for 2 minutes (120,000 miliseconds)
                    Trace.WriteLine("VS Leak Detector is now stabilized");

                    var stab = "";
                    bool stabilized = myLD.WaitForStabilizedReading(ref stab);
                    if (stabilized) Trace.WriteLine("Leak rate reading is stabilized!");
                    else Trace.WriteLine("Timeout to get stable leak rate reading!");

                    // Now calibrate the VS Leak Detector.
                    string calStatus = myLD.Calibrate();        // Default timeout for calibration is 300seconds, change to overload method and provide new Timeout value
                    Thread.Sleep(10000);
                    bool isCalibrationOK = myLD.CalIsOK();
                    if (isCalibrationOK)
                    {
                        Trace.WriteLine("Last calibration status is OK");

                    }
                    else
                    {
                        Trace.WriteLine("Fail Calibration!!");
                    }
                    
                    // Check the LeakRate
                    Trace.WriteLine("Read back the LeakRate from the VS Leak Detector");
                    var leakRate = Convert.ToDouble(myLD.ReadLeakRate().Trim());
                    Trace.WriteLine(string.Format("NIST LeakRate Measurement = {0}", leakRate));
                    var storedLeakRate = Convert.ToDouble(myLD.GetExternalLeakValue().Trim());
                    var leakDelta = leakRate - storedLeakRate;//Math.Abs(storedLeakRate - leakRate);
                    Trace.WriteLine("Difference between measured and stored leak rate = " + leakDelta.ToString());

                    // Save Result
                    myTestInfo.ResultsParams[2].Result = leakRate.ToString();
                    double lowerLimit = storedLeakRate - 0.2E-7;
                    double upperLimit = storedLeakRate + 0.2E-7;
                    myTestInfo.ResultsParams[2].SpecMax = Math.Round(upperLimit, 9).ToString();
                    myTestInfo.ResultsParams[2].SpecMin = Math.Round(lowerLimit, 9).ToString();
                    myTestInfo.ResultsParams[3].Result = Math.Round(leakDelta, 10).ToString();
                    if (leakDelta > 0.2E-7)   // If more than +-2 division.
                    {
                        // Calibrate fail.
                        // todo: do we need to recalibrate or just proceed.

                    }
                }

                // Close all Test Valves
                InstrumentIO.CloseAllTestValve(mySw);


            }
            catch (Exception)
            {

                throw;
            }
        }

        private void StoreLeakRate(ref TestInfo myTestInfo, ref UUTData myUUTData, ref CommonData myCommonData)
        {
            try
            {
                Trace.WriteLine("Measuring NIST Temperature...");
                var nistLeakTemp = InstrumentIO.MeasureCalLeakTemp(mySw, 0);
                Trace.WriteLine("NIST Temperature = " + nistLeakTemp.ToString());
                
                int nistLeakTempInt = Convert.ToInt32(nistLeakTemp);
                Trace.WriteLine("NIST Temperature = " + nistLeakTempInt.ToString());
                double leakRate = TestHelper.GetLeakRateFromTempMap(nistLeakTempInt, myNistData);
                Trace.WriteLine(String.Format("NIST Leak Rate @{0}deg C = {1}", nistLeakTempInt, leakRate));

                Trace.WriteLine("External Cal-Leak configuration in progress...");
                Trace.WriteLine("Set VS Leak Detector to use External Cal Leak");
                myLD.UseExtCalLeakRef();    // Set LD to use External CAL Leak

                //leakRate = Math.Round(leakRate, 8);     // the number in scientific must follow exactly this format (X.XE-7 or X.XE-8), else error will occurs
                string leakRateMod = "";
                string strLeakRate = "";
                if (leakRate < 1E-7) {
                    leakRateMod = Math.Round(leakRate * 100000000, 1).ToString();
                    Trace.WriteLine("External Cal-Leak value to be stored in VS Leak Detector = " + leakRateMod.ToString() + "E-8");
                    strLeakRate = string.Format("{0}E-8", leakRateMod);
                    Trace.WriteLine("Storing leak rate value....");
                    myLD.SetExtCalLeakValue(strLeakRate);
                    Trace.WriteLine("Done!");

                    Trace.WriteLine("Leak rate = " + strLeakRate);
                }
                else {
                    leakRateMod = Math.Round(leakRate * 10000000, 1).ToString();
                    Trace.WriteLine("External Cal-Leak value to be stored in VS Leak Detector = " + leakRateMod.ToString() + "E-7");
                    strLeakRate = string.Format("{0}E-7", leakRateMod);
                    Trace.WriteLine("Storing leak rate value....");
                    myLD.SetExtCalLeakValue(strLeakRate);
                    Trace.WriteLine("Done!");

                    Trace.WriteLine("Leak rate = " + strLeakRate);
                }
                
                
                
                var storedVal = myLD.GetExternalLeakValue().Trim();
                Trace.WriteLine("Readback Leak Rate Value from Leak Detector = " + storedVal.ToString());

                if (double.Parse(strLeakRate) == double.Parse(storedVal))
                {
                    // pass
                    myTestInfo.ResultsParams[1].Result = "PASS";
                    myTestInfo.ResultsParams[2].Result = storedVal;
                }
                else
                {
                    // FAIL
                    myTestInfo.ResultsParams[1].Result = "FAIL";
                    myTestInfo.ResultsParams[2].Result = storedVal;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

    }
}
