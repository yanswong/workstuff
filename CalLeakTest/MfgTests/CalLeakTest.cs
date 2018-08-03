using Agilent.TMFramework.InstrumentIO;
using CalLeakTest.Models;
using CalLeakTest.Repositories;
using PluginSequence;
using SerialPortIO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using SystemIO;

namespace CalLeakTest.MfgTests
{
    public class DUTTest
    {
        public DirectIO mySw { get; set; }
        public DirectIO myScope { get; set; }
        public NistData myNistData { get; set; }
        public VSLeakDetector myLD { get; set; }


        public double UUTTempFactor { get; set; }
        public double UUTLeakRate { get; set; }
        public double UUTTemp { get; set; }

        public void Execute(ref TestInfo myTestInfo, ref UUTData myUUTData, ref CommonData myCommonData)
        {
            try
            {
                switch (myTestInfo.TestLabel)
                {
                    case "LeakRateTest":
                        {
                            DoLeakRateTest(ref myTestInfo, ref myUUTData, ref myCommonData);
                        }
                        break;
                    case "LeakRateProfilingTest":
                        {
                            DoLeakRateProfilingTest(ref myTestInfo, ref myUUTData, ref myCommonData);
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

        

        private void DoLeakRateTest(ref TestInfo myTestInfo, ref UUTData myUUTData, ref CommonData myCommonData)
        {
            try
            {
                var slope = Convert.ToDouble(myTestInfo.TestParams[1].Value);
                var refTemp = Convert.ToDouble(myTestInfo.TestParams[2].Value);
                var loop = Convert.ToInt32(myTestInfo.TestParams[3].Value);
                var acquireDelay = Convert.ToInt32(myTestInfo.TestParams[4].Value);
                var stabilizeDelay = Convert.ToInt32(myTestInfo.TestParams[5].Value);

                int slot = Convert.ToInt32(myCommonData.Slot);

                // Step #1: Measure leak rate and calculate normalized leak rate
                // Close all valves
                Trace.WriteLine("Verifying test valves are closed...");
                InstrumentIO.CloseAllTestValve(mySw);
                Thread.Sleep(3000);
                Trace.WriteLine("All valves are closed");
                // hold valve
                Trace.WriteLine("Set VS Leak Detector to HOLD mode");
                myLD.Hold();
                Thread.Sleep(3000);
                // Open Test Port
                Trace.WriteLine(String.Format("Open SLOT#{0} DUT's Test Valve", slot));
                InstrumentIO.OpenTestValve(mySw, slot);
                Thread.Sleep(2000);
                myLD.Rough();
                
                string status = "";
                bool isMidStage = myLD.WaitForFineTest(ref status);
                if (!isMidStage)
                {
                    myTestInfo.ResultsParams[1].Result = status;
                    Trace.WriteLine("Unable to set to MIDSTAGE state.");
                }

                bool isReadingStabilized = myLD.WaitForStabilizedReading(ref status);
                if (!isReadingStabilized)
                {
                    Trace.WriteLine("Timeout to get stable leak rate reading");
                }
                Thread.Sleep(stabilizeDelay * 1000);

                // Loop measurement for DUT's Leak Rate
                var listOfLeakRate = new List<double>();
                var listOftemp = new List<double>();
                var listOfBoardTemp = new List<double>();
                // Configure Thermocouple measurement
                InstrumentIO.ConfigureThermocouple(mySw);
                Trace.WriteLine(String.Format("Measuring DUT's Leak Rate and Temperature for {0} times.", loop));   
                for (int i = 0; i < loop; i++)
                {
                    // read LeakRate
                    double leakRate = Convert.ToDouble(myLD.ReadLeakRate());
                    var dutTemp = InstrumentIO.MeasureCalLeakTemp(mySw, slot);
                    var boardTemp = InstrumentIO.MeasureBoardTemperature(mySw, myScope, slot);

                    Trace.WriteLine(string.Format("Leak Rate #{0} = {1}std cc/sec   &    DUT Temp #{0} = {2} Deg C   &   Board Temp #{0} = {3} Deg C", i + 1, leakRate, dutTemp, boardTemp));
                    listOfLeakRate.Add(leakRate);
                    listOftemp.Add(dutTemp);
                    listOfBoardTemp.Add(boardTemp);
                    Thread.Sleep(acquireDelay * 1000);     // delay in miliseconds
                }

                // calculate normalized leak rate
                var aveLeakRate = listOfLeakRate.Average();
                Trace.WriteLine("Average Leak Rate = " + aveLeakRate.ToString());

                var aveTemp = listOftemp.Average();
                Trace.WriteLine("Average DUT's Temperature = " + aveTemp.ToString());

                var aveBoardTemp = listOfBoardTemp.Average();
                Trace.WriteLine("Average Board's Temperature = " + aveBoardTemp.ToString());

                Trace.WriteLine(string.Format("Calculating DUT Normalized Leak Rate at {0} Deg Celcius reference temperature", refTemp));
                var normLeakRateDouble = TestHelper.CalculateNormLeakRate(slope, refTemp, aveLeakRate, aveTemp);
                Trace.WriteLine("Normalized Leak Rate = " + normLeakRateDouble.ToString());

                // store result
                var normLeakRate = Math.Round(normLeakRateDouble, 9);
                Trace.WriteLine("Normalized Leak Rate = " + normLeakRate.ToString());
                //myTestInfo.ResultsParams[1].Result = normLeakRate.ToString();
                myTestInfo.ResultsParams[1].Result = Math.Round(aveLeakRate, 9).ToString();

                // Step #2: Temperature measurement to get Factor (Delta DegC between board and cal leak can's temperature
                // Get Can's temp
                //var cansTemp = InstrumentIO.MeasureCalLeakTemp(mySw, slot);
                Trace.WriteLine("DUT Temperature (Thermocouple)  :  " + aveTemp.ToString());
                myTestInfo.ResultsParams[2].Result = Math.Round(aveTemp, 1).ToString();
                // Measure Board Temperature
                //InstrumentIO.SetupBoardTempMeasRoute(mySw, myScope, slot);   // setup route for BoardTemp supply and output 5V and TTL signal output

                myTestInfo.ResultsParams[3].Result = Math.Round(aveBoardTemp, 1).ToString();
                //InstrumentIO.DisconnectTempBoardMeasRoute(mySw, slot);
                Trace.WriteLine("DUT Temperature (PCB)           :  " + aveBoardTemp.ToString());
                // calc temp difference
                var deltaTemp = aveBoardTemp - aveTemp;
                Trace.WriteLine("Temperature Difference          :  " + deltaTemp.ToString());
                // store result
                myTestInfo.ResultsParams[4].Result = Math.Round(deltaTemp, 1).ToString();
                
                // Close Test Valve
                InstrumentIO.CloseTestValve(mySw, slot);

                // Set LeakRate and Factor properties to be used for averaging at the end of the test
                UUTLeakRate = Math.Round(aveLeakRate, 9);//normLeakRateDouble;
                UUTTempFactor = deltaTemp;
                UUTTemp = aveTemp;

            }
            catch (Exception)
            {

                throw;
            }
        }

        private void DoLeakRateProfilingTest(ref TestInfo myTestInfo, ref UUTData myUUTData, ref CommonData myCommonData)
        {
            try
            {
                var slope = Convert.ToDouble(myTestInfo.TestParams[1].Value);
                var refTemp = Convert.ToDouble(myTestInfo.TestParams[2].Value);
                var loop = Convert.ToInt32(myTestInfo.TestParams[3].Value);
                var acquireDelay = Convert.ToInt32(myTestInfo.TestParams[4].Value);
                //var holdDelay = Convert.ToInt32(myTestInfo.TestParams[5].Value);

                int slot = Convert.ToInt32(myCommonData.Slot);

                // Step #1: Measure leak rate and calculate normalized leak rate
                // Close all valves
                Trace.WriteLine("Verifying test valves are closed...");
                InstrumentIO.CloseAllTestValve(mySw);
                Thread.Sleep(5000);
                Trace.WriteLine("All valves are closed");
                // hold valve
                Trace.WriteLine("Set VS Leak Detector to HOLD mode");
                myLD.Hold();
                Thread.Sleep(5000);
                // Open Test Port
                Trace.WriteLine(String.Format("Open SLOT#{0} DUT's Test Valve", slot));
                InstrumentIO.OpenTestValve(mySw, slot);
                Thread.Sleep(1000);
                myLD.Rough();   // rough the LD
                //Thread.Sleep(holdDelay * 1000);

                // once LD in FineTest
                myTestInfo.ResultsParams = new PIResultsArray();
                List<PIResult> listOfPIResult = new List<PIResult>();
                for (int i = 0; i < loop; i++)
                {
                    listOfPIResult.Add(new PIResult
                        {
                            Label = string.Format("LeakRate#{0}", i + 1),
                            SpecMin = "1E-7", SpecMax = "2E-7", Nominal = "1.5E-7",
                            Unit = "std cc/sec"
                        });
                }
                for (int i = 0; i < loop; i++)
                {
                    listOfPIResult.Add(new PIResult
                    {
                        Label = string.Format("Temp#{0}", i + 1),
                        SpecMin = "15",
                        SpecMax = "30",
                        Nominal = "22.5",
                        Unit = "Deg C"
                    });
                }
                myTestInfo.ResultsParams.PIResArray = listOfPIResult.ToArray();
                myTestInfo.ResultsParams.NumResultParams = listOfPIResult.Count;


                // Loop measurement for DUT's Leak Rate
                var listOfLeakRate = new List<double>();
                var listOftemp = new List<double>();
                var listOfBoardTemp = new List<double>();
                // Configure Thermocouple measurement
                InstrumentIO.ConfigureThermocouple(mySw);
                Trace.WriteLine(String.Format("Measuring DUT's Leak Rate and Temperature for {0} times.", loop));
                int cnt1 = 0;
                int cnt2 = 0;
                for (int i = 1; i <= loop; i++)
                {
                    // read LeakRate
                    double leakRate = Convert.ToDouble(myLD.ReadLeakRate());
                    var dutTemp = InstrumentIO.MeasureCalLeakTemp(mySw, slot);
                    var boardTemp = InstrumentIO.MeasureBoardTemperature(mySw, myScope, slot);

                    Trace.WriteLine(string.Format("Leak Rate #{0} = {1}std cc/sec   &    DUT Temp #{0} = {2} Deg C   &   Board Temp #{0} = {3} Deg C", i, leakRate, dutTemp, boardTemp));
                    listOfLeakRate.Add(leakRate);
                    listOftemp.Add(dutTemp);
                    listOfBoardTemp.Add(boardTemp);
                    Thread.Sleep(acquireDelay * 1000);     // delay in miliseconds

                    myTestInfo.ResultsParams[i].Result = Math.Round(leakRate, 8).ToString();
                    myTestInfo.ResultsParams[i + loop].Result = Math.Round(dutTemp, 3).ToString();
                    


                }

                

            }
            catch (Exception)
            {

                throw;
            }
        }

    }
}
