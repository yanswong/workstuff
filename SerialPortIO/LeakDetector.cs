using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using SerialPortIO.Properties;

namespace SerialPortIO
{
    public class VSLeakDetector
    {
        #region Members
        private SerialPort mySerialPort;
        private string _Terminator = "ok\r\n";
        private int _Timeout = 10;      // default timeout is 10 seconds
        private bool Success = false;
        private bool _enableTraceLog = false;
        #endregion

        #region Properties
        public StringBuilder Message { get; set; }
        public int Timeout
        { 
            get { return _Timeout; }
            set {_Timeout = value;}
        }
        public string Terminator
        {
            get { return _Terminator; }
            set { _Terminator = value; }
        }
        public bool EnableTraceLog { 
            get {return _enableTraceLog;}
            set { _enableTraceLog = value; }
        }
        #endregion

        public VSLeakDetector() 
            : this("COM1", 9600, Parity.None, 8, StopBits.One)
        { }

        public VSLeakDetector(string portName)
            : this(portName, 9600, Parity.None, 8, StopBits.One)
        { }

        public VSLeakDetector(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {
            mySerialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
            Message = new StringBuilder();
        }
        /// <summary>
        /// Open serial port communication once the System.IO.SerialPort instance instantiated 
        /// </summary>
        public void Open()
        {
            try
            {
                if (mySerialPort == null)
                    throw new Exception("Serial Port Instance must be created before opening the COM port");

                // Open the serialPort communication session
                mySerialPort.Open();

            }
            catch (Exception e)
            {
                if (_enableTraceLog) Trace.WriteLine(e.Message);
                throw e;
            }
        }

        /// <summary>
        /// Close the serial port communication
        /// </summary>
        public void Close()
        {
            try
            {
                if (mySerialPort != null)
                {
                    if (mySerialPort.IsOpen)
                        mySerialPort.Close();
                    mySerialPort = null;
                }
            }
            catch (Exception e)
            {
                if (_enableTraceLog) Trace.WriteLine(e.Message);
                throw e;
            }
        }

        public void Write(string command)
        {
            try
            {
                Write(command, true);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void Write(string command, bool clearPrevMsg)
        {
            try
            {
                if (mySerialPort == null) throw new Exception("RS232IO wasn't set to an instance of an object");
                if (!mySerialPort.IsOpen) Open();

                if (clearPrevMsg) Message.Clear();
                mySerialPort.Write(command + "\r");
            }
            catch (Exception)
            {

                throw;
            }
        }

        public string Read()
        {
            try
            {
                DateTime startTime = DateTime.Now;
                Thread.Sleep(10);
                DateTime currTime = DateTime.Now;
                StringBuilder sbMessage = new StringBuilder();
                while ((currTime - startTime).TotalSeconds < Timeout && 
                       !sbMessage.ToString().Contains(_Terminator) &&
                       !sbMessage.ToString().Contains("#?\r\n"))
                {
                    Thread.Sleep(100);
                    sbMessage.Append(mySerialPort.ReadExisting());
                    currTime = DateTime.Now;
                }

                // Now check whether the read method is timedout
                if ((currTime - startTime).TotalSeconds > Timeout)
                    throw new Exception(String.Format("Timeout attempting to read serial port. Message :\r\n {0}", sbMessage.ToString()));

                return sbMessage.ToString();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public string Query(string command)
        {
            try
            {
                Write(command, true);
                string retVal = Read();
                if (retVal.Contains(_Terminator))
                    return retVal.Split(new string[] { command }, StringSplitOptions.None).LastOrDefault()
                             .Split(new string[] { _Terminator }, StringSplitOptions.None).FirstOrDefault();
                else
                    return retVal;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public string Query(string command, ref bool success)
        {
            try
            {
                Write(command, true);
                string retVal = Read();
                if (retVal.Contains(_Terminator))
                {
                    success = true;
                    return retVal.Split(new string[] { command }, StringSplitOptions.None).LastOrDefault()
                             .Split(new string[] { _Terminator }, StringSplitOptions.None).FirstOrDefault();
                }
                else
                {
                    success = false;
                    return retVal;
                }
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        /// <summary>
        /// Reports the status of AutoZero enabled/disabled
        /// </summary>
        /// <returns>true = enabled, false = disabled</returns>
        public bool IsAutozeroEnabled()
        {
            try
            {
                string retVal = Query(Resources.AutoZeroStatus, ref Success);
                if (Success)
                {
                    if (retVal.ToUpper().Contains("ON"))
                        return true;
                    else if (retVal.ToUpper().Contains("OFF"))
                        return false;
                    else
                        throw new Exception("Unable to check AutoZero status from device.");
                }
                else
                {
                    throw new Exception(String.Format("Error when sending command '{0}'", Resources.AutoZeroStatus));
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// Reports the status of the last calibration
        /// </summary>
        /// <returns>No,Yes,???</returns>
        public string GetLastCalibrationStatus()
        {
            try
            {
                string retVal = Query(Resources.LastCalStatus, ref Success);
                if (Success)
                {
                    return retVal;
                }
                else
                {
                    throw new Exception(String.Format("Error when sending command '{0}'", Resources.LastCalStatus));
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// Reports the currently stored contra-flow mode crossover pressure.
        /// </summary>
        /// <returns>pressure value</returns>
        public double GetContraFlowXOverPressure()
        {
            try
            {
                string retVal = Query(Resources.ContraModeXoverPressure, ref Success);
                if (Success)
                    return Convert.ToDouble(retVal);
                else
                    throw new Exception(String.Format("Error when sending command '{0}'", Resources.ContraModeXoverPressure));
            }
            catch (Exception)
            {

                throw;
            }
        }
        
        /// <summary>
        /// Reports the device current date and time in 'MM/dd/yyyy HH:mm:ss.ff' format
        /// </summary>
        /// <returns></returns>
        public DateTime GetDateTime()
        {
            try
            {
                string retVal = Query(Resources.DateTime, ref Success);
                string dateTime = String.Format("{0} {1}", retVal.Trim().Split(' ')[1], retVal.Trim().Split(' ')[2]);
                if (Success)
                    return DateTime.ParseExact(dateTime, "MM/dd/yyyy HH:mm:ss.ff", System.Globalization.CultureInfo.InvariantCulture);
                else
                    throw new Exception("Error when sending command " + Resources.DateTime);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public double GetGain()
        {
            try
            {
                string retVal = Query(Resources.Gain, ref Success);
                if (Success)
                    return Convert.ToDouble(retVal);
                else
                    throw new Exception("Error when sending command " + Resources.Gain);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public double[] GetOffset()
        {
            try
            {
                string command = Resources.Offset;
                string retVal = Query(command, ref Success);
                if (Success)
                {
                    List<double> readings = new List<double>();
                    foreach (var reading in retVal.Split(new string[] {" "}, StringSplitOptions.RemoveEmptyEntries))
                    {
                        readings.Add(Convert.ToDouble(reading));
                    }

                    return readings.ToArray();
                }
                else
                    throw new Exception("Error when sending command " + command);
            }
            catch (Exception)
            {

                throw;
            }
        }


        public double GetIonChamber()
        {
            try
            {
                string command = Resources.IonChamber;
                string retVal = Query(command, ref Success);
                if (Success)
                    return Convert.ToDouble(retVal);
                else
                    throw new Exception("Error when sending command " + command);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public double GetEmission()
        {
            try
            {
                string command = Resources.Emission;
                string retVal = Query(command, ref Success);
                if (Success)
                    return Convert.ToDouble(retVal);
                else
                    throw new Exception("Error when sending command " + command);
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// Get VGain array value
        /// </summary>
        /// <returns>An array with first value = fast VGain, and the second value = low VGain</returns>
        public double[] GetVGain()
        {
            try
            {
                string command = Resources.VGain;
                string retVal = Query(command, ref Success);
                if (Success)
                {
                    List<double> readings = new List<double>();
                    foreach (var reading in retVal.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        readings.Add(Convert.ToDouble(reading));
                    }

                    return readings.ToArray();
                }
                else
                    throw new Exception("Error when sending command " + command);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public string GetValveStatus()
        {
            try
            {
                string command = Resources.ValveState;
                string retVal = Query(command, ref Success);
                if (Success)
                {
                    return Convert.ToString(retVal);
                }
                else
                    throw new Exception("Error when sending command " + command);

            }
            catch (Exception)
            {

                throw;
            }
        }

        public void UseExtCalLeakRef()
        {
            try
            {
                string command = Resources.ExternalLeakStd;
                string retVal = Query(command);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void SetExtCalLeakValue(string leakRateScientificString)
        {
            try
            {
                string command = string.Format("{0} {1}", leakRateScientificString, Resources.InitExtLeak);
                string retVal = Query(command);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public string GetExternalLeakValue()
        {
            try
            {
                string command = Resources.ReadExtLeakVal;
                string retVal = Query(command, ref Success);
                if (Success)
                {
                    return Convert.ToString(retVal);
                }
                else
                    throw new Exception("Error when sending command " + command);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public string Rough()
        {
            try
            {
                
                string command = Resources.Rough;
                string retVal = Query(command, ref Success);
                if (Success)
                {
                    return Convert.ToString(retVal);
                }
                else
                    throw new Exception("Error when sending command " + command);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public string Hold()
        {
            try
            {
                string command = Resources.Hold;
                string retVal = Query(command, ref Success);
                if (Success)
                {
                    return Convert.ToString(retVal);
                }
                else
                    throw new Exception("Error when sending command " + command);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public string Zero()
        {
            try
            {
                string command = Resources.Zero;
                string retVal = Query(command, ref Success);
                if (Success)
                {
                    return Convert.ToString(retVal);
                }
                else
                    throw new Exception("Error when sending command " + command);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public bool WaitForContraFlow(ref string retVal)
        {
            try
            {
                Trace.WriteLine("Waiting for ContraFlow state...");
                string command = Resources.ValveState;
                int counter = 0;
                Repeat: retVal = Query(command, ref Success);
                if (Success)
                {
                    counter++;
                    if (retVal.Trim().ToLower() != "contraflow" && counter <= 10)
                    {
                        Thread.Sleep(1000);
                        goto Repeat;
                    }
                    else if (counter > 25)
                    {
                        Trace.WriteLine("Timeout waiting for ContraFlow state");
                        return false;
                    }
                    else if (retVal.Trim().ToLower() == "contraflow")
                    {
                        Trace.WriteLine("ContraFlow state.");
                        return true;
                    }
                    else
                        return false;
                }
                else
                    throw new Exception("Error when sending command " + command);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public bool WaitForFineTest(ref string retVal)
        {
            try
            {
                Trace.WriteLine("Waiting for FineTest or MIDSTAGE state...");
                Thread.Sleep(5000);
                string command = Resources.ValveState;
                int counter = 0;
            Repeat: retVal = Query(command, ref Success);
                if (Success)
                {
                    Trace.WriteLine(retVal);
                    counter++;
                    if (retVal.Trim().ToLower() != "midstage" && counter <= 60)
                    {
                        Thread.Sleep(1000);
                        goto Repeat;
                    }
                    else if (counter > 60)
                    {
                        Trace.WriteLine("Timeout waiting for MIDSTAGE state");
                        return false;
                    }
                    else if (retVal.Trim().ToLower() == "midstage")
                    {
                        Trace.WriteLine("MIDSTAGE state.");
                        return true;
                    }
                    else
                        return false;
                }
                else
                    throw new Exception("Error when sending command " + command);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public bool WaitForStabilizedReading(ref string retVal)
        {
            try
            {
                Trace.WriteLine("Waiting for stabilized reading...");
                bool stabilized = false;
                var startTime = DateTime.Now;
                var elapsedTime = DateTime.Now - startTime;
                do
                {
                    var readingBefore = Convert.ToDouble(ReadLeakRate());
                    Trace.WriteLine("Reading Before = " + readingBefore.ToString());
                    Thread.Sleep(10000);
                    var readingAfter = Convert.ToDouble(ReadLeakRate());
                    Trace.WriteLine("Reading After = " + readingAfter.ToString());
                    var pctDiff = 0.0;
                    if (readingAfter < readingBefore)
                    {
                        pctDiff = readingAfter / readingBefore;
                    }
                    else
                    {
                        pctDiff = readingBefore / readingAfter;
                    }
                    Trace.WriteLine("Percentage difference = " + (pctDiff * 100).ToString() + "%");
                    if (pctDiff > 0.95)
                    {
                        stabilized = true;
                        return true;
                    }

                    elapsedTime = DateTime.Now - startTime;

                } while (!stabilized && elapsedTime.TotalSeconds < 120);

                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public string Calibrate()
        {
            try
            {
                int timeout = 300;   // default cal timeout
                return Calibrate(timeout);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public string Calibrate(int timeout)
        {
            try
            {
                Trace.WriteLine("Calibration started.");
                string command = Resources.Calibrate;
                string retVal = Query(command, ref Success);
                if (Success)
                {
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();
                    string cmd2 = Resources.ValveState;
                    string calState = Query(cmd2, ref Success);
                    bool isCalStarted = false;
                    bool isZeroStarted = false;
                    bool isRoughingStarted = false;
                    bool isMidStageStarted = false;
                Repeat: ;
                    if (!isCalStarted)
                    {
                        calState = Query(cmd2, ref Success);
                        Trace.WriteLine(string.Format("Calibration Status    =    {0}", calState));
                        Thread.Sleep(5000);
                        if (stopWatch.Elapsed.TotalSeconds > timeout) return "TIMEOUT";
                        if (calState.ToLower().Trim() == "calibrate") isCalStarted = true;
                        goto Repeat;
                    }
                RepeatZero: ;
                    if (!isZeroStarted)
                    {
                        calState = Query(cmd2, ref Success);
                        Trace.WriteLine(string.Format("Calibration Status    =    {0}", calState));
                        Thread.Sleep(5000);
                        if (stopWatch.Elapsed.TotalSeconds > timeout) return "TIMEOUT";
                        if (calState.ToLower().Trim() == "zeroing") isZeroStarted = true;
                        goto RepeatZero;
                    }
                RepeatRough: ;
                    if (!isRoughingStarted)
                    {
                        calState = Query(cmd2, ref Success);
                        Trace.WriteLine(string.Format("Calibration Status    =    {0}", calState));
                        Thread.Sleep(5000);
                        if (stopWatch.Elapsed.TotalSeconds > timeout) return "TIMEOUT";
                        if (calState.ToLower().Trim() == "roughing") isRoughingStarted = true;
                        goto RepeatRough;
                    }
                    calState = Query(cmd2, ref Success);
                RepeatMidstage: ;
                    if (!isMidStageStarted)
                    {
                        //calState = Query(cmd2, ref Success);
                        //Trace.WriteLine(string.Format("Calibration Status    =    {0}", calState));
                        //Thread.Sleep(5000);
                        //if (stopWatch.Elapsed.TotalSeconds > timeout) return "TIMEOUT";
                        //if (calState.ToLower().Trim() == "midstage") isMidStageStarted = true;
                        //goto RepeatMidstage;
                        isMidStageStarted = WaitForFineTest(ref calState);
                        if (stopWatch.Elapsed.TotalSeconds > timeout) return "TIMEOUT";
                    }

                    stopWatch.Stop();
                    return Convert.ToString(retVal);
                }
                else
                    throw new Exception("Error when sending command " + command);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public string ReadLeakRate()
        {
            try
            {
                string command = Resources.LeakRate;
                string retVal = Query(command, ref Success);
                if (Success)
                {
                    return Convert.ToString(retVal);
                }
                else
                    throw new Exception("Error when sending command " + command);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public bool CalIsOK()
        {
            try
            {
                string command = Resources.IsCalOK;
                string retVal = Query(command, ref Success);
                if (Success)
                {
                    if (retVal.Trim().ToLower() == "no")
                        return false;
                    else
                        return true;
                }
                else
                    throw new Exception("Error when sending command " + command);
            }
            catch (Exception)
            {


                throw;
            }
        }

        public string NoSniff()
        {
            try
            {
                Trace.WriteLine("Set LeakDetector to NOSNIFF.");
                string command = Resources.NoSniff;
                string retVal = Query(command, ref Success);
                if (Success)
                {
                    return Convert.ToString(retVal);
                }
                else
                    throw new Exception("Error when sending command " + command);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public string UnlockFullCommand()
        {
            try
            {
                string command = Resources.UnlockFullCommand;
                string retVal = Query(command, ref Success);
                if (Success)
                {
                    return Convert.ToString(retVal);
                }
                else
                    throw new Exception("Error when sending command " + command);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public string Vent()
        {
            try
            {
                string command = Resources.Vent;
                string retVal = Query(command, ref Success);
                if (Success)
                {
                    return Convert.ToString(retVal);
                }
                else
                    throw new Exception("Error when sending command " + command);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void UseIntCalLeak()
        {
            try
            {
                
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void UseExtCalLeak()
        {
            try
            {

            }
            catch (Exception)
            {

                throw;
            }
        }

        
    }
}
