using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.IO;

namespace Ajustador_Calibrador_ADR3000.Devices
{
    public class AccuracyTestInfo
    {
        public float Energy { get; }
        public float Error { get; }
        public float MeterKd { get; }
        public float EstimatedKd { get; }
        public float OpenVoltage { get; }
        public float LoadVoltage { get; }
        public float LoadCurrent { get; }
        public float LoadPower { get; }
        public float LoadVoltAmperes { get; }
        public float LoadImpedance { get; }
        public int NumberOfPulses { get; }
        public bool TurboMode { get; }
        public string CtrlCode { get; }

        public AccuracyTestInfo(string _str)
        {
            string[] token = _str.Replace("[", "").Replace("]\n", "").Split(new char[] { ';' });

            Energy = Convert.ToSingle(token[0]);
            Error = Convert.ToSingle(token[1]);
            MeterKd = Convert.ToSingle(token[2]);
            EstimatedKd = Convert.ToSingle(token[3]);
            OpenVoltage = Convert.ToSingle(token[4]);
            LoadVoltage = Convert.ToSingle(token[5]);
            LoadCurrent = Convert.ToSingle(token[6]);
            LoadPower = Convert.ToSingle(token[7]);
            LoadVoltAmperes = Convert.ToSingle(token[8]);
            LoadImpedance = Convert.ToSingle(token[9]);
            NumberOfPulses = Convert.ToInt32(token[10]);
            TurboMode = token[11] == "1";
            CtrlCode = string.Copy(token[12]);            
        }
    }

    public class DemandTestInfo
    {
        public float CustomerPower { get; }
        public float CustomerVoltage { get; }

        public DemandTestInfo(string _str) 
        {
            string[] token = _str.Replace("[", "").Replace("]\n", "").Split(new char[] { ';' });

            CustomerPower = Convert.ToSingle(token[0]);
            CustomerVoltage = Convert.ToSingle(token[1]);
        }
    }

    public class ConnectionTestInfo
    {
        public float OpenVoltage { get; }
        public float LoadVoltage { get; }
        public float LoadCurrent { get; }
        public float VoltageDrop { get; }
        public float LoadInpedance { get; }

        public ConnectionTestInfo(string _str)
        {
            string[] token = _str.Replace("[", "").Replace("]\n", "").Split(new char[] { ';' });

            OpenVoltage = Convert.ToSingle(token[0]);
            LoadVoltage = Convert.ToSingle(token[1]);
            LoadCurrent = Convert.ToSingle(token[2]);
            VoltageDrop = Convert.ToSingle(token[3]);
            LoadInpedance = Convert.ToSingle(token[4]);
        }
    }

    public class ADR2000 : IDisposable
    {

        public static readonly int EE_RSTEE	=			0;
        public static readonly int EE_DEV_CONF	=			EE_RSTEE + 1;			/*!< Configuration of ADR 2000 --> 4 bytes */
        public static readonly int EE_MIN_ACCURACY_TIME=	EE_DEV_CONF + 4;
        public static readonly int EE_SERIAL	=			EE_MIN_ACCURACY_TIME + 4;	/*!< ADR 2000 serial number */
        public static readonly int EE_SHELF		=		EE_SERIAL + 4;				/*!< Shelf-life since calibration date in months */
        public static readonly int EE_REPORT	=			EE_SHELF + 2;				/*!< Calibration report. A 32 byte string */
        public static readonly int EE_CAL_DATE	=			EE_REPORT + 32;				/*!< Calibration date */

        public static readonly int EE_LPW1 = EE_CAL_DATE + 4;				/*!< Pulse output divider for LED 1 (0 to 15) */
        public static readonly int EE_LPW2 = EE_LPW1 + 1;				/*!< Pulse output divider for LED 2 (0 to 15) */
        public static readonly int EE_LD1_ENE = EE_LPW2 + 1;					/*!< Power source for LED 1 (0 to 3) */
        public static readonly int EE_LD2_ENE = EE_LD1_ENE + 1;			/*!< Power source for LED 2 (0 to 3) */

