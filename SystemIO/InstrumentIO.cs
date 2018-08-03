using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Agilent.TMFramework.InstrumentIO;
using System.Threading;
using System.Diagnostics;

namespace SystemIO
{
    public static class InstrumentIO
    {
        public static void ConfigureThermocouple(DirectIO mySw)
        {
            try
            {
                mySw.WriteLine("ROUTE:SCAN (@)");
                mySw.WriteLine("CONF:TEMP TC,J,(@1001:1005)");
                mySw.WriteLine("TEMP:TRAN:TC:RJUN:TYPE INT,(@1001:1005)");
                mySw.WriteLine("ROUTE:SCAN (@1001:1005)");
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static double MeasureCalLeakTemp(DirectIO mySw, int port)
        {
            try
            {
                mySw.WriteLine("INIT");
                mySw.WriteLine("FETCH?");
                var tempReadings = mySw.ReadList().Cast<double>().ToList();
                var UUTTemp = tempReadings[port];
                return UUTTemp;
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// Method to open electromagnetic valve.
        /// </summary>
        /// <param name="port">Integer 0 for NIST test valve. Integer 1,2,3 and 4 for Port1,Port2,Port3 and Port4 test valve respectively</param>
        public static void OpenTestValve(DirectIO mySw, int port)
        {
            try
            {
                string switchChannel = string.Empty;
                switch (port)
                {
                    case 0: switchChannel = "2002"; break;
                    case 1: switchChannel = "2021"; break;
                    case 2: switchChannel = "2022"; break;
                    case 3: switchChannel = "2023"; break;
                    case 4: switchChannel = "2024"; break;
                    default:
                        break;
                }

                mySw.WriteLine("ROUTE:CLOSE (@{0})", switchChannel);    // close means activate the 34937A SPDT Switch. Normally open port will now closed
                
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// Method to close electromagnetic valve.
        /// </summary>
        /// <param name="port">Integer 0 for NIST test valve. Integer 1,2,3 and 4 for Port1,Port2,Port3 and Port4 test valve respectively</param>
        public static void CloseTestValve(DirectIO mySw, int port)
        {
            try
            {
                string switchChannel = string.Empty;
                switch (port)
                {
                    case 0: switchChannel = "2002"; break;
                    case 1: switchChannel = "2021"; break;
                    case 2: switchChannel = "2022"; break;
                    case 3: switchChannel = "2023"; break;
                    case 4: switchChannel = "2024"; break;
                    default:
                        break;
                }

                mySw.WriteLine("ROUTE:OPEN (@{0})", switchChannel);     // open means de-activate the 34937A SPDT Switch. Normally open port will go back to open state.

            }
            catch (Exception)
            {

                throw;
            }
        }

        public static void CloseAllTestValve(DirectIO mySw)
        {
            try
            {
                mySw.WriteLine("ROUTE:OPEN (@2002,2021:2024)");
            }
            catch (Exception)
            {

                throw;
            }
        }


        public static void SetupBoardTempMeasRoute(DirectIO mySw, DirectIO myScope, int port)
        {
            try
            {
                // Disconnect 5v supply from the temp-board, and the load.
                mySw.WriteLine("ROUTE:OPEN (@5001:5040)");
                // then connect to the desired port to measure board temperature
                mySw.WriteLine("ROUTE:CLOSE (@500{0},502{0})", port);
                // autoscale Oscilloscope to get measurement
                myScope.WriteLine(":AUToscale CHANNEL{0}", port);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static void DisconnectTempBoardMeasRoute(DirectIO mySw, int port)
        {
            try
            {
                // Disconnect switches
                mySw.WriteLine("ROUTE:OPEN (@500{0},502{0})", port);
                Thread.Sleep(500);
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mySw"></param>
        /// <param name="myScope"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static double MeasureBoardTemperature(DirectIO mySw, DirectIO myScope, int port)
        {
            try
            {
                // Now measure the period T1 and T2 to calculate the temperature
                //myScope.WriteLine(":AUToscale CHANNEL{0}", port);
                myScope.WriteLine(":MEASure:DUTYcycle? CHANNEL{0}", port);
                double dutyCycle = myScope.ReadNumberAsDouble();
                myScope.WriteLine(":MEASure:PERiod? CHANNEL{0}", port);
                double period = myScope.ReadNumberAsDouble();       // T1 + T2
                myScope.WriteLine(":MARKer:MODE MEASurement");

                double t1 = dutyCycle / 100 * period;
                double t2 = period - t1;

                // formula for temp board.
                double temperature = 235 - (400 * t1 / t2);

                return temperature;
            }
            catch (Exception)
            {

                throw;
            }
        }

    }
}
