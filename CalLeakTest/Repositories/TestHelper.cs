using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CalLeakTest.Models;

namespace CalLeakTest.Repositories
{
    public static class TestHelper
    {
        public static double GetLeakRateFromTempMap(int temperature, NistData nistData)
        {
            try
            {
                double leakRate = 0;
                switch (temperature)
                {
                    case 15: leakRate = nistData.Temp15Leak; break;
                    case 16: leakRate = nistData.Temp16Leak; break;
                    case 17: leakRate = nistData.Temp17Leak; break;
                    case 18: leakRate = nistData.Temp18Leak; break;
                    case 19: leakRate = nistData.Temp19Leak; break;
                    case 20: leakRate = nistData.Temp20Leak; break;
                    case 21: leakRate = nistData.Temp21Leak; break;
                    case 22: leakRate = nistData.Temp22Leak; break;
                    case 23: leakRate = nistData.Temp23Leak; break;
                    case 24: leakRate = nistData.Temp24Leak; break;
                    case 25: leakRate = nistData.Temp25Leak; break;
                    case 26: leakRate = nistData.Temp26Leak; break;
                    case 27: leakRate = nistData.Temp27Leak; break;
                    case 28: leakRate = nistData.Temp28Leak; break;
                    case 29: leakRate = nistData.Temp29Leak; break;
                    case 30: leakRate = nistData.Temp30Leak; break;
                    default:
                        break;
                }

                return leakRate;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static double CalculateNormLeakRate(double slope, double refTemp, double measLeakRate, double measTemp)
        {
            try
            {
                // formula LR = measLeakRate * ( 1 + (0.038 * 23) - (0.038 * measTemp) )
                double normLeakRate = measLeakRate * (1 + (slope * refTemp) - (slope * measTemp));
                return normLeakRate;
            }
            catch (Exception)
            {

                throw;
            }
        }


    }
}