        public static readonly int EE_VOLTAGE_CONST = EE_LD2_ENE + 1;
        public static readonly int EE_CURRENT_CONST = EE_VOLTAGE_CONST + 4;
        public static readonly int EE_POWER_CONST = EE_CURRENT_CONST + 4;
        public static readonly int EE_ENERGY_CONST = EE_POWER_CONST + 4;

        public static readonly int EE_LCD_CONTRAST = EE_ENERGY_CONST + 4;		/*!< Contrast in % */
        public static readonly int EE_LCD_CONTRAST_GAIN = EE_LCD_CONTRAST + 1;
        public static readonly int EE_LCD_CONTRAST_OS = EE_LCD_CONTRAST_GAIN + 4;

        public static readonly int EE_dsp_cr1 = EE_LCD_CONTRAST_OS + 4;
        public static readonly int EE_dsp_cr2 = EE_dsp_cr1 + 4;
        public static readonly int EE_dsp_cr3 = EE_dsp_cr2 + 4;
        public static readonly int EE_dsp_cr4 = EE_dsp_cr3 + 4;
        public static readonly int EE_dsp_cr5 = EE_dsp_cr4 + 4;
        public static readonly int EE_dsp_cr6 = EE_dsp_cr5 + 4;
        public static readonly int EE_dsp_cr7 = EE_dsp_cr6 + 4;
        public static readonly int EE_dsp_cr8 = EE_dsp_cr7 + 4;
        public static readonly int EE_dsp_cr9 = EE_dsp_cr8 + 4;
        public static readonly int EE_dsp_cr10 = EE_dsp_cr9 + 4;
        public static readonly int EE_dsp_cr11 = EE_dsp_cr10 + 4;
        public static readonly int EE_dsp_cr12 = EE_dsp_cr11 + 4;
        public static readonly int EE_dfe_cr1 = EE_dsp_cr12 + 4;
        public static readonly int EE_dfe_cr2 = EE_dfe_cr1 + 4;
        public static readonly int EE_dsp_irq1 = EE_dfe_cr2 + 4;
        public static readonly int EE_dsp_irq2 = EE_dsp_irq1 + 4;
        public static readonly int EE_dsp_sr1 = EE_dsp_irq2 + 4;
        public static readonly int EE_dsp_sr2 = EE_dsp_sr1 + 4;
        public static readonly int EE_us_reg1 = EE_dsp_sr2 + 4;
        public static readonly int EE_us_reg2 = EE_us_reg1 + 4;
        public static readonly int EE_us_reg3 = EE_us_reg2 + 4;

        public static readonly int EE_E1_L0 = EE_us_reg3 + 4;		/*!< Lower threshold scale 0 channel 1 */
        public static readonly int EE_E1_H0 = EE_E1_L0 + 4;		/*!< Higher threshold scale 0 channel 1 */
        public static readonly int EE_CHV1 = EE_E1_H0 + 4;		/*!< Voltage gain for channel 1 */
        public static readonly int EE_PHV1_0 = EE_CHV1 + 2;			/*!< Voltage phase lag scale 0 for channel 1*/
        public static readonly int EE_CHC1_0 = EE_PHV1_0 + 1;			/*!< Current gain scale 0 for channel 1 */
        public static readonly int EE_PHC1_0 = EE_CHC1_0 + 2;			/*!< Current phase lag scale 0 for channel 1 */
        public static readonly int EE_OFA1_0 = EE_PHC1_0 + 2;		/*!< Offset for active power scale 0 channel 1 */
        public static readonly int EE_OFAF1_0 = EE_OFA1_0 + 2;		/*!< Offset for fundamental active power scale 0 channel 1 */
        public static readonly int EE_OFS1_0 = EE_OFAF1_0 + 2;			/*!< Offset for apparent power scale 0 channel 1 */
        public static readonly int EE_OFR1_0 = EE_OFS1_0 + 2;		/*!< Offset for reactive power scale 0 channel 1 */
        public static readonly int EE_PHV1_1 = EE_OFR1_0 + 2;			/*!< Voltage phase lag scale 1 for channel 1*/
        public static readonly int EE_CHC1_1 = EE_PHV1_1 + 1;			/*!< Current gain scale 1 for channel 1 */
        public static readonly int EE_PHC1_1 = EE_CHC1_1 + 2;			/*!< Current phase lag scale 1 for channel 1 */
        public static readonly int EE_OFA1_1 = EE_PHC1_1 + 2;			/*!< Offset for active power scale 1 channel 1 */
        public static readonly int EE_OFAF1_1 = EE_OFA1_1 + 2;			/*!< Offset for fundamental active power scale 1 channel 1 */
        public static readonly int EE_OFS1_1 = EE_OFAF1_1 + 2;		/*!< Offset for apparent power scale 1 channel 1 */
        public static readonly int EE_OFR1_1 = EE_OFS1_1 + 2;		/*!< Offset for reactive power scale 1 channel 1 */

