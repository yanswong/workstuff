using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PluginSequence;
using CalLeakTest.Repositories;
using System.Data.SqlClient;
using CalLeakTest.Models;
using SystemIO;
using SerialPortIO;
using Agilent.TMFramework.InstrumentIO;
using System.Threading;
using System.Diagnostics;

namespace CalLeakTest.MfgTests
{
    public class InitStation
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
                    case "NistLeakTempTest":
                        {
                            DoNistLeakTempTest(ref myTestInfo, ref myUUTData, ref myCommonData);
                        }
                        break;
                    case "InitNistDatatable":
                        {
                            DoInitNistDatatable(ref myTestInfo, ref myUUTData);
                        }
                        break;
                    case "TemperatureTest":
                        {
                            DoTempTest(ref myTestInfo, ref myUUTData, ref myCommonData);
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

        private void DoInitNistDatatable(ref TestInfo myTestInfo, ref UUTData myUUTData)
        {
            try
            {
                string sqlConStr = CalLeakConfigs.Default.SqlConString;
                using(SqlConnection conn = new SqlConnection(sqlConStr))
	            {
                    Trace.WriteLine("Connecting to SQL Database for NIST Data Retrieval");
                    conn.Open();
                    Trace.WriteLine("Connection succeeded");
                    // Get NIST Reference currently being used in the test system from app.config file
                    string model = CalLeakConfigs.Default.NistModel;
                    string serial = CalLeakConfigs.Default.NistSerial;
                    var nistData = SqlHelper.GetNistData(conn, model, serial);
                    Trace.WriteLine("NIST Std Leak Details:-");
                    Trace.WriteLine("     Model = " + model);
                    Trace.WriteLine("     Serial = " + serial);
                    Trace.WriteLine("     Cal Date = " + nistData.CalibrationDate.ToShortDateString());
                    Trace.WriteLine("     Cal Due Date = " + nistData.CalibrationDueDate.ToShortDateString());
                    Trace.WriteLine("     Leak_15Deg = " + nistData.Temp15Leak.ToString() + "std cc/s");
                    Trace.WriteLine("     Leak_16Deg = " + nistData.Temp16Leak.ToString() + "std cc/s");
                    Trace.WriteLine("     Leak_17Deg = " + nistData.Temp17Leak.ToString() + "std cc/s");
                    Trace.WriteLine("     Leak_18Deg = " + nistData.Temp18Leak.ToString() + "std cc/s");
                    Trace.WriteLine("     Leak_19Deg = " + nistData.Temp19Leak.ToString() + "std cc/s");
                    Trace.WriteLine("     Leak_20Deg = " + nistData.Temp20Leak.ToString() + "std cc/s");
                    Trace.WriteLine("     Leak_21Deg = " + nistData.Temp21Leak.ToString() + "std cc/s");
                    Trace.WriteLine("     Leak_22Deg = " + nistData.Temp22Leak.ToString() + "std cc/s");
                    Trace.WriteLine("     Leak_23Deg = " + nistData.Temp23Leak.ToString() + "std cc/s");
                    Trace.WriteLine("     Leak_24Deg = " + nistData.Temp24Leak.ToString() + "std cc/s");
                    Trace.WriteLine("     Leak_25Deg = " + nistData.Temp25Leak.ToString() + "std cc/s");
                    Trace.WriteLine("     Leak_26Deg = " + nistData.Temp26Leak.ToString() + "std cc/s");
                    Trace.WriteLine("     Leak_27Deg = " + nistData.Temp27Leak.ToString() + "std cc/s");
                    Trace.WriteLine("     Leak_28Deg = " + nistData.Temp28Leak.ToString() + "std cc/s");
                    Trace.WriteLine("     Leak_29Deg = " + nistData.Temp29Leak.ToString() + "std cc/s");
                    Trace.WriteLine("     Leak_30Deg = " + nistData.Temp30Leak.ToString() + "std cc/s");
                    if (nistData == null)
                    {
                        // problem getting NIST Data. --> FAIL
                        Trace.WriteLine("Unable to get NIST Data from SQL Database.");
                        myTestInfo.ResultsParams[1].Result = "FAIL";
                    }
                    else
                    {
                        Trace.WriteLine("NIST Data successfully transfered to local.");
                        myTestInfo.ResultsParams[1].Result = "PASS";
                    }

                    myNistData = nistData;
	            }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void DoTempTest(ref TestInfo myTestInfo, ref UUTData myUUTData, ref CommonData myCommonData)
        {
            try
            {
                int loop = int.Parse(myTestInfo.TestParams[1].Value);
                int delay = int.Parse(myTestInfo.TestParams[2].Value);
                double deltaSpec = double.Parse(myTestInfo.TestParams[3].Value);
                double deltaSpec2 = double.Parse(myTestInfo.TestParams[4].Value);
                int trial = Convert.ToInt32(myTestInfo.TestParams[5].Value);

                int slot = Convert.ToInt32(myCommonData.Slot);  // UUT Slot

                // Configure Thermocouple measurement
                Trace.WriteLine("Starting to initialize thermocouple and DUT's Temperature board.");
                InstrumentIO.ConfigureThermocouple(mySw);
                
                Trace.WriteLine("Measuring Thermocouple.");
                Trace.WriteLine("... waiting for the DUT's thermocouple to stabilized.");
                Trace.WriteLine("Loop 10 times");
                int counter = 0;
            Repeat: List<double> tempReadings = new List<double>(); 
                for (int i = 0; i < loop; i++)
                {
                    var dutTemp = InstrumentIO.MeasureCalLeakTemp(mySw, slot);
                    tempReadings.Add(dutTemp);
                    Trace.WriteLine("DUT Temperature: " + Math.Round(dutTemp, 2).ToString());
                    Thread.Sleep(delay);
                }
                Trace.WriteLine("DUT Average Temperature: " + tempReadings.Average().ToString());
                Trace.WriteLine("DUT Max Temperature: " + tempReadings.Max().ToString());
                Trace.WriteLine("DUT Min Temperature: " + tempReadings.Min().ToString());
                Trace.WriteLine("DUT Delta Temperature: " + (tempReadings.Max() - tempReadings.Min()).ToString());

                var delta = tempReadings.Max() - tempReadings.Min();
                counter++;

                if (delta > deltaSpec && counter < trial)
                    goto Repeat;

                // Measure Board Temperature
                Trace.WriteLine("Measuring DUT Board Temperature.");
                Trace.WriteLine("... waiting for the DUT's board's temperature to stabilized.");
                Trace.WriteLine("Loop 10 times");
                //InstrumentIO.SetupBoardTempMeasRoute(mySw, myScope, slot);   // setup route for BoardTemp supply and output 5V and TTL signal output
                int counter2 = 0;

            Repeat2: List<double> tempReadings2 = new List<double>(); 
                for (int i = 0; i < loop; i++)   // repeat 10times to make sure the temp is stable
                {
                    var boardTemp = InstrumentIO.MeasureBoardTemperature(mySw, myScope, slot);
                    boardTemp = Math.Round(boardTemp, 2);
                    tempReadings2.Add(boardTemp);
                    Trace.WriteLine("Board Temperature: " + boardTemp.ToString());
                    Thread.Sleep(delay);
                }
                Trace.WriteLine("Board Average Temperature: " + tempReadings2.Average().ToString());
                Trace.WriteLine("Board Max Temperature: " + tempReadings2.Max().ToString());
                Trace.WriteLine("Board Min Temperature: " + tempReadings2.Min().ToString());
                Trace.WriteLine("Board Delta Temperature: " + (tempReadings2.Max() - tempReadings2.Min()).ToString());
                var delta2 = tempReadings2.Max() - tempReadings2.Min();
                counter2++;
                if (delta2 > deltaSpec2 && counter2 < trial)
                    goto Repeat2;
                var boardTempFinal = InstrumentIO.MeasureBoardTemperature(mySw, myScope, slot);
                //InstrumentIO.DisconnectTempBoardMeasRoute(mySw, slot);


                // Save result
                if (counter == trial)   // more than 3 times try to get stable temperature, considered failed
                    myTestInfo.ResultsParams[1].Result = "TIMEOUT";
                else
                    myTestInfo.ResultsParams[1].Result = "PASS";
                //var averageTemperature = tempReadings.Average();
                var finalCanTemp = InstrumentIO.MeasureCalLeakTemp(mySw, 1);
                myTestInfo.ResultsParams[2].Result = Math.Round(delta, 3).ToString();
                myTestInfo.ResultsParams[3].Result = Math.Round(finalCanTemp, 1).ToString();

                myTestInfo.ResultsParams[4].Result = Math.Round(delta2, 3).ToString();
                //var boardTempAve = tempReadings2.Average();

                myTestInfo.ResultsParams[5].Result = Math.Round(boardTempFinal, 1).ToString();

                // Get Temperature difference between Can's temperature and board temperature. (Factor)
                double diffTemp = boardTempFinal - finalCanTemp;
                //myTestInfo.ResultsParams[6].Result = Math.Round(diffTemp, 3).ToString();

            }
            catch (Exception)
            {

                throw;
            }
        }

        private void DoNistLeakTempTest(ref TestInfo myTestInfo, ref UUTData myUUTData, ref CommonData myCommonData)
        {
            try
            {
                int loop = int.Parse(myTestInfo.TestParams[1].Value);
                int delay = int.Parse(myTestInfo.TestParams[2].Value);
                double deltaSpec = double.Parse(myTestInfo.TestParams[3].Value);
                int trial = Convert.ToInt32(myTestInfo.TestParams[4].Value);

                // Configure Thermocouple measurement
                InstrumentIO.ConfigureThermocouple(mySw);

                List<double> tempReadings = new List<double>();
                Trace.WriteLine("Measuring NIST Cal Leak Temperature.");
                Trace.WriteLine("... waiting for the NIST Cal-Leak's temperature to stabilized.");
                Trace.WriteLine("Loop 10 times");
                int counter = 0;
            Repeat: for (int i = 0; i < loop; i++)
                {
                    var nistTemp = InstrumentIO.MeasureCalLeakTemp(mySw, 0);
                    tempReadings.Add(nistTemp);
                    Trace.WriteLine("NIST Temperature: " + nistTemp.ToString());
                    Thread.Sleep(delay);
                }
                Trace.WriteLine("NIST Average Temperature: " + tempReadings.Average().ToString());
                Trace.WriteLine("NIST Max Temperature: " + tempReadings.Max().ToString());
                Trace.WriteLine("NIST Min Temperature: " + tempReadings.Min().ToString());
                Trace.WriteLine("NIST Delta Temperature: " + (tempReadings.Max() - tempReadings.Min()).ToString());

                var delta = tempReadings.Max() - tempReadings.Min();
                counter++;

                if (delta > deltaSpec && counter < trial)
                    goto Repeat;

                if (counter == trial)   // more than 3 times try to get stable temperature, considered failed
                {
                    myTestInfo.ResultsParams[1].Result = "TIMEOUT";
                }
                else
                {
                    var averageTemperature = tempReadings.Average();
                    myTestInfo.ResultsParams[1].Result = "PASS";
                    myTestInfo.ResultsParams[2].Result = Math.Round(delta, 3).ToString();
                    myTestInfo.ResultsParams[3].Result = averageTemperature.ToString();
                    //myTestInfo.ResultsParams[3].SpecMax = Convert.ToString(averageTemperature + deltaSpec);
                    //myTestInfo.ResultsParams[3].SpecMin = Convert.ToString(averageTemperature - deltaSpec);
                }

            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
