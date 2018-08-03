using CalLeakTest.Repositories;
using PluginSequence;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CalLeakTest.MfgTests
{
    public class ButtonUp
    {
        public void Execute(ref TestInfo myTestInfo, ref UUTData myUUTData, ref CommonData myCommonData, List<double> listOfLeakRate, List<double> listOfTempFactor, List<double> listOfTemp)
        {
            try
            {
                var slope = Convert.ToDouble(myTestInfo.TestParams[1].Value);
                var refTemp = Convert.ToDouble(myTestInfo.TestParams[2].Value);

                switch (myTestInfo.TestLabel)
                {
                    case "FinalizeLeakAndFactor":
                        {
                            Trace.WriteLine(string.Format("Total {0} measurements for DUT Leak Rate:-", listOfLeakRate.Count()));
                            for (int i = 0; i < listOfLeakRate.Count(); i++) Trace.WriteLine(String.Format("     Leak Rate #{0} = {1} std cc/s", i + 1, listOfLeakRate[i]));
                            var averageLeak = listOfLeakRate.Average();
                            Trace.WriteLine("Average Leak Rate = " + averageLeak.ToString());
                            var averageLeakSci = Math.Round(averageLeak, 9);
                            Trace.WriteLine("Average Leak Rate = " + averageLeakSci.ToString());

                            Trace.WriteLine(string.Format("Total {0} measurements for DUT Temperature Factor:-", listOfTempFactor.Count()));
                            for (int i = 0; i < listOfTempFactor.Count(); i++) Trace.WriteLine(String.Format("     Temp Factor #{0} = {1} deg C", i + 1, listOfTempFactor[i]));
                            var averageFactor = listOfTempFactor.Average();
                            Trace.WriteLine("Average Temp Factor = " + averageFactor.ToString());
                            var averageFactorSci = Math.Round(averageFactor, 2);
                            Trace.WriteLine("Average Temp Factor = " + averageFactorSci.ToString());

                            Trace.WriteLine(string.Format("Total {0} measurements for DUT Temperature :-", listOfTemp.Count()));
                            for (int i = 0; i < listOfTemp.Count(); i++) Trace.WriteLine(String.Format("     Temperature #{0} = {1} deg C", i + 1, listOfTemp[i]));
                            var averageTemp = listOfTemp.Average();
                            Trace.WriteLine("Average Temp = " + averageTemp.ToString());
                            var averageTempSci = Math.Round(averageTemp, 2);
                            Trace.WriteLine("Average Temp = " + averageTempSci.ToString());

                            Trace.WriteLine(string.Format("Calculating DUT Normalized Leak Rate at {0} Deg Celcius reference temperature", refTemp));
                            var normLeakRateDouble = TestHelper.CalculateNormLeakRate(slope, refTemp, averageLeak, averageTemp);
                            Trace.WriteLine("Normalized Leak Rate = " + normLeakRateDouble.ToString());


                            myTestInfo.ResultsParams[1].Result = averageLeakSci.ToString();
                            myTestInfo.ResultsParams[2].Result = Math.Round(averageFactorSci, 1).ToString();
                            myTestInfo.ResultsParams[3].Result = Math.Round(averageTempSci, 1).ToString();
                            myTestInfo.ResultsParams[4].Result = Math.Round(normLeakRateDouble, 9).ToString();
                        }
                        break;
                    case "SubmitSerialNumber":
                        {

                        }
                        break;
                    case "SubmitTestData":
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
    }
}