        public static readonly int EE_CONNEC_TEST_GAIN = EE_OFR1_1 + 2;
        public static readonly int EE_CONNEC_TEST_OFFSET = EE_CONNEC_TEST_GAIN + 4;
        public static readonly int EE_CONNEC_TEST_RESISTANCE = EE_CONNEC_TEST_OFFSET + 4;

        public const int V = 0;
        public const int I = 1;
        public const int P = 2;
        public const int S = 3;
        public const int Q = 4;
        public const int FP = 5;
        public const int F = 6;
        public const int PHI = 7;
        public const int WH = 8;
        public const int VAH = 9;
        public const int VARH = 10;

        public const int NUM_OF_MEASURES = 12;
        public const int REPORT_SIZE = 32;

        public const float VOLTAGE_TH = 170;

        private const string NAK_IO_EXCEPTION = "ADR 2000 retornou nak.";
        private const string PARAM_LIMITS = "Parâmetro fora dos limites permitidos.";
        private const string ack = "ack\n";
        private const string nak = "nak\n";
        private const int buffer_size = 1024;
        private readonly NetworkStream stream;
        private bool disposed;
        private readonly SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);
        public int ReadTimeout { get; set; } = 3000;
        public int TimeToCoolResistance { get; set; } = 8000;

        public ADR2000(NetworkStream _stream) 
        {
            if (_stream != null)
            {
                stream = _stream;
            }
        }

        public void GetMeasures(float[] measures)
        {
            string command = "vinst\r";

            byte[] buf = Encoding.ASCII.GetBytes(command);
            stream.Write(buf, 0, buf.Length);

            buf = new byte[buffer_size];
            int i = 0;
            do
            {
                stream.Read(buf, i++, 1);
            } while (buf[i - 1] != 0x0A);

            string answer = Encoding.ASCII.GetString(buf, 0, i);

            if (answer == nak) throw new IOException(NAK_IO_EXCEPTION);

            string[] token = answer.Replace("[", "").Replace("]\n", "").Split(new char[] { ';' });

            int len = token.Length;
            for (i = 0; i < len; i++) 
            {
                measures[i] = Convert.ToSingle(token[i]);
            }
        }

        public bool CoolResistance()
        {
            string command = "cool_resistance\r";

            byte[] buf = Encoding.ASCII.GetBytes(command);
            stream.Write(buf, 0, buf.Length);

            buf = new byte[buffer_size];
            int i = 0;
            do
            {
                stream.Read(buf, i++, 1);
            } while (buf[i - 1] != 0x0A);

            if (Encoding.ASCII.GetString(buf, 0, i) != ack) return false;

            System.Threading.Thread.Sleep(TimeToCoolResistance);

            return true;
        }

        public int ReadMemoryByte(int address)
        {
            string command = "read;byte;" + address.ToString() + "\r";

            byte[] buf = Encoding.ASCII.GetBytes(command);
            stream.Write(buf, 0, buf.Length);

            buf = new byte[buffer_size];
            int i = 0;
            do
            {
                stream.Read(buf, i++, 1);
            } while (buf[i - 1] != 0x0A);

            string answer = Encoding.ASCII.GetString(buf, 0, i);

            if (answer == nak) throw new IOException(NAK_IO_EXCEPTION);

            answer = answer.Replace("[", "").Replace("]\n", "");

            return Convert.ToInt32(answer);
        }

