using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PluginSequence;
using System.Windows.Forms;
using System.IO;
using CalLeakTest.MfgTests;
using CalLeakTest.Models;
using Agilent.TMFramework.InstrumentIO;
using SerialPortIO;
using System.Diagnostics;
using SystemIO;
using CalLeakTest.Repositories;
using System.Data.SqlClient;

namespace CalLeakTest
{
    public class PerformanceTestManager : ITest
    {
        #region Global Vars
        NistData myNistData;    // global variable to be used in perf tests
        UUTData g_UUTData;
        CommonData g_CommonData;
        CalLeakData myCalLeakData;
        #endregion

        #region TestInstances
        InitStation myInitStationTest;
        SystemCal mySystemCalTest;
        DUTTest myCalLeakTest;
        ButtonUp myButtonUpTest;
        SysMaintenance mySysMaintenance;
        #endregion

        #region DirectIO
        DirectIO mySw;      // DirectIO for 34980A Multifunction Switch/Measure Unit
        DirectIO myScope;
        VSLeakDetector myLD; 
        #endregion

        #region Forms
        FormNewUUT myUutForm;
        #endregion

        #region Test Vars
        List<double> ListOfUUTLeakRate;
        List<double> ListOfUUTTempFactor;
        List<double> ListOfUUTTemp;
        int TestPort;
        #endregion
        /// <summary>
        /// This method is called when the TestExec is loaded. User can customize PlugIn objects before 
        /// any event or action during test setup or test run
        /// </summary>
        /// <param name="_uutData"></param>
        /// <param name="_commonData"></param>
        public void TestExecLoaded(ref UUTData _uutData, ref CommonData _commonData)
        {
            try
            {

            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// This method is called when user clicks NewUUT button in case user want to customize the 
        /// Plugin objects
        /// </summary>
        /// <param name="_uutData"></param>
        /// <param name="_commonData"></param>
        public void UserBegins(ref UUTData _uutData, ref CommonData _commonData)
        {
            try
            {
                // For cal leak, Automated serial number generation is needed.
                // Therefore TestExecutive UUT information will be provided from user code (here)

                if (_uutData == null)
                    myUutForm = new FormNewUUT();

                myUutForm.isProdMode = _commonData.isProdMode;


                // Call user form
                myUutForm.ShowDialog();
                if (DialogResult.OK == myUutForm.dialogResult)
                {
                    _uutData = new UUTData();
                    _uutData.Model = myUutForm.ModelNumber;
                    _uutData.SerNum = myUutForm.serialNumber;
                    _uutData.Options = myUutForm.OptionNumber;
                    this.TestPort = myUutForm.TestPort;

                    _commonData = new CommonData();
                    _commonData.Slot = this.TestPort.ToString();
                    _commonData.Testplan = Path.Combine(@"C:\CalLeakTest.PRJ\Sequences", _uutData.Model + ".xml");

                    this.g_UUTData = _uutData;
                    this.g_CommonData = _commonData;
                }
                else if (myUutForm.dialogResult == DialogResult.Cancel)
                {
                    if (_commonData != null)
                        _commonData.Mode = "Abort";
                    else
                    {
                        _commonData = new CommonData();
                        _commonData.Mode = "Abort";
                    }
                    Trace.WriteLine("New UUT input aborted.");
                }



            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// This method is called when the Performance test is started
        /// </summary>
        /// <param name="_uutData"></param>
        /// <param name="_commonData"></param>
        public void UserSetups(ref UUTData _uutData, ref CommonData _commonData)
        {
            try
            {
                if (_uutData == null) _uutData = this.g_UUTData;
                if (_commonData == null) _commonData = this.g_CommonData;


                // Initialize Test Equipments
                mySw = new DirectIO("GPIB0::9::INSTR"); mySw.Timeout = 10000;
                myScope = new DirectIO("GPIB0::7::INSTR"); myScope.Timeout = 10000;
                //myScope = new DirectIO("USB0::2391::6056::MY53280249::0::INSTR"); myScope.Timeout = 10000;
                
                
                //ResourceManager rmc = new ResourceManager();
                //var rs = rmc.FindRsrc("USB?*");
                //foreach (var item in rs)
                //{
                //    Trace.WriteLine(item);
                //}
                


                string comPort = CalLeakConfigs.Default.ComPort;
                myLD = new VSLeakDetector(comPort, 9600, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One);
                myLD.UnlockFullCommand();
                myLD.NoSniff();     // Set the LD HighPressure test to OFF



                InstrumentIO.SetupBoardTempMeasRoute(mySw, myScope, TestPort);
                

                // Initialize list
                ListOfUUTLeakRate = new List<double>();
                ListOfUUTTempFactor = new List<double>();
                ListOfUUTTemp = new List<double>();

                // Initialize CalLeakData
                myCalLeakData = new CalLeakData();
                myCalLeakData.PartNumber = _uutData.Model;
                myCalLeakData.SerialNumber = _uutData.SerNum;
                myCalLeakData.TestPortId = Convert.ToInt32(_commonData.Slot);
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// This method is called at the beginning of each test cases
        /// </summary>
        /// <param name="_uutData"></param>
        /// <param name="_commonData"></param>
        public void TestSetup(ref TestInfo myTestInfo, ref UUTData _uutData, ref CommonData _commonData)
        {
            try
            {
                if (_commonData.IsFailureExist) return;

                switch (myTestInfo.GroupLabel)
                {
                    case "InitStation":
                        {
                            if (myInitStationTest == null)
                            {
                                myInitStationTest = new InitStation();
                                myInitStationTest.mySw = this.mySw;
                                myInitStationTest.myScope = this.myScope;
                                myInitStationTest.myNistData = this.myNistData;
                                myInitStationTest.myLD = this.myLD;
                            }

                        }
                        break;
                    case "SystemCalibration1":
                    case "SystemCalibration2":
                    case "SystemCalibration3":
                        {
                            if (mySystemCalTest == null)
                            {
                                mySystemCalTest = new SystemCal();
                                mySystemCalTest.mySw = this.mySw;
                                mySystemCalTest.myScope = this.myScope;
                                mySystemCalTest.myNistData = this.myNistData;
                                mySystemCalTest.myLD = this.myLD;
                            }
                        }
                        break;
                    case "CalLeakTest1":
                    case "CalLeakTest2":
                    case "CalLeakTest3":
                        {
                            if (myCalLeakTest == null)
                            {
                                myCalLeakTest = new DUTTest();
                                myCalLeakTest.mySw = this.mySw;
                                myCalLeakTest.myScope = this.myScope;
                                myCalLeakTest.myNistData = this.myNistData;
                                myCalLeakTest.myLD = this.myLD;
                            }
                        }
                        break;
                    case "ButtonUp":
                        {
                            if (myButtonUpTest == null)
                            {
                                myButtonUpTest = new ButtonUp();
                            }
                        }
                        break;
                    case "SystemMaintenance":
                        {
                            if (mySysMaintenance == null)
                            {
                                mySysMaintenance = new SysMaintenance();
                                mySystemCalTest.mySw = this.mySw;
                                mySystemCalTest.myScope = this.myScope;
                                mySystemCalTest.myNistData = this.myNistData;
                                mySystemCalTest.myLD = this.myLD;
                            }
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

        /// <summary>
        /// This method is called to execute performance testing of the UUT for each test cases
        /// </summary>
        /// <param name="myTestInfo"></param>
        /// <param name="myUUTData"></param>
        /// <param name="myCommonData"></param>
        /// <returns></returns>
        public TestInfo DoTests(TestInfo myTestInfo, ref UUTData myUUTData, ref CommonData myCommonData)
        {
            try
            {
                if (myCommonData.IsFailureExist) return myTestInfo;

                switch (myTestInfo.GroupLabel)
                {
                    case "InitStation":
                        {
                            myInitStationTest.Execute(ref myTestInfo, ref myUUTData, ref myCommonData);
                            myNistData = myInitStationTest.myNistData;  // store NIST Data
                                                                        //YS Wong added to stop if fail 26 Jul 2018
                            if (myCommonData.IsFailureExist)
                            {
                                MessageBox.Show("Fail test", "Fail", MessageBoxButtons.OK);
                                myCommonData.Mode = "Abort";
                            }
                        }
                        break;
                    case "SystemCalibration1":
                    case "SystemCalibration2":
                    case "SystemCalibration3":
                        {
                            mySystemCalTest.Execute(ref myTestInfo, ref myUUTData, ref myCommonData);
                            if (myCommonData.IsFailureExist)
                            {
                                MessageBox.Show("Fail test", "Fail", MessageBoxButtons.OK);
                                myCommonData.Mode = "Abort";
                            }
                        }
                        break;
                    case "CalLeakTest1":
                    case "CalLeakTest2":
                    case "CalLeakTest3":
                        {
                            myCalLeakTest.Execute(ref myTestInfo, ref myUUTData, ref myCommonData);
                            if (myCommonData.IsFailureExist)
                            {
                                MessageBox.Show("Fail test", "Fail", MessageBoxButtons.OK);
                                myCommonData.Mode = "Abort";
                            }
                        }
                        break;
                    case "ButtonUp":
                        {
                            myButtonUpTest.Execute(ref myTestInfo, ref myUUTData, ref myCommonData, ListOfUUTLeakRate, ListOfUUTTempFactor, ListOfUUTTemp);
                            if (myCommonData.IsFailureExist)
                            {
                                MessageBox.Show("Fail test", "Fail", MessageBoxButtons.OK);
                                myCommonData.Mode = "Abort";
                            }
                        }
                        break;
                    case "SystemMaintenance":
                        {
                            mySysMaintenance.Execute(ref myTestInfo, ref myUUTData, ref myCommonData);
                            if (myCommonData.IsFailureExist)
                            {
                                MessageBox.Show("Fail test", "Fail", MessageBoxButtons.OK);
                                myCommonData.Mode = "Abort";
                            }
                        }
                        break;
                    default:
                        break;
                }
                return myTestInfo;
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// This method is called at the end of each test cases
        /// </summary>
        /// <param name="_uutData"></param>
        /// <param name="_commonData"></param>
        public void TestCleanup(ref TestInfo myTestInfo, ref UUTData _uutData, ref CommonData _commonData)
        {
            try
            {
                //YS Wong added to stop if fail 26 Jul 2018
                if (_commonData.IsFailureExist)  return;
                
                switch (myTestInfo.GroupLabel)
                {
                    case "InitStation":
                        {
                            switch (myTestInfo.TestLabel)
                            {
                                case "NistLeakTempTest":
                                    {
                                        myCalLeakData.NISTCalDate = myNistData.CalibrationDate;
                                        myCalLeakData.NISTCalDueDate = myNistData.CalibrationDueDate;
                                        myCalLeakData.NISTPartNumber = myNistData.Model;
                                        myCalLeakData.NISTSerialNumber = myNistData.Serial;
                                        myCalLeakData.NISTDescription = CalLeakConfigs.Default.NistDescription;
                                        myCalLeakData.NISTReportNumber = CalLeakConfigs.Default.NistReportNumber;
                                    }
                                    
                                    break;
                                default: break;
                            }
                        }
                        break;
                    case "SystemCalibration":
                        {
                           
                        }
                        break;
                    case "CalLeakTest1":
                    case "CalLeakTest2":
                    case "CalLeakTest3":
                        {
                            ListOfUUTLeakRate.Add(myCalLeakTest.UUTLeakRate);
                            ListOfUUTTempFactor.Add(myCalLeakTest.UUTTempFactor);
                            ListOfUUTTemp.Add(myCalLeakTest.UUTTemp);
                        }
                        break;
                    case "ButtonUp":
                        {
                            switch (myTestInfo.TestLabel)
                            {
                                case "FinalizeLeakAndFactor":
                                    {
                                        myCalLeakData.Factor = Convert.ToDouble(myTestInfo.ResultsParams[2].Result);
                                        myCalLeakData.UUTTemp = Convert.ToDouble(myTestInfo.ResultsParams[3].Result);
                                        myCalLeakData.LeakRate = Convert.ToDouble(myTestInfo.ResultsParams[4].Result);
                                        myCalLeakData.BoardTemp = Math.Round(myCalLeakData.UUTTemp + myCalLeakData.Factor, 1);
                                        myCalLeakData.TestDate = _commonData.StartTime;
                                        myCalLeakData.TestedBy = _commonData.Tester;
                                        myCalLeakData.StationSerialNumber = CalLeakConfigs.Default.StationSerialNumber;
                                        myCalLeakData.StationDescription = CalLeakConfigs.Default.StationDescription;
                                        myCalLeakData.StationModelNumber = CalLeakConfigs.Default.StationModelNumber;
                                        myCalLeakData.StationReportNumber = CalLeakConfigs.Default.StationReportNumber;
                                        myCalLeakData.StationCalDate = DateTime.Now;
                                        myCalLeakData.StationCalDueDate = DateTime.Now;
                                    }
                                    break;
                                default: break;
                            }
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

        /// <summary>
        /// This method is called when the test is completed
        /// </summary>
        /// <param name="_uutData"></param>
        /// <param name="_commonData"></param>
        public void UserCleanup(ref UUTData _uutData, ref CommonData _commonData)
        {
            try
            {
                // Disconnect 5V supply and TTL signal from the Sensor Board
                InstrumentIO.DisconnectTempBoardMeasRoute(mySw, TestPort);
                InstrumentIO.CloseAllTestValve(mySw);

                if (myLD != null)
                {
                    // at the end of the test, vent the Leak Detector
                    //Trace.WriteLine("Vent the Leak Detector");
                    //myLD.Vent();
                    Trace.WriteLine("Disconnecting Serial Port Communication....");
                    myLD.Close();
                    myLD = null;
                    Trace.WriteLine("   ... disconnected");
                }

                // Destruct the Test Classes
                myInitStationTest = null;
                myCalLeakTest = null;
                mySystemCalTest = null;
                myButtonUpTest = null;

                // Check FinalTest outcome, Pass/Fail flag P,F,A,E and the test is in Production Mode
                if (_commonData.TestOutcome.ToUpper() == "P" && _commonData.isProdMode)
                {
                    Trace.WriteLine("Submitting Cal-Leak data summary to database...");
                    string sqlConStr = CalLeakConfigs.Default.SqlConString;
                    myCalLeakData.IsPass = true;    // Pass
                    using (SqlConnection conn = new SqlConnection(sqlConStr))
                    {
                        conn.Open();
                        var retVal = SqlHelper.SubmitFinalCalLeakData(conn, ref myCalLeakData);
                        Trace.WriteLine("CalLeak Data transfered to database.");

                        // Create certificate history for the unit
                        int id = Convert.ToInt32(retVal);
                        string eqIds = SqlHelper.GetCsvStringEquipmentUsed(conn, 2, 1);

                        // save UUT certificate details
                        SqlHelper.SaveCertDetails(conn, id, eqIds);
                    }

                    Trace.WriteLine("Generating new Serial Number...");
                    // Autogenerate Serial Number
                    
                    string modelNumber = _uutData.Model;
                    string serialNumber = string.Empty;
                    DateTime todayDate = DateTime.Now;
                    int workWeek = DateTimeHelper.GetIso8601WeekOfYear(todayDate);
                    SernumInfo mySerialNumInfo = new SernumInfo();
                    mySerialNumInfo.Model = modelNumber;
                    using (SqlConnection conn = new SqlConnection(sqlConStr))
                    {
                        Trace.WriteLine("Connecting to SQL Database for serial number retrieval");
                        conn.Open();
                        Trace.WriteLine("Connection succeeded");

                        var newSernum = SqlHelper.GetNextRunningNumber(conn, ref mySerialNumInfo);
                        //serialNumber = string.Format("MY{0}{1}{2}", todayDate.ToString("yy"), todayDate.ToString("MM"), newSernum); // commented out due to serial number generation must use YYWW
                        serialNumber = string.Format("MY{0}{1}{2}", todayDate.ToString("yy"), workWeek.ToString("00"), newSernum);
                        Trace.WriteLine("New Serial Number is = " + serialNumber);

                    }
                    
                    Trace.WriteLine("Submitting new serial number to database..");
                    DateTime date = DateTime.Now;
                    mySerialNumInfo.DummySernum = _uutData.SerNum;
                    mySerialNumInfo.Model = _uutData.Model;
                    mySerialNumInfo.IsPassTest = true;
                    mySerialNumInfo.Sernum = serialNumber;
                    mySerialNumInfo.LogDate = date;
                    mySerialNumInfo.RunningNumber = mySerialNumInfo.RunningNumber;// +1;
                    string conStr = CalLeakConfigs.Default.SqlConString;
                    using (SqlConnection conn = new SqlConnection(conStr))
                    {
                        conn.Open();
                        var retVal = SqlHelper.SubmitAutoGeneratedSernum(conn, ref mySerialNumInfo);
                        Trace.WriteLine(retVal.ToString());
                    }
                    Trace.WriteLine("Done");
                }
                else if (_commonData.isProdMode)
                {
                    Trace.WriteLine("Submitting Cal-Leak data summary to database...");
                    string sqlConStr = CalLeakConfigs.Default.SqlConString;
                    myCalLeakData.IsPass = false;    // Failed
                    using (SqlConnection conn = new SqlConnection(sqlConStr))
                    {
                        conn.Open();
                        var retVal = SqlHelper.SubmitFinalCalLeakData(conn, ref myCalLeakData);
                        Trace.WriteLine("CalLeak Data transfered to database.");
                    }
                }
            }
            catch (Exception)
            {

            }
            finally
            {
                _uutData = null;
                //_commonData = null;

                // todo: play sound for test completion or tower light
            }
        }
    }
}