        public int ReadMemoryShort(int address)
        {
            string command = "read;short;" + address.ToString() + "\r";

            byte[] buf = Encoding.ASCII.GetBytes(command);
            stream.Write(buf, 0, buf.Length);

            buf = new byte[buffer_size];
            int i = 0;
            do
            {
                stream.Read(buf, i++, 1);
            } while (buf[i - 1] != 0x0A);

            string answer = Encoding.ASCII.GetString(buf, 0, i);

            if (answer == nak) throw new IOException(NAK_IO_EXCEPTION);

            answer = answer.Replace("[", "").Replace("]\n", "");

            return Convert.ToInt32(answer);
        }

        public int ReadMemoryLong(int address)
        {
            string command = "read;long;" + address.ToString() + "\r";

            byte[] buf = Encoding.ASCII.GetBytes(command);
            stream.Write(buf, 0, buf.Length);

            buf = new byte[buffer_size];
            int i = 0;
            do
            {
                stream.Read(buf, i++, 1);
            } while (buf[i - 1] != 0x0A);

            string answer = Encoding.ASCII.GetString(buf, 0, i);

            if (answer == nak) throw new IOException(NAK_IO_EXCEPTION);

            answer = answer.Replace("[", "").Replace("]\n", "");

            return Convert.ToInt32(answer);
        }

        public float ReadMemoryFloat(int address)
        {
            string command = "read;float;" + address.ToString() + "\r";

            byte[] buf = Encoding.ASCII.GetBytes(command);
            stream.Write(buf, 0, buf.Length);

            buf = new byte[buffer_size];
            int i = 0;
            do
            {
                stream.Read(buf, i++, 1);
            } while (buf[i - 1] != 0x0A);

            string answer = Encoding.ASCII.GetString(buf, 0, i);

            if (answer == nak) throw new IOException(NAK_IO_EXCEPTION);

            answer = answer.Replace("[", "").Replace("]\n", "");

            return Convert.ToSingle(answer);
        }

        public string ReadMemoryString(int address, int length)
        {
            string command = "read;string;" + length.ToString() + ";" + address.ToString() + "\r";

            byte[] buf = Encoding.ASCII.GetBytes(command);
            stream.Write(buf, 0, buf.Length);

            buf = new byte[buffer_size];
            int i = 0;
            do
            {
                stream.Read(buf, i++, 1);
            } while (buf[i - 1] != 0x0A);

            string answer = Encoding.ASCII.GetString(buf, 0, i);

            if (answer == nak) throw new IOException(NAK_IO_EXCEPTION);

            answer = answer.Replace("[", "").Replace("]\n", "");

            return answer;
        }
    
        public bool WriteMemoryByte(int address, int data)
        {
            string command = "write;byte;" + data.ToString() + ";" + address.ToString() + "\r";

            byte[] buf = Encoding.ASCII.GetBytes(command);
            stream.Write(buf, 0, buf.Length);

            buf = new byte[buffer_size];
            int i = 0;
            do
            {
                stream.Read(buf, i++, 1);
            } while (buf[i - 1] != 0x0A);

            return Encoding.ASCII.GetString(buf, 0, i) == ack;
        }

        public bool WriteMemoryShort(int address, int data)
        {
            string command = "write;short;" + data.ToString() + ";" + address.ToString() + "\r";

            byte[] buf = Encoding.ASCII.GetBytes(command);
            stream.Write(buf, 0, buf.Length);

            buf = new byte[buffer_size];
            int i = 0;
            do
            {
                stream.Read(buf, i++, 1);
            } while (buf[i - 1] != 0x0A);

            return Encoding.ASCII.GetString(buf, 0, i) == ack;
        }

        public bool WriteMemoryLong(int address, int data)
        {
            string command = "write;long;" + data.ToString() + ";" + address.ToString() + "\r";

            byte[] buf = Encoding.ASCII.GetBytes(command);
            stream.Write(buf, 0, buf.Length);

            buf = new byte[buffer_size];
            int i = 0;
            do
            {
                stream.Read(buf, i++, 1);
            } while (buf[i - 1] != 0x0A);

            return Encoding.ASCII.GetString(buf, 0, i) == ack;
        }

        public bool WriteMemoryFloat(int address, float data)
        {
            string command = "write;float;" + data.ToString() + ";" + address.ToString() + "\r";

            byte[] buf = Encoding.ASCII.GetBytes(command);
            stream.Write(buf, 0, buf.Length);

            buf = new byte[buffer_size];
            int i = 0;
            do
            {
                stream.Read(buf, i++, 1);
            } while (buf[i - 1] != 0x0A);

            return Encoding.ASCII.GetString(buf, 0, i) == ack;
        }

        public bool WriteMemoryString(int address, string data)
        {
            string command = "write;string;" + data + ";" + address.ToString() + "\r";

            byte[] buf = Encoding.ASCII.GetBytes(command);
            stream.Write(buf, 0, buf.Length);

            buf = new byte[buffer_size];
            int i = 0;
            do
            {
                stream.Read(buf, i++, 1);
            } while (buf[i - 1] != 0x0A);

            return Encoding.ASCII.GetString(buf, 0, i) == ack;
        }
        
        public bool ConfigureAccuracyTest(float kd, int numberOfPulses, bool turbo)
        {
            string command = "config_test;" + kd.ToString() + ";" + numberOfPulses.ToString() + ";" + (turbo ? "1" : "0") + "\r";

            byte[] buf = Encoding.ASCII.GetBytes(command);
            stream.Write(buf, 0, buf.Length);

            buf = new byte[buffer_size];
            int i = 0;
            do
            {
                stream.Read(buf, i++, 1);
            } while (buf[i - 1] != 0x0A);

            return Encoding.ASCII.GetString(buf, 0, i) == ack;
        }

        public bool EnableAccuracyTest()
        {
            string command = "hab_accuracy\r";

            byte[] buf = Encoding.ASCII.GetBytes(command);
            stream.Write(buf, 0, buf.Length);

            buf = new byte[buffer_size];
            int i = 0;
            do
            {
                stream.Read(buf, i++, 1);
            } while (buf[i - 1] != 0x0A);

            return Encoding.ASCII.GetString(buf, 0, i) == ack;
        }

        public bool EnableConnectionTest()
        {
            string command = "hab_connection\r";

            byte[] buf = Encoding.ASCII.GetBytes(command);
            stream.Write(buf, 0, buf.Length);

            buf = new byte[buffer_size];
            int i = 0;
            do
            {
                stream.Read(buf, i++, 1);
            } while (buf[i - 1] != 0x0A);

            return Encoding.ASCII.GetString(buf, 0, i) == ack;
        }

        public bool GetTestStatus()
        {
            string command = "get_test_status\r";

            byte[] buf = Encoding.ASCII.GetBytes(command);
            stream.Write(buf, 0, buf.Length);

            buf = new byte[buffer_size];
            int i = 0;
            do
            {
                stream.Read(buf, i++, 1);
            } while (buf[i - 1] != 0x0A);

            return Encoding.ASCII.GetString(buf, 0, i).Replace("[", "").Replace("]\n", "") == "1";
        }

        public bool CancelTest()
        {
            string command = "cancel\r";

            byte[] buf = Encoding.ASCII.GetBytes(command);
            stream.Write(buf, 0, buf.Length);

            buf = new byte[buffer_size];
            int i = 0;
            do
            {
                stream.Read(buf, i++, 1);
            } while (buf[i - 1] != 0x0A);

            return Encoding.ASCII.GetString(buf, 0, i) == ack;
        }

        public bool StopAll()
        {
            string command = "stop_all\r";

            byte[] buf = Encoding.ASCII.GetBytes(command);
            stream.Write(buf, 0, buf.Length);

            buf = new byte[buffer_size];
            int i = 0;
            do
            {
                stream.Read(buf, i++, 1);
            } while (buf[i - 1] != 0x0A);

            return Encoding.ASCII.GetString(buf, 0, i) == ack;
        }

        public int GetDate(long day, long month, long year)
        {
            if ((day < 0) || (month < 0) || (year < 0) || (year > 65535) || (month > 12)) throw new ArgumentOutOfRangeException(PARAM_LIMITS);
            else if ((month == 1) || (month == 3) || (month == 5) || (month == 7) || (month == 8) || (month == 10) || (month == 12))
            {
                if (day > 31) throw new ArgumentOutOfRangeException(PARAM_LIMITS);
            }
            else if ((month == 4) || (month == 6) || (month == 9) || (month == 11))
            {
                if (day > 30) throw new ArgumentOutOfRangeException(PARAM_LIMITS);
            }
            if (month == 2)
            {
                bool isLeap = false;
                if ((year % 4) == 0)
                {
                    if ((year % 100) == 0)
                    {
                        if ((year % 400) == 0) isLeap = true;
                        else isLeap = false;
                    }
                    else isLeap = true;
                }
                else isLeap = false;

                if ((isLeap && (day > 29)) || ((!isLeap) && (day > 28))) throw new ArgumentOutOfRangeException(PARAM_LIMITS);
            }

            long date = ((day << 24) & 0xFF000000);
            date |= ((month << 16) & 0xFF0000);
            date |= (year & 0xFFFF);

            return ((int)date);
        }
        
        public string GetFWVersion()
        {
            string command = "get_fw\r";

            byte[] buf = Encoding.ASCII.GetBytes(command);
            stream.Write(buf, 0, buf.Length);

            buf = new byte[buffer_size];
            int i = 0;
            do
            {
                stream.Read(buf, i++, 1);
            } while (buf[i - 1] != 0x0A);

            string answer = Encoding.ASCII.GetString(buf, 0, i);

            if (answer == nak) throw new IOException(NAK_IO_EXCEPTION);

            return answer.Replace("[", "").Replace("]\n", "");
        }

        public AccuracyTestInfo GetAccuracyTestResult()
        {
            string command = "get_accuracy\r";

            byte[] buf = Encoding.ASCII.GetBytes(command);
            stream.Write(buf, 0, buf.Length);

            buf = new byte[buffer_size];
            int i = 0;
            do
            {
                stream.Read(buf, i++, 1);
            } while (buf[i - 1] != 0x0A);

            string answer = Encoding.ASCII.GetString(buf, 0, i);

            if (answer == nak) throw new IOException(NAK_IO_EXCEPTION);

            return new AccuracyTestInfo(answer);
        }

        public DemandTestInfo GetDemandTestInfo()
        {
            string command = "get_demand\r";

            byte[] buf = Encoding.ASCII.GetBytes(command);
            stream.Write(buf, 0, buf.Length);

            buf = new byte[buffer_size];
            int i = 0;
            do
            {
                stream.Read(buf, i++, 1);
            } while (buf[i - 1] != 0x0A);

            string answer = Encoding.ASCII.GetString(buf, 0, i);

            if (answer == nak) throw new IOException(NAK_IO_EXCEPTION);

            return new DemandTestInfo(answer);
        }

        public ConnectionTestInfo GetConnectionTestInfo()
        {
            string command = "get_connection\r";

            byte[] buf = Encoding.ASCII.GetBytes(command);
            stream.Write(buf, 0, buf.Length);

            buf = new byte[buffer_size];
            int i = 0;
            do
            {
                stream.Read(buf, i++, 1);
            } while (buf[i - 1] != 0x0A);

            string answer = Encoding.ASCII.GetString(buf, 0, i);

            if (answer == nak) throw new IOException(NAK_IO_EXCEPTION);

            return new ConnectionTestInfo(answer);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                //
                try
                {
                    if (handle != null) handle.Dispose();
                }
                catch (Exception) { }
            }

            // Free any unmanaged objects here.
            //
            disposed = true;
        }

        /// <summary>
        /// Disposes this instace of the ADR3000 class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
